using AsteriaGeneral;
using Human_Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{
    [ RequireComponent( typeof( HumanController     ) ) ]
    [ RequireComponent( typeof( ThirdPersonInput    ) ) ]
    public class WeaponsController : WeaponManager
    {
        #region Input handles

        public HandleInput InputFire;
        public HandleInput InputADS;
        public HandleInput InputReload;
        public HandleInput InputRotateWeapon;
        public HandleInput InputUenquip;
        public HandleInput InputWeapon1;
        public HandleInput InputWeapon2;
        public HandleInput InputWeapon3;
        public HandleInput InputToggleFiring;


        RaycastController rc;
        #endregion


        #region Editor
        [Header("Input Handling")]
        [SerializeField]
        [Tooltip( "Determine if we just want to press the ads button or hold it for aiming" ) ]
        bool toggleAim              = false;

        [SerializeField]
        [Tooltip( "Automtically reloads when we shoot at 0 bullets " ) ]
        protected bool autoReload   = true;

        #endregion


        #region Required Components

        HumanController     tpc;

        #endregion


        #region Properties

        public RaycastController PointingAt
        {
            set
            {
                rc = value;
            }
        }

        /// <summary>
        ///     Get our current recoil from our gun
        /// </summary>
        public Vector2 GetRecoil
        {
            get
            {
                if (CurrentWeapon ?? false)
                {
                    return CurrentWeapon.CurrentRecoil;
                }
                return new Vector2();
            }
        }

        #endregion


        #region MonoBehavoir

        // Start is called before the first frame update
        protected override void Start()
        {
            GetRequiredComponents();
            base.Start();
        }

        /// <summary>
        ///     To be called for updates on the calling party
        /// </summary>
        internal void OnUpdate()
        {
            if (Weapons == null) { return; }
            checkAiming();
            toggleFiringSelector();
            selectWeapon();
            reloadWeapon();
            fireWeapon();
        }

        internal void OnLateUpdate() { }


        internal void OnFixedUpdate() { } 
     
        #endregion


        #region Update Guns

        /// <summary>
        ///     Toggle the firing selector
        /// </summary>
        void toggleFiringSelector()
        {
            if (InputToggleFiring.GetButtonPressed)
            {
                CurrentWeapon.ToggleFireSelector();
            }
        }

        /// <summary>
        ///     Change the weapon selection or
        ///     put the weapon away
        /// </summary>
        void selectWeapon()
        {
            if( InputUenquip.GetButtonPressed )
            {
                tpc.isADS = false;
                PutAway();                
            }
            else if ( InputRotateWeapon.GetButtonPressed )
            {
                IncrementWeapon();
            }
            else if (checkWeaponSelection(out int i ) )
            {
                ChangeWeapon(i);
            }
            
            if (CurrentWeaponType == WeaponType.None) { tpc.isADS = false; }    // force us to go out of ads when holstered
            
            // requirements
            tpc.holdingWeaponType       = CurrentWeaponType;
            tpc.GunHandL                = CurrentWeapon.HandMountL;
            tpc.GunHandR                = CurrentWeapon.HandMountR;
        }

        /// <summary>
        ///     Select the weapon that we want to equip
        /// </summary>
        /// <param name="i">    The integer of the selection    </param>
        /// <returns>
        ///     A true value if a selection was made, returning the 
        ///     selection with the out parameter
        /// </returns>
        bool  checkWeaponSelection(out int i)
        {
            i = 0;
            if (InputWeapon1.GetButtonPressed)
            {
                i = 0;
                return true;
            }
            else if (InputWeapon2.GetButtonPressed)
            {
                i = 1;
                return true;
            }
            else if (InputWeapon3.GetButtonPressed)
            {
                i = 2;
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Reload the weapon
        /// </summary>
        void reloadWeapon()
        {
            bool started = false;
            
            bool r = ( InputReload.GetButtonPressed && CurrentWeapon.TryReload()) || ( autoReload &&  CurrentWeapon.CurrentClip == 0 && CurrentWeapon.CurrentAmmo != 0 && CurrentWeapon.TryReload() );

            if ( r )
            {
                tpc.isReloading     = true;
                started             = true;
            }         

            if ( !started ) { SetCharacterReloading( tpc.DetermineReloadStatus() ); };
        }

        /// <summary>
        ///     Fire the weapon. 
        ///     Update the muzzle
        /// </summary>
        void fireWeapon()
        {
            if ( tpc.isADS )
            {
                CurrentWeapon.UpdateWeaponPoint( rc.StraightLineHitAdjusted, rc.FurthestDistance, rc.FirstObjectHit );
            }
            
            bool firing = InputFire.isPressed && 
                    isAiming &&
                    CurrentWeaponType != WeaponType.None;

            if ( firing ){ CurrentWeapon.PointingAt = rc; doFire(); }

            CurrentWeapon.triggerDown   = firing;
            tpc.isFiring                = firing;
        }

   

        /// <summary>
        ///     Get all the components of requied for this
        ///     class
        /// </summary>
        void GetRequiredComponents()
        {

            tpc = GetComponent<HumanController>();
            if (!tpc)
            {
                Debug.LogError("There was an issue getting the human controller properly on " + gameObject.name);
            }
        }

        #endregion


        #region Update Controller

        /// <summary>
        ///     Check if we are aiming our weapon
        /// </summary>
        private void checkAiming()
        {
            if (tpc.canCombat)
            {
                if(toggleAim)
                {
                    bool action = tpc.isADS;
                    if( InputADS.GetButtonPressed )
                    {
                        tpc.isSprinting = false;
                        action = !action;
                    }
                    tpc.isADS           = action;
                    tpc.isStrafing      = action;
                    isAiming            = action;
                }
                else
                {

                    if ( InputADS.isPressed && tpc.canADS )
                    {
                        tpc.isADS           = true;
                        tpc.isStrafing      = true;
                        tpc.isSprinting     = false;
                        isAiming            = true;
                    }
                    else
                    {
                        tpc.isADS           = false;
                        tpc.isStrafing      = false;
                        isAiming            = false;
                    }
                }
            }
        }
        #endregion

    }
}