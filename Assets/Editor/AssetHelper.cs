using UnityEngine;
using UnityEditor;

class AssetHelper
{
	public static string CleanFileName(string fileName)
	{
		foreach (char c in System.IO.Path.GetInvalidFileNameChars())
			fileName = fileName.Replace(c.ToString(), string.Empty);
		return fileName;
	}

	public static string GetPathForGeneratedAsset(UnityEngine.Object o, GameObject go = null)
	{
		return GetPathForGeneratedAsset(o, ".asset", go);
	}

	public static string GetPathForGeneratedAsset(UnityEngine.Object o, string assetExtension, GameObject go = null)
	{
		string directoryName = System.IO.Path.Combine(
			System.IO.Path.GetDirectoryName(EditorApplication.currentScene),
			System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene));

		if (!System.IO.Directory.Exists (directoryName))
			System.IO.Directory.CreateDirectory (directoryName);

		return System.IO.Path.Combine(
			directoryName,
			((go != null)? (go.name + "_"): "") + 
			CleanFileName(o.name) + assetExtension);
	}


}