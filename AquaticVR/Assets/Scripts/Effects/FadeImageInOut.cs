using UnityEngine;
using System.Collections;

public class FadeImageInOut : MonoBehaviour {

	public Color fadeToColor;
	public float inTime = 10.0f;
	public float delayTime = 5.0f;
	public float outTime = 10.0f;
	// Use this for initialization
	void Start () {

		FadeImageIn();
	}
	
	// Update is called once per frame
	void Update () {
		//renderer.material.color.a = 1 - 1 * TimePassed / Lifetime;
		
	}
	
	public void FadeImageIn()
	{
		iTween.ColorTo(gameObject, iTween.Hash("a", 1, "time", inTime, "easeType", "easeInOutExpo", "delay", 0.1, "oncomplete", "FadeImageOut"));
		
	}
	
	void FadeImageOut()
	{
		
		iTween.ColorTo(gameObject, iTween.Hash("a", 0, "time", outTime, "easeType", "easeInOutExpo", "delay", delayTime));
	
	}
}
