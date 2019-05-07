using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SlamWalkerArea : Area
{
    public GameObject pyramid;
    public GameObject stoneSlamWalker;
    public GameObject[] spawnAreas;
    public int numPyra;
    public float range;

    // public void CreateSlamWalker(int numObjects, int spawnAreaIndex)
    // {
    //     CreateObject(numObjects, pyramid, spawnAreaIndex);
    // }
    
    // public void CreateStoneSlamWalker(int numObjects, int spawnAreaIndex)
    // {
    //     CreateObject(numObjects, stoneSlamWalker, spawnAreaIndex);
    // }
    
    // private void CreateObject(int numObjects, GameObject desiredObject, int spawnAreaIndex)
    // {
    //     for (var i = 0; i < numObjects; i++)
    //     {
    //         var newObject = Instantiate(desiredObject, Vector3.zero, 
    //             Quaternion.Euler(0f, 0f, 0f), transform);
    //         PlaceObject(newObject, spawnAreaIndex);
    //     }
    // }

    public void PlaceObject(GameObject objectToPlace, Transform spawnTransform)
    {
        var xRange = spawnTransform.localScale.x / 2.1f;
        var zRange = spawnTransform.localScale.z / 2.1f;
        
        objectToPlace.transform.position = new Vector3(Random.Range(-xRange, xRange), 2f, Random.Range(-zRange, zRange)) 
                                            + spawnTransform.position;
    }

    public void CleanSlamWalkerArea()
    {

    }

    public override void ResetArea()
    {
        
    }
}
