using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MovingObject
{
    // Use this for initialization
    protected override void OnCantMove<T>(T component)
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //Check if we are running either in the Unity editor or in a standalone build.

        // movement and aiming
        AttemptMove<Wall> (Vector2.zero);        
    }
}
