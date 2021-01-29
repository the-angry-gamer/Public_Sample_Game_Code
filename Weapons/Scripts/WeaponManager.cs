using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaWeapons
{
    public class WeaponManager : MonoBehaviour
    {
        #region Editor

        [Header("Weapons Manager")]
        [SerializeField]
        bool ShowLogs       = true;

        [SerializeField]
        [Tooltip("Determine if we want to check for a weapon holster")]
        bool holsterWeapon  = false;

        [SerializeField]
        [Tooltip("What layers to make contact with")]
        internal LayerMask LayersToHit = 1 << 0;

        [SerializeField]
        internal List<WeaponBase> Weapons;

        [SerializeField]
        internal bool InfiniteAmmo = false;

        #endregion


        #region Internal Declarations

        /// <summary>
        ///     How many shots total we have fired through this 
        ///     weapons manager
        /// </summary>
        public long TotalShotsFired
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets or Sets whether the current weapon is 
        ///     being aimed
        /// </summary>
        internal bool isAiming 
        {
            get { return CurrentWeapon.IsAiming; }
            set { CurrentWeapon.IsAiming = value; }
        }

        internal bool       isFiring { get { return CurrentWeapon.IsFiring; } }           
        
        internal Animator   animator;

        /// <summary>
        ///     The weapon index we currently have set
        /// </summary>
        internal int SelectedWeapon 
        {
            private set;
            get;
        }

        /// <summary>
        ///     Returns the weapon we currently have selected.
        ///     Will reset our machinery if we have a null.
        ///     Will return null if there are no weapons available
        ///</summary>
        internal WeaponBase CurrentWeapon
        {
            get
            {
                if ( Weapons == null )          { return null; }
                if ( SelectedWeapon < 0 )       { return null; }
                if ( Weapons.Count == 0 )       { return null; }
                
                if( Weapons[ SelectedWeapon ] == null) 
                { 
                    RecreateWeaponList();
                    if (SelectedWeapon > Weapons.Count - 1) { return null; }
                }
                if ( !Weapons[ SelectedWeapon ].enabled ) { Weapons[ SelectedWeapon ].enabled = true; }

                return Weapons[ SelectedWeapon ];
            }
        }

        /// <summary>
        ///     Will grrab our current weapon.
        ///     Will not reset if anything is null
        /// </summary>
        internal WeaponBase CurrentWeaponNoReset
        {
            get
            {
                if (SelectedWeapon < 0)     { return null; }
                if (Weapons ==  null)       { return null; }
                if (Weapons?.Count == 0)    { return null; }

                if (Weapons[ SelectedWeapon ] == null)
                {
                    if (SelectedWeapon > Weapons.Count - 1) { return null; }
                }
                
                return Weapons[ SelectedWeapon ];
            }
        }

        
        #endregion


        #region  Public Properties

        /// <summary>
        ///     The name of our current weapons
        /// </summary>
        public string CurrentWeaponName
        {
            get
            {
                if ( CurrentWeaponNoReset && CurrentWeaponNoReset.gameObject.activeSelf )
                {
                    return CurrentWeaponNoReset?.gameObject.name ?? "No Weapon Selected";
                }
                
                return "Nothing Active";
            }
        }

        /// <summary>
        ///     Determine what type of weapon we currently have
        /// </summary>
        public WeaponType CurrentWeaponType
        {
            get
            {
                if ( CurrentWeaponNoReset && CurrentWeaponNoReset.gameObject.activeSelf && !CurrentWeapon.isHolstered)
                {
                    return CurrentWeaponNoReset.WeaponTypeOf;
                }
                return WeaponType.None;
            }
        }

        #endregion


        #region Private Declarations


        #endregion


        #region Monobehavior

        // Start is called before the first frame update
        protected virtual void Start()
        {
            RecreateWeaponList();
            animator = GetComponent<Animator>();
            if (!animator) { if ( ShowLogs) Debug.LogError("There is no animator present for the weapons manager"); }
        }


        #endregion


        #region Weapon Actions

        #region Public

        public void EditorIncrement() { IncrementWeapon(); }
        public void EditorDecrement() { DecrementWeapon(); }

        /// <summary>
        ///     Put away all weapons - reset to unarmed
        /// </summary>
        public void PutAway()
        {
            toggleCurrentWeapon( false );
        }

        /// <summary>
        ///     Take out our weapon - return to armed
        /// </summary>
        public void TakeOut()
        {
            toggleCurrentWeapon(true);
        }

        #endregion


        #region Private

        
        #region Animator

        #endregion

        /// <summary>
        ///     Tun off / on the current weapon safely
        /// </summary>
        /// <param name="enable"></param>
        void toggleCurrentWeapon( bool enable)
        {
            if( CurrentWeapon )
            {
                if (holsterWeapon)
                {
                    CurrentWeapon.isHolstered = !enable;
                }
                else
                {
                    CurrentWeapon.gameObject.SetActive(enable);
                }
            }
        }

       

        /// <summary>
        ///     Enable one of our weapons
        /// </summary>
        /// <param name="selection">Selection</param>
        void switchWeapons(int selection)
        {
            SelectedWeapon = selection;

            // Disable and enable weapons
            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i] == null) { RecreateWeaponList(); }
                if (i != selection)
                {
                    if ( !holsterWeapon )
                    {
                        Weapons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        Weapons[i].isHolstered = true;
                    }
                }
                else
                {
                    Weapons[ selection ].gameObject.SetActive(true);
                    Weapons[ selection ].isHolstered = false;
                }
            }
        }

        /// <summary>
        ///     Rotate through our weapons.
        /// </summary>
        /// <param name="up">   Increment for yes, decrement for no </param>
        void rotateThroughWeapon(bool up)
        {            
            if ( !CurrentWeapon.CanSwitchWeapons ) { return; }

            if ( holsterWeapon )
            {
                if ( CurrentWeapon.isHolstered ) { CurrentWeapon.isHolstered = false; return; }
            }

            if ( !CurrentWeapon.gameObject.activeSelf )
            {
                CurrentWeapon.gameObject.SetActive( true );
            }
            else
            {
                int newWeapon = 0;
                if (up)
                {
                    newWeapon = SelectedWeapon == Weapons.Count - 1 ? 0 : SelectedWeapon + 1;
                }
                else
                {
                    newWeapon = SelectedWeapon == 0 ? Weapons.Count - 1 : SelectedWeapon - 1;
                }

                ChangeWeapon( newWeapon );
            }
        }

        #endregion


        #region Internal


        /// <summary>
        ///     Disable and enable weapons
        /// </summary>
        /// <param name="selection"> The weapon that was selected </param>
        internal void ChangeWeapon(int selection)
        {
            if ( selection > Weapons.Count - 1 || selection == SelectedWeapon) { return; }

            switchWeapons(selection);
           
            if (ShowLogs)
            {
                Debug.Log($"{gameObject.name} changed selected weapon has been changed to " +
                	$"number {SelectedWeapon.ToString()}: {CurrentWeapon.gameObject.name}");
            }
        }

        ///<summary>
        ///     Set the next weapon active, 
        ///     return to 0 if we are maxed out
        ///</summary>
        internal void IncrementWeapon()
        {
            rotateThroughWeapon(up: true);
        }

        ///<summary>
        ///     Set the previous weapon. 
        ///     If 0, goes back to top.
        ///</summary>
        internal void DecrementWeapon()
        {
            rotateThroughWeapon(up: false);
        }

      
        /// <summary>
        ///     Clear out any null weapons if we have any
        /// </summary>
        internal void RecreateWeaponList()
        {
            if (Weapons == null) { return; }
            var temp = new List<WeaponBase>();

            foreach (WeaponBase b in Weapons)
            {
                if ( b )
                {
                    if (holsterWeapon) { b.isHolstered = true; b.gameObject.SetActive(true); }
                    else { b.gameObject.SetActive(false); }
                    
                    if (InfiniteAmmo) { b.InfiniteAmmo = InfiniteAmmo; }
                    
                    b.manager = this;
                    temp.Add(b);
                }
            }
            Weapons = new List<WeaponBase>();
            Weapons.AddRange(temp);

            if (Weapons.Count == 0) 
            {
                Debug.LogError($"There were no weapons assigned to {gameObject.name} weapons manager");
                Destroy(gameObject);  
            }
        }

        
        /// <summary>
        ///     Fire the current weapon
        /// </summary>
        /// <returns>
        ///     A bool if he gun was fired this frame
        /// </returns>
        internal bool doFire()
        {
            bool shotsFired = CurrentWeapon.TryFire();
            if (shotsFired) { TotalShotsFired++; }
            return shotsFired;
        }

        internal void SetCharacterReloading(bool val)   { CurrentWeapon.IsReloading = val; }

        #endregion

        #endregion

    }
}