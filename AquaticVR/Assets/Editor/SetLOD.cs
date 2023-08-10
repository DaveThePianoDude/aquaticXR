using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class SetLOD : ScriptableWizard
{
	
	public int level = 0;
    public float percent = 0.8f;
 
    [MenuItem("GameObject/SetLODs")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Set LODs On Selected [0 = level1] [1 = level2] [2 = level3]...", typeof(SetLOD), "Set");
    }

	public void SetLODsOfSelected()
	{
		
	}
 
    void OnWizardCreate()
    {

        Undo.RegisterSceneUndo("Set LODs On Selected");
 
        GameObject[] go = Selection.gameObjects;
 
        foreach (GameObject g in Selection.gameObjects)
        {

			LODGroup lodGroup = g.GetComponent<LODGroup>();
			
			if (lodGroup != null) {
				//Debug.Log(lodGroup.);
			 
				SerializedObject obj = new SerializedObject(lodGroup);
			 
				SerializedProperty valArrProp = obj.FindProperty("m_LODs.Array");
				for (int i = 0; valArrProp.arraySize > i; i++) {
					SerializedProperty sHeight = obj.FindProperty("m_LODs.Array.data[" + i.ToString() + "].screenRelativeHeight");
			 
					if (i == level) {
						sHeight.doubleValue = percent;
					}
			
				}
				obj.ApplyModifiedProperties();
			}
		
		}
    }
	
}