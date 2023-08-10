using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MGTransition : System.Object
{
	
	public enum TransitionType
	{
		NoTransition,
		UseOffScreenPos,	
		SkyTransition,
		GroundTransition
	}
		
	public bool transitionAutoStart = true;
	public float delayStartTime = 0;	
	public bool fadeAlpha = false;
	public TransitionType transitionType = TransitionType.UseOffScreenPos;
	public bool useLocalSpace = true;
	public bool useCameraSpace = false;
	public bool posAnimEnabled = true;
	public Vector3 offScreenPos = Vector3.zero; //This is normalized 1,1,1
	public iTween.EaseType positionEaseType = iTween.EaseType.linear;
	public bool rotAnimEnabled = true;
	public Vector3 offScreenRot = Vector3.zero;
	public iTween.EaseType rotationEaseType = iTween.EaseType.linear;
	public bool scaleAnimEnabled = true;
	public Vector3 offScreenScale = new Vector3(1.0f,1.0f,1.0f);
	public iTween.EaseType scaleEaseType = iTween.EaseType.linear;
	public float transitionLength = 1;
	public GameObject transitionSound;

	
	
}

[System.Serializable]
public class ShakeMotion : System.Object
{
	public enum ShakeType
	{
		NoShakeType,
		NoiseShakeType,
		CameraShakeType,
		JitterShakeType
		
	}
	
	public bool shake = false;
	public float shakeAmount = 1;
	public ShakeType shakeType = ShakeType.NoiseShakeType;
	public iTween.EaseType shakepositionEaseType = iTween.EaseType.linear;
	
}

[System.Serializable]
public class AnimationRoutine : System.Object
{
	
	public enum AnimationRoutineType
	{
		NoAnimationRoutine,
		SpinRoutine,
		BounceRoutine,
		JerkyMotionsRoutine
		
	}
	
	public AnimationRoutineType[] animationRoutineType;
	public AnimationClip[] sourceAnimation;
	public float routineLength = 5;
	public GameObject routineSound;
	
}

public class MotionGraphicsAnimator : MonoBehaviour {

	
	public Transform targetTransform;
	public GameObject animatedObject;
	public bool translateAnimationToCameraSpace = false;
	
	public MGTransition transitionIn;
	
	public ShakeMotion shakeMotion;
	
	public AnimationRoutine[] animationRoutine;
	
	public MGTransition transitionOut;
	
	//End Action
	public bool loopAnimation = false;
	public bool selfDestruct = true;
	public GameObject newAnimation;

	private GameObject mainCamera;
	private Vector3 onScreenPos;
	private Vector3 originalPos;
	private Vector3 startPos;
	private Vector3 endPos;
	
	private Vector3 onScreenRot;
	private Vector3 originalRot;
	private Vector3 startRot;
	private Vector3 endRot;
	
	private Vector3 onScreenScale;
	private Vector3 originalScale;
	private Vector3 startScale;
	private Vector3 endScale;
	
	private float delayAdd = 0;
	
	// Use this for initialization
	void Awake () {
		
		Initialize();
		
		StartCoroutine(TransitionInDelay());
		
	}
	
	public void Initialize()
	{
		// Descide what gameObject to animate
		if(!animatedObject){
			animatedObject = gameObject;
		} 
		
		// Get the original transforms
		if (transitionIn.useLocalSpace){
			originalPos = animatedObject.transform.localPosition;
			originalRot = animatedObject.transform.localEulerAngles;
			originalScale = animatedObject.transform.localScale;
		} else {
			originalPos = animatedObject.transform.position;
			originalRot = animatedObject.transform.eulerAngles;
			originalScale = animatedObject.transform.localScale;
		}
		
		if(transitionIn.fadeAlpha){
			iTween.FadeTo(animatedObject, iTween.Hash("alpha", 1, "time",0));
		}
		
		//Find the Main Camera
		mainCamera = Camera.main.gameObject;
			
		//Initialize the Transition In
		if(transitionIn.transitionType != MGTransition.TransitionType.NoTransition){
			InitializeTransitionIn();
		}
		//Initialize the Transition Out
		if(transitionOut.transitionType != MGTransition.TransitionType.NoTransition){
			InitializeTransitionOut();
		}
		
		SetOnScreenTransform();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void SetOnScreenTransform()
	{
		if(targetTransform){
			if (transitionIn.posAnimEnabled){
				onScreenPos = targetTransform.position;
			}
			if (transitionIn.rotAnimEnabled){
				onScreenRot = targetTransform.localEulerAngles;
			}
			if (transitionIn.scaleAnimEnabled){
				onScreenScale = targetTransform.localScale;
			}
		} else {
			
			if (transitionIn.posAnimEnabled){
				onScreenPos = originalPos;
			}
			if (transitionIn.rotAnimEnabled){
				onScreenRot = originalRot;			
			}
			if (transitionIn.scaleAnimEnabled){
				onScreenScale = originalScale;
			}
		}
	}

	
	public void InitializeTransitionIn()
	{
		startPos = SetOffCameraPosition(transitionIn.offScreenPos);	
		if (transitionIn.useLocalSpace){
			if (transitionIn.posAnimEnabled){
				animatedObject.transform.localPosition = transitionIn.offScreenPos;
			}
			if (transitionIn.rotAnimEnabled){
				animatedObject.transform.localEulerAngles = transitionIn.offScreenRot;
			}
			if (transitionIn.scaleAnimEnabled){
				animatedObject.transform.localScale = transitionIn.offScreenScale;
			}
		} else {
			if (transitionIn.posAnimEnabled){
				animatedObject.transform.position = startPos;
			}
			if (transitionIn.rotAnimEnabled){
				animatedObject.transform.eulerAngles = transitionIn.offScreenRot;
			}
			if (transitionIn.scaleAnimEnabled){
				animatedObject.transform.localScale = transitionIn.offScreenScale;
			}
		}
		//Debug.Log("AnimatedObject : " + animatedObject.name.ToString() + " StartPos : " + startPos);
		
	}
		
	public void InitializeTransitionOut()
	{
		endPos = SetOffCameraPosition(transitionOut.offScreenPos);		
	
	}
	
	
	public Vector3 SetOffCameraPosition( Vector3 offScreenPos )
	{
		if (transitionIn.useLocalSpace){
			return offScreenPos;
		}
		Vector3 offCamPos;
		Vector3 offCamDir = offScreenPos;;
		float distToCam = Vector3.Distance(Camera.main.transform.position, onScreenPos);

		/*
		//In Transitions
		if(transitionIn.transitionInType == TransitionInType.FlyInFromLeftSide){
			offCamDir = Vector3(-distToCam,0,distToCam);
		}
		if(transitionIn.transitionInType == TransitionInType.FlyInFromRightSide){
			offCamDir = Vector3(distToCam,0,distToCam);
		}
		if(transitionIn.transitionInType == TransitionInType.FlyInFromCamera){
			offCamDir = Vector3(0,0,-1);
		}
		if(transitionIn.transitionInType == TransitionInType.FlyInFromDistance){
			offCamDir = Vector3(0,0,100);
		}
		if(transitionIn.transitionInType == TransitionInType.DropInFromSky){
			offCamDir = Vector3(onScreenPos.x,distToCam,onScreenPos.z);
		}
		if(transitionIn.transitionInType == TransitionInType.RiseInFromBottom){
			offCamDir = Vector3(0,-distToCam,onScreenPos.z);
		}
		*/
		/*
		var m = mainCamera.camera.cameraToWorldMatrix;
		var p = m.MultiplyPoint (offCamDir);	
		offCamPos = p;
		*/
		
		offCamPos = Camera.main.transform.TransformPoint(offCamDir);
		return offCamPos;
	}
	
			
	IEnumerator TransitionInDelay(){
		
		yield return new WaitForSeconds (transitionIn.delayStartTime);

		//Start the Animations
		if(transitionIn.transitionAutoStart){
			StartTransitionIn();
		} else {
			StartAnimationRoutine();
		}
	}	
	

	public void StartTransitionIn()
	{
			
		if(transitionIn.transitionSound){
			Instantiate(transitionIn.transitionSound);
		}	
		
		transitionIn.delayStartTime += delayAdd;
		
		if(transitionIn.fadeAlpha){
			iTween.FadeFrom(animatedObject, iTween.Hash("alpha", 0.0f, "time", transitionIn.transitionLength, "easeType", transitionIn.positionEaseType));
		}
		
		if (transitionIn.posAnimEnabled){
			iTween.MoveTo(animatedObject, iTween.Hash("x", onScreenPos.x, "y", onScreenPos.y, "z", onScreenPos.z, "delay", transitionIn.delayStartTime,
			"time", transitionIn.transitionLength, "easeType", transitionIn.positionEaseType, "islocal", transitionIn.useLocalSpace,
			"oncompletetarget", gameObject , "oncomplete", "StartAnimationRoutine"));
		}
		
		if (transitionIn.rotAnimEnabled){
			iTween.RotateTo(animatedObject, iTween.Hash("x",onScreenRot.x, "y",onScreenRot.y, "z",onScreenRot.z, "delay", transitionIn.delayStartTime, 
			"islocal", transitionIn.useLocalSpace, "time", transitionIn.transitionLength, "easeType", transitionIn.rotationEaseType));
		}
		
		if (transitionIn.scaleAnimEnabled){
			iTween.ScaleTo(animatedObject, iTween.Hash("x", onScreenScale.x, "y", onScreenScale.y, "z", onScreenScale.z, "delay", transitionIn.delayStartTime,
			"time", transitionIn.transitionLength, "easeType", transitionIn.scaleEaseType));
		}
		
		//Debug.Log("AnimatedObject : " + animatedObject.name.ToString() + " OnScreenPos : " + onScreenPos);

		delayAdd = delayAdd + 0.01f;
		//Debug.Log("DelayAdd : " + delayAdd);

	}

	/*
	IEnumerator StartShake(){
		Vector3 shakeMag = Vector3(0.1f,0.1f,0.1f);
		shakeMag = shakeMag * shakeObject.shakeAmount;
		float shakeLength = 1.0f;
		
		if(shakeObject.shakeType == ShakeType.JitterShakeType){
			iTween.ShakePosition(animatedObject, iTween.Hash("x", shakeMag.x,"y", shakeMag.y,"z", shakeMag.z, 
			"time", shakeLength,"delay", 0, "easeType", shakeObject.shakepositionEaseType, "looptype", "loop"));
			
			iTween.ShakeRotation(animatedObject,iTween.Hash("x",shakeMag.x,"y",shakeMag.y,"z",shakeMag.z, 
			"time", shakeLength,"delay",0, "easeType", shakeObject.shakepositionEaseType, "looptype","loop"));
		}	
		if(shakeObject.shakeType == ShakeType.NoiseShakeType){
			Vector3 shakeMagInit = shakeMag;
			float shakeLengthInit =  shakeLength;		
			while(true){
				shakeMag = shakeMagInit;
				shakeLength = shakeLengthInit;
				shakeMag.x = shakeMag.x * Random.Range(-2, 2);
				shakeMag.y = shakeMag.y * Random.Range(-2, 2);
				shakeMag.z = shakeMag.z * Random.Range(-2, 2);
				shakeLength = Random.Range(0,shakeLength);
				iTween.PunchPosition(animatedObject, iTween.Hash("x",shakeMag.x,"y",shakeMag.y,"z",shakeMag.z,
				"time", shakeLength, "easeType", shakeObject.shakepositionEaseType, "delay",0));
				
				//if (!disableRotation){
					iTween.ShakeRotation(animatedObject, iTween.Hash("x",shakeMag.x,"y",shakeMag.y,"z",shakeMag.z,
					"time", shakeLength, "easeType", shakeObject.shakepositionEaseType ));
				//}
				yield return new WaitForSeconds(shakeLength);
				
			}
		}
		
		if(shakeObject.shakeType == ShakeType.CameraShakeType){
			shakeMagInit = shakeMag;
			shakeLengthInit =  shakeLength;		
			while(true){
				shakeMag = shakeMagInit;
				shakeLength = shakeLengthInit;
				shakeMag.x = shakeMag.x * Random.Range(.5, 2);
				shakeMag.y = shakeMag.y * Random.Range(.5, 2);
				shakeMag.z = shakeMag.z * Random.Range(.5, 2);
				shakeLength = Random.Range(0,shakeLength);
				iTween.PunchPosition(animatedObject, iTween.Hash("x",shakeMag.x,"y",shakeMag.y,"z",shakeMag.z, 
				"time", shakeLength,"delay",0));
				
				//if (!disableRotation){
					iTween.ShakeRotation(animatedObject,iTween.Hash("x",shakeMag.x,"y",shakeMag.y,"z",shakeMag.z, 
					"time", shakeLength));
				//}
				
				yield return new WaitForSeconds(shakeLength);
				
			}
		}
	
	}
	*/
	
	public void StartAnimationRoutine()
	{
		/*
				//Debug.Log("AnimationRoutine has Started");
		if(shakeObject.shake){
			StartShake();
		}
		//Run all our Animation Routines
		if(animationRoutine.length >= 1){
			foreach(AnimationRoutine routines in animationRoutine){
				if(routines.routineSound){
					Instantiate(routines.routineSound);
				}
				
				float routineTime = routines.routineLength / animationRoutine.length;		
				foreach(AnimationRoutineType animRoutine in routines.animationRoutineType){
					StartCoroutine(RunAnimationRoutine(animRoutine, routineTime));
				}
			
			}	
		} else {
			return;
		}
		
		if(transitionOut.transitionAutoStart){	
			StartCoroutine(StartTransitionOut());
		}
		*/
	}

	/*	
	IEnumerator RunAnimationRoutine( AnimationRoutine.AnimationRoutineType animRoutine , float routineTime )
	{
		
		float runTime = Time.time + routineTime;
		Vector3 objPos = animatedObject.transform.position;
		Vector3 objRot = animatedObject.transform.eulerAngles;
		Vector3 objSize = animatedObject.transform.localScale;
		
		while(true){
			if(Time.time >= runTime){
				break;
			}
			
			//Define Routines Here.
			if(animRoutine == AnimationRoutineType.JerkyMotionsRoutine){
				yield return new WaitForSeconds(Random.Range(0,4));
				
				animatedObject.transform.position = 
				objPos + Vector3(Random.Range(-0.1,0.1),Random.Range(-0.1,0.1),Random.Range(-1,1));
				animatedObject.transform.eulerAngles = 
				objRot + Vector3(Random.Range(-1,1),Random.Range(-0.5,0.5),Random.Range(-40,40));
				
			}
			
			yield;
		}
		yield;
		
	}
	*/
	
	IEnumerator StartTransitionOut(){
		if(transitionOut.delayStartTime > 0){
			yield return new WaitForSeconds(transitionOut.delayStartTime);
		}
		if(transitionOut.transitionSound){
			Instantiate(transitionOut.transitionSound);
		}
		//Debug.Log("EndPos : " + endPos);
		if (transitionOut.posAnimEnabled){
			iTween.MoveTo(animatedObject,  iTween.Hash("x",endPos.x, "y",endPos.y, "z",endPos.z, "islocal", transitionOut.useLocalSpace,
			"time",transitionOut.transitionLength, "easeType", transitionOut.positionEaseType, "oncompletetarget", gameObject , "oncomplete", "EndAnimation"));
		}
		
		if (transitionOut.rotAnimEnabled){
			iTween.RotateTo(animatedObject,  iTween.Hash("x",endRot.x, "y",endRot.y, "z",endRot.z, "islocal", transitionOut.useLocalSpace, 
			"time",transitionOut.transitionLength, "easeType", transitionOut.rotationEaseType));
		}
		
		if (transitionOut.scaleAnimEnabled){
			iTween.ScaleTo(animatedObject,  iTween.Hash("x",endScale.x, "y",endScale.y, "z",endScale.z, 
			"time",transitionOut.transitionLength, "easeType", transitionOut.scaleEaseType, "oncompletetarget", gameObject , "oncomplete", "EndAnimation"));
		}
		
		if(transitionOut.fadeAlpha){
			iTween.FadeTo(animatedObject,  iTween.Hash("alpha",0.0f, "time",transitionOut.transitionLength, "easeType", transitionOut.positionEaseType));
		}
	}

	public void EndAnimation(){
		//Debug.Log("Animation Is Over!");
		if(newAnimation){
			Instantiate(newAnimation);
		}
		
		if(loopAnimation){
			Initialize();
			return;
		}
		
		if(selfDestruct){
			Destroy(animatedObject);
			Destroy(gameObject);		
		}
	}

}



