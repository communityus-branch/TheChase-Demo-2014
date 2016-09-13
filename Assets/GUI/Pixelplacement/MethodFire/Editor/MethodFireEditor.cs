using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor( typeof( MethodFire ) )]
public class MethodFireEditor : Editor
{
	MethodFire _target;
	
	
	void OnEnable()
	{
		_target = (MethodFire)target;
		if ( _target.target == null || System.Array.IndexOf( TargetMethods(), _target.methodName ) == -1 )
		{
			_target.methodID = 0;
			_target.methodName = "";
		}
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if ( _target.target != null )
		{
			string[] methods = TargetMethods();
			if ( methods.Length == 0 || _target.methodID > methods.Length-1 ) {
				return;
			}
			
			_target.methodID = EditorGUILayout.Popup( "Function:", _target.methodID, methods );
			
			_target.methodName = methods[ _target.methodID ];
			
			if ( EditorApplication.isPlaying && _target.methodName != "(No Function Selected)" && GUILayout.Button( "Fire" ) )
			{
				_target.Fire();
			}
		}
	}
	
	string[] TargetMethods()
	{
		List<string> methodNamesList = new List<string>();
		methodNamesList.Add( "(No Function Selected)" );
		foreach ( MonoBehaviour monoBehavoiour in _target.target.GetComponents<MonoBehaviour>() )
		{
			foreach ( MethodInfo methodInfo in monoBehavoiour.GetType().GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )  )
			{
				if ( !methodInfo.Name.Contains("add_") && !methodInfo.Name.Contains("remove_") && !methodInfo.Name.Contains("get_") && !methodInfo.Name.Contains("set_") )
				{
					methodNamesList.Add( methodInfo.Name );
				}
			}
		}
		
		string[] methodNamesArray = methodNamesList.ToArray();
		System.Array.Sort( methodNamesArray );
		return methodNamesArray;
	}
}
