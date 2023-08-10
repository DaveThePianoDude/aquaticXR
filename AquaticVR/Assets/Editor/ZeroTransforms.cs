using UnityEditor;
using UnityEngine;
 
class ZeroTransforms {
	/// <summary>
	/// Moves the selected game object(s) to (0, 0, 0).
	/// <summary>
	/// <remarks>Keyboard shortcut: cmd-0 (Mac), ctrl-0 (Windows).</remarks>
	[MenuItem ("GameObject/Zero Transform %8")]
	static void MenuZeroTransforms () {
		// Move each selected transform to (0, 0, 0)
		foreach (Transform t in Selection.transforms) {
			Undo.RecordObject(t, "Move " + t.name);
			//t.parent = null;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			
			Debug.Log("Zero Transforms : " + t.name);
		}
    }
 
	/// <summary>
	/// Validates the "Zero Transform" menu item.
	/// </summary>
	/// <remarks>The menu item will be disabled if no transform is selected.</remarks>
	[MenuItem ("GameObject/Zero Transform %8", true)]
	static bool ValidateZeroTransforms () {
		return Selection.activeTransform != null;
	}
}