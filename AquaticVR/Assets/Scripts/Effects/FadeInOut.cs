using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System;
using UnityEngine.UI;


public class FadeInOut : MonoBehaviour {

	public bool autoFade;
    public bool autoDeactivate;
	public Color fadeToColor;
	public float inDelay = 0.0f;
	public float inTime = 2.0f;
	public bool autoOut;
	public float delayTime = 5.0f;
	public float outTime = 2.0f;
	
	public float minAlpha = 0.0f;
	public float maxAlpha = 1.0f;
	
	public bool loop = false;
	public float loopWaitTime = 10.0f;
	
	private Text txt;
	private Image img; 
	private Projector proj;
	private MeshRenderer mr;
	public bool useTintColor;
	public bool useMainColor;
	
	
	// Use this for initialization
	void Awake () {
		txt = GetComponent<Text>();
		img = GetComponent<Image>();
		mr = GetComponent<MeshRenderer>();
		proj = GetComponent<Projector>();
		
		SetAlpha(minAlpha);
		
		if (autoFade){

			FadeIn();

		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void FadeImageIn()
	{
		iTween.ColorTo(gameObject, iTween.Hash("a", 1, "time", inTime, "easeType", "easeInOutExpo", "delay", inDelay, "oncomplete", "FadeImageOut"));
		
	}
	
	void FadeImageOut()
	{		
		iTween.ColorTo(gameObject, iTween.Hash("a", 0, "time", outTime, "easeType", "easeInOutExpo", "delay", delayTime));
	}
	
	public void FadeIn()
	{
		if (autoOut){
			iTween.ValueTo(gameObject, iTween.Hash("from", minAlpha, "to", maxAlpha, "time", inTime, "delay", inDelay, "onupdate", "SetAlpha", "oncomplete", "FadeOut"));
		} else {
			iTween.ValueTo(gameObject, iTween.Hash("from", minAlpha, "to", maxAlpha, "time", inTime, "delay", inDelay, "onupdate", "SetAlpha"));
		}		
	}
	
	public void FadeOut()
	{
        if (loop)
        {
            iTween.ValueTo(gameObject, iTween.Hash("from", maxAlpha, "to", minAlpha, "time", outTime, "delay", delayTime, "onupdate", "SetAlpha", "oncomplete", "Loop"));
        } else
        {
            iTween.ValueTo(gameObject, iTween.Hash("from", maxAlpha, "to", minAlpha, "time", outTime, "delay", delayTime, "onupdate", "SetAlpha"));
        }
		
        if (autoDeactivate)
        {
            StartCoroutine(AutoDeactivate(outTime));
        }
	}
	
	public void SetAlpha( float a )
	{
		if (txt){
			Color tempColor = txt.color;
			tempColor.a = a;
			txt.color = tempColor;
		} else if (img){
			Color tempColor = img.color;
			tempColor.a = a;
			img.color = tempColor;
		} else if (mr){
			
			Color tempColor = new Color();
			/*
			try {
				tempColor = mr.material.color;
				tempColor.a = a;
				mr.material.color = tempColor;
			} catch (Exception e) {
				Debug.LogError(e);
				return;
			}
			
			try {
				tempColor = mr.material.GetColor("_TintColor");
				tempColor.a = a;
				mr.material.SetColor("_TintColor", tempColor);
			} catch (Exception e) {
				Debug.LogError(e);
				return;
			}
			*/
			
			if(useTintColor){
				tempColor = mr.material.GetColor("_TintColor");
				tempColor.a = a;
				mr.material.SetColor("_TintColor", tempColor);
			} else {
				tempColor = mr.material.color;
				tempColor.a = a;
				mr.material.color = tempColor;
			}
			
			//mr.material.color = tempColor;
		} else if (proj){
			Color tempColor = new Color();
			//Debug.Log("Setting Projector Color");
	
				/*
				tempColor = proj.material.GetColor("_MainColor");
				tempColor.a = a;
				tempColor.r = a;
				tempColor.g = a;
				tempColor.b = a;
				proj.material.SetColor("_MainColor", tempColor);
*/

				tempColor = proj.material.color;
				tempColor.a = a;
				tempColor.a = a;
				tempColor.r = a;
				tempColor.g = a;
				tempColor.b = a;
				proj.material.color = tempColor;

		}
		
	}
	
	public void Loop(){
		if (loop){
			StartCoroutine(LoopFadeInOut());
		}
	}
	
	public void TriggerLoop(){

		//StartCoroutine(LoopFadeInOut());
		FadeIn();
	}
	
	IEnumerator LoopFadeInOut() {

		yield return new WaitForSeconds(loopWaitTime);
		
		FadeIn();

		
	}

    IEnumerator AutoDeactivate(float deativateTime)
    {

        yield return new WaitForSeconds(deativateTime);

        gameObject.SetActive(false);


    }

}
