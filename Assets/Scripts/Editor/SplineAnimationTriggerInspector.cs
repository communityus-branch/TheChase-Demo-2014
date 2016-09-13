using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SplineAnimationTrigger))]
class SplineAnimationTriggerInspector : Editor 
{
	string AnimationClipPopup(string label, string clipName, Animation anim)
	{
		if (!anim)
			return "";
		
		var names = new ArrayList();
		var index = 0;
		foreach (AnimationState state in anim)
		{
			names.Add(state.clip.name);
			if (state.clip.name == clipName)
				index = names.Count - 1;
		}
		
		var namesArray = names.ToArray(typeof(string)) as string[];
		index = EditorGUILayout.Popup(label, index, namesArray);
		if (index < 0 || index >= namesArray.Length)
			return "";
		return namesArray[index];
	}

	bool ArrayField (SerializedProperty p, bool folded)
	{
		EditorGUI.indentLevel--;
		EditorGUI.indentLevel+=2;
		bool childrenAreExpanded = true;
		while (p.NextVisible(childrenAreExpanded))
			childrenAreExpanded = EditorGUILayout.PropertyField(p);
		EditorGUI.indentLevel--;
		return folded;
	}
	
	public override void OnInspectorGUI () 
	{
		var t = target as SplineAnimationTrigger;
		EditorGUIUtility.LookLikeInspector ();
		DrawDefaultInspector();


		t.target = EditorGUILayout.ObjectField("Target", t.target, typeof(Animation), true) as Animation;
		if (t.target)
		{
			EditorGUI.indentLevel++;
			t.clipName = AnimationClipPopup("Clip", t.clipName, t.target);
			EditorGUI.indentLevel--;
		}
		
		t.target2 = EditorGUILayout.ObjectField("Target (secondary)", t.target2, typeof(Animation), true) as Animation;
		if (t.target2)
		{
			EditorGUI.indentLevel++;
			t.clipName2 = AnimationClipPopup("Clip", t.clipName2, t.target2);
			EditorGUI.indentLevel--;
		}
			
		t.fadeLength = EditorGUILayout.FloatField("Fade Length", t.fadeLength);

	}
}