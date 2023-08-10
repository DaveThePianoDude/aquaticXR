using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterColorFilterSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using DaydreamElements.Tunneling;

public class InfinitReefManager : MonoBehaviour {

	public enum LoopType
    {
        None,
        PingPong,
        Repeat

    }
	
	public enum ControlMode
	{
		Normal,
		Snorkeling
	}

    public enum MoveType {
        Time,
        Speed
    }
	
	public bool autoStart = true;
	public ControlMode controlMode;
	public int qualityLevel = 0;
	private int minQualityLevel = 4;
	public bool doTranslate;
	public bool waiting = true;
	public GameObject[] reefPrefabs;
	public GameObject initReef;
	public float reefSize = 100.0f;
	public float playerSpeed = 1.0f;
	public GameObject playerTransform ;
	public GameObject playerDepthTransform;
	
	public GameObject playerCamera;
	private Rigidbody playerRigidbody;
	public bool watercolorEffectEnabled = true;
	public bool watercolorTunnelEnabled = true;
	public GameObject watercolorTunnel;
	public bool watercolorDimensionEnabled = true;
	public GameObject watercolorDimension;
	public GameObject[] envParticles;
	public Light sunlight;
	public GameObject causticsLight;
	private int[] envParticleMax;
	private float curPlayerSpeed = 1.0f;
	public float minPlayerSpeed = 0.1f;
	public float maxPlayerSpeed = 20.0f;	
	public float speedChange = 1.0f;
	
	public float depthSpeed = 1.0f;
	public float minDepth = 0.0f;
	public float maxDepth = 10.0f;
	public float initDepth = 15.0f;	
	public float curDepth;
	
	public float maxX = 5.0f;
	public float curX = 0.0f;
	
	//public float translateSpeed = 1.0f;
	public LoopType loopType;
	public MoveType moveType;
	
	private Vector3 startPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 endPosition = new Vector3(0.0f, 0.0f, 100.0f);
	private float spawnReefLoc = 0.0f;
	private GameObject nextReef;
	private GameObject lastReef;
	private GameObject destroyReef;
	
	private int reefNum = 0;
	private int lastReefNum = 0;
	private int destroyReefNum = 0;
	
	public int maxReefCount = 2;
	private int reefCount = 1;
	
	public GameObject fishtankTransform;
	public GameObject fishtankReef;
	public GameObject[] fishtankReefPrefabs;
	public bool moveFishtankReef = false;
	public float fishtankReefSize = 100.0f;
	
	private float spawnFishtankReefLoc = 0.0f;
	
	
	private int fishtankReefCount = 1;
	
	private GameObject nextFishtankReef;
	private GameObject lastFishtankReef;
	private GameObject destroyFishtankReef;
	
	public List<GameObject> fishtankReefs = new List<GameObject>();	
	
	public AudioClip confirmationSound;
	public AudioClip[] notHappySound;
	
	//private AudioSource audioSource;
	private AudioSource audioSource;
	
	public GameObject mainMenu;
	private bool menuOpen = false;
	public GameObject startMenu;
	public float startMenuOpenTime = 8.0f;
	
	private PostProcessingBehaviour postProcessingBehaviour;
	
	private Vector2 lastTouchPos = new Vector2(0.0f,0.0f);
	
	public GameObject pickupGUI;
	private bool pickupMenuEnabled = false;
	
	public AudioClip[] pickupSoundClip;
	private int coinCount;
	private int trashCount;
	public Text coinCountText;
	public Text trashCountText;
	public float pickupMenuTimeout = 3.0f;
	private bool isTimingOutPickupGUI = false;
	private bool resetPickupTimer = false;
	
	public GameObject achievementGui;
	public AudioClip[] achievementSound;
	public int enviroAchievementInc = 5;
	private int enviroAchievementIndex = 0;
	public GameObject[] enviroAchievement;
	public int moneyAchievementInc = 10;
	public GameObject[] moneyAchievement;
	private int moneyAchievementIndex = 0;
	
	private bool holdingObject = false;
	
	public bool useTunneling = false;
	
	[SerializeField]
    private BaseVignetteController vignetteController;
	
	[Tooltip("Controls how far the user must touch on the touchpad to be moving at min speed.")]
    [Range(0.0f, 1.0f)]
    public float minInputThreshold = 0.3f;
	
	// Use this for initialization
    void Start () {
		//spawnReefLoc = 0 - (reefSize * 0.5f);
        if (autoStart)
        {
			if (initReef){
				nextReef = initReef;
			}
			
			if (fishtankReef){
				nextFishtankReef = fishtankReef;
			}
			//SpawnNextReef();
			/*
			//SpawnNextReef();
            if (doTranslate)
            {
                StartCoroutine(Translation(transform, startPosition, endPosition, playerSpeed, moveType));
            }
            */
        }
		
		StartCoroutine(TimeActivateGameObject(startMenu, startMenuOpenTime));
		
		// Set the initial player transforms
		initDepth = playerDepthTransform.transform.localPosition.y;
		curDepth = playerDepthTransform.transform.localPosition.y;
		//curDepth = initDepth;
		curPlayerSpeed = playerSpeed;
		playerTransform.transform.rotation = Quaternion.identity;
		
		// Get neccessary components
		audioSource = GetComponent<AudioSource>();
		postProcessingBehaviour = playerCamera.GetComponent<PostProcessingBehaviour>();
		//playerRigidbody = playerDepthTransform.GetComponent<Rigidbody>();
		
		// Deactivate gui elements
		pickupGUI.SetActive(false);
		achievementGui.SetActive(false);
		mainMenu.SetActive(false);
		menuOpen = false;
		
		watercolorTunnel.SetActive(watercolorTunnelEnabled);
		watercolorDimension.SetActive(watercolorDimensionEnabled);
		
		// Set particle counts
		envParticleMax = new int[envParticles.Length];
		for(int i =0;i< envParticles.Length; i++){
			ParticleSystem partSys = envParticles[i].GetComponent<ParticleSystem>();
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			envParticleMax[i] = maxParticles;
		}
		
		if (useTunneling){
			if (vignetteController){
				vignetteController.ShowVignette();
			}
		}
		
		if (moveFishtankReef ){
			spawnFishtankReefLoc = fishtankReefSize;
			fishtankReefs.Add(fishtankReef);
			SpawnNextFishtankReef();
			if (!fishtankTransform){
				Debug.Log("Cannot Move Fishtank Reef, You must assign a fishtankTransform!");
				moveFishtankReef = false;
			}
		}
	}
	// Update is called once per frame
	void Update ()
	{
		if (!waiting){
			transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed);
		}
		
		if (moveFishtankReef){
			fishtankTransform.transform.Translate(Vector3.left * Time.deltaTime * playerSpeed);
			
			if (fishtankTransform.transform.position.x <= ((0 - (spawnFishtankReefLoc)) * 1.0f)){
				SpawnNextFishtankReef();
				//if (fishtankReefCount > maxReefCount){
				//	DestroyFishtankReef();
				//}
			}
		
		}
		
	
		if (transform.position.z >= spawnReefLoc){
			SpawnNextReef();
			if (reefCount > maxReefCount){
				DestroyReef();
			}
		}
		
		// Get Daydream Controller Input
		if (GvrControllerInput.AppButtonUp){
			ToggleMainMenu();
		}
		/*
		if (GvrControllerInput.ClickButton){
			GVRChangeDepth();
		}
		*/
		
		if (GvrControllerInput.IsTouching){
			/*
			if (!GvrControllerInput.ClickButton){
				GVRChangeDepth();
			}*/
			/*
			if (!holdingObject){
				GVRChangeDepth();
			}
			*/
			GVRChangeDepth();
			//GVRMovePlayer();
		}
		/*
		if (Input.GetAxis("Vertical") > 0.01)
		{
			ChangeDepth();	
		}
		*/
		
		
		// UNITY EDITOR TESTING
		#if UNITY_EDITOR
		// Get Keyboard Input For Testing
		if (Input.GetKey("up") || Input.GetKey("down") ){
			KeyMove();
		}
		
		if (Input.GetKey("left") || Input.GetKey("right")   ){
			KeyMove();
		}
			
				
		if (Input.GetKey("left ctrl")){
			IncreasePlayerSpeed();
			//Debug.Log("ChangeDepth");
		}	
		
		if (Input.GetKey("right ctrl")){
			DecreasePlayerSpeed();
			//Debug.Log("ChangeDepth");
		}	
		
		if (Input.GetKeyUp("escape")){
			Reset();
			//Debug.Log("ChangeDepth");
		}	
		
		if (Input.GetKeyUp("tab")){
			if (waiting){	
				waiting = false;
				startMenu.SetActive(false);
				//StartSwimming();
			}
			StartSwimming();
			
		}	
		
		if (Input.GetKeyUp("tab")){
			//ToggleWatercolorEffect();
			//Debug.Log("ChangeDepth");
			//coinCount = moneyAchievementInc;
			
			//DisplayAcheivement();
			ToggleMainMenu();
			
			
		}	
		
		if (Input.GetKeyUp(KeyCode.PageUp)){
			IncreaseQualityLevel();
			Debug.Log("Increasing Quality Level");
		}	
		if (Input.GetKeyUp(KeyCode.PageDown)){
			DecreaseQualityLevel();
			Debug.Log("Decreasing Quality Level");
		}	
		
		#endif  // !UNITY_EDITOR
		
	}
	
	/*
	void FixedUpdate()
	{
		// UNITY EDITOR TESTING
		#if UNITY_EDITOR
		// Get Keyboard Input For Testing
		if (Input.GetKey("up") || Input.GetKey("down") ){
			KeyMove();
		}
		
		if (Input.GetKey("left") || Input.GetKey("right")   ){
			KeyMove();
		}
				
		
		#endif  // !UNITY_EDITOR
	}
	*/
	
	public void ChangeControlMode()
	{
		if (controlMode == ControlMode.Normal){
			controlMode = ControlMode.Snorkeling;
		} else {
			controlMode = ControlMode.Normal;
		}
		PlayConfirmationSound();
	}
	
	public void StartSwimming()
	{
		PlayConfirmationSound();
		waiting = false;
		startMenu.SetActive(false);
		
	}
	
	public void PlayConfirmationSound()
	{
		PlayAudioClip("confirmation");
		
	}
	
	public void PlayPickupSound()
	{
		PlayAudioClip("pickup");
	}
	
	public void PlayAudioClip( string clipType )
	{
		if (!audioSource){ return ;}
		if (clipType == "confirmation"){
			audioSource.clip = confirmationSound;
		} else if (clipType == "pickup"){
			int ranClip = Mathf.FloorToInt(Random.Range(0, pickupSoundClip.Length));	
			audioSource.clip = pickupSoundClip[ranClip];
		} else if (clipType == "achievement"){
			int ranClip = Mathf.FloorToInt(Random.Range(0, achievementSound.Length));	
			audioSource.clip = achievementSound[ranClip];
		} else if (clipType == "notHappy"){
			int ranClip = Mathf.FloorToInt(Random.Range(0, notHappySound.Length));	
			audioSource.clip = notHappySound[ranClip];
		}
				
		audioSource.Play();
		
	}
	
	public void HoldingObject()
	{
		holdingObject = true;
	}
	
	public void NotHoldingObject()
	{
		holdingObject = false;
	}
	
	public void ChangeDepth()
	{
		
		curDepth+= depthSpeed;
		float newDepth = Mathf.PingPong(curDepth, maxDepth);
		
		Vector3 newPos = new Vector3(transform.position.x, (0 - newDepth), transform.position.z);
		transform.position = newPos;
		
	}
	
	public void SwimUp()
	{
		curDepth = Mathf.Clamp((curDepth - depthSpeed), minDepth, maxDepth);
		Vector3 newPos = new Vector3(transform.position.x, (initDepth - curDepth), transform.position.z);
		playerDepthTransform.transform.position = newPos;
		/*
		curDepth+= depthSpeed;
		float newDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		
		Vector3 newPos = new Vector3(transform.position.x, (0 - newDepth), transform.position.z);
		transform.position = newPos;
		*/
		PlayConfirmationSound();
		
	}
	
	public void GVRChangeDepth()
	{
		Vector2 touchPos = GvrController.TouchPos;
		
		if (useTunneling){
			Vector2 touchPosCen = GvrControllerInput.TouchPosCentered;
			bool isTouchTranslating = IsTouchTranslating(touchPosCen);
			bool isTouchRotating = IsTouchRotating(touchPosCen);
			UpdateVignetteFOV(isTouchTranslating, isTouchRotating);
		}
		
		float moveDis = Vector2.Distance(lastTouchPos, touchPos);
		if (moveDis < 0.05){
			lastTouchPos = touchPos;
			//return ;
		}
		
		float yPos = -1.0f + (touchPos.y * 2.0f);
		if (holdingObject){
			yPos = 0.0f;
		}
		float wantedDepth = curDepth + yPos;		
		curDepth = Mathf.Lerp(curDepth, wantedDepth, (depthSpeed * Time.deltaTime));
		curDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);		
		
		float xPos = -1.0f + (touchPos.x * 2.0f);
		float wantedX = curX - xPos;
		curX = Mathf.Lerp(curX, wantedX, (depthSpeed * Time.deltaTime));
		curX = Mathf.Clamp(curX, (0.0f - maxX), maxX);
		
		
		Vector3 newPos = new Vector3((0.0f - curX), (0.0f - curDepth), 0.0f);
		
		if (controlMode == ControlMode.Snorkeling){
			newPos.y = initDepth;
		}
		
		playerDepthTransform.transform.localPosition = newPos;
		
		/*
		curDepth+= depthSpeed;
		float newDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		
		Vector3 newPos = new Vector3(transform.position.x, (0 - newDepth), transform.position.z);
		transform.position = newPos;
		*/
		//PlayConfirmationSound();
		lastTouchPos = touchPos;
	}
	
	private void UpdateVignetteFOV(bool isTranslating, bool isRotating) {
      if (isRotating) {
        vignetteController.SetFieldOfViewForRotation();
      } else if (isTranslating) {
        vignetteController.SetFieldOfViewForTranslation();
      }
    }
	
	private bool IsTouchTranslating(Vector2 touchPos) {
      return Mathf.Abs(touchPos.y) > minInputThreshold;
    }

    private bool IsTouchRotating(Vector2 touchPos) {
      return Mathf.Abs(touchPos.x) > minInputThreshold;
    }
	
	public void GVRMovePlayer()
	{
		Vector2 touchPos = GvrController.TouchPos;
		float moveDis = Vector2.Distance(lastTouchPos, touchPos);
		if (moveDis < 0.05){
			lastTouchPos = touchPos;
			//return ;
		}
		
		float yPos = -1.0f + (touchPos.y * 2.0f);
		if (holdingObject){
			yPos = 0.0f;
		}
		
		float wantedDepth = curDepth + yPos;		
		curDepth = Mathf.Lerp(curDepth, wantedDepth, (depthSpeed * Time.deltaTime));
		curDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);		
		
		float xPos = -1.0f + (touchPos.x * 2.0f);
		float wantedX = curX - xPos;
		curX = Mathf.Lerp(curX, wantedX, (depthSpeed * Time.deltaTime));
		curX = Mathf.Clamp(curX, (0.0f - maxX), maxX);
		
		/*
		// Rigidbody Movement
		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = playerCamera.transform.forward*yPos + playerCamera.transform.right*xPos;
		
		desiredMove.x = desiredMove.x*depthSpeed;
		//desiredMove.z = desiredMove.z*depthSpeed;
		desiredMove.z = 0.0f;
		desiredMove.y = desiredMove.y*depthSpeed;
		
		if (controlMode == ControlMode.Snorkeling){
			desiredMove.y = 0;
		}	
		
		playerRigidbody.AddForce(desiredMove, ForceMode.Impulse);
		*/
		
		
		
		Vector3 newPos = new Vector3((0.0f - curX), (initDepth - curDepth), transform.position.z);
		playerDepthTransform.transform.position = newPos;
		
		
		/*
		curDepth+= depthSpeed;
		float newDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		
		Vector3 newPos = new Vector3(transform.position.x, (0 - newDepth), transform.position.z);
		transform.position = newPos;
		*/
		//PlayConfirmationSound();
		
		lastTouchPos = touchPos;
		
	}
	
	public void KeyChangeDepth()
	{
		float touchPos = 0.0f;
		if (Input.GetKey("up")){
			touchPos = 1.0f;
		}
		if (Input.GetKey("down")){
			touchPos = 0.0f;
		}
		
		float sidePos = 0.0f;
		if (Input.GetKey("right")){
			sidePos = 1.0f;
		}
		if (Input.GetKey("left")){
			sidePos = 0.0f;
		}
		
		float yPos = -1.0f + (touchPos * 2);
		float wantedDepth = curDepth + yPos;
		curDepth = Mathf.Lerp(curDepth, wantedDepth, (depthSpeed * Time.deltaTime));
		curDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		
		float xPos = -1.0f + (sidePos * 2);
		float wantedX = curX + xPos;
		curX = Mathf.Lerp(curX, wantedX, (depthSpeed * Time.deltaTime));
		curX = Mathf.Clamp(curX, (0 - maxX), maxX);
		
		Vector3 newPos = new Vector3((0.0f - curX), (initDepth - curDepth), transform.position.z);
		playerDepthTransform.transform.position = newPos;
		
	}
	
	public void KeyMove()
	{
		float touchPos = 0.0f;
		if (Input.GetKey("up")){
			touchPos = 1.0f;
		}
		if (Input.GetKey("down")){
			touchPos = -1.0f;
		}
		
		float sidePos = 0.0f;
		if (Input.GetKey("right")){
			sidePos = 1.0f;
		}
		if (Input.GetKey("left")){
			sidePos = -1.0f;
		}
		
		float wantedDepth = touchPos;
		curDepth = Mathf.Lerp(curDepth, wantedDepth, (depthSpeed * Time.deltaTime));
		curDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		
		float xPos = sidePos;
		float wantedX = curX + xPos;
		curX = Mathf.Lerp(curX, wantedX, (depthSpeed * Time.deltaTime));
		curX = Mathf.Clamp(curX, (0.0f - maxX), maxX);
		
		// Rigidbody Movement
		//Vector3 desiredMove = playerCamera.transform.forward*wantedDepth + playerCamera.transform.right*xPos;
		
		//desiredMove.x = desiredMove.x*1.0f;
		//desiredMove.z = desiredMove.z*1.0f;
		//desiredMove.z = 0.0f;
		//desiredMove.y = desiredMove.y*1.0f;
		
		//playerRigidbody.AddForce(desiredMove, ForceMode.Impulse);
		//Vector3 newPos = new Vector3(curX, (initDepth - curDepth), playerRigidbody.transform.position.z);
		//playerRigidbody.MovePosition(newPos);
		
		/*
		// Rigidbody Movement
		Vector3 newPos = new Vector3(playerRigidbody.transform.position.x + (sidePos * 0.025f), playerRigidbody.transform.position.y + (touchPos * 0.025f), playerRigidbody.transform.position.z);
		playerRigidbody.MovePosition(newPos);
		*/
		
		// Simple Movement
		Vector3 newPos = new Vector3(curX, (0.0f - curDepth), 0.0f);
		//playerDepthTransform.transform.position = newPos;
		playerDepthTransform.transform.localPosition = newPos;
		
	}
	
	public void SwimDown()
	{
		curDepth = Mathf.Clamp((curDepth + depthSpeed), minDepth, maxDepth);
		Vector3 newPos = new Vector3(transform.position.x, (initDepth - curDepth), transform.position.z);
		playerDepthTransform.transform.position = newPos;
		
		/*
		curDepth-= depthSpeed;
		float newDepth = Mathf.Clamp(curDepth, minDepth, maxDepth);
		Vector3 newPos = new Vector3(transform.position.x, (0 - newDepth), transform.position.z);
		transform.position = newPos;
		*/
		
		PlayConfirmationSound();
		
	}
	
	public void ChangeSpeed()
	{
		
		curPlayerSpeed+= speedChange;
		float newSpeed = Mathf.PingPong(curPlayerSpeed, maxPlayerSpeed);
		
		playerSpeed = newSpeed;
		
		
		PlayConfirmationSound();
	}
	
	public void IncreasePlayerSpeed()
	{
		playerSpeed = Mathf.Clamp((playerSpeed + speedChange), 0.0f, maxPlayerSpeed);
		/*
		curPlayerSpeed+= speedChange;
		float newSpeed = Mathf.Clamp(curPlayerSpeed, 0.0f, maxPlayerSpeed);
		
		playerSpeed = newSpeed;
		*/
		
		PlayConfirmationSound();
	}
	
	public void DecreasePlayerSpeed()
	{
		playerSpeed = Mathf.Clamp((playerSpeed - speedChange), 0.0f, maxPlayerSpeed);
		/*
		curPlayerSpeed-= speedChange;
		float newSpeed = Mathf.Clamp(curPlayerSpeed, 0.0f, maxPlayerSpeed);
		
		playerSpeed = newSpeed;
		*/
		
		PlayConfirmationSound();
	}
	
	
	public void ToggleWatercolorEffect()
	{
		WaterColorFilter wcf = playerCamera.GetComponent<WaterColorFilter>();
		if (watercolorEffectEnabled){
			wcf.enabled = false;
			watercolorEffectEnabled = false;
		} else {
			wcf.enabled = true;
			watercolorEffectEnabled = true;
		}
		PlayConfirmationSound();
		//wcf.enabled = !wcf.enabled;
	}
	
	public IEnumerator TranslateTo(Transform thisTransform, Vector3 endPos, float value, MoveType mt)
    {
        yield return Translation(thisTransform, thisTransform.position, endPos, value, mt);
    }
	
	public IEnumerator Translation(Transform thisTransform, Vector3 endPos, float value, MoveType mt)
    {
        yield return Translation(thisTransform, thisTransform.position, thisTransform.position + endPos, value, mt);
    }
	
	public IEnumerator Translation(Transform thisTransform, Vector3 startPos, Vector3 endPos, float value, MoveType mt)
    {
		//value = playerSpeed;
        float rate = (mt == MoveType.Time) ? 1.0f / playerSpeed : 1.0f / Vector3.Distance(startPos, endPos) * playerSpeed;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += Time.deltaTime * rate;
            thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }
        if (loopType == LoopType.PingPong)
        {
            StartCoroutine(Translation(transform, endPos, startPos, playerSpeed, mt));
        } else if (loopType == LoopType.Repeat)
        {
            StartCoroutine(Translation(transform, startPos, endPos, playerSpeed, mt));
        }
		
		if (thisTransform.position.z >= spawnReefLoc){
			SpawnNextReef();
		}
    }
	
	/*
	public IEnumerator MoveReef()
	{
		transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed);
	}
	*/
	
	public void IncreaseQualityLevel()
	{
		qualityLevel--;
		if (qualityLevel < 0){
			qualityLevel = 0;
		}
		
		if (nextReef){
			nextReef.SendMessage("IncreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		if (lastReef){
			lastReef.SendMessage("IncreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		
		for(int i =0;i< envParticles.Length; i++){
			ParticleSystem partSys = envParticles[i].GetComponent<ParticleSystem>();
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			maxParticles = Mathf.FloorToInt(envParticleMax[i] / (qualityLevel + 1 ));		
			mainModule.maxParticles = maxParticles;
			
		}
		
		SetLightShadowType();
		PlayConfirmationSound();
	}
	
	public void DecreaseQualityLevel()
	{
		qualityLevel++;
		if (qualityLevel > minQualityLevel){
			qualityLevel = minQualityLevel;
		}
		
		if (nextReef){
			nextReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		if (lastReef){
			lastReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		if(destroyReef){
			destroyReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		
		for(int i =0;i< envParticles.Length; i++){
			ParticleSystem partSys = envParticles[i].GetComponent<ParticleSystem>();
			var mainModule = partSys.main;
			int maxParticles = mainModule.maxParticles;
			maxParticles = Mathf.FloorToInt(envParticleMax[i] / (qualityLevel + 1 ));		
			mainModule.maxParticles = maxParticles;
			
		}
		
		SetLightShadowType();
		PlayConfirmationSound();
	}
	
	public void SetPostProcessingBehavior()
	{
		if (qualityLevel == 1){
			postProcessingBehaviour.enabled = true;
		} else {
			postProcessingBehaviour.enabled = false;
		}
	}
	
	public void TogglePostProcessing()
	{
		
		if (postProcessingBehaviour.enabled){
			postProcessingBehaviour.enabled = false;
			
		} else {
			postProcessingBehaviour.enabled = true;
		}
		PlayAudioClip("confirmation");
		
	}
	
	public void ToggleWatercolorTunnel()
	{
		
		if (watercolorTunnelEnabled){
			watercolorTunnelEnabled = false;
			watercolorTunnel.SetActive(false);
			
		} else {
			watercolorTunnelEnabled = true;
			watercolorTunnel.SetActive(true);
		}
		PlayAudioClip("confirmation");
		
	}
	
	public void ToggleWatercolorDimension()
	{
		
		if (watercolorDimensionEnabled){
			watercolorDimensionEnabled = false;
			watercolorDimension.SetActive(false);
			
		} else {
			watercolorDimensionEnabled = true;
			watercolorDimension.SetActive(true);
		}
		PlayAudioClip("confirmation");
		
	}
	
	public void DisplayAcheivement()
	{
		
		if (coinCount >= moneyAchievementInc){
			EnableAchievementGUI();
		
			moneyAchievement[moneyAchievementIndex].SetActive(true);
			//moneyAchievement[moneyAchievementIndex].SendMessage("Timeout", SendMessageOptions.DontRequireReceiver);
			StartCoroutine(TimeoutGameObject(moneyAchievement[moneyAchievementIndex], 5));	
			moneyAchievementIndex++;
			moneyAchievementInc = moneyAchievementInc * 2;
			
		}
		
		if (trashCount >= enviroAchievementInc){
			EnableAchievementGUI();
			
			enviroAchievement[enviroAchievementIndex].SetActive(true);
			//enviroAchievement[enviroAchievementIndex].SendMessage("Timeout", SendMessageOptions.DontRequireReceiver);
			StartCoroutine(TimeoutGameObject(enviroAchievement[enviroAchievementIndex], 5));	
			enviroAchievementIndex++;
			enviroAchievementInc = enviroAchievementInc * 2;
			
		}
	}
	
	private void EnableAchievementGUI()
	{
		achievementGui.SetActive(true);
		//achievementGui.BroadcastMessage("TimeoutAchievementGUI", SendMessageOptions.DontRequireReceiver);
		StartCoroutine(TimeoutGameObject(achievementGui, 6));	
		PlayAudioClip("achievement");
	}
	
	public void SetLightShadowType()
	{
		if (!sunlight){ return ;}
	
		if (qualityLevel < 2){
			if (causticsLight){
				AnimatedCaustics ac = causticsLight.GetComponent<AnimatedCaustics>();
				ac.enabled = true;
			}
		}
			
		if (qualityLevel == 0){
			sunlight.shadows = LightShadows.Soft;
			
		}
		if (qualityLevel == 1){
			sunlight.shadows = LightShadows.Hard;
			
		}
		if (qualityLevel >= 2){
			sunlight.shadows = LightShadows.None;
			if (causticsLight){
				AnimatedCaustics ac = causticsLight.GetComponent<AnimatedCaustics>();
				ac.enabled = false;
			}
		}
		
		
	}
	
	public void SpawnNextReef()
	{
		if (lastReef){
			destroyReef = lastReef;
		}
		if (nextReef){
			lastReef = nextReef;
		}
		
		
		int ranReef = Mathf.FloorToInt(Random.Range(0, reefPrefabs.Length));		
		Vector3 newReefPos = new Vector3(0.0f, 0.0f, (lastReef.transform.position.z + reefSize));
		nextReef = (GameObject)Instantiate(reefPrefabs[ranReef], newReefPos, Quaternion.identity);
		nextReef.SendMessage("InitReef", SendMessageOptions.DontRequireReceiver);
				
		if (qualityLevel > 0){
			nextReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
				
		// For loading Reef Scenes - NOT WORKING
		/*
		reefNum = Mathf.FloorToInt(Random.Range(1, SceneManager.sceneCountInBuildSettings));
		SceneManager.LoadScene(reefNum, LoadSceneMode.Additive);
		Scene nextReefScene = SceneManager.GetSceneAt(reefNum);
		
		GameObject[] rootGo = nextReefScene.GetRootGameObjects();
		nextReef = rootGo[0];
		
		Vector3 newReefPos = new Vector3(0.0f, 0.0f, (transform.position.z + reefSize));
		nextReef.transform.position = newReefPos;
		
		nextReef.SendMessage("InitReef", SendMessageOptions.DontRequireReceiver);
		
		if (qualityLevel > 0){
			nextReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		*/
		
		spawnReefLoc += reefSize;
		Debug.Log("New Reef Spawned");
		reefCount++;
		
	}
	
	public void SpawnNextFishtankReef()
	{
		/*
		if (lastFishtankReef){
			destroyFishtankReef = lastFishtankReef;
		}
		if (nextFishtankReef){
			lastFishtankReef = nextFishtankReef;
		}
		*/
		
		int ranReef = Mathf.FloorToInt(Random.Range(0, fishtankReefPrefabs.Length));		
		//Vector3 newReefPos = new Vector3( (lastFishtankReef.transform.position.x + reefSize), 0.0f, 0.0f);
		Vector3 newReefPos = new Vector3( (spawnFishtankReefLoc), 0.0f, 0.0f);
		
		nextFishtankReef = (GameObject)Instantiate(fishtankReefPrefabs[ranReef], newReefPos, Quaternion.identity);
		fishtankReefs.Add(nextFishtankReef);
		
		nextFishtankReef.transform.SetParent(fishtankTransform.transform, false);
		nextFishtankReef.SendMessage("InitReef", SendMessageOptions.DontRequireReceiver);
				
		if (qualityLevel > 0){
			nextFishtankReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
				
		if (fishtankReefs.Count > maxReefCount){
			Destroy(fishtankReefs[0]);
			fishtankReefs.Remove(fishtankReefs[0]);
			
		}		
		//spawnReefLoc += reefSize;
		spawnFishtankReefLoc += fishtankReefSize;
		Debug.Log("New Fishtank Reef Spawned");
		fishtankReefCount++;
		
	}
	
	public void AddNewReef( GameObject newReef)
	{
		/* NOT USED
		int ranReefScene = Mathf.FloorToInt(Random.Range(1, SceneManager.sceneCountInBuildSettings));
		SceneManager.LoadScene(ranReefScene, LoadSceneMode.Additive);

		Vector3 newReefPos = new Vector3(0.0f, 0.0f, (transform.position.z + reefSize));
		
		//nextReef = (GameObject)Instantiate(reefPrefabs[ranReef], newReefPos, Quaternion.identity);
		nextReef = newReef;
		nextReef.transform.position = newReefPos;
		nextReef.SendMessage("InitReef", SendMessageOptions.DontRequireReceiver);
		
		if (qualityLevel > 0){
			nextReef.SendMessage("DecreaseReefQuality", qualityLevel, SendMessageOptions.DontRequireReceiver);
		}
		*/
	}
	
	public void DestroyReef()
	{
		if (destroyReef){
			Destroy(destroyReef);
			//SceneManager.UnloadSceneAsync(
		}
		reefCount--;
	}
	
	public void DestroyFishtankReef()
	{
		if (destroyFishtankReef){
			Destroy(destroyFishtankReef);
			//SceneManager.UnloadSceneAsync(
		}
		fishtankReefCount--;
	}
	
	public void ToggleMainMenu()
	{
		PlayConfirmationSound();
		
		if (!mainMenu){
			return ;
		}
		if (menuOpen){
			mainMenu.SetActive(false);
			menuOpen = false;
		} else {
			mainMenu.SetActive(true);
			menuOpen = true;
		}
	}
	
	public void Reset()
	{
		
		PlayConfirmationSound();
		SceneManager.LoadScene(0, LoadSceneMode.Single);
	}
	
	public void CollectTrash()
	{
		PlayPickupSound();
		pickupGUI.SetActive(true);		
		trashCount++;
		trashCountText.text = trashCount.ToString();
		//isTimingOutPickupGUI = true;
		StartCoroutine(TimeoutPickupMenu());
		
		DisplayAcheivement();
		
		
	}
	
	public void CollectTreasure()
	{
		PlayPickupSound();
		pickupGUI.SetActive(true);		
		coinCount++;
		coinCountText.text = coinCount.ToString();
		//isTimingOutPickupGUI = true;
		StartCoroutine(TimeoutPickupMenu());
		
		DisplayAcheivement();
		
	}
	
	public void CollectCreature()
	{
		PlayPickupSound();
		/*
		pickupGUI.SetActive(true);		
		coinCount++;
		coinCountText.text = coinCount.ToString();
		//isTimingOutPickupGUI = true;
		StartCoroutine(TimeoutPicupMenu());
		*/
	}
	
	IEnumerator TimeoutPickupMenu()
	{
		//isTimingOutPickupGUI = true;
		
		if (!isTimingOutPickupGUI){
			isTimingOutPickupGUI = true;			
		} else {
			resetPickupTimer = true;
		}
		
		yield return new WaitForSeconds(pickupMenuTimeout);
		
		if (resetPickupTimer){
			resetPickupTimer = false;
		} else {
			pickupGUI.SetActive(false);
			resetPickupTimer = false;
			isTimingOutPickupGUI = false;
		}
		/*
		if (isTimingOutPickupGUI){
			isTimingOutPickupGUI = false;
			pickupGUI.SetActive(false);
		}*/
		
	}
	
	IEnumerator TimeoutGameObject( GameObject go, float timeoutTime)
	{
	
		yield return new WaitForSeconds(timeoutTime);
		
		go.SetActive(false);
		
	}
	
	IEnumerator TimeActivateGameObject( GameObject go, float activateTime)
	{
	
		yield return new WaitForSeconds(activateTime);
		
		go.SetActive(true);
		
	}
}
