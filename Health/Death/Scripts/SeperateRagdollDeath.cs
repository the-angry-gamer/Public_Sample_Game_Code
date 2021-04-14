using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaHealth
{
    [System.Serializable]
    struct associatedVelocity
    {
        [SerializeField]
        public HumanHitTypes    hitType;
        [SerializeField][Range(0f, 50f)]
        public float            velocity;

        public associatedVelocity(HumanHitTypes type, float vel) { hitType = type; velocity = vel; }
    }


    public class SeperateRagdollDeath : MonoBehaviour, IDeath
    {

        #region Editor

        [Tooltip("Enable Logging")]
        [SerializeField] bool enableLogging;

        [SerializeField]
        [Tooltip("Determine if this object is nested underneath another object that needs to also be removed")]
        bool nestedUnderParent = false;
        
        [Tooltip("The object that is the ragdoll for this same character")]
        [SerializeField] GameObject ragdoll;

        [Tooltip("This is the item that will turn into the ragdoll. If left blank, will inherit the item the script is on")]
        [SerializeField] GameObject sourceCharacter;

        [SerializeField]
        [Tooltip("The velocity multiplier added to an object was hit for the kill")]
        List<associatedVelocity> VelocityMultipliers = new List<associatedVelocity>() { new associatedVelocity(HumanHitTypes.Head, 10.0f) };



        #endregion


        #region Private Declarations

        Rigidbody myRigid;
        HealthItem killedAtPoint;

        #endregion


        #region Properties

        /// <summary>
        ///     Determines if this object is nested under
        ///     a parent gameobject or not
        /// </summary>
        public bool isNested { get { return nestedUnderParent; } }

        /// <summary>
        ///     Determines if our ragdoll has been activated
        /// </summary>
        public bool isRagdoll
        {
            get
            {
                if (!sourceCharacter) { return false; }
                return !sourceCharacter.activeInHierarchy;
            }
        }

        /// <summary>
        ///     Determines if the source character has been set
        /// </summary>
        public bool isSourceSet
        {
            get
            {
                return (sourceCharacter != null);
            }
        }

        #endregion


        #region MonoBehavoir

        void Awake()
        {
            if (ragdoll == null)
            {
                if (enableLogging) { Debug.LogError($"There is no ragdoll on {sourceCharacter.name}. Removing script"); }
                Destroy(this);
                return;
            }

            ragdoll.SetActive(false);

            if (sourceCharacter == null)
            {
                sourceCharacter = gameObject;
            }
        }

        void Start()
        {
            myRigid = sourceCharacter.GetComponent<Rigidbody>();

            if (!myRigid)
            {
                if (enableLogging) { Debug.LogError($"There is no rigid body for the ragdoll death of {sourceCharacter.name}. Removing script"); }
                Destroy(this);
            }
            if (!ragdoll)
            {
                if (enableLogging) { Debug.LogError($"There is no ragdoll on {sourceCharacter.name}. Removing script"); }
                Destroy(this);
            }
        }

        #endregion


        #region Interface IDeath

        public void isDead(HealthItem hitType)
        {
            killedAtPoint = hitType;
            CreateRagdoll();
        }

        #endregion


        #region Utilities


        #region Custom editor


        public void EnableRagdoll() { CreateRagdoll(); }

        #endregion

        /// <summary>
        ///     Create the ragdoll events
        /// </summary>
        void CreateRagdoll()
        {
            copyTransformsToRagdoll(sourceCharacter.transform, ragdoll.transform);

            checkRagdollRequirements();

            sourceCharacter.SetActive(false);
            ragdoll.SetActive(true);
        }

        /// <summary>
        ///     Make sure all of our requirements for hte 
        ///     ragdoll are in place
        /// </summary>
        void checkRagdollRequirements()
        {
            var r = ragdoll.GetComponent<DestroyRagdoll>();
            if ( r == null )
            {
                r = ragdoll.AddComponent<DestroyRagdoll>();
                r.Original = this;
            }
            else
            {
                r.Original = this;
            }
        }

        /// <summary>
        ///     Copy over all the positions to their ragdoll counterparts.
        ///     This is a recursive function that will also do each transforms
        ///     child objects
        /// </summary>
        /// <param name="sourceTransform">      The source were are editing                 </param>
        /// <param name="destinationTransform"> The destination we are changing ( ragdoll ) </param>
        void copyTransformsToRagdoll(Transform sourceTransform, Transform destinationTransform)
        {
            if (sourceTransform.transform.childCount != destinationTransform.transform.childCount)
            {
                if (enableLogging) { Debug.LogError($"The {sourceTransform.name} does not have the same child count as destination {destinationTransform.name}"); }
            }

            int sourceOffset = 0;
            for (int i = 0; i < destinationTransform.transform.childCount; i++)
            {
                if (sourceTransform.childCount < i + sourceOffset) { break; }  // there has been an issue

                var source = sourceTransform.transform.GetChild(i + sourceOffset);
                var destination = destinationTransform.transform.GetChild(i);

                if (source.name == destination.name)
                {
                    destination.position = source.position;
                    destination.rotation = source.rotation;

                    var rb = destination.GetComponent<Rigidbody>();

                    if (rb)
                    {
                        rb.isKinematic = false;
                        var v = myRigid.velocity;
                        v += AddDirectionalForce(destination);
                        rb.velocity = v;
                    }
                }
                else
                {
                    sourceOffset++; i--;    // We have an extra item in the source
                }
                copyTransformsToRagdoll(source, destination);
            }
        }

        
        /// <summary>
        ///     Determine if we were hit by something we 
        ///     should accomadate force for. Adds velocity
        ///     in the opposite direction we are hit
        /// </summary>
        /// <param name="name"> The name of the object to check for accuracy    </param>
        /// <returns>
        ///     A relevant force in the direction opposite the hit
        /// </returns>
        Vector3 AddDirectionalForce(Transform hit)
        {                        
            if ( killedAtPoint && hit.name == killedAtPoint.gameObject.name )
            {
                float multi = 1f;

                foreach(associatedVelocity av in VelocityMultipliers)
                {   
                    if ( av.hitType == killedAtPoint.hitType )
                    {
                        multi = av.velocity;                        
                    }
                }

                var force   = (hit.position - killedAtPoint.HitPosition);
                var forcen  = force.normalized; var forcem  = forcen * multi;

                if (enableLogging) {Debug.Log($"Hit Type {killedAtPoint.hitType} on object {hit.name} ({gameObject.name}) was hit for a force of " +
                            $"{ force } (normalized at: {forcen}) by multiplier {multi} for a total of {forcem}"); }

                return forcem;
                
            }
            return new Vector3(0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        ///     Go through and set all the kinematics 
        ///     to the most efficient means
        /// </summary>
        /// <param name="set">  What to set the kinematic to    </param>
        void SetKinematic( Transform toSet, bool set)
        {
            for (int i = 0; i < toSet.childCount; i++)
            {


                SetKinematic(toSet.GetChild(i), set);
            }
        }

        /// <summary>
        ///     Reanimate the character with a bit of health
        /// </summary>
        public void Reanimate()
        {
            if (sourceCharacter) 
            { 
                sourceCharacter.SetActive(true);
                var rev = sourceCharacter.GetComponent<HealthManager>();
                var mot = sourceCharacter.GetComponent<Human_Controller.HumanMotor>();
                if (rev && mot) { rev.Revive(10.0f); mot.KillMe(0); }

                ragdoll.SetActive(false); 
            }

        }

        #endregion

    }

}