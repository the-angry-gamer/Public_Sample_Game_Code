using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI_Asteria
{
    [RequireComponent( typeof( Collider))]
    public class AISensor : MonoBehaviour
    {

        internal enum SensorType { box, sphere, capsule}

        AIStateMachine stateMachine;

        /// <summary>
        ///     Set the state machine associated with this sensor
        /// </summary>
        internal AIStateMachine StateMachine    { set { stateMachine = value; } }

        internal float          SensorRadius    { get; private set; }

        internal float          SensorHeight    { get; private set; }

        internal Vector3        SensorSize      { get; private set; }


        internal SensorType Type;


        Collider collider;
        

        private void Start()
        {
            collider = GetComponent<Collider>();
            DetermineType();

        }

        /// <summary>
        ///     Determine what type of collider we have
        /// </summary>
        void DetermineType()
        {
            if (collider == null) { Debug.LogError($"There is no collider attached to the game object {gameObject.name}"); return; }

            if ( collider.GetType()      == typeof( SphereCollider ) )
            {
                SensorRadius    = ( ( SphereCollider )collider ).radius;
                Type            = SensorType.sphere;
            }
            else if ( collider.GetType() == typeof( CapsuleCollider ) )
            { 
                SensorRadius    = ( ( CapsuleCollider )collider ).radius;
                SensorHeight    = ( ( CapsuleCollider )collider ).height;
                Type            = SensorType.capsule;
            }
            else if( collider.GetType()  == typeof( BoxCollider ) )
            {
                SensorSize      = ( ( BoxCollider ) collider ).size;
                Type            = SensorType.box;
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (stateMachine != null)
            {
                stateMachine.OnTriggerEvent(AITriggerEventType.Enter, col);
            }
        }

        private void OnTriggerStay(Collider col)
        {
            if (stateMachine != null)
            {
                stateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (stateMachine != null)
            {
                stateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
            }
        }

    }
}
