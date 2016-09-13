using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CinematicCamera : MonoBehaviour {
	
	public float focalLength = 32;
	public float aspectWidth = 16;
	public float aspectHeight = 9;
	public bool letterBox = false;
	public Transform lookAt;
	public Transform movesFrom;
	public Transform movesTo;
	public bool allowAnimationOfMovePointsAtPlayback = false;
	public float jitter = 1.0f;
	public float jitterFrequency = 1.0f;
	public float fogDistance = 2000.0f;
	
	private Vector3 _movesFromPos, _movesToPos;
	private Quaternion _initialLocalRot;	
	void Awake ()
	{
		_movesFromPos = (movesFrom)?WorldPositionToParentSpace(movesFrom.position): transform.localPosition;
		_movesToPos = (movesTo)?WorldPositionToParentSpace(movesTo.position): transform.localPosition;
		_initialLocalRot = transform.localRotation;
	}

	void LateUpdate ()
	{
		SaveTransforms(movesFrom, movesTo);

		if (lookAt)
			GetComponent<Camera>().transform.LookAt(lookAt, Vector3.up);
		
		const float filmWidthInMM = 36f;
		const float filmHeightInMM = filmWidthInMM / 1.5f;
		GetComponent<Camera>().fieldOfView = 2.0f * Mathf.Atan2(filmHeightInMM, 2f * focalLength) * Mathf.Rad2Deg;
		
		if (letterBox)
		{
			var targetAspect = aspectHeight / Mathf.Max(aspectWidth, 1e-3f);
			var totalPixelWidth = GetComponent<Camera>().pixelWidth / GetComponent<Camera>().rect.width;
			var totalPixelHeight = GetComponent<Camera>().pixelHeight / GetComponent<Camera>().rect.height;
			var pixelRect = GetComponent<Camera>().pixelRect;
			pixelRect.height = pixelRect.width * targetAspect;
			if (totalPixelHeight <= pixelRect.height)
			{
				pixelRect.height = totalPixelHeight;
				pixelRect.width = (pixelRect.height / targetAspect);
				pixelRect.x = Mathf.Round((totalPixelWidth - pixelRect.width) * 0.5f);
				pixelRect.width = Mathf.Round(pixelRect.width);
			}
			else
			{
				pixelRect.width = totalPixelWidth;
				pixelRect.x = 0;
			}
			pixelRect.y = Mathf.Round((totalPixelHeight - pixelRect.height) * 0.5f);
			pixelRect.height = Mathf.Round(pixelRect.height);
			GetComponent<Camera>().pixelRect = pixelRect;
		}
		else
			GetComponent<Camera>().rect = new Rect(0,0,1,1);

		if (!BulletTime.isEditing)
		{
			if (lookAt)
				GetComponent<Camera>().transform.localRotation *= _externalRotation;
			else
				GetComponent<Camera>().transform.localRotation = _initialLocalRot * _externalRotation;
		}
		
		if ((!BulletTime.isEditing || lookAt) && jitter > 0f && Time.timeScale > 0f)
		{
			var x = BulletTime.Noise1D(BulletTime.time * jitterFrequency * 0.33f) * 2f - 1f;
			var y = BulletTime.Noise1D((BulletTime.time + 1f)*jitterFrequency * 0.33f) * 2f - 1f;
			var n = new Vector2(x, y);
			n *= (GetComponent<Camera>().fieldOfView / 70f); // normalize jitter from 70 FOV angle
			n *= jitter * 0.2f;//0.25f;
			var q = Quaternion.Euler(n.x, n.y, 0.0f);
			GetComponent<Camera>().transform.localRotation *= q;
		}
	
		RestoreTransforms(movesFrom, movesTo);
	}
	
	private Quaternion _externalRotation = Quaternion.identity;
	public void OnExternalRotation (Quaternion externalRotation)
	{
		_externalRotation = externalRotation;
	}

	void OnCameraEnter ()
	{
		OnCameraStay(0f);
		
		if (Application.isPlaying)
			transform.localRotation = _initialLocalRot;
	
		Atmospherics.FogDistance = fogDistance;
	}

	void OnCameraStay (float t)
	{
		if (movesFrom || movesTo)
		{
			SaveTransforms(movesFrom, movesTo);

			var a = (movesFrom)? _movesFromPos: _movesToPos;
			var b = (movesTo)? _movesToPos: _movesFromPos;

			transform.localPosition = Vector3.Lerp(a, b, t);
			RestoreTransforms(movesFrom, movesTo);
		}

		Atmospherics.FogDistance = fogDistance;
	}
		
	Vector3 WorldPositionToParentSpace(Vector3 pos)
	{
		if (transform.parent)
			return transform.parent.InverseTransformPoint(pos);
		else
			return pos;
	}
	
	private Vector3 _pos0, _pos1;
	private Quaternion _rot0, _rot1;
	void SaveTransforms(Transform t0, Transform t1)
	{
		if (allowAnimationOfMovePointsAtPlayback || !Application.isPlaying)
		{
			if (t0)
				_movesFromPos = WorldPositionToParentSpace(t0.transform.position);
			if (t1)
				_movesToPos = WorldPositionToParentSpace(t1.transform.position);
		}	

		if (t0)
			_pos0 = t0.position;
		if (t0)
			_rot0 = t0.rotation;
		if (t1)
			_pos1 = t1.position;
		if (t1)
			_rot1 = t1.rotation;
	}
	void RestoreTransforms(Transform t0, Transform t1)
	{
		if (t0)
			t0.position = _pos0;
		if (t0)
			t0.rotation = _rot0;
		if (t1)
			t1.position = _pos1;
		if (t1)
			t1.rotation = _rot1;
	}

	void OnDrawGizmosSelected() 
	{
		if (movesFrom && movesTo)
		{
			Gizmos.color = new Color (0.5f,0.0f,0.5f,1.0f);
   			Gizmos.DrawLine (movesFrom.position, movesTo.position);
		}
	}
}
