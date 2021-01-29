using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PathDisplayMode {  None, Connections, Paths };

public class AIWaypointNetwork : MonoBehaviour
{
    // Hide these items in the default inspector so we can make them ourselves.
    [HideInInspector]
    public PathDisplayMode DisplayMode = PathDisplayMode.Connections;
    [HideInInspector]
    public int UIStart = 0;
    [HideInInspector]
    public int UIEnd = 0;

    // Can set a waypoints array for character random movement
    // Assign it to a game object and have multiple game objects that will 
    // have a different order of waypoints thus making it seems like enemiies have random movement.
    public List<GameObject> WaypointObjects = new List<GameObject>();

    /// <summary>
    ///     Grab a list of all the transforms that we want
    /// </summary>
    public List<Transform> Waypoints
    {
        get
        {
            List<Transform> temp = new List<Transform>();

            if (WaypointObjects.Count != 0)
            {
                foreach (GameObject go in WaypointObjects)
                {
                    temp.Add(go.transform);
                }
            }
            return temp;
        }
    }

}
