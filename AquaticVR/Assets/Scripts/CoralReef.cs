using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoralReef : MonoBehaviour {

	public int qualityLevel = 0;
	public GameObject[] fishParticles;
	private int[] fishParticleMax;
	public GameObject[] coralGenerators;
	public GameObject coral;
	
	// Use this for initialization
	void Start () {
		/*
		// Send the infinitReefManager this gameobject to add to new reef var
		GameObject irmGo = GameObject.FindWithTag("Player");
		if (irmGo){
			//infinitReefManager irm = irmGo.GetComponents<infinitReefManager>();
			irmGo.SendMessage("AddNewReef", gameObject);
		}
		*/
		
		fishParticleMax = new int[fishParticles.Length];
		for(int i =0;i< fishParticles.Length; i++){
			ParticleSystem partSys = fishParticles[i].GetComponent<ParticleSystem>();
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			fishParticleMax[i] = maxParticles;
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void DecreaseReefQuality( int ql )
	{
		qualityLevel = ql;
		for(int i =0;i< fishParticles.Length; i++){
			ParticleSystem partSys = fishParticles[i].GetComponent<ParticleSystem>();
			if(!partSys){
				continue ;
			}
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			maxParticles = Mathf.FloorToInt(fishParticleMax[i] / (qualityLevel + 1 ));			
			mainModule.maxParticles = maxParticles;
			
		}
		
		/*
		int maxCoralGen = Mathf.FloorToInt(coralGenerators.Length - (coralGenerators.Length / (qualityLevel + 1 )));
		for(int i =0;i< maxCoralGen; i++ ){
			coralGenerators[i].SetActive(false);
		}*/
		
		int children = coral.transform.childCount;
		children = Mathf.FloorToInt(children - (children / (qualityLevel + 1 )));		
		for (int i = 0; i < children; ++i){
			Transform t = coral.transform.GetChild(i);
			t.gameObject.SetActive(false);
		}
	}
	
	public void IncreaseReefQuality( int ql)
	{
		qualityLevel = ql;
		for(int i =0;i< fishParticles.Length; i++){
			ParticleSystem partSys = fishParticles[i].GetComponent<ParticleSystem>();
			if(!partSys){
				continue ;
			}
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			maxParticles = Mathf.FloorToInt(fishParticleMax[i] / (qualityLevel + 1 ));		
			mainModule.maxParticles = maxParticles;
			
		}
		
		int children = coral.transform.childCount;
		for (int i = 0; i < children; ++i){
			Transform t = coral.transform.GetChild(i);
			t.gameObject.SetActive(true);
		}
		
		children = Mathf.FloorToInt(children - (children / (qualityLevel + 1 )));	

		
		for (int i = 0; i < children; ++i){
			Transform t = coral.transform.GetChild(i);
			t.gameObject.SetActive(false);
		}
		/*
		int maxCoralGen = Mathf.FloorToInt(coralGenerators.Length - (coralGenerators.Length / (qualityLevel + 1 )));
		for(int i =0;i< maxCoralGen; i++ ){
			coralGenerators[i].SetActive(true);
		}*/
		
	}
	
}
