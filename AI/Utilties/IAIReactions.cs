using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Asteria
{

    public interface IAIReactions
    {
        /// <summary>
        ///     Determines if this enemy can be fired upon
        /// </summary>
        bool CanShoot { get; }
       
        void IsBeingAimedAt();

        void IsBeingHit();

        void IsTouchingFire();

    }

}