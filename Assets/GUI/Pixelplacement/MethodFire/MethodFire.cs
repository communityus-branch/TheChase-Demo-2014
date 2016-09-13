using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[ExecuteInEditMode]
public class MethodFire : MonoBehaviour
{
	public GameObject target;
	[HideInInspector]
	public int methodID;
	[HideInInspector]
	public string methodName;
	
	void OnEnable()
	{
		if ( target == null || System.Array.IndexOf( TargetMethods(), methodName ) == -1 )
		{
			methodID = 0;
			methodName = "(No Function Selected)";
		}
	}
	
	public void Fire( object value = null )
	{	
		if ( target == null || methodName == "(No Function Selected)" )
		{
			Debug.Log( gameObject.name + ": No target method has been chosen." );
			return;
		}

		try
		{
			if (value != null)
				target.SendMessage( methodName, value );
			else
				target.SendMessage( methodName );
		}
		catch( System.Exception e )
		{
			Debug.LogError( e );
		}
	}
	
	string[] TargetMethods()
	{
		List<string> methodNamesList = new List<string>();
		foreach ( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach ( MethodInfo methodInfo in monoBehavoiour.GetType().GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )  )
			methodNamesList.Add( methodInfo.Name );
		}
		
		return methodNamesList.ToArray();
	}
}

//list methods
//save name
//use name to relink at fire time
//check it exists before fire