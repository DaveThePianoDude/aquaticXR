#pragma strict

var autoSpin : boolean = true;
var spinRigidbody : GameObject;
var spinGraphic : GameObject;

var spinStrength : float = 200;
var curStrength : float;
var maxSpinStrength : float = 400;
var acceleration : float = 10;
var recoveryRate : float = 2;
var angularDrag : float = 0.1;
//var spinOffset : Vector3;
private var spinForce : Vector3;
var spinVector : Vector3;
var spinButtonName = "Spin";
var uniDirectionSpin : boolean = true;
var spinDirection : float = 1;
private var spinContinueForce : Vector3;

function Start () {
	curStrength = spinStrength;
	spinRigidbody.GetComponent.<Rigidbody>().angularDrag = angularDrag;
}

function Update () {
	
	if (autoSpin){
		SpinObject();
	}
	if(Input.GetButton("Jump")){
		//curStrength = Mathf.Lerp(curStrength, maxSpinStrength, 10);
		curStrength = Mathf.MoveTowards(curStrength, maxSpinStrength, acceleration * Time.deltaTime);
		SpinObject();
	} else {
		curStrength = Mathf.MoveTowards(curStrength, spinStrength, recoveryRate * Time.deltaTime);
		//curStrength = Mathf.Lerp(curStrength, spinStrength, 10);
	}
}

function SpinObject () {
	
	//var spinInput : float = Input.GetAxis("Spin");
	if (uniDirectionSpin){
		if (autoSpin){
			spinForce = (spinVector*curStrength) * 1;
			spinRigidbody.GetComponent.<Rigidbody>().AddRelativeTorque(spinForce);
		}
		/*
		if ((Input.GetAxis(spinButtonName)) == spinDirection){
			spinForce = (spinVector*curStrength) * Input.GetAxis(spinButtonName);
			spinRigidbody.GetComponent.<Rigidbody>().AddRelativeTorque(spinForce);
		}*/
	} else {
		if (autoSpin){
			spinForce = (spinVector*curStrength) * -1;
			spinRigidbody.GetComponent.<Rigidbody>().AddRelativeTorque(spinForce);
		} else {
			spinForce = (spinVector*curStrength) * Input.GetAxis(spinButtonName);
			spinRigidbody.GetComponent.<Rigidbody>().AddRelativeTorque(spinForce);
		}
	}

	//spinRigidbody.rigidbody.AddRelativeTorque(spinForce);

}

function SwitchDirection(){
	if (spinDirection == 1){
		spinDirection = -1;
	} else {
		spinDirection = 1;
	}
	
	Debug.Log ("Switch Direction!");
}

function StopSpinning(){
	spinRigidbody.GetComponent.<Rigidbody>().angularDrag = 2.0;
	Debug.Log ("Stop Spinning!");
}