using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;



namespace PathFindingAsteria
{ 

    /// <summary>
    ///     All the re-usable functions we can use in the namespace
    /// </summary>
    public static class Utilities_PF
    {

        /// <summary>
        ///     Check if an integer layer matches the 
        ///     layer mask we are trying to match it to
        /// </summary>
        /// <param name="layer">            The gameobjects layer   </param>
        /// <param name="compareAgainst">   The layer mask to check </param>
        /// <returns>
        ///     A bool if the layers match or not
        /// </returns>
        internal static bool IsLayerMatch(int layer, LayerMask compareAgainst)
        {
            return ( (compareAgainst.value & (1 << layer)) > 0);
        }

        /// <summary>
        ///     Create a full copy of a serialized item.
        ///     This copies the class at the binary level.
        /// </summary>
        /// <typeparam name="T">    This is the type    </typeparam>
        /// <param name="other">    The item to copy    </param>
        /// <returns>
        ///     An item copied from the memory location of the
        ///     original item
        /// </returns>
        internal static T DeepCopy<T>(T other)
        {            
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, other);
                    ms.Position = 0;
                    return (T)formatter.Deserialize(ms);
                }
            }
            catch { return other; }
        }

    }
}

