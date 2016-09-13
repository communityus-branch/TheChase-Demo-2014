Shader "TheChase/NoFog-CarPaint" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
	//_ColTex ("ColorMap (RGB)", 2D) = "white" {}


	_ColTex ("Color Tone", Color) = (0.5,0.5,0.5,0.5)
	
		
	_BumpMap ("Normalmap", 2D) = "bump" {}
	//_FlakeMap ("Flakes Normals", 2D) = "bump" {}
	//_FlakesFrequency (" Flakes Frequency", Float) = 1.0
	//_FlakesColor(" Flakes Color", Color) = (0.5,0.5,0.5,0.5)

	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	_ShinyShadingTex ("Shiny Shading Tex", 2D) = "black" {}

	_ReflectionGlossiness (" Reflection gloss", Float) = 0.3

	//_MetallicTone ("Color Tone (Base)", Color) = (0.5,0.5,0.5,0.5)
	_HighlightTone ("Color Tone (Highlight)", Color) = (0.5,0.5,0.5,0.5)

	_ShinyReflectivityMod("-", Vector) = (0.5,0.5,0.5,0.5)
}

SubShader { 
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
	LOD 350
	
CGPROGRAM

#pragma target 3.0

#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:separateSH approxview nolightmap

#include "Mobile-Types.cginc"

struct MySurfaceOutput {
	color3 Albedo;
	normal3 Normal;
	//color3 Flakes;
	color3 Emission;
	color Specular;
	color Gloss;
	color Alpha;
	color3 Refl;
};

color _CubeIntensity;
color _LightProbeIntensity;
color _FlakesFrequency;
color3 _FlakesColor;

color4 _SkinSubLayerColor;
color4 _SkinTopLayerColor;
color4 _SkinEpiLayerColor;

//color3 _MetallicTone;
color3 _HighlightTone;
color3 _ShinyReflectivityMod;

color _ReflectionGlossiness;

sampler2D _ShinyShadingTex;
sampler2D _BRDFTex;
samplerCUBE _Cube; 

sampler2D _MainTex;
//sampler2D _ColTex;
color3 _ColTex;
sampler2D _BumpMap;
sampler2D _FlakeMap;

inline color4 LightingPseudoBRDF (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	// Half vector
	normal3 halfDir = normalize(lightDir + viewDir);

	// dots: N.L, N2.L (not used), N.V, N.H
	// dot(s.Flakes,lightDir)
	color4 dots = color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir));
	dots = saturate(dots);

	color4 shinyLookup = tex2D (_ShinyShadingTex, dots.zw);

	dots = saturate(dots);
	color4 c;

	// OPTIMIZEME: diffuse, is almost zero anyways?
	c.rgb = shinyLookup.rrr * s.Albedo * _LightColor0.rgb * 2.0f * atten;

	// the magic is in the reflection(s)
	color3 refl = (shinyLookup.ggg * _ShinyReflectivityMod.xxx * _HighlightTone + shinyLookup.bbb * s.Albedo * _ShinyReflectivityMod.yyy) * dots.xxx * _LightColor0.rgb;
	c.rgb += (s.Refl.rgb * shinyLookup.aaa + refl) * s.Gloss;

	//c.rgb += PHBeckmann(dots.w,0.05) * _LightColor0.rgb * 2.0;

	c.a = 0;

	return c;
}

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float4 refl;
	color3 sh;
};

void separateSH (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);
	float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);
	o.sh = ShadeSH9 (float4(worldN, 1.0)) * _LightProbeIntensity; // do i really need this?
	o.refl.xyz = reflect(-worldN, normalize((_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz)));
	o.refl.y *= -1; // HACK: for some reason cubemap reflection is upside-down. Need to figure out and fix properly
	o.refl.w = _ReflectionGlossiness;
}

void surf (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
	color4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _ColTex.rgb;
	o.Gloss = tex.a;
   	o.Refl = texCUBE(_Cube, IN.refl).rgb * _CubeIntensity * o.Gloss;
   	o.Emission = IN.sh * o.Albedo.rgb;
}
ENDCG
	}

Fallback "TheChase/Reflective"
}
