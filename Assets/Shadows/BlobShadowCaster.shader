Shader "TheChase/Special/BlobShadow-Caster" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
		Blend One One
		ZWrite Off
		Cull Off

		CGPROGRAM
		#pragma surface surf Unlit

		struct Input {
			float2 uv_MainTex;
		};

		fixed3 _DisableBlobShadowCaster;
		sampler2D _MainTex;
		half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
			return half4(s.Albedo.rgb, s.Alpha);
		}
		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb + _DisableBlobShadowCaster.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}
