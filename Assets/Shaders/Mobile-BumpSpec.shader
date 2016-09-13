// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "TheChase/NoFog-Specular-Bumped" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview

inline half4 LightingMobileBlinnPhong (SurfaceOutput s, half3 lightDir, half3 halfDir, half atten)
{
	half diff = max (0, dot (s.Normal, lightDir));
	half nh = max (0, dot (s.Normal, halfDir));
	half spec = pow (nh, s.Specular*128) * s.Gloss;
	
	half4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _SpecColor * spec) * (atten*2);
	c.a = 0.0;
	return c;
}

sampler2D _MainTex;
sampler2D _BumpMap;
half _Shininess;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Mobile/VertexLit"
}
