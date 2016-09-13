Shader "TheChase/GUI/Value2"
{
	Properties
	{
    	_MainTex( "Base", 2D ) = "white" {}
		_Color( "Main Color", Color ) = (1,1,1,1)
		_Color2( "Color2", Color ) = (0,0,0,0)
    	_Value( "Value", float ) = 0
    	_Span( "Span", float ) = 0.1
	}

	Category
	{
    	Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
	    ZWrite Off
		Cull Off
	    Blend SrcAlpha OneMinusSrcAlpha

	    SubShader
	    {
	    	Pass
	    	{
				GLSLPROGRAM
				varying mediump vec2 uv;
#ifdef VERTEX
				uniform mediump vec4 _MainTex_ST;
				void main()
				{
					gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
					uv = gl_MultiTexCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				}
#endif
#ifdef FRAGMENT
				uniform lowp sampler2D _MainTex;
				uniform lowp float _Value;
				uniform lowp float _Span;
				uniform vec4 _Color;
				uniform vec4 _Color2;
				
				float cubicPulse( float c, float w, float x )
				{
					x = abs(x - c);
					if( x>w )
						return 0.0;
					x /= w;
					return 1.0 - x*x*(3.0-2.0*x);
				}

				void main()
				{
					lowp vec4 c = texture2D( _MainTex, uv );
					float f = uv.x > _Value ? 0.0:1.0;
					//float f = 1.0f - cubicPulse( _Value, _Span, uv.x );
					gl_FragColor = mix( _Color, _Color2, f );
				}
#endif
				ENDGLSL
	    	}
	    }
	}
}