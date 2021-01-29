using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PathFindingAsteria
{ 

    // Tell the editor to override the editor for the types of AIWaypoint Network objects
    [CustomEditor(typeof(AIWaypointNetwork))]
    public class AIWaypointNetworkEditor : Editor
    {
        /// <summary>
        ///     Overriding the default inspector to customize the look of it 
        /// </summary>
        public override void OnInspectorGUI()
        {
            // The order we create them in will be how they show up
            AIWaypointNetwork network = (AIWaypointNetwork)target;

            network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup( label: "Display Mode" , selected: network.DisplayMode );
            if (network.DisplayMode == PathDisplayMode.Paths )
            {
                network.UIStart     = EditorGUILayout.IntSlider(label: "Waypoint Start",    value: network.UIStart, leftValue: 0, rightValue: network.Waypoints.Count - 1 );
                network.UIEnd       = EditorGUILayout.IntSlider(label: "Waypoint End",      value: network.UIEnd,   leftValue: 0, rightValue: network.Waypoints.Count - 1 );
            }

            //base.OnInspectorGUI();
            DrawDefaultInspector();

        }


        // This class goes in the Editor folder.. which is generic no matter where it is placed
        // It will override the editor area and allow me to customize the editor for certain classes

        private void OnSceneGUI()
        {
            AIWaypointNetwork network = (AIWaypointNetwork)target;

            if (network.Waypoints.Count == 0) return;
             
            // Go through all waypoints of the waypoint network.
            for(int i = 0; i < network.Waypoints.Count; i++ )
            {
                // make sure we are not null
                if (network.Waypoints[i] != null)
                {
                    // Need to use a style to force the white text on the dark background
                    GUIStyle styleme = new GUIStyle();
                    styleme.normal.textColor = Color.white;

                    Handles.Label(network.Waypoints[i].position, "Waypoint " + i.ToString(), styleme);
                }
            }


            // Differentiate which display mode we are in
            if ( network.DisplayMode == PathDisplayMode.Connections )
            {

                // Want an additional one to store a duplicate of the first... 
                // this allows us to loop back from the final to the first
                Vector3[] linepoints = new Vector3[network.Waypoints.Count + 1];

                for(int i = 0; i <= network.Waypoints.Count; i++)
                {
                    int index = i != network.Waypoints.Count ? i : 0;

                    if (network.Waypoints[index] != null)
                    {
                        linepoints[i] = network.Waypoints[index].position;
                    }
                    else
                    {
                        linepoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
                    }
                }

                // Draw a line between all of the points in my array
                Handles.color = Color.cyan;
                Handles.DrawPolyLine(linepoints);
            }
            else if (network.DisplayMode == PathDisplayMode.Paths)
            {
                //NavMeshPath path    = new NavMeshPath();

                //if( network.Waypoints[ network.UIStart ] != null && network.Waypoints[ network.UIEnd ] != null ) 
                //{

                //    Vector3 from        = network.Waypoints[ network.UIStart ].position;
                //    Vector3 to          = network.Waypoints[ network.UIEnd   ].position;

                //    if (from != to)
                //    {
                //        // Static navmesh in a scene - only can be one navmesh in a scene
                //        NavMesh.CalculatePath( sourcePosition: from, targetPosition: to, areaMask: NavMesh.AllAreas, path: path );
                //        Handles.color = Color.yellow;
                //        Handles.DrawPolyLine( path.corners );
                //    }
                //}
            }
        }

}
}
