using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JeansTrack : MonoBehaviour {

	public GameObject[] jeans;
	public int duplicateCount = 10;
	public float offsetTime = 1.0f;
	public float randomTime = 0.25f;
	//public Animator anim;
	
	
	// Use this for initialization
	void Start () {
		//anim = jeans.GetComponent<Animator>();
		DuplicateJeans();
	}
	
	void DuplicateJeans()
	{
		
		for (int i=0;i<duplicateCount;i++){
			/*
			GameObject newGo = Instantiate(jeans, jeans.transform.position, jeans.transform.rotation);
			Animator animator = newGo.GetComponent<Animator>();
			//animator.SetTimeForCurrentClip(i + offsetTime);
			animator.StartPlayback();
			//animator.playbackTime = i + offsetTime;
			Debug.Log("PlaybackTime : " +  (i + offsetTime));
			animator.Play("Take 001", -1, (i + offsetTime));
			//animator.Play("Take 001", -1, normalizedTime);
			animator.speed = 1.0f;
			animator.StartPlayback();
			*/
			
			/*
			Animastion anim = newGo.GetComponent<Animation>();
			//anim.Stop ();
			anim["Take 001"].time = i + offsetTime;
			anim.Play("Take 001");
			*/
			
			StartCoroutine(NewJeans(i));
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	IEnumerator NewJeans(int i)
	{
		float t = (i * offsetTime) + (Random.Range(0-randomTime,randomTime));
		//Debug.Log("Wait For Seconds : " + t);
		if (t > 0){
			yield return new WaitForSeconds(t);
		}
		GameObject newGo = Instantiate(jeans[i], transform.position, transform.rotation);
	}
}
