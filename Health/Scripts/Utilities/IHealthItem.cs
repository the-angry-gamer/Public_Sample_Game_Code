using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaHealth
{

    public interface IHealthItem 
    {
        float TakeDamage(float damage, GameObject origin, GameObject hitBy);
    }

}