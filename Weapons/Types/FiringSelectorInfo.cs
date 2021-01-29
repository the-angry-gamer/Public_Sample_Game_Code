using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{
    [System.Serializable]
    internal class FiringSelectorInfo
    {
        [ Header( "Fire Selector Options " ) ]
        [ SerializeField ]
        [ Tooltip( "Determine what type of selector we want" ) ]
        internal FireType firingSelector    = FireType.Single;

        [SerializeField]
        [Tooltip( "Determine the rate of fire per second" ) ]
        [ Range( 0.1f, 50.0f ) ]
        internal   float   rateOfFire       = 1;

        [ SerializeField ]
        [ Tooltip( "Determine the rate of burst fire" ) ]
        [ Range( 4.0f, 20.0f ) ]
        internal   float   burstRate        = 1;

        public FiringSelectorInfo(float rOfFire, float bFireRate, FireType type)
        {
            rateOfFire      = rOfFire;
            burstRate       = bFireRate;
            firingSelector  = type;
        }

    }
}
