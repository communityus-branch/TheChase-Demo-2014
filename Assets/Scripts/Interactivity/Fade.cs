using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour
{
	public enum Mode
	{
		In,
		Out,
	}
	
	public Mode mode = Mode.In;
	public AnimationCurve curve = AnimationCurve.EaseInOut( 0.0f, 0.0f, 1.0f, 1.0f );
	private Texture2D texture = null;
	public float duration = 3.0f;
	public Color color = Color.black;
	
	private float startTime;
	
	void Start()
	{
		startTime = Time.time;
		Destroy( gameObject, duration );
		texture = new Texture2D( 1, 1 );
		texture.SetPixel( 0, 0, color );
		texture.Apply();
	}
	
	void OnGUI()
	{
		float t = (Time.time - startTime) / duration;
		Debug.Log( t );

		if( mode == Mode.In )
			color.a = curve.Evaluate( 1.0f - t );
		else
			color.a = curve.Evaluate( t );

		GUI.depth = -1000;
		GUI.color = color;
		GUI.DrawTexture( new Rect( 0,0,Screen.width, Screen.height ), texture );
	}
}
