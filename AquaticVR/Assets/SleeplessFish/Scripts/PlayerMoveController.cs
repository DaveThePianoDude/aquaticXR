using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// From the Unity AngryBots Demo
// We have added the ability to handle underwater effects, jumping and swimming in this script
public class PlayerMoveController : MonoBehaviour
{
    // Objects to drag in
    public FreeMovementMotor motor;
    public Transform character;
    public GameObject cursorPrefab;
    public GameObject player;

    // Settings
    public float cameraSmoothing = 0.01f;
    public float cameraPreview = 2.0f;

    // Cursor settings
    public float cursorPlaneHeight = 0;
    public float cursorFacingCamera = 0;
    public float cursorSmallerWithDistance = 0;
    public float cursorSmallerWhenClose = 1;

    // Private member data
    private Plane playerMovementPlane;

    public bool m_IsSwimming;
    public bool m_IsUnderwater;
    public bool m_IsJumping;

    private GameObject waterBody;
    void Awake()
    {
        m_IsSwimming = false;
        m_IsJumping = false;
        m_IsUnderwater = false;
        motor.movementDirection = Vector2.zero;
        motor.facingDirection = Vector2.zero;

        // Set main camera
        waterBody = GameObject.FindGameObjectWithTag("Water");
        // Ensure we have character set
        // Default to using the transform this component is on
        if (!character)
            character = transform;

        // caching movement plane
        playerMovementPlane = new Plane(character.up, character.position + character.up * cursorPlaneHeight);
    }

    void Start()
    {

    }

    void Update()
    {
        // HANDLE CHARACTER MOVEMENT DIRECTION
        if (Input.GetMouseButton(1) || Input.GetButton("Vertical"))
        {
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }
        // Make sure the direction vector doesn't exceed a length of 1
        // so the character can't move faster diagonally than horizontally or vertically
        if (motor.movementDirection.sqrMagnitude > 1)
            motor.movementDirection.Normalize();

        // optimization (instead of newing Plane):
        playerMovementPlane.normal = character.up;
        playerMovementPlane.distance = -character.position.y + cursorPlaneHeight;

        if (transform.position.y < waterBody.transform.position.y - 3.6f)
        {
            m_IsUnderwater = true;
        }
        else
        {
            m_IsUnderwater = false;
        }
    }

    public static Vector3 PlaneRayIntersection(Plane plane, Ray ray)
    {
        float dist;
        plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }

    public void inWater()
    {
        m_IsSwimming = true;
    }
    public void outOfWater()
    {
        m_IsSwimming = false;
    }
    public void underWater()
    {
        m_IsUnderwater = true;
    }
    public void aboveWater()
    {
        m_IsUnderwater = false;
    }
}