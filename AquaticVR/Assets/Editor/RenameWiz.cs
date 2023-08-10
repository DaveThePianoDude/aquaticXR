using UnityEngine;
using UnityEditor;

class RenameWizard : ScriptableWizard {

	// Use this for initialization
/*
	public Renderer rend;
    void Start() {
        rend = GetComponent<Renderer>();
    }
    void OnDrawGizmosSelected() {
        Vector3 center = rend.bounds.center;
        float radius = rend.bounds.extents.magnitude;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, radius);
    }
	
	*/

	public string prefix = "";
	public string suffix = "";
	
	[MenuItem ("GameObject/RenameWiz")]
	static void RenameWiz() {
		ScriptableWizard.DisplayWizard(
			"Rename Selected Objects",
			typeof(RenameWizard),
			"Rename!");
	}

	void OnWizardCreate() {
		GameObject[] gos = Selection.gameObjects;
		Debug.Log("Selected : " + (gos.Length));
		Debug.Log(gos);
		
		foreach (GameObject go in gos) {
			if (prefix != ""){
				AddPrefix(go);
			}
			if (suffix != ""){
				AddSuffix(go);
			}
		}
		
	}
	
	void AddPrefix( GameObject go )
	{
		string goName = go.name;
		go.name = (prefix + goName);
		
	}

	void AddSuffix( GameObject go )
	{
		string goName = go.name;
		go.name = (goName + suffix);
		
	}
}
