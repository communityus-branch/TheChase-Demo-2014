Shader "TheChase/Character/Skin" {
Properties {
	_MainTex ("Albedo (RGB)", 2D) = "grey" {}
	//_Scatter ("Scatter", 2D) = "black" {}
	_BumpMap ("Normals", 2D) = "bump" {}
	_BentNormals (" Bent normals", Float) = 1.0
	
	_Cube ("Reflection ", Cube) = "" {}
	_CubeIntensity (" Reflection intensity", Float) = 2.0
	_LightProbeIntensity ("Lightprobe intensity", Float) = 0.875
	[HideInInspector] _SkinLookupTex ("Skin Lookup", 2D) = "white" {}	
	[HideInInspector] _SkinScreenspaceLookup ("SSS Lookup", 2D) = "white" {}
	
	_DUMMY ("EYES ----------------------------------------", Float) = 0.0
	//_Parallax ("Parallax", Float) = 0.3
	_LightDirDependency ("Highlight Light Dependency", Float) = 0.3
	
	_DUMMY2 ("FALLBACK ----------------------------------------", Float) = 0.0
	_SkinFbDiffuse ("Skin Fallback diffuse", Vector) = (0.5,0.5,0.5, 0.5)
}

SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 300

	// ------------------------------------------------------------------
	Pass
	{
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

		CGPROGRAM
		#pragma multi_compile SKIN_PASS_1 SKIN_PASS_1_EYES SKIN_PASS_2 SKIN_PASS_2_EYES SKIN_FALLBACK
		
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_fwdbase nolightmap nodirlightmap
		#pragma glsl
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"
		#define UNITY_PASS_FORWARDBASE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
	

#if (SKIN_PASS_1_EYES || SKIN_PASS_2_EYES)
	#define SHADE_AS_EYES 1
#else
	#undef SHADE_AS_EYES
#endif

//Common data:--------------------------
// surface surf PseudoBRDF noambient vertex:PerVertexSkinFb approxview nolightmap noforwardadd 

#include "Mobile-Types.cginc"

struct Input {
	coord2 uv_MainTex;
	coord2 uv_BumpMap;
	color3 refl;
	color3 sh;
	coord4 screenPos;
};

#include "Mobile-SkinBase.cginc" // depends on Input struct

struct MySurfaceOutput {
	color3 Albedo;
	normal3 Normal;
	color3 Emission;
	color3 Specular;
	color Gloss;
	color Alpha;
	color3 Refl;
};

color _BentNormals;
color _CubeIntensity;

/*color3*/ half3 _SkinReflectivityMod; // requires half precision!

color4 _SkinFbDiffuse;

color _LightDirDependency;

sampler2D _SkinScreenspaceLookup;
sampler2D _SkinLookupTex;
sampler2D _BRDFTex;
samplerCUBE _Cube;

sampler2D _MainTex;
sampler2D _BumpMap;


inline void PerVertexSkin (inout appdata_full v, out Input o)
{
#if (SKIN_PASS_1 || SKIN_PASS_1_EYES)
	PerVertexSkin1st (v, o);
#endif
#if (SKIN_PASS_2 || SKIN_PASS_2_EYES)
	PerVertexSkin2nd (v, o);
#endif
#if (SKIN_FALLBACK)
	PerVertexSkinFb (v, o);
#endif
}

inline color4 LightingPseudoBRDF_1 (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	normal3 bentNormal = lerp(s.Normal, normal3(0,0,1), _BentNormals);

	normal3 halfDir = normalize(lightDir + viewDir);

	color3 dots = saturate(color3(dot(bentNormal, lightDir), dot(bentNormal, viewDir), 0));

	color4 skinLookup = tex2D (_SkinLookupTex, dots.yy);
	
	color4 c;
	c.rgb = s.Albedo * dots.x * skinLookup.r * _LightColor0.rgb * atten * 2.0;
	c.a = 1; // important for SSS masking

	return c;
}

inline color4 LightingPseudoBRDF_2 (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
#ifdef SHADE_AS_EYES
	normal3 halfDir = normalize(lightDir * _LightDirDependency + viewDir);
#else
	normal3 halfDir = normalize(lightDir + viewDir);
#endif

	// dots: N.L, N2.L (not used), N.V, N.H
	color4 dots = color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir));
	dots = saturate(dots);

	color4 skinLookup = (tex2D (_SkinLookupTex, dots.zw));

	color4 c;

	// diffuse
	c.rgb = dots.x * s.Albedo * _LightColor0.rgb * skinLookup.r * 2.0f * atten;

	// reflection(s)
#ifdef SHADE_AS_EYES
	color3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy) * saturate(1 - _LightDirDependency + dots.x) * _LightColor0.rgb;
	c.rgb += (s.Refl.rgb * (1-skinLookup.r) + refl);
#else
	color3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy ) * _LightColor0.rgb;
	c.rgb += (s.Refl.rgb * (skinLookup.a) + refl) * s.Gloss;
#endif
	c.a = 0;

	return c;
}

inline color4 LightingPseudoBRDF_Fallback (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
	normal3 halfDir = normalize(lightDir + viewDir);

	normal3 bentNormal = lerp(s.Normal, normal3(0,0,1), -_BentNormals * 0.1);
	normal3 bentNormal2 = lerp(s.Normal, normal3(0,0,1), _BentNormals * 0.05);
	normal3 bentNormal3 = lerp(s.Normal, normal3(0,0,1), _BentNormals);


	// dots: N.L, N2.L (not used), N.V, N.H
	color4 dots = (color4(dot(s.Normal, lightDir), 0, dot(s.Normal, viewDir), dot(s.Normal, halfDir)));
	color2 dotsRB = saturate(color2((dot(bentNormal3, lightDir)+0.1)/1.1, dot(bentNormal2, lightDir)));
	color4 skinLookup = tex2D (_SkinLookupTex, dots.zw);

	color4 c;

	// diffuse
	c.rgb = (_SkinSubLayerColor.rgb*dotsRB.x + _SkinTopLayerColor.rgb*dotsRB.y) * skinLookup.rrr * s.Albedo * _LightColor0.rgb * 2.0f * atten;

	// reflection(s)
	color3 refl = dot(skinLookup.gb, _SkinReflectivityMod.xy) * dots.x * _LightColor0.rgb;
	c.rgb += ((s.Refl.rgb + s.Specular.rgb * skinLookup.a) * (1.0-skinLookup.r) + refl) * s.Gloss;

	c.a = 0;

	return c;
}

inline color4 LightingPseudoBRDF (MySurfaceOutput s, normal3 lightDir, normal3 viewDir, color atten)
{
#if (SKIN_PASS_1 || SKIN_PASS_1_EYES)
	return LightingPseudoBRDF_1 (s, lightDir, viewDir, atten);
#endif
#if (SKIN_PASS_2 || SKIN_PASS_2_EYES)
	return LightingPseudoBRDF_2 (s, lightDir, viewDir, atten);
#endif
#if (SKIN_FALLBACK)
	return LightingPseudoBRDF_Fallback (s, lightDir, viewDir, atten);
#endif
}


void surf_1 (Input IN, inout MySurfaceOutput o) 
{
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));

#ifdef SHADE_AS_EYES
	o.Albedo = 1;
   	o.Emission = IN.sh.rgb;
#else
	o.Albedo = 0.5;
   	o.Emission = IN.sh.rgb * 0.5;
#endif
	o.Gloss = 0;

   	o.Alpha = 1; // important for SSS masking
}

void surf_2 (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));

	color4 tex = tex2D(_MainTex, IN.uv_MainTex);
	color4 reflTex = texCUBE(_Cube, IN.refl);
#ifdef SHADE_AS_EYES
	color3 albedo = tex.rgb;
	color3 reflection = reflTex.rgb;
	color3 sssColor = _SkinSubLayerColor.rgb * albedo.rgb;
#else
	color3 albedo = gamma2linear(tex.rgb);
	color3 reflection = gamma2linear(reflTex.rgb);
	color3 sssColor = _SkinSubLayerColor.rgb * albedo.rgb * 2.0 + albedo * 0.15;
#endif
	
	o.Albedo = albedo * _SkinTopLayerColor.rgb;
	o.Gloss = tex.a;
   	o.Refl = reflection * _CubeIntensity;
   	o.Emission = IN.sh * albedo;

	coord4 screenPos = IN.screenPos;

#if UNITY_UV_STARTS_AT_TOP
	if (_ProjectionParams.x < 0)
		screenPos.y = 1.0*screenPos.w - screenPos.y;
#endif

   	o.Emission += tex2Dproj(_SkinScreenspaceLookup, UNITY_PROJ_COORD(screenPos)).rgb * sssColor;
}

void surf_Fallback (Input IN, inout MySurfaceOutput o) {
    o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));

	color4 tex = tex2D(_MainTex, IN.uv_MainTex);

	o.Albedo = gamma2linear(tex.rgb);
	o.Gloss = tex.a;
   	o.Refl = gamma2linear(texCUBE(_Cube, IN.refl).rgb) * _CubeIntensity;

   	o.Specular = IN.sh; // ambient specular
   	o.Emission = IN.sh * o.Albedo * gamma2linear(_SkinFbDiffuse.rgb);
}


void surf (Input IN, inout MySurfaceOutput o) {
#if (SKIN_PASS_1 || SKIN_PASS_1_EYES)
	surf_1 (IN, o);
#endif
#if (SKIN_PASS_2 || SKIN_PASS_2_EYES)
	surf_2 (IN, o);
#endif
#if (SKIN_FALLBACK)
	surf_Fallback (IN, o);
#endif
}

void mycolor (Input IN, MySurfaceOutput o, inout color4 c)
{
#if (SKIN_PASS_2 || SKIN_FALLBACK)
	c = linear2gamma4 (c);

#if (SKIN_FALLBACK)
	c.rgb += 0.025; // compensate loss gamma approximation
#endif	

#endif
}
	
	
		struct VertexOutput
		{
			float4  pos					: SV_POSITION;
			coord2  tex					: TEXCOORD0;
			color3  refl				: TEXCOORD1;
			color3  sh					: TEXCOORD2;
			coord4  screenPos			: TEXCOORD3;
		
			normal3 lightDir			: TEXCOORD4;
			color3  vlight				: TEXCOORD5;
			normal3 viewDir				: TEXCOORD6;
			LIGHTING_COORDS(7,8)
		};


		float4 _MainTex_ST;
		VertexOutput vert (appdata_full v)
		{
			VertexOutput o;
			Input customVertexData;
			PerVertexSkin (v, customVertexData);
			o.refl = customVertexData.refl;
			o.sh = customVertexData.sh;
			
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.tex.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.screenPos = ComputeScreenPos (o.pos);
		
			TANGENT_SPACE_ROTATION;
			o.lightDir = mul (rotation, ObjSpaceLightDir(v.vertex));
			
			float3 viewDirForLight = mul (rotation, ObjSpaceViewDir(v.vertex));
			o.viewDir = normalize(viewDirForLight);
			
		#ifdef VERTEXLIGHT_ON
			float3 worldN = mul((float3x3)_Object2World, SCALED_NORMAL);
			float3 worldPos = mul(_Object2World, v.vertex).xyz;
			o.vlight = Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, worldPos, worldN );
		#else
			o.vlight = color3(0.0, 0.0, 0.0);
		#endif // VERTEXLIGHT_ON
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}
		
		
		color4 frag (VertexOutput IN) : COLOR
		{
			Input surfIN;
			surfIN.uv_MainTex = IN.tex.xy;
			surfIN.uv_BumpMap = IN.tex.xy;
			surfIN.refl = IN.refl;
			surfIN.sh = IN.sh;
			surfIN.screenPos = IN.screenPos;
			
		#ifdef UNITY_COMPILER_HLSL
			MySurfaceOutput o = (MySurfaceOutput)0;
		#else
			MySurfaceOutput o;
		#endif
			
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Specular = 0.0;
			o.Alpha = 0.0;
			surf (surfIN, o);
			
			color atten = LIGHT_ATTENUATION(IN);
			color4 c = LightingPseudoBRDF (o, IN.lightDir, IN.viewDir, atten);
			c.rgb += o.Albedo * IN.vlight;
			c.rgb += o.Emission;
			
			/*
			#if (SKIN_PASS_2)
				c = color4(1,0,0,1);
			#endif
			#if (SKIN_PASS_2_EYES)
				c = color4(0,1,0,1);
			#endif
			#if (SKIN_FALLBACK)
				c = color4(0,0,1,1);
			#endif
			*/
			
			mycolor (surfIN, o, c);
						
			return c;
		}	
		ENDCG
	}
	}

Fallback "TheChase/Reflective"
}
