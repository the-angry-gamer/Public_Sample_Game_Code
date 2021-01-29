using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaHealth
{

    [RequireComponent( typeof( Collider ) )]
    public class HealthInteract : HealthItem
    {

        /// <summary>
        ///     Takes damage associated with a collison hit.
        ///     This would be called from something that is 
        ///     causing the damage
        /// </summary>
        /// <returns>The damage.</returns>
        /// <param name="damage">   That damage to do                           </param>
        /// <param name="origin">   What gameobject created the collison item   </param>
        /// <param name="hitBy">    What gameobject is doing the damage         </param>
        public override float TakeDamage(float damage, GameObject origin, GameObject hitBy)
        {

            base.TakeDamage(damage, origin, hitBy);
            
            PlayImpactSound();            
            manager.alterHealth(amount: damage * multiplier, item: this);

            return manager.CurrentHealth;
        }


        /// <summary>
        ///     Plays the sound of the impact
        /// </summary>
        protected override void PlayImpactSound()
        {
            if ( impactSounds.Count > 0)
            {
                var test = HitBy.GetComponent<AsteriaWeapons.BulletBase>();
                if ( test )
                {
                    foreach(BulletImpactSounds sound in impactSounds)
                    {
                        if (sound.bullet == test.bulletType)
                        {
                            sound.impactSound.Play();
                            return;
                        }
                    }
                }
            }

            if (genericImpactSound)
            {
                genericImpactSound.Play();
            }

        }

    }
}
