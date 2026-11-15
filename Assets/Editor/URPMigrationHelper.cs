using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.FPS.Editor
{
    public static class URPMigrationHelper
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string UrpAssetPath = "Assets/Settings/UniversalRenderPipelineAsset.asset";
        private const string RendererDataPath = "Assets/Settings/UniversalRendererData.asset";

        [MenuItem("Tools/URP Migration/Run Full URP Migration (All Steps)", priority = 0)]
        public static void RunFullMigration()
        {
            Debug.Log("[URPMigration] Starting Full URP Migration...");
            CreateAndAssignUrpAsset();
            UpgradeMaterialsToURP();
            ConfigureCameraStackingInScenes();
            Debug.Log("[URPMigration] Full URP Migration completed successfully! You can also run 'Window > Rendering > Render Pipeline Converter' if needed.");
        }

        [MenuItem("Tools/URP Migration/1. Create & Assign URP Pipeline Assets", priority = 10)]
        public static void CreateAndAssignUrpAsset()
        {
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
                AssetDatabase.Refresh();
            }

            // Check if URP is loaded in the project
            Type urpAssetType = FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
            Type rendererDataType = FindType("UnityEngine.Rendering.Universal.UniversalRendererData");

            if (urpAssetType == null || rendererDataType == null)
            {
                Debug.LogWarning("[URPMigration] Universal Render Pipeline package is not yet fully compiled or installed. Please ensure com.unity.render-pipelines.universal is in Packages/manifest.json and Unity has finished compilation.");
                return;
            }

            RenderPipelineAsset existingAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(UrpAssetPath);
            if (existingAsset == null)
            {
                // Create Renderer Data
                ScriptableObject rendererData = ScriptableObject.CreateInstance(rendererDataType);
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);

                // Create URP Asset via reflection Create method if available, or CreateInstance
                MethodInfo createMethod = urpAssetType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { rendererDataType }, null);
                RenderPipelineAsset pipelineAsset;
                if (createMethod != null)
                {
                    pipelineAsset = (RenderPipelineAsset)createMethod.Invoke(null, new object[] { rendererData });
                }
                else
                {
                    pipelineAsset = (RenderPipelineAsset)ScriptableObject.CreateInstance(urpAssetType);
                }

                AssetDatabase.CreateAsset(pipelineAsset, UrpAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[URPMigration] Created URP assets at '{UrpAssetPath}' and '{RendererDataPath}'.");
                existingAsset = pipelineAsset;
            }

            // Assign to GraphicsSettings and QualitySettings
            GraphicsSettings.defaultRenderPipeline = existingAsset;
            QualitySettings.renderPipeline = existingAsset;
            Debug.Log("[URPMigration] Assigned URP Asset to GraphicsSettings and QualitySettings.");
        }

        [MenuItem("Tools/URP Migration/2. Upgrade All Project Materials to URP", priority = 20)]
        public static void UpgradeMaterialsToURP()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
            Shader urpParticlesLit = Shader.Find("Universal Render Pipeline/Particles/Lit");
            Shader urpParticlesUnlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (urpLit == null)
            {
                Debug.LogWarning("[URPMigration] 'Universal Render Pipeline/Lit' shader not found. Please ensure URP package is installed and compiled.");
                return;
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            int convertedCount = 0;

            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;

                string shaderName = mat.shader.name;

                // Built-in Standard or Standard (Specular)
                if (shaderName == "Standard" || shaderName == "Standard (Roughness setup)" || shaderName == "Standard (Specular setup)")
                {
                    UpgradeStandardMaterialToUrpLit(mat, urpLit);
                    EditorUtility.SetDirty(mat);
                    convertedCount++;
                }
                else if (shaderName.StartsWith("Particles/") || shaderName.StartsWith("Legacy Shaders/Particles/"))
                {
                    if (urpParticlesUnlit != null)
                    {
                        UpgradeParticleMaterialToUrp(mat, urpParticlesUnlit);
                        EditorUtility.SetDirty(mat);
                        convertedCount++;
                    }
                }
                else if (shaderName == "Unlit/Texture" || shaderName == "Unlit/Color")
                {
                    if (urpUnlit != null)
                    {
                        UpgradeUnlitMaterialToUrp(mat, urpUnlit);
                        EditorUtility.SetDirty(mat);
                        convertedCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[URPMigration] Upgraded {convertedCount} materials to Universal Render Pipeline shaders.");
        }

        private static void UpgradeStandardMaterialToUrpLit(Material mat, Shader urpLit)
        {
            // Cache properties
            Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Vector2 mainScale = mat.HasProperty("_MainTex") ? mat.GetTextureScale("_MainTex") : Vector2.one;
            Vector2 mainOffset = mat.HasProperty("_MainTex") ? mat.GetTextureOffset("_MainTex") : Vector2.zero;
            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
            float bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;

            Texture metallicGlossMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
            float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            float glossiness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;

            Texture occlusionMap = mat.HasProperty("_OcclusionMap") ? mat.GetTexture("_OcclusionMap") : null;
            float occlusionStrength = mat.HasProperty("_OcclusionStrength") ? mat.GetFloat("_OcclusionStrength") : 1f;

            Texture emissionMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
            Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;
            bool hasEmission = mat.IsKeywordEnabled("_EMISSION") || emissionColor.maxColorComponent > 0.01f || emissionMap != null;

            float mode = mat.HasProperty("_Mode") ? mat.GetFloat("_Mode") : 0f; // 0=Opaque, 1=Cutout, 2=Fade, 3=Transparent
            float cutoff = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;

            // Switch shader
            mat.shader = urpLit;

            // Set URP properties
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", mainTex);
                mat.SetTextureScale("_BaseMap", mainScale);
                mat.SetTextureOffset("_BaseMap", mainOffset);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

            if (mat.HasProperty("_BumpMap") && bumpMap != null)
            {
                mat.SetTexture("_BumpMap", bumpMap);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", bumpScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_MetallicGlossMap") && metallicGlossMap != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicGlossMap);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", glossiness);

            if (mat.HasProperty("_OcclusionMap") && occlusionMap != null)
            {
                mat.SetTexture("_OcclusionMap", occlusionMap);
                if (mat.HasProperty("_OcclusionStrength")) mat.SetFloat("_OcclusionStrength", occlusionStrength);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }

            if (hasEmission)
            {
                if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", emissionMap);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
            }

            // Surface type: Cutout or Transparent
            if (mode == 1f) // Cutout
            {
                if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 1f);
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", cutoff);
                mat.EnableKeyword("_ALPHATEST_ON");
            }
            else if (mode == 2f || mode == 3f) // Fade / Transparent
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 1 = Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f); // 0 = Alpha
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = (int)RenderQueue.Transparent;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }

        private static void UpgradeParticleMaterialToUrp(Material mat, Shader urpParticles)
        {
            Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            mat.shader = urpParticles;

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        }

        private static void UpgradeUnlitMaterialToUrp(Material mat, Shader urpUnlit)
        {
            Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            mat.shader = urpUnlit;

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        }

        [MenuItem("Tools/URP Migration/3. Configure Camera Stacking in Scenes", priority = 30)]
        public static void ConfigureCameraStackingInScenes()
        {
            Type cameraDataType = FindType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            Type cameraRenderType = FindType("UnityEngine.Rendering.Universal.CameraRenderType");

            if (cameraDataType == null || cameraRenderType == null)
            {
                Debug.LogWarning("[URPMigration] UniversalAdditionalCameraData not found. Please ensure URP package is installed and compiled.");
                return;
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/FPS/Scenes" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                Camera mainCam = null;
                Camera weaponCam = null;

                foreach (Camera cam in UnityEngine.Object.FindObjectsOfType<Camera>(true))
                {
                    if (cam.name.Contains("PlayerCamera") || cam.name.Contains("Main Camera") || cam.CompareTag("MainCamera"))
                    {
                        if (cam.name != "WeaponCamera") mainCam = cam;
                    }
                    if (cam.name.Contains("WeaponCamera"))
                    {
                        weaponCam = cam;
                    }
                }

                if (mainCam != null && weaponCam != null)
                {
                    // Configure WeaponCamera as Overlay
                    Component weaponData = weaponCam.GetComponent(cameraDataType);
                    if (weaponData == null) weaponData = weaponCam.gameObject.AddComponent(cameraDataType);

                    PropertyInfo renderTypeProp = cameraDataType.GetProperty("renderType");
                    if (renderTypeProp != null)
                    {
                        object overlayEnum = Enum.Parse(cameraRenderType, "Overlay");
                        renderTypeProp.SetValue(weaponData, overlayEnum);
                    }

                    // Configure MainCamera as Base and add WeaponCamera to cameraStack
                    Component mainData = mainCam.GetComponent(cameraDataType);
                    if (mainData == null) mainData = mainCam.gameObject.AddComponent(cameraDataType);

                    if (renderTypeProp != null)
                    {
                        object baseEnum = Enum.Parse(cameraRenderType, "Base");
                        renderTypeProp.SetValue(mainData, baseEnum);
                    }

                    PropertyInfo stackProp = cameraDataType.GetProperty("cameraStack");
                    if (stackProp != null)
                    {
                        var stack = stackProp.GetValue(mainData) as List<Camera>;
                        if (stack != null && !stack.Contains(weaponCam))
                        {
                            stack.Add(weaponCam);
                        }
                    }

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[URPMigration] Configured URP Camera Stacking (Base + WeaponCamera Overlay) in scene '{scene.name}'.");
                }
            }
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}

