using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour {

	public Transform controller;

	// Update is called once per frame
	void Update () {
		transform.rotation = controller.rotation;
	}
}
