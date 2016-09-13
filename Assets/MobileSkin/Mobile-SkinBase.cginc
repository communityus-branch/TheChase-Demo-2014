
#ifndef MOBILE_SKIN_BASE_INCLUDE
#define MOBILE_SKIN_BASE_INCLUDE

#include "UnityCG.cginc"
#include "Mobile-Types.cginc"

half _LightProbeIntensity;
color3 _SkinTopLayerColor;
color3 _SkinSubLayerColor;
color3 _SkinEpiLayerColor;

#if 1
/*
color3 linear2gamma(color3 a)
{
	return pow(a, 1.0/2.2);
}
color3 gamma2linear(color3 a)
{
	return pow(a, 2.2);
}
color4 linear2gamma4(color4 a)
{
	return pow(a, 1.0/2.2);
}
color4 gamma2linear4(color4 a)
{
	return color4(pow(a.rgb, 2.2), a.a);
}
*/
color3 linear2gamma(color3 a)
{
	return sqrt(a); // pow(a, 1.0/2.2);
}
color3 gamma2linear(color3 a)
{
	return a*a; // pow(a, 2.2);
}
color4 linear2gamma4(color4 a)
{
	return sqrt(a); // pow(a, 1.0/2.2);
}
color4 gamma2linear4(color4 a)
{
	return color4(a.rgb*a.rgb, a.a); //color4(pow(a.rgb, 2.2), a.a);
}

#else
color3 linear2gamma(color3 lin) { return lin; }
color3 gamma2linear(color3 srgb) { return srgb; }
color4 linear2gamma4(color4 lin) { return lin; }
color4 gamma2linear4(color4 srgb) { return srgb; }
#endif


void PerVertexSkin2nd (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);
	float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);
	o.sh = ShadeSH9 (float4(worldN, 1.0)) * _LightProbeIntensity * _SkinTopLayerColor;
	o.refl = reflect(worldN, normalize((_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz)));
}

void PerVertexSkin1st (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);
	float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);
	o.sh = ShadeSH9 (float4(worldN, 1.0)) * _LightProbeIntensity;
	o.refl = reflect(worldN, normalize((_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz)));
}

void PerVertexSkinFb (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);
	float3 worldN = mul ((float3x3)_Object2World, SCALED_NORMAL);
	o.sh = ShadeSH9 (float4(worldN, 1.0)) * _LightProbeIntensity;
	o.refl = reflect(worldN, normalize((_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz)));
}

color PHBeckmann(color ndoth, color m) 
{
	color roughness = m;
	color mSq = roughness * roughness;

	color a = 1.0f / (4.0f * mSq * pow (ndoth, 4.0f) + 1e-5f); 
	color b = ndoth * ndoth - 1.0f;
	color c = mSq * ndoth * ndoth + 1e-5f;
	
	color r = a * exp (b / c);
	return r;
}	

#endif