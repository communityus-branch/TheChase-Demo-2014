using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SplineMesh))]
public class SplineMeshInspector : Editor
{
	private Spline.UpdateMode uMode;

	private int segmentCount;
	private int deltaFrames;
	private float deltaSeconds;
	private float uvWrapAt;
	
	private bool swapUV;
	private bool splitMesh;
	private bool persistent;

	private Vector2 xyOffset;
	private Vector2 xyScale;
	private Vector2 uvScale;
	
	private int lightProbeSegmentCount;
	private Vector2 lightProbeExtrude;
	private float lightProbeHeight;

	private Spline spline;
	
	private Mesh baseMesh;
	private Material[] materials;
	
	public override void OnInspectorGUI( )
	{
		SplineMesh mesh = (SplineMesh) target;
		
		EditorGUILayout.BeginVertical( );
		
			EditorGUILayout.Space( );
			spline = (Spline) EditorGUILayout.ObjectField( "   Spline", mesh.spline, typeof( Spline ), true ); 
			baseMesh = (Mesh) EditorGUILayout.ObjectField( "   Base Mesh", mesh.baseMesh, typeof( Mesh ), false );
			int materialCount = (int)EditorGUILayout.IntField("   Materials Count", mesh.materials.Length);
			if (materials == null || materialCount != materials.Length)
				materials = new Material[materialCount];
			for (int q = 0; q < materialCount; ++q)
			{
				materials[q] = (q < mesh.materials.Length)? mesh.materials[q]: null;
				materials[q] = (Material)EditorGUILayout.ObjectField("   Material " + q, materials[q], typeof(Material), false);
			}
			EditorGUILayout.Space();
		
			uMode = (Spline.UpdateMode) EditorGUILayout.EnumPopup( "   Update Mode", mesh.uMode );
			
			if( uMode == Spline.UpdateMode.EveryXFrames )
				deltaFrames = EditorGUILayout.IntField( "   Delta Frames", mesh.deltaFrames );
			else if( uMode == Spline.UpdateMode.EveryXSeconds )
				deltaSeconds = EditorGUILayout.FloatField( "   Delta Seconds", mesh.deltaSeconds );

			uvWrapAt = EditorGUILayout.FloatField("   UV Wrap", mesh.uvWrapAt );
			
			segmentCount = Mathf.Max( EditorGUILayout.IntField( "   Segment Count", mesh.segmentCount ), 1 );

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("   Offset");
				xyOffset.x = EditorGUILayout.FloatField(mesh.xyOffset.x, GUILayout.MinWidth(10));
				xyOffset.y = EditorGUILayout.FloatField(mesh.xyOffset.y, GUILayout.MinWidth(10));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal( );
				EditorGUILayout.PrefixLabel( "   Scale" );
				xyScale.x = EditorGUILayout.FloatField( mesh.xyScale.x, GUILayout.MinWidth( 10 ) );
				xyScale.y = EditorGUILayout.FloatField( mesh.xyScale.y, GUILayout.MinWidth( 10 ) );
			EditorGUILayout.EndHorizontal( );
		
			EditorGUILayout.BeginHorizontal( );
				EditorGUILayout.PrefixLabel( "   UV-Scale" );
				uvScale.x = EditorGUILayout.FloatField( mesh.uvScale.x, GUILayout.MinWidth( 10 ) );
				uvScale.y = EditorGUILayout.FloatField( mesh.uvScale.y, GUILayout.MinWidth( 10 ) );
			EditorGUILayout.EndHorizontal( );
		
			swapUV = EditorGUILayout.Toggle( "   Swap UV", mesh.swapUV );
			splitMesh = EditorGUILayout.Toggle("   Split Mesh", mesh.splitMesh);

	

			EditorGUILayout.Space();

			// lightprobes
			lightProbeSegmentCount = Mathf.Max( EditorGUILayout.IntField( "   Light Volume Count", mesh.lightProbeSegmentCount ), 0 );

			EditorGUILayout.BeginHorizontal( );
				EditorGUILayout.PrefixLabel( "   Light Volume Extrude" );
				lightProbeExtrude.x = EditorGUILayout.FloatField( mesh.lightProbeExtrude.x, GUILayout.MinWidth( 10 ) );
				lightProbeExtrude.y = EditorGUILayout.FloatField( mesh.lightProbeExtrude.y, GUILayout.MinWidth( 10 ) );
			EditorGUILayout.EndHorizontal( );
		
			lightProbeHeight = EditorGUILayout.FloatField( "   Light Volume Height",  mesh.lightProbeHeight );

			EditorGUILayout.Space();

			persistent = EditorGUILayout.Toggle("   Persistent Mesh", mesh.persistent);
			if (persistent)
				uMode = Spline.UpdateMode.DontUpdate;

			EditorGUILayout.Space();
			
			
		EditorGUILayout.EndVertical( );

		bool rebuild = GUILayout.Button ("Rebuild");

		if (GUI.changed)
		{
			Undo.RegisterUndo( target, "Change Spline Mesh Settings" );
			EditorUtility.SetDirty( target );
		}

		if (GUI.changed || rebuild)
		{			
			if( baseMesh == null )
				Debug.LogWarning( "There is no base mesh assigned to your spline mesh! Check the inspector to assign it!", mesh.gameObject );
			
			mesh.uMode = uMode;
			mesh.spline = spline;
			mesh.swapUV = swapUV;
			mesh.uvWrapAt = uvWrapAt;
			mesh.xyOffset = xyOffset;
			mesh.xyScale = xyScale;
			mesh.uvScale = uvScale;
			mesh.baseMesh = baseMesh;
			mesh.materials = materials;
			mesh.deltaFrames = deltaFrames;
			mesh.deltaSeconds = deltaSeconds;
			mesh.segmentCount = segmentCount;
			mesh.lightProbeSegmentCount = lightProbeSegmentCount;
			mesh.lightProbeExtrude = lightProbeExtrude;
			mesh.lightProbeHeight = lightProbeHeight;
			mesh.persistent = persistent;
			mesh.splitMesh = splitMesh;


			if (mesh.persistent)
				foreach (var m in mesh.BentMeshes)
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m));
		
			mesh.UpdateMesh();

			if (mesh.persistent)
			{
				foreach (var m in mesh.BentMeshes)
				{
					var assetPath = AssetHelper.GetPathForGeneratedAsset(m, mesh.gameObject);

					Debug.Log("PATH: " + assetPath);
					AssetDatabase.CreateAsset(m, assetPath);
					AssetDatabase.SaveAssets();
				}
			}
		}
	}
	
}
