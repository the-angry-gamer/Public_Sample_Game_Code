using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AsteriaDialogue
{

    public static class DialogueUtilities
    {
        /// <summary>
        ///     Make a generic texture of a color
        /// </summary>
        /// <returns>The tex.</returns>
        /// <param name="color">    The color the background will be    </param>
        /// <param name="width">    The width of the texture            </param>
        /// <param name="height">   The height of the texture           </param>
        public static Texture2D MakeTex(this Color color, int width, int height)
        {
            Color[] pix = new Color[ width * height ];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

    }


}