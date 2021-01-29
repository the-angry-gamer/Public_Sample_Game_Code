//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//namespace AI
//{

//    enum RandomType
//    {
//        Walking,
//        Flying
//    }

   

//    public class AIRandom : AIBase
//    {
//        #region Editor

//        [Tooltip("Detemrine what sort of random movement we want")]
//        [SerializeField]
//        RandomType MovementType = RandomType.Walking;

//        [Tooltip("Use a timer to determine when to re-calculate the path")]
//        [SerializeField]
//        bool UseTimer = false;

//        [Tooltip("How long to wait until a new path is found")]
//        [SerializeField]
//        float timer = 10.0f;

//        [Tooltip("Use distance to objective to determine new path. This is the default")]
//        [SerializeField]
//        float distance = 1.0f;

//        [Tooltip("Determines if we want to ignore the fact that there is no animator and just move the object along")]
//        [SerializeField]
//        bool IgnoreAnimator = false;
//        #endregion


//        // Start is called before the first frame update
//        void Start()
//        {
//            AIstate = new AIStateRandom();
//        }

//        // Update is called once per frame
//        void Update()
//        {
//            try
//            {
//                MoveObject();

//                DetermineNewPath();
//            }
//            catch (Exception exc)
//            { 
//                Debug.LogError( $"An unhandled exception occured in the Update of AI.AIRandom(): {exc.Message}");
//            }
//        }

        

//        /// <summary>
//        ///     Move our object
//        /// </summary>
//        protected void MoveObject()
//        {
//            if ( !IgnoreAnimator && animator == null ) { return; }

//            if (MovementType == RandomType.Flying )
//            {
//                movementAction = BasicMovementFlying;
//            }
//            else if ( MovementType == RandomType.Walking)
//            {
//                movementAction = BasicMovementWalking;
//            }
//            else
//            {
//                movementAction = BasicMovementWalking;
//            }
            
//        }

//        void BasicMovementFlying()
//        {

//        }

//        /// <summary>
//        ///     Run the basic movements
//        /// </summary>
//        void BasicMovementWalking()
//        {
//            if (UseRootMotion)
//            {

//            }
//            else
//            {

//            }
//            UpdateAnimator();
//        }

//        /// <summary>
//        ///     Move the attached object using animator root motion
//        /// </summary>
//        void MoveRootMotion()
//        {
            
//        }

//        /// <summary>
//        ///     Move the attached object using code
//        /// </summary>
//        void MoveNoneRootMotion()
//        {

//        }

//        void UpdateAnimator()
//        {
//            if (IgnoreAnimator || animator == null)     { return; }
            
//            // Update the animator with the correct values
//        }

//    }
//}
