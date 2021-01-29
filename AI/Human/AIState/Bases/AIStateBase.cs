using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace AI_Asteria
{
    public abstract class AIStateBase : MonoBehaviour
    {
        [Header("Generic AI Editor")]
        [SerializeField]
        protected bool allowLogging = true;

        /// <summary>
        ///     The state machine that is governing our scripts
        /// </summary>
        protected AIStateMachine    baseStateMachine;

        protected bool              isAimedAt;

        /// <summary>
        ///     Determines if this object can be shoot at
        /// </summary>

        public virtual void SetStateMachine( AIStateMachine _statemachine) { baseStateMachine = _statemachine; }

        #region For Editor Scripts

        /// <summary>
        ///     The property to hold what the script is associated with
        /// </summary>
        internal abstract AIStateType StateAssociation { get; }
        
        /// <summary>
        ///     An action to add to a button
        /// </summary>
        internal abstract Action DoWork { get; }

        /// <summary>
        ///     What the action is doing if implemented
        /// </summary>
        internal abstract string ActionName { get; }

        /// <summary>
        ///     Get the functions state association
        /// </summary>
        public AIStateType getStateAssociation
        {
            get { return StateAssociation; }
        }

        /// <summary>
        ///     Get the work that is to be done
        /// </summary>
        public Action getWork
        {
            get { return DoWork; }
        }

        /// <summary>
        ///     Return the name of the action to be performed
        /// </summary>
        public string getActionName
        {
            get { return ActionName; }
        }

        #endregion


        #region For Inheritence


        public virtual void             OnEnterState() 
        {
            if (baseStateMachine.Logging) { Debug.Log($"{StateAssociation.ToString()} has been entered by gameobject {gameObject.name}"); }
        }

        public virtual void             OnExitState() 
        {
            if (baseStateMachine.Logging) { Debug.Log($"{StateAssociation.ToString()} has been exited by gameobject {gameObject.name}"); }
        }

        public virtual void             OnAnimatorIKUpdated() { }

        public virtual void             OnDestinationReached(bool isReached) { }

        public abstract AIStateType     OnUpdate();
        public virtual void            OnFixedUpdate()  { }
        public virtual void            OnLateUpdate()   { }
        public virtual void            OnTriggerEvent(  AITriggerEventType eventType,       Collider other) { }

        public virtual void            OnCollisionEvent(AITriggerEventType eventType,       Collision other) { }

        public virtual void            OnStateMachineUpdate() { }

        #endregion


    }
}
