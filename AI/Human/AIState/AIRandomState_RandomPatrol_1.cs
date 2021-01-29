using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Asteria
{
    [RequireComponent(typeof(Human_Controller.ObstacleAwareness))]
    public class AIRandomState_RandomPatrol_1 : AIRandomState
    {
        #region Editor
        
        [Header("Triggers")]
        [SerializeField]
        [Tooltip("Manually update the values to control the object")]
        bool randomStart                   = false;

        [Header("Controls")]


        [SerializeField]
        Vector2 randomSpeed                 = new Vector2(0.3f, 0.4f);

        [SerializeField]
        [Range(0, 1.5f)]
        float speed                         = 0.4f;

        [SerializeField]
        [Range(0, 1)] float rotationLerp    = 0.5f;

        [SerializeField]
        [Range(-180,180)]
        int rotation;


        [Header("Randomness")]
        [SerializeField]
        [Tooltip("The range to adjust when we hit an obstacle")]
        Vector2Int rotationAdjust   = new Vector2Int(20, 100);

        [SerializeField]
        [Tooltip("How long to wait before checking for a random rotation")]
        float timeBeforeUpdate      = 10;

        #endregion


        #region Internal Declarations

        internal override AIStateType StateAssociation => AIStateType.Patrol;

        internal override Action DoWork => null;

        internal override string ActionName => "";

        #endregion
        
        
        #region Private Declarations

        float       _lastRotate;
        bool        playerBlocking;

        #endregion


        #region Overrides

        /// <summary>
        ///     When the collider is entered
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="other"></param>
        public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
        {            
            if (eventType == AITriggerEventType.Enter)
            {

            }
            else if (eventType == AITriggerEventType.Stay)
            {
                CheckSurroundings();
            }
            else if (eventType == AITriggerEventType.Exit)
            {
                //if ( other.gameObject.CompareTag( "Player" ) )
                //{
                //    stateMachine.LookAtThis = null;
                //}
            }
            base.OnTriggerEvent(eventType, other);
        }

        /// <summary>
        ///     Check our surroundings to see if we 
        ///     are about to hit anything
        /// </summary>
        void CheckSurroundings()
        {
            if (rotation < 5.0f)
            {
                var checks = stateMachine.obstacleAwareness.AllHeightItems;

                bool left = false, front= false, right = false;
                foreach ( var check in checks )
                {
                    if (check.obstacles.left.collision)     { left  = true; }
                    if (check.obstacles.right.collision)    { right = true; }
                    if (check.obstacles.front.collision)    { front = true; }

                    if ( Utilities_AI.checkIfPlayer(check.obstacles.left.firstObjHit) 
                            || Utilities_AI.checkIfPlayer(check.obstacles.right.firstObjHit) || Utilities_AI.checkIfPlayer(check.obstacles.left.firstObjHit ) )
                    {
                        playerBlocking = true; return;
                    }
                    else
                    {
                        playerBlocking = false;
                    }
                }

                int dir = UnityEngine.Random.Range(-1, 2);
                if (!left && right)         { rotation = UnityEngine.Random.Range( -rotationAdjust.x, -rotationAdjust.y );  }
                else if (left && !right)    { rotation = UnityEngine.Random.Range( rotationAdjust.x, rotationAdjust.y   );  }
                else if (left && right)     { rotation = 180 * dir; }
                else if (front)             { rotation = 180 * dir; }
                    
                _lastRotate = Time.time;
                if ( allowLogging && (rotation > 5.0f ||  rotation < -5.0f ) ) { Debug.Log($"The determined rotation was {rotation}"); }

            }
        }

        public override void OnEnterState()
        {
            setIniitial();
            updateStateController();
        }

        public override void OnLateUpdate()
        {
            
        }
        
        public override void OnExitState()
        {
            stateMachine.checkObstacles = false;
            stateMachine.Speed          = 0;
            stateMachine.ClearTarget();
        }

        public override AIStateType OnUpdate()
        {
            randomRotation();
            updateStateController();
            return determineNextState();
        }

        #endregion


        #region Utilities        


        /// <summary>
        ///     Determine if we want to switch states
        /// </summary>
        /// <returns></returns>
        AIStateType determineNextState()
        {
            if (playerBlocking)
            {
                return AIStateType.Idle;
            }

            return AIStateType.Patrol;
        }


        /// <summary>
        ///     Determine a random rotation
        /// </summary>
        void randomRotation()
        {
            if ( Time.time - timeBeforeUpdate > _lastRotate)
            {
                int c       = ( ( int )UnityEngine.Random.Range(-1, 3 ) ) > 0 ? 1 : -1;
                
                rotation    = UnityEngine.Random.Range(rotationAdjust.x, rotationAdjust.y) * c;
                _lastRotate  = Time.time;
            }
        }

        /// <summary>
        ///     Set our initial values
        /// </summary>
        void setIniitial()
        {
            if (randomStart)
            {
                setRandomSpeed();
            }
        }

        /// <summary>
        ///     Get a random speed 
        /// </summary>
        void setRandomSpeed()
        {            
            speed = UnityEngine.Random.Range(randomSpeed.x, randomSpeed.y);
        }

        /// <summary>
        ///     Update the state controller with our movements
        /// </summary>
        void updateStateController()
        {
            stateMachine.SetTarget(AITargetType.Node, determineDirection(), 1.0f);
            stateMachine.Speed = speed;
        }

        /// <summary>
        ///     Determine our angle of movement
        /// </summary>
        /// <returns>
        ///     The direction that we want to go
        /// </returns>
        Vector3 determineDirection()
        {
            Vector3 v = new Vector3();

            v = Quaternion.Euler(0, rotation, 0) * transform.forward;
            v = transform.position + v;
            // TODO Lerp toward vector
            rotation = (int)Mathf.Lerp((float)rotation, 0, rotationLerp);

            // draw me
            if ( allowLogging ) { var o = new Vector3(0, .3f, 0); ; Debug.DrawLine(transform.position + o, v + o, Color.cyan); }

            return v;
        }

        #endregion
    }
}