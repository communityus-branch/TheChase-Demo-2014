
Shader "Hidden/MobileBloom" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bloom ("Bloom (RGB)", 2D) = "black" {}
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _Bloom;
				
		uniform half4 _MainTex_TexelSize;
		
		uniform half4 _Parameter;
		uniform half4 _OffsetsA;
		uniform half4 _OffsetsB;
		
		#define ONE_MINUS_THRESHHOLD_TIMES_INTENSITY _Parameter.w
		#define THRESHHOLD _Parameter.z

		struct v2f_simple {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
        	#if SHADER_API_D3D9
				half2 uv2 : TEXCOORD1;
			#endif
		};

		struct v2f_withMaxCoords {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv20 : TEXCOORD1;
			half2 uv21 : TEXCOORD2;
			half2 uv22 : TEXCOORD3;
			half2 uv23 : TEXCOORD4;		
		};		

		struct v2f_withBlurCoords {
			half4 pos : SV_POSITION;
			half2 uv20 : TEXCOORD0;
			half2 uv21 : TEXCOORD1;
			half2 uv22 : TEXCOORD2;
			half2 uv23 : TEXCOORD3;

		};	
		
		v2f_simple vertBloom (appdata_img v)
		{
			v2f_simple o;
			
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
        	o.uv = v.texcoord;		
        	
        	#if SHADER_API_D3D9
        	
        	o.uv2 = v.texcoord;			
        	        		        	
        		if (_MainTex_TexelSize.y < 0.0)
        			o.uv.y = 1.0 - o.uv.y;
        	
        	#endif
        	        	
			return o; 
		}

		v2f_simple vertBloomAberration (appdata_img v)
		{
			v2f_simple o;
			
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
        	o.uv = v.texcoord;		
        	
        	#if SHADER_API_D3D9
        	
        	o.uv2 = v.texcoord;			
        	        		        	
        		if (_MainTex_TexelSize.y < 0.0)
        			o.uv.y = 1.0 - o.uv.y;
        	
        	#endif
        	        	
			return o; 
		}

		v2f_withMaxCoords vertMax (appdata_img v)
		{
			v2f_withMaxCoords o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
        	o.uv20 = v.texcoord + _OffsetsA.xy;					
			o.uv21 = v.texcoord + _OffsetsA.zw;		
			o.uv22 = v.texcoord + _OffsetsB.xy;		
			o.uv23 = v.texcoord + _OffsetsB.zw;		
        	o.uv = v.texcoord;
			return o; 
		}			

		v2f_withBlurCoords vertBlur (appdata_img v)
		{
			v2f_withBlurCoords o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
        	o.uv20 = v.texcoord + _OffsetsA.xy;					
			o.uv21 = v.texcoord + _OffsetsA.zw;		
			o.uv22 = v.texcoord + _OffsetsB.xy;		
			o.uv23 = v.texcoord + _OffsetsB.zw;	
			return o; 
		}		

		// TODO: optimize and move stuffz to vertex shader

		fixed4 fragBloomAberration ( v2f_simple i ) : COLOR
		{	
        	#if SHADER_API_D3D9
			
			half4 color = tex2D(_MainTex, i.uv);

			half4 blurred = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1.5, 1.5));
		 	blurred += tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(1.5, 1.5));
			blurred += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(-1.5, 1.5));
			blurred += tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(-1.5, 1.5));

			half4 bloom = tex2D(_Bloom, i.uv2);
			color.rb = lerp(color.rb, blurred.rb/4, saturate(dot(bloom.rgb,4)));

			return color + bloom;
			
			#else

			half4 color = tex2D(_MainTex, i.uv);

			half4 blurred = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1.5, 1.5));
		 	blurred += tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(1.5, 1.5));
			blurred += tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(-1.5, 1.5));
			blurred += tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(-1.5, 1.5));

			half4 bloom = tex2D(_Bloom, i.uv);
			color.rb = lerp(color.rb, blurred.rb/4, saturate(dot(bloom.rgb,4)));

			return color + bloom;
						
			#endif
		} 
						
		fixed4 fragBloom ( v2f_simple i ) : COLOR
		{	
        	#if SHADER_API_D3D9
			
			half4 color = tex2D(_MainTex, i.uv);
			return color + tex2D(_Bloom, i.uv2);
			
			#else

			half4 color = tex2D(_MainTex, i.uv);
			return color + tex2D(_Bloom, i.uv);
						
			#endif
		} 
		
		fixed4 fragMax ( v2f_withMaxCoords i ) : COLOR
		{				
			half4 color = tex2D(_MainTex, i.uv.xy);
			color = max(color, tex2D (_MainTex, i.uv20));	
			color = max(color, tex2D (_MainTex, i.uv21));	
			color = max(color, tex2D (_MainTex, i.uv22));	
			color = max(color, tex2D (_MainTex, i.uv23));	
			return saturate(color - THRESHHOLD) * ONE_MINUS_THRESHHOLD_TIMES_INTENSITY;
		} 

		fixed4 fragBlur ( v2f_withBlurCoords i ) : COLOR
		{				
			half4 color = tex2D (_MainTex, i.uv20);
			color += tex2D (_MainTex, i.uv21);
			color += tex2D (_MainTex, i.uv22);
			color += tex2D (_MainTex, i.uv23);
			return color * 0.25;
		}



		// FASTER ///////////////////////////////////////////////////////////////////////
		uniform half _BlurWidthScale;

		static const half curve[7] = {0.016, 0.081, 0.232, 0.323, 0.232, 0.081, 0.016};  // gauss'ish blur weights
	
		static const half4 curve4[7] = {
			half4(0.016,0.016,0.016,0), 
			half4(0.081,0.081,0.081,0),
			half4(0.232,0.232,0.232,0),
			half4(0.323,0.323,0.323,1),
			half4(0.232,0.232,0.232,0),
			half4(0.081,0.081,0.081,0),
			half4(0.016,0.016,0.016,0) 
		};

		struct v2f_withBlurCoords8 {
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};	
		
		struct v2f_withBlurCoordsSGX {
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs[7] : TEXCOORD1;
		};

		v2f_withBlurCoords8 vertBlurHorizontal (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurWidthScale;

			return o; 
		}
		
		v2f_withBlurCoords8 vertBlurVertical (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurWidthScale;
			 
			return o; 
		}	

		v2f_withBlurCoordsSGX vertBlurHorizontalSGX (appdata_img v)
		{
			v2f_withBlurCoordsSGX o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurWidthScale; 
			half2 coords = o.uv - netFilterWidth * 3.0;
			
			for( int q = 0; q < 7; q++ )  
			{
				o.offs[q] = coords;
				coords += netFilterWidth;
			}

			return o; 
		}		

		v2f_withBlurCoordsSGX vertBlurVerticalSGX (appdata_img v)
		{
			v2f_withBlurCoordsSGX o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			half2 netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurWidthScale;
			half2 coords = o.uv - netFilterWidth * 3.0;
			
			for( int q = 0; q < 7; q++ )  
			{
				o.offs[q] = coords;
				coords += netFilterWidth;
			}

			return o; 
		}	

		half4 fragBlur8 ( v2f_withBlurCoords8 i ) : COLOR
		{
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
			
			half4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);
				color += tap * curve4[l];
				coords += netFilterWidth;
  			}
			return color;
		}

		half4 fragBlurSGX ( v2f_withBlurCoordsSGX i ) : COLOR
		{
			half2 uv = i.uv.xy;   
			
			half4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, i.offs[l]);  
				color += tap * curve4[l];
  			}

			return color;
		}	
			
	ENDCG
	
	SubShader {
	  ZTest Off Cull Off ZWrite Off Blend Off
	  Fog { Mode off }  
	  
	// 0
	Pass {
	
		CGPROGRAM
		#pragma vertex vertBloom
		#pragma fragment fragBloom
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
	// 1
	Pass { 
	
		CGPROGRAM
		
		#pragma vertex vertMax
		#pragma fragment fragMax
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}	
	// 2
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vertBlur
		#pragma fragment fragBlur
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}

	// 3
	Pass {
	
		CGPROGRAM

		#pragma vertex vertBloom
		#pragma fragment fragBloomAberration
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG

		}

	// FASTER

	// 4
	Pass {
		ZTest Always
		Cull Off
		
		CGPROGRAM 
		
		#pragma vertex vertBlurVertical
		#pragma fragment fragBlur8
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG 
		}	
		
	// 5		
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma vertex vertBlurHorizontal
		#pragma fragment fragBlur8
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		}	
	}

	FallBack Off
}
