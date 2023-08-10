using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using AirSig;

public class BasedGestureHandle : MonoBehaviour {

    // Reference to AirSigManager for setting operation mode and registering listener
    public AirSigManager airsigManager;

    public ParticleSystem track;

    // UI for displaying current status and operation results 
    public Text textMode;
    public Text textResult;
    public GameObject instruction;
    public GameObject cHeartDown;

    protected string textToUpdate;

    protected readonly string DEFAULT_INSTRUCTION_TEXT = "Pressing touchpad and write in the air\nReleasing touchpad when finish";
    protected string defaultResultText;

    // Set by the callback function to run this action in the next UI call
    protected Action nextUiAction;
    protected IEnumerator uiFeedback;

    protected string GetDefaultIntructionText() {
        return DEFAULT_INSTRUCTION_TEXT;
    }

    protected void ToggleGestureImage(string target) {
        if ("All".Equals(target)) {
            cHeartDown.SetActive(true);
            foreach (Transform child in cHeartDown.transform) {
                child.gameObject.SetActive(true);
            }
        } else if ("Heart".Equals(target)) {
            cHeartDown.SetActive(true);
            foreach (Transform child in cHeartDown.transform) {
                if (child.name == "Heart") {
                    child.gameObject.SetActive(true);
                } else {
                    child.gameObject.SetActive(false);
                }
            }
        } else if ("C".Equals(target)) {
            cHeartDown.SetActive(true);
            foreach (Transform child in cHeartDown.transform) {
                if (child.name == "C") {
                    child.gameObject.SetActive(true);
                } else {
                    child.gameObject.SetActive(false);
                }
            }
        } else if ("Down".Equals(target)) {
            cHeartDown.SetActive(true);
            foreach (Transform child in cHeartDown.transform) {
                if (child.name == "Down") {
                    child.gameObject.SetActive(true);
                } else {
                    child.gameObject.SetActive(false);
                }
            }
        } else {
            cHeartDown.SetActive(false);
        }
    }

    protected IEnumerator setResultTextForSeconds(string text, float seconds, string defaultText = "") {
        string temp = textResult.text;
        textResult.text = text;
        yield return new WaitForSeconds(seconds);
        textResult.text = defaultText;
    }

	protected IEnumerator IsDBExist() {
		yield return new WaitForSeconds(2.0f);
		bool isDbExist = airsigManager.IsDbExist;
		if(! isDbExist) {
			textResult.text = "<color=red>Cannot find DB files!\nMake sure\n'Assets/AirSig/Plugins/Android/res'\nis copied to\n'Assets/Plugins/Android/res'</color>";
			textMode.text = "";
			instruction.SetActive (false);
			cHeartDown.SetActive(false);
		}
		yield return null;
	}

	protected IEnumerator SetupKeys() {
		yield return new WaitForSeconds(2.0f);
		airsigManager.TriggerStartButton = AirSigManager.TriggerButton.Touchpad;
		airsigManager.UseTouchTrigger = false;
		//airsigManager.TriggerEndButton = AirSigManager.TriggerButton.Touchpad;
	}

    protected void UpdateUIandHandleControl() {

        if (null != textToUpdate) {
			if(null != uiFeedback) {
				StopCoroutine(uiFeedback);
			}
            uiFeedback = setResultTextForSeconds(textToUpdate, 5.0f, defaultResultText);
            StartCoroutine(uiFeedback);
            textToUpdate = null;
        }

		if (GvrControllerInput.ClickButtonDown) {
			track.Clear();
			track.Play();
		} else if (GvrController.TouchUp) {
			track.Stop ();
		}

        if (nextUiAction != null) {
            nextUiAction();
            nextUiAction = null;
        }
    }

}
