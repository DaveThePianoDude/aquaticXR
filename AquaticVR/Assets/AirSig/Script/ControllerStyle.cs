using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerStyle : MonoBehaviour {

	void OnEnable () {
		GameObject[] children = GetComponentsInChildren<GameObject> ();
		foreach(GameObject obj in children) {
			if (obj.name.EndsWith ("Vive")) {
				#if UNITY_ANDROID
				obj.SetActive(false);
				#endif
			}

			if (obj.name.EndsWith ("Daydream")) {
				#if UNITY_STANDALONE_WIN
				obj.SetActive(false);
				#endif
			}
		}
	}
}
