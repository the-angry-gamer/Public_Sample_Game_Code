using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaWeapons
{
    public interface IBulletInteract
    {

        bool OnBulletImpact(BulletBase bullet);

    }

}