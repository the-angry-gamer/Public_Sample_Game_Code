using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Human_Controller
{

    [System.Serializable]
    public class CameraOffSets
    {
        [SerializeField] internal float Distance               = 1.2f;
        [SerializeField] internal float HorizontalOffset       = 0.0f;
        [SerializeField] internal float HeightOffset           = 1.6f;
        [SerializeField] internal float SmoothFollow           = 10f;
        [SerializeField] internal float offSetPlayerPivotY     = 0.0f;
        [SerializeField] internal float offSetPlayerPivotX     = 0.0f;

        [Range(0f,1f)]
        [SerializeField] internal float OffsetLerp             = 0.5f;

        // min / max x and y
        float max_x = 360f;
        float min_x = 360f;
    }

}
