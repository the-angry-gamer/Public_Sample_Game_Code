using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaHealth
{

    public class DestroyRagdoll : MonoBehaviour
    {
        [ SerializeField ]
        [ Tooltip("Determine if we want to destroy the objects or keep them forever") ]
        bool keepObjectsAlive = false;

        [ SerializeField ]
        [ Tooltip("Determine how long to wait before destroying the ragdoll from the playing field (seconds)" ) ]
        [ Range ( 0, 600 ) ]
        int timeUntilRemoval = 60;

        [ SerializeField ]
        [ Tooltip( "The original device that turns into this ragdoll" ) ]
        RagdollDeath original;

        public bool isRagdoll { get { return gameObject.activeInHierarchy; } }


        /// <summary>
        ///     Set the object that called the ragdoll
        /// </summary>
        public RagdollDeath Original
        {
            set { original = value; }
        }

        float   timeCreated;
        bool    allowDestroy;
        private void OnEnable()
        {
            timeCreated     = Time.time;
            allowDestroy    = true;
        }

        private void OnDisable()
        {
            allowDestroy = false;
        }

        // Update is called once per frame
        void Update()
        {
            checkForDestruction();
        }

        /// <summary>
        ///     Destroy the objects from the game if we have elapsed time
        /// </summary>
        void checkForDestruction() 
        { 
            if (keepObjectsAlive) { return; }
            if ( Time.time - timeUntilRemoval > timeCreated && allowDestroy ) 
            { 
                if( original) 
                { 
                    if (original.isNested)
                    {
                        Destroy( original.gameObject.transform.parent.gameObject );
                    }
                    else
                    {
                        Destroy(original.gameObject); 
                        Destroy(gameObject); 
                    }
                }
                else
                {
                    Debug.LogError($"The original item has not been set on ragdoll {gameObject.name}");
                }
            }
        }

        /// <summary>
        ///     Reanimate our ragdoll
        /// </summary>
        public void Reanimate()
        {
            if (original && gameObject.activeInHierarchy )
            {
                original.Reanimate();
            }
            else
            {
                Debug.LogError($"The original item has not been set on ragdoll {gameObject.name}");
            }
        }
    }

}