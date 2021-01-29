using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_Asteria
{
    public static class Extensions_AI
    {
        internal static bool isEven(this int i)
        {
            if (i%2 > 0)
            {
                return false;
            }
            return true;


        }
    }
}