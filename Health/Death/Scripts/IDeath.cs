using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaHealth
{
    public interface IDeath
    {
        /// <summary>
        ///     Kill the object this is attached to
        /// </summary>
        /// <param name="hitType">  The type of (enum) that the object experienced to kill it   </param>
        void isDead(HealthItem item);
    }
}
