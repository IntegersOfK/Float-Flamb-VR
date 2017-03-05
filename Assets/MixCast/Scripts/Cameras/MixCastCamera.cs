/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastCamera : CameraComponent
    {
        public static List<MixCastCamera> ActiveCameras { get; protected set; }
        public static event System.Action<MixCastCamera> GameRenderStarted;
        public static event System.Action<MixCastCamera> GameRenderEnded;

        static MixCastCamera()
        {
            ActiveCameras = new List<MixCastCamera>();
        }

        public Transform rootTransform;
        public Camera gameCamera;

        public Texture Output { get; protected set; }

        private SteamVR_ControllerManager controllerManager;
        private SteamVR_TrackedObject trackedController;


        protected override void OnEnable()
        {
            gameCamera.stereoTargetEye = StereoTargetEyeMask.None;
            controllerManager = Camera.main != null ? Camera.main.GetComponentInParent<SteamVR_ControllerManager>() : null;
            ActiveCameras.Add(this);
            base.OnEnable();
            HandleDataChanged();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ActiveCameras.Remove(this);
        }

        protected override void HandleDataChanged()
        {
            base.HandleDataChanged();

            if (context.Data != null)
            {
                rootTransform.localPosition = context.Data.worldPosition;
                rootTransform.localRotation = context.Data.worldRotation;

                if (context.Data.deviceFoV > 0)
                    gameCamera.fieldOfView = context.Data.deviceFoV;
            }
        }


        protected virtual void LateUpdate()
        {
            if(context.Data != null )
            {
                if (context.Data.deviceFoV > 0)
                    gameCamera.fieldOfView = context.Data.deviceFoV;

                if (context.Data.wasTracked)
                    UpdatePlacementFromTracked();
            }
        }

        void UpdatePlacementFromTracked()
        {
            if (controllerManager != null && controllerManager.objects.Length > 0)
            {
                GameObject controllerObj = controllerManager.objects[controllerManager.objects.Length - 1];
                if (controllerObj.activeInHierarchy)
                {
                    if (trackedController == null)
                        trackedController = controllerObj.GetComponent<SteamVR_TrackedObject>();

                    if (trackedController != null && trackedController.index != SteamVR_TrackedObject.EIndex.None && trackedController.isValid)
                    {
                        rootTransform.position = trackedController.transform.TransformPoint(context.Data.trackedPosition);
                        rootTransform.rotation = trackedController.transform.rotation * context.Data.trackedRotation;
                    }

                    return;
                }
            }

            //If controller wasn't found, reset it for searching
            trackedController = null;
        }

        protected void RenderGameCamera(Camera cam, RenderTexture target)
        {
            cam.targetTexture = target;
            if (GameRenderStarted != null)
                GameRenderStarted(this);
            cam.Render();
            if (GameRenderEnded != null)
                GameRenderEnded(this);
            cam.targetTexture = null;
        }
    }
}