using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChildComponents : MonoBehaviour {

	public bool disableOnStart = true;
	public bool state = false;
	public bool addMeshColliders = false;
	public Component[] colliders;
	public MeshCollider[] meshColliders;
	private MeshRenderer[] meshRenderers;
	

    void Start()
    {
		if (addMeshColliders){
			AddMeshColliders();
		}
		
		colliders = GetComponentsInChildren<Collider>();
		meshColliders = GetComponentsInChildren<MeshCollider>();
		
		if (disableOnStart){
			state = false;
			ToggleComponenets();
		}
    }
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void AddMeshColliders()
	{
		meshRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer mren in meshRenderers){
			MeshCollider mc = mren.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
		}
		

	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (!other.transform.CompareTag("Player")){
			return ;
		} 
		state = true;
		ToggleComponenets();
	}

	public void ToggleComponenets()
	{
		foreach (MeshCollider mcol in meshColliders){
			mcol.enabled = state;
		}
		/*
		foreach (Collider col in colliders){
			col.enabled = state;
		}*/
	}
	
	private void OnTriggerExit(Collider other)
	{
		if (!other.transform.CompareTag("Player")){
			return ;
		} 
		state = false;
		ToggleComponenets();
	}
}
