using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Human_Controller;


namespace AI_Asteria
{

    [RequireComponent(typeof(HumanController))]
    public class AIRandomStateMachine : AIStateMachine
    {
        #region Protected

        /// <summary> The human controller </summary>
        protected HumanController movementController;

       
        #endregion


        #region Private

        float _speed = 0.0f;


        #endregion


        #region Properties

        /// <summary>
        ///     The speed the character is moving. Can be locked 
        ///     between 0-1.5
        /// </summary>
        internal float Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = Mathf.Clamp(value: value, min: 0, max: 1.5f);
            }
        }
        
        /// <summary>
        ///     Determines if we can use obstace awareness
        /// </summary>
        /// <returns></returns>
        internal bool obstacleAware
        {
            get
            {
                return !(obstacleAwareness == null);
            }
        }

        /// <summary>
        ///     The obstacle awareness on the character
        /// </summary>
        internal ObjectChecking obstacleAwareness
        {
            get { return movementController.Obstacles?.currentChecks ?? null; }
        }

        /// <summary>
        ///     Determine if the player is blocked
        ///      anywhere based on the 
        ///     object awareness controller
        /// </summary>
        internal bool playerBlocked
        {
            get
            {
                if (movementController?.Obstacles == null) { return false; }

                // Check the items at each height and angle
                foreach (var item in obstacleAwareness.AllHeightItems)
                {
                    // front items only
                    if (item.obstacles.front.collision)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        ///     All the items that the obstacle is touching
        /// </summary>
        internal List<GameObject> ObjectsHitting
        {
            get
            {
                List<GameObject> gos = new List<GameObject>();
                if (movementController?.Obstacles == null) { return gos; }

                // Check the items at each height and angle
                foreach (var item in obstacleAwareness.AllHeightItems)
                {
                    if (item.obstacles.front.collision)
                    {
                        gos.Add(item.obstacles.front.firstObjHit);
                    }                    
                }
                return gos;
            }
        }

        /// <summary>
        ///     Set the object that the human
        ///     controller should look at
        /// </summary>
        internal GameObject LookAtThis
        {
            //TODO change to transform
            set;
            get;
        }
        internal Vector3 LookAtOffset;

        #endregion


        #region Overrides

        protected override void Awake()
        {
            base.Awake();
        }


        // Start is called before the first frame update
        protected override void Start()
        {
            SetControllers();
            base.Start();
        }

        public override void isBeingTargeted()
        {
            base.isBeingTargeted();
        }

        #endregion

        /// <summary>
        ///     Get the human controller and do what we need with it
        /// </summary>
        void SetControllers()
        {
            movementController = GetComponent<HumanController>();
            if (movementController == null)
            {
                Debug.LogError($"There is no human controller for game object {gameObject.name} in the AIRandomState.Start().");
            }
            else
            {
                movementController.Init();
            }

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            activateController();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        /// <summary>
        ///     Update the controller values
        /// </summary>
        void updateController()
        {
            if (canMove && movementController != null)
            {
                movementController.inputMagnitude       = Speed;
                movementController.moveDirection        = Target.position - transform.position;
                movementController.input                = new Vector3(Speed, 0, Speed);
                movementController.checkObjectAwareness = checkObstacles;
                updateLookingAt();
            }
        }

        /// <summary>
        ///     Set the parameters to look at something
        /// </summary>
        void updateLookingAt() 
        {
            movementController.LookAtObject     = LookAtThis ? true : false;
            movementController.ObjectToLookAt   = LookAtThis?.transform.position + LookAtOffset ?? new Vector3();
        }

        /// <summary>
        ///     Update the motor
        /// </summary>
        void activateController()
        {
            if (canMove && movementController != null)
            {
                updateController();
                movementController.updateAnimator();
                movementController.ControlRotationType();
                movementController.UpdateMoveDirection();
                movementController.updateMotor();                
            } 
        }


        /// <summary>
        ///     Clear the target that we are currently set on
        /// </summary>
        internal override void ClearTarget()
        {
            base.ClearTarget();
        }
  

    }
}
