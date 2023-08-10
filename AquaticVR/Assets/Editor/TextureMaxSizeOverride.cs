using UnityEngine;
using UnityEditor;
using System;
 
public class TextureMaxSizeOverride : AssetPostprocessor {
    void OnPreprocessTexture() {
        TextureImporter importer = assetImporter as TextureImporter;
        String name = importer.assetPath.ToLower();
		if (!assetPath.Contains("Terrain")){ // Just a simple restriction to only process models that contains the word "model" in it's path    
			return;
		} else {
			//Debug.Log("Importing Terrain Texture At 8192");
		}
		 
		if (assetPath.Contains("BGC")){ // Just a simple restriction to only process models that contains the word "model" in it's path    
			importer.maxTextureSize = 2048;
		}
		if (assetPath.Contains("BGB")){
			importer.maxTextureSize = 2048;
		}
		if (assetPath.Contains("BGA")){
			importer.maxTextureSize = 2048;
		}
		
		if (assetPath.Contains("ACSPlanum") || assetPath.Contains("SSPlanum")){
			importer.maxTextureSize = 2048;
		}
		if (assetPath.Contains("BG")){
			importer.maxTextureSize = 2048;
		}
		if (assetPath.Contains("FG")){
			importer.maxTextureSize = 2048;
		}
		if (assetPath.Contains("LandingSite")){
			importer.maxTextureSize = 8192;
		}
		//importer.maxTextureSize = 8192;
		importer.filterMode = FilterMode.Trilinear;
		importer.anisoLevel = 16;
		if (assetPath.Contains("normal")){ // Just a simple restriction to only process models that contains the word "model" in it's path    
			Debug.Log("Converting To Normal Texture");
			importer.textureType = TextureImporterType.NormalMap;
		}
    }
}