using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;

[RequireComponent(typeof(CharacterController))]
public class SlamWalkerAgent : Agent
{
    public GameObject area;
    private SlamWalkerArea myArea;
    private RayPerception rayPer;
    private CharacterController charCon;
    public bool useVectorObs;
    
    private CollisionFlags lastMoveCollisionFlags;

    public TextAsset locationList;
    private List<Vector3> resetLocations;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        myArea = area.GetComponent<SlamWalkerArea>();
        rayPer = GetComponent<RayPerception>();
        charCon = GetComponent<CharacterController>();


        string[] fLines = Regex.Split ( locationList.text, "\n|\r|\r\n" );
        resetLocations = new List<Vector3>(fLines.Length);
        var commaRE = new Regex(",");
        for ( int i=0; i < fLines.Length; i++ ) 
        { 
            string[] values = commaRE.Split ( fLines[i] );
            var loc = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            resetLocations.Add(loc);
        }
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            AddVectorObs(transform.position - area.transform.position);
            AddVectorObs(transform.eulerAngles.y / 180f * Mathf.PI);
            // const float rayDistance = 35f;
            // float[] rayAngles = {20f, 90f, 160f, 45f, 135f, 70f, 110f};
            // float[] rayAngles1 = {25f, 95f, 165f, 50f, 140f, 75f, 115f};
            // float[] rayAngles2 = {15f, 85f, 155f, 40f, 130f, 65f, 105f};

            // string[] detectableObjects = {"block", "wall", "goal", "switchOff", "switchOn", "stone"};
            // AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            // AddVectorObs(rayPer.Perceive(rayDistance, rayAngles1, detectableObjects, 0f, 5f));
            // AddVectorObs(rayPer.Perceive(rayDistance, rayAngles2, detectableObjects, 0f, 10f));
            // AddVectorObs(switchLogic.GetState());
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, 10f)) {
                AddVectorObs(hit.distance);
            }
            else {
                AddVectorObs(10);
            }
            AddVectorObs(((lastMoveCollisionFlags & CollisionFlags.Sides) != 0) ? 1 : 0);
        }
    }

    public void MoveAgent(float[] act)
    {
        // Action parameters:  move distance forward in meters, move distance right in meters, rotation / 90 degrees
        var rotateAmt = 0f;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            var prevPos = transform.position;

            lastMoveCollisionFlags = charCon.Move(
                transform.forward * Mathf.Clamp(act[0], -1f, 1f) 
                + transform.right * Mathf.Clamp(act[1], -1f, 1f) 
                + transform.up * -0.5f
                );

            // Detect a fall and treat it like a collision
            var prefallPosition = transform.position;
            charCon.Move(transform.up * -100f);
            if ((transform.position - prefallPosition).magnitude > 0.5) {
                // Fell! Restore old position
                charCon.enabled = false;
                transform.position = prevPos;
                charCon.enabled = true;
                lastMoveCollisionFlags = CollisionFlags.Sides;
            }
            
            rotateAmt = Mathf.Clamp(act[2], -1f, 1f) * 90f;
            transform.Rotate(transform.up, rotateAmt);            
        }

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (vectorAction[3] > 0.5f) {
            AgentReset();
        }
        AddReward(-1f / agentParameters.maxStep);
        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        int spawnPoint = Random.Range(1, resetLocations.Count);
        Vector3 spawnVec = resetLocations[spawnPoint];

        Debug.LogWarning(String.Format("Choosing spawn point {0} at {1}", spawnPoint, spawnVec));

        charCon.enabled = false;
        transform.position = spawnVec;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        charCon.enabled = true;

        // Immediately move down to the ground
        charCon.Move(transform.up * -100);

        // And wander in some direction and turn some other direction
        charCon.Move(transform.forward * Random.Range(0.0f, 1.0f));
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        // transform.rotation = Quaternion.Euler(new Vector3(0f, 30f, 0f));

        // switchLogic.ResetSwitch(items[1], items[2]);
        // myArea.CreateStoneSlamWalker(1, items[3]);
        // myArea.CreateStoneSlamWalker(1, items[4]); 
        // myArea.CreateStoneSlamWalker(1, items[5]);
        // myArea.CreateStoneSlamWalker(1, items[6]);
        // myArea.CreateStoneSlamWalker(1, items[7]);
        // myArea.CreateStoneSlamWalker(1, items[8]);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.CompareTag("goal"))
        // {
        //     SetReward(2f);
        //     Done();
        // }
    }

    public override void AgentOnDone()
    {

    }
}
