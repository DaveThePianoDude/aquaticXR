#pragma strict

//var useCameraSpeed : boolean = true;
var minAlpha : float = 0;
var maxAlpha : float = 0.8;
var speedTransform : Transform;
var restSpeed : float = 1;
var movementFactor : float = 10;
private var speed : float;
private var lastPos : Vector3;
private var curPos : Vector3;
private var col : Color;

function Start () {
	col = GetComponent.<Renderer>().material.GetColor("_TintColor");
	//lastPos = speedTransform.position;
}

function Update () {

}

function AdjustParticleSpeed(){
	/*
	if(useCameraSpeed){
		speed = main.camera.velocity	
	}
	*/
	//speed = GetSpeed(speedTransform);
	
}

function GetSpeed( t : Transform ) : float{
	curPos = t.position;
	var dis : float = Vector3.Distance(lastPos, curPos);
	var currentSpeed : float = dis / Time.deltaTime;
	
	lastPos = curPos;
	return currentSpeed;
}

function FadeDustIn( fadeTime : float){
	while(true){

		col.a = Mathf.Lerp(minAlpha,maxAlpha,fadeTime);	
		GetComponent.<Renderer>().material.SetColor("_TintColor", col);
		yield;
	}
}