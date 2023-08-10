using UnityEditor;
using UnityEngine;
 
class MoveToOrigin {
	/// <summary>
	/// Moves the selected game object(s) to (0, 0, 0).
	/// <summary>
	/// <remarks>Keyboard shortcut: cmd-0 (Mac), ctrl-0 (Windows).</remarks>
	[MenuItem ("GameObject/Move To Origin %9")]
	static void MenuMoveToOrigin () {
		// Move each selected transform to (0, 0, 0)
		foreach (Transform t in Selection.transforms) {
			Undo.RecordObject(t, "Move " + t.name);
			t.position = Vector3.zero;
			//t.rotation = Quaternion.identity;
			//t.parent = null;
			Debug.Log("Moving " + t.name + " to origin");
		}
    }
 
	/// <summary>
	/// Validates the "Move To Origin" menu item.
	/// </summary>
	/// <remarks>The menu item will be disabled if no transform is selected.</remarks>
	[MenuItem ("GameObject/Move To Origin %9", true)]
	static bool ValidateMoveToOrigin () {
		return Selection.activeTransform != null;
	}
}