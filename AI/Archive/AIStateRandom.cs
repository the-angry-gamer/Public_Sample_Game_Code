//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace AI
//{

//    public class AIStateRandom : AIState
//    {
//        // Start is called before the first frame update
//        void Start()
//        {
        
//        }

//        // Update is called once per frame
//        void Update()
//        {
        
//        }

//        /// <summary>
//        ///     Determine if we need to find a new path
//        /// </summary>
//        /// <returns>
//        ///     A boolean if we want to determine a new path
//        /// </returns>
//        protected override bool DetermineNewPath()
//        {
//            if (Path.findNewPath) { return true; } // we already want a new path

//            // timer first
//            // if we want to keep searching options
//            bool determine = false;
            
//            // Determine over time if we want to change trajectory
//            if ( UseTimer && (Time.time - Path.lastChecked) > timer)
//            {
//                Path.findNewPath    = true;
//                determine           = true;
//            }

//            // Distance check - this should be last
//            if ( !determine && Vector3.Distance( gameObject.transform.position, Path.PO.EndPosition ) < distance )
//            {
//                Path.findNewPath    = true;
//            }

//            return base.DetermineNewPath();            
//        }
//    }
//}
