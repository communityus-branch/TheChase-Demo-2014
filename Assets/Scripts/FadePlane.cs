using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FadePlane : MonoBehaviour
{
	[Range(0.0f, 1.0f)]
	public float Factor = 0.0f;

	public float AngularExtent = 45.0f;
	
	void Start()
	{
		genNewOrientation();
		BulletTime.addRestartListener( genNewOrientation );
	}

	private void genNewOrientation()
	{
		transform.localRotation = Quaternion.AngleAxis( Random.Range( -1.0f, 1.0f ) * AngularExtent, Vector3.forward );
	}

	void Update()
	{
		if( Factor < 0 || Factor >= 1.0f )
			GetComponent<Renderer>().enabled = false;
		else
		{
			float x = Mathf.Lerp( -1.0f, 1.0f, Factor );
			GetComponent<Renderer>().enabled = true;
			GetComponent<Renderer>().sharedMaterial.SetTextureOffset( "_MainTex", new Vector2( x, 0 ) );
		}
	}
}
