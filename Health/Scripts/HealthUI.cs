
using AsteriaGeneral;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AsteriaHealth
{

    [RequireComponent(typeof(HealthManager))]
    public class HealthUI : MonoBehaviour
    {

        #region Editor

        [SerializeField]
        bool log = false;

        [SerializeField]
        [Tooltip("Whether to continously draw the object")]
        bool constant = false;

        [SerializeField]
        [Tooltip("If not constant, this will fade the object away")]
        CanvasGroupFade fade;

        [SerializeField]
        [Tooltip("The canvas item that contains the slider")]
        RectTransform healthBar;

        [SerializeField]
        [Tooltip("How much y offset compared to the parent you give it")]
        [Range(0,15)]
        float yOffset = 2.0f;

        #endregion


        #region Private Declarations

        HealthManager   manager;
        Slider          slider;
        
        bool            unregd      = false;

        #endregion


        #region Private functions
      
        /// <summary>
        ///     Draw the health on the user interface
        /// </summary>
        void DrawUI()
        {
            if (manager && healthBar)
            {
                draw();
            }
            else
            {
                if (log) { Debug.LogError("Something terrible has happened to our HealthUI manager"); }
            }
        }

        /// <summary>
        ///     draw the health slider
        /// </summary>
        void draw()
        {
            fade.draw();

            slider.maxValue = manager.TotalHealth;
            slider.value    = manager.CurrentHealth;
        }

        /// <summary>
        ///     Check to see if we want to continue to draw the item.
        /// </summary>
        void check()
        {
            if (constant) { draw(); }
            else
            {
                fade.fadeAway();
                if ( !fade.isActive && unregd)
                {
                    selfDestruct();
                }
            }
        }

        /// <summary>
        ///     Reposition the sliding health bar
        /// </summary>
        void positionSlider()
        {
            var v = gameObject.transform.position;
            healthBar.transform.position = new Vector3(v.x, v.y + yOffset, v.z);
            healthBar.transform.rotation = Camera.main.transform.rotation;
        }



        /// <summary>
        ///     Set the initial state of the health bar
        /// </summary>
        void setInitial()
        {
            if (!constant && fade.isActive)
            {
                fade.Deactivate();
            }
        }

        /// <summary>
        ///     Determine if we have no life left
        /// </summary>
        void NoLifeLeft()
        {
            if (manager.AmIDead)
            {
                manager.UnRegisterHitAction(DrawUI);
                unregd = true;
            }
            if (constant && unregd)
            {
                selfDestruct();
            }
        }

        /// <summary>
        ///     Remove the health bar and this script
        /// </summary>
        void selfDestruct()
        {
            Destroy(healthBar.gameObject);
            Destroy(this);            
        }

        #endregion


        #region Mono

        void Start()
        {
            if (!healthBar)
            {
                if (log) { Debug.LogError($"There is no health display on {gameObject.name}"); }
                Destroy(this);
            }
            healthBar = Instantiate(original: healthBar, gameObject.transform);

            manager = GetComponent<HealthManager>();
            var cg  = healthBar.GetComponent<CanvasGroup>();
            slider  = healthBar.GetComponentInChildren<Slider>();

            fade = new CanvasGroupFade(cg);

            if (!manager) 
            { 
                if(log) { Debug.LogError($"There is no health manager attached (HealthUI) on {gameObject.name}"); }
                selfDestruct();
            }
            else if ( !cg || !slider)
            {
                if (log) { Debug.LogError($"There is missing components on {gameObject.name} (a canvas group or slider)"); }
                selfDestruct();
            }
            else 
            {
                manager.RegisterHitAction(DrawUI);
            }
            setInitial();
        }


        // Update is called once per frame
        void Update()
        {
            if (!healthBar) { return; }
            positionSlider();
            check();
            NoLifeLeft();
        }


        #endregion
    }
}