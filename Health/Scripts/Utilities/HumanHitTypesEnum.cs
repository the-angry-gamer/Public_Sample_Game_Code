using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaHealth
{

    /// <summary>
    ///     The type of human hit that the 
    ///     health item is associated with
    /// </summary>
    [System.Serializable]
    internal enum HumanHitTypes
    {
        Generic = 1,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight,
        Chest,
        Head = 7,
        Explosion
    }

}
