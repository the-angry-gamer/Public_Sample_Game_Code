using System;
using System.Collections.Generic;
using UnityEngine;



namespace PathFindingAsteria
{

    /// <summary>
    ///     The avaliable graph types
    /// </summary>
    public enum GraphType
    {
        NodeBasedGrid,
        OverlayGrid,
        TerrainSnap
        //LineNode
    }

    /// <summary>
    ///     An error of what occured within the classes
    /// </summary>
    public struct PathFindingError
    {
     
        /// <summary> The time the error Occured    </summary>
        public DateTime Time    { internal set; get; }

        /// <summary> The error string               </summary>
        public string   Error   { internal set; get; }

        public PathFindingError( string error)
        {
            Error   = error;
            Time    = DateTime.Now;
        }

    }
    

    [Serializable]
    public struct NodeLayerCosts
    {        
        [SerializeField]
        public LayerMask  Layer;

        [SerializeField]
        public float      Cost;

        [SerializeField]
        public bool       isBlocked;
    }


    /// <summary>
    ///     The selection of path types we can search through
    /// </summary>
    public enum PathType
    {
        BreadthFirstSearch,
        GreedyBestFirst,
        AStar
    }


    /// <summary>
    ///     The various options for the types of nodes that we have. 
    ///     This is used in conjuction with layers attached to the node
    /// </summary>
    public enum NodeType
    {
        Blocked,
        Terrain,
        OpenBorder,
        Open
    }


}
