using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SkinShadingLookupTexture : MonoBehaviour
{
	//public float intensity = 1.0f;
	public float topLayerStrength = 0.3333f;
	public float epiLayerStrength = 0.3333f;
	public float subLayerStrength = 0.3333f;

	public float sssssBlurDistance = 1.5f;
	public float epiRelative = 0.3f;
	public bool fadeWithDistance = true;
	public bool isEyes = false;

	//public float diffuseIntensity = 0.8f;
	public Color keyColor = ColorRGB (150, 150, 150);
	public Color epiColor = ColorRGB (148, 175, 175); 
	public Color scatterColor = ColorRGB (179, 148, 148);

	public float saturation = 1.0f;

	public float reflectivityAt0 = 0.075f;
	public float reflectivityAt90 = 0.55f;
	public float reflectivityFalloff = 3.5f;
	
	public float specularIntensity = 1.0f;
	public float specularShininess = 0.7f;
	
	public float specularIntensity2 = 1.0f;
	public float specularShininess2 = 0.4f;

	public int lookupTextureWidth = 32;
	public int lookupTextureHeight = 256;
	
	public Texture2D lookupTexture;

	private Material[] _skinFallbackMaterials = null;
	private Material[] _skin1stPassMaterials = null;
	private Material[] _skin2ndPassMaterials = null;
	public Material[] skinFallbackMaterials { get { return _skinFallbackMaterials; } }
	public Material[] skin1stPassMaterials { get { return _skin1stPassMaterials; } }
	public Material[] skin2ndPassMaterials { get { return _skin2ndPassMaterials; } }

	private Material[] _skinKeywordMaterials = null;
	public Material[] skinKeywordMaterials { get { return _skinKeywordMaterials; } }
	

	static public ArrayList allSkinShadingComponents = new ArrayList();
	void OnEnable ()
	{
		allSkinShadingComponents.Add (this);
	}
	void OnDisable ()
	{
		allSkinShadingComponents.Remove (this);
	}
	
	void Awake ()
	{
		parametersAreDirty = true;
		if (!lookupTexture)
			Bake ();
	}
	
	public void SetupMaterials(CameraSkinScattering c)
	{
		if (Application.isPlaying && GetComponent<Renderer>() && _skin1stPassMaterials == null)
		{
			var count = GetComponent<Renderer>().sharedMaterials.Length;
			_skin1stPassMaterials = new Material[count];
			_skin2ndPassMaterials = new Material[count];
			_skinFallbackMaterials = new Material[count];
			_skinKeywordMaterials = new Material[count];
			for (int q = 0; q < count; ++q)
			{
				_skin2ndPassMaterials[q] = GetComponent<Renderer>().sharedMaterials[q];
				SetParameters(_skin2ndPassMaterials[q]);
			}
			for (int q = 0; q < count; ++q)
				_skin1stPassMaterials[q] = c.Create1stPassMaterial(_skin2ndPassMaterials[q], isEyes);
			for (int q = 0; q < count; ++q)
				_skinFallbackMaterials[q] = c.CreateFallbackMaterial(_skin2ndPassMaterials[q]);
			for (int q = 0; q < count; ++q)
				_skinKeywordMaterials[q] = c.CreateKeywordMaterial(_skin2ndPassMaterials[q]);
		}
	}
	

#if UNITY_EDITOR
	// Necessary to process Scene/Preview cameras in Editor
	void OnWillRenderObject ()
	{

		CameraSkinScattering sssCamera = GameObject.FindObjectOfType (typeof(CameraSkinScattering)) as CameraSkinScattering;
		if (sssCamera)
			sssCamera.EditorCameraOnPreRender (Camera.current);
	}
#endif

	static Color ColorRGB (int r, int g, int b) {
		return new Color ((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 0.0f);
	}

	public float GetDistanceFade(Camera c)
	{
		if(!fadeWithDistance)
			return 1.0f;

		return 1.0f/(1.0f + Mathf.Max(0.0f, c.WorldToViewportPoint(GetComponent<Renderer>().bounds.center).z));
	}
	
	void CheckConsistency () {
		specularIntensity = Mathf.Max (0.0f, specularIntensity);
		specularShininess = Mathf.Clamp (specularShininess, 0.01f, 1.0f);				
		specularIntensity2 = Mathf.Max (0.0f, specularIntensity2);
		specularShininess2 = Mathf.Clamp (specularShininess2, 0.01f, 1.0f);	
	}

	float PHBeckmann(float ndoth, float m) 
	{
		//ndoth*=4.0f;
		//if(ndoth>1.0f)ndoth=1.0f;

		float roughness = m;
		float mSq = roughness * roughness;
		// though original Beckmann doesn't have division by 4
		// in Engel's book and other implementations on the web have it
		// it makes specular look right as well, so what do I know?
		float a = 1.0f / (4.0f * mSq * Mathf.Pow (ndoth, 4.0f) + 1e-5f); 
		float b = ndoth * ndoth - 1.0f;
		float c = mSq * ndoth * ndoth + 1e-5f;
		
		float r = a * Mathf.Exp (b / c);
		return r;

		/*
	float mSq = roughness * roughness;
	// though original Beckmann doesn't have division by 4
	// in Engel's book and other implementations on the web have it
	// it makes specular look right as well, so what do I know?
	float a = 1.0 / (4.0 * mSq * pow (NdotH, 4.0) + 1e-5f); 
	float b = NdotH * NdotH - 1.0f;
	float c = mSq * NdotH * NdotH + 1e-5f;
	
	return a * pow (3, b / c);
		*/
	}	
	
	Color PixelFunc (float ndotv, float ndoth)
	{
		/*
		float ndoth = ndotl2*2.0f;
		if(ndoth>1.0f) ndoth = ndoth-1.0f;

		float modDiffuseIntensity = diffuseIntensity;//(1f + metalic * 0.25f) * Mathf.Max (0f, diffuseIntensity - (1f-ndoth) * metalic);
	
		// diffuse light
		float t0 = Mathf.Clamp01 (Mathf.InverseLerp (-wrapAround, 1f, ndotl * 2f - 1f));
		float t0s = Mathf.Clamp01 (Mathf.InverseLerp (-wrapAround-scatter, 1f, ndotl * 2f - 1f));

		//float t1 = Mathf.Clamp01 (Mathf.InverseLerp (-1f, Mathf.Max(-0.99f,-wrapAround), ndotl * 2f - 1f));
		Color diffuse = modDiffuseIntensity * Color.Lerp (backColor, keyColor, t0);
		//diffuse += backColor * (1f - modDiffuseIntensity) * Mathf.Clamp01 (diffuseIntensity);

		Color diffuseScatter = modDiffuseIntensity * Color.Lerp (backColor, scatterColor, t0);
		diffuse = Color.Lerp(diffuse, diffuseScatter, Mathf.Clamp01(4.0f*(ndotl2*2.0f-ndotl*1.0f)));

		// schlick
		float fresnel = Mathf.Pow( Mathf.Abs( 1.0f - (ndoth*2.0f-1.0f)), reflectivityFalloff);
		fresnel = reflectivityAt0 * (1.0f-Mathf.Clamp01(fresnel)) + Mathf.Clamp01(fresnel) * reflectivityAt90;

		//diffuse = Color.Lerp(diffuse, scatterColor, ndotl2 * scatter * Mathf.Max(0.0f, 1.0f-(ndotl*0.5f+0.5f) ) );
		
		// Blinn-Phong specular (with energy conservation)
		float n = specularShininess * 128f;
		float energyConservationTerm = ((n + 2f)*(n + 4f)) / (8f * Mathf.PI * (Mathf.Pow (2f, -n/2f) + n)); // by ryg
		//float energyConservationTerm = (n + 8f) / (8f * Mathf.PI); // from Real-Time Rendering
		float specular = specularIntensity * energyConservationTerm * Mathf.Pow (ndoth, n);
	
		// fresnel energy conservation
		Color c = (ndotl2 > 0.5f ? diffuseScatter : (1.0f-fresnel * 0.0f) * diffuse) * intensity + (ndotl2 > 0.5f ? new Color(0f,0f,0f, fresnel) : new Color(0f,0f,0f, specular));

		return c * intensity;
		*/	

		// OLD CYAN BLEED SHIT:
		
		//float ndoth = ndotl2;// *2.0f;
		//if(ndoth>1.0f) ndoth = ndoth-1.0f;

		// pseudo metalic falloff
		//ndotl *= 1.0f;//Mathf.Pow (ndoth, metalic);

		//float modDiffuseIntensity = diffuseIntensity;//(1f + metalic * 0.25f) * Mathf.Max (0f, diffuseIntensity - (1f-ndoth) * metalic);
	
		// diffuse light
		//float t0 = Mathf.Clamp01 (Mathf.InverseLerp (-wrapAround, 1f, ndotl * 2f - 1f));
		//float t0s = Mathf.Clamp01 (Mathf.InverseLerp (-wrapAround-scatter, 1f, ndotl * 2f - 1f));

		//float t1 = Mathf.Clamp01 (Mathf.InverseLerp (-1f, Mathf.Max(-0.99f,-wrapAround), ndotl * 2f - 1f));
		//Color diffuse = modDiffuseIntensity * Color.Lerp (backColor, keyColor, t0);
		//diffuse += backColor * (1f - modDiffuseIntensity) * Mathf.Clamp01 (diffuseIntensity);

		//Color diffuseScatter = modDiffuseIntensity * Color.Lerp (backColor, scatterColor, t0s);

		// schlick // *2.0f-1.0f
		float fresnel = Mathf.Pow( Mathf.Abs( 1.0f - (ndotv)), reflectivityFalloff);
		fresnel = reflectivityAt0 * (1.0f-Mathf.Clamp01(fresnel)) + Mathf.Clamp01(fresnel) * reflectivityAt90;
		
		float specular = 0.0f;
		float specular2 = 0.0f;

		float fMaxSpecMod = 2.0f;// * Mathf.Max(reflectivityAt90, reflectivityAt0);

		float fNormalizationSpec1 = 1.0f;
		float fNormalizationSpec2 = 1.0f;

		fNormalizationSpec1 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess) * fMaxSpecMod, 0.25f, 2.0f);
		fNormalizationSpec2 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess2) * fMaxSpecMod, 0.25f, 2.0f);

		specular = PHBeckmann(ndoth, specularShininess) / fNormalizationSpec1;
		specular2 = PHBeckmann(ndoth, specularShininess2) / fNormalizationSpec2;			

		// fresnel energy conservation
		Color c = new Color(1.0f-fresnel, specular * fresnel, specular2 * fresnel, fresnel);

		return c;	
	}
	
	void TextureFunc (Texture2D tex)
	{
		for (int y = 0; y < tex.height; ++y)
			for (int x = 0; x < tex.width; ++x)
			{
				float w = tex.width-1;
				float h = tex.height-1;
				float vx = x / w;
				float vy = y / h;
				
				float NdotV = vx;
				float NdotH = vy;
				Color c = PixelFunc (NdotV, NdotH);
				tex.SetPixel(x, y, c);
			}
	}
	
	void GenerateLookupTexture (int width, int height) {
		Texture2D tex;
		if (lookupTexture && lookupTexture.width == width && lookupTexture.height == height)
			tex = lookupTexture;
		else
			tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
		
		CheckConsistency ();
		TextureFunc (tex);
		tex.Apply();
		tex.wrapMode = TextureWrapMode.Clamp;

		if (lookupTexture != tex)
			DestroyImmediate (lookupTexture);
		lookupTexture = tex;
	}

	public void Preview () {
		GenerateLookupTexture (16, 128);
	}
	
	public void Bake () {
		GenerateLookupTexture (lookupTextureWidth, lookupTextureHeight);
	}

	float normalizationFactor;
	Color mixedTopLayerColor;
	Color mixedSubLayerColor;
	float fMaxSpecMod;
	float fNormalizationSpec1;
	float fNormalizationSpec2;
	public bool parametersAreDirty = true;
	public void SetParameters (Material m)
	{
		if (parametersAreDirty)
		{
			Color keyColor_ = keyColor;
			float colorLum = keyColor_.r * 0.3f + keyColor_.g * 0.59f + keyColor_.b * 0.11f;
			Vector4 v = new Vector4(colorLum * (1.0f-saturation)+keyColor_.r*saturation,colorLum * (1.0f-saturation)+keyColor_.g*saturation,colorLum * (1.0f-saturation)+keyColor_.b*saturation,1.0f );				
			keyColor_ = Color.Lerp(new Color(colorLum,colorLum,colorLum), keyColor_, saturation);
			keyColor_ = v;
			
			Color epiColor_ = epiColor;
			colorLum = epiColor_.r * 0.3f + epiColor_.g * 0.59f + epiColor_.b * 0.11f;
			v = new Vector4(colorLum * (1.0f-saturation)+epiColor_.r*saturation,colorLum * (1.0f-saturation)+epiColor_.g*saturation,colorLum * (1.0f-saturation)+epiColor_.b*saturation,1.0f );		
			epiColor_ = Color.Lerp(new Color(colorLum,colorLum,colorLum), epiColor_, saturation);
			epiColor_ = v;

			Color scatterColor_ = scatterColor;
			colorLum = scatterColor_.r * 0.3f + scatterColor_.g * 0.59f + scatterColor_.b * 0.11f;
			v = new Vector4(colorLum * (1.0f-saturation)+scatterColor_.r*saturation,colorLum * (1.0f-saturation)+scatterColor_.g*saturation,colorLum * (1.0f-saturation)+scatterColor_.b*saturation,1.0f );
			scatterColor_ = v;

			// the 2 layer model mixes epi into top & sub layers (optimization)
			/*float*/ normalizationFactor = 1.0f/(Mathf.Epsilon + epiLayerStrength + topLayerStrength + subLayerStrength);
			/*Color*/ mixedTopLayerColor = (epiColor_ * (1.0f - epiRelative) * epiLayerStrength + keyColor_ * topLayerStrength) * normalizationFactor;
			/*Color*/ mixedSubLayerColor = (epiColor_ * epiRelative * epiLayerStrength + scatterColor_ * subLayerStrength) * normalizationFactor;
			/*float*/ fMaxSpecMod = 2.0f;// * Mathf.Max(reflectivityAt90, reflectivityAt0);;//2.0f * Mathf.Max(specularIntensity,specularIntensity2);
			/*float*/ fNormalizationSpec1 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess) * fMaxSpecMod, 0.25f, 2.0f) * specularIntensity;
			/*float*/ fNormalizationSpec2 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess2) * fMaxSpecMod, 0.25f, 2.0f) * specularIntensity2;

			parametersAreDirty = false;
		}

		if(m) 
		{
			m.SetColor("_SkinSubLayerColor", mixedSubLayerColor);
			m.SetColor("_SkinTopLayerColor", mixedTopLayerColor);
			m.SetColor("_SkinEpiLayerColor", new Color(0,0,0,0));
			m.SetColor("_SkinFbDiffuse", mixedSubLayerColor+mixedTopLayerColor);
			m.SetVector("_SkinReflectivityMod", new Vector3(fNormalizationSpec1,fNormalizationSpec2,0.25f) );
			m.SetVector("_Roughness", new Vector4(specularShininess,specularShininess2, specularIntensity,specularIntensity2));
		}
	}
	/*
	public void OnRenderObject () 
	{
		// the 2 layer model mixes epi into top & sub layers (optimization)
		float normalizationFactor = 1.0f/(Mathf.Epsilon + epiLayerStrength + topLayerStrength + subLayerStrength);
		Color mixedTopLayerColor = (epiColor * 0.35f * epiLayerStrength + keyColor * topLayerStrength) * normalizationFactor;
		Color mixedSubLayerColor = (epiColor * 0.65f * epiLayerStrength + scatterColor * subLayerStrength) * normalizationFactor;

		Shader.SetGlobalColor("_SkinSubLayerColor", mixedSubLayerColor);
		Shader.SetGlobalColor("_SkinTopLayerColor", mixedTopLayerColor);
		Shader.SetGlobalColor("_SkinEpiLayerColor", new Color(0,0,0,0)); // mixed in to other players

		float fMaxSpecMod = 2.0f * Mathf.Max(specularIntensity,specularIntensity2);
		Shader.SetGlobalFloat("_SkinReflectivityMod", fMaxSpecMod);
	}
	*/
}