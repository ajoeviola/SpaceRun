using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
        The spawnermanager chooses when to spawn either an enemy or an obstacle, and chooses their position randomly 
     
     */


public class SpawnerManager : Singleton<SpawnerManager>
{
    //store all the possible spawning directions
    public static Vector3Int up = new Vector3Int(0, 3, 0);
    public static Vector3Int bot = new Vector3Int(0, -3, 0);
    public static Vector3Int left = new Vector3Int(5, 0, 0);
    public static Vector3Int right = new Vector3Int(-5, 0, 0);
    public static Vector3Int upleft = new Vector3Int(5, 3, 0);
    public static Vector3Int botlef = new Vector3Int(5, -3, 0);
    public static Vector3Int upright = new Vector3Int(-5, 3, 0);
    public static Vector3Int botright = new Vector3Int(-5, -3, 0);
    public static Vector3Int middle = new Vector3Int(0, 0, 0);



    public SpawnObstacle obstaclespawner;
    public SpawnEnemy enemyspawner;

    private void Start()
    {
        //spawn an obstacle at positon x,y,z(0,0,50)!
        obstaclespawner.spawn(new Vector3(0, 0, 50));

        
    }
    // Update is called once per frame
    void Update()
    {
        //Chooses to either spawn an obstacle or an enemy randomly(not implemented yet)
        
    }


}
