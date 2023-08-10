@script AddComponentMenu ("Particles/Collision Particles")
/* //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

*/ //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
class CollisionSoundEffect extends System.Object {
	var collisionSoundPrefab : GameObject[];
	var minHitForce : float = 1;
	var maxHitForce : float = 10;

}

var collisionSoundEffect : CollisionSoundEffect[];
private var hitForce : float;

function Start(){
		
}

function OnCollisionEnter(collision : Collision) {
/*
	var enoughForce = IsCollisionForceEnough(collision);
	if(!enoughForce){ return; }
	*/
	for(var cse : CollisionSoundEffect in collisionSoundEffect){
		for(var contact : ContactPoint in collision.contacts) {
			DoSoundEffect(contact.point, contact.normal);
		}		
	}
	
}

function IsCollisionForceEnough( collision : Collision ) : boolean {
	for(var cse : CollisionSoundEffect in CollisionSoundEffect){
		hitForce = collision.relativeVelocity.magnitude;
		if(hitForce > cse.minHitForce){				
				return true;			
		}
	}	
	return false;
}

function DoSoundEffect( pos : Vector3, dir : Vector3){
	for(var cse : CollisionSoundEffect in CollisionSoundEffect){
			if(hitForce > cse.minHitForce && hitForce < cse.maxHitForce){			
				var sound : GameObject = cse.collisionSoundPrefab[(Random.Range(0,cse.collisionSoundPrefab.length - 1))];
				var hs : GameObject = Instantiate(sound, pos, transform.rotation);
				Debug.Log(">>>>>>>>>>>>>>>>>>>>>Collision Sound Effect");
			}

	}

}
