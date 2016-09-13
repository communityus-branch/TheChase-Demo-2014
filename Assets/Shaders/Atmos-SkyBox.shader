Shader "TheChase/Skybox" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_PlanetRadius ("PlanetRadius", range (0,1)) = .75
		_AtmosphereHeight ("AtmosphereHeight", range (0,.1)) = .05
		_PlayerPos ("Pos", vector) = (0,1,0, 0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "AlphaTest+100"}
		Pass {
			ZWrite Off
CGPROGRAM

#pragma glsl
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 fogUV : TEXCOORD1;
	fixed fogAmount : TEXCOORD2;
};

float3 _SunDirection;
float _SunFalloff;
float _FogHeightFalloff;
float2 _FogSkyboxParams;

float4 SkyboxVertexShader (float3 pos)
{	
    // Calculate rotation. Using a float3 result, so translation is ignored
    float3 rotatedPosition = mul(UNITY_MATRIX_MV, float4 (pos, 0));
    // Calculate projection, moving all vertices to the far clip plane 
    // (w and z both 1.0)
    return mul(UNITY_MATRIX_P, float4(rotatedPosition, 1)).xyww;    
};

float CalcFogAmount(float3 dir)
{
	return pow (1-dir.y, _FogHeightFalloff);
}

float CalcSunAmount(float3 dir)
{
	return pow(abs(dot (dir, _SunDirection)), _SunFalloff);
}

v2f vert (appdata_base v) 
{
	v2f o;
#if (defined (SHADER_API_D3D11)) || (defined (SHADER_API_D3D11_9X))
	o = (v2f)0;
#endif
	o.pos = SkyboxVertexShader (v.vertex);
	float3 dir = mul (_Object2World, float4(v.vertex.xyz,0)).xyz;
	dir = normalize (dir);
	o.fogUV.x = 1;
	o.fogUV.y = dot (dir, _SunDirection) * .5 + .5;
	o.fogAmount = _FogSkyboxParams.x + CalcFogAmount(dir) * _FogSkyboxParams.y;
	o.uv = v.texcoord.xy;
	return o;
}

fixed _SkyboxIntensity;

sampler2D _FogTexture;
sampler2D _MainTex;
fixed4 _Color;

fixed4 frag (v2f i) : COLOR0
{ 
	//* half4 tex = (texCUBE (_Cube, i.uv.xyz)) * _SkyboxIntensity * _Color;
	//* _Color
	half4 tex = tex2D (_MainTex, i.uv) * _SkyboxIntensity * _Color;
	half4 fogcolor = tex2D (_FogTexture, i.fogUV);
	half4 c = tex + fogcolor * i.fogAmount;
	return c;
}

ENDCG
		}
	}
}
