using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class CameraSkinScattering : MonoBehaviour 
{
	public enum ScatteringModel {
		None, // same as Fallback now
		Fallback,
		TwoLayersCheap
	};

	public bool useKeywordShader = false;

	public ScatteringModel scattering = ScatteringModel.TwoLayersCheap;

	public LayerMask skinLayers;
	private Renderer[] skinnedMeshRenderers;
	public Color clearColor = new Color(0,0,0,0);

	public float blurOffset = 1.0f;
	public RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
	public float rtResolutionDivider = 2.0f;
	public int rtResolutionClampSourceWidth = 1024;
	public int rtResolutionClampSourceHeight = 768;

	public Shader skin1stPassShader = null;
	public Shader skin1stPassEyesShader = null;
	public Shader skin2ndPassLQShader = null;
	public Shader skin2ndPassLQEyesShader = null;

	public Shader skinFBShader = null;
	public Shader skinPostPassShader = null;
	Material skinPostPassMaterial = null;

	public Shader skinKeywordShader = null;
	
	public float fallbackDistance = 10;

	static Hashtable sssCameraByCamera = new Hashtable();
	static Hashtable sssRTByCamera = new Hashtable();
	
	private int frameCount = -1;
	
	public bool enableInSceneView = true;
	
	static public Renderer[] CollectSkinRenderers()
	{
		var skins = SkinShadingLookupTexture.allSkinShadingComponents;
		var renderers = new Renderer[skins.Count];
		for (int q = 0; q < skins.Count; ++q)
			renderers[q] = (skins[q] as SkinShadingLookupTexture).GetComponent<Renderer>();
		return renderers;
	}
	
	public bool SupportedMaterial(Material m)
	{
		return (m && (
			m.shader == null || (m.shader && m.shader.name == "") || 
			m.shader == skin1stPassShader ||
			m.shader == skin1stPassEyesShader ||
			m.shader == skin2ndPassLQShader ||
			m.shader == skin2ndPassLQEyesShader ||
			m.shader == skinFBShader ||
			m.shader == skinKeywordShader));
	}
	
	void Awake ()
	{
		skinnedMeshRenderers = CollectSkinRenderers();
	}
	
	void Start ()
	{
		if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
			skinnedMeshRenderers = CollectSkinRenderers();

		if(skinPostPassShader && skinPostPassShader.isSupported) 
			skinPostPassMaterial = new Material(skinPostPassShader);
	}
	
	void Update ()
	{
		if (!Application.isPlaying)
		{
			skinnedMeshRenderers = CollectSkinRenderers();
			#if UNITY_EDITOR
			EditorUtility.SetDirty (this);
			#endif
		}
	}

	private Camera GetSSSCamera (Camera cam) {
		System.Object searchCriteria;
		if (BulletTime.isEditing)
			searchCriteria = cam;
		else
			searchCriteria = cam.pixelRect;
		GameObject tmpCam = sssCameraByCamera[searchCriteria] as GameObject;
		if (tmpCam == null) {
			Debug.Log("Creating new camera: " + cam.pixelWidth + " x " + cam.pixelHeight + " for scene camera: " + cam);
			String name = "_" + cam.name + "_SSSCamera";
			tmpCam = new GameObject (name, typeof (Camera));
			sssCameraByCamera[searchCriteria] = tmpCam;
		}

		tmpCam.hideFlags = HideFlags.HideAndDontSave;
		tmpCam.transform.position = cam.transform.position;
		tmpCam.transform.rotation = cam.transform.rotation;
		tmpCam.transform.localScale = cam.transform.localScale;
		tmpCam.GetComponent<Camera>().CopyFrom (cam);

		tmpCam.GetComponent<Camera>().enabled = false;
		tmpCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
		tmpCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
		tmpCam.GetComponent<Camera>().backgroundColor = clearColor;
		tmpCam.GetComponent<Camera>().hdr = rtFormat == RenderTextureFormat.ARGBHalf;
		// stretch SSS camera over whole RenderTexture, even if source camera was using only portion of the Viewport
		tmpCam.GetComponent<Camera>().rect = new Rect(0,0,1,1);
		tmpCam.GetComponent<Camera>().aspect = cam.pixelWidth / cam.pixelHeight;

		// render texture allocation
		int rtWidth = (int)(Mathf.Min(cam.pixelWidth, rtResolutionClampSourceWidth) / rtResolutionDivider);
		int rtHeight = (int)(Mathf.Min(cam.pixelHeight, rtResolutionClampSourceHeight) / rtResolutionDivider);

		RenderTexture tmpTargetRt = sssRTByCamera[searchCriteria] as RenderTexture;
		if (null == tmpTargetRt)
		{
			tmpTargetRt = new RenderTexture (rtWidth, rtHeight, 16, rtFormat);//, RenderTextureReadWrite.sRGB);
			Debug.Log("Allocating new skin RT: " + tmpTargetRt.width + " x " + tmpTargetRt.height+ " format " + rtFormat);
		}
		else if (tmpTargetRt.width != rtWidth || tmpTargetRt.height != rtHeight || tmpTargetRt.format != rtFormat ) {
			DestroyImmediate (tmpTargetRt);
			tmpTargetRt = null;
			tmpTargetRt = new RenderTexture (rtWidth, rtHeight, 16, rtFormat);//, RenderTextureReadWrite.sRGB);
			Debug.Log("Allocating new skin RT: " + tmpTargetRt.width + " x " + tmpTargetRt.height+ " format " + rtFormat);
		}
		tmpTargetRt.hideFlags = HideFlags.DontSave;
		sssRTByCamera[searchCriteria] = tmpTargetRt;

		tmpCam.GetComponent<Camera>().targetTexture = tmpTargetRt;
		return tmpCam.GetComponent<Camera>();
	}

	void OnDisable () {
//;;Debug.Log(">CamSkin::OnDisable");

		var cam = sssCameraByCamera[GetComponent<Camera>()] as GameObject;
		sssCameraByCamera.Remove(GetComponent<Camera>());
		if (cam)
			DestroyImmediate (cam);

		var rt = sssRTByCamera[GetComponent<Camera>()] as RenderTexture;
		sssRTByCamera.Remove(GetComponent<Camera>());
		if (rt)
			DestroyImmediate (rt);		

//;;Debug.Log("<CamSkin::OnDisable");
	}

	void OnDestroy () {
//;;Debug.Log(">CamSkin::OnDestroy");
		foreach (DictionaryEntry kvp in sssCameraByCamera)
			DestroyImmediate ((kvp.Value as GameObject));
		sssCameraByCamera.Clear ();
		foreach (DictionaryEntry kvp in sssRTByCamera)
			DestroyImmediate ((kvp.Value as RenderTexture));
		sssRTByCamera.Clear ();
//;;Debug.Log("<CamSkin::OnDestroy");
	}

	void OnPreRender () {
		RenderSSS (GetComponent<Camera>());
	}

#if UNITY_EDITOR
	public void EditorCameraOnPreRender (Camera editorCamera) {
		if (!enableInSceneView)
			return;
//;;Debug.Log(">CamSkin::EditorCameraOnPreRender");
		// This will occasionally produce harmless "Flushing Culler" error in Editor
		// It would be good to get rid of r.sharedMaterial.shader changes to avoid this error in the future
		RenderSSS (editorCamera);
//;;Debug.Log("<CamSkin::EditorCameraOnPreRender");
	}

#endif

	public Material Create1stPassMaterial(Material srcMat, bool eyes)
	{
		Shader newShader;
		if (eyes)
			newShader = skin1stPassEyesShader;
		else
			newShader = skin1stPassShader;
		Material m = new Material(newShader);
		m.CopyPropertiesFromMaterial(srcMat);
		return m;
	}

	public Material CreateFallbackMaterial(Material srcMat)
	{
		Shader newShader = skinFBShader;
		Material m = new Material(newShader);
		m.CopyPropertiesFromMaterial(srcMat);
		return m;
	}

	public Material CreateKeywordMaterial(Material srcMat)
	{
		Shader newShader = skinKeywordShader;
		Material m = new Material(newShader);
		m.CopyPropertiesFromMaterial(srcMat);
		return m;
	}

	private static string[] defaultShaderKeywords = new string[] { "" };
	 
	public void SetupPass(Renderer r, int pass, RenderTexture sssRT = null, bool forceMaterial = false)
	{
		SkinShadingLookupTexture c = r.GetComponent<SkinShadingLookupTexture>();
		if (!Application.isPlaying || !c)
		{
			SetupPassFullButSlow(r, pass, sssRT, forceMaterial);
			return;
		}
		
		c.SetupMaterials(this);
			
		Material[] mats = null;
		if (useKeywordShader)
		{
			mats = c.skinKeywordMaterials;
			foreach (var m in mats)
			{
				// SKIN_PASS_1 SKIN_PASS_1_EYES SKIN_PASS_2 SKIN_PASS_2_EYES SKIN_FALLBACK
				m.shaderKeywords = defaultShaderKeywords;
				switch (pass)
				{
				case 0:
					m.shaderKeywords = new string[] { "SKIN_FALLBACK" };
					break;
				case 1:
					if (c.isEyes)
						m.shaderKeywords = new string[] { "SKIN_PASS_1_EYES" };
					else
						m.shaderKeywords = new string[] { "SKIN_PASS_1" };
					break;
				case 2:
					if (c.isEyes)
						m.shaderKeywords = new string[] { "SKIN_PASS_2_EYES" };
					else
						m.shaderKeywords = new string[] { "SKIN_PASS_2" };
					m.SetTexture ("_SkinScreenspaceLookup", sssRT);
					break;
				}
			}
		}
		else
		{
			if (pass == 0)
				mats = c.skinFallbackMaterials;
			else if (pass == 1)
				mats = c.skin1stPassMaterials;
			else if (pass == 2)
			{
				mats = c.skin2ndPassMaterials;
				foreach (var m in mats)
					m.SetTexture ("_SkinScreenspaceLookup", sssRT);
			}
		}	
		r.materials = mats;
	}
	
	public void SetupPassFullButSlow(Renderer r, int pass, RenderTexture sssRT = null, bool forceMaterial = false)
	{
		SkinShadingLookupTexture c = r.GetComponent<SkinShadingLookupTexture>();
		foreach (Material m in r.sharedMaterials)
		{
			if (SupportedMaterial(m) || forceMaterial)
			{
				if (useKeywordShader)
				{
					m.shader = skinKeywordShader;
					m.shaderKeywords = defaultShaderKeywords;
					// SKIN_PASS_1 SKIN_PASS_1_EYES SKIN_PASS_2 SKIN_PASS_2_EYES SKIN_FALLBACK					
					switch (pass)
					{
					case 0:
						m.shaderKeywords = new string[] { "SKIN_FALLBACK" };
						if (c) c.SetParameters(m);
						break;
					case 1:
						if (c && c.isEyes)
							m.shaderKeywords = new string[] { "SKIN_PASS_1_EYES" };
						else
							m.shaderKeywords = new string[] { "SKIN_PASS_1" };
						break;
					case 2:
						if (c && c.isEyes)
							m.shaderKeywords = new string[] { "SKIN_PASS_2_EYES" };
						else
							m.shaderKeywords = new string[] { "SKIN_PASS_2" };
						m.SetTexture ("_SkinScreenspaceLookup", sssRT);
						if (c) c.SetParameters(m);
						break;
					}
				}
				else
				{

					m.shaderKeywords = defaultShaderKeywords;
					if (pass == 0)
					{
						m.shader = skinFBShader;		
						if (c) c.SetParameters(m);
					}
					else if (pass == 1)
					{
						if(c && c.isEyes)
							m.shader = skin1stPassEyesShader;
						else
							m.shader = skin1stPassShader;
					}
					else if (pass == 2)
					{
						m.shader = skin2ndPassLQShader;
						if (c && c.isEyes)
						{
							m.shader = skin2ndPassLQEyesShader;
						}
						
						// set diffusion texture
						m.SetTexture ("_SkinScreenspaceLookup", sssRT);				
						if (c) c.SetParameters(m);
					}
				}
			}
		}
	}

	private bool UseFallback(Renderer r, Vector3 eye)
	{
		return Vector3.Distance(r.bounds.center, eye) >= fallbackDistance;
	}

	static bool insideSSSPass = false;	
	void RenderSSS (Camera targetCamera) 
	{		
		if (scattering < ScatteringModel.TwoLayersCheap)
		{
			foreach (Renderer r in skinnedMeshRenderers)
				if (r)
					SetupPass(r, 0); // fallback
			return;
		}
		
		if(frameCount == Time.frameCount)
		{
			frameCount = Time.frameCount;
			return;
		}
		frameCount = Time.frameCount;

		if (insideSSSPass)
			return;

		insideSSSPass = true;
		
		// create throwaway camera to render skin into
		Camera cam = GetSSSCamera (targetCamera);
		RenderTexture tmpTargetRt = cam.targetTexture;

		Vector3 eye = targetCamera.transform.position;
		if (cam && skinLayers.value != 0) 
		{
			// assign 1st pass shader and render:
			foreach(Renderer r in skinnedMeshRenderers) 
				if (r && !UseFallback(r, eye))
					SetupPass(r, 1, tmpTargetRt);
				else
					r.enabled = false;

			RenderTexture lastActiveRT = RenderTexture.active;
			if (lastActiveRT && Application.isPlaying)
				lastActiveRT.DiscardContents();
			tmpTargetRt.DiscardContents();

			RenderTexture tmp = RenderTexture.GetTemporary (tmpTargetRt.width, tmpTargetRt.height, 0, rtFormat);
			RenderTexture tmp2 = null;

			// all skinned objects are rendered for the first time here
			cam.cullingMask = skinLayers;
			cam.Render ();
			
			if (skinPostPassMaterial)
			{
				// do skin posting
				// TODO: investigate more optimizations such as stenciling etc

				skinPostPassMaterial.SetFloat ("_BlurWidthScale", blurOffset);

				// cache viewspace quads
				Rect[] rects = new Rect[skinnedMeshRenderers.Length];
				SkinShadingLookupTexture[] skins = new SkinShadingLookupTexture[skinnedMeshRenderers.Length];
				int skinCount = 0;

				foreach(Renderer r in skinnedMeshRenderers)
					if (r)
					{
						rects[skinCount] = ViewspaceQuad (r, cam);
						if (!UseFallback(r, eye))
							skins[skinCount] = r.GetComponent<SkinShadingLookupTexture>();
						else
							skins[skinCount] = null;
						skinCount++;
					}

				float fadeWithDistance = 1.0f;
				{
					RenderTexture.active = tmp;
					for (int i = 0; i < skinCount; ++i)
					{
						var c = skins[i];
						if(c && !c.isEyes)
						{	
							c.SetParameters(skinPostPassMaterial);
							fadeWithDistance = c.GetDistanceFade(targetCamera);
							skinPostPassMaterial.SetFloat ("_BlurWidthScale", c.sssssBlurDistance * blurOffset * fadeWithDistance);
							DrawRect (tmpTargetRt, skinPostPassMaterial, 0, rects[i]);
						}
					}
					tmpTargetRt.DiscardContents();
					RenderTexture.active = tmpTargetRt;
					for (int i = 0; i < skinCount; ++i)
					{
						var c = skins[i];
						if(c && !c.isEyes) 
						{
							c.SetParameters(skinPostPassMaterial);
							fadeWithDistance = c.GetDistanceFade(targetCamera);
							skinPostPassMaterial.SetFloat ("_BlurWidthScale", c.sssssBlurDistance * blurOffset * fadeWithDistance);
							DrawRect (tmp, skinPostPassMaterial, 1, rects[i]);
						}
					}
				}
			}

			RenderTexture.active = lastActiveRT;

			if (tmp) RenderTexture.ReleaseTemporary (tmp);
			if (tmp2) RenderTexture.ReleaseTemporary (tmp2);

			// reset stuff and continue, now we can easily sample from skin 1st pass
			// after this, the rendering of the std camera will happen and all skinned objects will be rendered a 2nd time
			foreach(Renderer r in skinnedMeshRenderers)
				if (r)
				{
					if (UseFallback(r, eye))
					{
						r.enabled = true;
						SetupPass(r, 2, null);
					}
					else
						SetupPass(r, 2, tmpTargetRt);
				}

		}
		
		insideSSSPass = false;
	}

	// renders the rect with specified material/pass m/pass
	private void DrawRect (RenderTexture s, Material m, int pass, Rect r) {
		       
		m.SetTexture ("_MainTex", s);

		GL.PushMatrix ();
		GL.LoadOrtho ();	
	    	
		m.SetPass (pass);	
		
	    GL.Begin (GL.QUADS);
		GL.MultiTexCoord2 (0, r.x, r.y+r.height); 
		GL.Vertex3 (r.x, r.y+r.height, 0.1f); // BL
		GL.MultiTexCoord2 (0, r.x+r.width, r.y+r.height); 
		GL.Vertex3 (r.x+r.width, r.y+r.height, 0.1f); // BR
		GL.MultiTexCoord2 (0, r.x+r.width, r.y); 
		GL.Vertex3 (r.x+r.width, r.y, 0.1f); // TR
		GL.MultiTexCoord2 (0, r.x, r.y); 
		GL.Vertex3 (r.x, r.y, 0.1f); // TL
		GL.End ();
	    GL.PopMatrix ();
	}

	private Rect ViewspaceQuad (Renderer r, Camera cam) {
	// UGLY: calculates camera space aligned (projected) bounding quad for renderer r
		Bounds	b = r.bounds;
			
		Vector3 p1 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(1.0f,0.0f,0.0f)));
		Vector3 p2 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(-1.0f,0.0f,0.0f)));
		Vector3 p3 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(0.0f,1.0f,0.0f)));
		Vector3 p4 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(0.0f,-1.0f,0.0f)));
		Vector3 p5 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(0.0f,0.0f,1.0f)));
		Vector3 p6 = cam.WorldToViewportPoint (b.center + Vector3.Scale(b.extents, new Vector3(0.0f,0.0f,-1.0f)));		
		
		float maxy = -Mathf.Infinity; float miny = Mathf.Infinity;
		float maxx = -Mathf.Infinity; float minx = Mathf.Infinity;

		maxy = Mathf.Max(p1.y, p2.y);
		maxy = Mathf.Max(maxy, p3.y);
		maxy = Mathf.Max(maxy, p4.y);
		maxy = Mathf.Max(maxy, p5.y);
		maxy = Mathf.Max(maxy, p6.y);

		miny = Mathf.Min(p1.y, p2.y);
		miny = Mathf.Min(miny, p3.y);
		miny = Mathf.Min(miny, p4.y);
		miny = Mathf.Min(miny, p5.y);
		miny = Mathf.Min(miny, p6.y);

		maxx = Mathf.Max(p1.x, p2.x);
		maxx = Mathf.Max(maxx, p3.x);
		maxx = Mathf.Max(maxx, p4.x);
		maxx = Mathf.Max(maxx, p5.x);
		maxx = Mathf.Max(maxx, p6.x);

		minx = Mathf.Min(p1.x, p2.x);
		minx = Mathf.Min(minx, p3.x);
		minx = Mathf.Min(minx, p4.x);
		minx = Mathf.Min(minx, p5.x);
		minx = Mathf.Min(minx, p6.x);		

		return new Rect(minx, miny, maxx-minx, maxy-miny);
	}
/*
	void OnGUI ()
	{
		if (!camera)
			return;
		var sss = GetSSSCamera(camera);
		if (!sss)
			return;
		if (sss.targetTexture)
			GUI.DrawTexture (new Rect(0,0,128,128), sss.targetTexture, ScaleMode.ScaleToFit, false);
	}
 */
}

