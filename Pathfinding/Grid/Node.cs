using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace PathFindingAsteria
{

    /// <summary>
    ///     A node on a grid
    /// </summary>
    public class Node : IComparable<Node>
    {
        /// <summary>
        ///     A list of costs associated with this node
        /// </summary>
        public List<NodeLayerCosts> Costs = new List<NodeLayerCosts>();

        /// <summary>
        ///     One instance of each layer this node touches
        /// </summary>
        internal List<int> Layers           = new List<int>();

        /// <summary>
        ///     Determines if this node is on terrain.
        ///     Can be used if it is a blocked node that is on 
        ///     terrain.
        /// </summary>
        internal bool isOnTerrain           = false;
        
        /// <summary>
        ///     The nodes priority number. The lower the number 
        ///     the better the priority
        /// </summary>
        internal float priority;

        /// <summary>
        ///     The type this node has been assigned.
        /// </summary>
        internal NodeType nodeType          = NodeType.Open;

        /// <summary>
        ///     x,y,z in space world location
        /// </summary>
        internal Vector3 Position;

        /// <summary>
        ///     All of the neighboring nodes for this 
        ///     specific node
        /// </summary>
        internal List<Node> Neighbors       = new List<Node>();

        /// <summary> 
        ///     The total distance traveled from the starting point       
        /// </summary>
        internal float distanceTraveled     = Mathf.Infinity;

        /// <summary> 
        ///     The total distance to travel to get to the target.
        ///     This will be set during pathfinding
        /// </summary>
        internal float distanceToTravel     = Mathf.Infinity;

        /// <summary>
        ///     Used in pathfinding, used to assign the node that
        ///     came before this node in a series to form a path.
        /// </summary>
        internal Node previous              = null;

        /// <summary>
        ///     The amount of colliders, or solid objects, that 
        ///     the node has encountered
        /// </summary>
        internal int hitCount               = 0;

        // Cached Index location in three dimensional graph
        int xIndex;
        int yIndex;
        int zIndex;

        /// <summary>
        ///     The Nodes location in a 3D grid
        /// </summary>
        internal Vector3Int Index
        {
            get { return new Vector3Int(xIndex, yIndex, zIndex); }
        }

        /// <summary> The nodes X index </summary>
        internal int X { get    { return xIndex; } }
        /// <summary> The nodes Y index </summary>
        internal int Y { get    { return yIndex; } }
        /// <summary> The nodes Z index </summary>
        internal int Z { get    { return zIndex; } }

        /// <summary>
        ///     Generic Constructor
        /// </summary>
        /// <param name="x">    The x index of the 3d array </param>
        /// <param name="y">    The y index of the 3d array </param>
        /// <param name="z">    The z index of the 3d array </param>
        /// <param name="_position">    The world space vector of the node  </param>
        /// <param name="nodeType">     The type of node we have assiged    </param>
        internal Node(int x, int y, int z, Vector3 _position, NodeType nodeType)
        {
            xIndex          = x;
            yIndex          = y;
            zIndex          = z;
            Position        = _position;
            this.nodeType   = nodeType;
        }

        /// <summary>
        ///     Compares a nodes priority for importance 
        ///     to determine if it is greater than, less than,
        ///     or equal to another node. Not used for true equality
        /// </summary>
        /// <param name="other">    The opposite node to compare    </param>
        /// <returns>
        ///     -1  This has a lower priority
        ///     0   They are the same
        ///     1   The other has a higher priority
        /// </returns>
        /// <remarks>
        ///     This is an extension of the Icomparable 
        ///     interface
        /// </remarks>
        public int CompareTo(Node other)
        {
            if (this.priority < other.priority)
            {
                return -1;
            }
            else if (this.priority > other.priority)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #region External Resources

        /// <summary>
        ///     Instantiate a copy of this node
        /// </summary>
        /// <param name="node">             The node to copy                                                    </param>
        /// <param name="KeepNeighbors">    Determines if we should clear or keep the neighbors                 </param>
        /// <param name="keepPrevious">     Determines if we should clear or keep the previous node             </param>
        /// <param name="keepLayers">       Determines if we should clear or keep the layers the node touches   </param>
        /// <returns>
        ///     A reintialized copy of the Node
        /// </returns>
        /// <remarks>
        ///     The defaults should typically remain.
        ///     Keeps the neighbor pointer, so those will need to be copied to.
        /// </remarks>
        internal Node Copy( bool KeepNeighbors = true, bool keepPrevious = false, bool keepLayers = true)
        {
            var new_node    = new Node(x: X, y: Y, z: Z, 
                                _position: new Vector3(Position.x, Position.y, Position.z), nodeType);
            new_node.Costs  = Costs;

            new_node.priority           = priority;
            new_node.distanceToTravel   = distanceToTravel;
            new_node.distanceTraveled   = distanceTraveled;
            new_node.isOnTerrain        = isOnTerrain;
            new_node.hitCount           = hitCount;            

            // Switches
            if ( KeepNeighbors  )   { new_node.Neighbors    = Neighbors;    }
            if ( keepPrevious   )   { new_node.previous     = previous;     }
            if ( keepLayers     )   { new_node.Layers       = Layers;       }
            
            return new_node;
        }

        /// <summary>
        ///     Re-initialize all the neighbor nodes. 
        ///     The neighbors of the neighbors will retain thier pointers
        /// </summary>
        internal void OrphanNeighbors()
        {
            for (int i = 0; i < Neighbors.Count; i++)
            {
                Neighbors[i] = Neighbors[i].Copy();
            }
        }

        /// <summary>
        ///     Compare this node to another seperate node
        ///     for equality
        ///     This uses the node index
        /// </summary>
        /// <param name="n">    The seperate node for comparison    </param>
        /// <returns>
        ///     A boolean if the other node is correct
        /// </returns>
        internal bool Equals(Node n)
        {
            if ( this.Index == n.Index )
            {
                return true;
            }
            return false;
        }


        #region Layer Checks

        /// <summary>
        ///     Will check if the layer is attached to the node
        /// </summary>
        /// <returns>
        ///     A boolean if the layer exists on the node
        /// </returns>
        internal bool CheckForLayer(NodeLayerCosts exCost)
        {
            // Need to search the layers not the fucking costs
            foreach(int layer in Layers)
            {
                if ( Utilities_PF.IsLayerMatch( layer, exCost.Layer) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Will check for a list of layers within the node
        /// </summary>
        /// <param name="costList"> A list of costs associated with what we want to compare    </param>
        /// <returns>
        ///     True if any of the layers exists on the node
        /// </returns>
        internal bool CheckForLayer(List<NodeLayerCosts> costList)
        {
            foreach( NodeLayerCosts cost in costList)
            {
                if ( CheckForLayer(cost) ) 
                { 
                    return true; 
                }
            }

            return false;
        }

        #endregion
        
        #endregion
                
    }
}