using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaDialogue
{
    public class ResponseTypes
    {
        /// <summary>
        ///     The type of response that will drive the conversation
        /// </summary>
        public enum TypesOfResponse
        {
            /// <summary> Will move the conversation forward            </summary>
            Forward,
            /// <summary> Will complete registered accept actions       </summary>
            Accept,
            /// <summary> Will complete registered decline actions      </summary>
            Decline,
            /// <summary> Used if having a single selectable response is desired </summary>
            SingleResponse
        }

    }
}