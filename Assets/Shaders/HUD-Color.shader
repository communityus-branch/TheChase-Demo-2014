Shader "TheChase/GUI/Color"
{
	Properties
	{
    	_MainTex( "Base", 2D ) = "white" {}
	}

	Category
	{
    	Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
	    ZWrite Off
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

				void main()
				{
					gl_FragColor = texture2D( _MainTex, uv );
				}
#endif
				ENDGLSL
	    	}
	    }
	}
}