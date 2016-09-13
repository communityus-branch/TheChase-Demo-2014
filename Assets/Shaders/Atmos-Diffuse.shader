Shader "TheChase/Diffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Fog { Mode Off }

		CGPROGRAM
		#pragma surface surf Lambert vertex:myvert finalcolor:mycolor noforwardadd

		#include "AtmosBase.cginc"
		
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			half2 fog;
		};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
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
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
	Fallback "VertexLit"
}
