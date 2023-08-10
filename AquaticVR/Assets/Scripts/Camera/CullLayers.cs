using UnityEngine;
using System.Collections;

public class CullLayers : MonoBehaviour {

	public bool cullOnStart = false;
	public Camera[] RenderingCameras;
	
	public string[] cullLayer;
	public float cullTimer = 2.0f;
	private bool waiting;
	// Use this for initialization
	void Start () {
		if (cullOnStart){
			CullingMasksOn();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void CullingMasksOn()
	{
		foreach (Camera cam in RenderingCameras){
			foreach(string cm in cullLayer){
				cam.cullingMask &= ~(1 << LayerMask.NameToLayer(cm));

			}
		}
		
	}
	
	public void CullingMasksOff()
	{
		foreach (Camera cam in RenderingCameras){
			cam.cullingMask = -1;

		}
		
	}
	
	public void CullForAMoment()
	{
		StartCoroutine (WaitAMoment ());
		
	}
	
	IEnumerator WaitAMoment() {
		waiting = true;
		CullingMasksOn();
		yield return new WaitForSeconds(cullTimer);	
		CullingMasksOff();
		waiting = false;
	}
}
