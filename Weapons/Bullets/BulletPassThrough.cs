using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{

    public class BulletPassThrough : MonoBehaviour, IBulletInteract
    {

        public bool OnBulletImpact(BulletBase bullet)
        {
            Debug.LogError("Passed through this object");
            return false;
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
