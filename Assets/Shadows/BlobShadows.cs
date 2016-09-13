using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlobShadows : MonoBehaviour {
	
	public int resolution = 512;
	public int border = 1;
	public float shadowDistance = 50f;
	public bool preview = false;
	public LayerMask cullingMask;
	
	static GameObject shadowMapGO;
	RenderTexture shadowMapRT;
	

	// Use this for initialization
	void Start()
	{
		ValidateShadowMap();
		DeactivateShadowMap();
		Shader.SetGlobalVector("_DisableBlobShadowCaster", Vector4.zero);
	}

	static Texture2D whiteTexture;
	void DeactivateShadowMap()
	{
		Color shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		if (!whiteTexture)
		{
			whiteTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			whiteTexture.SetPixel(0, 0, shadowColor);
			whiteTexture.SetPixel(1, 0, shadowColor);
			whiteTexture.SetPixel(0, 1, shadowColor);
			whiteTexture.SetPixel(1, 1, shadowColor);
			whiteTexture.Apply();
		}

		Shader.SetGlobalTexture("_PlanarShadowTex", whiteTexture);
	}

	void ValidateShadowMap ()
	{
		if (!shadowMapGO)
		{
			Color shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

				
			shadowMapGO = new GameObject ("__ShadowMap");
			shadowMapGO.hideFlags = HideFlags.HideAndDontSave;
			shadowMapGO.AddComponent(typeof(Camera));
			shadowMapGO.GetComponent<Camera>().orthographic = true;
			shadowMapGO.GetComponent<Camera>().orthographicSize = 1;
			shadowMapGO.GetComponent<Camera>().aspect = 1;
			shadowMapGO.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing; // CameraClearFlags.SolidColor;
			shadowMapGO.GetComponent<Camera>().backgroundColor = shadowColor;
			shadowMapGO.GetComponent<Camera>().depth = 0;
			shadowMapGO.transform.position = transform.position;
			shadowMapGO.transform.rotation = transform.rotation;
			shadowMapGO.transform.localScale = Vector3.one;
			shadowMapGO.GetComponent<Camera>().enabled = false;

			RenderBorder renderBorder = shadowMapGO.AddComponent(typeof(RenderBorder)) as RenderBorder;
			renderBorder.border = border;
			renderBorder.borderColor = shadowColor;
		}
	
		shadowMapRT = RenderTexture.GetTemporary (resolution, resolution, 0, RenderTextureFormat.ARGB32);
		if (!shadowMapRT)
			Debug.Log("Couldn't get ShadowMap RT for shadow blobs " + resolution);
	
		if (shadowMapGO && shadowMapRT)
		{
			shadowMapGO.GetComponent<Camera>().targetTexture = shadowMapRT;
			Rect rect = new Rect ();
			rect.x = border;
			rect.y = border;
			rect.width = shadowMapRT.width - border * 2;
			rect.height = shadowMapRT.height - border * 2;
			shadowMapGO.GetComponent<Camera>().pixelRect = rect;
		}
	}

	void OnPreRender () {
		ValidateShadowMap ();
		
		var far = Mathf.Min(shadowDistance, GetComponent<Camera>().farClipPlane);
		
		shadowMapGO.transform.position = transform.position;
		shadowMapGO.transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + 45f, Vector3.up) *
			Quaternion.LookRotation(Vector3.up * -1f, Vector3.forward);
		shadowMapGO.transform.position += transform.forward * far * 0.5f;
		shadowMapGO.transform.position += transform.up * far;
		shadowMapGO.GetComponent<Camera>().orthographicSize = far;
		shadowMapGO.GetComponent<Camera>().aspect = 1.0f;//aabb.extents.x / (aabb.extents.y + Mathf.Epsilon);
		shadowMapGO.GetComponent<Camera>().cullingMask = cullingMask;
		shadowMapGO.GetComponent<Camera>().targetTexture = shadowMapRT;

#if UNITY_EDITOR
		Shader.SetGlobalVector("_DisableBlobShadowCaster", Vector4.zero);
#endif
		
		RenderTexture lastActiveRT = RenderTexture.active;
		shadowMapGO.GetComponent<Camera>().Render();
		RenderTexture.active = lastActiveRT;

#if UNITY_EDITOR
		Shader.SetGlobalVector("_DisableBlobShadowCaster", Vector4.one);
#endif

		float scaleX = 1.0f / (shadowMapGO.GetComponent<Camera>().orthographicSize*shadowMapGO.GetComponent<Camera>().aspect);
		float scaleY = 1.0f / shadowMapGO.GetComponent<Camera>().orthographicSize;
		Matrix4x4 scaleOffsetMatrix = Matrix4x4.TRS (new Vector3(0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3(0.5f*scaleX, 0.5f*scaleY, 0.0f));
		Shader.SetGlobalMatrix("_World2PlanarShadow", scaleOffsetMatrix * shadowMapGO.GetComponent<Camera>().worldToCameraMatrix);
		Shader.SetGlobalTexture("_PlanarShadowTex", shadowMapRT);
	}
	
	void OnPostRender () {
		shadowMapGO.GetComponent<Camera>().targetTexture = null;
		shadowMapRT.DiscardContents();
		if (shadowMapRT)
			RenderTexture.ReleaseTemporary(shadowMapRT);
		shadowMapRT = null;
		DeactivateShadowMap();
	}

#if UNITY_EDITOR
/*
	void OnGUI ()
	{
		if (shadowMapRT)
			GUI.DrawTexture (new Rect(0,0,128,128), shadowMapRT, ScaleMode.ScaleToFit, false);
	}
*/	
	void OnDrawGizmos() {
		if (!preview)
			return;
		
		var l = shadowMapGO.GetComponent<Camera>().orthographicSize;
		//var c = shadowMapGO.transform.position;
		var vs = new Vector3[8] {
			new Vector3(-l, -l, -l), new Vector3(l, -l, -l), new Vector3(l, l, -l), new Vector3(-l, l, -l), 
			new Vector3(-l, -l, l), new Vector3(l, -l, l), new Vector3(l, l, l), new Vector3(-l, l, l) };
		for (int q = 0; q < vs.Length; ++q)
			vs[q] = shadowMapGO.transform.TransformPoint(vs[q]);
					
		Gizmos.DrawLine (vs[0], vs[1]);
		Gizmos.DrawLine (vs[1], vs[2]);
		Gizmos.DrawLine (vs[2], vs[3]);
		Gizmos.DrawLine (vs[3], vs[0]);
		Gizmos.DrawLine (vs[0+4], vs[1+4]);
		Gizmos.DrawLine (vs[1+4], vs[2+4]);
		Gizmos.DrawLine (vs[2+4], vs[3+4]);
		Gizmos.DrawLine (vs[3+4], vs[0+4]);
		Gizmos.DrawLine (vs[0], vs[0+4]);
		Gizmos.DrawLine (vs[1], vs[1+4]);
		Gizmos.DrawLine (vs[2], vs[2+4]);
		Gizmos.DrawLine (vs[3], vs[3+4]);
	}
#endif
}	

