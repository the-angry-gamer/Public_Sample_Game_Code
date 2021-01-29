using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{
    /// <summary>
    ///     Handle the weapons input. Allows player management
    ///     of the weapon input. 
    /// </summary>
    public struct HandleInput
    {
        public bool wasDown;
        public bool isPressed;

        /// <summary>
        ///     This button was pressed this frame
        /// </summary>
        public void buttonIsPressed()
        {
            wasDown = !wasDown;
        }

        /// <summary>
        ///     Get whether the button was pressed during this frame.
        ///     Will reset it to false
        /// </summary>
        public bool GetButtonPressed
        {
            get
            {
                var temp =  wasDown;
                wasDown = false;
                return temp;
            }
        }
    }

}
