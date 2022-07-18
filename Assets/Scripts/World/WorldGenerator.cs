using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldGenerator : MonoBehaviour
{
    // Public inspector variables
    [Header("Generator References")]
    public GameObject heroShip;
    public GameObject scrollerPrefab;

    [Header("Generator Properties")]
    public int trailEndChancePercent = 50; // Chance that the trail ends and thus a bunch of left/right/up/down turns are spawned
    public int trailLeftChancePercent = 50; // Chance to spawn a trail on the left side of the screen
    public int trailRightChancePercent = 50; // Chance to spawn a trail on the right side of the screen
    public int trailUpChancePercent = 50; // Chance to spawn a trail on the top of the screen
    public int trailDownChancePercent = 50; // Chance to spawn a trail on the bottom of the screen
    public float trailTickSeconds = 5f; // Determine's how often the trail will tick and check the percents to spawn a dead end with turns

    [Header("Dynamic Properties")]
    public bool isGenerating = true;

    // Private instance variables
    private float screenWidthFromCenter;
    private float screenHeightFromCenter;
    private GameObject[] buildingScrollers; // 0 = active, 1 = left, 2 = right, 3 = up, 4 = down
    private bool trailChangeScheduled = false;

    // Start is called before the first frame update
    void Start()
    {
        // Cache the screen height and width from the center
        screenHeightFromCenter = Camera.main.orthographicSize;
        screenWidthFromCenter = screenHeightFromCenter * Camera.main.aspect;

        // Initialize the trails which resets the world generator
        InitializeTrails();

        // Begin ticking the trail modifier
        Invoke("TickTrailModifier", trailTickSeconds);
    }

    public void InitializeTrails()
    {
        // Destroy the old trails if they exist
        if (buildingScrollers != null)
        {
            foreach (GameObject scroller in buildingScrollers) Destroy(scroller);
        }

        // Spawn 5 trails where 0 = Active while the other ones are inactive and will eventually represent turns
        buildingScrollers = new GameObject[5];
        for (int i = 0; i < buildingScrollers.Length; i++)
        {
            // Instantiate the building scroller under the world generator
            buildingScrollers[i] = Instantiate(scrollerPrefab, transform.position, Quaternion.identity);
            buildingScrollers[i].transform.parent = transform;

            // Only activate the first trail building scroller
            if (i == 0)
            {
                // Enable the scrolling on the building scroller for this trail
                buildingScrollers[i].name = "Primary";
                BuildingScroller buildingScroller = buildingScrollers[i].GetComponent<BuildingScroller>();
            }
        }

        // Disable inactive trails after initial spawn sequence
        Invoke("DisableInactiveTrails", 0.1f);
    }

    void DisableInactiveTrails()
    {
        // Disable all the inactive building scroller trails
        for (int i = 1; i < buildingScrollers.Length; i++)
        {
            // Disable the scrolling on the building scroller for this trail
            GameObject trail = buildingScrollers[i];
            trail.SetActive(false);
        }
    }

    void TickTrailModifier()
    {
        // Check if the trail should end with other paths
        if (isGenerating && UnityEngine.Random.Range(0, 100) <= trailEndChancePercent)
        {
            // Schedule a trail change to have the ship choose a new path
            ScheduleTrailChange();
        }

        // Tick the trail modifier after a certain amount of time
        Invoke("TickTrailModifier", trailTickSeconds);
    }

    void ScheduleTrailChange()
    {
        // Do not allow multiple changes to be scheduled
        if (trailChangeScheduled) return;

        // Retrieve the active trail and building scroller
        GameObject activeTrail = buildingScrollers[0];
        BuildingScroller activeScroller = activeTrail.GetComponent<BuildingScroller>();

        // Attempt to spawn different paths at the trail position
        if (SpawnDifferentPaths(activeScroller.trailPosition))
        {
            // Spawn a dead end with appropriate blocks on the left and right
            activeScroller.SpawnDeadEnd(
                !buildingScrollers[1].activeSelf,
                !buildingScrollers[2].activeSelf
            );

            // Mark this instance to have a scheduled trail change
            trailChangeScheduled = true;
        }
    }

    bool SpawnDifferentPaths(Vector3 trailPosition)
    {
        bool spawnedAPath = false;

        // See if we should spawn a left path
        if (UnityEngine.Random.Range(0, 100) <= trailLeftChancePercent)
        {
            // Utilize the disabled building scroller at index 1 as the left path
            // Debug.Log("Spawned a left path");
            spawnedAPath = true;
            TrackPath(trailPosition, 1);
        }

        // See if we should spawn a right path
        if (UnityEngine.Random.Range(0, 100) <= trailRightChancePercent)
        {
            // Utilize the disabled building scroller at index 2 as the right path
            // Debug.Log("Spawned a right path");
            spawnedAPath = true;
            TrackPath(trailPosition, 2);
        }

        // See if we should spawn an up path
        if (UnityEngine.Random.Range(0, 100) <= trailUpChancePercent)
        {
            // Utilize the disabled building scroller at index 3 as the up path
            // Debug.Log("Spawned an up path");
            spawnedAPath = true;
            TrackPath(trailPosition, 3);
        }

        // See if we should spawn a down path
        if (UnityEngine.Random.Range(0, 100) <= trailDownChancePercent)
        {
            // Utilize the disabled building scroller at index 4 as the down path
            // Debug.Log("Spawned a down path");
            spawnedAPath = true;
            TrackPath(trailPosition, 4);
        }

        return spawnedAPath;
    }

    void TrackPath(Vector3 trailPosition, int index)
    {
        // Retrieve the building scroller at the index
        GameObject trail = buildingScrollers[index];
        BuildingScroller scroller = trail.GetComponent<BuildingScroller>();

        // Initialize a mutable copy of the trail position
        Vector3 newPosition = trailPosition;

        // Set the appropriate rotation based on the proper index
        Vector3 buildingScrollVector = Vector3.back * scroller.buildingScrollSpeed * Time.fixedDeltaTime;
        float horizontalMovementDistance = screenWidthFromCenter * (scroller.buildingSeparationPercent / 100f);
        float verticalMovementDistance = screenHeightFromCenter * (scroller.buildingSeparationPercent / 100f);
        switch (index)
        {
            case 1:
                // Modify the position and rotation for a "Left" path
                trail.name = "Left";
                trail.transform.rotation = Quaternion.Euler(0, -90, 0);
                newPosition.x -= horizontalMovementDistance;
                newPosition.z += horizontalMovementDistance + buildingScrollVector.z;
                break;
            case 2:
                // Modify the position and rotation for a "Right" path
                trail.name = "Right";
                trail.transform.rotation = Quaternion.Euler(0, 90, 0);
                newPosition.x += horizontalMovementDistance;
                newPosition.z += horizontalMovementDistance + buildingScrollVector.z;
                break;
            case 3:
                // Modify the position and rotation for a "Up" path
                trail.name = "Up";
                trail.transform.rotation = Quaternion.Euler(-90, 0, 0);
                newPosition.y += verticalMovementDistance * 2f;
                newPosition.z += horizontalMovementDistance + buildingScrollVector.z;
                break;
            case 4:
                // Modify the position and rotation for a "Down" path
                trail.name = "Down";
                trail.transform.rotation = Quaternion.Euler(90, 0, 0);
                newPosition.y -= verticalMovementDistance * 2f;
                newPosition.z += horizontalMovementDistance + buildingScrollVector.z;
                break;
        }

        // Move the trail to the trail position
        trail.transform.position = newPosition;

        // Enable this trail so it gets tracked by the fixed update and is rendered in the world
        trail.SetActive(true);
    }

    void PerformTrailChange()
    {
        // Retrieve the hero ship script from the hero ship game object
        HeroShip heroShipScript = heroShip.GetComponent<HeroShip>();

        // Retrieve the horizontal and vertical ratios to the center of the hero script
        float horizontalRatio = heroShipScript.GetHorizontalDistanceToCenterRatio();
        float verticalRatio = heroShipScript.GetVerticalDistanceToCenterRatio();
        Vector2 minimumTurnRatios = heroShipScript.minimumTurnDistancePercent;

        // Pick the greater axis ratio to determine the desired trail change
        int trailChangeDirectionIndex = -1;
        if (Mathf.Abs(horizontalRatio) >= Mathf.Abs(verticalRatio) && Mathf.Abs(horizontalRatio) >= minimumTurnRatios.x)
        {
            trailChangeDirectionIndex = horizontalRatio < 0 ? 1 : 2;
            // Debug.Log("Turned " + (horizontalRatio < 0 ? "left" : "right"));
        }
        else if (Mathf.Abs(verticalRatio) >= minimumTurnRatios.y)
        {
            trailChangeDirectionIndex = verticalRatio > 0 ? 3 : 4;
            // Debug.Log("Turned " + (verticalRatio > 0 ? "up" : "down"));
        }

        // Determine if the hero has crashed meaning trail change direction index is -1
        if (trailChangeDirectionIndex == -1 || !buildingScrollers[trailChangeDirectionIndex].activeSelf)
        {
            // Hero has crashed! disable world generation
            isGenerating = false;

            // Reset the game which will send back to the main menu UI
            GameManager.Instance.ResetGame();
        }
        else
        {
            // Swap the trail at the direction index with the primary trail
            GameObject oldTrail = buildingScrollers[0];
            GameObject newTrail = buildingScrollers[trailChangeDirectionIndex];
            buildingScrollers[0] = newTrail;
            buildingScrollers[trailChangeDirectionIndex] = oldTrail;

            // Swap the positions and rotations of the new and old trails
            Vector3 oldTrailPosition = oldTrail.transform.position;
            Vector3 newTrailPosition = newTrail.transform.position;
            Quaternion oldTrailRotation = oldTrail.transform.rotation;
            Quaternion newTrailRotation = newTrail.transform.rotation;
            oldTrail.transform.position = newTrailPosition;
            newTrail.transform.position = oldTrailPosition;
            oldTrail.transform.rotation = newTrailRotation;
            newTrail.transform.rotation = oldTrailRotation;

            // Retrieve scroller references for the new and old trails
            BuildingScroller oldTrailScroller = oldTrail.GetComponent<BuildingScroller>();
            BuildingScroller newTrailScroller = newTrail.GetComponent<BuildingScroller>();

            // Reset the old trail in a proper state to prevent rotation errors
            oldTrail.transform.position = Vector3.zero;
            oldTrail.transform.rotation = Quaternion.identity;
            oldTrailScroller.Reset();

            // Track the old trail to the approriate future position and disable the game object
            TrackPath(oldTrailScroller.trailPosition, trailChangeDirectionIndex);
            oldTrail.SetActive(false);

            // Make the new trail begin scrolling as it is now the primary trail
            newTrailScroller.isScrolling = true;

            // Disable all of the other scroller trails until the next trail change
            for (int i = 1; i < buildingScrollers.Length; i++)
            {
                GameObject trail = buildingScrollers[i];
                trail.SetActive(false);
            }

            // Perform the world anchor animation
            PerformWorldAnchorRotation(trailChangeDirectionIndex);
        }

        // Unlock trail changes for the future
        trailChangeScheduled = false;
    }

    void PerformWorldAnchorRotation(int directionIndex, float turnDegrees = 60f)
    {
        // Make the world anchor instant rotate to the proper direction
        switch (directionIndex)
        {
            case 1:
                WorldAnchor.Instance.SetInstantRotation(0, turnDegrees, 0);
                break;
            case 2:
                WorldAnchor.Instance.SetInstantRotation(0, -turnDegrees, 0);
                break;
            case 3:
                WorldAnchor.Instance.SetInstantRotation(turnDegrees, 0, 0);
                break;
            case 4:
                WorldAnchor.Instance.SetInstantRotation(-turnDegrees, 0, 0);
                break;
        }

        // Set the animated world anchor rotation to zero to simulate animation
        WorldAnchor.Instance.rotationDegreesX = 0;
        WorldAnchor.Instance.rotationDegreesY = 0;
        WorldAnchor.Instance.rotationDegreesZ = 0;
    }

    void FixedUpdate()
    {
        // Halt exectuion if we are no longer building
        if (!isGenerating) return;

        // Determine if a new trail change should be scheduled
        if (trailChangeScheduled)
        {
            // Determine if the heroShip's z position has reached the trail position of the active trail
            GameObject activeTrail = buildingScrollers[0];
            BuildingScroller activeScroller = activeTrail.GetComponent<BuildingScroller>();
            if (heroShip.transform.position.z >= activeScroller.trailPosition.z)
            {
                // Stop the active scroller and perform the trail change
                activeScroller.isScrolling = false;
                PerformTrailChange();
            }

            // Loop through the inactive 1 - 4 index trails to see if they need to be tracked with the primary trail position
            for (int i = 1; i < buildingScrollers.Length; i++)
            {
                // Retrieve the trail and scroller references
                GameObject trail = buildingScrollers[i];
                BuildingScroller scroller = trail.GetComponent<BuildingScroller>();

                // Check if the trail is active
                if (trail.activeInHierarchy)
                {
                    // Retrieve the trail position from the active trail
                    Vector3 trailPosition = buildingScrollers[0].GetComponent<BuildingScroller>().trailPosition;

                    // Track the trail position
                    TrackPath(trailPosition, i);
                }
            }
        }
    }

    public BuildingScroller GetBuildingScrollerAtIndex(int index)
    {
        GameObject trail = buildingScrollers[index];
        if (trail != null)
        {
            BuildingScroller scroller = trail.GetComponent<BuildingScroller>();
            if (scroller != null)
            {
                return scroller;
            }
        }

        return null;
    }
}
