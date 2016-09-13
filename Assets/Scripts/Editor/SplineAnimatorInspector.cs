using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SplineAnimator))]
class SplineAnimatorInspector : Editor 
{
	public override void OnInspectorGUI () 
	{
		var t = target as SplineAnimator;
		EditorGUIUtility.LookLikeInspector ();
		DrawDefaultInspector();

		SplineCameraTrigger trigger;		
		var tweak = FindTweak(t, out trigger);
		if (tweak == null)
		{
			if (GUILayout.Button("Add Tweak to Active Trigger"))
			{
				tweak = AddTweak(t, out trigger);
			}
		}
		
		if (tweak != null)
		{
			if (GUILayout.Button("Remove Tweak"))
			{
				trigger.ResetTweaks ();
				RemoveTweak(t);
				trigger.ApplyTweaks ();
				EditorUtility.SetDirty(trigger);
			}
			else
			{
				tweak.offset = EditorGUILayout.FloatField("[Tweak] Off Set", tweak.offset);
				tweak.offsetSideways = EditorGUILayout.FloatField("[Tweak] Sideways", tweak.offsetSideways);
				tweak.offsetUp = EditorGUILayout.FloatField("[Tweak] Up", tweak.offsetUp);
				tweak.sway = EditorGUILayout.FloatField("[Tweak] Sway", tweak.sway);
				if (GUI.changed)
				{
					EditorUtility.SetDirty(trigger);
					Undo.RegisterUndo (trigger, "Spline Animator Tweak Changed");
				}
			}	
		}
	}
	
	static SplineCameraTrigger.SplineAnimatorTweak AddTweak(SplineAnimator target,
		out SplineCameraTrigger trigger)
	{
		trigger = null;
		
		var sequencers = Object.FindObjectsOfType(typeof(SplineCameraSequencer)) as SplineCameraSequencer[];
		if (sequencers.Length < 1)
			return null;
		
		var seq = sequencers[0];
		var t = FindTweak(seq, target, out trigger);
		if (t != null)
			return t;
		
		if (!seq.activeTrigger)
			return null;
		trigger = seq.activeTrigger;

		var arr = (trigger.tweaks != null) ? new ArrayList(trigger.tweaks): new ArrayList();
		t = new SplineCameraTrigger.SplineAnimatorTweak();
		t.target = target;
		arr.Add(t);
		trigger.tweaks = arr.ToArray(typeof(SplineCameraTrigger.SplineAnimatorTweak)) as SplineCameraTrigger.SplineAnimatorTweak[];
					
		return t;
	}

	static void RemoveTweak(SplineAnimator target)
	{
		var sequencers = Object.FindObjectsOfType(typeof(SplineCameraSequencer)) as SplineCameraSequencer[];
		if (sequencers.Length < 1)
			return;
		
		var seq = sequencers[0];
		if (!seq.activeTrigger)
			return;

		var trigger = seq.activeTrigger;

		var arr = (trigger.tweaks != null) ? new ArrayList(trigger.tweaks): new ArrayList();
		foreach (SplineCameraTrigger.SplineAnimatorTweak tweak in arr)
			if (tweak != null && tweak.target == target)
			{
				arr.Remove(tweak);
				break;
			}
		trigger.tweaks = arr.ToArray(typeof(SplineCameraTrigger.SplineAnimatorTweak)) as SplineCameraTrigger.SplineAnimatorTweak[];
	}

	static SplineCameraTrigger.SplineAnimatorTweak FindTweak(SplineAnimator target, out SplineCameraTrigger trigger)
	{
		trigger = null;
		var sequencers = Object.FindObjectsOfType(typeof(SplineCameraSequencer)) as SplineCameraSequencer[];
		foreach (var s in sequencers)
		{
			var tweak = FindTweak(s, target, out trigger);
			if (tweak != null)
				return tweak;
		}
		return null;
	}

	static SplineCameraTrigger.SplineAnimatorTweak FindTweak(SplineCameraSequencer seq, SplineAnimator target,
		out SplineCameraTrigger trigger)
	{	
		trigger = seq.activeTrigger;
		if (!trigger)
			return null;
		if (trigger.tweaks == null)
			return null;
		foreach (var tweak in trigger.tweaks)
			if (tweak != null && tweak.target == target)
				return tweak;
		trigger = null;
		return null;
	}

}