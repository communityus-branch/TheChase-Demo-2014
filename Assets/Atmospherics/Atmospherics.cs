using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Atmospherics : MonoBehaviour
{
	[HideInInspector]
	public Texture2D lookupTexture;
	// The mesh used for the skydome
	[HideInInspector]
	public Mesh m_SkyMesh;
	// The material used for the skydome
	//[HideInInspector]
	public Material m_SkyMaterial;
	
	// The sunLight to use. if NULL, sunDir is used instead
	public Transform sunLight;

	public Vector3 sunDir = new Vector3 (-.637f, .158f, .753f);

	[Range(0, 2)]
	public float m_SkyboxIntensity = 1.0f;
	[Range(0.5f, 8.0f)]
	public float m_SkyboxHeight = 2.0f;

	[Range(0.0f, 2.0f)]
	public float m_SkyboxAffectedBySun = 1.0f;
	[Range(0.0f, 2.0f)]
	public float m_SkyboxAffectedByFog = 1.0f;

	//	Editable from da huuud.
	private bool needsBake = false;
	public float SkyboxIntensity { get { return m_SkyboxIntensity; } set { m_SkyboxIntensity = value; } }
	public float SunBrightness { get { return m_SunBrightness; } set { m_SunBrightness = value; needsBake = true; } }
	public float SunGlowSize { get { return m_SunGlowFalloff; } set { m_SunGlowFalloff = value; needsBake = true; } }
	
	// Color of the sun
	public Color m_SunColor = new Color (.98f, .81f, .32f,1);
	private Color m_SunColorCached = Color.black;
	
	// Mulitplier (sun can get pretty bright you know :)
	[Range (0,3)]
	public float m_SunBrightness = 1;

	// How much does the sun spread?
	[Range(0, 1)]
	public float m_SunGlowSize = 0.5f;
	// How much does the sun spread?
	[Range(1, 10)]
	public float m_SunGlowFalloff = 3;

	// Phenomena on antisolar point
	[Range(0, 1)]
	public float m_OpposingSunBrightness = 0.0f;
	// Phenomena on antisolar point
	[Range(1.0f, 20.0f)]
	public float m_OpposingSunGlowFalloff = 3.0f;

	// Color of the sky.
	public Color m_SkyColor = new Color (.21f, .5f, .69f);
	// Color at the horizon. The world will also fade to this over distance
	public Color m_HorizonColor = new Color (.72f, .7f, .71f);
	// Fog over distance
	public AnimationCurve m_FogAmount;
	// Fog due to distance-vs-height
	[Range(0, 10)]
	public float m_FogVerticalDensity = 1.0f;

	[HideInInspector]
	public int lookupTextureWidth = 128;
	[HideInInspector]
	public int lookupTextureHeight = 128;


	static public float FogDistance = 2000.0f;

	void Awake()
	{
		if (!lookupTexture)
			Bake();
	}

	void Reset ()
	{
		// Make a curve that mimics OpenGL Exp2 mode
		m_FogAmount = new AnimationCurve(new Keyframe (0,0), new Keyframe (.89f, .89f));
	}

	// Update is called once per frame
	void Update()
	{
		if(	m_SunColorCached != m_SunColor )
		{
			m_SunColorCached = m_SunColor;
			needsBake = true;
		}
		
		if( needsBake == true )
			Bake();
		
		// Make the skyMesh's bounds really large so it's not cropped
		// The vertex shader pushes out the verts to far clip, so the actual size doesn't matter
		if (m_SkyMesh)
			m_SkyMesh.bounds = new Bounds (Vector3.zero, new Vector3 (100000, 100000, 100000));

		if (lookupTexture)
			Shader.SetGlobalTexture("_FogTexture", lookupTexture);
		
		if (sunLight)
			Shader.SetGlobalVector ("_SunDirection", -sunLight.forward);
		else
			Shader.SetGlobalVector ("_SunDirection", sunDir.normalized);

		float heightWarp = m_FogVerticalDensity / 2000.0f;
		float distanceWarp = 1.0f / FogDistance;
		Shader.SetGlobalVector("_FogWarp", new Vector4(distanceWarp, heightWarp, distanceWarp, 0.0f));
		
		if (m_SkyMesh && m_SkyMaterial) {
			m_SkyMaterial.SetFloat("_SkyboxIntensity", m_SkyboxIntensity);
			m_SkyMaterial.SetFloat("_SunFalloff", m_SunGlowFalloff);
			m_SkyMaterial.SetFloat("_FogHeightFalloff", 8-m_SkyboxHeight);
			m_SkyMaterial.SetVector("_FogSkyboxParams", new Vector4(m_SkyboxAffectedBySun, m_SkyboxAffectedByFog, 0, 0));

			Graphics.DrawMesh(m_SkyMesh, Matrix4x4.identity, m_SkyMaterial, 0);
		}
		else
			Debug.Log ("Atmospherics missing SkyMesh & SkyDome", this);
	}

	void GenerateLookupTexture(int width, int height)
	{
		Texture2D tex;
		if (lookupTexture && lookupTexture.width == width && lookupTexture.height == height)
			tex = lookupTexture;
		else
			tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

		CalculateLightTexture(tex);
		tex.Apply();
		tex.wrapMode = TextureWrapMode.Clamp;

		if (lookupTexture != tex)
			DestroyImmediate(lookupTexture);
		lookupTexture = tex;
	}

	public void Bake()
	{
		needsBake = false;
		GenerateLookupTexture( lookupTextureWidth, lookupTextureHeight );
	}

	void CalculateLightTexture( Texture2D dest )
	{
		Color[] pixels = new Color[dest.width * dest.height];
		int i = 0;
		for (int y = 0; y < dest.height; y++)
		{
			// cos (angle to sun)
			float theta = (float)y / (dest.height - 1) * 2 - 1;

			for (int x = 0; x < dest.width; x++)
			{
				float dist = (float)x / (dest.width - 1);

				// Calculate base color (this is the basic atmosphere)
				Color fogColor = Color.Lerp (m_SkyColor, m_HorizonColor, dist);
				fogColor.a = m_FogAmount.Evaluate(dist);

				// Add the sun hotspot
				Color sunColor = m_SunColor * m_SunBrightness;
				float sunFactor = Mathf.Pow(Mathf.InverseLerp(m_SunGlowSize, 1.0f, Mathf.Clamp01(theta)), m_SunGlowFalloff);
				float sunDistanceFactor = Mathf.Pow(dist * 3 + .3f, Mathf.Clamp01(1 - theta) + 1f);
				sunColor *= sunFactor;
				sunColor *= sunDistanceFactor;

				// Add reflective effect around antisolar point
				float sunFactorOpposing = Mathf.Pow(Mathf.Clamp01(-theta), m_OpposingSunGlowFalloff);
				float sunDistanceFactorOpposing = Mathf.Pow(dist * 3 + .3f, Mathf.Clamp01(1 + theta) + 1f);
				Color sunColorOpposing = Color.Lerp(m_SunColor, m_SkyColor, 0.5f) * m_OpposingSunBrightness;
				sunColor += sunColorOpposing * sunFactorOpposing * sunDistanceFactorOpposing;

				float midIntensity = ((fogColor*fogColor.a).grayscale + sunColor.grayscale) * 0.5f;
				pixels[i] = Color.Lerp(fogColor, sunColor, 0.5f + (sunColor.grayscale - midIntensity));
				pixels[i].a = Mathf.Max(fogColor.a, sunColor.grayscale * 1.0f);
				i++;
			}
		}

		dest.SetPixels (pixels);
		dest.Apply();
	}
}
