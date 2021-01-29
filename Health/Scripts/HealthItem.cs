using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaHealth
{

    public abstract class HealthItem : MonoBehaviour, IHealthItem
    {
        [SerializeField]
        [Tooltip("This can be assigned manually or automatically. If left blank, the manager at the parent level will assign it")]
        internal HealthManager  manager;

        [SerializeField]
        [Tooltip("Where this would be associated with")]
        internal HumanHitTypes  hitType = HumanHitTypes.Generic;

        [SerializeField]
        [Tooltip("How much of a multiplier this has item has on the damage recieved")]
        [Range(0,100)]
        protected float         multiplier = 1.0f;

        [SerializeField]
        [Tooltip("The sound that the item makes when it connects")]
        protected AudioSource   genericImpactSound;

        [SerializeField]
        [Tooltip("Specific Type impact sound overrides")]
        protected List<BulletImpactSounds> impactSounds;

        internal GameObject     OriginItem;
        internal GameObject     HitBy;
        internal Vector3        HitPosition;


        void Start()
        {

        }

        public      virtual  float  TakeDamage(float damage, GameObject origin, GameObject hitBy)
        {
            OriginItem  = origin;
            HitBy       = hitBy;
            HitPosition = hitBy.transform.position; return 0.0f; 
        }

        protected   abstract void   PlayImpactSound();

    }
}