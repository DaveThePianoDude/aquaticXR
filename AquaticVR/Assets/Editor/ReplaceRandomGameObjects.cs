/* This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 * Original coding by 'yesfish', nabbed from Unity Forums
 * 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
 */
using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class ReplaceRandomGameObjects : ScriptableWizard
{
    static GameObject[] replacements = null;
    static bool keep = false;
 
    public GameObject[] ReplacementObjects = null;
    public bool KeepOriginals = false;
 
    [MenuItem("GameObject/RandomReplacement")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Replace Random Game Objects", typeof(ReplaceRandomGameObjects), "Replace");
    }
 
    public ReplaceRandomGameObjects()
    {
        ReplacementObjects = replacements;
        KeepOriginals = keep;
    }
 
    void OnWizardUpdate()
    {
        replacements = ReplacementObjects;
        keep = KeepOriginals;
    }
 
    void OnWizardCreate()
    {
        if (replacements == null)
            return;
 
        Undo.RegisterSceneUndo("Replace Random Game Objects");
 
        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
 
        foreach (Transform t in transforms)
        {
			int tCount = replacements.Length;
			Debug.Log ("Random Selection From : " + tCount + " : Objects ");
			
			int randomI = (int) (Random.Range(0, (replacements.Length)));
			
			Debug.Log ("Selecting Index : " + randomI);
            GameObject g;
            PrefabType pref = EditorUtility.GetPrefabType(replacements[randomI]);
 
            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)EditorUtility.InstantiatePrefab(replacements[randomI]);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(replacements[randomI]);
            }
 
            Transform gTransform = g.transform;
            gTransform.parent = t.parent;
            g.name = replacements[randomI].name;
            gTransform.localPosition = t.localPosition;
            gTransform.localScale = t.localScale;
            gTransform.localRotation = t.localRotation;
        }
 
        if (!keep)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                GameObject.DestroyImmediate(g);
            }
        }
    }
}