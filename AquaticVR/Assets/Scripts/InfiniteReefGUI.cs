using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfiniteReefGUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void StartGame()
	{
		int nextScene = (int)Mathf.Repeat((SceneManager.GetActiveScene().buildIndex + 1), (SceneManager.sceneCountInBuildSettings));
		SceneManager.LoadScene(nextScene);
	}
}
