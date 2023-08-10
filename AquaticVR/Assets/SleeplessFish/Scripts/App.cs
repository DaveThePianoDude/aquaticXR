using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class App : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    // Data
    //-----------------------------------------------------------------------------
    public Entity templatePrefab = null;
    public GameObject shoalBounds;

    public float separationWeight = 0.75f;
    public float alignmentWeight = 0.8f;
    public float cohesionWeight = 0.7f;

    public bool moveShoal = false;

    public List<Entity> theFlock = new List<Entity>();

    public static App instance = null;

    public int numberOfEntities = 200;
	public bool randomEntityCount = true;
	public int minNumEntities = 100;
	public int maxNumEntities = 300;
    private int fishCount;

    public float speedCoefficient = 1.0f;
    public float maxSpeed = 0.3f;
    public GameObject finalPositionObject;
    private Vector3 finalPosition;
    private Vector3 direction;
	
	public float minScale = 0.5f;
	public float maxScale = 2.0f;
	
	public Vector3 spawnBoundsScale = new Vector3(20.0f, 5.0f, 20.0f);
	
    //-----------------------------------------------------------------------------
    // Functions
    //-----------------------------------------------------------------------------
    void Start ()
    {
        instance = this;
        fishCount = 0;
		if (randomEntityCount){
			numberOfEntities = Mathf.FloorToInt(Random.Range(minNumEntities, maxNumEntities));
		}
        StartCoroutine(InstantiateFlock());
        finalPosition = finalPositionObject.transform.position;
        direction = (finalPosition - transform.position).normalized;
		
    }
    void FixedUpdate()
    {
        if (moveShoal == true)
        {
            float distance = Vector3.Distance(finalPosition, transform.position); // Get distance to target
            float speed = Mathf.Clamp(distance * speedCoefficient, 0f, maxSpeed);
            GetComponent<Rigidbody>().velocity = direction * speed;
        }
    }
    //-----------------------------------------------------------------------------
    IEnumerator InstantiateFlock()
    {
        while (fishCount < numberOfEntities)
        {
            Entity flockEntity = Instantiate(templatePrefab, new Vector3(Random.Range(shoalBounds.transform.position.x - spawnBoundsScale.x, shoalBounds.transform.position.x + spawnBoundsScale.x), Random.Range(shoalBounds.transform.position.y - spawnBoundsScale.y, shoalBounds.transform.position.y + spawnBoundsScale.y), Random.Range(shoalBounds.transform.position.z - spawnBoundsScale.z, shoalBounds.transform.position.z + spawnBoundsScale.z)), templatePrefab.transform.rotation);
            flockEntity.transform.parent = gameObject.transform;
			float ranScale = Random.Range(minScale,maxScale);
			
			flockEntity.transform.localScale = new Vector3(transform.localScale.x * ranScale, transform.localScale.y * ranScale , transform.localScale.z * ranScale);
			
            flockEntity.SetID(fishCount);
            flockEntity.SetShoalBounds(shoalBounds);
            theFlock.Add(flockEntity);
			
            fishCount++;
            yield return null;
        }
    }
}
