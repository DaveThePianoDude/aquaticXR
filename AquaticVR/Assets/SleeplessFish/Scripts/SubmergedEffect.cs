using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityStandardAssets.ImageEffects;
public class SubmergedEffect : MonoBehaviour 
{
	public GameObject waterBody;
    public GameObject waterBodyUnder;
    //public Material waterMaterial;
    //private GameObject waterSplashPane;
    public Color underWaterColor;
	public float underWaterVisiblity;
	public bool aboveWaterFogMode;
	public Color aboveWaterColor;
	public float aboveWaterVisiblity;
    public GameObject WaterParticles;
    private GameObject Player;
	public bool checkedIfAboveWater;
	private float waterHeight;
    private AudioSource m_AudioSource;
    private AudioSource m_JumpInWaterAudioSource;
    private AudioSource m_JumpOutOfWaterAudioSource;

    //private GameObject playerCam;
    void Start () 
	{
        ParticleSystem.EmissionModule waterParticles_emission; // Locate and cache the underwater particles effect and enable it
        waterParticles_emission = WaterParticles.GetComponent<ParticleSystem>().emission;
        waterParticles_emission.enabled = true;
        if (WaterParticles.GetComponent<ParticleSystem>().isPlaying)
        {
            WaterParticles.GetComponent<ParticleSystem>().Stop();
            WaterParticles.GetComponent<ParticleSystem>().Clear();
        }
        //if (waterMaterial != null)
        //{
        //    waterMaterial.SetVector("_GAmplitude", new Vector4(0.03f, 0.03f, 0.03f, 0.03f));
        //}

        
        // playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        underWaterColor = new Color(0.0f/255.0F,74.0f/255.0F,190.0f/255.0F,255.0f/255.0F); // Set the color for underwater fog
		Player = GameObject.FindGameObjectWithTag("Player"); // Cache the player
		checkedIfAboveWater = true;
		underWaterVisiblity = 0.006f; // Set the Underwater Visibility - can be adjusted publicly
        // Cache the audiosources for underwater, splash in and splash out of water
		m_AudioSource = waterBodyUnder.GetComponent<AudioSource>();
		m_JumpInWaterAudioSource = GameObject.FindGameObjectWithTag("JumpInWater").GetComponent<AudioSource> ();
		m_JumpOutOfWaterAudioSource = GameObject.FindGameObjectWithTag("JumpOutOfWater").GetComponent<AudioSource> ();
		Camera.main.nearClipPlane = 0.1f;
		waterHeight = waterBody.transform.position.y; // This is critical! It is the height of the water plane to determine we are underwater or not
		AssignAboveWaterSettings (); // Initially set above water settings
    }
	// Update is called once per frame
	void Update () 
	{
        // the checkedifAboveWater stops it forcing it over and over every frame if we already know where we are
        // If tghe player is above water and we haven't confirmed this yet, then set settings for above water and confirm
		if (transform.position.y >= waterHeight && checkedIfAboveWater == false) 
		{
            checkedIfAboveWater = true;
			ApplyAboveWaterSettings ();
			ToggleFlares (true);
		}
        // If we are under water and we haven't confirmed this yet, then set for under water and confirm
		if (transform.position.y < waterHeight && checkedIfAboveWater == true) 
		{
			checkedIfAboveWater = false;
			ApplyUnderWaterSettings ();
			ToggleFlares (false);
		}
	}
    // Initially assign current world fog ready for reuse later in above water 
	void AssignAboveWaterSettings () 
	{
		aboveWaterFogMode = RenderSettings.fog;
		aboveWaterColor = RenderSettings.fogColor;
		aboveWaterVisiblity = RenderSettings.fogDensity;
	}
    // Apply Abovewater default settings - enabling the above water view and effects
    void ApplyAboveWaterSettings () 
	{
        waterBody.SetActive(true);
        waterBodyUnder.SetActive(false);
        StopUnderWaterSound ();
		PlayExitSplashSound();
		Player.SendMessage("outOfWater");
        if (WaterParticles.GetComponent<ParticleSystem>().isPlaying)
        {
            WaterParticles.GetComponent<ParticleSystem>().Stop();
            WaterParticles.GetComponent<ParticleSystem>().Clear();
        }
        //playerCam.GetComponent<GlobalFog>().enabled = false;
        RenderSettings.fog = aboveWaterFogMode;
		RenderSettings.fogColor = aboveWaterColor;
		RenderSettings.fogDensity = aboveWaterVisiblity;
	}

    // Apply Underwater default settings - enabling the under water view and effects
    void ApplyUnderWaterSettings () 
	{
        waterBody.SetActive(false);
        waterBodyUnder.SetActive(true);
        PlayEnterSplashSound();
		PlayUnderWaterSound ();
		RenderSettings.fog = aboveWaterFogMode;
		RenderSettings.fogColor = underWaterColor;
		RenderSettings.fogDensity = underWaterVisiblity;
		Player.SendMessage("inWater");
        if (!WaterParticles.GetComponent<ParticleSystem>().isPlaying)
        {
            WaterParticles.GetComponent<ParticleSystem>().Play();
        }
    }
    
    // Toggle flares on or off depending on whether we are underwater or not
    void ToggleFlares (bool state) 
	{
		LensFlare[] lensFlares = FindObjectsOfType(typeof(LensFlare)) as LensFlare[];
		foreach (LensFlare currentFlare in lensFlares) 
		{
			currentFlare.enabled = state;
		}
	}
	private void PlayUnderWaterSound()
	{
	m_AudioSource.Play ();
	}
	private void StopUnderWaterSound()
	{
		m_AudioSource.Stop ();
	}
	private void PlayEnterSplashSound()
	{
		m_JumpInWaterAudioSource.Play ();
	}
	private void PlayExitSplashSound()
	{
		m_JumpOutOfWaterAudioSource.Play ();
	}
}
