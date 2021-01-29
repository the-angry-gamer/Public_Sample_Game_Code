using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI_Asteria
{
    [RequireComponent(typeof ( AIRandomStateMachine ) )]
    public abstract class AIRandomState : AIStateBase
    {

        #region Editor

        [Header("AI Random")]
        [Tooltip("The amount of time to wait before shifting to movement")]
        [SerializeField] protected float idleWaitTime                       = 2.0f;
        
        [SerializeField]
        [Tooltip("Determines if we want to look at the main player when it enters ai sensor (requires AI Sensor)")]
        bool lookAtPlayer = true;

        #endregion

        #region Protected

        protected float timeEntered;

        protected AIRandomStateMachine  stateMachine;

        protected AIStateType           returnStateType;

        #endregion
        

        
        public override void OnEnterState()
        {
            base.OnEnterState();
            timeEntered = Time.time;
        }

        public override AIStateType OnUpdate()
        {
            // Determine if we want fleeing AI - Fleeing will always override current state;
            bool fleeing = false;

            if (fleeing)
            {
                returnStateType = AIStateType.Fleeing;
            }
            return returnStateType;
        }
    
        /// <summary>
        ///     Assign the state machine that is handling this script
        /// </summary>
        /// <param name="_statemachine"></param>
        public override void SetStateMachine(AIStateMachine _statemachine) 
        { 
            if (_statemachine.GetType() == typeof( AIRandomStateMachine ) )
            {
                stateMachine = (AIRandomStateMachine)_statemachine;
                base.SetStateMachine(stateMachine);
            }
            else
            {
                Debug.LogError("[AIRandomState.SetStateMachine()] Wrong state machine passed");
            }
        }

        /// <summary>
        ///     Monitors and determines the wait time. Handles resetting it as well
        /// </summary>
        /// <returns>
        ///     A bool if we have gone over and reset it
        /// </returns>
        protected bool passedWaitTime()
        {
            if (timeEntered + idleWaitTime < Time.time)
            {
                timeEntered = Time.time;
                return true;
            }
            return false;
        }


        public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
        {
            if ( eventType == AITriggerEventType.Enter )
            {
                if ( Utilities_AI.checkIfPlayer( other.gameObject ) )
                {
                    if ( !lookAtPlayer ) { stateMachine.LookAtThis = null; }
                    else
                    {
                        stateMachine.LookAtThis = other.gameObject;
                    }
                }
            }
            else if ( eventType == AITriggerEventType.Exit )
            {
                if ( Utilities_AI.checkIfPlayer( other.gameObject ) )
                {
                    stateMachine.LookAtThis = null;
                }

            }

            base.OnTriggerEvent(eventType, other);
        }

    }
}
