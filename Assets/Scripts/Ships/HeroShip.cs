using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroShip : MonoBehaviour
{
    // Public inspector variables
    [Header("Movement Properties")]
    public Vector2 movementSpeed;
    public Vector2 movementAreaPercent;
    public Vector2 minimumTurnDistancePercent;
    public float movementDragFactor = 0.1f;
    public float maxRotationXDegrees = 10f;
    public float maxRotationZDegrees = 10f;
    public float rotationParallaxFactor = 0.3f;
    public float rotationSpeed = 0.1f;

    [Header("Dynamic Properties")]
    public bool isMoving = true;

    // Private instance variables
    private Vector2 movementBounds;

    // Start is called before the first frame update
    void Start()
    {
        // Store the movement boundaries based on the orthographic size and the movement area percent (From SHMUP Tutorial)
        movementBounds.y = Camera.main.orthographicSize;
        movementBounds.x = movementBounds.y * Camera.main.aspect;

        // Convert the percent based properties to decimal values
        movementAreaPercent = movementAreaPercent / 100f;
        minimumTurnDistancePercent = minimumTurnDistancePercent / 100f;
    }

    // Use FixedUpdate() for movement so high frame rates don't cause the game to become insanely fast
    void FixedUpdate()
    {
        // Ensure that the ship is moving
        if (isMoving)
        {
            // Detect user input to determine if the ship should move
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            // Move the ship on the x and y axis based on the input values * the movement speeds
            // Clamp the movement to the movement area percents to prevent ship from going outside the bounds
            Vector3 postion = transform.position;
            postion.x = Mathf.Clamp(postion.x + (inputX * movementSpeed.x), -movementAreaPercent.x * movementBounds.x, movementAreaPercent.x * movementBounds.x);
            postion.y = Mathf.Clamp(postion.y + (inputY * movementSpeed.y), -movementAreaPercent.y * movementBounds.y, movementAreaPercent.y * movementBounds.y);

            // Smoothly lerp the position to the new position based on the movement drag factor
            transform.position = Vector3.Lerp(transform.position, postion, movementDragFactor);

            // Get the ship's current x and z rotation values
            // Offset the target rotation x value with the WorldAnchor x rotation value to maintain anchor positioning
            float targetRotationX = -(inputY * maxRotationXDegrees);
            float targetRotationZ = -(inputX * maxRotationZDegrees);

            // Set the new rotation values smoothly with a Lerp
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotationX, 0f, targetRotationZ), rotationSpeed);

            // Match the world anchor z rotation to the ship's z rotation with a parallax factor
            WorldAnchor.Instance.rotationDegreesZ = -(targetRotationZ * rotationParallaxFactor);
        }
    }

    public float GetHorizontalDistanceToCenterRatio()
    {
        return transform.position.x / movementBounds.x;
    }

    public float GetVerticalDistanceToCenterRatio()
    {
        return transform.position.y / movementBounds.y;
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Obstacle" || collision.gameObject.tag == "Window")
        {
            GameManager.Instance.ResetGame();
        }
    }
}
