using UnityEngine;
using System.Collections;

public class ScrubIndicator : MonoBehaviour
{
	public GameObject knob;
	public GameObject timecode;
	public GameObject background;
	
	public float duration = 0.5f;
	public float delay = 0.0f;
	public float introductoryDelay = 5.0f;
	public float knobReactiveScale = 0.5f;
	public Color color = new Color(1, 1, 1, 1);
	
	private bool visible = false;
	private Vector3 knobScale = new Vector3(.1f, .1f, .1f);
	private Interactivity ui;
	void Start()
	{
		if (!background || !knob || !timecode)
		{
			Debug.LogError("Knob, Timecode or Background are missing");
			return;
		}
		knobScale = knob.transform.localScale;
		
		ui = Interactivity.instance;
		if (!ui)
		{
			Debug.LogError("__Interactivity is missing from the scene");
			return;
		}
					
		if(!ui.allowScrubbing)
			return;		

		Fadein ();
		Fadeout (duration, introductoryDelay);

		ui.OnInputDown += (pos) =>
		{
			if (!ui.IsScreenPointInsideSrubber (pos))
				return;
			
			Fadein ();
			KnobScale (1.0f, 1.0f + knobReactiveScale, 0.3f);
			
			visible = true;
		};

		ui.OnInputUp += (pos) =>
		{
			if (!ui.IsScreenPointInsideSrubber (pos) && !visible)
				return;			
	
			Fadeout (duration, delay);
			KnobScale (1.0f + knobReactiveScale, 1.0f, 0.5f); 
			
			visible = false;
		};
	}
	
	void Fadein ()
	{
		iTween.Stop( gameObject );
		background.GetComponent<Renderer>().enabled = true;
		knob.GetComponent<Renderer>().enabled = true;
		color.a = 1.0f;
		background.GetComponent<Renderer>().material.SetColor( "_Color", color );
		knob.GetComponent<Renderer>().material.SetColor( "_Color", color );
		
		timecode.SetActive (true);
		timecode.GetComponent<GUIText>().material.color = color;
	} 
	
	void Fadeout (float duration, float delay)
	{
		iTween.Stop( gameObject );
		iTween.ValueTo( gameObject, iTween.Hash(
			"from", 0.0f,
			"to", 1.0f,
			"time", duration,
			"delay", delay,
			"easetype", iTween.EaseType.easeOutCubic,
			"ignoretimescale", true,
			"onComplete", (System.Action<object>)( ( x ) =>
			{
				background.GetComponent<Renderer>().enabled = false;
				knob.GetComponent<Renderer>().enabled = false;
				timecode.SetActive (false);
			} ),
			"onUpdate", (System.Action<object>)( ( x ) =>
			{
				color.a = Mathf.Lerp( 1.0f, 0.0f, (float)x );
				background.GetComponent<Renderer>().material.SetColor( "_Color", color );
				knob.GetComponent<Renderer>().material.SetColor( "_Color", color );
				timecode.GetComponent<GUIText>().material.color = color;
			} ) ) );
	}
	
	void KnobScale (float from, float to, float time)
	{
		iTween.Stop( knob.gameObject );
		iTween.ValueTo( knob.gameObject, iTween.Hash(
			"from", from,
			"to", to,
			"time", time,
			"easetype", iTween.EaseType.easeOutCubic,
			"ignoretimescale", true,
			"onUpdate", (System.Action<object>)( ( x ) =>
			{
				knob.transform.localScale = knobScale * (float)x;
			} ) ) );
	}
}
