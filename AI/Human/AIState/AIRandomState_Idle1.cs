using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Asteria
{
    /// <summary>
    ///     The idle movement for the random character
    /// </summary>
    public class AIRandomState_Idle1 : AIRandomState
    {
        /// <summary>
        ///     The state associated with this script
        /// </summary>
        internal override AIStateType StateAssociation { get; } = AIStateType.Idle;

        internal override Action DoWork { get; } = null;

        internal override string ActionName { get; } = string.Empty;

        /// <summary>
        ///     This needs to be in awake so it triggers prior
        ///     the state machines start function 
        /// </summary>
        private void Awake()
        {
            
        }

        /// <summary>
        ///     When the random npc enters the idle state
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            updateMachine();
        }

        public override AIStateType OnUpdate()
        {
            returnStateType = determineStateChange();
            base.OnUpdate();
            if (stateMachine == null) 
            {
                Debug.LogError("The state machine is not set.");    
                return AIStateType.Idle; 
            }
            
            return returnStateType;
        }

        /// <summary>
        ///     Update the values of the state machine
        /// </summary>
        void updateMachine()
        {
            stateMachine.Speed = 0.0f;
            stateMachine.ClearTarget();            
        }

        /// <summary>
        ///     Determine if we want to change states
        /// </summary>
        /// <returns></returns>
        AIStateType determineStateChange()
        {

            if ( passedWaitTime() ) 
            {
                return AIStateType.Patrol;
            }

            return AIStateType.Idle;
        }

    }
}
