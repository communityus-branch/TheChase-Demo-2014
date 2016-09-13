Shader "TheChase/Character/Skin-Fallback" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
	_Scatter ("Scatter", 2D) = "black" {}

	_BumpMap ("Normalmap", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	_SkinLookupTex ("Skin Lookup", 2D) = "white" {}

	_SkinFbDiffuse ("Skin Fallback diffuse", Vector) = (0.5,0.5,0.5, 0.5)
}

SubShader { 
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
	LOD 100
	
CGPROGRAM

#pragma target 3.0

#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:PerVertexSkinFb approxview nolightmap noforwardadd finalcolor:mycolor

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	fixed3 refl;
	fixed3 sh;
};

#include "Mobile-SkinBase.cginc" // depends on Input struct

struct MySurfaceOutput {
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Normal2;
	fixed3 Normal3;
	fixed3 Emission;
	half3 Specular;
	fixed Gloss;
	fixed Alpha;
	fixed3 Refl;
};

fixed _BentNormals;
fixed _CubeIntensity;

fixed4 _SkinFbDiffuse;

fixed3 _SkinReflectivityMod;

sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
samplerCUBE _Cube;

sampler2D _MainTex;
sampler2D _BumpMap;

#if 1
color3 _linear2gamma(color3 a)
{
	return pow(a, 1.0/2.2);
}
color3 _gamma2linear(color3 a)
{
	return pow(a, 2.2);
}

color4 _linear2gamma4(color4 a)
{
	return pow(a, 1.0/2.2);
}

color4 _gamma2linear4(color4 a)
{
	return color4(pow(a.rgb, 2.2), a.a);
}
#else
color3 _linear2gamma(color3 a)
{
	return a;
}
color3 _gamma2linear(color3 a)
{
	return a;
}

color4 _linear2gamma4(color4 a)
{
	return a;
}

color4 _gamma2linear4(color4 a)
{
	return a;
}

#endif

void mycolor (Input IN, MySurfaceOutput o, inout color4 c)
{
	c = _linear2gamma4(c);
}

inline fixed4 LightingPseudoBRDF (MySurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
{
	// Half vector
	fixed3 halfDir = normalize(lightDir + viewDir);

	// dots: N.L, N2.L (not used), N.V, N.H
	fixed4 dots = (fixed4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir)));
	fixed2 dotsRB = saturate(fixed2((dot(s.Normal3, lightDir)+0.1)/1.1, dot(s.Normal2, lightDir)));
	fixed4 skinLookup = tex2D (_SkinLookupTex, dots.zw);

	fixed4 c;

	// diffuse
	c.rgb = (_SkinSubLayerColor.rgb*dotsRB.x + _SkinTopLayerColor.rgb*dotsRB.y) * skinLookup.rrr * s.Albedo * _LightColor0.rgb * 2.0f * atten;

	// reflection(s)
	fixed3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy) * dots.x * _LightColor0.rgb;
	c.rgb += ((s.Refl.rgb + s.Specular.rgb * skinLookup.a) * (1.0-skinLookup.r) + refl) * s.Gloss;

	c.a = 0;

	return c;
}

void surf (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
    o.Normal = lerp(o.Normal, fixed3(0,0,1), -_BentNormals * 0.1);
    o.Normal2 = lerp(o.Normal, fixed3(0,0,1), _BentNormals * 0.05);
    o.Normal3 = lerp(o.Normal, fixed3(0,0,1), _BentNormals);

	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

	o.Albedo = _gamma2linear(tex.rgb);
	o.Gloss = tex.a;
   	o.Refl = _gamma2linear(texCUBE(_Cube, IN.refl).rgb) * _CubeIntensity;

   	o.Specular = IN.sh; // ambient specular
   	o.Emission = IN.sh * _gamma2linear(tex.rgb) * _gamma2linear(_SkinFbDiffuse.rgb);
}
ENDCG

	}

	Fallback "TheChase/Reflective"
}
