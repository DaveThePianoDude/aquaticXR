using UnityEngine;
using UnityEditor;
using System;
 
public class FBXScaleOverride : AssetPostprocessor {
    void OnPreprocessModel() {
        ModelImporter importer = assetImporter as ModelImporter;
        String name = importer.assetPath.ToLower();
		if (!assetPath.Contains("PlutoVR")){ // Just a simple restriction to only process models that contains the word "model" in it's path    
			return;
		} else {
			Debug.Log("Importing Terrain At 100 Scale");
		}
		 
		/*
        if (name.Substring(name.Length - 4, 4)==".fbx") {
			if (assetPath.Contains("Rocks")){ 
				importer.globalScale = 10.0F;
			} else {
				importer.globalScale = 100.0F;
			}
            importer.generateAnimations = ModelImporterGenerateAnimations.None;
        }
		*/
    }
}