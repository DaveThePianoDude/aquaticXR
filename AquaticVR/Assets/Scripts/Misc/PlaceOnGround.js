#pragma strict

var maxHeight : float = 10000;
var minHeight : float = -10000;

function Start () {
	//transform.position = startPos;
	PlaceGameObjectOnGround();
}

function Update () {

}

function PlaceGameObjectOnGround(){	
	
	var rcPos : Vector3 = transform.position;
	rcPos.y = rcPos.y + maxHeight;
	rcPos = transform.TransformPoint(rcPos);
	//Debug.Log("Ray Start: " + rcPos.ToString());
	
	var rcEnd : Vector3 = transform.position;
	rcEnd.y = rcEnd.y - Mathf.Abs(minHeight);
	rcEnd = transform.TransformPoint(rcEnd);
	//Debug.Log("Ray End: " + rcEnd.ToString());
	
	transform.position = rcPos;
	
	var rcDistance : float = maxHeight + Mathf.Abs(minHeight);
	//rcPos.y = rcPos.y - 0.001;
	
	var hit : RaycastHit;
	
	Debug.DrawLine(rcPos, rcEnd, Color.green, 10, false);
	
	if(Physics.Raycast (rcPos, -Vector3.up, hit, rcDistance)){
		var hitY : float;
		if(hit.collider.gameObject == this.gameObject){
			if(Physics.Raycast (hit.point, -Vector3.up, hit, rcDistance)){	
				hitY = hit.point.y;
				transform.position.y = hitY;
				return;
			}
			
		}
		hitY = hit.point.y;			
		transform.position.y = hitY;		
	}	
}