using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanline : MonoBehaviour {

	public Vector3 minRotation;
	public Vector3 maxRotation;
	public float speed = 4.0f;
	public bool autoStart = true;
	public iTween.EaseType easeType = iTween.EaseType.linear;
	
	// Use this for initialization
	void Start () {
		transform.eulerAngles = minRotation;
		if (autoStart){
			ScanUpAndDown();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void ScanUpAndDown()
	{
		iTween.RotateTo(gameObject, iTween.Hash("x",maxRotation.x, "y",maxRotation.y, "z",maxRotation.z, 
		"islocal", true, "time", speed, "easeType", easeType, "looptype", "pingpong"));
	
		
	}
}
