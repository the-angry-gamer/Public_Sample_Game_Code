using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaEnvironment
{

    /// <summary>
    ///     Allow an object to slowly rotate towards another object.
    ///     Meant for a stationary object, as original fronts and ups
    ///     are are cached to determine overall movement
    /// </summary>
    public class LookAtTarget : MonoBehaviour
    {
        #region Editor

        [SerializeField][Tooltip("The speed to rotate the object over time")]
        float    rotationSpeed  = 1;

        [SerializeField]
        [Tooltip("How far the object can rotate")]
        [Range(0,180)]
        float    maxRotation    = 90;

        [SerializeField]
        [Tooltip("Determines if we want to hold our position at max or return straight")]
        bool     returnStraight = false;

        [SerializeField]
        [Tooltip("The target we want to look at")]
        GameObject target;

        [SerializeField]
        [Tooltip("Determine if we want to use a collider entry and exit for targe")]
        bool        useTrigger = true;

        [SerializeField]
        [Tooltip("Add in the tags of items we want to look at, case insensitive")]
        List<string> LookAtTags;


        #endregion


        #region Properties

        /// <summary>
        ///     The name of the object we are currently 
        ///     targeting
        /// </summary>
        /// <value>The name of the object</value>
        public string ObjectName
        {
            get
            {
                return target == null ? "No target" : target.name;
            }
        }

        /// <summary>
        ///     The tag assigned to the target
        /// </summary>
        /// <value>The object tag.</value>
        public string ObjectTag
        {
            get
            {
                return target == null ? "No target" : target.tag;
            }
        }

        /// <summary>
        ///     The current rotation the object needs to achieve
        ///     to be staring at the target along the y axis
        /// </summary>
        /// <value>The current rotation.</value>
        public float TargetRotation
        {
            get;
            private set;
        }

        /// <summary>
        ///     Where this object is currently rotated towards
        /// </summary>
        /// <value>The current rotation.</value>
        public float CurrentRotation
        {
            get
            {
                var angle = Mathf.Round( transform.rotation.eulerAngles.y );
                return angle <= 180 ? -angle : 360 - angle;
            }
        }

        public float RequiredRotation
        {
            get;
            private set;
        }

        #endregion 


        #region Private Cache

        Vector3 startForward;
        Vector3 startUp;
        Vector3 originalPos;

        #endregion


        #region Unity Mono

        // Start is called before the first frame update
        void Start()
        {
            // Cache me outside
            startForward    = transform.forward;
            originalPos     = gameObject.transform.position;
            startUp         = gameObject.transform.up;

        }

       
        // Update is called once per frame
        void Update()
        {
            float angleToTarget = 0.0f;
            RequiredRotation    = 0.0f;
            if (target != null)
            {
                Vector3 targetDir   = target.transform.position - originalPos;
                Vector3 forward     = startForward;
                angleToTarget       = Vector3.SignedAngle(targetDir, forward, startUp);

                RequiredRotation    = Mathf.Round( angleToTarget );
                angleToTarget       = LockRotation(angleToTarget);
            }
            TargetRotation      = Mathf.Round( angleToTarget );
            transform.rotation  = Quaternion.Slerp(transform.rotation,
                    Quaternion.Euler(0, -angleToTarget, 0), Time.deltaTime * rotationSpeed);

        }

        #endregion


        #region Collisions

        /// <summary>
        ///     When we enter the collider
        /// </summary>
        /// <param name="collision">Collision</param>
        private void OnTriggerEnter(Collider collision)
        {
            if (!useTrigger) { return; }

            var entry = collision.gameObject;

            if (target == null && entry != gameObject && CIContains(LookAtTags, entry.tag))
            {
                //Debug.Log(entry.name + " has entered the arena");
                target = entry;
            }
        }


        private bool CIContains(List<string> list, string value)
        {
            foreach (string s in list)
            {
                return s.ToLower().Equals(value.ToLower());
            }
            return false;
        }


        /// <summary>
        ///     When a gameobject exits our collider
        /// </summary>
        /// <param name="collision">Collision.</param>
        private void OnTriggerExit(Collider collision)
        {
            if (!useTrigger) { return; }
            if ( target == collision.gameObject )
            {
                //Debug.Log(target.name + " has exited the arena");
                target = null;
            }
        }

        #endregion


        #region Utilities

        /// <summary>
        ///     Locks the rotation into place
        /// </summary>
        /// <returns> A rotation allowed by the rules.</returns>
        /// <param name="rot"> the rotation to check.</param>
        float LockRotation(float rot)
        {
            var temp    = rot;
            bool locked = false;

            if (rot > maxRotation)
            {
                temp = maxRotation;
                locked = true;
            }
            else if (rot < -maxRotation)
            {
                temp = -maxRotation;
                locked = true;
            }

            if (locked && returnStraight)
            {
                temp = 0.0f;
            }

            return temp;
        }
        #endregion
    }    

}