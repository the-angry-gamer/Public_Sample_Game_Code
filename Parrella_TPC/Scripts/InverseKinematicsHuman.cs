using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Human_Controller
{
    [RequireComponent(typeof(HumanController))]
    [RequireComponent(typeof(Animator))]
    public class InverseKinematicsHuman : MonoBehaviour
    {
        #region Editor
        [Header("Turn on / off IK features")]
        [Tooltip("Turn on / off gun IK mechanics. Requires a ")]
        [SerializeField] bool IKGunHands    = true;
        [SerializeField] bool IKFeet        = true;
        [SerializeField] bool HeadMovement  = true;
        [SerializeField] bool EyeMovement   = true;
        [SerializeField] bool ChestMovement = false;
        [SerializeField] bool enableLogging = false;


        [Header("Weapon Parameters")]
        [SerializeField] bool lockLeftHand  = true;

        [Header("Feet Parameters")]
        [Tooltip("Calculate the feet position on the late update. If false it will calculate during update")]
        [SerializeField]                bool        feetFixedUpdate              = true;
        [Range(0,2)][SerializeField]    float       heightFromGroundRCast       = 0.50f;
        [Range(0,2)][SerializeField]    float       rayCastDistance             = 0.60f;
        [SerializeField]                float       pelvisOffset                = 0.00f;
        [Range(0,1)][SerializeField]    float       pelvisLerpSpeed             = 0.25f;
        [Range(0,1)][SerializeField]    float       feetLerpSpeed               = 0.50f;
        [SerializeField]                LayerMask   feetHitLayer                = ~0;
        [Tooltip("Use the advanced feet curvature rules")]
        [SerializeField]                bool        useAdvancedCurves           = false;
        [SerializeField]                string      leftFootAnimVariable        = "LeftFootCurve";
        [SerializeField]                string      rightFootAnimVariable       = "RightFootCurve";

        [Header("Head Parameters")]
        [Tooltip("Allows the head to rotate towards camera")]
        [SerializeField] bool    rotateHeadWithMouse         = true;
        [Tooltip("Move the head while crouching")]
        [SerializeField] bool    headRotatationCrouch        = false;

        [Tooltip("Determines if we want to be able to look at an object")]
        [SerializeField] bool    lookAtObject            = false;
        [Tooltip("The object to rotate the heads towards")]
        [SerializeField] Vector3 objectToLookAt;

        [Range(0,360)]
        [SerializeField] int     maxHeadLeft             = 60;
        [Range(0,360)]
        [SerializeField] int     maxHeadRight            = 60;
        [Range(0,360)]
        [SerializeField] int     maxHeadUp               = 50;
        [Range(0,360)]
        [SerializeField] int     maxHeadDown             = 50;
        [Range(0,360)]
        [SerializeField] int     resetHeadRight          = 150;
        [Range(0,360)] 
        [SerializeField] int     resetHeadLeft           = 150;
        [Range(0,1)]
        [Tooltip("How fast to move the head")]
        [SerializeField] float   headMovementSpeed       = 0.5f;
        [Tooltip("The amount of angle to wait before rotating back to start")]
        [Range(0,360)] 
        [SerializeField] int     headBuffer              = 0;

        [Header("Rotations - Eyes")]
        [Tooltip("Allows the eyes to rotate towards camera")]
        [Range(0,180)]
        [SerializeField] int     maxEyeHorizontal        = 20;
        [Range(0,180)]
        [SerializeField] int     maxEyeVertical          = 5;

        #endregion


        #region Private

        HumanController                 human;
        AsteriaWeapons.WeaponManager    weapons;
        HumanoidBodyParts               BodyParts;
        Animator                        animator;

        // Feet and hips
        Vector3         rightFootPos, leftFootPos, leftFootIKpos, rightFootIKPos;
        Quaternion      leftFootIKRot, rightFootIKRot;
        private float   lastPelvisPositionY, lastRightPosY, lastLeftPosY;

        #endregion


        #region Properties

        internal Vector3 ObjectToLookAt
        {
            set { objectToLookAt = value; }
        }

        internal bool LookAtObject
        {
            set { lookAtObject = value; }
        }

        internal bool AllowEyesMovement
        {
            set { EyeMovement = value; }
        }
        #endregion


        #region Monobehavoir

        // Start is called before the first frame update
        void Start()
        {
            human   = GetComponent<HumanController>();
            weapons = GetComponent<AsteriaWeapons.WeaponManager>();

            if (!human) { Destroy(this); return; }

            human.ik            = this;
            human.IKGunHands    = IKGunHands;

            animator            = GetComponent<Animator>();
            if (!animator) { Destroy(this); return; }
            BodyParts           = new HumanoidBodyParts(animator);
        }


        private void OnAnimatorIK(int layerIndex)
        {
            if ( !human     ) { return; }
            if ( weapons    ) { updateGunHands(); }
            updateFeetLocations();            
        }


        private void LateUpdate()
        {
            if (weapons) { LockLeftHand(); }
            updateBody();
        }


        private void FixedUpdate()
        {
            if (animator == null) { return; }

            if (feetFixedUpdate) { determineFeetLocations(); }
        }

        private void Update()
        {
            if (animator == null) { return; }

            if ( !feetFixedUpdate ) { determineFeetLocations(); }
        }

        #endregion


        #region Legs and Feet

        /// <summary>
        ///     Update the location of the feet
        /// </summary>
        void updateFeetLocations()
        {
            if ( !IKFeet )  { return; }
            if ( !human.isGrounded || human.isDiving )    { return; }

            MovePelvisHeight();

            // Left Foot
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            if (useAdvancedCurves)
            {
                animator.SetIKRotationWeight( AvatarIKGoal.LeftFoot, animator.GetFloat( leftFootAnimVariable ) );
            }
            MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKpos, leftFootIKRot, ref lastLeftPosY);

            // Right Foot
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            if (useAdvancedCurves)
            {
                animator.SetIKRotationWeight( AvatarIKGoal.RightFoot, animator.GetFloat( rightFootAnimVariable ) );
            }
            MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPos, rightFootIKRot, ref lastRightPosY);
        }

        /// <summary>
        ///     Do the math to determine the location of the feet / pelvis
        ///     so the animator / IK can put them in the correct positions
        /// </summary>
        void determineFeetLocations()
        {
            if ( !IKFeet ) { return; }

            adjustFeetTarget( ref rightFootPos,  HumanBodyBones.RightFoot   );
            adjustFeetTarget( ref leftFootPos,   HumanBodyBones.LeftFoot    );

            feetPositionSolver( rightFootPos,   ref rightFootIKPos, ref rightFootIKRot  );
            feetPositionSolver( leftFootPos,    ref leftFootIKpos,  ref leftFootIKRot   );
        }

        /// <summary>
        ///     Move the feet to the inverse kinematics pos
        /// </summary>
        /// <param name="foot"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="lastFootPosY"></param>
        void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 position, Quaternion rotation, ref float lastFootPosY)
        {
            var targetIKPosition = animator.GetIKPosition(foot);

            if (position != Vector3.zero)
            {
                targetIKPosition    = transform.InverseTransformPoint(targetIKPosition);
                position            = transform.InverseTransformPoint(position);

                float yVar = Mathf.Lerp(lastFootPosY, position.y, feetLerpSpeed );
                targetIKPosition.y += yVar;

                lastFootPosY        = yVar;
                targetIKPosition    = transform.TransformPoint(targetIKPosition);
                animator.SetIKRotation(foot, rotation);
            }

            animator.SetIKPosition(foot, targetIKPosition);

        }

        /// <summary>
        ///     Update the pelvis location aka the whole body pos
        ///     to match where the feet have gone
        /// </summary>
        void MovePelvisHeight()
        {
            if (rightFootIKPos == Vector3.zero || leftFootIKpos == Vector3.zero || lastPelvisPositionY == 0)
            {
                lastPelvisPositionY = animator.bodyPosition.y;
                return;
            }

            float lOffsetPos = leftFootIKpos.y  - transform.position.y;
            float rOffsetPos = rightFootIKPos.y - transform.position.y;

            float totalOffset = (lOffsetPos < rOffsetPos) ? lOffsetPos : rOffsetPos;

            var newPelvisPos = animator.bodyPosition + (Vector3.up * totalOffset);

            newPelvisPos.y          = Mathf.Lerp(lastPelvisPositionY, newPelvisPos.y, pelvisLerpSpeed);
            animator.bodyPosition   = newPelvisPos;
            lastPelvisPositionY     = animator.bodyPosition.y;
        }

        /// <summary>
        ///     Set the feet position using raycast handling
        /// </summary>
        /// <param name="heightPos">    Where facing down we shoot the raycast from     </param>
        /// <param name="feetIKPos">    The position of the foot                        </param>
        /// <param name="feetIKRot">    The rotation of the foot                        </param>
        void feetPositionSolver(Vector3 heightPos, ref Vector3 feetIKPos, ref Quaternion feetIKRot)
        {
            if (enableLogging)
            {
                Debug.DrawLine(heightPos, heightPos + Vector3.down * (rayCastDistance + heightFromGroundRCast), Color.blue);
            }

            if( Physics.Raycast( heightPos, Vector3.down, out RaycastHit feetHit, rayCastDistance + heightFromGroundRCast, feetHitLayer ) )
            {
                feetIKPos = heightPos;
                feetIKPos.y = feetHit.point.y + pelvisOffset;
                feetIKRot = Quaternion.FromToRotation(Vector3.up, feetHit.normal) * transform.rotation;
                
                return;
            }

            feetIKPos = Vector3.zero;
        }

        /// <summary>
        ///     Adjust the feet to the position of the ground   
        /// </summary>
        /// <param name="feetPos"></param>
        /// <param name="foot"></param>
        void adjustFeetTarget(ref Vector3 feetPos, HumanBodyBones foot)
        {
            feetPos     = animator.GetBoneTransform(foot).position;
            feetPos.y   = transform.position.y + heightFromGroundRCast;
        }
        #endregion


        #region Weapons Locks

        /// <summary>
        ///     Update the hands to the correct positions
        /// </summary>
        void updateGunHands()
        {
            if ( IKGunHands )
            {
                var GunHandL    = human.GunHandL;

                if ( !human.isWeaponEquipped || !GunHandL ) { return; }

                var animator    = human.animator;
                var weight      = human.isReloading ? 0.0f : 1.0f; // if reloding we want 0 weight to IK


                if ( human.isADS )
                {
                    var GunHandR = human.GunHandR;
                    if (GunHandR)
                    {
                        animator.SetIKPositionWeight(   AvatarIKGoal.RightHand, weight);
                        animator.SetIKRotationWeight(   AvatarIKGoal.RightHand, weight);
                        animator.SetIKPosition(         AvatarIKGoal.RightHand, GunHandR.transform.position);
                        animator.SetIKRotation(         AvatarIKGoal.RightHand, GunHandR.transform.rotation);
                    }
                }
                animator.SetIKPositionWeight(           AvatarIKGoal.LeftHand, weight      );
                animator.SetIKRotationWeight(           AvatarIKGoal.LeftHand, weight      );
                animator.SetIKPosition(                 AvatarIKGoal.LeftHand, GunHandL.transform.position );
                animator.SetIKRotation(                 AvatarIKGoal.LeftHand, GunHandL.transform.rotation );
            }
        }


        /// <summary>
        ///     Lock the left hand into position
        /// </summary>
        void LockLeftHand()
        {
            if (!lockLeftHand) { return; }
            if (human.isWeaponEquipped && human.GunHandL && human.isADS && !human.isReloading)
            {
                BodyParts.LeftHand.position = human.GunHandL.transform.position;  // force the hand to lock into place               
            }
        }



        #endregion


        #region Body

        /// <summary>
        ///     Move certain parts of the body
        /// </summary>
        void updateBody()
        {
            updateHead();
        }

        /// <summary>
        ///     Update head movement
        /// </summary>
        void updateHead()
        {
            if (HeadMovement)
            {
                HeadRotation();
            }
            if (EyeMovement)
            {
                MoveEyes();
            }
            if (ChestMovement)
            {
                ChestRotation();
            }
        }

        
        /// <summary>
        ///     Rotate the player head
        /// </summary>
        internal void HeadRotation()
        {
            if (human.isCrouched && !headRotatationCrouch || human.isDiving) { return; }

            if (BodyParts.PlayerHead == null) { return; }

            var originalrot = BodyParts.PlayerHead.transform.localEulerAngles;
            if ( lookAtObject )
            {
                //Vector3 targetDir = objectToLookAt.transform.position - transform.position;
                //var angle = Vector3.SignedAngle(targetDir, transform.forward, transform.up);
                //BodyParts.PlayerHead.rotation = Quaternion.Slerp(BodyParts.PlayerHead.rotation,
                //        Quaternion.Euler(0, -angle, 0), Time.deltaTime * 0.1f);
                //return;
                BodyParts.PlayerHead.LookAt(objectToLookAt, transform.up);

            }
            else if (human.AIEntity || !rotateHeadWithMouse)
            {
                return;
            }
            else
            {
                BodyParts.PlayerHead.transform.rotation = human.cameraPointingDirection;
            
            }

            var localRotation = BodyParts.PlayerHead.transform.localEulerAngles;

            // Check Y
            localRotation = ClampYAngles(original: localRotation, preUpdate: originalrot, right: maxHeadRight, left: maxHeadLeft, resetRight: resetHeadRight, resetLeft: resetHeadLeft, buffer: headBuffer);

            // Check X
            localRotation = ClampXAngles(original: localRotation, high: maxHeadUp, low: maxHeadDown);
            
            // Set New
            BodyParts.PlayerHead.localEulerAngles = localRotation;
            
        }


        /// <summary>
        ///     Move the characters eyes
        /// </summary>
        private void MoveEyes()
        {

            List<Transform> eyes = new List<Transform>() { BodyParts.PlayerLeftEye, BodyParts.PlayerRightEye };
            
            foreach (Transform eye in eyes)
            {
                if (eye == null) break;

                if (lookAtObject)
                {
                    eye.LookAt(objectToLookAt, transform.up);

                }
                else
                {
                    eye.transform.rotation  = human.cameraPointingDirection;
                }

                var localRotation       = eye.transform.localEulerAngles;
                
                // Check Horizontal
                localRotation = ClampYAngles(original: localRotation, preUpdate: new Vector3(0,0,0), right: maxEyeHorizontal, left: maxEyeHorizontal, resetRight: resetHeadRight, resetLeft: resetHeadLeft, buffer: headBuffer);

                localRotation = ClampXAngles( original: localRotation, high: maxEyeVertical, low: maxEyeVertical );

                eye.localEulerAngles = localRotation;            
            }            
        }

        /// <summary>
        ///     Rotate the player chest
        /// </summary>
        private void ChestRotation()
        {
            if (BodyParts.UpperChest == null) { return; }

            BodyParts.UpperChest.transform.rotation = human.cameraPointingDirection;
            BodyParts.UpperChest.transform.localEulerAngles = new Vector3(BodyParts.UpperChest.localEulerAngles.x + human.ChestOffsets.x,
                    BodyParts.UpperChest.localEulerAngles.y + human.ChestOffsets.y,
                    BodyParts.UpperChest.localEulerAngles.z + human.ChestOffsets.z);
            
        }

        #endregion


        #region Maths
        /// <summary>
        ///     Clamp the angles along the y axis
        /// </summary>
        /// <param name="original"></param>
        /// <param name="preUpdate"></param>
        /// <param name="right"></param>
        /// <param name="left"></param>
        /// <param name="resetRight"></param>
        /// <param name="resetLeft"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private Vector3 ClampYAngles(Vector3 original, Vector3 preUpdate ,float right, float left, float resetRight, float resetLeft, float buffer)
        {
            try
            {
                if (original.y < headBuffer || original.y > ( 360 - buffer) )
                {
                    original = preUpdate;
                }
                else if (original.y > resetRight && original.y < (360 - resetLeft))
                {
                    // Looking directly behind me, no rotation
                    original = new Vector3(0, 0, 0);
                }
                else if (original.y > right && original.y < resetRight)
                {
                    original.y = right;
                }
                else if (original.y > right && original.y < (360 - left))
                {
                    original.y = ( (360 - buffer) - left );
                }
                return original;
            }
            catch { }
            return new Vector3(0,0,0);
        }

        /// <summary>
        ///     Clamp the angles along the x axis
        /// </summary>
        /// <param name="original"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public Vector3 ClampXAngles(Vector3 original, int high, int low)
        {
            try
            {
                if( original.x < high || original.x > 360 - low )
                {
                    return original;
                }
                // Check Vertical
                else if ( original.x > high && original.x < 180 )
                {
                    original.x = high;
                }
                else if ( !( original.x > 360 - low ) )
                {
                    original.x = 360 - low;
                }

            }
            catch { }
            return original;
        }

        #endregion
    }
}
