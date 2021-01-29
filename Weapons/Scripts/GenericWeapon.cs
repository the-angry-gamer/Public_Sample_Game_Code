using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{

    public class GenericWeapon : WeaponBase
    {
        #region Editor


        [Header("Gun Specific")]
        [SerializeField]
        [Tooltip("Draw a debug ray where the gun is aiming in the editor window")]
        bool drawAiming = true;

        #endregion

        protected override void instantiateBullet()
        {
            if ( !bullet )
            {
                Debug.LogError($"There is no bullet for {gameObject.name} defined.");
                return;
            }

            var bull    = Instantiate(original: bullet, position: muzzleArea.transform.position, rotation: muzzleArea.transform.rotation, parent: BulletParent.transform) ;
            bull.name   = $"{gameObject.name}_{currentClip}_{currentAmmo}";

            // Fire the bullet
            bull.FireBullet( firedBy: Owner, doDamage: Random.Range( damageRange.x, damageRange.y ), 
                        speed: bulletSpeed, range: bulletRange, useRay: useRay, layers: LayersToHit, hitAction: OnBulletCollision );

            base.instantiateBullet();            
        }

        /// <summary>
        ///     Do this while aiming
        /// </summary>
        protected override void doWhileAiming()
        {
            if (drawAiming)
            {
                Vector3 start   = muzzleArea.transform.position;
                Vector3 end     = start + ( muzzleArea.transform.forward * bulletRange );

                Debug.DrawLine(start, end, Color.red);
            }
            base.doWhileAiming();
        }

    }
}
