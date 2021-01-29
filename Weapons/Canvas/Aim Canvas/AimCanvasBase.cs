using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsteriaWeapons
{

    [System.Serializable]
    public struct AimArea
    {
        [SerializeField]
        Sprite sprite;
        [SerializeField]
        float startOffset;
        [SerializeField]
        float maxOffset;
        [SerializeField]
        [Range(1,100)]
        float increment;
        [SerializeField]
        [Range(0.001f, 2)]
        float scale;

        float currentOffset;

        public Sprite   CanvasImage     { get { return sprite; } }
        public float    StartOffset     { get { return startOffset; } }
        public float    MaxOffset       { get { return maxOffset; } }
        public float    CurrentOffset   { get { return currentOffset; } }
        public float    Scale           { get { return scale; } }


        public void AddOffset(float offset, bool neg)
        {
            var temp    = currentOffset + (offset * increment);
            temp        = Mathf.Clamp(value: Mathf.Abs( temp ), min: startOffset, max: maxOffset);

            if (neg) { temp = temp * -1; }

            currentOffset = temp;
        }

        public void ResetSpread(float time, bool neg)
        {
            var temp    = Mathf.Lerp(a: Mathf.Abs(currentOffset), b: startOffset, t: time);
            temp        = Mathf.Clamp(value: temp, min: startOffset, max: maxOffset);
            if (neg) { temp = temp * -1; }

            currentOffset = temp;
        }
    }

    [System.Serializable]
    public class AimCanvasBase
    {

        #region Editor
        [SerializeField]
        AimArea left;

        [SerializeField]
        AimArea right;

        [SerializeField]
        AimArea top;

        [SerializeField]
        AimArea bottom;

        [SerializeField]
        AimArea center;
        #endregion


        #region Properties

        public AimArea Left     { get { return left;        } }
        public AimArea Right    { get { return right;       } }
        public AimArea Top      { get { return top;         } }
        public AimArea Bottom   { get { return bottom;      } }
        public AimArea Center   { get { return center;      } }
     
        #endregion


        #region External Classes


        /// <summary>
        ///     Update the spread of the 
        ///     aiming canvas
        /// </summary>
        internal void UpdateSpreads(Vector2 offsets)
        {
            left.AddOffset(     offset: -offsets.x, true   );
            right.AddOffset(    offset: offsets.x,  false  );

            top.AddOffset(      offset: offsets.y, true    );
            bottom.AddOffset(   offset: -offsets.y, false  );
        }

        internal void ResetSpreads(float time)
        {
            left.ResetSpread(   time, true  );
            right.ResetSpread(  time, false );

            top.ResetSpread(    time, false );
            bottom.ResetSpread( time, true  );
        }

        #endregion

    }
}
