Shader "Hidden/TheChase/Mobile-Skin-Pass-1" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
	_Scatter ("Scatter", 2D) = "black" {}

	_BumpMap ("Normalmap", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	_SkinLookupTex ("Skin Lookup", 2D) = "white" {}
}	
SubShader { 
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }

CGPROGRAM

#pragma target 3.0
#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:PerVertexSkin1st approxview nolightmap noforwardadd

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	fixed3 sh;
	fixed3 refl;
};

#include "Mobile-SkinBase.cginc" // depends on Input struct

struct MySurfaceOutput {
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Normal2;
	fixed3 Emission;
	half Specular;
	fixed Gloss;
	fixed Alpha;
};

fixed _BentNormals;
fixed _CubeIntensity;

sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
sampler2D _Scatter;
samplerCUBE _Cube;

inline fixed4 LightingPseudoBRDF (MySurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
{
	// half vector
	fixed3 halfDir = normalize(lightDir + viewDir);

	fixed3 dots = saturate(fixed3(dot(s.Normal2, lightDir), dot(s.Normal2, viewDir), 0));//dot(-s.Normal2, lightDir)));

	fixed4 skinLookup = tex2D (_SkinLookupTex, dots.yy);
	
	fixed4 c;

	c.rgb = s.Albedo * dots.x * skinLookup.r * _LightColor0.rgb * atten * 2.0;
	
	//fixed3 bs = s.Albedo * _LightColor0.rgb;
	//c.rgb += lerp(Luminance(bs).xxx, bs, 4.0) * s.Gloss * dots.z;

	c.a = 1; // important for SSS masking

	return c;
}

sampler2D _MainTex;
sampler2D _BumpMap;

void surf (Input IN, inout MySurfaceOutput o) 
{
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
    o.Normal2 = lerp(o.Normal, fixed3(0,0,1), _BentNormals);

	//fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

	o.Albedo = 0.5;//tex.rgb;// * _SkinSubLayerColor.rgb;
	o.Gloss = 0;//tex2D(_Scatter, IN.uv_MainTex).r; // hijacking this for the scatter mask
   	o.Emission = IN.sh.rgb * 0.5;//tex.rgb;

   	o.Alpha = 1; // important for SSS masking
}
ENDCG

	}

Fallback "VertexLit"
}
