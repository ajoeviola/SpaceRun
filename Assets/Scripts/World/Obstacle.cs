using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
        This script  controls the obstacles movement
     
     */
public class Obstacle : MonoBehaviour
{

    public int speed = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //move our object
        Move(new Vector3(this.transform.position.x, this.transform.position.y, 0));
    }   

    //moves the object across the screen
    public void Move(Vector3 direction)
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, direction, Time.deltaTime * speed);
    }

    
}
