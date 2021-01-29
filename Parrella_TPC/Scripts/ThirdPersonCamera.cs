using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using AsteriaGeneral;
using static Human_Controller.ExtensionMethods;

namespace Human_Controller
{ 

    public class ThirdPersonCamera : MonoBehaviour
    {
        #region Inspector Properties    

        public Transform target;

        [Tooltip("What to do with the cursor")]
        public CursorLockMode cursorLockMode    = CursorLockMode.Confined;
    
        [Tooltip("What layer will be culled")]
        public LayerMask cullingLayer           = 1 << 0;
    
        [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
        public bool lockCamera;

        [Tooltip("The camera to assign to this. Will default to the main camera if there is none assigned")]
        Camera AssignCamera;

        [Header("Camera Offset Positions")]
        [Tooltip("Allow for camera to switch shoulders")]
        [SerializeField]    bool    CanSwitchOffset    = true;
    
        [Tooltip("Set the Offsets automatically")]
        [SerializeField]    bool    DefaultOffSets     = false;

        [Tooltip("Flip the camera to the other shoulder")]
        [SerializeField]    bool     SwitchOffSet       = false;

        [Tooltip("The location of the offsets by character movement")]
        public  CameraOffSets ADSStandingOffSet, ADSCrouchingOffset, DefaultOffset, SprintingOffset, crouchOffset;
        internal CameraOffSets currentCamera { get; private set; }

        [Header("Aiming Raycasts")]
        [SerializeField] bool       drawRayCasts    = true;
        [SerializeField] Color      rayColor        = Color.red;
        [SerializeField] LayerMask  AimingLayers    = 1 << 0;


        /*
         *  ADS
         *      dist    =   .7
         *      hor     =   .6
         *      height  =   1.6
         *      Smooth  =   10
         *  Sprint
         *      dist    =   .7
         *      hor     =   .5
         *      height  =   1.6
         *      Smooth  =   2
         *  Crouch
         *      dist    =   1.2
         *      hor     =   0.0
         *      height  =   1.2
         *      smooth  =   10.0
         */



        [Header("Mouse and Movement Information")]
        [Tooltip("The amount of yeild before stopping the camera movement")]
        [SerializeField]
        float CameraToggleYield  = 0.02f;
        [SerializeField]
        float xMouseSensitivity  = 3f;
        [SerializeField]
        float yMouseSensitivity  = 3f;
        [SerializeField]
        float yMinLimit          = -40f;
        [SerializeField]
        float yMaxLimit          = 80f;

        #endregion

        #region hide properties    


        [HideInInspector]
        public int indexList, indexLookPoint;
        [HideInInspector]
        public string currentStateName;
        [HideInInspector]
        public Transform currentTarget;
        [HideInInspector]
        public Vector2 movementSpeed;

        private Transform   targetLookAt;
        private Vector3     currentTargetPos;
        private Vector3     current_cPos;
        private Vector3     desired_cPos;
        private Camera      _camera;
        private float       mouseY = 0f;
        private float       mouseX = 0f;
        private float       currentHeight;
        private float       cullingDistance;
        private float       checkHeightRadius = 0.4f;
        private float       clipPlaneMargin = 0f;
        private float       forward = -1f;
        private float       xMinLimit = -360f;
        private float       xMaxLimit = 360f;
        private float       cullingHeight = 0.2f;
        private float       cullingMinDist = 0.1f;

        // Hold the current distances
        private float       _currentDistance;
        private float       _currentHeight;
        private float       _currentOffset;

        // Account for recoil
        float xRecoil = 0;
        float yRecoil = 0;

        #region Properties

        /// <summary>
        ///     The position of the camera
        /// </summary>
       public  Transform CameraPosition
        {
            get
            {
                return _camera.transform;
            }
        }

        #endregion

        #region Internal Declarations

        //internal GameObject adsMount;

        #endregion

        #endregion

        void Start()
        {
            Init();
        }

        /// <summary>
        ///     Set where the camera should currently be
        /// </summary>
        /// <param name="current"></param>
        public void SetCurrentCamera( CameraOffSets current )
        {
            currentCamera = current;
        }

        public void Init()
        {
            if (target == null)
                return;

            getCamera();
            currentTarget           = target;
            currentTargetPos        = new Vector3(currentTarget.position.x, currentTarget.position.y + DefaultOffset.offSetPlayerPivotY, currentTarget.position.z);

            targetLookAt            = new GameObject("targetLookAt").transform;
            targetLookAt.position   = currentTarget.position;
            targetLookAt.hideFlags  = HideFlags.HideInHierarchy;
            targetLookAt.rotation   = currentTarget.rotation;

            mouseY = currentTarget.eulerAngles.x;
            mouseX = currentTarget.eulerAngles.y;

            SetCurrentCamera(DefaultOffset);
            _currentDistance    = DefaultOffset.Distance;
            _currentOffset      = DefaultOffset.HorizontalOffset;
            _currentHeight      = DefaultOffset.HeightOffset;
            SetCameraOffsetDefaults();
        }

  

        /// <summary>
        ///     Determine what  camera we are using
        ///     based on cascading rule set
        /// </summary>
        void getCamera()
        {
            _camera = GetComponent<Camera>();
            if (_camera != null) { return; }

            if (AssignCamera) { _camera = AssignCamera; }
            else { _camera = Camera.main; }
        }

        private void SetCameraOffsetDefaults()
        {
            if ( DefaultOffSets )
            {
                // ADS Offsets
                ADSStandingOffSet.Distance                  = 1.0f;
                ADSStandingOffSet.HorizontalOffset          = 0.6f;
                ADSStandingOffSet.HeightOffset              = 1.6f;
                ADSStandingOffSet.SmoothFollow              = 5f;

                // Sprinting Offsets
                SprintingOffset.Distance            = 1.2f;
                SprintingOffset.HorizontalOffset    = 0.5f;
                SprintingOffset.HeightOffset        = 1.6f;
                SprintingOffset.SmoothFollow        = 2f;

                // Crouching offsets
                crouchOffset.Distance               = 1.2f;
                crouchOffset.HorizontalOffset       = 0.0f;
                crouchOffset.HeightOffset           = 1.2f;
                crouchOffset.SmoothFollow           = 10f;

            }
        }

        void FixedUpdate()
        {
            if (target == null || targetLookAt == null) return;

            Cursor.lockState = cursorLockMode;

            CameraMovement();

        }


        /// <summary>
        /// Set the target for the camera
        /// </summary>
        /// <param name="New cursorObject"></param>
        public void SetTarget(Transform newTarget)
        {
            currentTarget = newTarget ? newTarget : target;
        }

        public void SetMainTarget(Transform newTarget)
        {
            target          = newTarget;
            currentTarget   = newTarget;
            mouseY          = currentTarget.rotation.eulerAngles.x;
            mouseX          = currentTarget.rotation.eulerAngles.y;
            Init();
        }

        /// <summary>    
        /// Convert a point in the screen in a Ray for the world
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        public Ray ScreenPointToRay(Vector3 Point)
        {
            return this.GetComponent<Camera>().ScreenPointToRay(Point);
        }



        /// <summary>
        /// Camera Rotation behaviour
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RotateCamera(float x, float y)
        {
            // free rotation 
            mouseX += x * xMouseSensitivity;
            mouseY -= y * yMouseSensitivity;

            movementSpeed.x = x;
            movementSpeed.y = -y;
            if ( !lockCamera )
            {
                mouseY = Human_Controller.ExtensionMethods.ClampAngle(mouseY, yMinLimit, yMaxLimit);
                mouseX = Human_Controller.ExtensionMethods.ClampAngle(mouseX, xMinLimit, xMaxLimit);        
            }
            else
            {
                mouseY = currentTarget.root.localEulerAngles.x;
                mouseX = currentTarget.root.localEulerAngles.y;
            }
        }

        /// <summary>
        ///     Update the recoil offsets 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateRecoilOffset(float x, float y)
        {
            xRecoil = x;
            yRecoil = y;
        }

        /// <summary>
        ///     Add in the recoil when updating the location of the 
        ///     camera looking
        /// </summary>
        void adjustRecoil()
        {
            mouseX += xRecoil;
            mouseY += -yRecoil;
        }

        Vector3 update;
        /// <summary>
        ///     Get the position we want the camera to rotate around
        /// </summary>
        /// <returns>
        ///     A vector 3 where we want to spin around
        /// </returns>
        Vector3 getTargetPOS()
        {
            var pos         = new Vector3(currentTarget.position.x, currentTarget.position.y + currentCamera.offSetPlayerPivotY, currentTarget.position.z);

            var newSpot     = pos + (currentCamera.offSetPlayerPivotX * (currentTarget.right * (SwitchOffSet ? -1 : 1)) );

            var lerpSpeed   = currentCamera.OffsetLerp;

            pos             = update = Vector3.Slerp(update, newSpot, lerpSpeed);

            return pos;
        }

        /// <summary>
        /// Camera behaviour
        /// </summary>    
        void CameraMovement()
        {
            if (currentTarget == null)
                return;

            adjustRecoil();

            // Determine distance
            float udistance     = _currentDistance;
            float urightOffset  = _currentOffset;
            float uheight       = _currentHeight;

            lerpPositions( uheight: ref uheight, udistance: ref udistance, urightOffset: ref urightOffset);

            cullingDistance = Mathf.Lerp( cullingDistance, udistance, Time.deltaTime);
            var camDir          = (forward * targetLookAt.forward) + (urightOffset * targetLookAt.right);

            camDir              = camDir.normalized;

            var targetPos       = getTargetPOS();
            currentTargetPos    = targetPos;
            desired_cPos        = targetPos + new Vector3(0, uheight, 0);
            current_cPos        = currentTargetPos + new Vector3(0, currentHeight, 0);
            RaycastHit hitInfo;

            ClipPlanePoints planePoints = _camera.NearClipPlanePoints_Asteria( current_cPos + ( camDir * ( udistance ) ) , clipPlaneMargin);
            ClipPlanePoints oldPoints   = _camera.NearClipPlanePoints_Asteria( desired_cPos + ( camDir * udistance), clipPlaneMargin);

            //Check if Height is not blocked 
            if ( Physics.SphereCast( targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer ) )
            {
                var t = hitInfo.distance - 0.2f;
                t -= uheight;
                t /= (cullingHeight - uheight);
                cullingHeight = Mathf.Lerp(uheight, cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            }

            //Check if desired target position is not blocked       
            if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, udistance + 0.2f, cullingLayer, Color.blue))
            {
                udistance = hitInfo.distance - 0.2f;
                if ( udistance < DefaultOffset.Distance )
                {
                    var t   = hitInfo.distance;
                    t       -= cullingMinDist;
                    t       /= cullingMinDist;
                    currentHeight   = Mathf.Lerp(cullingHeight, uheight, Mathf.Clamp(t, 0.0f, 1.0f));
                    current_cPos    = currentTargetPos + new Vector3(0, currentHeight, 0);
                }
            }
            else
            {
                currentHeight = uheight;
            }

            //Check if target position with culling height applied is not blocked
            if (CullingRayCast(current_cPos, planePoints, out hitInfo, udistance, cullingLayer, Color.cyan)) udistance = Mathf.Clamp(cullingDistance, 0.0f, DefaultOffset.Distance );

            Transform trans         = targetLookAt;

            var lookPoint           = current_cPos + trans.forward * 2f;
            lookPoint               += (trans.right * Vector3.Dot(camDir * (udistance), trans.right));
            trans.position          = current_cPos;

            Quaternion newRot       = Quaternion.Euler(mouseY, mouseX, 0);
            trans.rotation          = Quaternion.Slerp(trans.rotation, newRot, currentCamera.SmoothFollow * Time.deltaTime);
            transform.position      = current_cPos + (camDir * (udistance));
            var rotation            = Quaternion.LookRotation((lookPoint) - transform.position);

            transform.rotation      = rotation;
            movementSpeed           = Vector2.zero;

        }

        /// <summary>
        ///     Slowly move our object between camera locations
        /// </summary>
        /// <param name="uheight">      the height to lerp between              </param>
        /// <param name="udistance">    the distance from the object to lerp to </param>
        /// <param name="urightOffset"> the right horizontal offset             </param>
        void lerpPositions(ref float uheight, ref float udistance, ref float urightOffset)
        {
             // Get the distance smoothly
            if (!_currentDistance.checkIfBetween(min: (currentCamera.Distance - CameraToggleYield), max: (currentCamera.Distance + CameraToggleYield)))
            {
                udistance = Mathf.Lerp(a: _currentDistance, b: currentCamera.Distance, t: (currentCamera.SmoothFollow * Time.deltaTime));
                _currentDistance = udistance;
            }
            else { udistance = currentCamera.Distance; }

            // Get the Horizontal offset smoothly
            if (!(Mathf.Abs(_currentOffset).checkIfBetween(min: (Mathf.Abs(currentCamera.HorizontalOffset) - CameraToggleYield),
                                max: (Mathf.Abs(currentCamera.HorizontalOffset) + CameraToggleYield))))
            {
                urightOffset = Mathf.Lerp(a: urightOffset, b: SwitchOffSet ? -currentCamera.HorizontalOffset : currentCamera.HorizontalOffset, t: (currentCamera.SmoothFollow * Time.deltaTime));
                _currentOffset = urightOffset;
            }
            else { urightOffset = SwitchOffSet ? -currentCamera.HorizontalOffset : currentCamera.HorizontalOffset; }

            // Get the height smoothly
            if (!uheight.checkIfBetween(min: (currentCamera.HeightOffset - CameraToggleYield), max: (currentCamera.HeightOffset + CameraToggleYield)))
            {
                uheight = Mathf.Lerp(a: _currentHeight, b: currentCamera.HeightOffset, t: (currentCamera.SmoothFollow * Time.deltaTime));
                _currentHeight = uheight;
            }
            else { uheight = currentCamera.HeightOffset; }
        }

        /// <summary>
        /// Custom Raycast using NearClipPlanesPoints
        /// </summary>
        /// <param name="_to"></param>
        /// <param name="from"></param>
        /// <param name="hitInfo"></param>
        /// <param name="distance"></param>
        /// <param name="cullingLayer"></param>
        /// <returns></returns>
        bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
        {
            bool value = false;

            if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
            }

            return hitInfo.collider && value;
        }

        /// <summary>
        ///     Swtitch the camera shoulder
        /// </summary>
        public void SwitchShoulder()
        {
            if( CanSwitchOffset )
            {
                SwitchOffSet = !SwitchOffSet;
            }
        }

        /// <summary>
        ///     Get the nearest position that we hit. The layers 
        ///     culled are the aiming layers in the camera editor
        /// </summary>
        /// <param name="distance"> The distance to check   </param>
        /// <returns>
        ///     The position of the nearest hit object straight 
        ///     from the camera
        /// </returns>
        public RaycastController getNearestStraightLineTarget(float distance)
        {
            RaycastController rayController = new RaycastController(start: transform.position, direction: transform.forward, distance: distance, layerMasks: AimingLayers );

            if (drawRayCasts) { rayController.DrawRay( c: rayColor ); }

            return rayController;
        }

        /// <summary>
        ///     Get the vector the desired distance directly in front of 
        ///     the camera looking
        /// </summary>
        /// <param name="distance"> The distance to grab the vector3 offset </param>
        /// <returns>
        ///     A vector 3 a distancec directly offset in front of the camera
        /// </returns>
        public Vector3 StraightLineAhead(float distance)
        {
            return transform.position + (transform.forward * distance); //_start + (_direction * d);
        }

    }

}