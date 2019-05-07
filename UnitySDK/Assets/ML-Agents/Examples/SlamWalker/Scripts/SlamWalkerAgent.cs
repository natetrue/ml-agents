﻿using System;
using System.Collections;
using System.Collections.Generic;
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

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        myArea = area.GetComponent<SlamWalkerArea>();
        rayPer = GetComponent<RayPerception>();
        charCon = GetComponent<CharacterController>();
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
            lastMoveCollisionFlags = charCon.Move(
                transform.forward * Mathf.Clamp(act[0], -1f, 1f) 
                + transform.right * Mathf.Clamp(act[1], -1f, 1f) 
                + transform.up * -0.5f
                );
            
            rotateAmt = Mathf.Clamp(act[2], -1f, 1f) * 90f;
            transform.Rotate(transform.up, rotateAmt);
        }

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        int spawnPoint = Random.Range(1, area.transform.childCount);
        Transform spawnTransform = area.transform.GetChild(spawnPoint);

        Debug.LogWarning(String.Format("Choosing spawn point {0} at {1}", spawnPoint, spawnTransform), spawnTransform);

        var xRange = spawnTransform.localScale.x / 2.1f;
        var zRange = spawnTransform.localScale.z / 2.1f;

        charCon.enabled = false;
        transform.position = new Vector3(Random.Range(-xRange, xRange), 1f, Random.Range(-zRange, zRange))
                                            + spawnTransform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
        charCon.enabled = true;

        // Immediately move down to the ground
        charCon.Move(transform.up * -100);

        // And wander in some direction and turn some other direction
        charCon.Move(transform.forward * Random.Range(0.0f, 5.0f));
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

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
