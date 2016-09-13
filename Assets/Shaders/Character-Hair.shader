Shader "TheChase/Character/Hair" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
	_TransTex ("Alpha (RGB)", 2D) = "white" {}
	_AnimationSpeed ("Speed", Range(0.0, 200)) = 150
	_NoiseScale ("NoiseScale", Range(0.0, 32)) = 1
	_VertexFactor( "Vertex wiggleability factor", Range(0.0, 0.1)) = 0.01
	_NormalFactor( "Normal wiggleability factor", Range(0.0, 1)) = 0.5
	_RootGloss( "Hair root gloss", Range(0.0, 1.0)) = 0.0
	_RootAlpha( "Hair root alpha", Range(0.0, 1.0)) = 1
	_TipGloss( "Hair tip gloss", Range(0.0, 1.0)) = 0.0
	_TipAlpha( "Hair tip alpha", Range(0.0, 1.0)) = 1.0
	_HairInfluenceValue( "magic value", Range(-0.25, 0.25) ) = -0.007462686
	_HairInfluenceRange( "magic range", Range(0.0, 0.25) ) = 0.09208956
	_HairInfluenceValue2( "magic value2", Range(-0.25, 0.25) ) = -0.007462686
	_HairInfluenceRange2( "magic range2", Range(0.0, 0.25) ) = 0.09208956
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	Blend SrcAlpha OneMinusSrcAlpha
	LOD 100
	Cull OFF

CGPROGRAM
#pragma target 3.0
#pragma surface surf BlinnPhong alpha vertex:vertModifier 

fixed4 _Color;
half _Shininess;

sampler2D _TransTex;
uniform half3 _Wind;

uniform half _Animation; 
uniform half _AnimationSpeed;
uniform half _NoiseScale;
uniform half _VertexFactor;
uniform half _NormalFactor;

uniform half _HairInfluenceValue;
uniform half _HairInfluenceRange;

uniform half _HairInfluenceValue2;
uniform half _HairInfluenceRange2;

uniform half _T;
uniform half _RootGloss;
uniform half _RootAlpha;
uniform half _TipGloss;
uniform half _TipAlpha;

struct Input {
	float2 uv2_TransTex;
	fixed2 glossAlpha;
};

float2 hash( float2 p )
{
	p = float2( dot(p,float2(127.1,311.7)), dot(p,float2(269.5,183.3)) );
	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

float noise( in float2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;
	float2 i = floor( p + (p.x+p.y)*K1 );
    float2 a = p - i + (i.x+i.y)*K2;
    float2 o = (a.x>a.y) ? float2(1.0,0.0) : float2(0.0,1.0);
    float2 b = a - o + K2;
	float2 c = a - 1.0 + 2.0*K2;
    float3 h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
	float3 n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    return dot( n, 70.0 );
}

float cubicPulse( float c, float w, float x )
{
	x = abs(x - c);
	if( x>w )
		return 0.0;
	x /= w;
	return 1.0 - x*x*(3.0-2.0*x);
}

void vertModifier( inout appdata_full v, out Input IN )
{
	UNITY_INITIALIZE_OUTPUT(Input, IN);

	//	The following tries to decrese the influence of the wind on the hairs in the back of the head,
	//	by combining two factors along the X & Y planes of the hair mesh. (or something, it's in an unintuitive space!)
	//	This is hard coded to the current male hair mesh, pretty much! :)
	float localWiggleReductionFactor = 0.2 + (1-saturate((cubicPulse( _HairInfluenceValue, _HairInfluenceRange, v.vertex.y ) * cubicPulse( _HairInfluenceValue2, _HairInfluenceRange2, v.vertex.x )))) * 0.8;
	//IN.color = lerp( float4(1,0,0,1), float4(0,1,0,1), localWiggleReductionFactor );

	half animOffset = _T * _AnimationSpeed;
	half x = v.texcoord1.x * localWiggleReductionFactor;

	half vertexFactor = x * _VertexFactor;
	half normalFactor = x * _NormalFactor;

	float3 worldNormal = mul( (float3x3)_Object2World, SCALED_NORMAL );
	vertexFactor *= lerp( 0.2, 1, saturate( dot( worldNormal, _Wind ) ) );

	half v_disp = 0.5 + noise( (v.vertex.xz*_NoiseScale) + animOffset ) * 0.5;
	half n_disp = v_disp * 1.25;

	v.vertex.xyz += v.normal.xyz * (v_disp * vertexFactor );
	v.normal = normalize( v.normal + (n_disp * normalFactor ));
	
	
	half f = v.texcoord1.x;	//	scalp(1.0) -> tip(0.0)
	IN.glossAlpha.x = lerp( _RootGloss, _TipGloss, f );
	IN.glossAlpha.y = lerp( _RootAlpha, _TipAlpha, f );
}

void surf( Input IN, inout SurfaceOutput o )
{
	half4 transtex = tex2D(_TransTex, IN.uv2_TransTex);
	o.Albedo = _Color.rgb;
	half v = transtex.r;//smoothstep( 0, 1, transtex.a );
	o.Gloss = IN.glossAlpha.x * v;
	o.Alpha = IN.glossAlpha.y * v;
	o.Specular = _Shininess;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
