using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SunOcclusion : MonoBehaviour {

	public Light sun;
	public Light sun2;
	public bool occludeGlobalLightOnly = false;
	public Color globalSunColor = Color.white;
	public Color sunColorInShadow = new Color(27f / 255f, 28f / 255f, 34f / 255f, 1f);
	public LayerMask characterLayers = -1;//(1 << LayerMask.NameToLayer("Character")) | (1 << LayerMask.NameToLayer("Skin"));
	public float near = 2.0f;
	public float far = 1000.0f;
	public int rayCount = 5;
	public float receiverLength = 5.0f;
	public float occlusionDistanceFade = 0.75f;
	public Transform receiverAnchor;
	public bool debug = false;
	
	[HideInInspector]
	/*new*/ public Renderer[] affected;
	private bool perRendererOcclusionApplied = false;

	static public Renderer[] CollectRenderers(Transform root)
	{
		var rendererComponents = (root == null) ?
			Object.FindObjectsOfType(typeof(Renderer)) as Renderer[] :
			root.GetComponentsInChildren<Renderer>(false) as Renderer[];
		return rendererComponents;
	}

	void Start () {
		if (characterLayers == -1)
			characterLayers = (1 << LayerMask.NameToLayer("Character")) | (1 << LayerMask.NameToLayer("Skin"));
		if (affected == null || affected.Length == 0)
			affected = CollectRenderers (transform);
	}

	void OnDestroy()
	{
		ResetOcclusion(affected);
	}

	void OnDisable()
	{
		ResetOcclusion(affected);
	}

	private float prevOcclusion = 0f;
	void Update () {
		if (!sun)
			return;

		if (BulletTime.isEditing)
			affected = CollectRenderers(transform);

		float occ = GetOcclusion(receiverAnchor ? receiverAnchor : transform, sun);

		float realDeltaTime = (Time.timeScale < Mathf.Epsilon) ? (Time.deltaTime / Time.timeScale) : 0.1f;
		prevOcclusion = Mathf.Lerp(prevOcclusion, occ, 0.75f * 30f * realDeltaTime);
		ApplyOcclusion(affected, FindRealtimeCharacterKeyLight(), sun, (BulletTime.isEditing) ? occ : prevOcclusion);
	}

	Light FindRealtimeCharacterKeyLight () {
		if (occludeGlobalLightOnly)
			return sun2;
		if (BulletTime.activeLightGroup)
			foreach (Light l in BulletTime.activeLightGroup.lights)
				if ((l.renderMode == LightRenderMode.Auto || l.renderMode == LightRenderMode.ForcePixel) &&
					((l.cullingMask & characterLayers) > 0))
					return l;
		return sun;
	}

	MaterialPropertyBlock SetupOcclusionPropertyBlock(Light l, float occlusion)
	{
		MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
		propBlock.Clear();
		propBlock.AddColor("_LightColor0", l.intensity * Color.Lerp(l.color, sunColorInShadow, occlusion));
		return propBlock;
	}

	void ApplyOcclusion (Renderer[] renderers, Light characterLight, Light worldLight, float occlusion)
	{
		if (occludeGlobalLightOnly)
		{
			if (perRendererOcclusionApplied)
				ResetOcclusion (affected);

			if (characterLight)
				characterLight.color = Color.Lerp(globalSunColor, sunColorInShadow, occlusion);
			if (worldLight)
				worldLight.color = Color.Lerp(globalSunColor, sunColorInShadow, occlusion);
			return;
		}

		// Set primary pixel light color directly
		// it seems to override scene light settings per renderer - that's exactly what we want
		MaterialPropertyBlock charPropertyBlock = SetupOcclusionPropertyBlock (characterLight, occlusion);
		MaterialPropertyBlock worldPropertyBlock = SetupOcclusionPropertyBlock (worldLight, occlusion);

		foreach (Renderer r in renderers)
			if (((1 << r.gameObject.layer) & characterLayers) > 0)
				r.SetPropertyBlock(charPropertyBlock);
			else
				r.SetPropertyBlock(worldPropertyBlock);
		perRendererOcclusionApplied = true;
	}

	void ResetOcclusion (Renderer[] renderers)
	{
		foreach (Renderer r in renderers)
			if (r)
				r.SetPropertyBlock(null);
		perRendererOcclusionApplied = false;	
	}

	#if UNITY_EDITOR
	struct GizmoInfo
	{
		public Vector3 from;
		public Vector3 to;
		public float occlusion;
	}
	/*new*/ private GizmoInfo[] gizmoInfo;
	#endif
	float Raycast (Vector3 o, Vector3 n, float len, int i)
	{
		#if UNITY_EDITOR
		gizmoInfo[i].from = o;
		gizmoInfo[i].to = o + n * len;
		#endif
		RaycastHit hit;
		float occ = Physics.Raycast (o, n, out hit, len) ?
			Mathf.Lerp(1f, (1f - occlusionDistanceFade), hit.distance / len) :
			0.0f;
		#if UNITY_EDITOR
		gizmoInfo[i].occlusion = occ;
		#endif
		return occ;
	}

	float GetOcclusion (Transform from, Light to)
	{
#if UNITY_EDITOR 
		gizmoInfo = new GizmoInfo[rayCount];
#endif
		Vector3 ray = (to.type == LightType.Point || to.type == LightType.Spot)?
			(to.transform.position - from.position):
			(-to.transform.forward * far);

		Vector3 d = ray.normalized;
		float dist = ray.magnitude - near;
		dist = Mathf.Max(Mathf.Epsilon, dist);

		float occlusion = 0.0f;
		for (int q = 0; q < rayCount; ++q)
		{
			Vector3 offset = from.forward * ((float)q / (float)rayCount - 0.5f) * receiverLength;
			occlusion += Raycast(from.position + d * near + offset, d, dist, q);
		}
		return occlusion / (float)rayCount;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (!debug || gizmoInfo == null) return;
		foreach (var g in gizmoInfo)
		{
			Gizmos.color = new Color(0.9f, 0.1f + (1-g.occlusion), 0.1f + (1-g.occlusion), 1.0f);
			Gizmos.DrawLine(g.from, g.to);
		}
	}
	#endif
}
