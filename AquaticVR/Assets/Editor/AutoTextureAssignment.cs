using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class AutoTextureAssignment : EditorWindow {
	
	
	
	[MenuItem("Custom/Assign Textures To New Material")]
	static void AssignTexturesToNewMaterial()
	{
		string albedoTextureID = "_MainTex";
		string specularTextureID = "_SpecGlossMap";
		string normalTextureID = "_BumpMap";
		string heightTextureID = "_ParallaxMap";
		string occlusionTextureID = "_OcclusionMap";
		string detailNormalMapTextureID = "_DetailNormalMap";
		string emissionTextureID = "_EmissionMap";
		
		string albedoTextureSuffix = "diffuse";
		string specularTextureSuffix = "specular";
		string normalTextureSuffix = "normal";
		string heightTextureSuffix = "height";
		string aoTextureSuffix = "ambientOcclusion";
		string emissionTextureSuffix = "emission";
		
		float detailTexScale = 20.0f;
		string detailNormalMap = "Assets/Textures/Noise/frost-normal.png";
		float detailNormalScale = 0.5f;
		float parallax = 0.0062f;
		float glossMapScale = 0.6f;
		float occlusionStrength = 0.00f;
		
		Debug.Log("Assign Textures To New Material");
		//AssignTexturesToNewMaterial();
		string selPath = AssetDatabase.GetAssetPath(Selection.objects[0].GetInstanceID());
		//string[] parts = selPath.Split('\\');
		string dir = Path.GetDirectoryName(selPath);
		Debug.Log("Dir : " + dir);
			
		string[] parts = selPath.Split('_');
		string matName = parts[parts.Length - 2];
		string[] matNameParts = matName.Split('/');
		matName = matNameParts[matNameParts.Length - 1];		
		Debug.Log("MatName : " + matName);
		
		Material material = new Material(Shader.Find("Standard (Specular setup)"));
        AssetDatabase.CreateAsset(material, (dir + "/" + matName + ".mat"));

		UnityEditor.AssetDatabase.Refresh();
		
		foreach (Object o in Selection.objects)
		{
			string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
			Debug.Log("Path : " + path);
			
			string[] pathParts = path.Split('_');
			string channelPart = pathParts[pathParts.Length - 1];
			string[] chparts = channelPart.Split('.');
			string channelName = chparts[0];
			
			
			// Get the texture from the texture path
			Texture2D texture = (Texture2D)(AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)));;
			/*
			if (texture == null)
			{
				continue;
			}
			*/
			Debug.Log("ChannelName : " + channelName);
			
			// Assign the texture to the appropriate texture channel
			if (channelName == albedoTextureSuffix || channelName == "watercolor1"){
				material.SetTexture(albedoTextureID, texture);
			}
			
			if (channelName == specularTextureSuffix || channelName == "Specular"){
				material.SetTexture(specularTextureID, texture);
				material.SetFloat("_Glossiness", glossMapScale);
				material.SetFloat("_GlossMapScale", glossMapScale);
			}
			
			if (channelName == normalTextureSuffix || channelName == "Normal"){
				material.SetTexture(normalTextureID, texture);
			}
			
			if (channelName == heightTextureSuffix || channelName == "Height"){
				material.SetTexture(heightTextureID, texture);
			}
			
			if (channelName == aoTextureSuffix || channelName == "Occlusion"){
				material.SetTexture(occlusionTextureID, texture);
			}
			
			if (channelName == emissionTextureSuffix || channelName == "Emission"){
				material.SetTexture(emissionTextureID, texture);
			}
			
			if (channelName == emissionTextureSuffix || channelName == "watercolor1"){
				material.SetTexture(emissionTextureID, texture);
			}
		}
		
		material.SetFloat("_Parallax", parallax);
		material.SetFloat("_OcclusionStrength", occlusionStrength);
				
		Texture2D dNorMapTexture = (Texture2D)(AssetDatabase.LoadAssetAtPath(detailNormalMap, typeof(Texture2D)));;
		material.SetTexture(detailNormalMapTextureID, dNorMapTexture);
		material.SetFloat("_DetailNormalMapScale", detailNormalScale);
	
		material.SetTextureScale("_DetailAlbedoMap", new Vector2(detailTexScale, detailTexScale));

	}
	
	static void CreateMaterial()
    {
        // Create a simple material asset

        Material material = new Material(Shader.Find("Standard (Specular setup)"));
        AssetDatabase.CreateAsset(material, "Assets/MyMaterial.mat");

        // Print the path of the created asset
        Debug.Log(AssetDatabase.GetAssetPath(material));
    }
}
