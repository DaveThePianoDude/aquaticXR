using UnityEngine;
using UnityEditor;

class MultiplyScale : ScriptableWizard {

	public float factor = 10;

	[MenuItem ("GameObject/Multiply Scale Of Selected Objects")]
	static void MultiplyScaleWizard() {
		ScriptableWizard.DisplayWizard(
			"Mutiply Selected Objects Scale",
			typeof(MultiplyScale),
			"Do IT!");
	}

	void OnWizardCreate() {
		GameObject[] gos = Selection.gameObjects;
		Debug.Log("Selected : " + (gos.Length));
		Debug.Log(gos);
		
		foreach (GameObject go in gos) {
			MultiplyScaleOfGameObject(go);
		}
		
	}
	
	void MultiplyScaleOfGameObject( GameObject go )
	{
		Vector3 curScale = go.transform.localScale;
		go.transform.localScale = new Vector3((curScale.x * factor),(curScale.y * factor),(curScale.z * factor));
		
		
	}
	
	
}
