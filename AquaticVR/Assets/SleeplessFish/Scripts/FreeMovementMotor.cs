using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMovementMotor : MonoBehaviour
{
    public Vector3 movementDirection;

    // Simpler motors might want to drive movement based on a target purely
    public Vector3 movementTarget;

    // The direction the character wants to face towards, in world space.
    public Vector3 facingDirection;

    public float walkingSpeed = 5.0f;
	public float walkingSnappyness = 50f;
	public float turningSmoothing = 0.3f;
	
	void FixedUpdate () {
        // Handle the movement of the character
        Vector3 targetVelocity = movementDirection * walkingSpeed;
        Vector3 deltaVelocity = targetVelocity - GetComponent<Rigidbody>().velocity;
		if (GetComponent<Rigidbody>().useGravity)
			deltaVelocity.y = 0;
		GetComponent<Rigidbody>().AddForce (deltaVelocity * walkingSnappyness, ForceMode.Acceleration);

        // Setup player to face facingDirection, or if that is zero, then the movementDirection
        Vector3 faceDir = facingDirection;
		if (faceDir == Vector3.zero)
			faceDir = movementDirection;
		
		// Make the character rotate towards the target rotation
		if (faceDir == Vector3.zero) {
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		else {
            float rotationAngle = AngleAroundAxis (transform.forward, faceDir, Vector3.up);
			GetComponent<Rigidbody>().angularVelocity = (Vector3.up * rotationAngle * turningSmoothing);
		}
        // If we are swimming, then disable gravity and slowly sink
        if (transform.gameObject.GetComponent<PlayerMoveController>().m_IsUnderwater == true)
        {
            GetComponent<Rigidbody>().useGravity = false;
            //GetComponent<Rigidbody>().AddForce(-Vector3.up * -5f, ForceMode.Acceleration);
        }
        else
        {
            // Otherwise we are not swimming so enable gravity
            GetComponent<Rigidbody>().useGravity = true;
        }
        // If we are swimming, and we hit SPACE, then head up slowly
        if (Input.GetKey(KeyCode.Space) && transform.gameObject.GetComponent<PlayerMoveController>().m_IsUnderwater == true)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 80, ForceMode.Acceleration);
        }
    }
	void Update()
    {
        // Player is jumping if SPACE and not already jumping and not swimming
        if (Input.GetKeyDown(KeyCode.Space) && transform.gameObject.GetComponent<PlayerMoveController>().m_IsJumping == false && transform.gameObject.GetComponent<PlayerMoveController>().m_IsUnderwater == false)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 600, 0), ForceMode.Impulse);
        }

        var moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = Camera.main.transform.TransformDirection(moveDirection);

        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + moveDirection * 9.0f * Time.deltaTime);
    }
    // Check to see if we are touching a ground surface, and if so, we are not jumping
    void OnCollisionEnter(Collision theCollision)
    {
        if (theCollision.gameObject.tag == "Terrain" || theCollision.gameObject.tag == "Obstacle")
        {
            transform.gameObject.GetComponent<PlayerMoveController>().m_IsJumping = false;
        }
    }
    // Check to see if we are leaving a ground surface, we are most likely jumping
    void OnCollisionExit(Collision theCollision)
    {
        if (theCollision.gameObject.tag == "Terrain" || theCollision.gameObject.tag == "Obstacle")
        {
            transform.gameObject.GetComponent<PlayerMoveController>().m_IsJumping = true;
        }
    }
    // The angle between dirA and dirB around axis
    static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3  axis) {
	    // Project A and B onto the plane orthogonal target axis
	    dirA = dirA - Vector3.Project (dirA, axis);
	    dirB = dirB - Vector3.Project (dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle (dirA, dirB);
	   
	    // Return angle multiplied with 1 or -1
	    return angle * (Vector3.Dot (axis, Vector3.Cross (dirA, dirB)) < 0 ? -1 : 1);
	}
	
}
