Shader "TheChase/Character/Skin-FinalPass-Eyes" {
Properties {
	_MainTex ("Albedo (RGB), Parallax (A)", 2D) = "grey" {}
	//_Scatter ("Scatter", 2D) = "black" {}
	_BumpMap ("Normals", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	_Parallax ("Parallax", Float) = 0.3
	_LightDirDependency ("Highlight Light Dependency", Float) = 0.3

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

#pragma glsl
#pragma surface surf PseudoBRDF noambient vertex:PerVertexSkinFb approxview nolightmap noforwardadd

#include "Mobile-Types.cginc"

struct Input {
	coord2 uv_MainTex;
	//float2 uv_BumpMap;
	color3 refl;
	color3 sh;
	//coord3 viewDirTS;
	coord4 screenPos;
};

#include "Mobile-SkinBase.cginc" // depends on Input struct

struct MySurfaceOutput {
	color3 Albedo;
	normal3 Normal;
	color3 Emission;
	color Specular;
	color Gloss;
	color Alpha;
	color3 Refl;
};

color _BentNormals;
color _CubeIntensity;

color3 _SkinReflectivityMod;

coord _Parallax;
color _LightDirDependency;

sampler2D _SkinScreenspaceLookup;
sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
samplerCUBE _Cube;

sampler2D _MainTex;
sampler2D _BumpMap;

inline color4 LightingPseudoBRDF (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	normal3 halfDir = normalize(lightDir*_LightDirDependency + viewDir);

	// dots: N.L, N2.L, (not used), N.V, N.H
	color4 dots = color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir));
	dots = saturate(dots);

	color4 skinLookup = tex2D (_SkinLookupTex, dots.zw);

	color4 c;

	// diffuse
	c.rgb = dots.x * skinLookup.r * s.Albedo * _LightColor0.rgb * 2.0f * atten;

	// reflection(s)
	color3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy) * saturate(1-_LightDirDependency+dots.x) * _LightColor0.rgb;
	c.rgb += (s.Refl.rgb * (1-skinLookup.r) + refl);

	c.a = 0;

	return c;
}

// for parallax thingy
//coord3 viewDirTS = ObjSpaceViewDir(v.vertex);
//TANGENT_SPACE_ROTATION;
//o.viewDirTS = normalize(mul(rotation,viewDirTS));

void surf (Input IN, inout MySurfaceOutput o) {
	color4 tex = tex2D(_MainTex, IN.uv_MainTex);
	coord2 uv = IN.uv_MainTex*2-1; 
	coord2 parallax = coord2(0,0);//(IN.viewDirTS).xy * _Parallax * tex.a;
	uv *= 1.0; uv = uv * 0.5 + 0.5;
	tex = tex2D(_MainTex, uv + parallax);
    o.Normal = UnpackNormal (tex2D(_BumpMap, uv + parallax));
	o.Albedo = tex.rgb * _SkinTopLayerColor.rgb;
	o.Gloss = tex.a;
   	o.Refl = texCUBE(_Cube, IN.refl).rgb * _CubeIntensity;
   	o.Emission = IN.sh * o.Albedo.rgb;

	coord4 screenPos = IN.screenPos;
	#if UNITY_UV_STARTS_AT_TOP
	if (_ProjectionParams.x < 0)
		screenPos.y = 1.0*screenPos.w - screenPos.y;
	#endif
   	o.Emission += tex2Dproj(_SkinScreenspaceLookup, UNITY_PROJ_COORD(screenPos)).rgb * _SkinSubLayerColor.rgb * tex.rgb;
}
ENDCG

	}

Fallback "TheChase/Reflective"
}
