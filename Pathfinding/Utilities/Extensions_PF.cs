using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{
    public static class Extensions_PF
    {
        /// <summary>
        ///     Add a full array to this current array at the end of vector
        /// </summary>
        /// <typeparam name="T">        The type consisting of the array    </typeparam>
        /// <param name="ThisArray">    The array we are inserting the new values into  </param>
        /// <param name="copyArray">    The array the new values are coming from        </param>
        internal static T[] AddArray<T>(this T[] ThisArray, T[] copyArray)
        {
            T[] tempArray = new T[ ThisArray.Length + copyArray.Length];

            Array.Copy(sourceArray: ThisArray, destinationArray: tempArray, length: ThisArray.Length);

            Array.Copy(sourceArray: copyArray, sourceIndex: 0, destinationArray: tempArray, destinationIndex: ThisArray.Length, length: copyArray.Length);

            return tempArray;
        }


        /// <summary>
        ///     Clamp this integer between two values
        /// </summary>
        /// <param name="me">   The int we are checking     </param>
        /// <param name="low">  The lowest possible value   </param>
        /// <param name="high"> The highest possible value  </param>

        internal static int Clamp(this int me, int low, int high)
        {
            if ( me > high ) { me = high;   }
            if ( me < low  ) { me = low;    }
            return me;
        }

        /// <summary>
        ///     Add a value safely to a dictionary
        ///     If the key exists, override it
        /// </summary>
        /// <typeparam name="TKey">     </typeparam>
        /// <typeparam name="TValue">   </typeparam>
        /// <param name="dict">         this item               </param>
        /// <param name="key">          The key to check        </param>
        /// <param name="value">        The value to change/add </param>
        /// <param name="overWiteExisting"> determines if we want to overwrite the existing value associated with the key if it exists  </param>
        internal static void AddKeySafe<TKey,TValue> (this Dictionary<TKey,TValue> dict, TKey key, TValue value, bool overWiteExisting= true)
        {
            if ( !dict.ContainsKey( key ) )
            {
                dict.Add(key: key, value: value);
            }
            else
            {
                if (overWiteExisting)
                {
                    dict[key] = value;
                }
            }
        }

        /// <summary>
        ///     Add a value safely to a dictionary
        ///     If the key exists, override it
        /// </summary>
        /// <typeparam name="TKey">     </typeparam>
        /// <typeparam name="TValue">   </typeparam>
        /// <param name="dict">         this item               </param>
        /// <param name="key">          The key to check        </param>
        /// <param name="value">        The value to change/add </param>
        /// <param name="overWiteExisting"> determines if we want to overwrite the existing value   </param>
        internal static TValue SafeContains<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key )
        {
            if ( dict.ContainsKey( key ) )
            {
                return dict[key];
            }

            return default(TValue);
        }


    }

}
