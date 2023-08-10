#pragma strict
/*==============================================================
Author: Glen Johnson
Email: plasticarm@gmail.com
Website: plasticarm.com / hrpictures.com
----------------------------------------------------------------
Summary:

Turbulence Scale :
	10 - Slight Itch
	100 - Flyable
	1000 - Rough
	10000 - Out Of Control

Increase min and max turbulence to shake more
Decrease min and max frequence to shake more
==============================================================*/

//COMPONENT MENU
@script AddComponentMenu ("Physics/Air Turbulence")

//////////////////////////////////////////////////////////////
//						VARIABLES
//////////////////////////////////////////////////////////////
var intensity : float = 1;
var minTurbulence : float = -100;
var maxTurbulence : float = 100;
var turbulenceScale : float = 1;
var minFrequency : float = 1;
var maxFrequency : float = 10;
var frequencyScale : float = 1;

var torqueScale : float = 20;

var fadeTurbulenceToCenter : boolean;
var maxDistance : float;

private var turbulence : Vector3;
private var collider : Collider;
private var addTurbulence : boolean = true;
private var playerInside : boolean = false;
private var player : GameObject;

//============================================================
///////////////////////FUNCTIONS//////////////////////////////
//============================================================

function Start(){
	
}

//////////////////////////////////////////////////////////////
//						FIXED UPDATE
//////////////////////////////////////////////////////////////
function FixedUpdate(){
	if(playerInside){
		if(addTurbulence){
			DoTurbulence();
		}
	}
}

//////////////////////////////////////////////////////////////
//						COLLISIONS
//////////////////////////////////////////////////////////////
function OnTriggerEnter(col : Collider) {	
	var isPlayer : boolean = IsColliderPlayer(col);	
	//Debug.Log("OnTriggerEnter : " + col.gameObject.name.ToString() + " : AirTurbulence");	
	if(isPlayer){
		player = col.gameObject;
		playerInside = true;		
		//Debug.Log("Player Entered Air Turbulence");				
	}	
}

function OnTriggerStay(col : Collider) {
	if(playerInside){
		DoTurbulence();
	}
}

function OnTriggerExit( col : Collider ){
	var isPlayer : boolean = IsColliderPlayer(col);
	if(isPlayer){
		player = col.gameObject;
		playerInside = false;
	}
}

function IsColliderPlayer( col : Collider ) : boolean{
	var isPlayer : boolean = false;
	if(col.CompareTag("Player")){	
		isPlayer = true;
	} else {
		isPlayer = false;
	}
	return isPlayer;	
}

//////////////////////////////////////////////////////////////
//						CLASS FUNCTIONS
//////////////////////////////////////////////////////////////

function DoTurbulence(){
	
	var minT : float = (minTurbulence * turbulenceScale) * intensity;
	var maxT : float = (maxTurbulence * turbulenceScale) * intensity;
		
	if(fadeTurbulenceToCenter){
		var dis : float = Vector3.Distance(player.transform.position, gameObject.transform.position);
		maxT = maxT * (dis / maxDistance);

	}
	
	turbulence = new Vector3((Random.Range(minT, maxT)),(Random.Range(minT, maxT)),(Random.Range(minT, maxT)));
	
	if(player.GetComponent(Rigidbody)) {
		player.GetComponent(Rigidbody).AddForce(turbulence);
		player.GetComponent(Rigidbody).AddRelativeTorque(turbulence * (torqueScale * intensity));
	}

	WaitRandom();

}

function WaitRandom(){
	addTurbulence = false;
	
	var minF : float = (minFrequency * frequencyScale) / intensity;
	var maxF : float = (maxFrequency * frequencyScale) / intensity;
	
	var wait : float = Random.Range(minF, maxF);
	yield WaitForSeconds(wait);
	
	addTurbulence = true;	
	
}

//////////////////////////////////////////////////////////////
//						GET SET
//////////////////////////////////////////////////////////////
function GetRandomDirection() : Vector3{
	var dir : Vector3 = new Vector3((Random.Range(minTurbulence, maxTurbulence)),(Random.Range(minTurbulence, maxTurbulence)),(Random.Range(minTurbulence, maxTurbulence)));
	return dir;
}

function SetTurbulenceScale( val : float ){
	turbulenceScale = val;
}

function SetTurbulenceFrequencyScale( val : float ){
	frequencyScale = val;
}

function SetTurbulenceIntensity( val : float ){
	intensity = val;
}