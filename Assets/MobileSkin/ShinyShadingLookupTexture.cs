using UnityEngine;

[ExecuteInEditMode]
public class ShinyShadingLookupTexture : MonoBehaviour
{
	public float reflectivityAt0 = 0.075f;
	public float reflectivityAt90 = 0.55f;
	public float reflectivityFalloff = 3.5f;
	
	public float specularIntensity = 1.0f;
	public float specularShininess = 0.7f;
	
	public float specularIntensity2 = 1.0f;
	public float specularShininess2 = 0.4f;

	public int lookupTextureWidth = 32;
	public int lookupTextureHeight = 128;
	
	public Texture2D lookupTexture;
	
	void Awake () {
		if (!lookupTexture)
			Bake ();
	}
	
	static Color ColorRGB (int r, int g, int b) {
		return new Color ((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 0.0f);
	}
	
	void CheckConsistency () {
		specularIntensity = Mathf.Max (0.0f, specularIntensity);
		specularShininess = Mathf.Clamp (specularShininess, 0.01f, 1.0f);				
		specularIntensity2 = Mathf.Max (0.0f, specularIntensity2);
		specularShininess2 = Mathf.Clamp (specularShininess2, 0.01f, 1.0f);	
	}

	float PHBeckmann(float ndoth, float m) 
	{

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
		
		//float ndoth = ndotl2;//*2.0f;
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

	public void SetParameters (Material m)
	{
		float fMaxSpecMod = 2.0f;// * Mathf.Max(reflectivityAt90, reflectivityAt0);;//2.0f * Mathf.Max(specularIntensity,specularIntensity2);
		float fNormalizationSpec1 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess), 0.25f,  2.0f) * specularIntensity * fMaxSpecMod;
		float fNormalizationSpec2 = Mathf.Clamp(PHBeckmann(1.0f, specularShininess2), 0.25f, 2.0f) * specularIntensity2 * fMaxSpecMod;
		
		m.SetVector("_ShinyReflectivityMod", new Vector3(fNormalizationSpec1,fNormalizationSpec2,0.0f) );
	}
}