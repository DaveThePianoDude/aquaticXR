#pragma strict

var autoStartStorm : boolean = false;
var startPosition : Vector3;
var endPosition : Vector3;
var windDirection : Vector3;
var loop : boolean;
var stormPrefab : GameObject;
var cameraDust : GameObject;
var stormTurbulence : GameObject;
var minCamDustAlpha : float = 0;
var maxCamDustAlpha : float = 0.3;
var stormDuration : float = 120;

var elapsedStormTime : float;

private var storm : GameObject;
private var camDust : GameObject;
private var col : Color;
private var freeze : boolean;

function Start () {
	col = cameraDust.GetComponent.<Renderer>().material.GetColor("_TintColor");
	col.a = minCamDustAlpha;
	cameraDust.GetComponent.<Renderer>().material.SetColor("_TintColor", col);
	//transform.position = startPosition;
	//ResetStorm();
	
	//MoveStorm();
	if(autoStartStorm){
		StartStorm();
	}
}

function Update () {
	
}

function MoveStorm(){
	var st : float = Time.time;
	storm.transform.position = startPosition;
	var startTime : float = Time.time;
	while(true){
		if(!storm){ return;}
		//Debug.Log("******Move Storm : " + storm.transform.position.ToString());
		//Dont move while freeze
		if(freeze){
			while(freeze){
				yield;
			}
		}
		var dis : float = Vector3.Distance(storm.transform.position, endPosition);
		storm.transform.position += (endPosition - storm.transform.position) / ((stormDuration - (Time.time - startTime)) / Time.deltaTime);	
	
		
		if(dis < 10){
			var curTime : float = Time.time;
			elapsedStormTime = curTime - st;
			Debug.Log("//////////Storm Elapsed Time: " + elapsedStormTime.ToString());
			return;
		}
		/*
		
		storm.transform.position = Vector3.Lerp(startPosition, endPosition,  (Time.time / stormDuration ));
		*/
		//storm.transform.position = Vector3.MoveTowards(storm.transform.position, endPosition,  (Time.deltaTime * stormDuration ));
		//storm.transform.Translate(windDirection * Time.deltaTime, Space.World);
		yield;
	}
}

function ResetStorm(){	
	storm.transform.position = startPosition;
}

function StopStorm(){
	if(!storm){ return;}
	storm.SetActive(false);
	cameraDust.SetActive(false);
}

function StartStorm(){
	StopStorm();
	storm = Instantiate(stormPrefab, startPosition, stormPrefab.transform.rotation);
	MoveStorm();
	FadeCameraDustIn();
}

function Freeze( state : boolean ){
	freeze = state;
}

function FadeCameraDustIn(){
	
	cameraDust.SetActive(true);
	col.a = minCamDustAlpha;
	
	var startTime : float = Time.time;
	while(col.a < maxCamDustAlpha){	
		col.a += (maxCamDustAlpha - col.a) / ((stormDuration - (Time.time - startTime)) / Time.deltaTime);		
		cameraDust.GetComponent.<Renderer>().material.SetColor("_TintColor", col);		
		yield;
	}
	//Debug.Log("Fade In Time: " + (Time.time - startTime).ToString());
}

function FadeCameraDustOut( fadeTime : float ){
	var startTime : float = Time.time;	
	var currentAlpha : float = col.a;
	while(col.a < minCamDustAlpha){
		col.a += (currentAlpha - col.a) / ((stormDuration - (Time.time - startTime)) / Time.deltaTime);	
		cameraDust.GetComponent.<Renderer>().material.SetColor("_TintColor", col);
		
		yield;
	}
	
}

function SetStormDuration( dur : float ){
	stormDuration = dur;
}
