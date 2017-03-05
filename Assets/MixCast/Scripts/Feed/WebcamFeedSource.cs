/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public class WebcamFeedSource : CameraComponent
    {
        public const string COLOR_SPACE_EXP_PROP = "_ColorExponent";

        public MixCastCamera cam;

        public Material blitMaterial;
        public string playerDepthParameter = "_PlayerDepth";    //0 is at near plane, 1 is far
        public string chromaKeyColorParameter = "_KeyHsvMid";
        public string chromaKeyLimitsParameter = "_KeyHsvRange";

        private CommandBuffer insertFeedCommands;

        private string texSourceName;
        private RenderingPath renderPath;
        public WebCamTexture Texture { get; protected set; }

        protected override void OnEnable()
        {
            if (cam == null)
                cam = GetComponentInParent<MixCastCamera>();

            base.OnEnable();

            HandleDataChanged();
        }
        
        protected override void OnDisable()
        {
            ClearTexture();
            base.OnDisable();
        }

        void AddBlitCommand()
        {
            if (cam is ImmediateMixCastCamera)
            {
                insertFeedCommands = new CommandBuffer();

                if (cam.gameCamera.actualRenderingPath == RenderingPath.Forward)
                {
                    insertFeedCommands.Blit(Texture, BuiltinRenderTextureType.CurrentActive, blitMaterial);    //Insert real world
                    cam.gameCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, insertFeedCommands);
                }
                else
                {
                    //insertFeedCommands.SetRenderTarget(BuiltinRenderTextureType.GBuffer0);
                    insertFeedCommands.Blit(Texture, BuiltinRenderTextureType.CurrentActive, blitMaterial);    //Insert real world
                    cam.gameCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, insertFeedCommands);
                }
                renderPath = cam.gameCamera.actualRenderingPath;
            }
            else if( cam is BufferedMixCastCamera)
            {
                BufferedMixCastCamera buffCam = cam as BufferedMixCastCamera;
                buffCam.inputMaterial = blitMaterial;
            }
        }
        private void RemoveBlitCommand()
        {
            if (cam is ImmediateMixCastCamera)
            {
                if (renderPath == RenderingPath.Forward)
                {
                    cam.gameCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, insertFeedCommands);
                }
                else
                {
                    cam.gameCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, insertFeedCommands);
                }
                insertFeedCommands.Dispose();

                insertFeedCommands = null;
            }
            else if (cam is BufferedMixCastCamera)
            {
                BufferedMixCastCamera buffCam = cam as BufferedMixCastCamera;
                buffCam.inputTexture = null;
                buffCam.inputMaterial = null;
            }
        }

        private void Update()
        {
            if( context.Data != null && (context.Data.deviceName != texSourceName || renderPath != cam.gameCamera.actualRenderingPath) )
            {
                ClearTexture();
                SetTexture(context.Data.deviceName);
            }
        }

        protected void LateUpdate()
        {
            if (blitMaterial != null)
            {
                blitMaterial.mainTexture = Texture;

                //Tell the material if linear color space needs to be corrected
                //if (blitMaterial.HasProperty(COLOR_SPACE_EXP_PROP))
                    blitMaterial.SetFloat(COLOR_SPACE_EXP_PROP, QualitySettings.activeColorSpace == ColorSpace.Linear ? 2.2f : 1);

                if( cam is BufferedMixCastCamera )
                    (cam as BufferedMixCastCamera).inputTexture = Texture;
                if (Texture != null && cam.Output != null)
                {
                    //set transform to correct for different aspect ratios between screen and camera texture
                    float ratioRatio = ((float)Texture.width / Texture.height) / ((float)cam.Output.width / cam.Output.height);
                    blitMaterial.SetVector("_TextureTransform", new Vector4(1f / ratioRatio, 1, 0.5f * (1f - 1f / ratioRatio), 0));
                }
                //update the player's depth for the material
                if (!string.IsNullOrEmpty(playerDepthParameter) && blitMaterial.HasProperty(playerDepthParameter))
                    blitMaterial.SetFloat(playerDepthParameter, CalculatePlayerDepth());

                if (context.Data != null)
                {
                    if (!string.IsNullOrEmpty(chromaKeyColorParameter))
                        blitMaterial.SetVector(chromaKeyColorParameter, context.Data.chromakeying.active ? context.Data.chromakeying.keyHsvMid : Vector3.zero);
                    if (!string.IsNullOrEmpty(chromaKeyLimitsParameter))
                        blitMaterial.SetVector(chromaKeyLimitsParameter, context.Data.chromakeying.active ? context.Data.chromakeying.keyHsvRange : Vector3.zero);
                }
            }
        }

        private float CalculatePlayerDepth()
        {
            float distance = Vector3.Dot(cam.gameCamera.transform.forward, Camera.main.transform.position - cam.gameCamera.transform.position);
            return Mathf.Clamp01(Mathf.InverseLerp(cam.gameCamera.nearClipPlane, cam.gameCamera.farClipPlane, distance));
        }



        protected override void HandleDataChanged()
        {
            base.HandleDataChanged();
            ClearTexture();

            if (context.Data != null)
                SetTexture(context.Data.deviceName);
        }

        protected virtual void SetTexture(string deviceName)
        {
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                if (WebCamTexture.devices[i].name == deviceName)
                {
                    texSourceName = deviceName;
                    Texture = new WebCamTexture(deviceName);
                    Texture.wrapMode = TextureWrapMode.Clamp;
                    Texture.Play();

                    AddBlitCommand();
                }
            }
        }

        protected virtual void ClearTexture()
        {
            if (Texture != null)
            {
                RemoveBlitCommand();

                Texture.Stop();
                Texture = null;
                texSourceName = null;
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (string.IsNullOrEmpty(playerDepthParameter))
                return;

            float playerDepth = blitMaterial.GetFloat(playerDepthParameter);
            float playerDistance = Mathf.Lerp(cam.gameCamera.nearClipPlane, cam.gameCamera.farClipPlane, playerDepth);
            Vector3 playerCenter = cam.gameCamera.transform.position + cam.gameCamera.transform.forward * playerDistance;
            Color gizCol = Color.blue;
            gizCol.a = 0.1f;
            Gizmos.color = gizCol;
            Gizmos.matrix = Matrix4x4.TRS(playerCenter, cam.gameCamera.transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, new Vector3(GetWidthAtDistance(cam.gameCamera, playerDistance), GetHeightAtDistance(cam.gameCamera, playerDistance), 0));
        }
#endif
        public static float GetHeightAtDistance(Camera cam, float distance)
        {
            return 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
        public static float GetWidthAtDistance(Camera cam, float distance)
        {
            return cam.aspect * GetHeightAtDistance(cam, distance);
        }
    }
}