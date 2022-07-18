using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    //the prefab of our obstacle (for the future we can create an array of different obstacles to randomly select from)
    public GameObject prefab;


    public void spawn(Vector3 startposition)
    {

        //create the object at our start position 
        GameObject enemy = Instantiate(prefab, startposition, Quaternion.identity) as GameObject;
    }
}
