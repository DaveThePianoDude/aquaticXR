using UnityEditor;
using UnityEngine;
 
class ResetTransform {
	/// <summary>
	/// Moves the selected game object(s) to (0, 0, 0).
	/// <summary>
	/// <remarks>Keyboard shortcut: cmd-0 (Mac), ctrl-0 (Windows).</remarks>
	[MenuItem ("GameObject/Reset Transform %0")]
	static void MenuResetTransform () {
		// Move each selected transform to (0, 0, 0)
		foreach (Transform t in Selection.transforms) {
			Undo.RecordObject(t, "Move " + t.name);
			t.position = Vector3.zero;
			t.rotation = Quaternion.identity;
			//t.localScale = Vector3.one;
			t.parent = null;
			Debug.Log("Reseting Transform : " + t.name);
		}
    }
 
	/// <summary>
	/// Validates the "Reset Transform" menu item.
	/// </summary>
	/// <remarks>The menu item will be disabled if no transform is selected.</remarks>
	[MenuItem ("GameObject/Reset Transform %0", true)]
	static bool ValidateResetTransform () {
		return Selection.activeTransform != null;
	}
}