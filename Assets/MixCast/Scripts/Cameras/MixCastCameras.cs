using System;
using System.Collections.Generic;
/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastCameras : MonoBehaviour
    {
        public static MixCastCameras Instance { get; protected set; }

        public CameraConfigContext cameraPrefab;

        public List<CameraConfigContext> CameraInstances { get; protected set; }

        private void OnEnable()
        {
            if ( Instance != null )
            {
                Debug.LogError("Should only have one MixCastCameras in the game");
                return;
            }

            
            Instance = this;

            MixCast.MixCastEnabled += HandleMixCastEnabled;
            MixCast.MixCastDisabled += HandleMixCastDisabled;

            GenerateCameras();
        }

        private void OnDisable()
        {
            DestroyCameras();

            MixCast.MixCastEnabled -= HandleMixCastEnabled;
            MixCast.MixCastDisabled -= HandleMixCastDisabled;

            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if( MixCast.Settings.cameras.Count != CameraInstances.Count )
            {
                DestroyCameras();
                GenerateCameras();
            }
        }

        void GenerateCameras()
        {
            CameraInstances = new List<CameraConfigContext>();

            bool wasPrefabActive = cameraPrefab.gameObject.activeSelf;
            cameraPrefab.gameObject.SetActive(false);
            for (int i = 0; i < MixCast.Settings.cameras.Count; i++)
            {
                CameraConfigContext instance = Instantiate(cameraPrefab, transform, false);

                instance.Data = MixCast.Settings.cameras[i];

                CameraInstances.Add(instance);
            }
            cameraPrefab.gameObject.SetActive(wasPrefabActive);

            SetCamerasActive(MixCast.Active);
        }
        void DestroyCameras()
        {
            for (int i = 0; i < CameraInstances.Count; i++)
                Destroy(CameraInstances[i].gameObject);

            CameraInstances.Clear();
            CameraInstances = null;
        }
        private void HandleMixCastEnabled()
        {
            SetCamerasActive(true);
        }
        private void HandleMixCastDisabled()
        {
            SetCamerasActive(false);
        }

        void SetCamerasActive(bool active)
        {
            if (CameraInstances == null)
                return;

            for (int i = 0; i < CameraInstances.Count; i++)
                CameraInstances[i].gameObject.SetActive(active);
        }
    }
}