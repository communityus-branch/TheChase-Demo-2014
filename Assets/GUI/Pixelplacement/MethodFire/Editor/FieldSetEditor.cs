using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor( typeof( FieldSet ) )]
public class FieldSetEditor : Editor
{
	FieldSet _target;

	void OnEnable()
	{
		_target = (FieldSet)target;
		if( _target.target == null || System.Array.IndexOf( TargetFields(), _target.fieldName ) == -1 )
		{
			_target.fieldID = 0;
			_target.fieldName = "";
		}
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if( _target.target != null )
		{
			string[] fields = TargetFields();
			if( fields.Length == 0 || _target.fieldID > fields.Length - 1 )
				return;

			_target.fieldID = EditorGUILayout.Popup( "Field:", _target.fieldID, fields );
			_target.fieldName = fields[ _target.fieldID ];
		}
	}

	string[] TargetFields()
	{
		List<string> fieldNamesList = new List<string>();
		fieldNamesList.Add( "(No field selected)" );

		foreach( MonoBehaviour monoBehavoiour in _target.target.GetComponents<MonoBehaviour>() )
		{
			foreach( FieldInfo fieldInfo in monoBehavoiour.GetType().GetFields( FieldSet.flags ) )
			{
				fieldNamesList.Add( fieldInfo.Name );
			}
		}

		string[] fieldNamesArray = fieldNamesList.ToArray();
		System.Array.Sort( fieldNamesArray );
		return fieldNamesArray;
	}
}