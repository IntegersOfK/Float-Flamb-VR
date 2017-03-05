/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    [System.Serializable]
    public class MixCastData
    {
        public enum CameraMode
        {
            Immediate, Buffered, Quadrant
        }

        [System.Serializable]
        public class CameraCalibrationData
        {
            public string deviceName;

            public float deviceFoV;

            public Vector3 worldPosition;
            public Quaternion worldRotation;

            public bool wasTracked;
            public Vector3 trackedPosition;
            public Quaternion trackedRotation;


            public CameraMode cameraMode = CameraMode.Immediate;
            public ChromakeyCalibrationData chromakeying = new ChromakeyCalibrationData();
        }

        [System.Serializable]
        public class ChromakeyCalibrationData
        {
            public bool active;
            public Vector3 keyHsvMid, keyHsvRange, keyHsvFeathering;
        }

        public List<CameraCalibrationData> cameras = new List<CameraCalibrationData>();
    }
}