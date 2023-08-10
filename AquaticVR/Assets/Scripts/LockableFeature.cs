using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockableFeature : MonoBehaviour {

	public enum UnlockType
    {
        Treasure,
        Trash,
        Creature

    }
	
	public bool locked = false;
	public UnlockType unlockType;
	public int unlockLevel = 0;
	private GameObject player;
	// Use this for initialization
	void Start () {		
		if (locked){
			gameObject.SetActive(false);
			
		}
		player = GameObject.FindWithTag("Player");
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void UnlockFeature()
	{
		
		gameObject.SetActive(true);
	}
	
}
