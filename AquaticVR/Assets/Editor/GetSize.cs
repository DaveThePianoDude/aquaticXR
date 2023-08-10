using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class GetSize : ScriptableObject {
	private static Quaternion rotation;
	private static Vector3 size;
	private static GameObject go;
	[MenuItem ("GameObject/Get Renderer Size")]
	static void MenuGetSize(){
		rotation = Selection.activeTransform.localRotation;
		Selection.activeTransform.localRotation = Quaternion.identity;
		if (Selection.activeTransform.gameObject.GetComponent<Renderer>() != null){
			size = Selection.activeTransform.gameObject.GetComponent<Renderer>().bounds.size;
			Selection.activeTransform.localRotation = rotation;
			EditorUtility.DisplayDialog("Object Scale", "X: "+size.x+", Y: "+size.y+", Z: "+size.z, "OK", "");
		} else {
			EditorUtility.DisplayDialog("Oops", "There is no renderer available on this object.", "OK", "");	
		}
	}
}