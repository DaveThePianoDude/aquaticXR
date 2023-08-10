using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using AirSig;
using UnityEngine.Diagnostics;

public class GestureHandle : MonoBehaviour {

    // Reference to AirSigManager for setting operation mode and registering listener
    public AirSigManager airsigManager;

#if UNITY_ANDROID
#elif UNITY_STANDALONE_WIN
    // Reference to the vive right hand controller for handing key pressing
    public SteamVR_TrackedObject rightHandControl;
#endif
    public ParticleSystem track;

    // UI for displaying current status and operation results 
    public Text textMode;
    public Text textResult;
    string textToUpdate;
    public GameObject instruction;
    public GameObject cHeartDown;
    public GameObject heart;

    // Operations that can be performed using AirSig engine
    int currentModeIndex = 0;
    readonly AirSigManager.Mode[] availableMode = {
        // None: No operations will be performed even a gesture is received
        AirSigManager.Mode.None,

		// DeveloperDefined: Recevied gesture will be used to verify against developer predefined gestures
		AirSigManager.Mode.DeveloperDefined,

        // Train: Received gesture will be used to train as signature level accuracy
        AirSigManager.Mode.TrainPlayerSignature,

        // IdentifyUser: Received gesture will be verified against trained data in Mode.Train
        AirSigManager.Mode.IdentifyPlayerSignature,

        // AddCustomGesture: Received gesture will be used to train as gesture level accuracy
        // (Less strict and consistency of gesture will not be verified)
        AirSigManager.Mode.AddPlayerGesture,

        // IdentifyPlayerGesture: Recived gesture will be verified aganist trained data in
        // Mode.AddPlayerGesture
        AirSigManager.Mode.IdentifyPlayerGesture,

        // SmartTrainDeveloperDefined: Similar to the SmartTrain but for custom gesture profile
        AirSigManager.Mode.SmartTrainDeveloperDefined,

        // SmartIdentifyDeveloperDefined: Similar to the SmartIdentify but for custom gesture profile
        AirSigManager.Mode.SmartIdentifyDeveloperDefined,
       
    };

    readonly string DEFAULT_INSTRUCTION_TEXT = "Pressing touchpad and write in the air\nReleasing touchpad when finish";
    string defaultResultText;

    // Gesture index to use for training and verifying user gesture. Valid index is 100 only
    readonly int PLAYER_SIGNATURE_INDEX = 100;

    // Gesture index to use for training and verifying custom gesture. Valid range is between 1 and 1000
    // Beware that setting to 100 will overwrite your user signature.
    readonly int PLAYER_GESTURE_ONE = 101;
    readonly int PLAYER_GESTURE_TWO = 102;

    readonly int MAX_TRAIN_COUNT = 5;
    int smartTrainCount = 0;

    // Use these steps to iterate gesture when train 'Smart Train' and 'Custom Gesture'
	int currentPlayerGestureStep; // 101 = heart, 102 = down
    string currentSmartTrainDeveloperDefinedStep;

    // Set by the callback function to run this action in the next UI call
	Action nextUiAction;
    IEnumerator uiFeedback;

    // Callback for receiving signature/gesture progression or identification results
    AirSigManager.OnPlayerSignatureTrained signatureTrained;
    AirSigManager.OnPlayerSignatureMatch playerSignatureMatch;
    AirSigManager.OnPlayerGestureMatch playerGestureMatch;
    AirSigManager.OnPlayerGestureAdd playerGestureAdd;
    AirSigManager.OnDeveloperDefinedMatch developerDefined;
    AirSigManager.OnSmartIdentifyDeveloperDefinedMatch smartDeveloperDefined;

    void ResetPlayerGestureStep () {
		currentPlayerGestureStep = PLAYER_GESTURE_ONE;
	}

    void ResetSmartTrainDeveloperDefinedStep () {
        currentSmartTrainDeveloperDefinedStep = "HEART";
    }

	void ToggleGestureImage (string target)
	{
		if ("All".Equals (target)) {
			cHeartDown.SetActive (true);
            foreach (Transform child in cHeartDown.transform) {
                child.gameObject.SetActive(true);
            }
        } else if ("Heart".Equals (target)) {
			cHeartDown.SetActive (true);
			foreach (Transform child in cHeartDown.transform) {
				if (child.name == "Heart") {
					child.gameObject.SetActive (true);
				} else {
					child.gameObject.SetActive (false);
				}
			}
		} else if ("Down".Equals (target)) {
			cHeartDown.SetActive (true);
			foreach (Transform child in cHeartDown.transform) {
				if (child.name == "Down") {
					child.gameObject.SetActive (true);
				} else {
					child.gameObject.SetActive (false);
				}
			}
		} else {
			cHeartDown.SetActive(false);
		}
	}


    // Handling player signature match callback - This is invoked when the Mode is set to Mode.IdentifyPlayerSignature and a gesture is recorded.
    // gestureId - a serial number
    // match - true/false indicates that whether a gesture recorded match the gesture trained
    // targetIndex - one of the index in the SetTarget range.
    void HandleOnPlayerSignatureMatch(long gestureId, bool match, int targetIndex) {
        string result = "<color=red>Player signature failed to match</color>";
        if(PLAYER_SIGNATURE_INDEX == targetIndex && match) { 
            result = string.Format("<color=cyan>Player signature match ^_^</color>");
        }
        textToUpdate = result;
    }

    // Handling player signature training callback - This is invoked when the Mode is set to Mode.TrainPlayerSignature and a gesture is recorded.
    // gestureId - a serial number
    // error - error while training for this signature
    // progress - progress of training. 1 indicates the training is completed
    // securityLevel - the strength of this player sinature
    void HandleOnPlayerSignatureTrained(long gestureId, AirSigManager.Error error, float progress, AirSigManager.SecurityLevel securityLevel) {
        if(null == error) {
            if (progress < 1.0f) {
                textToUpdate = string.Format("Player signature training\nunder progress {0}%", Mathf.RoundToInt(progress * 100));
            }
            else {
                nextUiAction = () => {
                    StopCoroutine(uiFeedback);
                    textResult.text = defaultResultText = string.Format("<color=cyan>Player signature trained completed\nSwitch to IdentifyPlayerSignature to try it.</color>");
                };
            }
        }
        else {
            textToUpdate = string.Format("<color=red>This attempt of training failed\ndue to {0} (see error code document),\ntry again</color>", error.code);
        }
    }

    // Handling custom gesture match callback - This is inovked when the Mode is set to Mode.IdentifyPlayerGesture and a gesture
    // is recorded.
    // gestureId - a serial number
    // match - the index that match or -1 if no match. The match index must be one in the SetTarget()
    void HandleOnPlayerGestureMatch(long gestureId, int match) {
        if(gestureId == 0) {

        }
        else {
            string result = "<color=red>Cannot find closest custom gesture</color>";
            if (PLAYER_GESTURE_ONE == match) {
                result = string.Format("<color=#FF00FF>Closest Custom Gesture GESTURE #1</color>");
            } else if (PLAYER_GESTURE_TWO == match) {
                result = string.Format("<color=yellow>Closest Custom Gesture GESTURE #2</color>");
            }

            // Check whether this gesture match any custom gesture in the database
            AndroidJavaObject data = airsigManager.GetFromCache(gestureId);
            bool isExisted = airsigManager.IsPlayerGestureExisted(data);
            result += isExisted?string.Format("\n<color=green>There is a similar gesture in DB!</color>"):
                string.Format("\n<color=red>There is no similar gesture in DB!</color>");

            textToUpdate = result;
        }
    }

    // Handling custom gesture adding callback - This is invoked when the Mode is set to Mode.AddPlayerGesture and a gesture is
    // recorded. Gestures are only added to a cache. You should call SetCustomGesture() to actually set gestures to database.
    // gestureId - a serial number
    // result - return a map of all un-set custom gestures and number of gesture collected.
    void HandleOnPlayerGestureAdd (long gestureId, Dictionary<int, int> result)
	{
		int count = result [currentPlayerGestureStep];
		textToUpdate = string.Format ("{0}{1}/{2} gesture(s) collected for {3}\nContinue to collect more samples</color>",
            currentPlayerGestureStep == PLAYER_GESTURE_ONE ? "<color=#FF00FF>" : "<color=yellow>",
            count, MAX_TRAIN_COUNT,
            currentPlayerGestureStep == PLAYER_GESTURE_ONE ? "GESTURE #1" : "GESTURE #2");
		if (count >= MAX_TRAIN_COUNT) {
			currentPlayerGestureStep++;
            nextUiAction = () => {
                StopCoroutine(uiFeedback);
                textResult.text = defaultResultText = "Think of a gesture\nWrite it 5 times~\n<color=yellow>GESTURE #2</color>";
            };
        }

		if (currentPlayerGestureStep > PLAYER_GESTURE_TWO) {
			nextUiAction = () => {
				NextMode();
			};
		} else {
            airsigManager.SetTarget(new List<int> { currentPlayerGestureStep });
        }
    }

    // Handling developer defined gesture match callback - This is invoked when the Mode is set to Mode.DeveloperDefined and a gesture is recorded.
    // gestureId - a serial number
    // gesture - gesture matched or null if no match. Only guesture in SetDeveloperDefinedTarget range will be verified against
    // score - the confidence level of this identification. Above 1 is generally considered a match
    void HandleOnDeveloperDefinedMatch(long gestureId, string gesture, float score) {
        if (AirSigManager.Mode.DeveloperDefined == availableMode[currentModeIndex]) {
            textToUpdate = string.Format("<color=cyan>Gesture Match: {0} Score: {1}</color>", gesture.Trim(), score);
        } else if (AirSigManager.Mode.SmartTrainDeveloperDefined == availableMode[currentModeIndex]) {
            // Handle SmartTrain's progress result here
            if (currentSmartTrainDeveloperDefinedStep == gesture) {
                smartTrainCount++;
                string extraText = "Continue to add more samples";
                if (smartTrainCount >= MAX_TRAIN_COUNT) {
                    smartTrainCount = 0;
                    if ("HEART" == currentSmartTrainDeveloperDefinedStep) {
                        currentSmartTrainDeveloperDefinedStep = "C";
                        airsigManager.SetDeveloperDefinedTarget(new List<string> { "C" });
                        nextUiAction = () => {
                            textResult.text = defaultResultText = "Please write 'C' gesture 5 times\nPress touchpad to start\nRelease touchpad when finish";
                        };
                    } else if ("C" == currentSmartTrainDeveloperDefinedStep) {
                        currentSmartTrainDeveloperDefinedStep = "DOWN";
                        airsigManager.SetDeveloperDefinedTarget(new List<string> { "DOWN" });
                        nextUiAction = () => {
                            textResult.text = defaultResultText = "Please write 'DOWN' gesture 5 times\nPress touchpad to start\nRelease touchpad when finish";
                        };
                    } else {
                        nextUiAction = () => NextMode();
                    }
                }
                textToUpdate = string.Format(
                    "<color=cyan>Smart Train passed criteria ({0}/{1})\n" +
                    "{2}</color>", smartTrainCount, MAX_TRAIN_COUNT, extraText);
            } else {
                textToUpdate = string.Format(
                    "<color=red>Smart Train failed criteria\n" +
                    "Continue to add more samples</color>");
            }
        }
    }

    // Handling smart identify match callback for developer defined gesture profile - This is invoked when the Mode is set to
    // Mode.SmartIdentify and a gesture is recorded.
    // The result is combination of common gesture result and custom gesture result. Either one match will return a positive result.
    // gestureId - a serial number
    // gesture - the gesture that match or None. If match, then must be one of gesture in SetTarget()
    void HandleOnSmartDeveloperDefinedMatch(long gestureId, string gesture) {
        string result = string.Format("<color=cyan>Smart Identify Match '{0}'</color>", gesture);
        textToUpdate = result;
    }

    void NextMode ()
	{
        if (null != uiFeedback) {
            StopCoroutine(uiFeedback);
        }

        AirSigManager.Mode prevMode = availableMode[currentModeIndex];
        
		// Cycle through available gesture mode
		currentModeIndex++;
		if (currentModeIndex >= availableMode.Length) {
			currentModeIndex = 0;
		}
		airsigManager.SetMode (availableMode [currentModeIndex]);

        // Before actually change mode, set collected gesture as custom gesture if previous mode is AddPlayerGesture
        if (AirSigManager.Mode.AddPlayerGesture == prevMode) {
            airsigManager.SetPlayerGesture(new List<int> {
                PLAYER_GESTURE_ONE,
                PLAYER_GESTURE_TWO
            }, true);
        } else if (AirSigManager.Mode.SmartTrainDeveloperDefined == prevMode) {
            StartCoroutine(airsigManager.UpdateDeveloperDefinedGestureStat(true));
        }

        // Update the display text
        textMode.text = string.Format ("Mode: {0}", availableMode [currentModeIndex].ToString ());
		textResult.text = defaultResultText = DEFAULT_INSTRUCTION_TEXT;
		textResult.alignment = TextAnchor.UpperCenter;
		instruction.SetActive (false);
		ToggleGestureImage("");


        if(AirSigManager.Mode.DeveloperDefined == availableMode[currentModeIndex]) {
            airsigManager.SetDeveloperDefinedTarget(new List<string> { "HEART", "C", "DOWN" });
			airsigManager.SetClassifier("sample_gesture_profile", "");
        }

		else if (AirSigManager.Mode.TrainPlayerSignature == availableMode [currentModeIndex] ||
		         AirSigManager.Mode.IdentifyPlayerSignature == availableMode [currentModeIndex]) {
			// Set target index to a pre-defined gesture index. The same index must be used for both train and identify
			airsigManager.SetTarget (new List<int> { PLAYER_SIGNATURE_INDEX });
		}

        else if(AirSigManager.Mode.SmartTrainDeveloperDefined == availableMode[currentModeIndex]) {
            textResult.text = defaultResultText = "Please write 'HEART' gesture 5 times\nPress touchpad to start\nRelease touchpad when finish";

            smartTrainCount = 0;
			airsigManager.SetClassifier("sample_gesture_profile", "");
            airsigManager.SetDeveloperDefinedTarget(new List<string> { "HEART" });
            ResetSmartTrainDeveloperDefinedStep();
        }

        else if(AirSigManager.Mode.SmartIdentifyDeveloperDefined == availableMode[currentModeIndex]) {
            textResult.text = defaultResultText = "Try write 'Heart', 'C' and 'Down' gesture";
			airsigManager.SetClassifier("sample_gesture_profile", "");
            airsigManager.SetDeveloperDefinedTarget(new List<string> { "HEART", "C", "DOWN" });
        }

		else if (AirSigManager.Mode.AddPlayerGesture == availableMode [currentModeIndex]) {
            // Set target index to one of pre-define gesture index The same index must be used for both add and identify.
            // Notice that adding custom gesture only adds to the engine's cache and must call SetCustomGesture with index
            // in order to train this gesture before it can be used to identify.
            textResult.text = defaultResultText = "Think of a gesture\nWrite it 5 times~\n<color=#FF00FF>GESTURE #1</color>";
            airsigManager.SetTarget (new List<int> { PLAYER_GESTURE_ONE });
			ResetPlayerGestureStep();
		}

		else if (AirSigManager.Mode.IdentifyPlayerGesture == availableMode[currentModeIndex]) {
            textResult.text = defaultResultText = "Write gestures you just trained\nin AddPlayerGesture";
            airsigManager.SetTarget (new List<int> { PLAYER_GESTURE_ONE, PLAYER_GESTURE_TWO });
        }

		else if (AirSigManager.Mode.None == availableMode[currentModeIndex]) {
            setModeNone();
        }
    }

    private IEnumerator setResultTextForSeconds(string text, float seconds, string defaultText = "") {
        string temp = textResult.text;
        textResult.text = text;
        yield return new WaitForSeconds(seconds);
        textResult.text = defaultText;
    }

    private void setModeNone() {
        textMode.text = "Mode: None";
        textResult.text = "How to use:";
        instruction.SetActive(true);
		ToggleGestureImage("");
    }

    // Use this for initialization
    void Awake () {
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        // Registering callback events
        playerSignatureMatch = new AirSigManager.OnPlayerSignatureMatch(HandleOnPlayerSignatureMatch);
        signatureTrained = new AirSigManager.OnPlayerSignatureTrained(HandleOnPlayerSignatureTrained);
        playerGestureMatch = new AirSigManager.OnPlayerGestureMatch(HandleOnPlayerGestureMatch);
        playerGestureAdd = new AirSigManager.OnPlayerGestureAdd(HandleOnPlayerGestureAdd);
        developerDefined = new AirSigManager.OnDeveloperDefinedMatch(HandleOnDeveloperDefinedMatch);
        smartDeveloperDefined = new AirSigManager.OnSmartIdentifyDeveloperDefinedMatch(HandleOnSmartDeveloperDefinedMatch);
        
        airsigManager.onPlayerSignatureMatch += playerSignatureMatch;
        airsigManager.onPlayerSignatureTrained += signatureTrained;
        airsigManager.onPlayerGestureMatch += playerGestureMatch;
        airsigManager.onPlayerGestureAdd += playerGestureAdd;
        airsigManager.onDeveloperDefinedMatch += developerDefined;
        airsigManager.onSmartIdentifyDeveloperDefinedMatch += smartDeveloperDefined;

        airsigManager.SetMode(availableMode[currentModeIndex]);
        textMode.text = availableMode[currentModeIndex].ToString();
        setModeNone();
        StartCoroutine(IsDBExist());
		StartCoroutine(SetupKeys());
    }

    IEnumerator IsDBExist() {
        yield return new WaitForSeconds(2.0f);
        bool isDbExist = airsigManager.IsDbExist;
        if(! isDbExist) {
            textResult.text = "<color=red>Cannot find DB files!\nMake sure\n'Assets/AirSig/Plugins/Android/res'\nis copied to\n'Assets/Plugins/Android/res'</color>";
            textMode.text = "";
            instruction.SetActive (false);
            cHeartDown.SetActive(false);
            heart.SetActive(false);
        }
        yield return null;
    }

	IEnumerator SetupKeys() {
		yield return new WaitForSeconds(2.0f);
		airsigManager.TriggerStartButton = AirSigManager.TriggerButton.Touchpad;
		airsigManager.UseTouchTrigger = false;
		//airsigManager.TriggerEndButton = AirSigManager.TriggerButton.Touchpad;
	}

    void OnDestroy() {
        // Unregistering callback
        airsigManager.onPlayerSignatureMatch -= playerSignatureMatch;
        airsigManager.onPlayerSignatureTrained -= signatureTrained;
        airsigManager.onPlayerGestureMatch -= playerGestureMatch;
        airsigManager.onPlayerGestureAdd -= playerGestureAdd;
        airsigManager.onDeveloperDefinedMatch -= developerDefined;
        airsigManager.onSmartIdentifyDeveloperDefinedMatch -= smartDeveloperDefined;
    }

    void Update ()
	{
		if (null != textToUpdate) {
			uiFeedback = setResultTextForSeconds (textToUpdate, 1.5f, defaultResultText);
			StartCoroutine (uiFeedback);
			textToUpdate = null;
		}

		if (GvrControllerInput.AppButtonUp) {
			NextMode ();
		}

		if (GvrControllerInput.ClickButtonDown) {
			track.Play ();
		} else if (GvrController.ClickButtonUp) {
			track.Stop ();
		}

		if (nextUiAction != null) {
			nextUiAction ();
			nextUiAction = null;
		}
    }

    void FixedUpdate() {
    }
}
