using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilding : MonoBehaviour
{
    // Public inspector variables
    [Header("Building Properties")]
    public Color[] possibleWallColors;

    // Private instance variables
    private Renderer[] wallRenderers;

    void Start()
    {
        // Attempt to find the "Walls" child object
        Transform walls = transform.Find("Walls");
        if (walls != null)
        {
            // Get all the wall renderers
            wallRenderers = walls.GetComponentsInChildren<Renderer>();

            // Set the wall color based on possible colors
            SetWallColor();
        }
    }

    void SetWallColor()
    {
        // Pick a random color to be set on all wall renderers
        Color randomColor = possibleWallColors[Random.Range(0, possibleWallColors.Length)];

        // Set the color on all wall renderers
        foreach (Renderer wallRenderer in wallRenderers)
        {
            wallRenderer.material.color = randomColor;
        }
    }
}
