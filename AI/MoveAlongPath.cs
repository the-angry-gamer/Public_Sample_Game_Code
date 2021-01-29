using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Asteria
{
    public class MoveAlongPath
    {

        #region Private Declarations

        bool checkYDiff            = false;

        /// <summary> All of our path nodes                     </summary>
        List<Vector3> pathNodes    = new List<Vector3>();

        /// <summary> the current integer of node we are on     </summary>
        int current = 0;

        /// <summary> The current node we are going towards     </summary>
        Vector3 currentNode;

        #endregion


        #region Properties

        /// <summary>
        ///     The distance to the next node 
        ///     before triggering an update of 
        ///     the target node
        /// </summary>
        public float DistanceBufferXZ
        {
            get;
            set;
        } = 1.0f;

        public float DistanceBufferY
        {
            get;
            set;
        } = 1.0f;

        /// <summary>
        ///     A buffer to the end point to consider  us complete
        /// </summary>
        public float EndBuffer
        {
            get;
            set;
        } = 1.0f;

        
        /// <summary>
        ///     the target node
        /// </summary>
        internal Vector3 TargetNode
        {
            get
            {
                return pathNodes[pathNodes.Count - 1];
            }
        }
        /// <summary>
        ///     The full set of nodes that make up the path
        ///     we are going to move along
        /// </summary>
        internal List<Vector3> PathNodes
        {
            set
            {
                current = 0;
                pathNodes = value;
            }
            get
            {
                return pathNodes;
            }
        }


        /// <summary>
        ///     Skip this many nodes when iterating
        /// </summary>
        public int iterationCount
        {
            get;
            set;
        } = 1;


        /// <summary>
        ///     General Constructor - does not check y differences
        /// </summary>
        /// <param name="distanceBufferXZ">     The max distance to our next node before auto iterating </param>
        /// <param name="path">                 The collection of nodes that make up our path           </param>        
        public MoveAlongPath(float distanceBufferXZ,  List<Vector3> path)
        { 
            DistanceBufferXZ    = distanceBufferXZ;
            checkYDiff          = false;
            PathNodes           = path;
            if (path.Count > 0)
            {
                currentNode     = path[0];
            }
            Completed = false;
        }



        /// <summary>
        ///     General Constructor - turns on the Y coord check
        /// </summary>
        /// <param name="distanceBufferXZ">     The max distance to our next node before auto iterating </param>
        /// <param name="path">                 The collection of nodes that make up our path           </param>
        /// <param name="distanceBuffY">        The distance buffer of the y axis                       </param>
        public MoveAlongPath(float distanceBufferXZ, float distanceBuffY, List<Vector3> path)
        {
            DistanceBufferXZ    = distanceBufferXZ;
            DistanceBufferY     = distanceBuffY;
            checkYDiff          = true;
            PathNodes           = path;
            if (path.Count > 0)
            {
                currentNode     = path[0];
            }
            Completed = false;
        }


        #region public Properties

        /// <summary>
        ///     A bit to tell us if we have completed our path
        /// </summary>
        public bool Completed
        {
            get;
            private set;
        }


        /// <summary>
        ///     The current target node
        /// </summary>
        public Vector3 CurrentTarget
        {
            get { return currentNode; }
        }
    
        /// <summary>
        ///     Gets the amount of path nodes that we 
        ///     have.
        /// </summary>
        public int PathCount
        {
            get
            {
                return pathNodes.Count;
            }
        }

        #endregion

        #endregion


        #region Public functions

        /// <summary>
        ///     Get our distance to the next node in 
        ///     our path
        /// </summary>
        /// <param name="location"> Where the object is currently located   </param>
        /// <returns>
        ///     The distance in float to the next object
        /// </returns>
        public float distanceToNextNode( Vector3 location )
        {
            return Vector3.Distance(location, currentNode);
        }

        /// <summary>
        ///     Check if we are in within range of our next node and 
        ///     move if we are
        /// </summary>
        /// <param name="pos">  the position to check the distance of to the next node</param>
        /// <returns>
        ///     The current node we have targeted
        /// </returns>
        public Vector3 checkTarget(Vector3 pos)
        {
            checkCompletion(pos);
            if ( distanceToNextNode(pos) <  DistanceBufferXZ || isWithinY(pos) )
            {
                Completed = TargetNode == currentNode; // We have made it within range of the last path point
                nextTarget();
            }

            return currentNode;
        }

        /// <summary>
        ///     Check if we are within the y range of our target
        /// </summary>
        /// <param name="pos">  Our position in the world   </param>
        /// <returns>
        ///     a bool if we are in range     
        /// </returns>
        bool isWithinY(Vector3 pos)
        {
            if ( !checkYDiff) { return false; }
            if ( (int)pos.x == (int)currentNode.x && (int)pos.z == (int)currentNode.z) 
            {   
                if ( Mathf.Abs(pos.y - currentNode.y) < DistanceBufferY)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Check for completion. Checks if we are in range of
        ///     the final node.
        /// </summary>
        /// <param name="pos">  Our current position</param>
        void checkCompletion(Vector3 pos)
        {
            if (distanceToNextNode(pos) < EndBuffer)
            {
                Completed = true;
            }

        }

        /// <summary>
        ///     increment the target node to the next
        ///     node along the path
        /// </summary>
        public void nextTarget()
        {
            current+=iterationCount;
            currentNode = pathNodes[ Mathf.Clamp(value: current, min: 0, max: pathNodes.Count - 1) ];
        }

        /// <summary>
        ///     decrement the target node to the previous
        ///     node along the path        
        /// /// </summary>
        public void previousTarget()
        {
            current-=iterationCount;
            currentNode = pathNodes[ Mathf.Clamp(value: current, min: 0, max: pathNodes.Count - 1) ];
        }

        /// <summary>
        ///     Get the node at the specified increment
        /// </summary>
        /// <param name="item">    the item to get </param>
        /// <returns>
        ///     A node from the item location specified
        /// </returns>
        public Vector3 peekTargetAt(int item)
        {
            return pathNodes[Mathf.Clamp(value: item, min: 0, max: pathNodes.Count - 1)];
        }

        /// <summary>
        ///     Reset our current vector3 to the first item
        /// </summary>
        public void resetCount()
        {
            current = 0;
        }


        #endregion

    }
}
