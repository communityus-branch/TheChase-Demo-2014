Shader "TheChase/Reflective-Self-Illum" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Illum ("Illumin (RGB)", 2D) = "black" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
	_EmissionLM ("Emission (Lightmapper)", Float) = 0
}
SubShader {
	LOD 300
	Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert vertex:myvert finalcolor:mycolor approxview noforwardadd
#pragma target 3.0

#include "AtmosBase.cginc"

sampler2D _MainTex;
sampler2D _Illum;
samplerCUBE _Cube;

struct Input {
	float2 uv_MainTex;
	float2 uv_Illum;
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


void surf (Input IN, inout SurfaceOutput o) {
	fixed4 glow = tex2D( _Illum, IN.uv_Illum);
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb;
	fixed4 reflcol = texCUBE (_Cube, IN.perVertexRefl) * tex.a;
	o.Emission = reflcol.rgb + glow;
}

ENDCG
}

FallBack "TheChase/Diffuse"
}
