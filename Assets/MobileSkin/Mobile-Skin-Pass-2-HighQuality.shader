Shader "OLE/Mobile-Skin-2-JustForReference" {
Properties {
	_MainTex ("Albedo (RGB), Parallax (A)", 2D) = "grey" {}
	_Scatter ("Scatter", 2D) = "black" {}

	_BumpMap ("Normalmap", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	_SkinLookupTex ("Skin Lookup", 2D) = "white" {}
	_SkinScreenspaceLookup ("SSS Lookup", 2D) = "white" {}

	_Roughness ("- (internal)", Vector) = (.5, .5, .0, .0)
	//_SkinReflectivityMod ("(Internal, refl mod)", Float) = 0.5
}	
SubShader { 
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
	LOD 400
	
CGPROGRAM

#pragma target 3.0
#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:PerVertexSkin2nd approxview nolightmap noforwardadd

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
	normal3 Normal2;
	color3 Emission;
	color3 Specular;
	color Gloss;
	color Alpha;
	color3 Refl;
};

color _BentNormals;
color _CubeIntensity;

color3 _SkinReflectivityMod;

color4 _Roughness;

sampler2D _SkinScreenspaceLookup;
sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
samplerCUBE _Cube;	
sampler2D _MainTex;
sampler2D _BumpMap;

inline color4 LightingPseudoBRDF (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	// Half vector
	normal3 halfDir = normalize(lightDir + viewDir);

	// N.L, N2.L, N.V, N.H
	color4 dots = saturate(color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir)));
	color4 skinLookup = tex2D (_SkinLookupTex, dots.zw);

	color4 c;
	
	// diffuse
	c.rgb = dots.x * skinLookup.r * s.Albedo * _LightColor0.rgb * 2.0f * atten;

	// reflection(s)
	color3 refl = dot(color2(PHBeckmann(dots.w,_Roughness.x),PHBeckmann(dots.w,_Roughness.y)), color2((1,1)-skinLookup.rr)*_Roughness.zw) * dots.x * _LightColor0.rgb;
	c.rgb += ((s.Refl.rgb + s.Specular.rgb * skinLookup.a) * (1.0-skinLookup.r) + refl) * s.Gloss;

	c.a = 0;

	return c;
}

void surf (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
    o.Normal2 = lerp(o.Normal, normal3(0,0,1), 0.8);

	color4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _SkinTopLayerColor.rgb;
	o.Gloss = 1.0;//tex.a;
   	o.Refl = texCUBE(_Cube, IN.refl).rgb * _CubeIntensity;
   	o.Emission = IN.sh * tex.rgb;
   	o.Specular = IN.sh; // ambient specular
   	o.Alpha = 1.0-tex.a;

	coord4 screenPos = IN.screenPos;
   	o.Emission += tex2Dproj(_SkinScreenspaceLookup, UNITY_PROJ_COORD(screenPos)).rgb * 2.0 * tex.rgb * _SkinSubLayerColor.rgb;
}
ENDCG

	}

Fallback "VertexLit"
}
