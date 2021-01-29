using UnityEngine;
using AsteriaGeneral;

namespace Human_Controller
{
    [RequireComponent(typeof(HumanController))]
    public class ThirdPersonInput : MonoBehaviour
    {
        #region Variables       

        [Header("Controller Input")]
        public string horizontalInput   = "Horizontal";
        public string verticallInput    = "Vertical";
        public KeyCode jumpInput        = KeyCode.Space;
        public KeyCode strafeInput      = KeyCode.Tab;
        public KeyCode sprintInput      = KeyCode.LeftShift;
        
        [Header("Camera Input")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        [Header("Chest Directions Offset")]
        [SerializeField] Vector3 chestOffsets = new Vector3(18, 18, 0);

        [Header("Sprinting")]
        [SerializeField]
        bool limitSprint        = true;

        [SerializeField]
        [Tooltip("The usage meter for sprinting")]
        UsageBar sprintUsage;

        [HideInInspector] public HumanController tpc;
        [HideInInspector] public ThirdPersonCamera tpCamera;

        #endregion

        #region Private declarations
        

        #endregion

        #region Properties

        internal bool       LimitSprint     { get { return limitSprint;     } }
        internal UsageBar   SprintUsage     { get { return sprintUsage;     } }

        #endregion

        protected virtual void Start()
        {
            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {       
            tpc.updateMotor();               // updates the ThirdPersonMotor methods
            tpc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            tpc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            tpc.updateAnimator();           // updates the Animator Parameters
        }

        private void LateUpdate()
        {
            tpc.updateAnimatorLate();
        }



        public virtual void OnAnimatorMove()
        {
            tpc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            tpc = GetComponent<HumanController>();

            if (tpc != null)
            {
                tpc.Init();
                tpc.AIEntity = false;
            }
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<ThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();                    
                }
            }
        }

        /// <summary>
        ///     Handle all of the inputs
        /// </summary>
        protected virtual void InputHandle()
        {
            CameraInput();
            isDiving();
            MoveInput();
            checkCrouch();
            SprintInput();
            JumpInput();
            UpdateCameraLocation();
        }


        private void UpdateCameraLocation()
        {
            CameraOffSets offsets = new CameraOffSets();
            if (tpc.isCrouched && tpc.isADS  )
            {
                offsets = tpCamera.ADSCrouchingOffset;
            }
            else if ( tpc.isSprinting )
            {
                offsets = tpCamera.SprintingOffset;
            }
            else if ( tpc.isCrouched )
            {
                offsets = tpCamera.crouchOffset;
            }
            else if ( tpc.isADS )
            { 
                offsets = tpCamera.ADSStandingOffSet;            
            }
            else
            {
                offsets = tpCamera.DefaultOffset;
            }

            //  If we changed our cameras - update
            if (offsets != tpCamera.currentCamera)
            {
                tpCamera.SetCurrentCamera( current: offsets );
            }
        }

        /// <summary>
        ///     Determine if we are lunging forward
        /// </summary>
        /// <returns></returns>
        private bool isDiving()
        {
            bool diving = false;

            if ( tpc.isJumping ) { return diving; } // Do not allow within jump

            if ( Input.GetButtonDown("Lunge" ) )
            {
                diving      = true;
                tpc.isADS   = false;
            }
            else
            {
                diving      = tpc.AnimatorGetBoolValue(TPCAnimatorParameters.DoDive);
            }

            tpc.isDiving = diving;
            return tpc.isDiving;
        }

        /// <summary>
        ///     Crouch or stand up
        /// </summary>
        /// <returns></returns>
        private bool checkCrouch()
        {

            // Get the opposite
            if (Input.GetButtonDown("Crouch") && tpc.CanCrouch )
            {
                if ( tpc.isCrouched && !tpc.CheckStandUpConditions() )
                {
                    tpc.isCrouched = true;
                }
                else
                {
                    tpc.isCrouched = !(tpc.isCrouched);
                }
            }
        
            return tpc.isCrouched;
        }

        public virtual void MoveInput()
        {
            tpc.input.x = Input.GetAxis(horizontalInput);
            tpc.input.z = Input.GetAxis(verticallInput);
        }

        protected virtual void CameraInput()
        {

            if (tpCamera == null)
                return;

            tpc.rotateTarget            = tpCamera.gameObject.transform;

            tpc.UpdateMoveDirection(tpCamera.gameObject.transform);

            tpc.cameraPointingDirection = tpCamera.gameObject.transform.rotation;

            tpc.ChestOffsets = chestOffsets;


            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);

            // Switch Shoulders
            if (  Input.GetButtonDown("CameraSwitch") )
            {
                tpCamera.SwitchShoulder();
            }
        }


        protected virtual void StrafeInput()
        {
            if (Input.GetKeyDown(strafeInput))
                tpc.Strafe();
        }

        protected virtual void SprintInput()
        {

            SprintUsage.Calculate(tpc.isSprinting);

            if ( limitSprint && !sprintUsage.AnyLeft )
            {
                tpc.Sprint(false);
                return;
            }

            if ( Input.GetKeyDown( sprintInput ) )
            {
                tpc.Sprint(true);
            }
            else if ( Input.GetKeyUp( sprintInput ) )
            {
                tpc.Sprint(false);
            }
        }
       

        /// <summary>
        /// Input to trigger the Jump or vault
        /// </summary>
        protected virtual void JumpInput()
        {
            if ( Input.GetKeyDown(jumpInput) )
            {
                if ( tpc.vaultAble )
                {
                    // make it vault
                }
                else if ( tpc.JumpConditions() )
                {
                    tpc.Jump();
                }
            }
        }

        #endregion
    }
}