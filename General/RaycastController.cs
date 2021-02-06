using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AsteriaGeneral
{
    public class RaycastController 
    {

        Vector3         _start;        
        float           _distance;
        Vector3         _direction;
        RaycastHit[]    hits;

        //GameObject      closestHit;

        internal Vector3 Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        ///     Get all the hits the raycast found
        ///     in order
        /// </summary>
        public RaycastHit[] OrderedHits
       {
            get
            {
                return hits;
            }
        }

        /// <summary>
        ///     The closest hit that we found. This could be the 
        ///     parent of a collider that was hit. To get the collider
        ///     use the first collider hit
        /// </summary>
        public GameObject FirstObjectHit
        {
            get
            {
                if (hits.Count() > 0)
                {
                    return hits[0].transform.gameObject;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        ///     Get the closest hits distance from the object 
        /// </summary>
        internal float ClosestHitDistance
        {
            get
            {
                if (hits.Count() > 0)
                {
                    return hits[0].distance;
                }
                return _distance;
            }

        }

        /// <summary>
        ///     Get the collider of the object that we 
        ///     hit to see if we want to do damage to it
        /// </summary>
        public GameObject FirstColliderHit
        {
            get
            {
                if (hits.Count() > 0)
                {
                    return hits[0].collider.gameObject;
                }
                else
                {
                    return null;
                }
            }       
        }

        /// <summary>
        ///     How many total hits we have
        /// </summary>
        public int AmountOfHits
        {
            get
            {
                return hits.Length;
            }
        }

        /// <summary>
        ///     Get the closest Vector hit from our raycast
        /// </summary>
        public Vector3 StraightLineHit
        {
            get
            {
                var endPos = _start + ( _direction * ClosestHitDistance );

                return endPos;
            }
        }

        /// <summary>
        ///     Get the closest hit adjusted by a a fraction closer
        /// </summary>
        public Vector3 StraightLineHitAdjusted
        {
            get
            {
                var endPos = _start + (_direction * (ClosestHitDistance + .1f));

                return endPos;
            }
        }


        /// <summary>
        ///     Get the furthest distance we can go
        /// </summary>
        internal Vector3 FurthestDistance
        {
            get
            {
                return _start + (_direction * _distance);
            }
        }
        /// <summary>
        ///     Create a raycast and store the results in an
        ///     easy to use controller
        /// </summary>
        /// <param name="start">        The start position of the ray       </param>
        /// <param name="direction">    The direction the ray is pointed    </param>
        /// <param name="distance">     The distance to shoot ray           </param>
        /// <param name="layerMasks">   The layer masks to cull out</param>
        public RaycastController( Vector3 start,  Vector3 direction, float distance, int layerMasks)
        {
            _start      = start;
            _distance   = distance;
            _direction  = direction;

            Ray ray     = new Ray(start, direction);
            hits        = Physics.RaycastAll(ray: ray, maxDistance: distance, layerMask: layerMasks);

            sortHits();
        }

        /// <summary>
        ///     Draw this ray in the editor
        /// </summary>
        /// <param name="c"></param>
        public void DrawRay(Color c)
        {
            Debug.DrawLine(start: Start, end: StraightLineHit, c);
        }

        /// <summary>
        ///     Draw the ray in the editor with a black line
        /// </summary>
        public void DrawRay()
        {
            DrawRay(Color.black);
        }

        /// <summary>
        ///     Sort all of our hits in the order that they occured
        /// </summary>
        void sortHits()
        {
            int n = hits.Count();

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - 1; j++)
                {
                    if (hits[j].distance > hits[j + 1].distance)
                    {
                        var temp    = hits[j];
                        hits[j]     = hits[j + 1];
                        hits[j + 1] = temp;
                    }
                }
            }
        }

    }
}
