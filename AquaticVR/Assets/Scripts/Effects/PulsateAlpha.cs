using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsateAlpha : MonoBehaviour {

	public Color fadeToColor;
	public float maxAlpha = 1.0f;
	public float minAlpha = 0.0f;
	public float pulseTime = 2.0f;

	public iTween.EaseType easeType;
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
		iTween.ColorTo(gameObject, iTween.Hash("a", maxAlpha, "time", pulseTime, "easeType", easeType, "oncomplete", "FadeImageOut"));
		
	}
	
	void FadeImageOut()
	{
		
		iTween.ColorTo(gameObject, iTween.Hash("a", minAlpha, "time", pulseTime, "easeType", easeType, "oncomplete", "FadeImageIn"));
	
	}
}
