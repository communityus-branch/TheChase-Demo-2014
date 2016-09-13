using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor( typeof( PropSet ) )]
public class PropSetEditor : Editor
{
	PropSet _target;


	void OnEnable()
	{
		_target = (PropSet)target;
		if( _target.target == null || System.Array.IndexOf( TargetProperties(), _target.propName ) == -1 )
		{
			_target.propID = 0;
			_target.propName = "";
		}
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if( _target.target != null )
		{
			string[] props = TargetProperties();
			if( props.Length == 0 || _target.propID > props.Length - 1 )
				return;

			_target.propID = EditorGUILayout.Popup( "Property:", _target.propID, props );
			_target.propName = props[ _target.propID ];
		}
	}

	string[] TargetProperties()
	{
		List<string> propNamesList = new List<string>();
		propNamesList.Add( "(No property selected)" );

		foreach( MonoBehaviour monoBehavoiour in _target.target.GetComponents<MonoBehaviour>() )
		{
			/*FieldInfo[] fields = monoBehavoiour.GetType().GetFields( flags );
			foreach( FieldInfo fieldInfo in fields )
				Debug.LogError( "Obj: " + monoBehavoiour.name + ", Field: " + fieldInfo.Name );*/

			/*PropertyInfo[] properties = monoBehavoiour.GetType().GetProperties( flags );
			foreach( PropertyInfo propertyInfo in properties )
				Debug.LogError( "Obj: " + monoBehavoiour.name + ", Property: " + propertyInfo.Name );*/

			foreach( PropertyInfo propInfo in monoBehavoiour.GetType().GetProperties( PropSet.flags ) )
			{
				if( propInfo.CanWrite && propInfo.CanRead )
					propNamesList.Add( propInfo.Name );
			}
		}

		string[] propNamesArray = propNamesList.ToArray();
		System.Array.Sort( propNamesArray );
		return propNamesArray;
	}
}