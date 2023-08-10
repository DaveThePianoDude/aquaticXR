using UnityEngine;
using UnityEditor;
using System.Collections;


public class RotateArray : ScriptableWizard
{ 
	public int count = 8;
	

	[MenuItem ("Custom/Rotation Array")]


	static void CreateWizard ()
	{
	ScriptableWizard.DisplayWizard("Rotation Array", typeof(RotateArray), "RotateArray"); 
	}

	void OnWizardCreate ()
	{
		Undo.RegisterSceneUndo("Rotation Array");
		float rotY = 360.0f / ((float)count);
		GameObject useGo = Selection.activeGameObject;
		if (useGo == null)
            return;
 
		for(int i = 0; i < count ; i++)
		{
			GameObject newObject;
			
			newObject = (GameObject)Editor.Instantiate(useGo);
			newObject.name = useGo.name + i;
			//newObject.transform.position = useGo.transform.position;
			//newObject.transform.rotation = useGo.transform.rotation;
			Vector3 newRot = newObject.transform.rotation.eulerAngles;
			newRot.y += (rotY * ((float)i));
			newObject.transform.rotation = Quaternion.Euler(newRot);

		}

	}
}