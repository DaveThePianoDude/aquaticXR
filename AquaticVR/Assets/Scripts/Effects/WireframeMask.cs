using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireframeMask : MonoBehaviour {

	public bool autoOpen ;
	public float startFOV = 5.0f;
	public float openFOV = 180.0f;
	public float openTime = 5.0f;
	public iTween.EaseType openEaseType = iTween.EaseType.linear;
	
	private Projector projector;
	// Use this for initialization
	void Start () {
		projector = GetComponent<Projector>();
		if (autoOpen){
			OpenMask();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void OpenMask()
	{
        Debug.Log("oscar openmask");
		iTween.ValueTo(gameObject, iTween.Hash("from", startFOV, "to", openFOV, "time", openTime, "easeType", openEaseType, "onupdate", "SetFOV"));
	
	}
	
	public void SetFOV( float fov )
	{
		projector.fieldOfView = fov;
	}
}
