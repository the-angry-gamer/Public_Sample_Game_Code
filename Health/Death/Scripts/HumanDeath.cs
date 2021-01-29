using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AsteriaHealth
{
    [RequireComponent(typeof(Human_Controller.HumanController))]
    public class HumanDeath : MonoBehaviour, IDeath
    {

        Human_Controller.HumanController human;

        void Start()
        {
            human = GetComponent<Human_Controller.HumanController>();
            human.Init();
        }

        public void isDead(HealthItem hitType)
        {
            if (human)
            {
                human.KillMe( ( int )hitType.hitType );
                human.updateAnimator();
            }
        }
    }
}
