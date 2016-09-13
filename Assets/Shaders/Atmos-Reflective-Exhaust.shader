Shader "TheChase/Reflective-Exhaust"
{
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_EmissiveTex ("Base(RGB) Mask (A)", 2D) = "white" {}
	_Cold ("Cold Color", Color) = (0,0.1,0.2,1)
	_Warm ("Warm Color", Color) = (0.8,0.7,0.1,1)
	_Intensity ("Intensity", Float) = 0
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}

SubShader {
	LOD 300
	Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert exclude_path:prepass nolightmap noforwardadd halfasview vertex:myvert approxview finalcolor:mycolor
#pragma target 3.0

#include "AtmosBase.cginc"

sampler2D _MainTex;
sampler2D _EmissiveTex;
samplerCUBE _Cube;

fixed4 _Color;
fixed4 _Warm;
fixed4 _Cold;
half _Intensity;

struct Input {
	float2 uv_MainTex;
	float2 uv2_EmissiveTex;
	half3 perVertexRefl;
	half2 fog;	
};

void myvert (inout appdata_full v, out Input data)
{
	UNITY_INITIALIZE_OUTPUT(Input, data);
	float3 viewDir = -ObjSpaceViewDir(v.vertex);
	float3 viewRefl = reflect (viewDir, v.normal);
	data.perVertexRefl = mul ((float3x3)_Object2World, viewRefl);

	data.fog = CalcFogParams(v);
}

void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
{
	fixed4 fogcolor = tex2D (_FogTexture, IN.fog.xy);
	#ifdef UNITY_PASS_FORWARDADD
	fogcolor.rgb = 0;
	#endif
	color.rgb = lerp (color.rgb, fogcolor.rgb, fogcolor.a);
}

void surf (Input IN, inout SurfaceOutput o)
{
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb  * _Color;
	fixed4 reflcol = texCUBE( _Cube, IN.perVertexRefl) * tex.a;
	half3 glow = tex2D( _EmissiveTex, IN.uv2_EmissiveTex );

	glow = glow * lerp( _Cold.rgb, _Warm.rgb, glow.r ) * _Intensity;
	
	o.Emission = reflcol.rgb + glow;
}

ENDCG
}

FallBack "TheChase/Diffuse"
}
