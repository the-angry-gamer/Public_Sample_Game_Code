using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaGeneral
{

    [System.Serializable]
    public class UsageBar
    {

        #region Editor

        [SerializeField]
        [Tooltip("How much we can use before running out")]
        float total         = 10.0f;

        [SerializeField]
        [Tooltip("How much over time it is going to cost to use")]
        float usageCost     = 1.0f;

        [SerializeField]
        [Tooltip("How fast we reset this value")]
        float resetRate     = 1.0f;

        [SerializeField]
        [Tooltip("How long to wait before reloading")]
        float resetTime     = 1.0f;


        #endregion

        #region Properties

        /// <summary> The current amount of usage we have   </summary>
        public float Current    { get; private set; }    
        /// <summary> The total amount of usage we have     </summary>
        public float Total      { get { return total; } }


        /// <summary>
        ///     If we have any usage left
        /// </summary>
        public bool AnyLeft
        {
            get
            {
                if ( Current < 0.01f)
                {
                    return false;
                }
                return true;
            }
        }


        #endregion

        #region Private Declarations

        float lastUsed;

        #endregion


        public UsageBar()
        {
            Current = total;
        }

        /// <summary>
        ///     Calculate if we want to keep using it or not
        /// </summary>
        /// <param name="useIt"></param>
        public void Calculate(bool useIt)
        {
            if (useIt) { Use(); }
            else { tryReset(); }

        }


        void Use()
        {
            Current -= (usageCost * Time.deltaTime);
            lastUsed = Time.time;

            if (Current < 0) { Current = 0.0f; }
        }


        void tryReset()
        {
            if ( lastUsed < Time.time - resetTime )
            {
                if (Current > total) { Current = total; }
                else
                {
                    Current += (resetRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        ///     Add extra to what we are using
        /// </summary>
        /// <param name="amount"></param>
        public void AddExtra(float amount)
        {
            Current = Mathf.Clamp(value: amount + Current, min: 0, max: total);
        }



    }

}