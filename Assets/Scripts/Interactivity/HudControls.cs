using UnityEngine;
using System.Collections;

public class HudControls : MonoBehaviour
{
	public static float LabelSlider( Rect screenRect, float sliderValue, float sliderMaxValue, string labelText )
	{
		GUI.Label( screenRect, labelText );
		screenRect.x += screenRect.width;
		sliderValue = GUI.HorizontalSlider( screenRect, sliderValue, 0.0f, sliderMaxValue );
		return sliderValue;
	}
	
	public static float LabelSliderLayout( float sliderValue, float min, float max, string labelText )
	{
		GUILayout.BeginHorizontal();
			GUILayout.Label( labelText );
			sliderValue = GUILayout.HorizontalSlider( sliderValue, min, max );
			//sliderValue = GUILayout.HorizontalScrollbar( sliderValue, 1.0f, min, max );
		GUILayout.EndHorizontal();
		return sliderValue;
	}
}