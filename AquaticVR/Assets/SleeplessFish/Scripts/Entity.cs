using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    // Data
    //-----------------------------------------------------------------------------
    private float mRadiusSquared = 5.0f;

    public int mID = 0;
    public GameObject shoalBounds;
    private App app;
    private Vector3 velocity = new Vector3();
    public float maxVelocity = 10.0f; // Speed of Fish Swimming
    public bool turning;
    //-----------------------------------------------------------------------------
    // Functions
    //-----------------------------------------------------------------------------
    void Start()
    {
        app = gameObject.GetComponentInParent<App>();
        velocity = transform.forward;
        velocity = Vector3.ClampMagnitude( velocity, maxVelocity );
        turning = false;
    }

    //-----------------------------------------------------------------------------
    void Update()
    {
        if (turning == false)
        {
            velocity += FlockingBehaviour();
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
            transform.position += velocity * Time.deltaTime;
            transform.forward = velocity.normalized;
        }
        Reposition();
    }

    //-----------------------------------------------------------------------------
    // This is used to ensure that the fish flock, but remain in the bounds of the hidden cube so as not to stray too far from the shoal.
    // If they do go out too far, this method tells the flocking logic to turn them back in towards the center of the shoal.
    private void Reposition()
    {
        Vector3 position = transform.position;
        if (position.x >= (shoalBounds.transform.position.x + (shoalBounds.transform.localScale.x / 2)))
        {
            turning = true;
        }
        if (position.x <= (shoalBounds.transform.position.x - (shoalBounds.transform.localScale.x / 2)))
        {
            turning = true;
        }
        if (position.y >= (shoalBounds.transform.position.y + (shoalBounds.transform.localScale.y / 2)))
        {
            turning = true;
        }
        if (position.y <= (shoalBounds.transform.position.y - (shoalBounds.transform.localScale.y / 2)))
        {
            turning = true;
        }
        if (position.z >= (shoalBounds.transform.position.z + (shoalBounds.transform.localScale.z / 2)))
        {
            turning = true;
        }
        if (position.z <= (shoalBounds.transform.position.z - (shoalBounds.transform.localScale.z / 2)))
        {
            turning = true;
        }
        if (IsLookingAtObject(transform, shoalBounds.transform.position,  20.0f) && turning == true)
        {
            turning = false;
        }
        
        if (turning == true)
        {
            Quaternion targetrotation = Quaternion.LookRotation(shoalBounds.transform.position - position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetrotation, 60.0f * Time.deltaTime);
            velocity = transform.forward;
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
            transform.position += velocity * Time.deltaTime;
            transform.forward = velocity.normalized;
        }
    }

    // Check to see if the fish has turned back around towards the center of the shoal and is now facing right at it
    bool IsLookingAtObject(Transform looker, Vector3 targetPos, float FOVAngle)
    {
        float checkAngle = Mathf.Min(FOVAngle, 359.9999f) / 2;
        float dot = Vector3.Dot(looker.forward, (targetPos - looker.position).normalized);
        float viewAngle = (1 - dot) * 90;
        if (viewAngle <= checkAngle)
            return true;
        else
            return false;
    }

    //-----------------------------------------------------------------------------
    public void SetID( int ID )
    {
        mID = ID;
    }

    //-----------------------------------------------------------------------------
    public GameObject ShoalBounds
    {
        get { return shoalBounds; }
    }
    //-----------------------------------------------------------------------------
    public void SetShoalBounds(GameObject cube)
    {
        shoalBounds = cube;
    }

    //-----------------------------------------------------------------------------
    public int ID
    {
        get { return mID; }
    }
    //-----------------------------------------------------------------------------
    // Flocking Behavior
    //-----------------------------------------------------------------------------
    private Vector3 FlockingBehaviour()
    {
        List<Entity> theFlock = app.theFlock;

        Vector3 cohesionVector = new Vector3();
        Vector3 separateVector = new Vector3();
        Vector3 forward = new Vector3();

        int count = 0;

        for ( int i = 0; i < theFlock.Count; i++ )
        {
            if ( mID != theFlock[ i ].ID )
            {
                float distance = (transform.position - theFlock[ i ].transform.position).sqrMagnitude;

                if ( distance > 0 && distance < mRadiusSquared )
                {
                    separateVector += theFlock[ i ].transform.position - transform.position;
                    forward += theFlock[ i ].transform.forward;
                    cohesionVector += theFlock[ i ].transform.position;
                    count++;
                }
            }
        }

        if ( count == 0 )
        {
            return Vector3.zero;
        }

        // revert vector
        // separation step
        separateVector /= count;
        separateVector *= -1;

        // forward step
        forward /= count;

        // cohesion step
        cohesionVector /= count;
        cohesionVector = ( cohesionVector - transform.position );

        Vector3 flockingVector =    ( ( separateVector.normalized * app.separationWeight ) + 
                                    ( cohesionVector.normalized * app.cohesionWeight ) + 
                                    ( forward.normalized * app.alignmentWeight ) );

        return flockingVector;
    }
}
