using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFindingAsteria;
using System;

namespace AI_Asteria
{
    /// <summary>
    ///     Control Movement along a determined path of vector 3 locations
    /// </summary>
    [RequireComponent(typeof(PathFindingObject))]
    [RequireComponent(typeof(AIWaypointNetwork))]
    public class AIRandomState_PatrolWaypoint_1 : AIRandomState
    {

        #region Editor

        [Header("Movement")]
        [SerializeField]
        [Tooltip("Will determine if we want to shift back to idle after a given amount of time. Will also reset the path we are on")]
        bool EnableWaitTime = true;

        [SerializeField]
        [Range(0.1f, 1.5f)]
        [Tooltip("The speed we want to move about our random path")]
        float speed = 0.5f;

        [SerializeField]
        [Tooltip("The amount of nodes we want to skip while traversing path")]
        [Range(1, 15)]
        int skipLocations = 0;

        [SerializeField]
        [Tooltip("Determine how many waypoints to interate prior to going back to idle")]
        [Range(1,10)]
        int waitIdle = 1;

        [Header("Finding a Path ")]
        [Tooltip("What to include when searching for a straight path")]
        [SerializeField] LayerMask includeObstacles = ~0;

        [Tooltip("Determine if we want to check for a straight line override between objectives")]
        [SerializeField] bool checkStraightLine     = true;

        [Tooltip("Set the Y height check minimum for the box collider to go")]
        [SerializeField] [Range(0.1f, 15)]
        float yHeighCheck          = 0.2f;
        
        [Tooltip("How far in front of the character the check will go")]
        [SerializeField]
        [Range(0.1f, 15)]
        float frontOffSet = 0.5f;

        [Header("Positional Buffers")]
        [Tooltip("The buffer distance for path point recognition")]
        [SerializeField] [Range(0, 15)] protected float pointXZBuffer = 1.0f;

        [Tooltip("The buffer distance for path point recognition on the y axis")]
        [SerializeField] [Range(0, 15)] protected float pointYBuffer = 1.0f;

        [Tooltip("The buffer to the end point to be considered complete")]
        [SerializeField] [Range(0, 15)] protected float endBuffer = 1.0f;

        #endregion


        #region Internal

        /// <summary>
        ///     The type of state this script is associated with
        /// </summary>
        internal override AIStateType StateAssociation { get; } = AIStateType.Patrol;

        /// <summary>
        ///     An action to accomplish when we push a button
        /// </summary>
        internal override Action DoWork { get { return DrawLines; } }

        /// <summary>
        ///     What goes on the button text   
        /// </summary>
        internal override string ActionName { get { return forButton; } }

        #endregion


        #region Private

        string              forButton           = "Show Paths";

        AIWaypointNetwork   wayPoints;

        PathFindingObject   pfo;

        bool                havePath            = false;

        int                 waypointsPassed = 0;

        int                 currenWaypoint = 0;
        
        List<Vector3>       nextPath;

        List<Transform>     wayPointLocations   = new List<Transform>();

        Dictionary<int, PathFinder> Paths       = new Dictionary<int, PathFinder>();

        bool                ShowPaths           = false;

        List<GameObject>    blockingPath        = new List<GameObject>();



        #endregion


        #region For Editor

        /// <summary>
        ///     The gameobject that we are hitting
        /// </summary>
        public GameObject straightOutHit
        {
            get
            {
                GameObject temp = null;
                float tempf     = float.PositiveInfinity;
                foreach (RaycastHit hit in rhit )
                {  
                    if (hit.distance < tempf)
                    {
                        temp    = hit.transform.gameObject;
                        tempf   = hit.distance;
                    }
                }                
                return temp;
            }
        }


        /// <summary>
        ///     Return the current waypoint
        /// </summary>
        public int GetCurrentWayPoint
        {
            get
            {
                return currenWaypoint;
            }
        }

        /// <summary>
        ///     Get the next waypoint in the list
        /// </summary>
        public int GetNextWaypoint
        {
            get
            {
                return determineFollowingWaypoint(currenWaypoint);
            }
        }

        /// <summary>
        ///     Get the location of the player
        /// </summary>
        public Vector3 GetPlayerLocation
        {
            get
            {
                return transform.position;
            }
        }

        /// <summary>
        ///     The location of our next target
        /// </summary>
        public Vector3 GetTargetLocation
        {
            get
            {
                if(stateMachine == null ) { return new Vector3(); }
                if (stateMachine.Path.move == null)
                {
                    return new Vector3();
                }
                return stateMachine.Path.move.CurrentTarget;
            }
        }

        /// <summary>
        ///     The final target of where we are heading
        /// </summary>
        public Vector3 EndTargetLocation
        {
            get
            {
                if (wayPointLocations.Count <= currenWaypoint) { return new Vector3(); }
                return wayPointLocations[currenWaypoint].position;
            }
        }

        #endregion


        /// <summary>
        ///     All the paths we have created for the 
        ///     pathfinders
        /// </summary>
        public List<PathFinder> DevelopedPaths
        {
            get
            {
                List<PathFinder> temp = new List<PathFinder>();

                foreach(KeyValuePair<int, PathFinder> item in Paths)
                {
                    if (item.Value.PathFinished)
                    {
                        temp.Add(item.Value);
                    }
                }
                return temp;
            }
        }


        void Start()
        {
            pfo         = GetComponent<PathFindingObject>();
            wayPoints   = GetComponent<AIWaypointNetwork>();

            setDictionary();

            setPath();
        }


        public override AIStateType OnUpdate()
        {
            determineNextLocation();
            updateMachine();

            drawLines();

            // new state 
            returnStateType = determineNewState();

            base.OnUpdate();
            return returnStateType;
        }

        public override void OnEnterState()
        {
            stateMachine.checkObstacles = true;
            stateMachine.Speed          = speed;
            setPath();
            base.OnEnterState();
        }

        public override void OnExitState()
        {
            stateMachine.checkObstacles = false;
            stateMachine.Speed          = 0;
            stateMachine.ClearTarget();
        }

        /// <summary>
        ///     Set the pathfinding dictionary values
        /// </summary>
        void setDictionary()
        {
            int i = 0;
            foreach(Transform go in wayPoints.Waypoints)
            {
                wayPointLocations.Add(go);
            }
            foreach (Transform t in wayPointLocations)
            {
                PathFinder pf = new PathFinder();        
                if (Paths.ContainsKey(i))
                {
                    Paths[i] = pf;
                }
                else
                {
                    Paths.Add(key: i, value: pf);
                }
                i++;
            }
        }

        /// <summary>
        ///     Run the path between the two positions
        /// </summary>
        /// <param name="start">    the starting position       </param>
        /// <param name="end">      the ending position         </param>
        /// <param name="path">     The path number to create   </param>
        void runPathCreation(Vector3 start, Vector3 end, int path)
        {
            PathfindingParameters pfm   = pfo.pathParameters;
            pfm.pathFinderObject        = null;
            pfm.startPosition           = start;
            pfm.EndPosition             = end;

            Paths[path].Init(pfm);
        }

        /// <summary>
        ///     Get our current Path
        /// </summary>
        void setPath()
        {
            checkPathObstacles(); 
            if ( checkPathStraight() )
            {
                // Go straight towards the obstacle                
                stateMachine.Path.move = new MoveAlongPath(distanceBufferXZ: pointXZBuffer, distanceBuffY: pointYBuffer, path: new List<Vector3>() { wayPointLocations[determineFollowingWaypoint(currenWaypoint)].position }); ;
                havePath = true;
            }
            else
            {                
                if ( Paths[currenWaypoint].PathFinished )
                {
                    if ( !havePath )
                    {
                        stateMachine.Path.move = new MoveAlongPath( distanceBufferXZ: pointXZBuffer, distanceBuffY: pointYBuffer, path: Paths[currenWaypoint].PathCoordinates);
                        setNextPath();
                    }
                    havePath = true;
                }
                else
                {
                    havePath = false;
                    resetCurrentPath();
                }
            }
            
        }

        /// <summary>
        ///     Get the path between the next objects
        /// </summary>
        void setNextPath()
        {
            var next        = determineFollowingWaypoint(currenWaypoint);
            var following   = determineFollowingWaypoint(next);
            if ( !Paths[next].PathStarted )
            {
                runPathCreation( wayPointLocations[next].position, wayPointLocations[following].position, next );
            }
        }

        /// <summary>
        ///     Get where we want to be heading
        /// </summary>
        void determineNextLocation()
        {
            setPath();
            if (!havePath) { return; }

            updateMovement();
           
            if (stateMachine.Path.move.Completed)
            {
                havePath = false;
                waypointsPassed++;
                incrementWayPoint();
                setPath();
            }
        }

        /// <summary>
        ///     Check our movement controller
        /// </summary>
        void updateMovement()
        {
            stateMachine.Path.move.iterationCount       = Mathf.Clamp(skipLocations, 1, 15);
            stateMachine.Path.move.DistanceBufferXZ     = pointXZBuffer;
            stateMachine.Path.move.DistanceBufferY      = pointYBuffer;

            stateMachine.Path.move.checkTarget(transform.position);
        }

        /// <summary>
        ///     Check in front of our current object to see if we have to 
        ///     update our path
        /// </summary>
        bool checkPathObstacles()
        {
            if (stateMachine.playerBlocked)
            {
                bool rerun = false;
                foreach (GameObject go in stateMachine.ObjectsHitting) 
                {
                    if (!blockingPath.Contains(go))
                    {
                        rerun = true;
                        blockingPath.Add(go);
                    }
                }

                if (rerun)
                {
                    string item = (blockingPath.Count > 0) ? item = blockingPath[0].name : "No gameobject";
                    Debug.Log($"Player blocked. First Object [{item}]");
                    resetCurrentPath();
                }
            }
            else
            {
                blockingPath = new List<GameObject>();
            }
            return stateMachine.playerBlocked;
        }

        /// <summary>
        ///     Reset the path coming from the waypoint designated
        /// </summary>        
        void resetCurrentPath()
        {
            if ( !Paths[ currenWaypoint ].PathStarted && pfo.pathParameters.gridBase.CreationComplete )
            {
                havePath = false;
                runPathCreation(transform.position, wayPointLocations[determineFollowingWaypoint(currenWaypoint)].position, currenWaypoint);
            }
        }

        /// <summary>
        ///     Safely increment our waypoint
        /// </summary>
        /// <returns>
        ///     What the next increment is
        /// </returns>
        int incrementWayPoint()
        {
            if (currenWaypoint < wayPointLocations.Count - 1)
            {
                currenWaypoint++;
            }
            else
            {
                currenWaypoint = 0;
            }
            return currenWaypoint;
        }


        /// <summary>
        ///     Get the waypoint that follows our current objective
        /// </summary>
        /// <returns></returns>
        int determineFollowingWaypoint(int count)
        {
            int temp = count + 1;
            if (temp < wayPointLocations.Count)
            {
                return temp;
            }
            else
            {
                return 0;
            }
        }

       
        /// <summary>
        ///     Determine what state we are going to enter
        /// </summary>
        /// <returns>
        ///     A new state to transition to
        /// </returns>
        AIStateType determineNewState()
        {
            if ( EnableWaitTime && passedWaitTime() || !havePath )
            {
                return AIStateType.Idle;
            }

            if( waypointsPassed > waitIdle )
            {
                waypointsPassed = 0;
                return AIStateType.Idle;
            }

            return AIStateType.Patrol;
        }

        /// <summary>
        ///     Update the values of the state machine
        /// </summary>
        void updateMachine()
        {            
            if( havePath )
            {
                stateMachine.SetTarget(AITargetType.Node, stateMachine.Path.move.CurrentTarget, stateMachine.Path.move.distanceToNextNode(transform.position));
            }

            stateMachine.Speed      = speed;
        }

        /// <summary>
        ///     A trigger to be used in the state base action
        /// </summary>
        void DrawLines()
        {
            ShowPaths = !ShowPaths;

            forButton = ShowPaths ? "Hide Paths" : "Show Paths";
        }

        /// <summary>
        ///     Draw our lines on the editor
        /// </summary>
        void drawLines()
        {
            if (!ShowPaths) { return; }
            var count   = Paths[currenWaypoint].PathCoordinates.Count - 1;
            count = count < 0 ? 0 : count;

            if (!usingStraightLine) { Debug.DrawLine(transform.position, Paths[currenWaypoint].PathCoordinates[count]); }
            Debug.DrawLine(transform.position, stateMachine.Path.move.CurrentTarget, Color.black);

            for (int j = 0; j < Paths.Count; j++)
            {                
                if (!Paths[j].PathFinished) { return; }
                var path = Paths[j];
                for (int i = 0; i < path.PathCoordinates.Count - 1; i++)
                {
                    if (i != 0)
                    {
                        Debug.DrawLine(path.PathCoordinates[i], path.PathCoordinates[i + 1], j.isEven() ? Color.blue : Color.red);
                    }
                }
            }
        }

        bool usingStraightLine = false;
        RaycastHit[] rhit = new RaycastHit[0];
        /// <summary>
        ///     Determine if the path to our current waypoint 
        ///     is straight without any interruptions
        /// </summary>
        /// <returns>
        ///     A bool stating if we hit something. 
        ///     True    = nothing in paths way;
        ///     False   = there is something in our way.
        /// </returns>
        bool checkPathStraight()
        {
            if ( checkStraightLine )
            {
                var target          = wayPointLocations[determineFollowingWaypoint( currenWaypoint )].position;
                target              = new Vector3(target.x, target.y < yHeighCheck ? yHeighCheck : target.y , target.z);
                var position        = (transform.position + (transform.forward * frontOffSet)) + new Vector3(0,.5f,0);
                position            = new Vector3(position.x, position.y < yHeighCheck ? yHeighCheck : position.y , position.z);

                Vector3 halfExtents = Vector3.one;

                Quaternion rotation = Quaternion.LookRotation(target - position);
                Vector3 direction   = target - position;
                float distance      = Vector3.Distance(position, target);

                rhit =  Physics.BoxCastAll(position, halfExtents, direction, rotation, distance, includeObstacles);
                bool result         = rhit.Length > 0 ? true : false;
                halfExtents         = new Vector3(1, 1, (target - position).magnitude) / 2;

                if (ShowPaths) { DrawBoxRay.DrawBox(Vector3.Lerp(position, target, 0.5f), halfExtents, rotation, result ? Color.red : Color.green); }

                if (usingStraightLine && !result) { havePath = false; }

                usingStraightLine   = !result;
                return !result;
            }
            return false;

        }

       
    }

}