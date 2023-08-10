using UnityEngine;
using UnityEditor;
using System;
// Automatically convert any texture file with "_Normal"
// in its file name into a normal map.

public class NormalMapImport : AssetPostprocessor {
	
	
    void OnPreprocessTexture () {
		TextureImporter importer = assetImporter as TextureImporter;
        if (assetPath.Contains("_normal") || assetPath.Contains("_Normal")) {
            //TextureImporter textureImporter  = (TextureImporter) assetImporter;
            //textureImporter.convertToNormalmap = true;
			importer.textureType = TextureImporterType.NormalMap;
        }
    }
	
	
}