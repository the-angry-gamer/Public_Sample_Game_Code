using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms;



namespace Human_Controller
{

    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class HumanMotor : MonoBehaviour
    {

        #region Inspector Variables

        [Header("Combat")]
        [SerializeField] public AsteriaWeapons.WeaponType holdingWeaponType = AsteriaWeapons.WeaponType.None;

        [SerializeField] public bool canCombat = true;

        [Header("Movement")]

        [Tooltip("Determine if this will be AI movement")]
        public bool AIEntity = true;

        [Tooltip("Determines if different movements can toggle Root Motion")]
        public bool canToggleRootMotion = true;

        [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        public bool useRootMotion = false;

        [Tooltip("Allows the character to turn on the spot if the angle of attack is > 150 degrees")]
        public bool turnOnSpot = false;

        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        public bool rotateByWorld = false;

        [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
        public bool useContinuousSprint = true;

        [Tooltip("Check this to sprint always in free movement")]
        public bool sprintOnlyFree = true;

        [Tooltip("Determines if we can roll")]
        public bool CanRoll = true;

        [Header("Crouch and Roll")]
        [Tooltip("Determines if we can crouch")]
        public bool CanCrouch = true;

        [SerializeField]
        ColliderDimensions crouchCollider;

        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree,
        }
        [Header("Movement Speeds")]
        public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;

        public tpcMovementSpeed freeSpeed, strafeSpeed, crouchSpeed, adsSpeed;

        public float generalAnimationSpeed = 1.0f, divingAnimationSpeed = 1.0f;

        

        [Header("Vaulting")]
        [Tooltip("Determines if we can vault")]
        public bool canVault = true;
        public bool isVaulting = false;

        [Header("Object Awareness")]
        [Tooltip("Check for nearby colliders object")]
        public bool checkObjectAwareness = true;


        [Header("Airborne")]
        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        public bool jumpWithRigidbodyForce = false;
        [Tooltip("Rotate or not while airborne")]
        public bool jumpAndRotate = true;
        [Tooltip("How much time the character will be jumping")]
        public float jumpTimer = 0.3f;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        public float jumpHeight = 4f;

        [Tooltip("Speed that the character will move while airborne")]
        public float airSpeed = 5f;
        [Tooltip("Smoothness of the direction while airborne")]
        public float airSmooth = 6f;
        [Tooltip("Apply extra gravity when the character is not grounded")]
        public float extraGravity = -10f;
        [HideInInspector]
        public float limitFallVelocity = -15f;

        [Header("Ground")]
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        public float groundMinDistance = 0.45f;
        public float groundMaxDistance = 0.5f;
        [Tooltip("Max angle to walk")]
        [Range(30, 80)] public float slopeLimit = 75f;



        #endregion


        #region Components

        internal Animator           animator;
        internal Rigidbody          _rigidbody;                                             // access the Rigidbody component
        internal PhysicMaterial     frictionPhysics, maxFrictionPhysics, slippyPhysics;     // create PhysicMaterial for the Rigidbody
        internal CapsuleCollider    _capsuleCollider;                                       // access CapsuleCollider information

        #endregion


        #region Internal Variables

        #region Movement Variables

        #region Weapons

        public bool     isReloading         { get; set; }
        public bool     isFiring            { get; set; }

        #endregion


        internal bool   isJumping           { get; set; }

        internal bool   isADS               { get; set; }

        /// <summary> Determine whether we can go into ADS based on motor params    </summary>
        internal bool   canADS              { get { return !isJumping; }  }

        internal bool   isWeaponEquipped    { get { return holdingWeaponType == AsteriaWeapons.WeaponType.None ? false : true; } }

        protected int   deathType           { get; set; } = 0;

        protected bool  isDead              { get { return deathType > 0 ? true : false; } }

        internal bool   isGrounded          { get; set; }

        internal bool   isSprinting         { get; set; }

        public bool     stopMove            { get; protected set; }

        internal bool   isCrouched          { get; set; }

        internal bool   canStandUp          { get; set; }

        internal bool   isDiving            { get; set; }


        /// <summary>
        ///     Sets the locomotion type (strafe / no strafe) if 
        ///     we are not limiting the types available
        /// </summary>
        internal bool isStrafing
        {
            get
            {
                if (locomotionType == LocomotionType.OnlyStrafe) { return true; }
                else if (locomotionType == LocomotionType.OnlyFree) { return false; }

                return _isStrafing;
            }
            set
            {
                if (locomotionType == LocomotionType.OnlyStrafe)
                {
                    _isStrafing = true;
                }
                else if (locomotionType == LocomotionType.OnlyFree)
                {
                    _isStrafing = false;
                }
                else
                {
                    _isStrafing = value;
                }
            }
        }


        /// <summary> Determine if we want to keep us straight and prevent rotation </summary>
        protected bool KeepStraight
        {
            get
            {
                if (isDiving || isVaulting)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        ///     Determines if we can vault. Uses the 
        ///     obstacle awaereness med high and med low checks 
        /// </summary>
        internal bool vaultAble
        {
            get
            {
                if (!canVault) { return false; }

                // TODO test
                // low is obstructed, high is not
                if (Obstacles)
                {
                    var checks = Obstacles.currentChecks;
                    if (checks.MedLow.obstacles.front.collision && !checks.MedHigh.obstacles.front.collision)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        #endregion

        internal float          inputMagnitude;                         // sets the inputMagnitude to update the animations in the animator controller
        internal float          verticalSpeed;                          // set the verticalSpeed based on the verticalInput
        internal float          horizontalSpeed;                        // set the horizontalSpeed based on the horizontalInput       
        internal float          moveSpeed;                              // set the current moveSpeed for the MoveCharacter method
        internal float          verticalVelocity;                       // set the vertical velocity of the rigidbody
        internal float          colliderRadius, colliderHeight;         // storage capsule collider extra information        
        internal float          heightReached;                          // max height that character reached in air;
        internal float          jumpCounter;                            // used to count the routine to reset the jump
        internal float          groundDistance;                         // used to know the distance from the ground
        internal RaycastHit     groundHit;                              // raycast to hit the ground 
        internal bool           lockMovement = false;                   // lock the movement of the controller (not the animation)
        internal bool           lockRotation = false;                   // lock the rotation of the controller (not the animation)        
        internal bool           _isStrafing;                            // internally used to set the strafe movement                
        internal Transform      rotateTarget;                           // used as a generic reference for the camera.transform
        internal Vector3        input;                                  // generate raw input for the controller
        internal Vector3        colliderCenter;                         // storage the center of the capsule collider info                
        internal Vector3        inputSmooth;                            // generate smooth input based on the inputSmooth value       
        internal Vector3        moveDirection;                          // used to know the direction you're moving 
        internal float          rotationAmount;

        /// <summary> The direction we want a head to turn to</summary>
        internal Quaternion         cameraPointingDirection;
        internal ObstacleAwareness  Obstacles;
        internal Vector3            ChestOffsets;

        #endregion


        public void Init()
        {
            animator = GetComponent<Animator>();
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

            // slides the character through walls and edges
            frictionPhysics = new PhysicMaterial();
            frictionPhysics.name = "frictionPhysics";
            frictionPhysics.staticFriction = .25f;
            frictionPhysics.dynamicFriction = .25f;
            frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

            // prevents the collider from slipping on ramps
            maxFrictionPhysics = new PhysicMaterial();
            maxFrictionPhysics.name = "maxFrictionPhysics";
            maxFrictionPhysics.staticFriction = 1f;
            maxFrictionPhysics.dynamicFriction = 1f;
            maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

            // air physics 
            slippyPhysics = new PhysicMaterial();
            slippyPhysics.name = "slippyPhysics";
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;
            slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

            // rigidbody info
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;

            // capsule collider info
            _capsuleCollider = GetComponent<CapsuleCollider>();

            getColliderInformation();

            if (!Obstacles) { Debug.LogWarning("There is no obstacle awareness added to this script. Obstacles will not be registed"); }

            isGrounded = true;
        }

        /// <summary>
        ///     Get all of our collider information
        /// </summary>
        void getColliderInformation()
        {
            // save your collider preferences 
            colliderCenter = GetComponent<CapsuleCollider>().center;
            colliderRadius = GetComponent<CapsuleCollider>().radius;
            colliderHeight = GetComponent<CapsuleCollider>().height;
            Obstacles = GetComponent<ObstacleAwareness>();
        }


        protected virtual void UpdateMotor()
        {
            if (isDead) { return; }
            CheckNearObject();
            CheckForceCrouch();
            CheckGround();
            CheckSlopeLimit();
            ControlJumpBehaviour();
            AirControl();
        }


        #region Locomotion

        #region Object Awareness

        /// <summary>
        ///     Updates our modifier to determine if we are near obstacles
        /// </summary>
        public virtual void CheckNearObject()
        {
            if (checkObjectAwareness && Obstacles)
            {
                if (isCrouched)         { Obstacles.SetCurrentCheckType( ObjectChecking.CheckType.crouching );      }
                else if (isSprinting)   { Obstacles.SetCurrentCheckType( ObjectChecking.CheckType.running   );      }
                else                    { Obstacles.SetCurrentCheckType( ObjectChecking.CheckType.walking   );      }
            }
        }

        #endregion


        #region Crouching
        /// <summary>
        ///     See if we have to force a crouch
        /// </summary>
        private void CheckForceCrouch()
        {
            if (!isGrounded) { isCrouched = false; }
            if (CanCrouch)
            {
                if (!CheckStandUpConditions())
                {
                    isCrouched = true;
                }
                else
                {
                    if (isCrouched && AIEntity)
                    {
                        isCrouched = false;
                    }
                }

                ForceCrouchSizes();
            }
        }

        /// <summary>
        ///     Force the capsule collider crouch sizes to make sure we straight
        ///     to stand up or stay down
        /// </summary>
        void ForceCrouchSizes()
        {
            if (isCrouched)
            {
                _capsuleCollider.height = crouchCollider.height;
                _capsuleCollider.center = new Vector3(colliderCenter.x + crouchCollider.centerOffset.x, 
                        colliderCenter.y + crouchCollider.centerOffset.y, colliderCenter.z + crouchCollider.centerOffset.z);
                _capsuleCollider.radius = crouchCollider.radius;
            }
            else
            {
                _capsuleCollider.height = colliderHeight;
                _capsuleCollider.center = colliderCenter;
                _capsuleCollider.radius = colliderRadius;
            }
        }


        /// <summary>
        ///     Raycast to determine if we can stand up
        /// </summary>
        /// <returns>
        ///     A bool of true if we can stand up
        /// </returns>
        public virtual bool CheckStandUpConditions()
        {
            if (isJumping) { return true; } // if we are jumping we can stand

            if (Obstacles)
            {
                canStandUp = Obstacles.canStandUp;
                return canStandUp;
            }
            else
            {
                return true;
            }
        }


        #endregion


        #region Moving and Rotation


        public virtual void SetControllerMoveSpeed(tpcMovementSpeed speed)
        {

            if (speed.walkByDefault)
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.deltaTime);
            else
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.deltaTime);
        }


        public virtual void MoveCharacter(Vector3 _direction)
        {
            // calculate input smooth
            inputSmooth = Vector3.Lerp(inputSmooth, input, GetMovementSpeed().movementSmooth * Time.deltaTime);

            if (!isGrounded || isJumping) return;

            _direction.y = 0;
            _direction.x = Mathf.Clamp(_direction.x, -1f, 1f);
            _direction.z = Mathf.Clamp(_direction.z, -1f, 1f);

            // limit the input
            if (_direction.magnitude > 1f)
                _direction.Normalize();


            Vector3 targetPosition = (useRootMotion ? animator.rootPosition : _rigidbody.position) + _direction * (stopMove ? 0 : moveSpeed) * Time.deltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            bool useVerticalVelocity = true;
            if (useVerticalVelocity) targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = targetVelocity;
        }

        public virtual void CheckSlopeLimit()
        {
            if (input.sqrMagnitude < 0.1) return;

            RaycastHit hitinfo;
            var hitAngle = 0f;

            if (Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), transform.position + moveDirection.normalized * (_capsuleCollider.radius + 0.2f), out hitinfo, groundLayer))
            {
                hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                var targetPoint = hitinfo.point + moveDirection.normalized * _capsuleCollider.radius;
                if ((hitAngle > slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), targetPoint, out hitinfo, groundLayer))
                {
                    hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                    if (hitAngle > slopeLimit && hitAngle < 85f)
                    {
                        stopMove = true;
                        return;
                    }
                }
            }
            stopMove = false;
        }

        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public virtual void RotateToDirection(Vector3 direction)
        {
            rotationAmount = 0.0f;  // TODO calculate (from transform.forward)
            RotateToDirection(direction, GetMovementSpeed().rotationSpeed);
        }

        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (!jumpAndRotate && !isGrounded) return;
            if (isDiving) { return; }

            direction.y = 0f;
            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed * Time.deltaTime, .1f);
            Quaternion _newRotation = Quaternion.LookRotation(desiredForward);
            transform.rotation = _newRotation;
        }


        public tpcMovementSpeed GetMovementSpeed()
        {
            tpcMovementSpeed movement = freeSpeed;

            if (isCrouched && !isADS)
            {
                movement = crouchSpeed;
            }
            else if (isStrafing)
            {
                movement = strafeSpeed;
            }

            return movement;
        }
        #endregion

        #endregion

        #region Jump Methods

        /// <summary>
        ///     Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        internal virtual bool JumpConditions()
        {
            // Do not jump if we are rolling
            if (isDiving) { return false; }
            return isGrounded && GroundAngle() < slopeLimit && !isJumping && !stopMove;
        }

        /// <summary>
        ///     Set the death parameters for this object
        /// </summary>
        /// <param name="death"></param>
        public void KillMe(int death)
        {
            deathType = death;
        }

        protected virtual void ControlJumpBehaviour()
        {
            if (!isJumping) return;

            jumpCounter -= Time.deltaTime;
            if (jumpCounter <= 0)
            {
                jumpCounter = 0;
                isJumping = false;
            }
            // apply extra force to the jump height   
            var vel = _rigidbody.velocity;
            vel.y = jumpHeight;
            _rigidbody.velocity = vel;
        }

        public virtual void AirControl()
        {
            if ((isGrounded && !isJumping)) return;
            if (transform.position.y > heightReached) heightReached = transform.position.y;
            inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.deltaTime);

            if (jumpWithRigidbodyForce && !isGrounded)
            {
                _rigidbody.AddForce(moveDirection * airSpeed * Time.deltaTime, ForceMode.VelocityChange);
                return;
            }

            moveDirection.y = 0;
            moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
            moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);

            Vector3 targetPosition = _rigidbody.position + (moveDirection * airSpeed) * Time.deltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, airSmooth * Time.deltaTime);
        }

        protected virtual bool jumpFwdCondition
        {
            get
            {
                Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
                Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
            }
        }

        #endregion


        #region Ground Check                

        protected virtual void CheckGround()
        {
            CheckGroundDistance();
            ControlMaterialPhysics();

            if (groundDistance <= groundMinDistance)
            {
                isGrounded = true;
                if (!isJumping && groundDistance > 0.05f)
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);

                heightReached = transform.position.y;
            }
            else
            {
                if (groundDistance >= groundMaxDistance)
                {
                    // set IsGrounded to false 
                    isGrounded = false;
                    // check vertical velocity
                    verticalVelocity = _rigidbody.velocity.y;
                    // apply extra gravity when falling
                    if (!isJumping)
                    {
                        _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                    }
                }
                else if (!isJumping)
                {
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
                }
            }
        }

        protected virtual void ControlMaterialPhysics()
        {
            // change the physics material to very slip when not grounded
            _capsuleCollider.material = (isGrounded && GroundAngle() <= slopeLimit + 1) ? frictionPhysics : slippyPhysics;

            if (isGrounded && input == Vector3.zero)
                _capsuleCollider.material = maxFrictionPhysics;
            else if (isGrounded && input != Vector3.zero)
                _capsuleCollider.material = frictionPhysics;
            else
                _capsuleCollider.material = slippyPhysics;
        }

        protected virtual void CheckGroundDistance()
        {
            if (_capsuleCollider != null)
            {
                // radius of the SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                var dist = 10f;
                // ray for RayCast
                Ray ray2 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
                // raycast for check the ground distance
                if (Physics.Raycast(ray2, out groundHit, (colliderHeight / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
                    dist = transform.position.y - groundHit.point.y;
                // sphere cast around the base of the capsule to check the ground distance
                if (dist >= groundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                    Ray ray = new Ray(pos, -Vector3.up);
                    if (Physics.SphereCast(ray, radius, out groundHit, _capsuleCollider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
                    {
                        Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                        float newDist = transform.position.y - groundHit.point.y;
                        if (dist > newDist) dist = newDist;
                    }
                }
                groundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }

        public virtual float GroundAngleFromDirection()
        {
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, groundHit.normal) - 90;
            return movementAngle;
        }

        #endregion

        [System.Serializable]
        public class tpcMovementSpeed
        {
            [Range(1f, 20f)]
            public float movementSmooth = 6f;
            [Range(0f, 1f)]
            public float animationSmooth = 0.2f;
            [Tooltip("Rotation speed of the character")]
            public float rotationSpeed = 16f;
            [Tooltip("Character will limit the movement to walk instead of running")]
            public bool walkByDefault = false;
            [Tooltip("Rotate with the Camera forward when standing idle")]
            public bool rotateWithCamera = false;
            [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
            public float walkSpeed = 2f;
            [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
            public float runningSpeed = 4f;
            [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
            public float sprintSpeed = 6f;
        }

        [System.Serializable]
        class ColliderDimensions
        {
            [SerializeField]
            internal float height = 1.4f;

            [SerializeField]
            internal float radius = 0.5f;

            [SerializeField]
            internal Vector3 centerOffset = new Vector3(0, -.25f, .2f);
        }

    }
}