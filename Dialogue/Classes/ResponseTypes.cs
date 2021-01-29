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
            Forward,
            Accept,
            Decline,
            SingleResponse
        }
    }
}