using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Human_Controller
{

    public static class ExtensionMethods
    {

        #region Floats

        /// <summary>
        ///     Will determine if our value exceeds the maximum
        /// </summary>
        /// <param name="value">    Our floatation value        </param>
        /// <param name="checkMax"> The max avlue we can have   </param>
        /// <returns>
        ///     If the value exceeds the max, will return max
        ///     else it will return the good value
        /// </returns>
        public static float CheckMaximum(this float value, float checkMax)
        {
            if (value > checkMax)
            {
                return checkMax;
            }
            return value;
        }


        /// <summary>
        ///     Check the maximum absolute values 
        /// </summary>
        /// <param name="value">    The float to pass in    </param>
        /// <param name="checkMax"> The value to set as max </param>
        /// <returns>
        ///     If the value exceeds the max, will return max
        ///     else it will return the good value
        /// </returns>
        /// <remarks> Good for locking angles in    </remarks>
        public static float CheckMaximumAbsoluteValue(this float value, float checkMax)
        {

            int sign = value > 0 ? 1 : -1;

            if (Math.Abs(value) > Math.Abs(checkMax))
            {
                return checkMax * sign;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        ///     Check if this is between two different values
        /// </summary>
        /// <param name="value">    The float value calling </param>
        /// <param name="min">      The minimum value       </param>
        /// <param name="max">      The maximum value       </param>
        /// <returns>
        ///     A bool if we lie between the values
        /// </returns>
        public static bool checkIfBetween(this float value, float min, float max)
        {
            if (value >= min && value <= max)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion


        #region From Invictus

        public static float ClampAngle(float angle, float min, float max)
        {
            do
            {
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
            } while (angle < -360 || angle > 360);

            return Mathf.Clamp(angle, min, max);
        }






        public struct ClipPlanePoints
        {
            public Vector3 UpperLeft;
            public Vector3 UpperRight;
            public Vector3 LowerLeft;
            public Vector3 LowerRight;
        }

        public static ClipPlanePoints NearClipPlanePoints_Asteria(this Camera camera, Vector3 pos, float clipPlaneMargin)
        {
            var clipPlanePoints = new ClipPlanePoints();

            var transform = camera.transform;
            var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
            var aspect = camera.aspect;
            var distance = camera.nearClipPlane;
            var height = distance * Mathf.Tan(halfFOV);
            var width = height * aspect;
            height *= 1 + clipPlaneMargin;
            width *= 1 + clipPlaneMargin;
            clipPlanePoints.LowerRight = pos + transform.right * width;
            clipPlanePoints.LowerRight -= transform.up * height;
            clipPlanePoints.LowerRight += transform.forward * distance;

            clipPlanePoints.LowerLeft = pos - transform.right * width;
            clipPlanePoints.LowerLeft -= transform.up * height;
            clipPlanePoints.LowerLeft += transform.forward * distance;

            clipPlanePoints.UpperRight = pos + transform.right * width;
            clipPlanePoints.UpperRight += transform.up * height;
            clipPlanePoints.UpperRight += transform.forward * distance;

            clipPlanePoints.UpperLeft = pos - transform.right * width;
            clipPlanePoints.UpperLeft += transform.up * height;
            clipPlanePoints.UpperLeft += transform.forward * distance;

            return clipPlanePoints;
        }
        #endregion

    }
}