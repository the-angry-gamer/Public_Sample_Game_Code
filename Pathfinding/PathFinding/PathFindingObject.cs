using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{
    public class PathFindingObject : MonoBehaviour
    {

        #region Editor

        [Header("Path Options")]
        [Tooltip("A trigger to reinitiate the graph")]
        [SerializeField]
        public bool RecreatePath        = false;

        [Tooltip("Constantly update the path upon every completion")]
        [SerializeField]
        public bool ContinousRefresh    = false;

        [Tooltip("Determine the time interval, in seconds, on how long to wait before re-calculating the path")]
        [Range(0, 10)]
        [SerializeField] public double RefreshInterval = 1;

        [Tooltip("Log our actions to alert operator where we stand")]
        [SerializeField]
        public bool LogDebug            = false;

        [SerializeField]
        [Tooltip("Highlight the path in this color. For use with the GridViewer")]
        internal Color pathColor        = Color.magenta;

        [Header("Internal Path Parameters")]
        [SerializeField]
        public PathfindingParameters pathParameters;

      
        #endregion


        #region Properties

        public bool isCreated
        {
            get
            {
                if (pathFinder != null) { return true; }
                return false;
            }
        }

        /// <summary>
        ///     Get the internal ID of this item
        /// </summary>
        internal string ID
        {
            set
            {
                id = value;
            }
            get
            {
                if (id == null || id == string.Empty)
                {
                    id = gameObject.name;
                }
                return id;
            }
        }

        /// <summary>
        ///     The actual objective we are looking for
        /// </summary>
        public GameObject EndObject
        {
            set
            {
                if (value != null)
                {
                    endLocation = value.transform.position;
                }
                endObject   = value;
            }
            get { return endObject; }
        }

        /// <summary>
        ///     The location we are trying to search for when finding the path
        /// </summary>
        /// <remarks>
        ///     Setting this will override the end game object we have set.
        ///     If the endobject has been set, it will return the location of that.
        /// </remarks>
        public Vector3 EndPosition
        {
            set
            {
                EndObject   = null;
                endLocation = value;
            }
            get
            { 
                if (EndObject != null) { return endObject.transform.position; }
                return endLocation; 
            }
        }

        /// <summary>
        ///     The coordinates of the path we traverse along
        /// </summary>
        public List<Vector3> PathCoordinates
        {
            get 
            { 
                if ( pathFinder != null )
                {
                    return pathFinder.PathCoordinates; 
                }
                return null;
            }
        }

        /// <summary>
        ///     Get a list of all the path nodes for this object
        /// </summary>
        public List<Node> PathNodes
        {
            get;
            private set;
        } = new List<Node>();

        /// <summary>
        ///     A list of all explored nodes in the the 
        ///     pathfinding search
        /// </summary>
        public List<Node> ExploredNodes
        {
            get { return pathFinder.ExploredNodes; }
        }

        /// <summary>
        ///     A trigger to let us know if the path thread has been finished
        /// </summary>
        public bool PathFinished
        {
            get 
            {
                if( pathFinder != null )
                {
                    return pathFinder.PathFinished;
                }
                return false;
            }
        }

        /// <summary>
        ///     The time take to complete the path in seconds
        /// </summary>
        public double TimeTaken
        {
            get
            {
                return pathFinder?.TimeTaken ?? 0; 
            }
        }

        /// <summary>
        ///     The date and time when the pathfinding completed
        /// </summary>
        public DateTime PathCompletionDateTime
        {
            get
            {
                return pathFinder?.EndTime ?? DateTime.Now;
            }
        }

        /// <summary>
        ///     The start time of the path
        /// </summary>
        public DateTime PathStartTime
        {
            get
            {
                return pathFinder?.StartTime ?? DateTime.Now;
            }
        }

        /// <summary>
        ///     The game time when the path was completed
        /// </summary>
        public float PathCompletionGameTime
        {
            get;
            private set;
        }

        /// <summary>
        ///     Determines if we have started the path
        /// </summary>
        public bool PathStarted
        {
            get
            {
                if (pathFinder != null)
                {
                    return pathFinder.PathStarted;
                }
                return false;
            }
        }

        /// <summary>
        ///     Set the type of path we want to use
        /// </summary>
        public PathType TypeOfPath
        {
            set
            {
                pathParameters.TypeOfPath = value;
            }
            get
            {
                return pathParameters.TypeOfPath;
            }
        }
        
        /// <summary>
        ///     The node that we are starting from
        /// </summary>
        public Node StartNode
        {
            get { return pathFinder.StartNode; }
        }

        /// <summary>
        ///     The node closest to the target location
        /// </summary>
        public Node EndNode
        {
            get { return pathFinder.EndNode; }
        }

        /// <summary>
        ///     Determines whether we found the objective. 
        ///     If we did not find this is false and we are 
        ///     pathfinding based off of the closest node
        /// </summary>
        public bool ObjectiveFound
        {
            get { return pathFinder.ObjectiveFound; }
        }

        #endregion


        #region Private Declarations

        /// <summary> the pathfinding algorithm </summary>
        PathFinder  pathFinder = new PathFinder();
        GameObject  endObject;
        Vector3     endLocation;
        string id;
        bool pathStarted = false;

        #endregion


        #region Constructors

        /// <summary>
        ///     A constructor to only pass the end object
        /// </summary>
        /// <param name="end"></param>
        public PathFindingObject(GameObject end)
        {
            pathParameters.targetObject = end;
        }

        #endregion


        #region Monobehavior

        private void Awake()
        {
            SetInitialLists();
        }

        void SetInitialLists()
        {
            // Set Params
            PathNodes   = new List<Node>();
            pathFinder  = new PathFinder();

            if ( pathParameters.pathFinderObject == null)
            {
                pathParameters.pathFinderObject = gameObject;
            }
        }

        private void FixedUpdate()
        {
            checkFindPath();
        }

        /// <summary>
        ///     Find our path for this object. 
        ///     This will wait for both the grid and 
        ///     the target objective to be assigned
        /// </summary>
        void checkFindPath()
        {
            // If we are not ready get out of here
            if (pathParameters.gridBase == null || !checkTimeInterval() ) { return; }

            // check if we want to find it
            if ( ContinousRefresh || RecreatePath )
            {
                if ( !pathFinder.PathStarted && !pathStarted)      
                {
                    pathStarted = true;
                    LogStart();
                    createThePath();
                }   
                RecreatePath = false;
            }

            // this is here to control thread protection
            if ( pathFinder.PathFinished && pathStarted )      
            {
                LogEnd();
                PathNodes               = pathFinder.pathNodes;
                pathStarted             = false;
                PathCompletionGameTime  = Time.time;
            }
        }

        #endregion


        #region  Private Functions
        /// <summary>
        ///     Determines if we have passed the interval in which to recreate the path 
        ///     while doing a continous refresh
        /// </summary>
        /// <returns>
        ///     A boolean of yes if we are good to go,
        ///     or false if we are still waiting
        /// </returns>
        bool checkTimeInterval()
        {

            if (ContinousRefresh && ( pathFinder.EndTime > DateTime.Now.AddSeconds( -RefreshInterval ) ) )
            {
                return false;
            }
            return true;
        }

        #region Logging

        /// <summary>
        ///     Log the start of the pathfinding
        /// </summary>
        void LogStart()
        {
            if (LogDebug) { Debug.Log($"Path for {ID} has started at {DateTime.Now}."); }
        }

        /// <summary>
        ///     Log the end of the pathfinding object
        /// </summary>
        void LogEnd()
        {
            if (LogDebug)
            {
                Debug.Log($"Path for {ID} has finished at {DateTime.Now}. Time taken was {pathFinder.TimeTaken}");
            }
        }

        #endregion

        /// <summary>
        ///     Get the parameters ready for the pathfinding class
        /// </summary>
        void Setup()
        {            
            pathFinder                      = new PathFinder();
            pathParameters.pathFinderObject = gameObject;

            pathParameters.EndPosition      = EndPosition;

            AssignCostsDictionary();
        }


        /// <summary>
        ///     Create the path for the object        
        /// </summary>
        /// <remarks>
        ///     A grid must be cached for this to work
        /// </remarks>
        void createThePath()
        {
            Setup();
            
            if (pathFinder == null)
            {
                pathFinder = new PathFinder();
            }
            
            pathFinder.Init(pathParameters);
        }

        /// <summary>
        ///     Assign all the costs to the dictionary
        /// </summary>
        void AssignCostsDictionary()
        {
            // Clear our dictionaries and lists
            pathParameters.blockedLayers = new List<NodeLayerCosts>();
            pathParameters.LayerCostDictionary = new Dictionary<LayerMask, NodeLayerCosts>();

            // Set the dictionary values for easy consumption
            foreach (NodeLayerCosts costs in pathParameters.OverrideCosts)
            {
                if (costs.isBlocked) { pathParameters.blockedLayers.Add(costs); }

                pathParameters.LayerCostDictionary.AddKeySafe(costs.Layer, costs);
            }
        }

        #endregion


        #region Public

        /// <summary>
        ///     Generate a new random color for 
        ///     path outline
        /// </summary>
        public void RandomPathColor()
        {
            int r = UnityEngine.Random.Range(0, 255);
            int g = UnityEngine.Random.Range(0, 255);
            int b = UnityEngine.Random.Range(0, 255);
            pathColor = new Color(r,g,b);
        }

        /// <summary>
        ///     Set all the default values for the pathfinding 
        ///     object
        /// </summary>
        public void SetDefaultValues()
        {
            pathParameters = new PathfindingParameters();
            LogDebug = false;

            pathParameters.threadPathCreation = true;
            pathColor = Color.magenta;
        }

        #region Path Creation

        /// <summary>
        ///     Create the path from the object this is attached to 
        ///     all the way to the target that is passed
        /// </summary>
        /// <param name="grid">     The total grid the object can traverse      </param>
        /// <param name="target">   The gameobject that will be the objective   </param>
        public void createThePath(Grid grid, GameObject target)
        {
            EndObject                   = target;
            pathParameters.gridBase     = grid;

            createThePath();
        }

        /// <summary>
        ///     Create the path from the object this is attached to 
        ///     all the way to the target that is passed
        /// </summary>
        /// <param name="grid">     The total grid the object can traverse      </param>
        /// <param name="target">   The location that will be the objective   </param>
        public void createThePath(Grid grid, Vector3 target)
        {
            EndPosition             = target;
            pathParameters.gridBase = grid;

            createThePath();
        }

        /// <summary>
        ///     recreate the path, assumes we have already assigned the grid.
        ///     Will return if we have no grid to run
        /// </summary>
        /// <param name="target"></param>
        public void createThePath(Vector3 target)
        {
            if (pathParameters.gridBase == null) { return; }

            EndPosition = target;

            createThePath();
        }

        #endregion

        #endregion
    }

}