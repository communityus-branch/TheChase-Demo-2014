
#ifndef ATMOS_BASE_INCLUDE
#define ATMOS_BASE_INCLUDE

sampler2D _FogTexture;
float3 _SunDirection;
float3 _FogWarp;


half2 CalcFogParams (appdata_full v)
{
	// eye-to-vertex
	float3 wpos = mul (_Object2World, v.vertex);
	float3 toEye = wpos.xyz - _WorldSpaceCameraPos;

	float2 fog;
	// distance
	fog.x = length (toEye * _FogWarp);
	// theta
	fog.y = dot (normalize (toEye), _SunDirection) * .5 + .5;
	return fog;
}


half _FogStart;
fixed4 _FogColor;
half _FogSmoothness;

half3 CalcFogYParams (appdata_full v)
{
	// eye-to-vertex
	float3 wpos = mul (_Object2World, v.vertex);
	float3 toEye = wpos.xyz - _WorldSpaceCameraPos;

	float3 fog;
	// distance
	fog.x = length (toEye * _FogWarp);
	// theta
	fog.y = dot (normalize (toEye), _SunDirection) * .5 + .5;

	// height fog
	fog.z = saturate((_FogStart - wpos.y)/_FogSmoothness);

	return fog;
}

#endif // ATMOS_BASE_INCLUDE