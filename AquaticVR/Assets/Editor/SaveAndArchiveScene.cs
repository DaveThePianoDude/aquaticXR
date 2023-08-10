using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SaveAndArchiveScene : EditorWindow {

	[MenuItem("Custom/Save And Archive Current Scene")]
	static void SaveAndArchiveCurrentScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		Debug.Log("Active scene is : '" + scene.name + "'.");
		Debug.Log("Scene Path is : '" + scene.path + "'.");
		string[] pathParths = scene.path.Split('/');
		
	}
}
