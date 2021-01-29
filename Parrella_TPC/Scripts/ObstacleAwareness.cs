using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Human_Controller
{
    #region Additional Classes

    [System.Serializable]
    public class ObjectChecking
    {        
        public enum CheckType { walking, running, crouching };
        public CheckType type;

        [System.Serializable]
        public class heightItem
        {
            public bool check               = true;
            public ObstaclesInWay obstacles = new ObstaclesInWay();

            [Range(0.1f, 10f)]
            [Tooltip("The distance to check for impact")]
            public float distance       = 1f;

            [Range(0.1f, 10f)]
            [Tooltip("The height at which to check for this item")]
            public float height         = 1f;

            [Range(0, 180)]
            [Tooltip("The angle to set out about our left and right checks")]
            public float offSetAngle = 45.0f;

            [Tooltip("Determines whether or not to calculate the angle of attack")]
            public bool calculateAOA = false;

            public heightItem(float _distance, float _height, float _angle)
            {
                distance    = _distance;
                height      = _height;
                offSetAngle = _angle;
            }
        }

        [SerializeField]
        [Tooltip("Determine how far forward to start searching for obstacles")]
        [Range(0, 2.0f)]
        internal float vertOffset = 0.1f;

        [Header("Ground Checks")]
        [Tooltip("Determine if we need to check for ground impact")]
        public bool checkGround     = true;        

        [Tooltip("Determines if the ground is about to drop out in front of us")]
        [Range(0.1f, 2f)]
        public float groundDrop     = 0.2f;

        [Tooltip("Determines how far out to check for the ground drop")]
        [Range(0.1f, 2f)]
        public float dropBuffer     = 1.0f;
        
        [Tooltip("The angle which to check for ground drops")]
        public float groundOffSetAngle = 45.0f;

    
        [Header("Height Checks")]
        [SerializeField]
        public heightItem High      = new heightItem(1.0f, 1.8f,  45.0f);

        [SerializeField]
        public heightItem MedHigh   = new heightItem(1.0f, 1.0f, 45.0f);
        
        [SerializeField]
        public heightItem MedLow    = new heightItem(1.0f, 0.8f, 45.0f);

        [SerializeField]
        public heightItem Low       = new heightItem(1.0f, 0.2f, 45.0f);
        
        
        #region Properties

        /// <summary>
        ///     A list of each of the height items
        /// </summary>
        public List<heightItem> AllHeightItems
        {
            get
            {
                return new List<heightItem>() { High, MedHigh, MedLow, Low };
            }
        }

        
        #endregion

        #region Obstacles

        public ObstaclesInWay crouchObstacles   { get; internal set; } = new ObstaclesInWay();
        public ObstaclesInWay groundDropObst    { get; internal set; } = new ObstaclesInWay();
        public ObstaclesInWay ledgeObstacle     { get; internal set; } = new ObstaclesInWay();

        #endregion
    }

    /// <summary>
    ///     The information of the obstacle that can be in our way
    /// </summary>
    public class ObstaclesInWay
    {
        /// <summary>
        ///     Holds hit information
        /// </summary>
        public struct item
        {
            /// <summary> Whether or not we are hitting something                   </summary>
            public  bool         collision       { get; private set; }
            /// <summary> The first object that we have come in contact with        </summary>
            public  GameObject   firstObjHit     { get; private set; }
            public  float        distance        { get; private set; }
            /// <summary> The angle at which the object we are hitting is leaning   </summary>
            public  float        angleOfAttack   { get; private set; }
            /// <summary> The total, unorganized list of hits that we have          </summary>
            public  RaycastHit[] allHits         { get { return allHits; } set { hits = value; setFirstObject(); } }

            private RaycastHit[] hits;

            /// <summary>   An ordered list by distance of all the hits encountered         </summary>
            /// <<remarks>  This is calculated at time of call and not cached in class      </remarks>
            public RaycastHit[] OrderedHits
            {
                get
                {
                    var arr = hits;
                    int n = arr.Length;
                    for (int i = 0; i < n - 1; i++)
                    {
                        for (int j = 0; j < n - i - 1; j++)
                            if (arr[j].distance > arr[j + 1].distance)
                            {
                                // swap temp and arr[i] 
                                var temp    = arr[j];
                                arr[j]      = arr[j + 1];
                                arr[j + 1]  = temp;
                            }
                    }
                    return arr;
                }
            }

            /// <summary>
            ///     Using the first object hit, calculate the angle of attack
            /// </summary>
            /// <param name="calcFrom"> The position to calculate the angle of attack from </param>
            public void setAngleOfAttack( Vector3 calcFrom ) 
            { 
                angleOfAttack = firstObjHit == null ? 0.0f : Vector3.Angle(firstObjHit.transform.position, calcFrom );                 
            }

            /// <summary>
            ///     Set the first game object from the raycasts.
            ///     This will clear the game object if we have
            ///     no hits
            /// </summary>
            void setFirstObject()
            {
                if (hits == null) { return; }
                float d         = float.PositiveInfinity;
                RaycastHit hit  = new RaycastHit();
                foreach( RaycastHit h in hits )
                {
                    if (d > h.distance)
                    {
                        hit = h;
                        d = h.distance;
                    }
                }
                //  we have something
                if ( hits.Length > 0 ) 
                {   
                    firstObjHit = hit.transform.gameObject;
                    distance    = hit.distance;
                    collision   = true;
                }
                else // clear the item - we have nothing
                {
                    collision   = false;
                    distance    = 0.0f;
                    firstObjHit = null;
                }

            }
        }
        
        public item front   { get; internal set; }      = new item();
        public item left    { get; internal set; }      = new item();
        public item right   { get; internal set; }      = new item();
        public item top     { get; internal set; }      = new item();
        public item bottom  { get; internal set; }      = new item();

        /// <summary>  All the items that we want in this  </summary>
        internal List<item> AllItems    { get { return new List<item>() { front, left, right, top, bottom   }; } }
        internal List<item> FrontItems  { get { return new List<item>() { front, left, right                }; } }

        /// <summary>
        ///     If any of our checks are hitting a solid game object
        ///     at the front, left, back or right
        /// </summary>
        internal bool isHittingSomething
        {
            get
            {
                bool hitting = false;
                foreach (item item in FrontItems)
                {
                    if (item.collision) { return true; }
                }
                return hitting;
            }
        }

        /// <summary>
        ///     Get all the objects that this awareness is touching
        /// </summary>
        internal List<GameObject> ObjectsTouching
        {
            get
            {
                List<GameObject> gos = new List<GameObject>();
                foreach (item item in FrontItems)
                {
                    gos.Add(item.firstObjHit);
                }
                return gos;
            }
        }
    }

    #endregion
    
    
    /// <summary>
    ///     A class to be attached to a human controller that allows 
    ///     for object awareness in its surrounding environment. This class
    ///     can be added multiple times to improve performance if necessary
    /// </summary>
    public class ObstacleAwareness : MonoBehaviour
    {

        #region Editor

        [Header("Object Awareness")]
        [Tooltip("Check for nearby colliders object")]
        public bool checkObjectAwareness        = true;

        [Tooltip("Determine if we want this done on the late update or the regular update")]
        [SerializeField]
        bool runOnFixedUpdate                    = true;

        [SerializeField]
        bool checkLedges                        = true;
        
        [SerializeField]
        [Tooltip("Can check crouch")]
        bool checkCrouch                        = true;

        [SerializeField]
        [Tooltip("The lowest ledge we can climb")]
        [Range(0.1f, 10f)]
        float minLedge                          = 2.3f;

        [SerializeField]
        [Tooltip("The highest ledge we can climb")]
        [Range(0.1f, 10f)]
        float maxLedge                          = 2.7f;

        [SerializeField]
        [Tooltip("The distance the ledge would be away")]
        [Range(0.1f, 10f)]
        float ledgeDistance                     = 1.0f;


        [Tooltip("What Layers we want to include as possible obstacles")]
        [SerializeField] 
        LayerMask includeObstacles              = ~0;
                
        [SerializeField]
        [Tooltip("The check distances when we are crouching")]
        ObjectChecking _crouchChecks            = new ObjectChecking();
        
        [SerializeField]
        [Tooltip("The check distances when we are walking")]
        ObjectChecking _walkChecks              = new ObjectChecking();
       
        [SerializeField]
        [Tooltip("The check distances when we are running")]
        ObjectChecking _runChecks               = new ObjectChecking();


        #endregion

        #region Properties

        public bool             DrawRays        { get; set; } = true;
        ObjectChecking   crouchChecks    { get { return _crouchChecks; } }
        ObjectChecking   walkChecks      { get { return _walkChecks; } }
        ObjectChecking   runChecks       { get { return _runChecks; } }

        /// <summary> Determines whether we can stand up or not </summary>
        public bool             canStandUp      { get; internal set; }


        #endregion


        #region Private 

        float colliderHeight = 0.0f;

        #endregion


        /// <summary>
        ///     The check we are currently working off of
        /// </summary>
        public ObjectChecking currentChecks
        {
            get;
            internal set;
        } = new ObjectChecking();


        // Start is called before the first frame update
        void Start()
        {
            var get = GetComponent<CapsuleCollider>();

            SetCurrentCheckType(ObjectChecking.CheckType.walking);

            if ( get ) { colliderHeight = get.height; }
            else
            {
                colliderHeight = 0.0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!runOnFixedUpdate)   { runCode(); }
        }

        private void FixedUpdate()
        {
            if (runOnFixedUpdate)    { runCode(); }            
        }

        /// <summary>
        ///     The code that  will check for obstacles
        /// </summary>
        void runCode()
        {
            if (currentChecks == null) { return; }

            checkGroundDrop();
            CheckLedge();
            CheckNearObjects();
            checkCrouchConditions();
        }

        /// <summary>
        ///     Set the type of checking that we would like to do
        /// </summary>
        /// <param name="type"></param>
        public void SetCurrentCheckType(ObjectChecking.CheckType type)
        {
            if (type == ObjectChecking.CheckType.crouching)
            {
                currentChecks       = crouchChecks;
                currentChecks.type  = ObjectChecking.CheckType.crouching;
            }
            else if (type == ObjectChecking.CheckType.running)
            {
                currentChecks       = runChecks;
                currentChecks.type  = ObjectChecking.CheckType.running;
            }
            else if (type == ObjectChecking.CheckType.walking)
            {
                currentChecks = walkChecks;
                currentChecks.type = ObjectChecking.CheckType.walking;
            }
        }

        /// <summary>
        ///     check if the ground in front of use is about to drop off
        /// </summary>
        void checkGroundDrop()
        {
            if ( currentChecks.checkGround )
            {
                currentChecks.groundDropObst.right  = checkEachGroundDrop( currentChecks.groundOffSetAngle    );              
                currentChecks.groundDropObst.left   = checkEachGroundDrop( -currentChecks.groundOffSetAngle   );
                currentChecks.groundDropObst.front  = checkEachGroundDrop( 0.0f );              
            }
        }

        /// <summary>
        ///     Check the ground drop in front of use
        /// </summary>
        /// <param name="angle">    The angle in front to check </param>
        /// <returns>
        ///     The ground item at that angle off 
        ///     of our characters transform
        /// </returns>
        ObstaclesInWay.item checkEachGroundDrop(float angle)
        {
            var rayDir          = Quaternion.Euler(0, angle, 0) * transform.forward;
            var startVector     = (transform.position + (rayDir * currentChecks.dropBuffer ));

            Ray outray  = new Ray(transform.position, transform.forward);
            RaycastHit[] straightHits = Physics.RaycastAll( ray: outray, maxDistance: currentChecks.dropBuffer, layerMask: includeObstacles );
            
            // Draw me like one of our your French Girls
            drawRaycasts( transform.position, startVector, Color.green );
           
            // Only go down if we are uninterrupted outwards - helps with slopes
            if ( straightHits.Length == 0 )
            {
                Ray downRay         = new Ray(startVector, -transform.up);
                RaycastHit[] hits   = Physics.RaycastAll( ray: downRay, maxDistance: currentChecks.groundDrop, layerMask: includeObstacles );
                
                drawRaycasts(startVector, startVector + (-transform.up) * currentChecks.groundDrop, Color.green);
                return createItem(hits);
            }

            return createItem( new RaycastHit[0] );
        }

        /// <summary>
        ///     Check our conditions for standing up
        /// </summary>
        void checkCrouchConditions()
        {
            if (checkCrouch)
            {
                // Check Stand Up
                var StartRay        = transform.position;
                Ray crouchRay       = new Ray(StartRay, transform.up);
                RaycastHit[] hits   = Physics.RaycastAll(ray: crouchRay, maxDistance: colliderHeight);

                // Draw the raycast line I want
                drawRaycasts(StartRay, StartRay + transform.up * colliderHeight, Color.magenta);

                canStandUp = !UtilityClass.CheckSolidRayCastHit(hits: hits, includePlayer: false);

                currentChecks.crouchObstacles.top   = createItem(hits);
            }
            else
            {
                canStandUp = true;
            }
        }

        /// <summary>
        ///     Create an item from the hit gameobjeect
        /// </summary>
        /// <param name="go">   The gameobject hit, if nothing was hit, the value should be null    </param>
        /// <returns>
        ///     An item created from the gameobject passd
        /// </returns>
        ObstaclesInWay.item createItem(RaycastHit[] hits)
        {
            ObstaclesInWay.item temp    = new ObstaclesInWay.item();
            temp.allHits                = hits;
            return temp;
        }

        /// <summary>
        ///     Check directly in front of the character to see 
        ///     if there is a ledge that can be grabbed
        /// </summary>
        void CheckLedge()
        {
            if (checkLedges)
            {
                Color ledgeColor    = Color.cyan;
                
                var startDraw       = new Vector3( transform.position.x, transform.position.y + colliderHeight,  transform.position.z );
                var startHeight     = new Vector3( transform.position.x, transform.position.y + maxLedge,        transform.position.z );

                // draw line forward                                
                RaycastHit[] hits   = Physics.RaycastAll( ray: new Ray(startHeight, transform.forward), maxDistance: ledgeDistance, layerMask: includeObstacles);
                               
                // If we do not hit anything
                if ( hits.Length == 0 )
                {
                    var startDown       = (ledgeDistance * transform.forward) + startHeight;
                    RaycastHit[] dhits  = Physics.RaycastAll(ray: new Ray(startDown, -transform.up), maxDistance: maxLedge-minLedge, layerMask: includeObstacles);
                    currentChecks.ledgeObstacle.front = createItem(dhits);

                    // TODO determine angle of attack on ledge
                    currentChecks.ledgeObstacle.front.setAngleOfAttack( new Vector3() );

                    // draw line down
                    drawRaycasts(startDown,     startDown   + (-transform.up * (maxLedge - minLedge)),  ledgeColor );
                }
                else
                {
                    // Clear out any items
                    currentChecks.ledgeObstacle.front = createItem(new RaycastHit[0]); ; 
                }

                // Draw Lines on editor
                drawRaycasts(startDraw,     startDraw   + transform.up * (maxLedge-colliderHeight), ledgeColor );
                drawRaycasts(startHeight,   startHeight + (transform.forward * ledgeDistance),         ledgeColor );

            }
        }


        /// <summary>
        ///     Checks if we are near an object for vaulting / jumping.
        ///     This will assign items to each of the awareness areas that we want
        /// </summary>
        void CheckNearObjects()
        {
            if ( checkObjectAwareness )
            {
                List<ObjectChecking.heightItem> items = new List<ObjectChecking.heightItem>() { currentChecks.High, currentChecks.MedHigh, currentChecks.MedLow, currentChecks.Low };

                foreach( ObjectChecking.heightItem item in items)
                {
                    if( item.check)
                    {
                        item.obstacles = checkObstacles(item);
                    }
                }
            }
        }

        /// <summary>
        ///     Check if obstacles are in the way here
        ///     for the required directions
        /// </summary>
        /// <param name="item">The item at a specific height to check</param>
        /// <returns>
        ///     An updated obstacles class with information regarding its hit registrations
        /// </returns>
        ObstaclesInWay checkObstacles(ObjectChecking.heightItem item)
        {
                ObstaclesInWay obst = new ObstaclesInWay();
                obst.front  = checkHit(  distance: item.distance,    angle: 0.0f,                height: item.height, aoa: item.calculateAOA );
                obst.left   = checkHit(  distance: item.distance,    angle: -item.offSetAngle,   height: item.height, aoa: item.calculateAOA );
                obst.right  = checkHit(  distance: item.distance,    angle: item.offSetAngle,    height: item.height, aoa: item.calculateAOA );
                return obst;         
        }

        /// <summary>
        ///     Will create a raycast in the direction and distance to check 
        ///     for any hits
        /// </summary>
        /// <param name="distance"> The distance we are checking for a hit          </param>
        /// <param name="angle">    The angle to offset the left and right checks   </param>
        /// <param name="height">   The height at which to start the hit checking   </param>
        /// <returns>
        ///     A new item filled with information
        /// </returns>
        ObstaclesInWay.item checkHit(float distance, float angle, float height, bool aoa )
        {            
            var StartRay        = new Vector3(transform.position.x, transform.position.y + height, transform.position.z) + (transform.forward * currentChecks.vertOffset);
            var rayDir          = Quaternion.Euler(0, angle, 0) * transform.forward;
            Ray ray             = new Ray(StartRay, rayDir);

            RaycastHit[] hits   = Physics.RaycastAll(ray: ray, maxDistance: distance, layerMask: includeObstacles);
            
            if ( aoa )
            {
                // TODO calculate the angle of attack
            }
            
            // Draw if we want to
            drawRaycasts( StartRay, StartRay + rayDir * distance, Color.blue );

            return createItem(hits);
            
        }

        /// <summary>
        ///     Draw the raycasts in the scene
        /// </summary>
        void drawRaycasts(Vector3 start, Vector3 end, Color color)
        {
            if ( DrawRays )
            {
                Debug.DrawLine( start, end, color );
            }
        }

    }


}