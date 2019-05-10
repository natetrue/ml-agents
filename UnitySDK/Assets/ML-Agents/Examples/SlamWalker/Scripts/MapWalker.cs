using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

class PositionTuple {
    public Vector3 startPosition;
    public Vector2 endPosition;

    public PositionTuple(Vector3 sp, Vector2 ep)
    {
        this.startPosition = sp;
        this.endPosition = ep;
    }
}

[RequireComponent(typeof(CharacterController))]
public class MapWalker : MonoBehaviour
{
    private CharacterController charCon;

    private LinkedList<PositionTuple> positionQueue = new LinkedList<PositionTuple>();

    private HashSet<Vector2> visitedPositions = new HashSet<Vector2>();
    private HashSet<Vector2> consideredPositions = new HashSet<Vector2>();

    private StreamWriter fileWriter;

    public float granularity = 0.3f;

    public int stepsPerFrame = 50;

    // Start is called before the first frame update
    void Start()
    {
        charCon = GetComponent<CharacterController>();    
        enqueueTravelFrom(transform);  
        fileWriter = new StreamWriter("locations.csv", false);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < stepsPerFrame; i++) {
            // Check if done
            if (positionQueue.Count == 0) {
                Debug.LogWarning("Done!");
                fileWriter.Close();
                this.enabled = false;
                return;
            }

            // Grab one position from the queue
            var p = positionQueue.Last.Value;
            positionQueue.RemoveLast();
            var dest = p.endPosition;
            var dest3 = new Vector3(dest.x, p.startPosition.y, dest.y);

            // Warp to that position
            charCon.enabled = false;
            transform.position = p.startPosition;
            transform.LookAt(dest3);
            charCon.enabled = true;

            // Attempt to move there
            charCon.Move(dest3 - transform.position);

            // Check if succeeded
            if ((dest3 - transform.position).magnitude < granularity/2) {
                charCon.Move(new Vector3(0, -100, 0));
                var v = new Vector2(transform.position.x, transform.position.z);
                v.x = Mathf.Round(v.x / granularity) * granularity;
                v.y = Mathf.Round(v.y / granularity) * granularity;
                // Debug.LogWarning(String.Format("Visited {0}, {1} elev {2}", v.x, v.y, transform.position.y));
                visitedPositions.Add(v);
                fileWriter.WriteLine(String.Format("{0},{1},{2}", v.x, transform.position.y, v.y));
                enqueueTravelFrom(transform);
            }
        }
    }

    private void enqueueTravelFrom(Transform t) {
        enqueuePosition(t, new Vector2(t.position.x + granularity, t.position.z));
        enqueuePosition(t, new Vector2(t.position.x - granularity, t.position.z));
        enqueuePosition(t, new Vector2(t.position.x, t.position.z + granularity));
        enqueuePosition(t, new Vector2(t.position.x, t.position.z - granularity));
    }

    private void enqueuePosition(Transform t, Vector2 v) {
        v.x = Mathf.Round(v.x / granularity) * granularity;
        v.y = Mathf.Round(v.y / granularity) * granularity;
        if (!consideredPositions.Contains(v))
            positionQueue.AddLast(new PositionTuple(t.position, v));
            consideredPositions.Add(v);
    }
}
