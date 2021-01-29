//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Pathfinding;
//using System;

//namespace AI
//{

   
//    /// <summary>
//    ///     A generic base class for AI base.
//    ///     This class will serve as the basis for 
//    ///     other AI within the pathfinding namespace 
//    /// </summary>
//    [RequireComponent( typeof(PathFindingObject) ) ]
//    public abstract class AIBase : MonoBehaviour
//    {

//        #region Editor

//        [Tooltip("Determine whether the AI uses root motion in animations or is controlled through scripting")]
//        [SerializeField]
//        internal bool UseRootMotion = true;

//        #endregion

//        //internal AIPath     Path = new AIPath();
//        //internal Action     movementAction;
//        //internal Animator   animator;
//        internal AIState    AIstate;


//        #region Properties

//        /// <summary>
//        ///     The target object of the pathfinding object. 
//        ///     This will perputuate down into the pathfinding object.
//        /// </summary>

//        #endregion

//        //// Start is called before the first frame update
//        //void Start()
//        //{
//        //    //Path.PO = gameObject.GetComponent<PathFindingObject>();

//        //    //if (Path.PO == null) { Destroy(gameObject); }

//        //    //targetObject.Set(t: AITargetType.None, d: Vector3.Distance(gameObject.transform.position, Path.PO.EndObject.transform.position), Path.PO.EndObject.transform.position);

//        //    //animator = gameObject.GetComponent<Animator>();

//        //}

//        //// Update is called once per frame
//        //protected virtual void FixedUpdate()
//        //{
//        //    //if (Path.findNewPath) 
//        //    //{
//        //    //    Path.PO.EndPosition     = targetObject.TargetPosition;
//        //    //    Path.PO.RecreatePath    = true;
//        //    //    Path.findNewPath        = false;
//        //    //}

//        //}

//        ///// <summary>
//        /////     Determines when we want to find a new path
//        ///// </summary>
//        ///// <returns></returns>
//        //protected virtual bool DetermineNewPath() 
//        //{ 
//        //    if (Path.findNewPath) { Path.lastChecked = Time.time; }
//        //    return Path.findNewPath; 
//        //}

//        ///// <summary>
//        /////     Move our character through either animator root motion 
//        /////     or through code
//        ///// </summary>
//        //protected virtual void MoveObject()
//        //{
//        //    movementAction?.Invoke();
//        //    targetObject.Distance = Vector3.Distance(gameObject.transform.position, targetObject.TargetPosition);
//        //}


//    }

//}