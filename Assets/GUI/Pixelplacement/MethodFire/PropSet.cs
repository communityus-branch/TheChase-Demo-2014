using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class PropSet : MonoBehaviour
{
	public GameObject target;

	[HideInInspector]
	public string propName;

	[HideInInspector]
	public int propID;

	public static BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;

	void OnEnable()
	{
		if( target == null || System.Array.IndexOf( TargetProperties(), propName ) == -1 )
			propName = "(No property selected)";
	}

	public void Set( object v )
	{
		if( target == null || propName == "(No property selected)" )
		{
			Debug.LogError( gameObject.name + ": No target property has been chosen." );
			return;
		}

		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach( PropertyInfo prop in monoBehavoiour.GetType().GetProperties( PropSet.flags ) )
			{
				if( prop == null || propName != prop.Name )
					continue;

				if( !prop.CanRead || !prop.CanWrite )
				{
					Debug.LogError( string.Format( "property '{0}' cant read/write", propName ) );
					continue;
				}

				if( v is Color )
				{
					Debug.Log( string.Format( gameObject.name + ": NOT setting prop '{0}.{1}' to {2}", target.name, propName, v.ToString() ) );
					//monoBehavoiour.GetType().GetField( propName ).SetValue( monoBehavoiour, v );
					//prop.SetValue( monoBehavoiour, v, null );
				}
				else
					prop.SetValue( monoBehavoiour, v, null );
			}
		}
	}

	public object Get()
	{
		if( target == null || propName == "(No property selected)" )
		{
			Debug.LogError( gameObject.name + ": No target property has been chosen." );
			return null;
		}

		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach( PropertyInfo prop in monoBehavoiour.GetType().GetProperties( PropSet.flags ) )
			{
				if( prop == null || propName != prop.Name )
					continue;

				if( !prop.CanRead || !prop.CanWrite )
				{
					Debug.LogError( string.Format( "property '{0}' cant read/write", propName ) );
					continue;
				}

				return prop.GetValue( monoBehavoiour, null ) as object;
			}
		}

		return null;
	}
	
	string[] TargetProperties()
	{
		//propMap.Clear();

		List<string> propNamesList = new List<string>();
		foreach( MonoBehaviour monoBehavoiour in target.GetComponents<MonoBehaviour>() )
		{
			foreach( PropertyInfo propInfo in monoBehavoiour.GetType().GetProperties( PropSet.flags ) )
			{
				propNamesList.Add( propInfo.Name );
				//propMap[ propInfo.Name ] = propInfo;
			}
		}

		return propNamesList.ToArray();
	}
}