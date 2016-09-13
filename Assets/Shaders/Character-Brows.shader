Shader "TheChase/Character/Brows" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_TransTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _TransTex;
fixed4 _Color;

struct Input {
	float2 uv_TransTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_TransTex, IN.uv_TransTex);
	o.Albedo = _Color;
	o.Alpha = c.rgb;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
