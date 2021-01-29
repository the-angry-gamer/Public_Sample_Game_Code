using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{

    /// <summary>
    ///     This lays out the parameters used for the development
    ///     of an objects path within a node based grid
    /// </summary>
    [Serializable]
    public class PathfindingParameters
    {
        #region Editor components

        //////////////////////////////////
        [Header("OBJECT INFORMATION")]

        [Tooltip("This is the object that we want to find. Its vector3 will be used to determine the end point")]
        [SerializeField] public GameObject  targetObject;

        [Tooltip("Determines whether to create this path when we draw it out")]
        [SerializeField] public bool        canShowPath         = true;

        [Tooltip("Determines whether or not we create the path of this object during the next path finding iterations")]
        [SerializeField] public bool        createPath          = true;

        [Tooltip("The vector3 offset for the starting position. Can be used to make sure we do not fall through what we are standing on")]
        [SerializeField] public Vector3     startOffset         = new Vector3 ( 0.0f, 0.0f, 0.0f );
        
        [Tooltip("Stop the search for our closeness to objective when we are within one iteration of it. Best guess closest node")]
        [SerializeField] public bool        findObjQuick        = true;

        [Tooltip("The vector3 offset for the ending position")]
        [SerializeField] public Vector3     targetOffset        = new Vector3 ( 0.0f, 0.0f, 0.0f );

        //////////////////////////////////
        [Header("SEARCH OPTIONS")]

        [SerializeField]
        [Tooltip("The search algorithm to use")]
        internal PathType TypeOfPath    = PathType.AStar;

        [SerializeField]
        [Tooltip("Create the path on a seperate thread")]
        public bool threadPathCreation  = true;

        [SerializeField]
        [Tooltip("The maximum allowed time for the pathfinder to execute")]
        [Range(0,60)] 
        public int timeAllowed          = 10;

        [Tooltip("Snap to the ground. Make sure the path follows the ground or solid object")]
        [SerializeField] public bool    snapToGround        = true;

        [Tooltip("Determines if the terrain should act as a barrier, this will include non terrain nodes that are also on the terrain")]
        [SerializeField] public bool    TerrainIsBarrier    = true;


        [Tooltip("The cost of each height iteration traversed")]
        [SerializeField] public float heighIterationCost    = 5.0f;

        [Tooltip("Determine what the costs of the ")]
        [SerializeField]
        internal List<NodeLayerCosts>   OverrideCosts                       = new List<NodeLayerCosts>();
        // This is a list to hold the necessary information
        internal Dictionary<LayerMask, NodeLayerCosts> LayerCostDictionary = new Dictionary<LayerMask, NodeLayerCosts>();
        // this holds all the layers that are blocked
        internal List<NodeLayerCosts> blockedLayers                         = new List<NodeLayerCosts>();

        [Tooltip("Determines what nodes can be used to create a path - can pass through these nodes when searching")]
        public List<NodeType> pathTypes         = new List<NodeType>() { NodeType.OpenBorder, NodeType.Open };
        [Tooltip("What types of nodes are blockers")]
        public List<NodeType> barrierTypes      = new List<NodeType>() { NodeType.Blocked, NodeType.Terrain };
        [Tooltip("The types of nodes we can walk on but no pass through")]
        public List<NodeType> walkAndStopSearch = new List<NodeType>() { NodeType.Terrain };

        [Header("TRAVERSING OPTIONS")]
        
        [Tooltip("Restrict the maximum height that we can climb")]
        [SerializeField] public bool    restrainTraverseHeight      = false;
        [Tooltip("The maximum height we can traverse over in one vertical")]
        [SerializeField] public float   maxHeightTraversal          = Mathf.Infinity;

        [Tooltip("Restrict the maximum height that this object can drop")]
        [SerializeField] public bool    restrainTraverseDrop        = false;
        [Tooltip("The maximum height we can be above the blocked object in one vertical")]
        [SerializeField] public float   maxDropHeight               = 2.0f;
        
        [Tooltip("Allow us to search for a height and width outside of the grid when we are overriding dimensions.")]
        [SerializeField] public bool    IgnoreOutOfGridDimensions   = true;

        [Tooltip("Allows us to override the height parameters when finding a path")]
        [SerializeField] public bool    overridePathHeight          = true;      

        [Tooltip("The height of the object traversing the path")]
        [Range(0, 100)]
        [SerializeField] public float   ObjectHeight                = 0.5f;
        
        [Tooltip("Allows us to override the width parameters of a when checking for a path")]
        [SerializeField] public bool    overridePathWidth           = true;

        [Tooltip("The width of the object traversing the path")]
        [Range(0, 100)]
        [SerializeField] public float   ObjectWidth                 = 0.5f;

        //[Range(0, 180)]
        //[Tooltip("Determine the angle at which we can climb ")]
        //[SerializeField] public int     climbAngle = 45;

        /// <summary> Hold the grid </summary>
        [HideInInspector]
        public Grid gridBase;

        internal GameObject pathFinderObject;

        internal Vector3    StartingPosition;

        #endregion

        #region Cached

        Vector3 endPosition;

        #endregion

        #region properties

        /// <summary>
        ///     The end position of the path.
        ///     Having a target object will ovveride any set value here
        /// </summary>
        internal Vector3 EndPosition
        {
            get
            {
                if (targetObject != null)
                {
                    return targetObject.transform.position + targetOffset;
                }
                else
                {
                    return endPosition;
                }
            }
            set
            {
                endPosition = value;
            }
        }

        /// <summary>
        ///     The location of the start position. 
        ///     This uses the gameobject transform (pathFinderObject) 
        ///     so that we can track movements
        /// </summary>
        internal Vector3 startPosition
        {
            get 
            {
                if (pathFinderObject != null)
                {
                    return pathFinderObject.transform.position + startOffset;
                }
                else
                {
                    return StartingPosition;
                }               
            }
            set
            {
                StartingPosition = value + startOffset;
            }
        }

        #endregion


    }

}