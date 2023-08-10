using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements : MonoBehaviour {

	public float timeoutTime = 5.0f;
	public bool autoTimeout = true;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void Awake()
	{
		if (autoTimeout){
			StartCoroutine(TimeoutGUI());
		}
	}
	
	public void TimeoutAchievementGUI()
	{
		StartCoroutine(TimeoutGUI());
	}
	
	IEnumerator TimeoutGUI()
	{
		
		yield return new WaitForSeconds(timeoutTime);
		
		gameObject.SetActive(false);
		
	}
}
