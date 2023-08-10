#pragma strict

enum RandomizeEvent{
	AtStart,
	OnClick,
	Periodically,
	Trigger
	
}





enum RandomizeType{
	GameObject,
	Component,
	Color,
	Texture,
	TextureURL,
	Transform
	
}


class RandomizationEvent extends System.Object {
	var active : boolean = true;
	var randomizeEvent : RandomizeEvent;
	var minRandomTime : float = 10.0;
	var maxRandomTime : float  = 100.0;
	var randomTrigerEventName = "Randomize";
	
	function RunPeriodicRandomization( go : GameObject, callback : String ){
		while(true){
			go.SendMessage (callback);
			Debug.Log ("Run Periodic Randomization " + callback);
			//randomImage.RandomizeImage(this.gameObject);
			yield WaitForSeconds((Random.Range(minRandomTime, maxRandomTime)));
			
			//RunPeriodicRandomization();
		}
	}
}

class RandomizationMethod extends System.Object {
	var active : boolean = true;
	//var randomizeEvent : RandomizeEvent;
	var methodName : String;
	var randomizeType : RandomizeType;
	var actionableObject : GameObject;

}

class RandomGameObject extends System.Object{
	var active : boolean = true;
	var name : String = "RandomGameObject";
	var gameObject : GameObject[];
	var curObject :  GameObject;
	var randomizationEvent : RandomizationEvent;
	private var selectedIndex : int;
	var randomScale : boolean = true;
	var minScale : float = 0.5f;
	var maxScale : float = 2.0f;
	
	function RandomizeGameObject( go : GameObject ){
		var rend : Renderer = go.GetComponent(Renderer);
		var index: int = Mathf.FloorToInt(Random.Range(0, (gameObject.length)));
		selectedIndex = index;
		
	}
}

class RandomColor extends System.Object{
	var active : boolean = true;
	var name : String = "RandomColor";
	var color : Color[];
	var randomizationEvent : RandomizationEvent;
	private var selectedIndex : int;
	
	function RandomizeColor( go : GameObject ){
		var rend : Renderer = go.GetComponent(Renderer);
		var index: int = Mathf.FloorToInt(Random.Range(0, (color.length)));
		selectedIndex = index;
		
	}
}

class RandomTexture extends System.Object{
	var active : boolean = true;
	var name : String = "Head";
	var texture : Texture[];
	var randomizationEvent : RandomizationEvent;
	private var selectedIndex : int;
	
	function RandomizeTexture( go : GameObject ){
		var rend : Renderer = go.GetComponent(Renderer);
		var index: int = Mathf.FloorToInt(Random.Range(0, (texture.length)));
		selectedIndex = index;
		
		rend.material.mainTexture = texture[index];
		
		
	}

}

class RandomTextureURL extends System.Object{
	var active : boolean = true;
	var name : String = "Head";
	var url : String[];
	var randomizationEvent : RandomizationEvent;
	private var selectedIndex : int;
	
	function RandomizeTextureURL( go : GameObject ){
		var rend : Renderer = go.GetComponent(Renderer);
		var index: int = Mathf.FloorToInt(Random.Range(0, (url.length)));
		var www : WWW = new WWW (url[index]);

		// Wait for download to complete
		yield www;

		// Print the error to the console
		if (www.error != null)
			Debug.Log(www.error);

		// assign texture
		rend.material.mainTexture = www.texture;
	
		//rend.material.mainTexture = image[index];
		selectedIndex = index;
		
	}

}


class RandomMessage extends System.Object{

	var message : String[];
	var randomizationEvent : RandomizationEvent;
	private var selectedIndex : int;
	
	function SendRandomMessage( go : GameObject ){
		var index: int = Mathf.FloorToInt(Random.Range(0, (message.length)));
		selectedIndex = index;
		Debug.Log ("Sending Random Message : " + message[index] );
		go.SendMessage ((message[index]), SendMessageOptions.DontRequireReceiver);
		
	}

}

var randomGameObject : RandomGameObject[];
var parentGameObjects : Boolean = true;
var randomTexture : RandomTexture[];
var randomMessage : RandomMessage[];
var randomEnabled : Boolean = false;
var disableChildrenOnStart : Boolean = false;
var dropObject : Boolean = false;

function Start(){
	//randomImage.randomizeImage(this.gameObject);
	if (disableChildrenOnStart){
		DisableChildren();
	}
	
	for(ranTexture in randomTexture){
		if (ranTexture.randomizationEvent.active == true){
			if (ranTexture.randomizationEvent.randomizeEvent == RandomizeEvent.Periodically){
				
				//randomImage.randomizationEvent.RunPeriodicRandomization(this.gameObject, randomImage.randomizationEvent.randomTrigerEventName);
				RunPeriodicRandomization("RandomImage", ranTexture.randomizationEvent.minRandomTime, ranTexture.randomizationEvent.maxRandomTime);
				//RunPeriodicRandomization();
			}
		}
	}
	
	for (ranMessage in randomMessage){
		if (ranMessage.randomizationEvent.active == true){
			if (ranMessage.randomizationEvent.randomizeEvent == RandomizeEvent.Periodically){
				RunPeriodicRandomization("RandomMessage", ranMessage.randomizationEvent.minRandomTime, ranMessage.randomizationEvent.maxRandomTime);
	
			}
		}
	}
	
	for (ranGameObject in randomGameObject){
		if (ranGameObject.randomizationEvent.active == true){
			if (ranGameObject.randomizationEvent.randomizeEvent == RandomizeEvent.Periodically){
				RunPeriodicRandomization("RandomizeGameObject", ranGameObject.randomizationEvent.minRandomTime, ranGameObject.randomizationEvent.maxRandomTime);
	
			}
			
			if (ranGameObject.randomizationEvent.randomizeEvent == RandomizeEvent.AtStart){
				RandomizeGameObject(ranGameObject);
	
			}
		}
	}
	
	if (randomEnabled){
		var en: int = Mathf.FloorToInt(Random.Range(0, 2));
		if (en == 0){
			Destroy(gameObject);
			}
		}
	
	if (dropObject){
		DropObject();
	}
}

function DropObject()
{
	var hit : RaycastHit;
    var down = Vector3(0,-1, 0);
	if (Physics.Raycast (transform.position, down, hit)) {
		var distanceToGround = hit.distance;
		var currentPos = transform.position;
		var newY = currentPos.y-distanceToGround;
		transform.position = Vector3(currentPos.x, newY, currentPos.z);
	}
}

function DisableChildren(){
	
	var children : int = transform.childCount;
	for (var i : int = 0; i < children; ++i){
			var t : Transform = transform.GetChild(i);
			
			t.gameObject.SetActive(false);
			
		}
}

function RandomizeGameObject( ranGo : RandomGameObject ){
	if (ranGo.curObject){
		Destroy(ranGo.curObject);
		
	}
	
	var index: int = Mathf.FloorToInt(Random.Range(0, (ranGo.gameObject.length)));
	ranGo.curObject = Instantiate(ranGo.gameObject[index], transform.position, transform.rotation) as GameObject;
	if (parentGameObjects){
		ranGo.curObject.transform.SetParent(transform, true);
	}
	if (ranGo.randomScale){
		var ranScale : float = Random.Range(ranGo.minScale, ranGo.maxScale);
		ranGo.curObject.transform.localScale = new Vector3((ranScale),(ranScale), (ranScale));
		
	}
	
}

function RandomizeTexture(){
	for(ranTexture in randomTexture){
		ranTexture.RandomizeTexture(this.gameObject);
	}
	
}

function RandomizeMessage(){
	for (ranMessage in randomMessage){
		ranMessage.SendRandomMessage(this.gameObject);
	
	}
}

function PeriodicTextureRandomization(){
	while(true){

		//gameObject.SendMessage(callback, SendMessageOptions.DontRequireReceiver);
		//yield WaitForSeconds((Random.Range(minTime, maxTime)));
		
	}
}

function RunPeriodicRandomization( callback : String, minTime : float, maxTime : float ){
	while(true){

		gameObject.SendMessage(callback, SendMessageOptions.DontRequireReceiver);
		yield WaitForSeconds((Random.Range(minTime, maxTime)));
		//randomImage.RandomizeImage(this.gameObject);
		//yield WaitForSeconds((Random.Range(randomImage.randomizationEvent.minRandomTime, randomImage.randomizationEvent.maxRandomTime)));
		
	}
}
