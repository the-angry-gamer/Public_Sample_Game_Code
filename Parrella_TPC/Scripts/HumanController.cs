using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Human_Controller
{

    public class HumanController : HumanAnimator
    {
    
        /// <summary>
        ///     Set the reload status of the character
        ///     also return what that status is
        /// </summary>
        /// <returns>
        ///     the current reload status of the animator
        /// </returns>
        public bool DetermineReloadStatus()
        {
            isReloading = AnimatorGetBoolValue(TPCAnimatorParameters.isReloading);
            return isReloading;
        }

        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;
        
            // Keeps us diving straight
            if(KeepStraight)  { inputSmooth = Vector3.zero; }

            if (inputSmooth == Vector3.zero)
            {            
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
                MoveCharacter(moveDirection);
        }
    
        public virtual void ControlLocomotionType()
        {
            if (lockMovement || isDead ) return;

            // Set speed of ADS movement
            if ( isADS )
            {
                SetControllerMoveSpeed(adsSpeed);
                SetAnimatorMoveSpeed(adsSpeed);
            }
            else if (isCrouched && !isADS)  // Set Speed of crouch movement
            {
                SetControllerMoveSpeed(crouchSpeed);
                SetAnimatorMoveSpeed(crouchSpeed);
            }
            else if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
            {
                SetControllerMoveSpeed(freeSpeed);
                SetAnimatorMoveSpeed(freeSpeed);
            }
            else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
            {
                isStrafing = true;
                SetControllerMoveSpeed(strafeSpeed);
                SetAnimatorMoveSpeed(strafeSpeed);
            }

            if (!useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlRotationType()
        {
            if ( lockRotation || isDead ) return;

            bool validInput = input != Vector3.zero || isADS || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);
          
            if (validInput)
            {
                // calculate input smooth            
                inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

                Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection(dir);
            }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if ( input.magnitude <= 0.01 )
            {            
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);            
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        /// <summary>
        ///     If we are crouching check if we can stand up and runs
        /// </summary>
        /// <returns></returns>
        private bool CrouchToSprint()
        {
            if (isCrouched)
            {
                if ( CheckStandUpConditions() )
                {
                    isCrouched = false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }


        public virtual void Sprint(bool value)
        {

            var sprintConditions = (input.sqrMagnitude > 0.1f && isGrounded && !isADS &&
                !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

        
            if ( value && sprintConditions && CrouchToSprint() )
            {

                if (input.sqrMagnitude > 0.1f)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                    }
                    else if (!isSprinting)
                    {
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting)
            {
                isSprinting = false;
            }
        }

        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        public virtual void Jump()
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping   = true;

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
            
        }

        /// <summary>
        ///     Update the underlying animator
        /// </summary>
        public void updateAnimator()
        {
            UpdateAnimator();
        }

        /// <summary>
        ///     Update the animator late calls
        /// </summary>
        public void updateAnimatorLate()
        {
            UpdateAnimatorLate();
        }


        public void updateMotor()
        {
            UpdateMotor();
        }
    }
}