using UnityEngine;
using System.Collections;

public class ScrubGlitcher : MonoBehaviour
{
	public float Duration = 10.0f;
	public bool Active { get { return m_Active; } protected set { m_Active = value; } }
	private bool m_Active = false;
	private float accTime = 0.0f;
	private GameObject currentCamGO = null;

	public Shader glitchShader = null;
	//public Mesh glitchMesh = null;

	public string Dbg_Camera = "";
	public string Dbg_Factor = "";
	
	public float Rate = 1.0f;
	private float UnitScrubValue = 0.0f;
	
	[RangeAttribute(0.0f,0.25f)]
	public float FirstChaos = 0.225f;

	[RangeAttribute(0.0f,1.0f)]
	public float SecondChaos = 0.499f;

	[RangeAttribute(0.0f,1.0f)]
	public float Span = 0.1f;

	private void glitchCamera( Camera cam, float factor )
	{
		if( !cam )
			return;

		GameObject go = cam.gameObject;
		
		//	Not the same camera, make sure any glitches are removed from the previous camera.
		if( currentCamGO && currentCamGO != go )
		{
			var tmp = currentCamGO.GetComponent<GlitchEffect>();
			if( tmp )
			{
				tmp.enabled = false;
				Destroy( tmp );
			}
		}

		GlitchEffect glitch = go.GetComponent<GlitchEffect>();
		if( glitch == null )
		{
			Debug.LogWarning( "New glitch!" );
			glitch = go.AddComponent<GlitchEffect>();
			glitch.shader = glitchShader;
		}

		glitch.O1 = Mathf.Lerp( 0.025f, 0.75f, FirstChaos );
		glitch.S1 = Mathf.Lerp( 0.5f, 10.0f, FirstChaos );
		glitch.O2 = Mathf.Lerp( 0.001f, 0.08f, SecondChaos );
		glitch.S2 = Mathf.Lerp( 5.0f, 100.0f, SecondChaos );
		
		glitch.Factor = factor;
		glitch.unitValue = UnitScrubValue;
		glitch.Span = Span;
		glitch.Rate = Rate;
		//glitch.glitchMesh = glitchMesh;
		glitch.enabled = true;
		currentCamGO = go;
		
		Dbg_Camera = go.name.ToString();
		Dbg_Factor = factor.ToString();
	}

	void Update()
	{
		if( !Active )
			return;

		accTime += Time.deltaTime;
		var unitTime = accTime / Duration;

		if( unitTime > 1.0f )
		{
			unitTime = 1.0f;
			Active = false;
			var tmp = currentCamGO.GetComponent<GlitchEffect>();
			if( tmp )
			{
				tmp.enabled = false;
				Destroy( tmp );
			}
		}
		else
			glitchCamera( Camera.main, Mathf.Lerp( 1.0f, 0.0f, unitTime ) );	//	Duh :)
	}

	public void OnScrub( float scrubValue )
	{
		if( Duration <= 0 )
			return;

		UnitScrubValue = scrubValue;
		accTime = 0.0f;
		Active = true;
	}
}
