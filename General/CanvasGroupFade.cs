using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaGeneral
{

    [System.Serializable]
    public class CanvasGroupFade
    {
        #region Editor

        [SerializeField]
        [Tooltip("The rate to fade away at")]
        [Range(0, 20)]
        float                   fadeScale = 2.0f;

        [SerializeField]
        [Tooltip("If not a constant bar, how long before we start to fade away")]
        float                   timeToKeep = 1.0f;


        #endregion


        #region Private

        float           lastDisplay;
        
        float           alpha       = 1.0f;
        float           duration    = 0.0f;

        CanvasGroup      CanGroupObject;


        #endregion

        #region Properties

        /// <summary>
        ///     Determines if we are active in the 
        ///     scene
        /// </summary>
        public bool isActive
        {
            get
            {
                return CanGroupObject.gameObject.activeInHierarchy;
            }
        }

        #endregion

        #region Public Calls


        public CanvasGroupFade ( CanvasGroup g)
        {
            CanGroupObject = g;
        }


        /// <summary>
        ///     Set the fade back to 0% transparency
        /// </summary>
        public void draw()
        {
            if (!isActive) { CanGroupObject.gameObject.SetActive(true); }

            alpha                   = 1.0f;
            CanGroupObject.alpha    = alpha;
            lastDisplay             = Time.time;
        }

        /// <summary>
        ///     Incementally fadeaway this object
        /// </summary>
        public void fadeAway()
        {
            if (!isActive) return;

            if ( lastDisplay < Time.time - timeToKeep)
            {
                duration = Time.deltaTime * fadeScale;
                alpha       = Mathf.Lerp(alpha, 0, duration);
                CanGroupObject.alpha    = alpha;
            
                if ( alpha < .1f )
                {               
                    if (isActive) { CanGroupObject.gameObject.SetActive(false); }
                
                    alpha       = 1.0f;
                    duration    = 0.0f;
                    CanGroupObject.alpha    = alpha;                
                }
            }
        }

        /// <summary>
        ///     Turn on this object
        /// </summary>
        public void Activate()
        {
            if ( !isActive ) 
            { 
                CanGroupObject.gameObject.SetActive(true); 
            }
        }

        /// <summary>
        ///     Turn off this object
        /// </summary>
        public void Deactivate()
        {
            if (isActive)
            {
                CanGroupObject.gameObject.SetActive(false);
            }
        }

        #endregion
    }

}