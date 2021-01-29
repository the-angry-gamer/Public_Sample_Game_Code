using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaGeneral
{
    [Serializable]
    public class SoundsController
    {
        [ SerializeField ]
        [ Tooltip( "The sound to play at the given location" ) ]
        internal AudioClip  sound;

        [ SerializeField]
        [ Tooltip( "The percent volume to play the sound at" ) ]
        [ Range( 0, 1.0f ) ]
        internal float      volume;

        /// <summary>
        ///     Play the specified sound at a specified volume
        /// </summary>
        /// <param name="position"> The position to play the sound at   </param>
        internal void PlaySound(Vector3 position)
        {
            AudioSource.PlayClipAtPoint(sound, position: position, volume);
        }

    }

}
