using UnityEngine;

public class ScrollUVWater : MonoBehaviour
{
	public Vector2 AnimatRate = new Vector2(0f, 0f);

	private Vector2 UVOffs = Vector2.zero;

	private void Start()
	{
	}

	private void Update()
	{
		UVOffs += AnimatRate * Time.deltaTime;
		GetComponent<Renderer>().materials[0].SetTextureOffset("_MainTex", UVOffs);
	}
}
