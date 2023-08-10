using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixMissingSprites : ScriptableWizard
{
    public Sprite replaceSprite;

	public Sprite dropdownArrowSprite;
	
	public Sprite checkMarkSprite;
	
    [MenuItem("Custom/Fix Missing Sprites")]
    static void DoAddTexturesToTextureMapping()
    {
        ScriptableWizard.DisplayWizard("Replace Images With missing sprites ", typeof(FixMissingSprites), "Fix");
    }

    void OnWizardUpdate()
    {

    }

    void OnWizardCreate()
    {
        ReplaceMissingSprites();
    }
    
    void ReplaceMissingSprites ()
    {


        Image[] images = FindObjectsOfTypeAll(typeof(Image)) as Image[];

        foreach (Image img in images)
            {
                
				if (img.gameObject.name == "Arrow"){
					img.sprite = dropdownArrowSprite;
				} else if (img.gameObject.name == "Checkmark" || img.gameObject.name == "Item Checkmark"){
					img.sprite = checkMarkSprite;
				} else if (img.gameObject.name == "Item Background"){
					img.sprite = null;
				}
				else if (img.sprite == null)
                {
                    Debug.Log(img.gameObject.name.ToString() + " Is Missing Sprite");
                    img.sprite = replaceSprite;
                }
                
				
            }
        
    }
    
}