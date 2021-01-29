using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Human_Controller
{

    public static class UtilityClass
    {

        private static int PlayerLayer = LayerMask.NameToLayer( "Player" );

        /// <summary>
        ///     Will check to see if a raycast hit something other than the player
        ///     that is solid
        /// </summary>
        /// <param name="hit">  The individual hit of the raycast   </param>
        /// <returns>
        ///     A bool of true or false
        /// </returns>
        internal static bool CheckSolidRayCastHit(RaycastHit[] hits,bool includePlayer = false)
        {
            foreach (RaycastHit hit in hits)
            {
                var item = hit.collider;
                if ( item )
                {
                    if ( !includePlayer && hit.transform.gameObject.layer == PlayerLayer)
                    {
                        // skip this here
                    }
                    else if ( item.GetType() == typeof( MeshCollider ) )
                    {
                        return true;
                    }
                    else if (!item.isTrigger)
                    {
                        return true;
                    }
                }

            }
            return false;
        }

    }
}
