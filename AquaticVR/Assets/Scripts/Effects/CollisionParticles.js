#pragma strict
@script AddComponentMenu ("Particles/Collision Particles")
/* //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

*/ //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


enum HitObjectType {
	HitBody,
	HitMetal,
	HitGround,
	HitWood,
	HitGlass,
	HitWater

}

class HitParticles extends System.Object {

	var hitParticlesName : String;
	var hitObjectType : HitObjectType;
	var particlePrefab : GameObject;

	var hitSoundPrefab : GameObject[];
	
	var hitSound : AudioClip[];
	var minPitch : float = 0.25;
	var maxPitch : float = 2.0;
	var minHitForce : float = 1;
	var maxHitForce : float = 10;
	//var continuousCollisions : boolean = false;
	var ignoreTags : String[];
	
}

var collisionParticlesEnabled : boolean;

var myHitParticles : HitParticles[];
var otherHitParticles : HitParticles[];

private var bodyTags : String[] = ["Player", "Enemy", "Friendly", "Frienemy", "Boss"];
private var metalTags : String[];
private var groundTags : String[] = ["Ground", "Rock", "Untagged"];
private var woodTags : String[];
private var glassTags : String[];
private var hitForce : float;


function Start(){
		
}

function BulletHit( pos : Vector3, dir : Quaternion, hitType : HitObjectType ){
	for(var hp : HitParticles in otherHitParticles){
		if(hp.hitObjectType == hitType){
			var newP : GameObject = hp.particlePrefab;
		}
	}
	var newParticles : GameObject = Instantiate(newP, pos, dir);	
}

function GetObjectHitType( hitObject : Transform ) : HitObjectType{
	//This is alternative if we want to pass the object instead of the hitObjType.
	//This does not work! It seems that it doesn't work because we can't pass the raycast hit info on to this.
	
	var hitType : HitObjectType;
	
	/*
	for(var bt : String in bodyTags){
		if(otherHitParticles.CompareTag(bt)){
			hitType = HitObjectType.HitBody;
		}
	}
	*/
	/*
	for(var mt : String in metalTags){
		if(hitObject.CompareTag(mt)){
			hitType = HitObjectType.HitMetal;
		}
	}
	
	for(var gt : String in metalTags){
		if(hitObject.CompareTag(gt)){
			hitType = HitObjectType.HitGround;
		}
	}
	*/
	return hitType;
}

function OnCollisionEnter(collision : Collision) {
	
    // Debug-draw all contact points and normals
	//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "DebugMN", ("Collision : RelativeVelocityMagnitude : " + collision.relativeVelocity.magnitude.ToString())));
	
	//Debug.Log("HIT");
	var enoughForce = IsCollisionForceEnough(collision);
	if(!enoughForce){ return; }
	
	for(var mhp : HitParticles in myHitParticles){
		for(var il : String in mhp.ignoreTags){
			if(collision.gameObject.tag == il){
				return;
			}
		}
		if(mhp.hitObjectType == HitObjectType.HitGround){
			for(var contact : ContactPoint in collision.contacts) {
				GroundHit(contact.point, contact.normal);
			}
		}
		
	}
	
	//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "DebugMN", "Collision Particles : "));
	
	
	/*
    if(collision.Transform.CompareTag("Player")){
		var newParticles : GameObject = Instantiate(
	}
	*/
	
	/*
    // Play a sound if the coliding objects had a big impact.        
    if(collision.relativeVelocity.magnitude > 2)
        audio.Play();
		*/
}

function IsCollisionForceEnough( collision : Collision ) : boolean {
	for(var hp : HitParticles in myHitParticles){
		//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "DebugMN", ("Hit Force : " + hitForce.ToString())));
		hitForce = collision.relativeVelocity.magnitude;
		if(hitForce > hp.minHitForce){				
			return true;			
		}
	}
	
	return false;
}

function PlayerHit( pos : Vector3, dir : Vector3){
	for(var hp : HitParticles in otherHitParticles){
		if(hp.hitObjectType == HitObjectType.HitBody){
			var newP : GameObject = hp.particlePrefab;
		}
	}
	
	var newParticles : GameObject = Instantiate(newP, pos, Quaternion.identity);

}

function GroundHit( pos : Vector3, dir : Vector3){
	//Debug.Log("Ground Hit : " + pos.ToString());
	//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "DebugMN", " - Ground Hit - "));
	if(!collisionParticlesEnabled){ return; }
	
	for(var mhp : HitParticles in myHitParticles){
		if(mhp.hitObjectType == HitObjectType.HitGround){
			//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "DebugMN", (" - Ground Hit Force : " + hitForce.ToString())));
			if(hitForce > mhp.minHitForce && hitForce < mhp.maxHitForce){
				//Debug.Log("Hit Force : " + (hitForce.ToString()));
				var newP : GameObject = mhp.particlePrefab;
				var hitPitch : float = Mathf.Clamp((mhp.maxPitch - hitForce), mhp.minPitch, mhp.maxPitch);
				//Debug.Log("MinPitch : " + mhp.minPitch + " MaxPitch : " + mhp.maxPitch);
				DoHitSound(mhp.hitSoundPrefab[(Random.Range(0,mhp.hitSoundPrefab.length - 1))], pos, Quaternion.identity, hitPitch);
			}
		}
	}
	
	var newParticles : GameObject = Instantiate(newP, pos, Random.rotation);
	//Debug.Log("Hit Particles!");
}

function DoHitSound( hitSound : GameObject , pos : Vector3, rot : Quaternion , hitPitch : float ){
	var hs : GameObject = Instantiate(hitSound, pos, rot);
	hs.transform.parent = transform;
	var au : AudioSource = hs.GetComponent("AudioSource");
	if (au){
		//Debug.Log("Hit Pitch : " + hitPitch);
		au.pitch = hitPitch;
	}
}

function TriggerParticles( pos : Vector3, hitType : HitObjectType ){
	for(var hp : HitParticles in otherHitParticles){
		if(hp.hitObjectType == hitType){
			var newP : GameObject = hp.particlePrefab;
		}
	}
	var newParticles : GameObject = Instantiate(newP, pos, transform.rotation);	
}

function ToggleCollisionParticles( state : boolean ){
	collisionParticlesEnabled = state;
}