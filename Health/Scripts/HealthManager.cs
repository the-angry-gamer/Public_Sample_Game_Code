using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaHealth
{
    public class HealthManager : MonoBehaviour
    {

        #region Editor
        [SerializeField]
        [Tooltip("Assign this manager automatically to each child")]
        bool ManageChildren = true;

        [SerializeField]
        [Tooltip("Determines if this item is invincible")]
        bool Invincible = false;

        [SerializeField]
        [Tooltip("The amount of health that we have")]
        float totalHealth;

        [SerializeField]
        [Tooltip("All the items in our children that has the heath item")]
        List<HealthItem> healthItems;

        #endregion


        #region Properties


        public List<HealthItem> HealthItems
        {
            get
            {
                return healthItems;
            }
        }

        /// <summary>
        ///     Determines if we have no life left
        /// </summary>
        public bool AmIDead
        {
            get
            {
                if (CurrentHealth < 0.01f)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     How much health we have left
        /// </summary>
        /// <value>The health left.</value>
        public float CurrentHealth
        {
            get;
            private set;
        }

        /// <summary>
        ///     Returns our total health of this class
        /// </summary>
        /// <value>The total health.</value>
        public float TotalHealth
        {
            get
            {
                return totalHealth;
            }
        }

        /// <summary>
        ///     Get the last game object that inflicted damage
        ///     unto me
        /// </summary>
        /// <value>The last object hit from.</value>
        public GameObject LastObjectHitFrom
        {
            get
            {
                if (_lastHealthItem && _lastHealthItem.OriginItem)
                {
                    return _lastHealthItem.OriginItem;
                }
                return null;

            }
        }

        /// <summary>
        ///     The name of the last object that hit this
        /// </summary>
        /// <value>The last name of the object hit.</value>
        public string LastObjectHitName
        {
            get
            {
                if (_lastHealthItem && _lastHealthItem.OriginItem)
                {
                    return _lastHealthItem.OriginItem.name;
                }
               
                return "Nothing";
                
            }
        }

        /// <summary>
        ///     The amount of damage the last item did
        /// </summary>
        /// <value>The last hit.</value>
        public float LastHitDamage
        {
            get;
            private set;
        }

        /// <summary>
        ///     Where we were last hit
        /// </summary>
        /// <value>The last type of the hit.</value>
        public string LastHitTypeString
        {
            get
            {
                return _lastHealthItem?.hitType.ToString() ?? "None";
            }
        }

        /// <summary>
        ///     The last hit type that we encountered
        /// </summary>
        public int LastHitType
        {
            get
            {
                if (_lastHealthItem !=null)
                {
                    return (int)_lastHealthItem?.hitType;
                }
                return 0;
            }
        }

        /// <summary>
        ///     The item that was send by the originating item
        ///     to do the damage
        /// </summary>
        /// <value>The connecting item.</value>
        public GameObject ConnectingItem
        {
            get
            {
                if (_lastHealthItem)
                {
                    return _lastHealthItem.HitBy;
                }
                return null;
            }
        }

        public string ConnectingItemName
        {
            get
            {
                if (_lastHealthItem && _lastHealthItem.HitBy)
                {
                    return _lastHealthItem.HitBy.name;
                }
                return "Nothing";
            }
        }

        /// <summary>
        ///     Get the type of impact that last caused my damage
        /// </summary>
        /// <value>The type of the impact.</value>
        public string ImpactType
        {
            get
            {
                var temp = string.Empty;

                if(_lastHealthItem)
                {

                    var test = _lastHealthItem.HitBy.GetComponent<AsteriaWeapons.BulletBase>();

                    if (test)
                    {
                        temp = test.GetBulletType.ToString();
                    }
                }
                return temp;
            }
        }

        #endregion


        #region Private

        private     List<Action>    HitActions = new List<Action>();
        private     List<Action>    RevActions = new List<Action>();
        private     HealthItem      _lastHealthItem;
        private     bool            killed;

        IDeath death;

        #endregion

        #region Monobehavoir


        // Start is called before the first frame update
        void Start()
        {
            AssignMe(gameObject);
            getDeathType();
            CurrentHealth = totalHealth;
        }


        // Update is called once per frame
        void Update()
        {
            if ( !killed && CurrentHealth <= 0.01f)
            {
                killMe();
            }
        }

        #endregion
       
        
        #region Internal Use

        /// <summary>
        ///     Kill this object. no mercy
        /// </summary>
        void killMe()
        {
            if (death != null)
            {
                death.isDead(_lastHealthItem);
            }
            else
            {
                Destroy(gameObject);        
                Debug.LogError($"There is no death interface on this game object {gameObject.name}");
            }
            killed = true;
        }

        /// <summary>
        ///     Go through children and assign the health manager
        ///     to any take health item.
        ///     This is a recusive function to get all children
        /// </summary>
        void AssignMe(GameObject go)
        {
            if (!ManageChildren) { return; }

            for(int i = 0; i < go.transform.childCount; i++)
            {
                var child       = go.transform.GetChild(i);

                AssignMe(child.gameObject);
            }
            var healthItem  = go.GetComponent<HealthItem>();
            if (healthItem != null)
            {
                healthItems.Add(healthItem);
                healthItem.manager = this;
            }
        }

        /// <summary>
        ///     Obtain he death actions on this character
        /// </summary>
        void getDeathType()
        {
            death = GetComponent<IDeath>();
        }

        #endregion


        #region External Use


        #region Actions

        
        /// <summary>
        ///     Register an action to occur when this
        ///     game object gains health
        /// </summary>
        /// <param name="a"></param>
        public void RegisterRevAction(Action a)
        {
            if (!RevActions.Contains(a))
            {
                RevActions.Add(a);
            }
        }

        /// <summary>
        ///     Remove any actions the game object will run through
        ///     when gaining health
        /// </summary>
        /// <param name="a"></param>
        public void UnRegisterRevAction(Action a)
        {
            if (RevActions.Contains(a))
            {
                RevActions.Remove(a);
            }
        }

        /// <summary>
        ///     Register an action to occur when this
        ///     game object takes damage
        /// </summary>
        /// <param name="a"></param>
        public void RegisterHitAction( Action a)
        {
            if (!HitActions.Contains(a))
            {
                HitActions.Add(a);
            }
        }

        /// <summary>
        ///     Remove any actions the game object will run through
        ///     when taking damage
        /// </summary>
        /// <param name="a"></param>
        public void UnRegisterHitAction( Action a)
        {
            if (HitActions.Contains(a))
            {
                HitActions.Remove(a);
            }
        }

        #endregion


        /// <summary>
        ///     Alter our total health 
        ///     with the amount of damage to do. A negetive
        ///     number will do negetive damager, so plus health
        /// </summary>
        /// <param name="amount">Amount.</param>
        public void alterHealth(float amount, HealthItem item)
        {
            if (CurrentHealth <= 0.0f) { return; }            

            if (!Invincible)
            {
                amount          = amount * -1;
                LastHitDamage   = amount;
                CurrentHealth   = Mathf.Clamp(value: CurrentHealth + amount, min: 0, max: totalHealth);
            }

            // Objects
            _lastHealthItem  = item;

            DoHitActions(amount);
        }

        /// <summary>
        ///     Revive the character    
        /// </summary>
        /// <param name="amount">   The amount of health to revive with</param>
        public void Revive(float amount)
        {
            if (amount < 0) { return; }
            CurrentHealth = Mathf.Clamp(value: CurrentHealth + amount, min: 0, max: totalHealth);
            if (CurrentHealth > 0.1f) { killed = false; }
        }

        /// <summary>
        ///     Complete any required actions when we take damage
        /// </summary>
        /// <param name="damage">   How much damage or health was gained    </param>
        void DoHitActions(float damage)
        {
            if (damage < 0)
            {
                foreach (Action a in HitActions)
                {
                    a.Invoke();
                }
            }
            else if (damage > 0)
            {
                foreach (Action a in RevActions)
                {
                    a.Invoke();
                }
            }

        }

        #endregion
    }
}