Shader "TheChase/NoFog-Reflective-Glossy" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader {
	LOD 100
	Tags { "RenderType"="Opaque" }

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
samplerCUBE _Cube;

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float3 worldRefl;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	o.Gloss = tex.a;
	o.Specular = _Shininess;
	
	fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
	reflcol *= tex.a;
	o.Emission = reflcol.rgb * _ReflectColor.rgb;
	o.Alpha = reflcol.a * _ReflectColor.a;
}
ENDCG
}

FallBack "Mobile/VertexLit"
}
