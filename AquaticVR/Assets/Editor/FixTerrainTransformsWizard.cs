using UnityEngine;
using UnityEditor;

class FixTerrainTransforms : ScriptableWizard {

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
	//string tagName = "ExampleTag";

	[MenuItem ("GameObject/FixTerrainTransformsWizard")]
	static void FixTerrainTransformsWizard() {
		ScriptableWizard.DisplayWizard(
			"Select Terrain Top Level Transforms...",
			typeof(FixTerrainTransforms),
			"Do IT!");
	}

	void OnWizardCreate() {
		GameObject[] gos = Selection.gameObjects;
		Debug.Log("Selected : " + (gos.Length));
		Debug.Log(gos);
		
		foreach (GameObject go in gos) {
			TransformByBoundsCenter(go);
		}
		
	}
	
	void TransformByBoundsCenter( GameObject go )
	{
		//]Renderer rend = go.GetComponent<Renderer>();
		Bounds b = GetBounds(go);
		Vector3 center = b.center;
		Debug.Log((go.ToString()) + " : Center : " + (center.ToString()));
		Debug.Log((go.ToString()) + " : Size : " + (b.size.ToString()));
		Transform trans = go.GetComponent<Transform>();
		trans.position = center;
		
	}
	
	Bounds GetBounds(GameObject go){
        Bounds bounds;
        Renderer childRender;
        bounds = GetRenderBounds(go);
        if(bounds.extents.x == 0){
			// Do this first so we don't include the top transform which is empty
			Vector3 cc = Vector3.zero;
			
			/*
			foreach (Transform child in go.transform) {
				childRender = child.GetComponent<Renderer>();
				cc = childRender.bounds.center;
			}
			*/
			Renderer[] cRens = go.GetComponentsInChildren<Renderer>(false);
			cc = cRens[0].bounds.center;
			
            bounds = new Bounds(cc,Vector3.zero);
            foreach (Transform child in go.transform) {
                childRender = child.GetComponent<Renderer>();
                if (childRender) {
                    bounds.Encapsulate(childRender.bounds);
                }else{
                    bounds.Encapsulate(GetBounds(child.gameObject));
                }
            }
        }
        return bounds;
    }
	
	Bounds GetRenderBounds(GameObject go){
        Bounds bounds = new  Bounds(Vector3.zero,Vector3.zero);
        Renderer render = go.GetComponent<Renderer>();
        if(render!=null){
            return render.bounds;
        }
        return bounds;
    }
}
