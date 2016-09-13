using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/**
* @class SplineMesh
*
* @brief This class provides functions for generating curved meshes around a Spline.
*
* This class allows you to dynamically generate curves meshes (e. g. streets, rivers, tubes, ropes, tunnels, etc).
*/ 

[ExecuteInEditMode]
public class SplineMesh : MonoBehaviour
{
	public Spline spline;///< Reference to the spline that defines the path.
	
	public Spline.UpdateMode uMode = Spline.UpdateMode.DontUpdate; ///< Specifies when the mesh will be updated.
	
	public float deltaSeconds = 0.1f; ///< Specifies after how much time the mesh will be updated (see UpdateMode).
	public int deltaFrames = 2; ///< Specifies after how many frames the mesh will be updated (see UpdateMode).
	
	public Mesh baseMesh; ///< Reference to the base mesh that will be created around the spline.
	public Material[] materials;
	public int segmentCount = 100; ///< Number of segments (base meshes) stringed together per generated mesh.
	public Vector2 xyOffset = Vector2.zero; ///< Mesh offset in along direction of the spline.
	public Vector2 xyScale = Vector2.one; ///< Mesh scale in the directions arount the spline.
	
	public Vector2 uvScale = Vector2.one; ///< Affects the calculation of texture coordinates along the streched mesh
	public float uvWrapAt = 4f;
	public bool swapUV = false; ///< Defines which UV component will be extruded.

	public bool splitMesh = true;
	public int splineSegmentCount { get { return (splitMesh)? spline.SegmentCount: 1; } }
	
	public int lightProbeSegmentCount = 10;
	public Vector2 lightProbeExtrude = new Vector2(0.1f, 0.1f);
	public float lightProbeHeight = 1.0f;

	private float passedTime = 0f;
	
	public GameObject[] bentGOs;
	public Mesh[] bentMeshes;
	public GameObject[] GameObjects { get { return bentGOs; } } ///< Returns a reference to the spline mesh.
	public Mesh[] BentMeshes { get { return bentMeshes; } } ///< Returns a reference to the spline mesh.
	
	public bool persistent = true;
	
	void Start( )
	{
		if( spline == null )
			return;
		
		spline.UpdateSplineNodes( );
		if (!persistent)
			UpdateMesh();
	}
	
	void OnEnable( )
	{
		if( spline == null )
			return;
		
		spline.UpdateSplineNodes( );
		if (!persistent)
			UpdateMesh();
	}
	
	void LateUpdate( )
	{
		switch( uMode )
		{
		case Spline.UpdateMode.EveryFrame:
			UpdateMesh( );
			break;
			
		case Spline.UpdateMode.EveryXFrames:
			if( deltaFrames <= 0 )
				deltaFrames = 1;
			
			if( Time.frameCount % deltaFrames == 0 )
				UpdateMesh( );
			
			break;
			
		case Spline.UpdateMode.EveryXSeconds:
			passedTime += Time.deltaTime;
			
			if( passedTime >= deltaSeconds )
			{
				UpdateMesh( );
				passedTime = 0f;
			}
			
			break;
		}
	}
	
	/** 
	* This function updates the spline mesh. It is called automatically once in a while, if updateMode isn't set to DontUpdate.
	*/
	public void UpdateMesh( )
	{
		Setup( );
		
		if( spline == null || segmentCount <= 0 )
			return;

		// lightprobes
		List<Vector3> lightProbePositions = new List<Vector3> ();

		for (int q = 0; q < splineSegmentCount; ++q)
			UpdateMesh(bentMeshes[q], q, ref lightProbePositions);

		LightProbeGroup lightProbes = gameObject.GetComponent<LightProbeGroup>();
		if (lightProbeSegmentCount > 0)
		{
			if (!lightProbes)
				lightProbes = gameObject.AddComponent<LightProbeGroup>();
			lightProbes.probePositions = lightProbePositions.ToArray ();
		}
		else if (lightProbes)
		{
			#if UNITY_EDITOR
			DestroyImmediate (lightProbes);
			#else
			Destroy (lightProbes);
			#endif
		}
	}

	private void UpdateMesh(Mesh dstMesh, int splineSegment, ref List<Vector3> lightProbePositions)
	{
		Vector2 probeExtents = new Vector2 (baseMesh.bounds.extents.x + lightProbeExtrude.x, lightProbeExtrude.y);
		Vector3[] segmentProbePositions = {
			new Vector3 (0,					probeExtents.y + lightProbeHeight * 0.5f, 0),
			new Vector3 (probeExtents.x,	probeExtents.y, 0),
			new Vector3 (probeExtents.x,	probeExtents.y + lightProbeHeight, 0),
			new Vector3 (-probeExtents.x,	probeExtents.y, 0),
			new Vector3 (-probeExtents.x,	probeExtents.y + lightProbeHeight, 0) };
		
		//Gather model data
		Vector3[] verticesBase = baseMesh.vertices;
		Vector3[] normalsBase = baseMesh.normals;
		Vector4[] tangentsBase = baseMesh.tangents;
		Vector2[] uvBase = baseMesh.uv;
		Vector2[] uvLightmap = baseMesh.uv2;
		
		ArrayList allTrianglesBase = new ArrayList();
		for (int q = 0; q < baseMesh.subMeshCount; ++q)
			allTrianglesBase.Add(baseMesh.GetTriangles(q));
		
		var localSegmentCount = segmentCount / splineSegmentCount;
		
		//Allocate some memory for new mesh data
		Vector3[] verticesNew = new Vector3[verticesBase.Length * localSegmentCount];
		Vector3[] normalsNew = new Vector3[normalsBase.Length * localSegmentCount];
		Vector4[] tangentsNew = new Vector4[tangentsBase.Length * localSegmentCount];
		Vector2[] uvNew = new Vector2[uvBase.Length * localSegmentCount];
		Vector2[] lightmapUvNew = new Vector2[uvBase.Length * localSegmentCount];

		ArrayList allTrianglesNew = new ArrayList();
		for (int q = 0; q < baseMesh.subMeshCount; ++q)
			allTrianglesNew.Add(new int[baseMesh.GetTriangles(q).Length * localSegmentCount]);

		//Group front/rear vertices together 
		List<int> verticesFront = new List<int>( );
		List<int> verticesBack = new List<int>( );
		
		Vector3 centerFront = Vector3.zero;
		Vector3 centerBack = Vector3.zero;
		Vector3 minCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

		for( int i = 0; i < verticesBase.Length; i++ )
		{
			if( verticesBase[i].z > 0f )
			{
				verticesFront.Add( i );
				centerFront += verticesBase[i];
			}
			else if( verticesBase[i].z < 0f )
			{
				verticesBack.Add( i );
				centerBack += verticesBase[i];
			}
			
			minCorner = Vector3.Min(minCorner, verticesBase[i]);

			verticesBase[i].x = verticesBase[i].x + xyOffset.x;
			verticesBase[i].y = verticesBase[i].y + xyOffset.y;
		}
		
		centerFront /= verticesFront.Count;
		centerBack /= verticesBack.Count;


//		float meshAspect = (xyScale.x * baseMesh.bounds.size.x) / baseMesh.bounds.size.z;
//		Vector2 lightmapScale = new Vector2(
//			(baseMesh.bounds.size.x * xyScale.x),
//			spline.Length/(float)splineSegmentCount);
		//;;Debug.Log ("lightmapScale " + lightmapScale.x + "," + lightmapScale.y + " " + splineSegmentCount + " " + localSegmentCount + " " + baseMesh.bounds.size);
		
			
		int vIndex = 0;
		
		int boxDimension = (int)Mathf.Round(Mathf.Sqrt(localSegmentCount) + 0.5f);
		//;;Debug.Log ("boxDimension " + boxDimension);

		for (int segment = 0; segment < localSegmentCount; segment++)
		{
			float localParam0 = (float)segment / localSegmentCount;
			float localParam1 = (float)(segment + 1) / localSegmentCount;
			
			if (localParam1 == 1f) localParam1 -= 0.00001f;
			
			float splineParam0 = localParam0;
			float splineParam1 = localParam1;
			
			if (splitMesh && splineSegment < spline.SegmentCount)
			{
				SplineSegment currentSegment = spline.SplineSegments[splineSegment];

				splineParam0 = currentSegment.ConvertSegmentToSplineParamter( localParam0 );
				splineParam1 = currentSegment.ConvertSegmentToSplineParamter( localParam1 );
			}
			
			int onX = segment / boxDimension;
			//int onY = segment % boxDimension;
			//;;Debug.Log(onX + " " + onY);
			
			var columnStartingSegment = onX * boxDimension;
			var lightmapOffset = new Vector2(
				onX,
				-Mathf.Max(0f, (float)columnStartingSegment / (float)localSegmentCount));
			
			CalculateBentMesh( ref vIndex, verticesFront, verticesBack, ref centerFront, ref centerBack,
				splineParam0, splineParam1, localParam0, localParam1, new Vector2(1.0f/((float)boxDimension), (float)(boxDimension-1)), lightmapOffset,
				verticesBase, normalsBase, tangentsBase, uvBase, uvLightmap,
				verticesNew, normalsNew, tangentsNew, uvNew, lightmapUvNew);

			for (int q = 0; q < allTrianglesNew.Count; ++q)
			{
				int[] trianglesNew = allTrianglesNew[q] as int[];
				int[] trianglesBase = allTrianglesBase[q] as int[];
				for (int i = 0; i < trianglesBase.Length; i++)
					trianglesNew[i+(segment*trianglesBase.Length)] = trianglesBase[i] + (verticesBase.Length * segment);
			}
		}

		var localLightProbeSegmentCount = lightProbeSegmentCount / splineSegmentCount;
		for (int segment = 0; segment < localLightProbeSegmentCount + 1; segment++)
		{
			float param0 = (float)segment / localLightProbeSegmentCount;
			float paramC = ((float)segment + 0.5f) / localLightProbeSegmentCount;
			if (splitMesh && splineSegment < spline.SegmentCount)
			{
				SplineSegment currentSegment = spline.SplineSegments[splineSegment];
				param0 = currentSegment.ConvertSegmentToSplineParamter(param0);
				paramC = currentSegment.ConvertSegmentToSplineParamter(paramC);
			}

			Vector3 pos0 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(param0));
			Quaternion rot0 = spline.GetOrientationOnSpline(param0) * Quaternion.Inverse(spline.transform.localRotation);
			Vector3 posC = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(paramC));
			Quaternion rotC = spline.GetOrientationOnSpline(paramC) * Quaternion.Inverse(spline.transform.localRotation);

			foreach (var probePos in segmentProbePositions)
				lightProbePositions.Add(pos0 + rot0 * probePos);
			if (segment != localLightProbeSegmentCount)
				lightProbePositions.Add(posC + rotC * segmentProbePositions[0]);
		}
		
		dstMesh.vertices = verticesNew;
		dstMesh.uv = uvNew;
		dstMesh.uv2 = lightmapUvNew;
		
		if( normalsBase.Length > 0 )
			dstMesh.normals = normalsNew;
		
		if( tangentsBase.Length > 0 )
			dstMesh.tangents = tangentsNew;
		
		dstMesh.subMeshCount = allTrianglesNew.Count;
		for (int q = 0; q < allTrianglesNew.Count; ++q)
		{
			int[] trianglesNew = allTrianglesNew[q] as int[];
			dstMesh.SetTriangles(trianglesNew, q);
		}
	}
	
	private void Setup( )
	{
		if( spline == null )
			return;

		bool needsRebuild = false;
		if (bentGOs != null && bentMeshes != null && bentGOs.Length == splineSegmentCount && bentMeshes.Length == splineSegmentCount)
		{
			foreach (var m in bentMeshes)
				if (m)
					m.Clear();

			foreach (var g in bentGOs)
				if (!g)	needsRebuild = true;
			foreach (var m in bentMeshes)
				if (!m) needsRebuild = true;

			if (!needsRebuild)
			{
				UpdateProperties();
				return;
			}
		}

		if (bentGOs != null && bentGOs.Length != splineSegmentCount)
		{
			foreach (var b in bentGOs)
				DestroyImmediate(b);
			bentGOs = new GameObject[splineSegmentCount];			
		}

		if (!persistent && bentMeshes != null)
			foreach (var m in bentMeshes)
				DestroyImmediate(m);
		
		bentMeshes = new Mesh[splineSegmentCount];

		for (int q = 0; q < splineSegmentCount; ++q)
		{
			if (bentGOs[q] == null)
				bentGOs[q] = new GameObject("BentMesh" + q);
			var meshFilter = bentGOs[q].GetComponent<MeshFilter>();
			if (!meshFilter)
				meshFilter = bentGOs[q].AddComponent<MeshFilter>();
			var meshRenderer = bentGOs[q].GetComponent<MeshRenderer>();
			if (!meshRenderer)
				meshRenderer = bentGOs[q].AddComponent<MeshRenderer>();
			bentGOs[q].transform.parent = transform;
			bentGOs[q].transform.localPosition = Vector3.zero;
			bentGOs[q].transform.localRotation = Quaternion.identity;
			bentGOs[q].transform.localScale = Vector3.one;

			bentMeshes[q] = new Mesh();
			bentMeshes[q].name = bentGOs[q].name;
			if (!persistent)
				bentMeshes[q].hideFlags = HideFlags.HideAndDontSave;
			meshFilter.sharedMesh = bentMeshes[q];
		}

		UpdateProperties();
	}

	private void UpdateProperties()
	{
#if UNITY_EDITOR
		var staticEditorFlags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(gameObject);
#endif
		foreach (var g in bentGOs)
			if (g)
			{
				if (materials.Length > 0)
					g.GetComponent<Renderer>().sharedMaterials = materials;
#if UNITY_EDITOR
				UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, staticEditorFlags);
#endif
			}
	}
	
	private void CalculateBentMesh( ref int vIndex, List<int> verticesFront, List<int> verticesBack, ref Vector3 centerFront, ref Vector3 centerBack,
		float param0, float param1, float localParam0, float localParam1,
		Vector2 lightmapScale, Vector2 lightmapOffset,
		Vector3[] verticesBase, Vector3[] normalsBase, Vector4[] tangentsBase, Vector2[] uvBase, Vector2[] uvLightmap,
		Vector3[] verticesNew, Vector3[] normalsNew, Vector4[] tangentsNew, Vector2[] uvNew, Vector2[] lightmapUvNew )
	{
		Vector3 pos0 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param0 ));
		Vector3 pos1 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param1 ));
		
		Quaternion rot0 = spline.GetOrientationOnSpline( param0 ) * Quaternion.Inverse( spline.transform.localRotation );
		Quaternion rot1 = spline.GetOrientationOnSpline( param1 ) * Quaternion.Inverse( spline.transform.localRotation );

		float uvRange = uvWrapAt;

		float uvParamScale = (swapUV)? uvScale.x: uvScale.y;
		float uvWrap = uvRange / uvParamScale;

		float uvParam0 = param0 % uvWrap;
		float uvParam1 = uvParam0 + (param1 - param0);

		var maxLightmapScale = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
		var minLightmapScale = new Vector2(Mathf.Infinity, Mathf.Infinity);
		for( int i = 0; i < verticesBase.Length; i++ )
		{
			Vector3 tmpVert;
			Vector3 tmpUV;
			Vector2 lightmapUV;
			
			Vector3 tmpNormal;
			Vector3 tmpTangent;
			
			tmpVert = verticesBase[i];
			tmpUV = uvBase[i];
			lightmapUV = uvBase[i];
			
			if( normalsBase.Length > 0 )
				tmpNormal = normalsBase[i];
			else
				tmpNormal = Vector3.zero;
			
			if( tangentsBase.Length > 0 )
				tmpTangent = tangentsBase[i];
			else
				tmpTangent = Vector3.zero;
			
			if( verticesBack.Contains( i ) )
			{
				//lightmapUV = new Vector2(tmpVert.x*xyScale.x, 0f);
				tmpVert -= centerBack;
				
				tmpVert.Scale( new Vector3( xyScale[0], xyScale[1], 1f ) );
				
				tmpVert = rot0 * tmpVert;
				tmpVert += pos0;
				
				tmpNormal = rot0 * tmpNormal;
				tmpTangent = rot0 * tmpTangent;
				
				if( !swapUV )
				{
					tmpUV.y = uvParam0;
					lightmapUV.x *= xyScale.x;
					lightmapUV.y = localParam0;
				}
				else
				{
					tmpUV.x = uvParam0;
					lightmapUV.y *= xyScale.x;
					lightmapUV.x = localParam0;
				}
			}
			else if( verticesFront.Contains( i ) )
			{
				//lightmapUV = new Vector2(tmpVert.x*xyScale.x, Vector3.Distance(pos1, pos));

				tmpVert -= centerFront;
				
				tmpVert.Scale( new Vector3( xyScale[0], xyScale[1], 1f ) );
				
				tmpVert = rot1 * tmpVert;
				tmpVert += pos1;
				
				tmpNormal = rot1 * tmpNormal;
				tmpTangent = rot1 * tmpTangent;
				
				if( !swapUV )
				{
					tmpUV.y = uvParam1;
					lightmapUV.x *= xyScale.x;
					lightmapUV.y = localParam1;//Vector3.Distance(pos1, pos0);
				}
				else
				{
					tmpUV.x = uvParam1;
					lightmapUV.y *= xyScale.x;
					lightmapUV.x = localParam1;//Vector3.Distance(pos1, pos0);
				}
			}
			
			verticesNew[vIndex] = tmpVert;
			uvNew[vIndex] = Vector2.Scale(tmpUV, uvScale);
			lightmapUvNew[vIndex] = Vector2.Scale(lightmapUV + lightmapOffset, lightmapScale);
			minLightmapScale.x = Mathf.Min(minLightmapScale.x, lightmapUvNew[vIndex].x);
			maxLightmapScale.x = Mathf.Max(maxLightmapScale.x, lightmapUvNew[vIndex].x);
			minLightmapScale.y = Mathf.Min(minLightmapScale.y, lightmapUvNew[vIndex].y);
			maxLightmapScale.y = Mathf.Max(maxLightmapScale.y, lightmapUvNew[vIndex].y);
			
			if( normalsBase.Length > 0 )
				normalsNew[vIndex] = tmpNormal.normalized;
			
			if( tangentsBase.Length > 0 )
				tangentsNew[vIndex] = tmpTangent.normalized;
			
			vIndex++;
		}
		
		//Debug.Log ("Lightmap UV Range " + minLightmapScale.x + "," + minLightmapScale.y + " " + maxLightmapScale.x + "," + maxLightmapScale.y + " off " + lightmapOffset);
	}
	
}
