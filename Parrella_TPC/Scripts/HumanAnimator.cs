using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Human_Controller
{

    public class HumanAnimator : HumanMotor
    {
        #region Variables                

        public const float  walkSpeed       = 0.5f;
        public const float  runningSpeed    = 1f;
        public const float  sprintSpeed     = 1.5f;


        internal    bool    IKGunHands      = false;

        #endregion


        #region Properties

        internal InverseKinematicsHuman ik { private get; set; }

        public GameObject       GunHandL        { set; internal get;                 }
        public GameObject       GunHandR        { set; internal get;                 }
        
        public Vector3          ObjectToLookAt  { set { if (ik) { ik.ObjectToLookAt     = value; }    } }
        public bool             LookAtObject    { set { if (ik) { ik.LookAtObject       = value; }  } }
        public bool             AllowsEyeMove   { set { if (ik) { ik.AllowEyesMovement  = value; }  } }  

        #endregion
      

        #region External Calls
  
        /// <summary>
        ///     Animator functions to be called by the late updates, 
        ///     after IK pass and move
        /// </summary>
        protected virtual void UpdateAnimatorLate()
        {
        }

        protected virtual void UpdateAnimator()
        {
            if (animator == null || !animator.enabled) return;

            
            DetermineRootMotion();

            animator.SetBool(       TPCAnimatorParameters.IsStrafing,       isStrafing      );
            animator.SetBool(       TPCAnimatorParameters.IsCrouched,       isCrouched      );
            animator.SetBool(       TPCAnimatorParameters.IsSprinting,      isSprinting     );
            animator.SetBool(       TPCAnimatorParameters.IsADS,            isADS           );
            animator.SetBool(       TPCAnimatorParameters.IsGrounded,       isGrounded      );
            animator.SetBool(       TPCAnimatorParameters.DoDive,           isDiving        );
            animator.SetFloat(      TPCAnimatorParameters.GroundDistance,   groundDistance  );
            animator.SetBool(       TPCAnimatorParameters.isFiring,         isFiring        );
            animator.SetInteger(    TPCAnimatorParameters.isDead,           deathType       );
        
            if ( isVaulting )
            {
                animator.SetBool(TPCAnimatorParameters.doVault,      isVaulting     );
            }

            if (isStrafing)
            {
                animator.SetFloat(TPCAnimatorParameters.InputHorizontal, stopMove ? 0 : horizontalSpeed, strafeSpeed.animationSmooth, Time.deltaTime);
                animator.SetFloat(TPCAnimatorParameters.InputVertical, stopMove ? 0 : verticalSpeed, strafeSpeed.animationSmooth, Time.deltaTime);
            }
            else
            {
                animator.SetFloat(TPCAnimatorParameters.InputVertical, stopMove ? 0 : verticalSpeed, freeSpeed.animationSmooth, Time.deltaTime);
            }

            if (turnOnSpot)
            {
                animator.SetFloat(TPCAnimatorParameters.Rotation, rotationAmount); ;
            }

            if (isDiving) { inputMagnitude = .5f;   }   // Need to make sure we come out of dive alright

            animator.SetFloat(TPCAnimatorParameters.InputMagnitude, stopMove ? 0f : inputMagnitude, isStrafing ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth, Time.deltaTime);

            updateWeapons();
        }

        /// <summary>
        ///     Update the weapon animations information
        /// </summary>
        void updateWeapons()
        {
            animator.SetInteger(TPCAnimatorParameters.WeaponSelected, (int)holdingWeaponType);
            animator.SetBool(TPCAnimatorParameters.isReloading, isReloading);
        }


        /// <summary>
        ///     Get the boolean value currently set on the animator 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        ///     The boolean the requested animator value is set to.
        /// </returns>
        internal bool AnimatorGetBoolValue(int id)
        {
            return animator.GetBool(id);            
        }

        /// <summary>
        ///     Get the float value currently set on the animator
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        ///     A float value of the requested animator variable
        /// </returns>
        internal float AnimatorGetFloatValue(int id)
        {
            return animator.GetFloat(id);
        }
        #endregion


        #region Extra Functions

        
        /// <summary>
        ///     Determine what sort of motion we are going to be using
        /// </summary>
        private void DetermineRootMotion()
        {
            animator.speed      = generalAnimationSpeed;

            if( canToggleRootMotion )
            {
                if ( isDiving )
                {
                    animator.speed          = divingAnimationSpeed;
                }
            }
        }

        public virtual void SetAnimatorMoveSpeed(tpcMovementSpeed speed)
        {        
            Vector3 relativeInput   = transform.InverseTransformDirection(moveDirection);
            verticalSpeed           = relativeInput.z;
            horizontalSpeed         = relativeInput.x;
        
            var newInput = new Vector2(verticalSpeed, horizontalSpeed);

            if (speed.walkByDefault)
                inputMagnitude = Mathf.Clamp(newInput.magnitude, 0, isSprinting ? runningSpeed : walkSpeed);
            else
            {   
                inputMagnitude = Mathf.Clamp(isSprinting ? newInput.magnitude + 0.5f : newInput.magnitude, 0, isSprinting ? sprintSpeed : runningSpeed);
            }    
        }

        #endregion

    }


    /// <summary>
    /// The general hashes for the animitor keys
    /// </summary>
    public static partial class TPCAnimatorParameters
    {
        public static int InputHorizontal   = Animator.StringToHash( "InputHorizontal"  );
        public static int InputVertical     = Animator.StringToHash( "InputVertical"    );
        public static int InputMagnitude    = Animator.StringToHash( "InputMagnitude"   );
        public static int IsGrounded        = Animator.StringToHash( "IsGrounded"       );
        public static int IsStrafing        = Animator.StringToHash( "IsStrafing"       );
        public static int IsSprinting       = Animator.StringToHash( "IsSprinting"      );
        public static int GroundDistance    = Animator.StringToHash( "GroundDistance"   );
        public static int DoDive            = Animator.StringToHash( "doDive"           );
        public static int WeaponSelected    = Animator.StringToHash( "Selected_Weapon"  );
        public static int IsCrouched        = Animator.StringToHash( "isCrouched"       );
        public static int IsADS             = Animator.StringToHash( "isAds"            );
        public static int doVault           = Animator.StringToHash( "doVault"          );
        public static int Rotation          = Animator.StringToHash( "Rotation"         );
        public static int isFiring          = Animator.StringToHash( "isFiring"         );
        public static int isReloading       = Animator.StringToHash( "isReloading"      );
        public static int isDead            = Animator.StringToHash( "isDead"           );
    }
}

    