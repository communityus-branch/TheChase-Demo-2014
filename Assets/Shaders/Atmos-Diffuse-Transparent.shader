Shader "TheChase/Diffuse-Transparent" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	SubShader {
		LOD 100
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType" = "Transparent" }
		Fog { Mode Off }

		CGPROGRAM
		#pragma surface surf Lambert alpha vertex:myvert finalcolor:mycolor noforwardadd

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
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "VertexLit"
}
