using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAnchor : MonoBehaviour
{
    [HideInInspector]
    public static WorldAnchor Instance;

    [Header("Anchor References")]
    public Transform heroShipTransform;

    [Header("Anchor Properties")]
    public float rotationDegreesX = 0f;
    public float rotationDegreesY = 0f;
    public float rotationDegreesZ = 0f;
    public float rotationSpeed = 0.1f;

    // Private instance variables
    private List<Action> onceRotationCompleteActions;

    void Awake()
    {
        // Set the instance to this object for a bad but usuable singleton pattern
        Instance = this;

        // Initialize the once rotation complete actions list
        onceRotationCompleteActions = new List<Action>();
    }

    void FixedUpdate()
    {
        // Smoothly match the current x and y rotation to the target rotation
        // Smoothly match the z rotation to that of the hero ship for the parallax effect
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(rotationDegreesX, rotationDegreesY, rotationDegreesZ);
        transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed);

        // Flush the once rotation complete actions list if we have some
        if (currentRotation == targetRotation && onceRotationCompleteActions.Count > 0) FlushRotationCompleteActions();
    }

    void FlushRotationCompleteActions()
    {
        // Loop through all the actions and execute them
        for (int i = 0; i < onceRotationCompleteActions.Count; i++) onceRotationCompleteActions[i]();

        // Clear the actions list
        onceRotationCompleteActions.Clear();
    }

    public void OnceRotationComplete(Action action)
    {
        // Add the action to the list
        onceRotationCompleteActions.Add(action);
    }

    public void SetInstantRotation(float x, float y, float z)
    {
        // Write the appropriate values
        rotationDegreesX = x;
        rotationDegreesY = y;
        rotationDegreesZ = z;

        // Set the rotation instantly
        transform.rotation = Quaternion.Euler(rotationDegreesX, rotationDegreesY, rotationDegreesZ);
    }
}
