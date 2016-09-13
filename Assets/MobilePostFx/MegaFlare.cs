// TODO: could implement gradual fade-in in the following manner:
// 1) Render each flare as 1 pixel into offscreen "visibility" buffer (still sampling actual depth-buffer in the vertex shader) and storing visibility there.
// 2) Cycle 2 offscreen "visibility" buffer for every other frame and blend Xpercent of last frame visibility into current frame's buffer.
// 3) Sample offscreen "visibility" buffer, while rendering flares into frame-buffer 
// NOTE: adding/removing flares at run-time can become vert tricky

using UnityEngine;
using System.Collections;

// Inner/Outer defined as separate structures because it is easy to handle different defaults this way
[System.Serializable]
public class MegaFlareInnerModifier
{
	public float regionSize = 0.1f;
	public float flareScale = 1.0f;
	public float intensity = 1.0f;
	public Color color = Color.white;
}

[System.Serializable]
public class MegaFlareOuterModifier
{
	public float regionSize = -1.0f;
	public float flareScale = 0.0f;
	public float intensity = 1.0f;
	public Color color = Color.white;
}

[System.Serializable]
public class MegaFlareLayer
{
	public bool primary = false;
	public bool anamorphic = false;
	public bool cull = false;
	public bool dirt = true;
	public float scale = 0.1f;
	public float aspect = 1.0f;
	public float distance = 0.0f;
	public MegaFlareInnerModifier inner;
	public MegaFlareOuterModifier outer;
	public Vector2 offset = Vector2.zero;
	public Texture texture;
	public Color color = Color.white;
	public bool enabled = true;
}

[System.Serializable]
public class MegaFlareConfig
{
	public bool forceCPU = true;
	public bool debugMode = false;
	public float rayDistanceFudge = 0.25f;
	public float rayDensity = 100f;
	public float distanceCull = 1000f;
	public int maxRaysPerFlare = 7;
	public float maxRayLength = 1000f;
	public float guardBandAngle = 10f;
	public float smoothTime = 0.1f;
}

[RequireComponent (typeof(Camera))]
[ExecuteInEditMode]
public class MegaFlare : MonoBehaviour {
	public MegaFlareConfig configuration;
	public LayerMask onlyInLayer;
	
	public Texture defaultIrisTexture;
	public Texture dirtTexture;
	public float lightCoreScale = 0.1f;
	public float lightCoreAspect = 1.0f;
	public Color primaryColor = Color.white;
	public Color secondaryColor = Color.white;
	public float primaryScale = 1.0f;
	public float secondaryScale = 1.0f;
	public float innerCullSize = 0.3f;
	public float innerCullAspect = 1.0f;
	public float innerCullByDistance = 0.33f;
	public Color innerColor = Color.white;
	public Color outerColor = Color.white;
	public bool ignoreCullOnPrimary = true;
	public bool ignoreDirtOnSecondary = true;
	public bool mirrorAnamorphicLayers = false;
	
	public MegaFlareLayer[] layers = new MegaFlareLayer[1];
	Vector2[] occlusionCache = new Vector2[1];
	
	[HideInInspector] // hide so Shader can't be changed per instance
	public Shader megaFlareShader;
	[HideInInspector] // hide so Shader can't be changed per instance
	public Shader megaFlareOnCPUShader;
	Material megaFlareMaterial;
	
	static Material megaFlareMaterialOnGPU;
	static Material megaFlareMaterialOnCPU;

	void Start () {
		if (megaFlareShader.isSupported && !configuration.forceCPU)
		{
			if (!megaFlareMaterialOnGPU) // rely on the fact that Shader is hidden from user and is the same for all instances
				megaFlareMaterialOnGPU = new Material (megaFlareShader);

			megaFlareMaterial = megaFlareMaterialOnGPU;
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
		else
		{
			if (!megaFlareMaterialOnCPU) // rely on the fact that Shader is hidden from user and is the same for all instances
				megaFlareMaterialOnCPU = new Material (megaFlareOnCPUShader);

			megaFlareMaterial = megaFlareMaterialOnCPU;
		}
	}
	
	#region Flare Objects
	static private int dirtyOnFrame = 0;
	static private ArrayList allMegaFlares = new ArrayList();
	static public void AddFlare (MegaFlareLight l)
	{
		allMegaFlares.Add (l);
		dirtyOnFrame = Time.frameCount;
	}
	static public void RemoveFlare (MegaFlareLight l)
	{
		allMegaFlares.Remove (l);
		dirtyOnFrame = Time.frameCount;
	}

	private int cachedFlaresOnFrame = 0;
	private Transform[] cachedFlares = null;
	private Transform[] flares {
		get {
			if (cachedFlares != null && cachedFlaresOnFrame >= dirtyOnFrame)
				return cachedFlares;

			var filteredMegaFlares = new ArrayList(allMegaFlares.Count);
			foreach (MegaFlareLight f in allMegaFlares)
				if ((1 << f.gameObject.layer & onlyInLayer.value) != 0)
					filteredMegaFlares.Add(f.transform);
			cachedFlares = filteredMegaFlares.ToArray(typeof(Transform)) as Transform[];
			return cachedFlares;
		}
	}
	#endregion

	#region Important Occluders
	static private ArrayList importantOccluders = new ArrayList();
	static public void AddImportantOccluder (MegaFlareImportantOccluder o)
	{
		importantOccluders.Add(o);
		dirtyOnFrame = Time.frameCount;
	}
	static public void RemoveImportantOccluder (MegaFlareImportantOccluder o)
	{
		importantOccluders.Remove(o);
		dirtyOnFrame = Time.frameCount;
	}
	#endregion
	
	void OnPostRender ()
	{
		DrawFlares (layers, flares, megaFlareMaterial);
	}

	private void DoCPUOcclusion (Camera cam, Transform[] xforms)
	{
		int rayTraced = 0;

		if (occlusionCache.Length != xforms.Length)
		{
			occlusionCache = new Vector2[xforms.Length];
			for (int q = 0; q < occlusionCache.Length; ++q)
				occlusionCache[q] = Vector2.one;
		}

		if (!cam)
			return;

		var cameraConeAngle = Mathf.Max(cam.fieldOfView * cam.aspect, cam.fieldOfView);
		var clipBandAngle = 10f;
		var cameraConeCosA = Mathf.Cos((cameraConeAngle + clipBandAngle) * 0.5f * Mathf.Deg2Rad);

		for (int q = 0; q < xforms.Length; ++q)
		{
			var xform = xforms[q];
			if (!xform)
				continue;

			var eye = cam.transform.position;
			var masterRay = xform.position - eye;
			if (Vector3.Dot(masterRay.normalized, cam.transform.forward) < cameraConeCosA ||
				masterRay.magnitude >= configuration.distanceCull)
			{
				occlusionCache[q] = Vector2.one;
				continue; // outside view frustum
			}

			var core = new Vector3(lightCoreScale, lightCoreScale * lightCoreAspect, lightCoreScale);
			var estimatedCoreSize = Vector3.Distance(cam.WorldToViewportPoint(xform.position), cam.WorldToViewportPoint(xform.position + core));
			int rayCount = Mathf.Clamp((int)Mathf.Round(configuration.rayDensity * estimatedCoreSize), 0, configuration.maxRaysPerFlare);
			
			if (rayCount == 0)
			{
				occlusionCache[q] = Vector2.one;
				continue; // too small
			}

			float occlusion = 0f;
			Vector2 orientation = Vector2.zero;
			
			bool occluded = false;
			{ // important occluder
				var ray = xform.position - eye;
				var rayLen = ray.magnitude - Mathf.Max(core.x, core.y);
				rayLen = Mathf.Min(rayLen, configuration.maxRayLength);
				if (rayLen <= Mathf.Epsilon)
					continue;

				RaycastHit hit;
				foreach (MegaFlareImportantOccluder occluder in importantOccluders)
					if (occluder.GetComponent<Collider>().Raycast(new Ray(eye, ray.normalized), out hit, rayLen))
					{
						occluded = true;
						orientation = new Vector2(1f, 1f);
						occlusion = 1f;
						break;
					}
			}
			
			if (!occluded)
			{
				for (int w = 0; w < rayCount; ++w)
				{
					Vector3 rnd = (rayCount > 1)? Random.insideUnitSphere: Vector3.zero;
					var ray = xform.position + Vector3.Scale(rnd, core) - eye;
					var rayLen = ray.magnitude - Mathf.Max(core.x, core.y);
					rayLen = Mathf.Min(rayLen, configuration.maxRayLength);
					if (rayLen > Mathf.Epsilon) rayTraced++;
					if (rayLen <= Mathf.Epsilon)
						continue;
	
					if (!Physics.Raycast(eye, ray.normalized, rayLen))
						continue;
	
					var onX = Mathf.Abs(Vector3.Dot(rnd, cam.transform.right));
					var onY = Mathf.Abs(Vector3.Dot(rnd, cam.transform.up));
					
					orientation.x += onX + (1-onY);
					orientation.y += onY + (1-onX);
					occlusion++;
				}
				
				orientation.Normalize();
				occlusion /= (float)rayCount;
			}

			var orientedOcclusion = orientation * 2.0f * occlusion;
			occlusionCache[q] = Vector2.Lerp(orientedOcclusion, occlusionCache[q],
				configuration.smoothTime);
			//occlusionCache[q] = Vector2.one * occlusion;
		}
	}

	private void DrawFlares( MegaFlareLayer[] layers, Transform[] xforms, Material m )
	{
		if( xforms == null || m == null )
			return;

		if( configuration.forceCPU )
			DoCPUOcclusion( Camera.current, xforms );

		if( configuration.forceCPU && occlusionCache.Length != xforms.Length )
			return;

		GL.PushMatrix();

		float farDistance = 0.0f;
		foreach( var layer in layers )
			if( !layer.primary && !layer.anamorphic )
				farDistance = Mathf.Max( farDistance, layer.distance );
		var normalizedInnerCullByDistance = innerCullByDistance / ( ( farDistance > 1e-3f ) ? farDistance : 1.0f );

		bool primary = true;
		foreach( var layer in layers )
		{
			primary |= layer.primary;

			if( !layer.enabled )
			{
				primary = false;
				continue;
			}

			float scale = layer.scale * ( ( primary ) ? primaryScale : secondaryScale );
			bool dirt = ( dirtTexture != null ) && layer.dirt && ( primary || !ignoreDirtOnSecondary );
			bool cull = layer.cull && ( !primary || !ignoreCullOnPrimary );
			Vector2 cullParams = new Vector2( innerCullSize + normalizedInnerCullByDistance * layer.distance - 0.5f, innerCullAspect );
			cullParams.y *= cullParams.x;
			cullParams.x = 1.0f / cullParams.x;
			cullParams.y = 1.0f / cullParams.y;

			int anamorphicPasses = ( mirrorAnamorphicLayers && layer.anamorphic && !primary && Mathf.Abs( layer.distance ) > 1e-3f ) ? 2 : 1;
			for( int q = 0; q < anamorphicPasses; ++q )
			{
				m.SetTexture( "_MainTex", ( layer.texture != null ) ? layer.texture : defaultIrisTexture );
				m.SetTexture( "_DirtTex", dirtTexture );
				m.SetVector( "_Parameters", new Vector4( scale, scale * layer.aspect, lightCoreScale, lightCoreScale * lightCoreAspect ) );
				m.SetVector( "_Parameters2", new Vector4( layer.anamorphic ? 1f : 0f, layer.distance * ( ( q == 0 ) ? 1f : -1f ), layer.offset.x, layer.offset.y ) );
				m.SetVector( "_Parameters3", new Vector4( primary ? 1f : 0f, primary ? 1f : 0f, cullParams.x, cullParams.y ) );
				m.SetVector( "_Parameters4", new Vector4(
					Mathf.Max( 1e-3f, layer.inner.regionSize ),
					Mathf.Max( 1e-3f, primary ? layer.outer.regionSize + 1 : 1f ), layer.inner.flareScale, primary ? layer.outer.flareScale : 1f ) );
				m.SetColor( "_Color", layer.color * ( primary ? primaryColor : secondaryColor ) );
				m.SetColor( "_InnerColor", innerColor * layer.inner.color * layer.inner.intensity );
				m.SetColor( "_OuterColor", outerColor * layer.outer.color * layer.inner.intensity );
				m.SetPass( ( dirt ? 1 : 0 ) + ( cull ? 2 : 0 ) );

				if( configuration.debugMode )
				{
					m.SetTexture( "_MainTex", null );
					Color debugColor = layer.color * ( primary ? primaryColor : secondaryColor ) * 0.33f;
					if( debugColor.grayscale < 0.033f )
					{
						debugColor.r = Mathf.Max( 0.1f, debugColor.r );
						debugColor.g = Mathf.Max( 0.1f, debugColor.g );
						debugColor.b = Mathf.Max( 0.1f, debugColor.b );
					}
					debugColor.a = 1.0f;
					m.SetColor( "_Color", debugColor );
					m.SetPass( ( dirt ? 1 : 0 ) + ( cull ? 2 : 0 ) );
				}

				GL.Begin( GL.QUADS );
				for( int w = 0; w < xforms.Length; ++w )
				{
					var xform = xforms[ w ];
					if( !xform )
						continue;

					float occlusionX = 0f;
					float occlusionY = 0f;
					if( configuration.forceCPU )
					{
						occlusionX = occlusionCache[ w ].x;
						occlusionY = occlusionCache[ w ].y;
						if( occlusionX >= 0.99f && occlusionY >= 0.99f )
							continue;
					}

					Vector3 p = xform.position;
					GL.MultiTexCoord2( 0, 0f, 0f );
					GL.MultiTexCoord2( 1, -1f, 0f );
					GL.Color( new Color( occlusionX, occlusionY, 0f, 1f ) );
					GL.Vertex( p );
					GL.MultiTexCoord2( 0, 0f, 1f );
					GL.MultiTexCoord2( 1, 0f, 1f );
					GL.Color( new Color( occlusionX, occlusionY, 0f, 1f ) );
					GL.Vertex( p );
					GL.MultiTexCoord2( 0, 1f, 1f );
					GL.MultiTexCoord2( 1, 1f, 0f );
					GL.Color( new Color( occlusionX, occlusionY, 0f, 1f ) );
					GL.Vertex( p );
					GL.MultiTexCoord2( 0, 1f, 0f );
					GL.MultiTexCoord2( 1, 0f, -1f );
					GL.Color( new Color( occlusionX, occlusionY, 0f, 1f ) );
					GL.Vertex( p );
				}
				GL.End();
			}
			primary = false;
		}
		GL.PopMatrix();
	}
}
 