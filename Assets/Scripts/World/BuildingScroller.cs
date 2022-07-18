using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingScroller : MonoBehaviour
{
    // Public inspector variables
    [Header("Building Properties")]
    public ComponentPool buildingsPool;
    public ComponentPool obstaclesPool;
    public Vector3 buildingScale;
    public Vector3 buildingOffset;

    [Header("Building Spawner Properties")]
    public int despawnAfterRows = 2;
    public int numBuildingsPerRow;
    public float buildingScrollSpeed;
    public float spaceInBetweenBuildings;
    public float buildingSeparationPercent;

    [Header("Obstacle Spawner Properties")]
    public Vector2 obstacleMinScale;
    public Vector2 obstacleMaxScale;
    public float obstacleSpawnerTickSeconds;
    public float obstacleSpawnChancePercent;
    public Vector2 obstacleSpawnRangePercent;

    [Header("Dynamic Properties")]
    public bool isSpawning = true;
    public bool isScrolling = false;
    public Vector3 trailPosition;

    // Private instance variables
    private GameObject trailObject;
    private float screenWidthFromCenter;
    private float screenHeightFromCenter;
    private List<GameObject> leftSideBuildings;
    private List<GameObject> rightSideBuildings;
    private List<GameObject> scrollingObstacles;
    private bool pendingObstacleSpawn = false;

    void Start()
    {
        // Initialize private list variables
        leftSideBuildings = new List<GameObject>();
        rightSideBuildings = new List<GameObject>();
        scrollingObstacles = new List<GameObject>();

        // Cache the screen height and width from the center
        screenHeightFromCenter = Camera.main.orthographicSize;
        screenWidthFromCenter = screenHeightFromCenter * Camera.main.aspect;

        // Spawn the initial set of buildings
        for (int i = 0; i < numBuildingsPerRow; i++) SpawnBuildingPair(i);

        // Spawn an empty game object to be positioned at the trail position
        trailObject = new GameObject();
        trailObject.transform.parent = transform;
        trailObject.name = "Trail Position";

        // Begin ticking the obstacle spawner
        TickObstacleSpawner();
    }

    public void Reset()
    {
        // Release all the buildings
        foreach (GameObject building in leftSideBuildings) buildingsPool.pool.Release(building);
        foreach (GameObject building in rightSideBuildings) buildingsPool.pool.Release(building);

        // Clear the active buildings list
        leftSideBuildings.Clear();
        rightSideBuildings.Clear();

        // Re-Spawn the initial set of buildings
        for (int i = 0; i < numBuildingsPerRow; i++) SpawnBuildingPair(i);

        // Destroy all the obstacles if we have some
        if (scrollingObstacles.Count > 0)
        {
            foreach (GameObject obstacle in scrollingObstacles) obstaclesPool.pool.Release(obstacle);
            scrollingObstacles.Clear();
        }

        // Enable spawning but disable scrolling back to default state
        isSpawning = true;
        isScrolling = false;
    }

    void TickObstacleSpawner()
    {
        // Ensure we are spawning obstacles only when we are scrolling and spawning buildings
        if (isSpawning && isScrolling)
        {
            // Check if we should spawn an obstacle
            if (UnityEngine.Random.Range(0f, 100f) <= obstacleSpawnChancePercent)
            {
                // Mark that we should spawn an obstacle on the next buildnig pair
                pendingObstacleSpawn = true;
            }
        }

        // Schedule the next tick
        Invoke("TickObstacleSpawner", obstacleSpawnerTickSeconds);
    }

    List<GameObject> SpawnObstacles(int row, int maxCount = 8)
    {
        // Initialize the obstacles array
        List<GameObject> obstacles = new List<GameObject>();

        // Loop through the number of obstacles to spawn
        for (int i = 0; i < maxCount; i++)
        {
            // Determine based on the probability if we should spawn an obstacle after the first one
            if (i == 0 || UnityEngine.Random.Range(0f, 100f) <= obstacleSpawnChancePercent)
            {
                // Get an obstacle from the pool
                GameObject obstacle = obstaclesPool.pool.Get();

                // Initialize the obstacle's position
                Vector3 position = Vector3.zero;

                // Set the obstacle's x position randomly based on the spawn range
                position.x = UnityEngine.Random.Range(-screenWidthFromCenter * (obstacleSpawnRangePercent.x / 100f), screenWidthFromCenter * (obstacleSpawnRangePercent.x / 100f));

                // Set the obstacle's y position based on the spawn range
                position.y = UnityEngine.Random.Range(-screenHeightFromCenter * (obstacleSpawnRangePercent.y / 100f), screenHeightFromCenter * (obstacleSpawnRangePercent.y / 100f));

                // Set the obstacle's z position based on the row
                position.z = row * (spaceInBetweenBuildings + buildingScale.z);

                // Set the obstacle's position
                obstacle.transform.position = position;

                // Set the obstacle's scale randomly
                obstacle.transform.localScale = new Vector3(
                    UnityEngine.Random.Range(obstacleMinScale.x, obstacleMaxScale.x),
                    UnityEngine.Random.Range(obstacleMinScale.y, obstacleMaxScale.y), 10f);

                // Add the obstacle to the list
                obstacles.Add(obstacle);
            }
        }

        // Return the obstacles array
        return obstacles;
    }

    GameObject SpawnBuilding(Vector3 position)
    {
        // Retrieve a building game object from the pool
        GameObject building = buildingsPool.pool.Get();

        // Set the building's scale
        building.transform.localScale = buildingScale;

        // Set the building's position
        building.transform.position = position;

        // Return the building game object
        return building;
    }

    GameObject SpawnBuilding(int column, int row)
    {
        // Retrieve a modifiable position for the building based on parent
        Vector3 position = transform.position;

        // Set the x position of the building based on the iteration index such that 0 = left and 1 = right
        // Multiply the screen width by the separation percent to determine distance from center of screen
        position.x = (column == -1 ? -screenWidthFromCenter : column == 1 ? screenWidthFromCenter : 0f) * (buildingSeparationPercent / 100f);

        // Set the z position based on the position index which multiplies the space in between buildings by the position index
        position.z = row * (spaceInBetweenBuildings + buildingScale.z);

        // Offset the position by the building offset
        position += buildingOffset;

        // Determine if we should spawn obstacles for this row
        if (pendingObstacleSpawn)
        {
            // Spawn the obstacles for this row
            List<GameObject> obstacles = SpawnObstacles(row);

            // Track the obstacles in the scrolling obstacles list
            foreach (GameObject obstacle in obstacles) scrollingObstacles.Add(obstacle);

            // Mark that we shouldn't spawn an obstacle on the next row
            pendingObstacleSpawn = false;
        }

        // Return the spawned building at the calculated position
        return SpawnBuilding(position);
    }

    void SpawnBuildingPair(int row)
    {
        // Since this is a pair of buildings, we must spawn twice
        for (int iteration = 0; iteration < 2; iteration++)
        {
            // Spawn a building at the appropriate column and row
            GameObject building = SpawnBuilding(iteration == 0 ? -1 : 1, row);

            // Add the building to the appropriate side buildings list
            if (iteration == 0)
            {
                leftSideBuildings.Add(building);
            }
            else
            {
                rightSideBuildings.Add(building);
            }

            // Track the furthest trail anchor to represent the end of the buildings trail
            if (trailPosition != null && building.transform.position.z > trailPosition.z)
            {
                // Keep the x position at 0f as it represents the center of the trail path
                trailPosition = new Vector3(0f, building.transform.position.y, building.transform.position.z);
            }
        }
    }

    public void SpawnDeadEnd(bool blockLeft, bool blockRight)
    {
        // Determine the last buildings on the left and right sides
        GameObject lastLeftBuilding = leftSideBuildings[leftSideBuildings.Count - 1];
        GameObject lastRightBuilding = rightSideBuildings[rightSideBuildings.Count - 1];

        // Determine if we are blocking the left side
        if (blockLeft)
        {
            // Spawn an additional building behind the last left-side building
            leftSideBuildings.Add(
                SpawnBuilding(
                    new Vector3(
                        lastLeftBuilding.transform.position.x,
                        lastLeftBuilding.transform.position.y,
                        lastLeftBuilding.transform.position.z + (spaceInBetweenBuildings + buildingScale.z)
                    )
                )
            );
        }

        // Determine if we are blocking the right side
        if (blockRight)
        {
            // Spawn an additional building behind the last right-side building
            rightSideBuildings.Add(
                SpawnBuilding(
                    new Vector3(
                        lastRightBuilding.transform.position.x,
                        lastRightBuilding.transform.position.y,
                        lastRightBuilding.transform.position.z + (spaceInBetweenBuildings + buildingScale.z)
                    )
                )
            );
        }

        // Spawn another building in the left side buildings to cover the back left
        leftSideBuildings.Add(
                SpawnBuilding(
                    new Vector3(
                        lastLeftBuilding.transform.position.x,
                        lastLeftBuilding.transform.position.y,
                        lastLeftBuilding.transform.position.z + ((spaceInBetweenBuildings + buildingScale.z) * 2f)
                    )
                )
            );

        // Spawn another building the in the right side buildings to cover the back right
        rightSideBuildings.Add(
                SpawnBuilding(
                    new Vector3(
                        lastRightBuilding.transform.position.x,
                        lastRightBuilding.transform.position.y,
                        lastRightBuilding.transform.position.z + ((spaceInBetweenBuildings + buildingScale.z) * 2f)
                    )
                )
            );

        // Spawn a building in the middle of the trail to cover the back middle
        leftSideBuildings.Add(
                SpawnBuilding(
                    new Vector3(
                        0f,
                        lastLeftBuilding.transform.position.y,
                        lastLeftBuilding.transform.position.z + ((spaceInBetweenBuildings + buildingScale.z) * 2f)
                    )
                )
            );

        // Disable spawning to signify the end of this trail with this dead end
        isSpawning = false;
    }

    void FixedUpdate()
    {
        // Ensure we are scrolling
        if (isScrolling)
        {
            // Determine various properties for world generation and movement
            float deSpawnCutoff = despawnAfterRows * (spaceInBetweenBuildings + buildingScale.z);

            // Determine if we have some obstacles
            if (scrollingObstacles.Count > 0)
            {
                // Loop through the obstacles and move them forward
                List<GameObject> obstaclesToDespawn = null;
                foreach (GameObject obstacle in scrollingObstacles)
                {
                    // Move the obstacle forward with the building speed
                    obstacle.transform.position -= Vector3.forward * buildingScrollSpeed * Time.deltaTime;

                    // Determine if this obstacle should be despawned
                    if (obstacle.transform.position.z < -deSpawnCutoff)
                    {
                        // Initialize the obstacles to remove list
                        if (obstaclesToDespawn == null) obstaclesToDespawn = new List<GameObject>();

                        // Mark this obstacle for removal
                        obstaclesToDespawn.Add(obstacle);
                    }
                }

                // Remove the obstacles that should be despawned
                if (obstaclesToDespawn != null)
                {
                    foreach (GameObject obstacle in obstaclesToDespawn)
                    {
                        // Remove the obstacle from the scrolling obstacles list
                        scrollingObstacles.Remove(obstacle);

                        // Release the obstacle from to the pool
                        obstaclesPool.pool.Release(obstacle);
                    }
                }
            }

            // Determine if we have some active buildings
            if (leftSideBuildings.Count > 0)
            {
                // Determine the de-spawn cutoff based on one space in between building times the z scale aka. 1 column
                GameObject despawnBuildingLeft = null;
                GameObject despawnBuildingRight = null;

                // Iterate through each building in the active buildings list and move it along the z axis backwards based on the scroll speed
                int interationTarget = Mathf.Max(leftSideBuildings.Count, rightSideBuildings.Count);
                for (int i = 0; i < interationTarget; i++)
                {
                    // Ensure the left building index is valid and in range
                    if (i > -1 && i <= leftSideBuildings.Count - 1)
                    {
                        // Retrieve the left buildnig
                        // Move it along the z axis to simulate "scrolling" effect
                        // Mark it for despawn if it is behind the cutoff
                        GameObject leftBuilding = leftSideBuildings[i];
                        leftBuilding.transform.position += Vector3.back * buildingScrollSpeed * Time.fixedDeltaTime;
                        if (leftBuilding.transform.position.z < -deSpawnCutoff)
                        {
                            despawnBuildingLeft = leftBuilding;
                        }
                    }

                    // Ensure the right building index is valid and in range
                    if (i > -1 && i <= rightSideBuildings.Count - 1)
                    {
                        // Retrieve the left buildnig
                        // Move it along the z axis to simulate "scrolling" effect
                        // Mark it for despawn if it is behind the cutoff
                        GameObject rightBuilding = rightSideBuildings[i];
                        rightBuilding.transform.position += Vector3.back * buildingScrollSpeed * Time.fixedDeltaTime;
                        if (rightBuilding.transform.position.z < -deSpawnCutoff)
                        {
                            despawnBuildingRight = rightBuilding;
                        }
                    }
                }

                // Despawn the left building if it is behind the cutoff
                if (despawnBuildingLeft != null)
                {
                    leftSideBuildings.Remove(despawnBuildingLeft);
                    buildingsPool.pool.Release(despawnBuildingLeft);
                }

                // Despawn the right building if it is behind the cutoff
                if (despawnBuildingRight != null)
                {
                    rightSideBuildings.Remove(despawnBuildingRight);
                    buildingsPool.pool.Release(despawnBuildingRight);
                }

                // Spawn another building pair if we successfully despawned both left and right side
                if (isSpawning && despawnBuildingLeft != null && despawnBuildingRight != null)
                {
                    SpawnBuildingPair(numBuildingsPerRow - despawnAfterRows);
                }

                // Move the trail anchor along the z axis to track the furthest building
                if (trailPosition != null)
                {
                    trailPosition += Vector3.back * buildingScrollSpeed * Time.fixedDeltaTime;
                    trailObject.transform.position = trailPosition;
                }
            }
        }
    }
}
