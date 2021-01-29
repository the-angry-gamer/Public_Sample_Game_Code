using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Human_Controller;

namespace AsteriaHUD
{
    public class HUDController : MonoBehaviour
    {
        #region Editor

        [Header("Parent Objects")]
        [SerializeField]
        [Tooltip("The main character the HUD will display on")]
        GameObject character;

        [SerializeField]
        [Tooltip("The game object holding the ammo section of the HUD")]
        GameObject ammoSection;

        [SerializeField]
        [Tooltip("The game object holding the health section of the HUD")]
        GameObject healthSection;

        [SerializeField]
        [Tooltip("The game object holding the aiming section of the HUD")]
        GameObject aimingSection;

        [SerializeField]
        [Tooltip("The game object holding the stamina section of the HUD")]
        GameObject staminaSection;

        [Header("Weapon Components")]
        [SerializeField]
        bool        showWeapons = true;

        [SerializeField]
        GameObject clipItem;

        [SerializeField]
        GameObject totalClipItem;

        [SerializeField]
        GameObject currentAmmoItem;

        [SerializeField]
        GameObject totalAmmoItem;

        [SerializeField]
        GameObject gunImageItem;

        [Header("Health Components")]
        [SerializeField]
        bool showHealth = true;

        //[SerializeField]
        //GameObject TotalHealthItem;

        [SerializeField]
        GameObject HealthLeftItem;

        [SerializeField]
        GameObject HealthSlider;

        [Header("Stamina Components")]
        [SerializeField]
        bool showStamina = true;

        [SerializeField]
        GameObject StaminaSlider;

        #endregion


        #region Required Components

        AsteriaWeapons.WeaponManager    weapons;

        AsteriaHealth.HealthManager     health;

        ThirdPersonInput                characterInput;

        Slider                          healthSliderObj;
        Slider                          staminaSliderObj;

        GameObject                      leftAim;
        GameObject                      rightAim;
        GameObject                      topAim;
        GameObject                      botAim;
        GameObject                      centAim;

        Image                           gunImage;


        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if ( !findCharacter() ) { Destroy(gameObject); }
            
            GetComponentsRequired();
            findAllCanvasItems();
        }

        // Update is called once per frame
        void Update()
        {
            updateAmmo();
            updateHealth();
            updateStamina();            
        }

        private void LateUpdate()
        {
            updateAiming();    //    here so it accumulates changes on the fire per frame        
        }

        /// <summary>
        ///     Update the gunz on the HUD
        /// </summary>
        void updateAiming() 
        { 
            if ( weapons && weapons.isAiming && !weapons.CurrentWeapon.isHolstered )
            {
                AsteriaWeapons.AimCanvasBase aim = weapons.CurrentWeapon.AimCanvas;
                if ( !aimingSection.activeInHierarchy ) { aimingSection.SetActive(true); }

                if (leftAim)    { updateCanvasAim( item: leftAim,    draw: aim.Left.CanvasImage,        x: (int)aim.Left.CurrentOffset,    y: 0,   scale: aim.Left.Scale       );  }
                if (rightAim)   { updateCanvasAim( item: rightAim,   draw: aim.Right.CanvasImage,       x: (int)aim.Right.CurrentOffset,   y: 0,   scale: aim.Right.Scale      );  }
                if (topAim)     { updateCanvasAim( item: topAim,     draw: aim.Top.CanvasImage,         x: 0,  y: (int)aim.Top.CurrentOffset,      scale: aim.Top.Scale        );  }
                if (botAim)     { updateCanvasAim( item: botAim,     draw: aim.Bottom.CanvasImage,      x: 0,  y: (int)aim.Bottom.CurrentOffset,   scale: aim.Bottom.Scale     );  }
                if (centAim)    { updateCanvasAim( item: centAim,    draw: aim.Center.CanvasImage,      x: 0,  y: 0,                               scale: aim.Center.Scale     );  }
            }
            else
            {
                if (aimingSection && aimingSection.activeInHierarchy) { aimingSection.SetActive(false); }
            }
        }

        /// <summary>
        ///     Update each canvas aiming item
        /// </summary>
        /// <param name="item"> The item on the canvase to update   </param>
        /// <param name="draw"> The sprite to draw on the canvas    </param>
        /// <param name="x">    The x offset                        </param>
        /// <param name="y">    The y offset                        </param>
        /// <param name="scale"> The scale of the object            </param>
        void updateCanvasAim(GameObject item, Sprite draw, int x, int y, float scale)
        {
            if (draw)
            {
                if (!item.activeInHierarchy) { item.SetActive(true); }
                var drawMe = item.GetComponent<Image>();
                if (!drawMe) { return; }

                item.transform.localPosition    = new Vector3(x: x, y: y, z: item.transform.position.z);                
                item.transform.localScale       = new Vector3(1.0f * scale, 1.0f * scale, 1.0f * scale);

                drawMe.sprite = draw;
            }
            else
            {
                item.SetActive(false);
            }
        }

        /// <summary>
        ///     Update the stamina of the HUD
        /// </summary>
        void updateStamina()
        {
            if (showStamina != staminaSection.gameObject.activeInHierarchy) { staminaSection.gameObject.SetActive(showStamina); }

            if (staminaSection && showStamina)
            {
                if ( staminaSliderObj )
                {
                    staminaSliderObj.maxValue   = characterInput.SprintUsage.Total;
                    staminaSliderObj.value      = characterInput.SprintUsage.Current;
                }
            }
        }

        /// <summary>
        ///     Update the health on the HUD
        /// </summary>
        void updateHealth()
        {
            if (showHealth != healthSection.gameObject.activeInHierarchy) { healthSection.gameObject.SetActive(showHealth); }

            if (health && showHealth)
            {
                var h = health.CurrentHealth;
                var t = health.TotalHealth;

                HealthLeftItem.GetComponent<Text>().text    = h.ToString();
                
                if (healthSliderObj)
                {
                    healthSliderObj.maxValue  = t; ;
                    healthSliderObj.value     = h;
                }
            }
        }

        /// <summary>
        ///     Update the ammo on the HUD
        /// </summary>
        void updateAmmo()
        {
            if (!ammoSection) { return; }
            if (showWeapons != ammoSection.gameObject.activeInHierarchy) { ammoSection.gameObject.SetActive(showWeapons); }

            if (weapons && showWeapons )
            {
                var clip            = weapons.CurrentWeapon.CurrentClip;
                var totalClip       = weapons.CurrentWeapon.MaxClip;
                var current         = weapons.CurrentWeapon.CurrentAmmo;
                var totalAmmo       = weapons.CurrentWeapon.AmmoLimit;

                clipItem.GetComponent<Text>().text          = clip.ToString();
                totalClipItem.GetComponent<Text>().text     = $"/ {totalClip.ToString()}";
                currentAmmoItem.GetComponent<Text>().text   = current.ToString();
                totalAmmoItem.GetComponent<Text>().text     = totalAmmo.ToString();
            }
            updateGunImage();
        }

        /// <summary>
        ///     Update the gun image
        /// </summary>
        void updateGunImage()
        {
            if(gunImage)
            {
                Sprite s = weapons.CurrentWeapon.GunSpriteHUD;
                if (s)
                {
                    if (!gunImage.gameObject.activeInHierarchy) { gunImage.gameObject.SetActive(true); }

                    gunImage.sprite = s;
                }
                else
                {
                    gunImage.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        ///     Find the character we want the HUD on 
        /// </summary>
        /// <returns>
        ///     A boolean if we succesfully found the main character
        /// </returns>
        bool findCharacter()
        {
            if (!character)
            {
                Debug.LogError("The character was not added, attempting to resolve");
                character = FindObjectOfType<ThirdPersonInput>()?.transform.gameObject;
            }

            if (character)
            {
                characterInput = character.GetComponent<ThirdPersonInput>();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Get all the required components
        /// </summary>
        void GetComponentsRequired()
        {
            health  = character.GetComponent<AsteriaHealth.HealthManager>();
            weapons = character.GetComponent<AsteriaWeapons.WeaponManager>();
            
            if (!health)    
            {
                Debug.LogWarning("There is no health manager on the character. No health will be drawn");
                Destroy(healthSection); 
            }
            if (!weapons)   
            { 
                Debug.LogWarning("There is no weapons manager on the character. No ammo or weapons will be drawn");
                Destroy(ammoSection);
                Destroy(aimingSection);
            }
            if (!characterInput.LimitSprint)
            {
                Debug.LogWarning("The characters stamina is not limited. The limit will not be displayed");
                Destroy(staminaSection);
            }

        }

        /// <summary>
        ///     Get the items we want to update
        /// </summary>
        /// <returns>
        ///     A boolean on whether we could find something
        /// </returns>
        bool findAllCanvasItems()
        {
            bool w = weapons ? findAllWeaponAmmoItems()  && getAimingSections() : true;
            bool h = health ? findAllHealthItems() : true;

            if( StaminaSlider)
            {
                staminaSliderObj = StaminaSlider.GetComponent<Slider>();
                if (!staminaSliderObj)
                {
                    Debug.LogError("The stamina slider does not have a slider component on it");
                    return false;
                }
            }


            return w && h;
        }

        /// <summary>
        ///     Find all the weapon items for the ammo
        /// </summary>
        /// <returns></returns>
        bool findAllWeaponAmmoItems()
        {
            if ( !ammoSection ) { Debug.LogError("The ammo section has not been assigned."); return false; }

            if ( !clipItem          )   { clipItem          = ammoSection.transform.Find( "currentClip" )?.gameObject;  }
            if ( !totalClipItem     )   { totalClipItem     = ammoSection.transform.Find( "TotalClip"   )?.gameObject;  }
            if ( !currentAmmoItem   )   { currentAmmoItem   = ammoSection.transform.Find( "CurrentAmmo" )?.gameObject;  }
            if ( !totalAmmoItem     )   { totalAmmoItem     = ammoSection.transform.Find( "AmmoLimit"   )?.gameObject;  }
            if ( !gunImageItem      )   { gunImageItem      = ammoSection.transform.Find( "GunImage"    )?.gameObject;  }

            if (gunImageItem) { gunImage = gunImageItem.GetComponent<Image>(); }

            return clipItem && totalClipItem && currentAmmoItem && totalAmmoItem && gunImageItem;
        }

        /// <summary>
        ///     Find all the health items on the HUD
        /// </summary>
        /// <returns></returns>
        bool findAllHealthItems()
        {
            if ( !healthSection ) { Debug.LogError("The Health section has not been assigned."); return false; }

            if ( !HealthLeftItem    ) { HealthLeftItem  = healthSection.transform.Find(     "HealthLeft")?.gameObject;      }
          //  if ( !TotalHealthItem   ) { TotalHealthItem = healthSection.transform.Find(     "TotalHealth")?.gameObject;     }
            if ( !HealthSlider      ) { HealthSlider    = healthSection.transform.Find(     "HealthSlider")?.gameObject;    }
            if ( !StaminaSlider     ) { StaminaSlider   = staminaSection.transform.Find(    "StaminaSlider")?.gameObject;   }

            if (HealthSlider)
            {
                healthSliderObj = HealthSlider.GetComponent<Slider>();
                if (!healthSliderObj)
                {
                    Debug.LogError("The health slider does not have a slider component on it");
                    return false;
                }
            }
            return HealthLeftItem && /*TotalHealthItem &&*/ HealthSlider;
        }

        /// <summary>
        ///     Get the aiming sections
        /// </summary>
        /// <returns>
        ///     a boolean if everything is found correctly
        /// </returns>
        bool getAimingSections()
        {
            if ( aimingSection )
            {
                leftAim     = aimingSection.transform.Find( "Left"      )?.gameObject;
                rightAim    = aimingSection.transform.Find( "Right"     )?.gameObject;
                topAim      = aimingSection.transform.Find( "Top"       )?.gameObject;
                botAim      = aimingSection.transform.Find( "Bottom"    )?.gameObject;
                centAim     = aimingSection.transform.Find( "Center"    )?.gameObject;

                return leftAim && rightAim && topAim && botAim && centAim;
            }
            return false;
        }
    }

}