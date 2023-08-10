using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using GoogleVR;
using DaydreamElements.ObjectManipulation;

[RequireComponent(typeof(Collider))]
public class ObjPickup : MonoBehaviour {
	
	public enum PickupType
    {
        Treasure,
        Trash,
        Creature,
		Coral,
		Rock

    }
	
	public PickupType pickupType;
	public bool holdObject = true;
	public bool keepObject = true;
	private bool isHeld = false;
	public float throwMultiplier = 1.0f;
	
	private Vector3 startingPosition;
	private Renderer myRenderer;

	public Material inactiveMaterial;
	public Material gazedAtMaterial;

	public AudioClip clickedClip;
	private AudioSource audioSource;
	
	private Vector3 _throwVelocity;
	private Vector3 _previousPosition;
	
	void Start() {
		if (holdObject){
			gameObject.AddListener(EventTriggerType.PointerDown, Hold);
			gameObject.AddListener(EventTriggerType.PointerUp, Release);
		}		
  
		startingPosition = transform.localPosition;
		myRenderer = GetComponent<Renderer>();
		audioSource = GetComponent<AudioSource>();
		SetGazedAt(false);
	}

	public void SetGazedAt(bool gazedAt) {
		if (inactiveMaterial != null && gazedAtMaterial != null) {
			myRenderer.material = gazedAt ? gazedAtMaterial : inactiveMaterial;
			return;
		}
	}
	
	void Update()
	{
		
		if (Input.GetKey("m")){
			Collect();
			
		}	
		
		if (isHeld){
			// the velocity is based on the previous position
			Vector3 frameVelocity = (transform.position - _previousPosition) / Time.deltaTime;

			const int samples = 3;
			// average the velocity calculate over the last number of frames
			_throwVelocity = _throwVelocity * (samples - 1) / samples + frameVelocity / samples;

			// update previous position
			_previousPosition = transform.position;
		}
	}

	/*
	public void Reset() {
	  int sibIdx = transform.GetSiblingIndex();
	  int numSibs = transform.parent.childCount;
	  for (int i=0; i<numSibs; i++) {
		GameObject sib = transform.parent.GetChild(i).gameObject;
		sib.transform.localPosition = startingPosition;
		sib.SetActive(i == sibIdx);
	  }
	}

	
	public void Recenter() {
	#if !UNITY_EDITOR
	  GvrCardboardHelpers.Recenter();
	#else
	  if (GvrEditorEmulator.Instance != null) {
		GvrEditorEmulator.Instance.Recenter();
	  }
	#endif  // !UNITY_EDITOR
	}

	public void TeleportRandomly(BaseEventData eventData) {
	  // Pick a random sibling, move them somewhere random, activate them,
	  // deactivate ourself.
	  int sibIdx = transform.GetSiblingIndex();
	  int numSibs = transform.parent.childCount;
	  sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
	  GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

	  // Move to random new location ±100º horzontal.
	  Vector3 direction = Quaternion.Euler(
		  0,
		  Random.Range(-90, 90),
		  0) * Vector3.forward;
	  // New location between 1.5m and 3.5m.
	  float distance = 2 * Random.value + 1.5f;
	  Vector3 newPos = direction * distance;
	  // Limit vertical position to be fully in the room.
	  newPos.y = Mathf.Clamp(newPos.y, -1.2f, 4f);
	  randomSib.transform.localPosition = newPos;

	  randomSib.SetActive(true);
	  gameObject.SetActive(false);
	  SetGazedAt(false);
	}
*/

	public void Collect()
	{
		
		
		//gameObject.SetActive(false);
		
		/*
		SetGazedAt(false);
		*/
		
		/*
		GameObject player = GameObject.FindWithTag("Player");
		if (player){
			if (pickupType == PickupType.Treasure){
				player.SendMessage("CollectTreasure", SendMessageOptions.DontRequireReceiver);
			}
			if (pickupType == PickupType.Trash){
				player.SendMessage("CollectTrash", SendMessageOptions.DontRequireReceiver);
			}
			
			if (pickupType == PickupType.Creature){
				player.SendMessage("CollectCreature", SendMessageOptions.DontRequireReceiver);
			}
		}
		*/
	}
	
	public void KeepObject()
	{
		
		//gameObject.SetActive(false);
		
		SetGazedAt(false);
		
		GameObject player = GameObject.FindWithTag("Player");
		if (player){
			if (pickupType == PickupType.Treasure){
				player.SendMessage("CollectTreasure", SendMessageOptions.DontRequireReceiver);
			}
			if (pickupType == PickupType.Trash){
				player.SendMessage("CollectTrash", SendMessageOptions.DontRequireReceiver);
			}
			
			if (pickupType == PickupType.Creature){
				player.SendMessage("CollectCreature", SendMessageOptions.DontRequireReceiver);
			}
		}
		
	}
	
	public void Hold() 
	{
		
		isHeld = true;
		
		
		// get the Transform component of the pointer
		Transform pointerTransform = GvrPointerInputModule.Pointer.PointerTransform;

		//GvrPointerInputModule.Pointer.MaxPointerDistance = 0.5f;
		//GVRLaserVisual gvrlv = (GVRLaserVisual)FindObjectOfType(typeof(GVRLaserVisual));
		//gvrlv.LaserVisual.SetDistance(0.5f);
		
		//GVRLaserPointer gvrlp = (GVRLaserPointer)FindObjectOfType(typeof(GVRLaserPointer));
		//gvrlp.LaserVisual.SetDistance(0.5f);
		
		/*
		GvrLaserVisual.SetDistance(0.5f);
		*/
		
		// set the GameObject's parent to the pointer
		transform.SetParent(pointerTransform, false);

		// position it in the view
		transform.localPosition = new Vector3(0, 0, 2);

		transform.localScale = new Vector3(0.1f, 0.1f, .1f);
		
		// Add a rigidbody if there is none
		if (!gameObject.GetComponent<Rigidbody>()){			
			Rigidbody rb = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
		}
		
		// disable physics
		GetComponent<Rigidbody>().isKinematic = true;
	}
	
	public void Release() {
		
		if (keepObject){
			KeepObject();
			Destroy(gameObject, 1);
			
		}
		

		//LaserVisual.SetDistance(defaultReticleDistance);
		
		// set the parent to the world
		transform.SetParent(null, true);

		// get the rigidbody physics component
		Rigidbody rigidbody = GetComponent<Rigidbody>();

		// reset velocity
		rigidbody.velocity = Vector3.zero;

		// enable physics
		rigidbody.isKinematic = false;
		
		rigidbody.AddForce((_throwVelocity * throwMultiplier), ForceMode.VelocityChange);
	}
}

public static class EventExtensions {

	public static void AddListener(this GameObject gameObject,
								EventTriggerType eventTriggerType,
								UnityAction action) {
		// get the EventTrigger component; if it doesn't exist, create one and add it
		EventTrigger eventTrigger = gameObject.GetComponent<EventTrigger>()
								?? gameObject.AddComponent<EventTrigger>();

		// check to see if the entry already exists
		EventTrigger.Entry entry;
		entry = eventTrigger.triggers.Find(e => e.eventID == eventTriggerType);

		if (entry == null) {
			// if it does not, create and add it
			entry = new EventTrigger.Entry {eventID = eventTriggerType};

			// add the entry to the triggers list
			eventTrigger.triggers.Add(entry);
		}

		// add the callback listener
		entry.callback.AddListener(_ => action());
	}

}

