Shader "TheChase/Character/Skin-FinalPass" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
	_Scatter ("Scatter", 2D) = "black" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	_SkinLookupTex ("Skin Lookup", 2D) = "white" {}
	_SkinScreenspaceLookup ("SSS Lookup", 2D) = "white" {}
}

SubShader { 
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
	LOD 300
	
CGPROGRAM

#pragma target 3.0

#pragma debug

#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:PerVertexSkin2nd approxview nolightmap noforwardadd finalcolor:mycolor

#include "Mobile-Types.cginc"

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	color3 refl;
	color3 sh;
	coord4 screenPos;
};

#include "Mobile-SkinBase.cginc" // depends on Input struct

struct MySurfaceOutput {
	color3 Albedo;
	normal3 Normal;
	color3 Emission;
	color3 Specular;
	color Gloss;
	color Alpha;
	color3 Refl;
};

color _BentNormals;
color _CubeIntensity;

/*color3*/ half3 _SkinReflectivityMod; // requires half precision!

sampler2D _SkinScreenspaceLookup;
sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
sampler2D _Scatter;
samplerCUBE _Cube;

sampler2D _MainTex;
sampler2D _BumpMap;

void mycolor (Input IN, MySurfaceOutput o, inout color4 c)
{
	c = linear2gamma4(c);
}

inline color4 LightingPseudoBRDF (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	// Half vector
	normal3 halfDir = normalize(lightDir + viewDir);

	// dots: N.L, N2.L (not used), N.V, N.H
	color4 dots = color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir));
	dots = saturate(dots);

	color4 skinLookup = (tex2D (_SkinLookupTex, dots.zw));

	color4 c;

	// diffuse
	c.rgb = dots.x * s.Albedo * _LightColor0.rgb * skinLookup.r * 2.0f;

	// reflection(s)
	color3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy ) * _LightColor0.rgb;
	c.rgb += ((s.Refl.rgb /* + s.Specular.rgb * skinLookup.a*/ ) * (skinLookup.a) + refl) * s.Gloss;

	c.a = 0;

	return c;
}

void surf (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));

	color4 tex = tex2D(_MainTex, IN.uv_MainTex);
	color3 albedoTex = gamma2linear(tex.rgb);
	o.Albedo = albedoTex * _SkinTopLayerColor.rgb;
	o.Gloss = tex.a;
   	o.Refl = gamma2linear(texCUBE(_Cube, IN.refl).rgb) * _CubeIntensity;// * saturate(o.Gloss-0.25)*12;
   	o.Emission = IN.sh * albedoTex;
   	//o.Specular = 0.5; // hackyhack but nice: ambient specular

	coord4 screenPos = IN.screenPos;

	#if UNITY_UV_STARTS_AT_TOP
	if (_ProjectionParams.x < 0)
		screenPos.y = 1.0*screenPos.w - screenPos.y;
	#endif

   	o.Emission += tex2Dproj(_SkinScreenspaceLookup, UNITY_PROJ_COORD(screenPos)).rgb * (2.0 * albedoTex * _SkinSubLayerColor.rgb + albedoTex * 0.15);

}


ENDCG

	}

Fallback "TheChase/Reflective"
}
