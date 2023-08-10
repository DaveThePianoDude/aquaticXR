using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour {

	public Vector3 spinDirection = new Vector3(0,0,1);
	public float speed = 1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(0, (Time.deltaTime * speed), 0);
	}
}
