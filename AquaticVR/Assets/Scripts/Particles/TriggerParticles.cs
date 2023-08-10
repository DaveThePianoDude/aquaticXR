using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticles : MonoBehaviour
{
	//public int emitCount = 10;
	public GameObject particles;
	
    void Awake()
    {
       
    }

    void Update()
    {

    }
/*
	void OnCollisionEnter(Collision other)
    {
		Vector3 pos = other.transform.position;
		GameObject newParticles = Instantiate(particles, pos, Quaternion.identity);
        Debug.Log("entered particle trigger");
	}
	*/
	
    private void OnTriggerEnter(Collider other)
    {
		Vector3 pos = other.transform.position;
		GameObject newParticles = Instantiate(particles, pos, Quaternion.identity);
        Debug.Log("entered particle trigger");
        
    }


/*
    private void OnTriggerExit(Collider other)
    {
		
    }
	*/
}