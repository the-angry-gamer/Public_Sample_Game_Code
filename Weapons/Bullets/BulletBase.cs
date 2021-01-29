using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AsteriaGeneral;
using System;

namespace AsteriaWeapons
{
    [RequireComponent(typeof (CapsuleCollider))]
    public abstract class BulletBase : MonoBehaviour
    {
        #region Editor

        [SerializeField]
        internal BulletTypes        bulletType;

        [SerializeField]
        internal SoundsController   genericHitSound;

        [SerializeField]
        [Tooltip("Determine how much further this bullet can go than the base weapons range")]
        internal float              rangeMultiplier = 1.0f;
                
        //[SerializeField]
        //[Tooltip("What layers to make contact with")]
        LayerMask                   LayersToHit     = 1 << 0;

        Action<bool> onHit;

        #endregion


        #region Private Declarations

        /// <summary>  Delete when we made contact and the hit sound is playing  </summary>
        float   bulletSpeed     = 0.0f;
        float   bulletRange     = 0.0f;
        Vector3 initPos         = new Vector3();
        bool    bulletRay       = true;
        #endregion


        #region Properties

        /// <summary>
        ///     Returns the type of bullet that we are using
        /// </summary>
        /// <value> The bullet type enum    </value>
        public BulletTypes GetBulletType
        {
            get;
        }

        /// <summary>
        ///     The game object that created the bullet
        /// </summary>
        /// <value>The owner.</value>
        public GameObject Owner
        {
            get;
            private set;
        }

        /// <summary>
        ///     Alerts if we have fired the bullet
        /// </summary>
        /// <value><c>true</c> if fired; otherwise, <c>false</c>.</value>
        internal bool Fired
        {
            get;
            private set;
        }

        /// <summary>
        ///     The amount of damage this bullet is set to do
        /// </summary>
        /// <value>The do damage.</value>
        internal float DoDamage
        {
            get;
            private set;
        }

        #endregion


        #region Monobehavoir


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if ( Fired)
            {
                checkDistance();
                moveBullet();
            }   
        }

        #endregion


        #region Public 

        /// <summary>
        ///     Play the hit sound if we have one
        /// </summary>
        void playGenericHitSound()
        {
            if (genericHitSound.sound)
            {
                genericHitSound.PlaySound( position: transform.position );
            }
        }


        /// <summary>
        ///     Fire this bullet off
        /// </summary>
        /// <param name="firedBy">      The parent object who fired this                </param>
        /// <param name="doDamage">     The amount of damage to do                      </param>
        /// <param name="range">        The distace this bullet can travel before dying </param>
        /// <param name="speed">        The speed at which the bullet travels           </param>
        /// <param name="useRay">       Determines if we use a raycast or a bullet      </param>
        /// <param name="hitAction">    An action to complete on compact                </param>
        public void FireBullet(GameObject firedBy, float doDamage, float speed, float range, bool useRay, LayerMask layers, Action<bool> hitAction = null) 
        {
            Fired           = true;
            Owner           = firedBy;
            DoDamage        = doDamage;
            bulletSpeed     = speed;
            bulletRange     = range * rangeMultiplier;
            initPos         = firedBy.transform.position;
            bulletRay       = useRay;
            onHit           = hitAction;
            LayersToHit     = layers;
        }

        #endregion


        #region Utility

        /// <summary>
        ///     Create any actions we would want for the impact
        /// </summary>
        void createGenericImpactActions()
        {
            if ( onHit != null ) { onHit.Invoke(false); }

            playGenericHitSound();
        }

        /// <summary>
        ///     Move our bullet forwards
        /// </summary>
        void moveBullet()
        {
            transform.position = CheckForward(); 
            if (bulletRay) { Destroy(gameObject); }
        }
        

        /// <summary>
        ///     Check our bullet moving forward
        /// </summary>
        /// <returns>
        ///     The location of the hit
        /// </returns>
        Vector3 CheckForward()
        {
            var d = bulletRay ? bulletRange : (bulletSpeed * Time.deltaTime);

            RaycastController ray   = new RaycastController(start: transform.position, 
                    direction: transform.forward, distance: d, layerMasks: LayersToHit);
            
            Vector3 distace         = ray.StraightLineHit;

            CheckHit(ray);

            return distace;
        }

        /// <summary>
        ///     Check our hit... see if we hit something
        ///     and if we did if that something had a health manager
        ///     attached to it
        /// </summary>
        /// <param name="ray"></param>
        void CheckHit(RaycastController ray)
        {
            if ( !ray.FirstObjectHit )
            {
                Destroy(gameObject);
                return;         // we hit nothing
            }

            foreach (RaycastHit hit in ray.OrderedHits)
            {
                if (hit.transform.gameObject.GetComponent<IBulletInteract>() != null)
                {
                    var ibi = hit.transform.GetComponent<IBulletInteract>();
                    if (onHit != null) { onHit.Invoke(false); }

                    if ( ibi.OnBulletImpact( this ) )
                    {
                        break;
                    }
                }
                else if ( hit.transform.gameObject.GetComponent<AsteriaHealth.HealthManager>() != null)
                {
                    var healthManager = hit.transform.gameObject.GetComponent<AsteriaHealth.HealthManager>();
                    foreach (RaycastHit hits in ray.OrderedHits)
                    {
                        foreach (AsteriaHealth.HealthItem hi in healthManager.HealthItems)
                        {
                            if (hi.gameObject == hits.collider.gameObject) 
                            {
                                if (onHit != null) { onHit.Invoke(true); }

                                hi.TakeDamage(damage: DoDamage, origin: Owner, hitBy: gameObject);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    createGenericImpactActions();
                    break;
                }
            }
            Destroy(gameObject);

        }

        
        /// <summary>
        ///     Check if our bullet has fallen out range
        /// </summary>
        void checkDistance()
        {
            if (Vector3.Distance( initPos, gameObject.transform.position ) > bulletRange )
            {
                Destroy(gameObject);
            }
        }
        
        
        #endregion
    }
}
