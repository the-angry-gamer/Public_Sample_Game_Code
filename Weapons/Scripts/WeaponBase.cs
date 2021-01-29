using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AsteriaGeneral;

namespace AsteriaWeapons
{

    public abstract class WeaponBase : MonoBehaviour
    {
        #region Editor

        [SerializeField]
        [Tooltip("Show or hide the console logs")]
        bool showLogs = true;

        [Header("Gun Info")]

        [SerializeField]
        [Tooltip("The display name of the gun")]
        protected string gunName = "";

        [SerializeField]
        [Tooltip("The description of this gun")]
        protected string gunDescription = "";

        [SerializeField]
        [Tooltip("What type of weapon this is")]
        protected WeaponType    weaponType;

        [Header("Fire Selector")]
        [SerializeField]
        [Tooltip("Determine what type of selector we want")]
        protected FireType      firingSelector = FireType.Single;

        [SerializeField]
        [Tooltip("Determine the rate of fire per second")]
        [Range(0.1f, 50.0f)]
        protected float         rateOfFire      = 1;

        [SerializeField]
        [Tooltip("Determine the rate of burst fire")]
        [Range(4.0f, 20.0f)]
        protected float         burstRate       = 1;

        [SerializeField]
        [Tooltip("Add additional levels of firing that this weapon is capabale of ")]
        internal List<FiringSelectorInfo>    AddFiringTypes;

        [Header("Magazine Info")]
        [SerializeField]
        [ Tooltip("The amount of damage to do") ]
        protected Vector2           damageRange;

        [SerializeField]
        [Tooltip("The maximum ammo that this gun can hold")]
        protected int maxAmmo;

        [SerializeField]
        [Tooltip("The maximum ammo each clip in this gun can hold")]
        protected int maxClip;

        [SerializeField]
        [Tooltip("Determines if the gun consumes ammo or not")]
        protected bool infiniteAmmo     = false;

        [Header("Bullet Info")]
        [SerializeField]
        [Tooltip("How fast to fire the bullet from this gun")]
        [Range(0,200)]
        protected float         bulletSpeed;

        [SerializeField]
        [Tooltip("Determines if we use a raycast when shooting the bullet")]
        protected bool          useRay = true;
        
        [SerializeField]
        [Tooltip("How far the bullet  will go before destruction")]
        [Range(0,1000)]
        protected float         bulletRange;

        [Header("Recoil and Spread")]
        [SerializeField]
        [Tooltip("The recoil of the gun horizontally")]
        protected float recoilX;

        [SerializeField]
        [Tooltip("The maximum amount of recoil it can reach vertically")]
        protected float recoilXMax;

        [SerializeField]
        [Tooltip("The recoil of the gun vertically")]
        protected float recoilY;

        [SerializeField]
        [Tooltip("How far the recoil can reach horizontally")]
        protected float recoilYMax;

        [SerializeField]
        [Tooltip("How fast the recoil resets")]
        [Range(0, 1)]
        protected float recoilReset = 0.5f;

        [SerializeField]
        [Tooltip("How fast to reset  the spread")]
        [Range(0, 1)]
        protected float spreadReset;

        [Header("Gun Objects")]
        [SerializeField]
        bool useRightHandIK = true;

        [SerializeField]
        GameObject              gunObject;
        [SerializeField]
        Vector3 aimingoffests = new Vector3(0.32f, 0.08f, 1.232f);

        [SerializeField]
        [Tooltip("The anchor point for our idle gun")]
        protected GameObject    idleAnchor;

        [SerializeField]
        [Tooltip("The anchor point for our aimed gun")]
        protected GameObject    aimingAnchor;

        [SerializeField]
        [Tooltip("The gameobject that will identify as the muzzle")]
        protected GameObject    muzzleArea;

        [SerializeField]
        [Tooltip("The gameobject that will identify as the reciever")]
        protected GameObject    recieverArea;

        [SerializeField]
        [Tooltip("The gameobject that will identify as the reciever")]
        protected GameObject    holsterAnchor;

        [SerializeField]
        [Tooltip("Where to mount the off hand")]
        protected GameObject    handMountL;
        [SerializeField]
        [Tooltip("Where to mount the Trigger hand")]
        protected GameObject    handMountR;


        [SerializeField]
        [Tooltip("The image to show of the weapon on the HUD")]
        Sprite                  gunSpriteHUD;

        [SerializeField]
        [Tooltip("The gameobject that will act as the bullet shoot from the gun")]
        protected BulletBase    bullet;

        [SerializeField]
        [Tooltip("The Canvas that will be drawn for aiming, this can be left blank for NPC")]
        protected AimCanvasBase aimCanvas;

        [Header("Sounds")]
        [SerializeField]
        SoundsController        shootingSound;

        [SerializeField]
        SoundsController        reloadSound;

        [SerializeField]
        SoundsController        emptySound;

        #endregion


        #region Protected Declerations

        protected int           currentAmmo = 10;
        protected int           currentClip = 10;        

        #endregion


        #region Properties

               /// <summary>
        ///     How many shots total we have fired through this 
        ///     weapons manager
        /// </summary>
        public long ShotsFired
        {
            get;
            private set;
        }

        /// <summary>
        ///     Determines if the gun consumes ammo or not
        /// </summary>
        internal bool InfiniteAmmo
        {
            set
            {
                infiniteAmmo = value;
            }
        }

        /// <summary>
        ///     Grab the layers that we can hit with the weapon from the manager
        /// </summary>
        protected LayerMask LayersToHit
        {
            get
            {
                if (manager)
                {
                    return manager.LayersToHit;
                }
                LayerMask l = 1 << 0; ;
                return l;
            }
        }

        public GameObject HandMountL    { get { return handMountL; } }
        public GameObject HandMountR    { get { return useRightHandIK ? handMountR : null; } }

        public string GunName           { get { return gunName;         } }
        public string GunDesc           { get{ return gunDescription;   } }
        public GameObject Owner 
        { 
            get 
            { 
                if (manager) { return manager.gameObject; }
                return null;
            }  
        }

        /// <summary>
        ///     The sprite to show on the hud
        /// </summary>
        public Sprite GunSpriteHUD
        {
            get
            {
                return gunSpriteHUD;
            }
        }

        /// <summary>
        ///     The game object to hold the bullets in
        /// </summary>
        protected GameObject BulletParent
        {
            get
            {
                if ( !bulletParent )
                {
                    DevelopBulletParent();
                }
                return bulletParent;
            }
        }

        /// <summary>
        ///     Returns the unity time of the last fired bullet
        /// </summary>
        /// <value>The last fire time.</value>
        public float LastFireTime
        {
            get;
            private set;
        }

        /// <summary>
        ///     The current amount of ammo we have
        /// </summary>
        /// <value>The current ammo.</value>
        public int CurrentAmmo
        {
            get
            {
                return currentAmmo;
            }
        }

        /// <summary>
        ///     How much a single clip can hold
        /// </summary>
        public int MaxClip
        {
            get
            {
                return maxClip;
            }
        }

        /// <summary>
        ///     Return how much is in the current clip
        /// </summary>
        /// <value>The current clip.</value>
        public int CurrentClip
        {
            get
            {
                return currentClip;
            }
        }


        /// <summary>
        ///     How much ammo the gun can hold
        /// </summary>
        public int AmmoLimit
        {
            get
            {
                return maxAmmo;
            }
        }

        
        /// <summary>
        ///     What this type of weapon this is
        /// </summary>
        public WeaponType WeaponTypeOf
        {
            get
            {
                return weaponType;
            }
        }

        /// <summary>
        ///     Indicates if we are firing 
        /// </summary>
        /// <value><c>true</c> if is firing; otherwise, <c>false</c>.</value>
        internal bool IsFiring
        {
            get;
            set;
        }

        /// <summary>
        ///     Indicates if we are aiming the weapon
        /// </summary>
        /// <value><c>true</c> if is aiming; otherwise, <c>false</c>.</value>
        public bool IsAiming
        {
            get;
            internal set;
        }

        /// <summary>
        ///     Determines if the weapon is holstered
        /// </summary>
        public bool isHolstered
        {
            get;
            internal set;
        }

        /// <summary>
        ///     Indicates if we are reloading
        /// </summary>
        /// <value><c>true</c> if is reloading; otherwise, <c>false</c>.</value>
        public bool IsReloading
        {
            get;
            internal set;
        }
        
        /// <summary>
        ///     How fast the gun shoots per minute
        /// </summary>
        /// <value>The fire rate time.</value>
        public float FireRateIntervals
        {
            get
            {
                if (rateOfFire == 0) { return 0; }
                return ( (1000 / rateOfFire) / 1000 ); 
            }
        }

        /// <summary>
        ///     How fast the gun shoots per minute
        /// </summary>
        /// <value>The fire rate time.</value>
        public float BurstFireIntervals
        {
            get
            {
                if (burstRate == 0) { return 0; }
                return ( (1000 / burstRate) / 1000 ); 
            }
        }

        /// <summary>
        ///     If we can fire another bullet
        /// </summary>
        /// <value> if the firing conditions have been met and we are actively trying to fire   </value>
        public virtual bool CanFireWeapon
        {
            get
            {
                return FiringConditionsMet && IsFiring;
            }
        }


        /// <summary>
        ///     If our conditions to fire have been met. Takes
        ///     into account single / multi fire, as well as 
        ///     ammo and our last time fired
        /// </summary>
        public bool FiringConditionsMet
        {
            get
            {
                return !IsReloading &&  DetermineLastFire() && checkFireRate()
                    && checkAmmo() && checkIfWeCanShootObject() && IsAiming;
            }
        }

        /// <summary>
        ///     Determines if we can reload at the moment
        /// </summary>
        /// <value> compares the clip amount and the if we are holsterd </value>
        public bool CanReload
        {
            get
            {
                return maxClip != currentClip && currentAmmo != 0 && !IsReloading && !isHolstered;
            }
        }

        /// <summary>
        ///     Our current x and y recoil
        /// </summary>
        /// <value>The current recoil</value>
        public Vector2 CurrentRecoil
        {
            get
            {
                return new Vector2(recoilXCurrent, recoilYCurrent);
            }
        }

        /// <summary>
        ///     Get the aiming canvas associated with this weapon
        /// </summary>
        internal AimCanvasBase AimCanvas
        {
            get
            {
                if(aimCanvas != null)
                {
                    return aimCanvas;
                }
                return null;
            }
        }

        /// <summary>
        ///     Get the maximum range of a bullet that can be fired from this gun
        /// </summary>
        internal float BulletRange
        {
            get
            {
                return bulletRange * bullet.rangeMultiplier;
            }
        }

        /// <summary>
        ///     Determines if we can switch away from this weapon
        ///     cleanly or not
        /// </summary>
        public bool CanSwitchWeapons
        {
            get
            {
                return !( IsReloading || switchingWeapons );
            }
        }

        /// <summary>
        ///     All the objects we are pointing at
        /// </summary>
        internal RaycastController PointingAt
        {
            set
            {
                rcc = value;
            }
        }

        #endregion


        #region Private Declarations

        float       recoilXCurrent  = 0;
        float       recoilYCurrent  = 0;
        GameObject  bulletParent;
        bool        reloadComplete  = false;
        bool        firedThisFrame  = false;
        int         recoilCount     = 0;
        int         selectorCount   = 0;
        RaycastController rcc;

        /// <summary> The amount of times fired before we pull finger off the trigger </summary>
        long firecount       = 0;

        #endregion


        #region Internal Declarations

        internal bool           triggerDown       = false;
        internal bool           switchingWeapons  = false;
        internal WeaponManager  manager;

        #endregion


        #region Monobehavoir


        // Start is called before the first frame update
        void Start()
        {
            currentClip = maxClip;
            currentAmmo = maxAmmo;
            if ( !checkResources() ) { Destroy(gameObject); return; }
        } 

        // Update is called once per frame
        void Update()
        {
            resets();
            gunLocation();
            DoAiming();            
            CheckSingleFireReset();
            CheckBursting();
        }


        private void FixedUpdate()
        {
            updateRecoil();
            updateSpread();            
        }

        private void LateUpdate()
        {
            rcc = null;
        }
        #endregion



        #region Utility Classes


        #region Internal Classes

        /// <summary>
        ///     Passed to the bullet for a collison
        /// </summary>
        internal virtual void OnBulletCollision(bool isIHealth)
        {

        }

        /// <summary>
        ///     Try to reload this weapon
        /// </summary>
        /// <returns>
        ///     A boolean on whether we are reloading
        /// </returns>
        internal virtual bool TryReload()
        {
            if ( !CanReload ) { return false; }
            else
            {
                reloadComplete = false;
            }
            
            IsReloading         = true;
            OnReload();
            return true;
        }
        
        /// <summary>
        ///     Try to fire the weapon
        /// </summary>
        /// <returns>
        ///     Returns whether or not we fired 
        ///     during this frame
        /// </returns>
        internal virtual bool TryFire()
        {
            IsFiring = true;

            TryToFire();

            updateRecoil();
            updateSpread();

            IsFiring = false;            
            return firedThisFrame;
        }

        #endregion


        #region Public

        /// <summary>
        ///     Toggle the firing selector of this gun
        /// </summary>
        /// <returns>
        ///     The index of the selected fire selector
        /// </returns>
        public int ToggleFireSelector()
        {
            if ( isHolstered || AddFiringTypes.Count == 1 ) { return 0; }
            selectorCount++;
            if (selectorCount > AddFiringTypes.Count - 1)
            {
                selectorCount = 0;
            }

            var selection   = AddFiringTypes[selectorCount];
            firingSelector  = selection.firingSelector;
            rateOfFire      = selection.rateOfFire;
            burstRate       = selection.burstRate;

            return selectorCount;            
        }

        /// <summary>
        ///     Makes the muzzle look at the target.
        /// </summary>
        /// <param name="closest">      the position we are looking at          </param>
        /// <param name="furthest">     The furthest position that we can go    </param>
        /// <param name="go">           The game object we are looking at       </param>
        internal void UpdateWeaponPoint(Vector3 closest, Vector3 furthest, GameObject go)
        {
            muzzleArea.transform.LookAt(closest);
            if ( !IsReloading && !isHolstered ) { transform.LookAt(furthest); }

            if ( go && IsAiming ) { alertAimedAtObject(go); }
        }

        /// <summary>
        ///     Determine if this object we are looking can
        ///     absorb being aimed at
        /// </summary>
        /// <param name="go">   The game object we are looking at</param>
        void alertAimedAtObject(GameObject go)
        {
            var react = go.GetComponent<AI_Asteria.IAIReactions>();

            if ( react != null ) { react.IsBeingAimedAt(); }
        }

        /// <summary>
        ///     Add bullets to our inventory
        /// </summary>
        /// <param name="amount"></param>
        public void AddAmmo(int amount)
        {
            int a = currentAmmo + amount;
            currentAmmo = a > maxAmmo ? maxAmmo : a;
        }

        #endregion


        #region Sounds
        
        /// <summary>
        ///     Play the empty bullet sound
        /// </summary>
        void PlayEmptySound()
        {
            if (emptySound.sound)
            {
                if ( firecount == 0 )
                {
                    emptySound.PlaySound(muzzleArea.transform.position);
                    firecount++;
                }
            }
            else
            {
                if(showLogs)
                {
                    Debug.LogError("There is no empty sound for " + gameObject.name);
                }
            }
        }

        /// <summary>
        ///     Play the shooting sound
        /// </summary>
        void playShootSound()
        {
            if (shootingSound.sound)
            {
                shootingSound.PlaySound(muzzleArea.transform.position);
            }
            else
            {
                if(showLogs)
                {
                    Debug.LogError("There is no shoot sound for " + gameObject.name);
                }
            }
        }

        /// <summary>
        ///     Plays the reload sound
        /// </summary>
        void playReloadSound()
        {
            if (reloadSound.sound)
            {
                reloadSound.PlaySound(transform.position);
            }
            else
            {
                if (showLogs) 
                { 
                    Debug.LogError("There is no reload sound for " + gameObject.name); 
                }
            }
        }

        #endregion


        #region Private

        /// <summary>
        ///     Reset all values if we meet the criteria
        /// </summary>
        void resets()
        {
            if (isHolstered)
            {
                IsAiming    = false;
                IsFiring    = false;
            }
        }

        /// <summary>
        ///     Reset the fire count when we pull
        ///     our finger off the trigger
        /// </summary>
        void CheckSingleFireReset()
        {
            if ( !triggerDown && !isBursting )
            {
                firedThisFrame  = false;
                firecount       = 0;
            }
        }
        
        /// <summary>
        ///     Update the guns location to 
        ///     desired its anchor point
        /// </summary>
        void gunLocation()
        {
            if ( isHolstered )
            {
                if ( !holsterAnchor )
                {
                    gameObject.SetActive(false); return;
                }

                setToPositionOfObject( holsterAnchor );
                return;
            }

            if ( IsAiming && !IsReloading )
            {
                setToPositionOfObject( aimingAnchor );
            }
            else
            {
                setToPositionOfObject( idleAnchor );
            }
        }

        /// <summary>
        ///     Set tje position and rotation of this 
        ///     weapon to its anchor point (the passed gameobject)
        /// </summary>
        /// <param name="pos">  The gameobjects to get the pos of</param>
        void setToPositionOfObject(GameObject pos)
        {
            gameObject.transform.position = pos.transform.position;
            
            if ( useRightHandIK &&  IsAiming && !IsReloading)
            {
                gunObject.transform.localPosition = aimingoffests;
            }
            else
            {
                gunObject.transform.localPosition = Vector3.zero;
                gameObject.transform.rotation = pos.transform.rotation;
            }
            
        }

        /// <summary>
        ///     Check all of our required resources
        ///     Called on the start of this class
        /// </summary>
        /// <returns> True if we clear all our pre-reqs </returns>
        bool checkResources()
        {
            bool passed = true;

            if (!muzzleArea) 
            {
                if (showLogs) Debug.LogError("There was no muzzle gameobject added on " + gameObject.name);
                passed = false; 
            }

            if (!recieverArea)
            {
                if (showLogs) Debug.LogError("There was no reciever gameobject added on " + gameObject.name);
                passed = false;
            }

            if (!bullet)
            {
                if (showLogs) Debug.LogError("There was no bullet gameobject added on " + gameObject.name);
                passed = false;
            }

            if (!idleAnchor)
            {
                if (showLogs) { Debug.Log($"There was no idle anchor for {gameObject.name}"); }
                passed = false;
            }

            if (!aimingAnchor)
            {
                if (showLogs) { Debug.Log($"There was no aiming anchor for {gameObject.name}"); }
                passed = false;
            }

            if (!holsterAnchor)
            {
                Debug.LogWarning("There is no anchor for the holster for ");
            }

            if (!gunObject)
            {
                if (showLogs) { Debug.LogError("The gun object holding the gun mesh was not present"); }
                passed = false;
            }

            // Firing Selectors
            if (AddFiringTypes == null) { AddFiringTypes = new List<FiringSelectorInfo>(); }
            selectorCount = AddFiringTypes.Count;
            AddFiringTypes.Add(new FiringSelectorInfo( rOfFire: rateOfFire, bFireRate: burstRate, type: firingSelector ) );
            
            return passed;
        }

        /// <summary>
        ///     Decrement the ammo count in the current clip
        /// </summary>
        void decrementAvailableAmmo()
        {
            currentClip--;
        }

        /// <summary>
        ///     Determine if we are single shot and
        ///     need to stop the shooting
        /// </summary>
        bool checkFireRate()
        {
            if ( firecount > 0 && ( firingSelector == FireType.Single ) )
            {
                return false;
            }
            if (firecount > 2 && firingSelector == FireType.Burst)
            {
                return false;
            }
            return true;

        }


        /// <summary>
        ///     Determine how much will be in the clip
        ///     based on how much ammo we have
        /// </summary>
        /// <returns>
        ///     The clip amount.
        /// </returns>
        int DetermineClipAmount()
        {
            if (infiniteAmmo)
            {
                currentAmmo = maxClip;
                return maxClip;
            }

            var needed          = maxClip - currentClip;
            if (currentAmmo > needed)
            {
                currentAmmo     = currentAmmo - needed;
                return maxClip;
            }
            else
            {
                var amount      = currentClip + currentAmmo;
                currentAmmo     = 0;
                return amount;
            }
        }


        /// <summary>
        ///     Check if we can fire our weapon
        /// </summary>
        /// <returns></returns>
        bool checkAmmo()
        {
            if (currentClip <= 0)
            {
                PlayEmptySound();
                return false;
            }
            else
            {
                return true;
            }            
        }


        /// <summary>
        ///     Create the bullet parent
        /// </summary>
        void DevelopBulletParent()
        {
            if ( !bulletParent )
            {
                bulletParent = GameObject.Find("BulletParent");
                if (!bulletParent)
                {
                    bulletParent = new GameObject("BulletParent");
                }
            }
        }

        /// <summary>
        ///     Check if we can shoot this object
        /// </summary>
        /// <returns>
        ///     A boolean if we can shoot the object
        /// </returns>
        bool checkIfWeCanShootObject()
        {
            if ( rcc == null ) { return true; }
            foreach (var c in rcc.OrderedHits) 
            {
                var react = c.transform.gameObject.GetComponent<AI_Asteria.IAIReactions>();
                if (react != null)
                {
                    return react.CanShoot;
                }
            }
            return true;
        }

        #endregion


        #region Protected
        
        /// <summary>
        ///     Aiming our weapon
        /// </summary>
        protected virtual void DoAiming()
        {
            if ( IsAiming )
            {
                doWhileAiming();
            }
        }

        /// <summary>
        ///     Reload the weapon. Handles the ammo count
        ///     and sounds
        /// </summary>
        protected virtual void OnReload()
        {
            if ( IsReloading )
            {
                if( !reloadComplete )
                {
                    if (currentAmmo > 0)
                    {
                        currentClip     = DetermineClipAmount();
                        playReloadSound();
                    }
                    else
                    {
                        // play gun empty sound or do no ammo anim?
                    }
                    reloadComplete = true;
                }
            }
        }

        /// <summary>
        ///     Determine our rate of fire and if we can shoot again
        /// </summary>
        /// <returns> if enough time has passed.</returns>
        protected virtual bool DetermineLastFire()
        {
            if ( isBursting )
            {
                if ((Time.time - LastFireTime) > BurstFireIntervals)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ( ( Time.time - LastFireTime ) > FireRateIntervals )
            {
                return true;
            }
            return false;
        }

        bool isBursting = false;
        /// <summary>
        ///     Fire the weapon. Will instantiate an instance
        ///     of a bullet and shoot in the general direction 
        ///     of where we are aiming
        /// </summary>
        protected virtual void TryToFire()
        {
            firedThisFrame = false;

            if ( CanFireWeapon )
            {
                DoFire();
            }
        }

        /// <summary>
        ///     If we can fire the weapon, this will call
        /// </summary>
        protected virtual void DoFire()
        {
            decrementAvailableAmmo();
            playShootSound();
            instantiateBullet();
            firedThisFrame = true;
            firecount++;
            ShotsFired++;
            if (firingSelector == FireType.Burst) { isBursting = true; }
        }

        /// <summary>
        ///     Check if we should continue the burst firing
        /// </summary>
        protected virtual void CheckBursting()
        {
            if ( firingSelector != FireType.Burst || !isBursting) return;

            if ( firecount > 2 || IsReloading || !IsAiming)
            {
                isBursting  = false;
                return;
            }

            if (FiringConditionsMet)
            {
                DoFire();                
            }            
            else
            {
                firedThisFrame = false;
            }
        }

        /// <summary>
        ///     Create an instance of the bullet the gun fires
        /// </summary>
        protected virtual void instantiateBullet()  { LastFireTime = Time.time; }
        protected virtual void doWhileAiming()      { }

        float spreadNum = 1.0f;
        protected virtual void updateSpread()
        {
            if ( aimCanvas != null )
            {
                if ( firedThisFrame )
                {
                    // udpate spread
                    Vector2 spreadL = new Vector2(spreadNum *Time.deltaTime, spreadNum + 1.0f * Time.deltaTime);                
                    spreadNum++;
                    aimCanvas.UpdateSpreads(spreadL);
                }
                else
                {
                    aimCanvas.ResetSpreads(spreadReset);
                }
            }        
        }

        /// <summary>
        ///     Update the weapon recoil
        /// </summary>
        protected virtual void updateRecoil()
        {
            if (firedThisFrame)
            {
                bool odd = false;
                if (recoilCount % 2 > 0) { odd = true; }
                recoilCount++;

                var xRange = odd ? -1 : 1;

                recoilXCurrent = Mathf.Clamp(value: recoilXCurrent += recoilX * xRange,
                    min: -recoilXMax, max: recoilXMax);

                recoilYCurrent = Mathf.Clamp(value: recoilYCurrent += recoilY, min: 0, max: recoilYMax);
            }
            else
            {
                if (recoilXCurrent == 0.0f && recoilYCurrent == 0.0f) { recoilCount = 0; return; }    
                
                // TODO test this out side the if else loop
                recoilXCurrent = Mathf.Lerp(recoilXCurrent, 0, recoilReset);
                recoilYCurrent = Mathf.Lerp(recoilYCurrent, 0, recoilReset);
            }
        }

        #endregion

        #endregion

    }
}