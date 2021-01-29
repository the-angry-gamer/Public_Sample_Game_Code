//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Pathfinding;

//namespace AI
//{
//    public enum AIStateType     { None, Idle, Alerted, Patrol, Attack, Pursuit, Dead, Following }
//    public enum AITargetType    { None, Waypoint, Visual_Player, Visual_Light, Audio, Threat, Friendly }
  
//    /// <summary>
//    ///     Our Target information
//    /// </summary>
//    internal struct AITarget
//    {
//        /// <summary> The Gameobject we have targeted   </summary>
//        public GameObject   Target          { get; private set; }

//        public Vector3      TargetPosition  { get; private set; }

//        /// <summary> The cached distance to our target </summary>
//        public float        Distance        { get; set; }

//        /// <summary> The type of target our AI has     </summary>
//        public AITargetType Type        { get; private set; }

//        /// <summary>
//        ///     Set our AI target
//        /// </summary>
//        /// <param name="t">    The type of the target              </param>
//        /// <param name="d">    The distance to the target          </param>
//        /// <param name="go">   The gameobject that is the target   </param>
//        public void Set(AITargetType t, float d, GameObject go)
//        {
//            Target      = go;
//            TargetPosition = go.transform.position;
//            Distance    = d;
//            Type        = t;
//        }

//        public void Set(AITargetType t, float d, Vector3 goPos)
//        {
//            Target          = null;
//            TargetPosition  = goPos;
//            Distance        = d;
//            Type            = t;
//        }

//        public void Clear()
//        {
//            Type        = AITargetType.None;
//            Distance    = Mathf.Infinity;
//            Target      = null;
//        }

//        /// <summary>
//        ///     The distance from me to the target
//        /// </summary>
//        /// <param name="me"></param>
//        /// <returns></returns>
//        public float DistanceToTarget(Vector3 me)
//        {
//            return Vector3.Distance(me, TargetPosition);
//        }

//    }

//    /// <summary>
//    ///     An object that can hold tha path and its parameter information
//    /// </summary>
//    internal struct AIPath
//    {
//        internal PathFindingObject PO;

//        /// <summary> The last time the path was created for this object    </summary>
//        internal float lastChecked;

//        /// <summary> Trigger to find a new path                            </summary>
//        internal bool findNewPath;
//    }

//    public abstract class AIState : MonoBehaviour
//    {

//        AITarget targetObject       = new AITarget();
//        internal AIPath     Path    = new AIPath();
//        internal Animator   animator;


//        // Start is called before the first frame update
//        void Start()
//        {
        
//        }

//        // Update is called once per frame
//        void Update()
//        {
//            checkPath();
//        }

//        /// <summary>
//        ///     Check to see if we need a new path
//        /// </summary>
//        void checkPath()
//        {
//            if (Path.findNewPath)
//            {
//                Path.PO.EndPosition = targetObject.TargetPosition;
//                Path.PO.RecreatePath = true;
//                Path.findNewPath = false;
//            }
//        }

//        internal virtual void MoveObject() { }

//        internal virtual void UpdateTarget(GameObject go) { }

//        /// <summary>
//        ///     Determines when we want to find a new path
//        /// </summary>
//        /// <returns></returns>
//        protected virtual bool DetermineNewPath()
//        {
//            if (Path.findNewPath) { Path.lastChecked = Time.time; }
//            return Path.findNewPath;
//        }

//    }

//}