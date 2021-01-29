using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Human_Controller;

namespace AsteriaWeapons
{
    
    public class WeaponsInput : MonoBehaviour
    {
        WeaponsController weapons;
        ThirdPersonCamera tpCamera;


        // Start is called before the first frame update
        void Start()
        {
            weapons     = GetComponent<WeaponsController>();
            InitializeTpCamera();

            if (!tpCamera)
            {
                Debug.LogError($"There is no tp camera associated with the character {gameObject.name}");
                Destroy(this);
            }
            if (!weapons) 
            {
                Debug.LogError($"There is no weapons controller associated with the character {gameObject.name}");
                Destroy(this); 
            }
        }

        // Update is called once per frame
        void Update()
        {
            checkAiming();
            selectWeapon();
            reloadWeapon();
            fireWeapon();
            updateHeadzUp();
            checkFiringToggle();

            weapons.OnUpdate();
        }

        /// <summary>
        ///     Get the aiming inputs
        /// </summary>
        void checkAiming()
        {
            if ( Input.GetButtonDown("Fire2" ) ) { weapons.InputADS.buttonIsPressed(); }            
            weapons.InputADS.isPressed  = Input.GetButton( "Fire2" );
        }

        /// <summary>
        ///     Accept our firing toggle
        /// </summary>
        void checkFiringToggle()
        {
            if (Input.GetButtonDown("FiringSelector")) { weapons.InputToggleFiring.buttonIsPressed(); }
            weapons.InputToggleFiring.isPressed = Input.GetButton("FiringSelector");

        }

        /// <summary>
        ///     Get the weapons change inputs
        /// </summary>
        void selectWeapon()
        {
            if ( Input.GetButtonDown( "Unequip" ) )  { weapons.InputUenquip.buttonIsPressed(); }
            weapons.InputUenquip.isPressed  = Input.GetButton("Unequip");


            if ( Input.GetButtonDown("NextWeapon") )  { weapons.InputRotateWeapon.buttonIsPressed(); }
            weapons.InputRotateWeapon.isPressed = Input.GetButton("NextWeapon");


            if (Input.GetButtonDown("WeaponOne")) { weapons.InputWeapon1.buttonIsPressed(); }
            weapons.InputWeapon1.isPressed = Input.GetButton("WeaponOne");


            if (Input.GetButtonDown("WeaponTwo")) { weapons.InputWeapon2.buttonIsPressed(); }
            weapons.InputWeapon2.isPressed = Input.GetButton("WeaponTwo");


            if (Input.GetButtonDown("WeaponThree")) { weapons.InputWeapon3.buttonIsPressed(); }
            weapons.InputWeapon3.isPressed = Input.GetButton("WeaponThree");
        }

        /// <summary>
        ///     reload weapon inputs
        /// </summary>
        void reloadWeapon()
        {
            if (Input.GetButtonDown("Reload")) { weapons.InputReload.buttonIsPressed(); }
            weapons.InputReload.isPressed = Input.GetButton("Reload");
        }

        /// <summary>
        ///     Fire the weapon inputs
        /// </summary>
        void fireWeapon()
        {
            if (Input.GetButtonDown("Fire1")) { weapons.InputFire.buttonIsPressed(); }
            weapons.InputFire.isPressed = Input.GetButton("Fire1");
        }

        /// <summary>
        ///     Update the camera an UI for aiming
        /// </summary>
        void updateHeadzUp()
        {
            gunPointUpdate();
            sendRecoilToCamera();
        }
        void gunPointUpdate()
        {
            weapons.PointingAt = tpCamera.getNearestStraightLineTarget(50);

        }

        /// <summary>
        ///     Send the recoid to the camera
        /// </summary>
        void sendRecoilToCamera()
        {
            if (tpCamera) { tpCamera.UpdateRecoilOffset(x: weapons.GetRecoil.x, y: weapons.GetRecoil.y ); }
        }


        #region Camera Stuff

        /// <summary>
        ///     Set the TPC Camara
        /// </summary>
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


        #endregion

    }

}