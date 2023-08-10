#pragma strict

public var player : GameObject;
public var lightHeight : float = 500.0;
public var xPosFolow : boolean = true;
public var refreshTime : float = 10.0;
var doRefresh : boolean = false;
var useRefreshRate : boolean = false;
private var startTime : float ;
private var journeyLength : float;
public var speed : float = 1.0;
private var lastPos : Vector3;

function Start () {
	if (useRefreshRate){
		RefreshLightPosition();
	} else {
		doRefresh = true;
	}
}

function Update () {
	/*
	var pt : Vector3 = player.transform.position;
		pt.y = lightHeight;
		if (xPosFolow == false){
			pt.x = 0.0;
		}
		var lp : Vector3 = transform.position;
		transform.position = Vector3.Lerp(transform.position, pt, (Time.deltaTime * speed));
	*/
	
	/*
	var distCovered : float = (Time.time - startTime) * speed;
    var fracJourney : float = distCovered / journeyLength;
	if (doRefresh){
		var pt : Vector3 = player.transform.position;
		pt.y = lightHeight;
		if (xPosFolow == false){
			pt.x = 0.0;
		}
		var lp : Vector3 = transform.position;
		
		transform.position = Vector3.Lerp(transform.position, pt, (Time.deltaTime * speed ));
		transform.position = pt;
		if (useRefreshRate == true ){
			var dist : float = Vector3.Distance(lp, pt);
			if (dist < 0.01){
				doRefresh = false;
			}
			
		}
	}
	*/

	if (doRefresh){
		var pt : Vector3 = player.transform.position;
		pt.y = lightHeight;
		if (xPosFolow == false){
			pt.x = 0.0;
		}
		
		var lp : Vector3 = transform.position;
		var distP : float = Vector3.Distance(lastPos, pt);
		if (distP > 100.0){
			//transform.position = Vector3.Lerp(transform.position, pt, (Time.deltaTime * speed ));
			transform.position = pt;
		}
		//
		if (useRefreshRate == true ){
			//var dist : float = Vector3.Distance(lp, pt);
			//if (dist < 0.01){
			//	doRefresh = false;
			//}
			doRefresh = false;
		}
		lastPos = pt;
	}
	
}

function RefreshLightPosition(){
	while (true ){
		yield WaitForSeconds(refreshTime);
		doRefresh = true;
		
		//Debug.Log ("Refresh Light Position");
		yield;
		
	}
	
}