using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class LowResDuplicate : ScriptableObject {

	private static GameObject go;
	[MenuItem ("GameObject/Low Resolution Texture Duplicate")]
	static void CreateLowResTextureDuplicate(){
		 SceneView.lastActiveSceneView.Focus ();
         EditorWindow.focusedWindow.SendEvent (EditorGUIUtility.CommandEvent ("Duplicate"));
	}
}