using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : Singleton<CloudSpawner>
{
    // Public inspector variables
    public ComponentPool cloudsPool;
    public int numberOfClouds = 40;
    public float movementSpeed = 0.5f;
    public float spawnWidthRangeMinPercent;
    public float spawnWidthRangeMaxPercent;
    public float spawnHeightRangeMinPercent;
    public float spawnHeightRangeMaxPercent;
    public float spawnDistanceRangeZMin;
    public float spawnDistanceRangeZMax;
    public Vector3 cloudRandomScaleMin;
    public Vector3 cloudRandomScaleMax;

    // Private variables
    private float screenWidthFromCenter;
    private float screenHeightFromCenter;
    private List<GameObject> activeClouds;

    // Start is called before the first frame update
    void Start()
    {
        // Cache the screen height and width from the center
        screenHeightFromCenter = Camera.main.orthographicSize;
        screenWidthFromCenter = screenHeightFromCenter * Camera.main.aspect;

        // Initialize the active clouds list
        activeClouds = new List<GameObject>();
    }

    public void Reset()
    {
        // Loop through all the clouds to remove
        foreach (GameObject cloud in activeClouds)
        {
            // Return the cloud to the pool
            cloudsPool.pool.Release(cloud);
        }

        // Clear the active clouds list
        activeClouds.Clear();

        // Spawn the initial clouds
        for (int i = 0; i < numberOfClouds; i++) InitialSpawnCloud();
    }

    void DespawnCloud(GameObject cloud)
    {
        // Remove the cloud from the active clouds list
        activeClouds.Remove(cloud);

        // Return the cloud to the pool
        cloudsPool.pool.Release(cloud);
    }

    void SpawnCloud()
    {
        // Retrieve a cloud from the pool and retrieve its transform animator
        GameObject cloud = cloudsPool.pool.Get();
        TransformAnimator animator = cloud.GetComponent<TransformAnimator>();

        // Set the cloud's position
        Vector3 spawnerPosition = transform.position;
        float randomDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        cloud.transform.position = transform.position + new Vector3(
            randomDirection * Random.Range(screenWidthFromCenter * (spawnWidthRangeMinPercent / 100f), screenWidthFromCenter * (spawnWidthRangeMaxPercent / 100f)),
            randomDirection * Random.Range(screenHeightFromCenter * (spawnHeightRangeMinPercent / 100f), screenHeightFromCenter * (spawnHeightRangeMaxPercent / 100f)),
            Random.Range(spawnDistanceRangeZMin, spawnDistanceRangeZMax)
        );

        // Set the cloud's instant scale to 0 and animate it to the desired scale
        animator.SetInstantScale(Vector3.zero);
        animator.targetScale = new Vector3(
            Random.Range(cloudRandomScaleMin.x, cloudRandomScaleMax.x),
            Random.Range(cloudRandomScaleMin.y, cloudRandomScaleMax.y),
            Random.Range(cloudRandomScaleMin.z, cloudRandomScaleMax.z)
        );

        // Make the cloud rotation point to the left on the screen if it is on the right and vice versa
        cloud.transform.rotation = Quaternion.Euler(0, cloud.transform.position.x > 0 ? -180 : 0, 0);

        // Add the cloud to the active clouds list
        activeClouds.Add(cloud);
    }

    void InitialSpawnCloud()
    {
        // Retrieve a cloud from the pool and retrieve its transform animator
        GameObject cloud = cloudsPool.pool.Get();
        TransformAnimator animator = cloud.GetComponent<TransformAnimator>();

        // Set the cloud's position
        Vector3 spawnerPosition = transform.position;
        float randomDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        cloud.transform.position = transform.position + new Vector3(
            randomDirection * Random.Range(-screenWidthFromCenter * (spawnWidthRangeMaxPercent / 100f), screenWidthFromCenter * (spawnWidthRangeMaxPercent / 100f)),
            randomDirection * Random.Range(screenHeightFromCenter * (spawnHeightRangeMinPercent / 100f), screenHeightFromCenter * (spawnHeightRangeMaxPercent / 100f)),
            Random.Range(spawnDistanceRangeZMin, spawnDistanceRangeZMax)
        );

        // Set the cloud's instant scale to 0 and animate it to the desired scale
        animator.SetInstantScale(Vector3.zero);
        animator.targetScale = new Vector3(
            Random.Range(cloudRandomScaleMin.x, cloudRandomScaleMax.x),
            Random.Range(cloudRandomScaleMin.y, cloudRandomScaleMax.y),
            Random.Range(cloudRandomScaleMin.z, cloudRandomScaleMax.z)
        );

        // Make the cloud rotation point to the left on the screen if it is on the right and vice versa
        cloud.transform.rotation = Quaternion.Euler(0, Random.value > 0.5 ? -180 : 0, 0);

        // Add the cloud to the active clouds list
        activeClouds.Add(cloud);
    }

    void FixedUpdate()
    {
        // Check if we have some active clouds
        if (activeClouds.Count > 0)
        {
            // Initiialize a list of clouds to remove
            List<GameObject> cloudsToRemove = null;
            float despawnPositionXMax = transform.position.x + screenWidthFromCenter * (spawnWidthRangeMaxPercent / 100f);

            // Loop through all the active clouds
            for (int i = 0; i < activeClouds.Count; i++)
            {
                // Get the cloud
                GameObject cloud = activeClouds[i];

                // Determine the direction of the cloud movement based on whether it's y rotation is 0 or 180
                float cloudMovementDirection = cloud.transform.rotation.eulerAngles.y == 0 ? 1 : -1;

                // Move the cloud on the x axis towards the cloud movement direction
                cloud.transform.position = new Vector3(
                    cloud.transform.position.x + (cloudMovementDirection * movementSpeed * Time.deltaTime),
                    cloud.transform.position.y,
                    cloud.transform.position.z
                );

                // Determine if the cloud has moved past the maxiimum spawn width on either side to the left or right to despawn it
                if (Mathf.Abs(cloud.transform.position.x) > despawnPositionXMax)
                {
                    // If we haven't yet created the list of clouds to remove, create it
                    if (cloudsToRemove == null) cloudsToRemove = new List<GameObject>();

                    // Add the cloud to the list of clouds to remove
                    cloudsToRemove.Add(cloud);
                }
            }

            // If we have some clouds to remove, remove them
            if (cloudsToRemove != null)
            {
                // Loop through all the clouds to remove
                foreach (GameObject cloud in cloudsToRemove)
                {
                    // Despawn the cloud
                    DespawnCloud(cloud);

                    // Spawn a new cloud to make up for the one that was despawned
                    SpawnCloud();
                }
            }
        }
    }
}
