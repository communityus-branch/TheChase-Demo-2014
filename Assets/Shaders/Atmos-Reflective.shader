Shader "TheChase/Reflective" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader {
	LOD 300
	Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert vertex:myvert finalcolor:mycolor approxview
#pragma target 3.0

#include "AtmosBase.cginc"

sampler2D _MainTex;
samplerCUBE _Cube;

struct Input {
	float2 uv_MainTex;
	half3 perVertexRefl;
	fixed4 color : COLOR;
	half2 fog;
};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
			float3 viewDir = -ObjSpaceViewDir(v.vertex);
			float3 viewRefl = reflect (viewDir, v.normal);
			data.perVertexRefl = mul ((float3x3)_Object2World, viewRefl);

			data.fog = CalcFogParams(v);
			data.color = v.color;
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
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * IN.color.rgb;
	fixed4 reflcol = texCUBE (_Cube, IN.perVertexRefl) * tex.a * IN.color.a;
	o.Emission = reflcol.rgb;
}

ENDCG
}

FallBack "TheChase/Diffuse"
}
