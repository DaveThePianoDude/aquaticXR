using UnityEngine;
using UnityEditor;
using System;
//C#
class FBXGenerateCollidersOverride : AssetPostprocessor
{
	void OnPostprocessModel (GameObject g)
	{
		if (!assetPath.Contains("Terrain")){ // Just a simple restriction to only process models that contains the word "model" in it's path    
			return;
		} else {
		Debug.Log("Importing Terrain With Mesh Collider");
	}
		  // without a check it will do the following with every imported asset!
		Renderer[] allRenderers = g.GetComponentsInChildren<Renderer>();
		foreach(Renderer R in allRenderers)
		{
			//R.gameObject.AddComponent<MeshCollider>();
		}
	}
}