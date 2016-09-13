using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SpikeSlides : MonoBehaviour
{
	public Camera _cam = null;

	public float _offset_x = 0.0f;
	public float _offset_y = 0.0f;

	[Range(0.0f, 1.0f)]
	public float  _distance = 0.0f;

	public GameObject[] slides;
	private TextMesh[] slidesTextMesh;

	private iTweenPath path = null;
	private Vector3[] anim = null;

	[Range( 0.0f, 1.0f )]
	public float visibility = 0;

	private string _text;
	public string text
	{
		get
		{ return _text; }
		set
		{
			_text = value;
			string[] lines = _text.Split(';');
			
			for (int q = 0; q < slidesTextMesh.Length; ++q)
				if (slidesTextMesh[q] != null && lines.Length > q)
					slidesTextMesh[q].text = lines[q];
		}
	}

	void Awake ()
	{
		slidesTextMesh = new TextMesh[slides.Length];
		for (int q = 0; q < slides.Length; ++q)
			slidesTextMesh[q] = slides[q].GetComponentInChildren<TextMesh>();
	}

	private float easeOutQuad( float start, float end, float value )
	{
		end -= start;
		return -end * value * ( value - 2 ) + start;
	}

	private float easeOutQuart( float start, float end, float value )
	{
		value--;
		end -= start;
		return -end * ( value * value * value * value - 1 ) + start;
	}

	private float easeOutCubic( float start, float end, float value )
	{
		value--;
		end -= start;
		return end * ( value * value * value + 1 ) + start;
	}

	void Update()
	{
		if( !_cam )
			return;

		//if( visibility >= 0 )
		{
			if( anim == null )
			{
				path = GetComponent<iTweenPath>();
				if( path == null )
					Debug.LogError( "No path" );

				anim = iTweenPath.rawPath( path, false );
			}

			if( anim != null && anim.Length > 0 )
			{
				float f = Mathf.Clamp01(visibility);
				foreach (var slide in slides)
				{
					slide.transform.localPosition = iTween.PointOnPath(anim, easeOutQuart(0.0f, 1.0f, f));
					f *= f;
				}
			}
		}

		var ray = _cam.ViewportPointToRay( new Vector3( _offset_x, _offset_y ) );
		var p = ray.GetPoint( Mathf.Lerp( _cam.nearClipPlane, _cam.farClipPlane, _distance ) );
		transform.position = p;
	}
}
