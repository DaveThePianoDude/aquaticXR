using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using GoogleVR;
using DaydreamElements.ObjectManipulation;

public class Pickup : MonoBehaviour {

	public enum PickupType
    {
        Treasure,
        Trash,
        Creature,
		Coral,
		Rock

    }
	
	public enum ObjectState{ None, Selected, Released }
	
	public PickupType pickupType;
	
	public bool holdObject = true;
	public bool keepObject = true;
	private bool isHeld = false;
	public bool sleepOnStart = true;
	private Rigidbody rigidbody;
	
	private Vector3 startingPosition;

	[Tooltip("Sound played when the object is selected.")]
    public GvrAudioSource selectSound;
	
	[Tooltip("The object with the MoveablePhysicsObject script.")]
	public MoveablePhysicsObject moveObj;
	private bool isSelected;

	private BaseInteractiveObject.ObjectState state;
    private BaseInteractiveObject.ObjectState stateLastFrame;
	
	void Awake() {
		isSelected = false;
    }
	
	void Start() {
		if (holdObject){
			gameObject.AddListener(EventTriggerType.PointerDown, Hold);
			gameObject.AddListener(EventTriggerType.PointerUp, Release);
		}
		if (sleepOnStart){
			rigidbody = moveObj.GetComponent<Rigidbody>();
			if (rigidbody){
				rigidbody.Sleep();
			}
		}
		startingPosition = transform.localPosition;
				
	}

	
	void OnValidate() {
		if (!moveObj) {
			moveObj = GetComponent<MoveablePhysicsObject>();
		}
    }
	
	void Update()
	{
		
		state = moveObj.State;

		if (state == BaseInteractiveObject.ObjectState.Selected) {		
			if (rigidbody){
				rigidbody.WakeUp();
			}
			
			isSelected = true;
			isHeld = true;
		} else {
			isSelected = false;
		}

		if (isSelected && state != stateLastFrame) {
			if (selectSound != null) {
				selectSound.Play();
			}
			
			if (!holdObject){
				KeepObject();
			}
		}

		/*
		if (isHeld && !isSelected && keepObject){
			KeepObject();
		}
		*/
		stateLastFrame = moveObj.State;
	  
		
	}
	
	public void Hold() 
	{
		
		isHeld = true;
		
	}
	
	public void Release()
	{
		if (keepObject){
			KeepObject();
			Destroy(gameObject, 1);
			
		}
		
	}
	
	public void KeepObject()
	{
			
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
	
}

/*
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
	
}*/