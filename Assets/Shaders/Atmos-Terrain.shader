Shader "TheChase/Terrain" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Detail ("Detail", 2D) = "gray" {}
}
SubShader {
	LOD 300
	Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert vertex:myvert finalcolor:mycolor approxview noforwardadd
#pragma target 3.0

#include "AtmosBase.cginc"

sampler2D _MainTex;
sampler2D _Detail;

struct Input {
	float2 uv_MainTex;
	float2 uv_Detail;
	fixed4 color : COLOR;
	half2 fog;
};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);

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
	//fixed3 detail = tex2D (_Detail, IN.uv_Detail).rgb * 2;
	o.Albedo = tex.rgb/* * detail*/ * IN.color.rgb;
}

ENDCG
}

FallBack "TheChase/Diffuse"
}
