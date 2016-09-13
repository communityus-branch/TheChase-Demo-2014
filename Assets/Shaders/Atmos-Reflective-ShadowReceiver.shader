Shader "TheChase/Reflective-ShadowReceiver" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader {
	LOD 350
	Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert vertex:myvert finalcolor:mycolor approxview noforwardadd
#pragma target 3.0

#include "AtmosBase.cginc"

sampler2D _MainTex;
samplerCUBE _Cube;

sampler2D _PlanarShadowTex;
float4x4 _World2PlanarShadow;

struct Input {
	float2 uv_MainTex;
	half3 perVertexRefl;
	half2 fog;	
	half4 shadowCoord;
};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
			float3 viewDir = -ObjSpaceViewDir(v.vertex);
			float3 viewRefl = reflect (viewDir, v.normal);
			data.perVertexRefl = mul ((float3x3)_Object2World, viewRefl);

			float4 wpos = mul (_Object2World, v.vertex);
			data.shadowCoord = mul(_World2PlanarShadow, wpos);

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
	fixed4 s = tex2D (_PlanarShadowTex, IN.shadowCoord.xy);
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 reflcol = texCUBE (_Cube, IN.perVertexRefl) * tex.a;
	o.Albedo = tex.rgb * (1-s.a);
	o.Emission = reflcol.rgb * (1-s.a) + s.rgb;
}


ENDCG
}

FallBack "TheChase/Reflective"
}
