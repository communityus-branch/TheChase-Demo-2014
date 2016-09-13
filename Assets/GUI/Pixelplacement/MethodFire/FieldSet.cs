using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class FieldSet : MonoBehaviour
{
	public GameObject target;

	[HideInInspector]
	public string fieldName;

	[HideInInspector]
	public int fieldID;

	public static BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField;

	void OnEnable()
	{
		if( target == null || System.Array.IndexOf( TargetFields(), fieldName ) == -1 )
			fieldName = "(No field selected)";
	}

	public void Set( object v )
	{
		if( target == null || fieldName == "(No field selected)" )
		{
			Debug.LogError( gameObject.name + ": No target field has been chosen." );
			return;
		}

		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach( FieldInfo field in monoBehavoiour.GetType().GetFields( FieldSet.flags ) )
			{
				if( field == null || fieldName != field.Name )
					continue;

				//Debug.Log( string.Format( monoBehavoiour.gameObject.name + ": setting field '{0}.{1}' to {2}", target.name, fieldName, v.ToString() ) );
				field.SetValue( monoBehavoiour, v );
			}
		}
	}
	

	public object Get()
	{
		if( target == null || fieldName == "(No field selected)" )
		{
			Debug.LogError( gameObject.name + ": No target field has been chosen." );
			return null;
		}

		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach( FieldInfo field in monoBehavoiour.GetType().GetFields( FieldSet.flags ) )
			{
				if( field == null || fieldName != field.Name )
					continue;

				return field.GetValue( monoBehavoiour ) as object;
			}
		}

		return null;
	}
	

	string[] TargetFields()
	{
		List<string> fieldNamesList = new List<string>();
		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
			foreach( FieldInfo fieldInfo in monoBehavoiour.GetType().GetFields( FieldSet.flags ) )
				fieldNamesList.Add( fieldInfo.Name );

		return fieldNamesList.ToArray();
	}
}