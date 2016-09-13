using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(CinematicCamera))]
class CinematicCameraInspector : Editor 
{
	public override void OnInspectorGUI () 
	{		
		var cam = target as CinematicCamera;
		
		EditorGUIUtility.LookLikeInspector ();
	
		{ // Focal length
			var names = new string [] { "18mm (ultra-wide)", "20mm", "24mm (wide)", "28mm", "35mm (standard)", "50mm", "60mm", "70mm", "85mm (telephoto)", "105mm", "135mm", "200mm", "300mm (super-telephot)", "400mm", "600mm" };
			var values = new int [] { 18, 20, 24, 28, 35, 50, 60, 70, 85, 105, 135, 200, 300, 400, 600 };
			cam.focalLength = (float)EditorGUILayout.IntPopup("Focal Length", (int)cam.focalLength, names, values);
			var index = 0;
			for (; index < values.Length; ++index)
				if ((int)cam.focalLength == values[index])
					break;
			if (index < values.Length)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				index = (int)GUILayout.HorizontalSlider((int)index, (int)0, (int)values.Length - 1);
				cam.focalLength = values[index];
				EditorGUILayout.EndHorizontal();
			}
		}
		
		{ // Letter box
			var names = new string [] { "None", "2.39:1 (Cinema)", "1.85:1", "16:9 (HD)", "16:10", "4:3 (TV)" };
			var values = new Vector2 [] { Vector2.zero,
				new Vector2(2.39f,1f), new Vector2(1.85f,1f),
				new Vector2(16f,9f), new Vector2(16f,10f),
				new Vector2(4f,3f) };
			var index = 0;
			if (cam.letterBox)
				index = vec2Index(new Vector2(cam.aspectWidth, cam.aspectHeight), values);
			index = EditorGUILayout.Popup("Letter box", index, names);
			cam.letterBox = (index > 0);
			if (cam.letterBox)
			{
				cam.aspectWidth = values[index].x;
				cam.aspectHeight = values[index].y;
			}
		}
		
		cam.lookAt = EditorGUILayout.ObjectField(new GUIContent("Look At", "Camera will orient towards this transform"), cam.lookAt, typeof(Transform), true) as Transform;
		cam.movesFrom = EditorGUILayout.ObjectField(new GUIContent("Moves From", "Starting point of the camera constrain line"), cam.movesFrom, typeof(Transform), true) as Transform;
		cam.movesTo = EditorGUILayout.ObjectField(new GUIContent("Moves To", "Ending point of the camera constrain line"), cam.movesTo, typeof(Transform), true) as Transform;
		var jitterValues = new float [] { 0, 1, 5, 10 };
		cam.jitter = StepSlider(new GUIContent("Jitter strength", "Shakey camera"), cam.jitter, jitterValues, 0);
		var frequencyValues = new float[] { 1.0f, 50.0f, 100f };
		cam.jitterFrequency = StepSlider(new GUIContent("          frequency", "Shakey camera"), cam.jitterFrequency, frequencyValues, 1);
		cam.fogDistance = EditorGUILayout.FloatField(new GUIContent("Fog Distance", "Fog will be remaped onto this range"), cam.fogDistance);
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}

	private float StepSlider( GUIContent c, float v, float[] values, int startsWith)
	{
		int idx = EditorGUILayout.IntSlider(c, float2Index(v, values) + startsWith, startsWith, values.Length - 1 + startsWith) - startsWith;
		return values[idx];
	}

	private int float2Index(float v, float[] values)
	{
		for (var q = 0; q < values.Length; ++q)
			if (values[q] >= v)
				return q;
		return 0;
	}

	private int vec2Index(Vector2 v, Vector2[] values)
	{
		for (var q = 0; q < values.Length; ++q)
			if (Mathf.Approximately(values[q].x, v.x) &&
				Mathf.Approximately(values[q].y, v.y))
				return q;
		return 0;
	}
	
}