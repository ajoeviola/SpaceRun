using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ComponentPool : MonoBehaviour
{
    // Define public inspector variables
    [Header("Pool Properties")]
    public int defaultSize = 10;
    public int maxSize = 100;
    public GameObject[] componentPrefabs;

    [HideInInspector]
    public ObjectPool<GameObject> pool;

    [HideInInspector]
    public List<GameObject> activeComponents;

    // Use Awake so that the pool is created before the Start() method is called
    void Awake()
    {
        // Initialize the active components list
        activeComponents = new List<GameObject>();

        // Initialize the pool
        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                // Choose a random prefab from the list of prefabs
                GameObject prefab = componentPrefabs[Random.Range(0, componentPrefabs.Length)];

                // Instantiate a component under current transform
                GameObject component = Instantiate(prefab);
                component.transform.parent = transform;
                return component;
            },
            actionOnGet: (obj) =>
            {
                // Add the component to the active components list and set it to active
                activeComponents.Add(obj);
                obj.SetActive(true);
            },
            actionOnRelease: (obj) =>
            {
                // Remove the component from the active components list and set it to inactive
                activeComponents.Remove(obj);
                obj.SetActive(false);
            },
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: defaultSize,
            maxSize: maxSize
        );
    }
}
