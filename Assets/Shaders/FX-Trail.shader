Shader "TheChase/FX/Trail"
{
Properties {
	_Texture1("_Texture1", 2D) = "black" {}
	_MainTexSpeed("_MainTexSpeed", Float) = 0
}
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
CGPROGRAM
#pragma surface surf Lambert approxview
#pragma target 3.0

		uniform sampler2D _Texture1;
		half _MainTexSpeed;

struct Input {
	float2 uv_Texture1;
};

void surf (Input IN, inout SurfaceOutput o) {
				
			float MainTexPos = _Time * _MainTexSpeed;
			float2 MainTexUV=(IN.uv_Texture1.xy) + MainTexPos; 
			fixed4 Tex1=tex2D(_Texture1,MainTexUV);
			o.Albedo = Tex1;
			o.Alpha = 1;
}
ENDCG
}

Fallback "Diffuse"
}
