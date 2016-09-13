Shader "Hidden/TheChase/Mobile-Skin-Pass-SSS" {

	Properties {
		_MainTex ("-", 2D) = "white" {}
	} 
	 
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		uniform half _BlurWidthScale;
		uniform half4 _MainTex_TexelSize;
		uniform half4 _SkinEpiLayerColor;
		uniform half4 _SkinSubLayerColor;
		uniform half _SkinEpiToSubRel;

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
		
		struct v2f_simple { 
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half4 uvTex : TEXCOORD1;
		};

		struct v2f_withBlurCoords {
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};	
		
		struct v2f_withBlurCoordsSGX {
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs[7] : TEXCOORD1;
		};	
				
		v2f_simple vert (appdata_img v)
		{
			v2f_simple o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = half4(v.texcoord.xy,1,1);	
			o.uvTex = half4(v.texcoord.xy,1,1);	
			return o; 
		}			

		v2f_simple vertSs (appdata_img v)
		{
			v2f_simple o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = ComputeScreenPos(o.pos);
			o.uvTex = half4(v.texcoord.xy,1,1);	 
			return o; 				
		}

		v2f_withBlurCoords vertBlurHorizontal (appdata_img v)
		{
			v2f_withBlurCoords o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurWidthScale;

			return o; 
		}
		
		v2f_withBlurCoords vertBlurVertical (appdata_img v)
		{
			v2f_withBlurCoords o;
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

		half4 fragCopy (v2f_simple i) : COLOR
		{
			return tex2D(_MainTex, i.uv.xy);
		}

		half4 fragBlurWeighted ( v2f_withBlurCoords i ) : COLOR
		{				
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
  			half4 sum = 0;  
			half weightSum = 0.00001;
			
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);  
				half4 weight = curve4[l] * tap.a;
				sum += tap * weight;
				weightSum += weight;
				coords += netFilterWidth;  
  			}
			
			sum = sum / weightSum;
			return sum;
		}	

		half4 fragBlurWeightedSGX ( v2f_withBlurCoordsSGX i ) : COLOR
		{
			half2 uv = i.uv.xy;   
  			half4 sum = 0;  
			half weightSum = 0.00001;
			
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, i.offs[l]);  
				half4 weight = curve4[l] * tap.a;
				sum += tap * weight;
				weightSum += weight;
  			}
			
			sum = sum / weightSum;
			return sum;
		}	

		// 3 layer shaders are NOT UP TO DATE

		half4 fragBlurWeightedSubFinal ( v2f_withBlurCoords i ) : COLOR
		{				
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
  			half4 sum = 0;  
			half weightSum = 0.00001;
			
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);  
				half weight = 4.0 * curve[l] * saturate(tap.a);// - 0.175)
				sum += tap * weight; 
				weightSum += weight;
				coords += netFilterWidth;  
  			}
			
			sum = sum * _SkinSubLayerColor * 3 / weightSum;
			half4 center = tex2D(_MainTex, i.uv.xy);
			sum.a = center.a;
			return sum;
		}	

		half4 fragBlurWeightedEpiFinalBlend ( v2f_withBlurCoords i ) : COLOR
		{				
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
  			half4 sum = 0;  
			half weightSum = 0.00001;
			
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);  
				half weight = 4.0 * curve[l] * saturate(tap.a);// - 0.175)
				sum += tap * weight;
				weightSum += weight;
				coords += netFilterWidth;  
  			}
			
			sum = sum * _SkinEpiLayerColor * 3 / weightSum;
			half4 center = tex2D(_MainTex, i.uv.xy);
			sum.a = center.a;
			return sum;
		}
			 
	ENDCG
	
	SubShader 
	{
	  ZTest Always Cull Off ZWrite Off Blend Off
	  Fog { Mode off }  
		 
	// 0
	Pass {
		ZTest Always
		Cull Off
		
		CGPROGRAM 
		
		#pragma exclude_renderers flash		
		#pragma vertex vertBlurVertical
		#pragma fragment fragBlurWeighted
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG 
		}	
		
	// 1		
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma exclude_renderers flash		
		#pragma vertex vertBlurHorizontal
		#pragma fragment fragBlurWeighted
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		}	

	// 2
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma exclude_renderers flash		
		#pragma vertex vertBlurHorizontal 
		#pragma fragment fragBlurWeightedSubFinal
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		}

	// 3
	Pass {		
		ZTest Always
		Cull Off
		Blend One One
		BlendOp Add
				
		CGPROGRAM
		
		#pragma exclude_renderers flash
		#pragma vertex vertBlurHorizontal
		#pragma fragment fragBlurWeightedEpiFinalBlend
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		}	

	}
	FallBack Off
}
