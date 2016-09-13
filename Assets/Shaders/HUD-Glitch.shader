Shader "Hidden/TheChase/Glitch" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_O1("O1", float) = 0.5
		_O2("O2", float) = 0.1
		_S1("S1", float) = 5.0
		_S2("S2", float) = 150.0
		_T("T", float) = 1.0
	}
	
	CGINCLUDE

	#pragma target 3.0 
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;

	uniform half4	_MainTex_TexelSize;
	uniform half	_O2;
	uniform half	_O1;
	uniform half	_S2;
	uniform half	_S1;
	uniform half	_Factor;
	uniform half	_T;
	uniform half	_Value;
	uniform half	_Span;

	float cubicPulse( float c, float w, float x )
	{
		x = abs(x - c);
		if( x>w )
			return 0.0;
		x /= w;
		return 1.0 - x*x*(3.0-2.0*x);
	}

	half2 hash( float2 p )
	{
		p = half2( dot(p,half2(127.1,311.7)), dot(p,half2(269.5,183.3)) );
		return -1.0 + 2.0*frac(sin(p)*43758.5453123);
	}

	half noise( in half2 p )
	{
		const half K1 = 0.366025404; // (sqrt(3)-1)/2;
		const half K2 = 0.211324865; // (3-sqrt(3))/6;
		half2 i = floor( p + (p.x+p.y)*K1 );
		half2 a = p - i + (i.x+i.y)*K2;
		half2 o = (a.x>a.y) ? half2(1.0,0.0) : half2(0.0,1.0);
		half2 b = a - o + K2;
		half2 c = a - 1.0 + 2.0*K2;
		half3 h = max( 0.5-half3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
		half3 n = h*h*h*h*half3( dot(a,hash(i)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
		return dot( n, half3(70.0,70.0,70.0) );
	}
		
	v2f vert( appdata_img v )
	{
		v2f o;
		float dist = noise( half2( v.texcoord.y * _S1 + _T, 0 ) ) * _O1;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex + float4( dist, 0, 0, 0 ) );
		o.uv = v.texcoord.xy + float2( dist, 0 );
		return o;
	} 
	
	half4 frag(v2f i) : COLOR
	{
		half2 uv = i.uv;
		half dist = noise( half2( uv.y * _S2 + _T, 0 ) ) * _O2;
		half4 baseColor = tex2D( _MainTex, uv );
		half4 distColor = tex2D( _MainTex, half2( uv.x+dist, uv.y ) ).xzyw * 2;
		float f = cubicPulse( _Value, _Span, uv.x );
		return lerp( baseColor, distColor, f );
		//return baseColor + (distColor*f);
		//return half4(f,f,f,1);
	}

	ENDCG 

Subshader {
 Pass {
      ZTest Always Cull Off ZWrite Off
      Fog { Mode off }      

      CGPROGRAM

      #pragma fragmentoption ARB_precision_hint_fastest 
      
      #pragma vertex vert
      #pragma fragment frag

      ENDCG
  }
}
	
Fallback off

} // shader