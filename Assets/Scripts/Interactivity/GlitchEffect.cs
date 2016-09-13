using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Glitch")]
public class GlitchEffect : ImageEffectBase
{
	public float O1 = 0.5f;
	public float O2 = 0.02f;
	public float S1 = 5.0f;
	public float S2 = 50.0f;
	public float Rate = 1.0f;
	
	[RangeAttribute(0.0f,1.0f)]
	public float Factor = 1.0f;
	
	[RangeAttribute(0.0f,1.0f)]
	public float Span = 0.1f;

	//public Mesh glitchMesh;
	
	[HideInInspector]
	public float unitValue;
	
	private void quad( float x1, float x2, float y1, float y2, RenderTexture source, RenderTexture dest, Material material ) 
	{
		RenderTexture.active = dest;
		material.SetTexture( "_MainTex", source );
		GL.PushMatrix();
		GL.LoadOrtho();
		
		for( int i=0; i<material.passCount; i++ )
		{
			material.SetPass( i );
			GL.Begin( GL.QUADS );
				GL.TexCoord2( x1, y1 ); GL.Vertex3( x1, y1, 0.1f);
				GL.TexCoord2( x2, y1 ); GL.Vertex3( x2, y1, 0.1f);
				GL.TexCoord2( x2, y2 ); GL.Vertex3( x2, y2, 0.1f);
				GL.TexCoord2( x1, y2 ); GL.Vertex3( x1, y2, 0.1f);
			GL.End();	
		}	
		
		GL.PopMatrix();
	}
	
	void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		//Graphics.Blit(source, destination);	return;

		material.SetFloat( "_O1", O1 );
		material.SetFloat( "_O2", O2 );
		material.SetFloat( "_S1", S1 );
		material.SetFloat( "_S2", S2 );
		material.SetFloat( "_Factor", Factor );
		material.SetFloat( "_Value", unitValue );
		material.SetFloat( "_Span", Span );
		material.SetFloat( "_T", (Time.realtimeSinceStartup * Rate)%100.0f );
		/*if( glitchMesh != null )
		{
			RenderTexture.active = destination;
			material.SetTexture( "_MainTex", source );
			GL.PushMatrix();
			GL.LoadOrtho();
			for( int i=0; i<material.passCount; i++ )
			{
				if( material.SetPass(i) )
					Graphics.DrawMeshNow( glitchMesh, Matrix4x4.TRS( new Vector3( 0.0f, 0.0f, 0.0f ), Quaternion.identity, new Vector3( 1.0f, 1.0f, 1.0f ) ) );
			}
			GL.PopMatrix();
		}
		else*/
		{
			float cnt = 8.0f;
			for( float i=0; i<cnt; i++ )
				quad( unitValue-Span, unitValue+Span, i/cnt, (i+1)/cnt, source, destination, material );
		}
	}
}
