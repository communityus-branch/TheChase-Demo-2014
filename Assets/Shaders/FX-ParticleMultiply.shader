Shader "TheChase/FX/Particles-Multiply" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		//Tags { "RenderType"="Opaque" }
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend Zero SrcColor
		Cull Off Lighting Off ZWrite Off Fog { Color (1,1,1,1) }

		LOD 200
		
		CGPROGRAM
		#pragma surface surf Unlit
		
		fixed4 LightingUnlit (SurfaceOutput s, fixed3 lightDir, half atten) {
			return fixed4(s.Albedo.rgb, 1);
		}

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR0;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb + (1 - IN.color.aaa);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
