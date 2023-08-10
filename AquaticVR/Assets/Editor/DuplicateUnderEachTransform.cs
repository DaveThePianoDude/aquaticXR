using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DuplicateUnderEachTransform  : ScriptableWizard
{
	static GameObject duplicateObj = null;

 
    public GameObject DuplicateObject = null;

 
    [MenuItem("GameObject/Duplicate Under Each Transform")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Duplicate Under Each Transform", typeof(DuplicateUnderEachTransform), "Duplicate");
    }
 
    public DuplicateUnderEachTransform()
    {
        DuplicateObject = duplicateObj;
        
    }
 
    void OnWizardUpdate()
    {
        duplicateObj = DuplicateObject;
        
    }
 
    void OnWizardCreate()
    {
        if (duplicateObj == null)
            return;
 
        Undo.RegisterSceneUndo("Duplicate Under Each Transform");
 
        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
 
        foreach (Transform t in transforms)
        {
            GameObject g;
            PrefabType pref = EditorUtility.GetPrefabType(duplicateObj);
 
            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)EditorUtility.InstantiatePrefab(duplicateObj);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(duplicateObj);
            }
 
            Transform gTransform = g.transform;
            gTransform.parent = t;
            g.name = duplicateObj.name;
            gTransform.localPosition = Vector3.zero;
            //gTransform.localScale = t.localScale;
            gTransform.localRotation = Quaternion.identity;
        }
 
        
    }
}
