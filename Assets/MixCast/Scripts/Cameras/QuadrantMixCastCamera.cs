/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public class QuadrantMixCastCamera : MixCastCamera
    {
        public const string FIX_FG_SHADOW_SHADER = "Hidden/BPR/ScreenSpaceShadows NoFade";

        public float layerOverlap = 0f;
        public Color clearColor = new Color(0, 0, 0, 0);

        private Shader noForegroundShadowFadeShader;
        private Material postBlit;
        private RenderTexture quadrantTex;
        private RenderTexture fpTex;
        CommandBuffer captureFP;

        private void Awake()
        {
            noForegroundShadowFadeShader = Shader.Find(FIX_FG_SHADOW_SHADER);
            postBlit = new Material(Shader.Find("Hidden/BPR/AlphaWrite"));
            fpTex = new RenderTexture(1000, 1000, 24);
        }

        protected override void OnEnable()
        {
            gameCamera.enabled = false;

            BuildOutput();

            captureFP = new CommandBuffer();
            captureFP.Blit(BuiltinRenderTextureType.CameraTarget, fpTex);

            Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, captureFP);

            base.OnEnable();

            StartCoroutine(RenderLoop());
        }
        protected override void OnDisable()
        {
            gameCamera.enabled = true;

            Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, captureFP);
            ReleaseOutput();

            StopCoroutine("RenderLoop");

            base.OnDisable();
        }

        void BuildOutput()
        {
            Output = new RenderTexture(Screen.width, Screen.height, 24);
            quadrantTex = new RenderTexture(Mathf.CeilToInt(Output.width * 0.5f), Mathf.CeilToInt(Output.height * 0.5f), 24);
        }
        void ReleaseOutput()
        {
            if (Output != null)
            {
                (Output as RenderTexture).Release();
                Output = null;
            }
            if( quadrantTex != null )
            {
                quadrantTex.Release();
                quadrantTex = null;
            }
        }
        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (Screen.width != Output.width || Screen.height != Output.height)
            {
                ReleaseOutput();
                BuildOutput();
            }
        }


        IEnumerator RenderLoop()
        {
            while (isActiveAndEnabled)
            {
                

                ClearBuffer();
                RenderBackground();
                RenderForeground();
                RenderFirstPerson();
                Graphics.Blit(null, postBlit);

                yield return null;
            }
        }

        void ClearBuffer()
        {
            Graphics.SetRenderTarget(Output as RenderTexture);
            GL.Clear(true, true, clearColor);
            Graphics.SetRenderTarget(null);
        }
        void RenderBackground()
        {
            float oldNearClip = gameCamera.nearClipPlane;
            Color oldBackgroundColor = gameCamera.backgroundColor;

            gameCamera.nearClipPlane = Mathf.Min(gameCamera.farClipPlane - 0.001f, CalculatePlayerDepth() - 0.5f * layerOverlap);
            gameCamera.backgroundColor = clearColor;

            RenderGameCamera(gameCamera, quadrantTex);

            gameCamera.nearClipPlane = oldNearClip;
            gameCamera.backgroundColor = oldBackgroundColor;

            Graphics.SetRenderTarget(Output as RenderTexture);
            Graphics.DrawTexture(new Rect(0, 0, Screen.width * 0.5f, Screen.height * 0.5f), quadrantTex);
        }
        void RenderForeground()
        {
            float oldFar = gameCamera.farClipPlane;
            CameraClearFlags oldFlags = gameCamera.clearFlags;
            Color oldBackgroundColor = gameCamera.backgroundColor;

            gameCamera.farClipPlane = Mathf.Max(gameCamera.nearClipPlane + 0.001f, CalculatePlayerDepth() + 0.5f * layerOverlap);

            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = clearColor;

            Shader originalScreenSpaceShadowShader = GraphicsSettings.GetCustomShader(BuiltinShaderType.ScreenSpaceShadows);
            GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, noForegroundShadowFadeShader);

            RenderGameCamera(gameCamera, quadrantTex);

            GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, originalScreenSpaceShadowShader);

            gameCamera.farClipPlane = oldFar;
            gameCamera.clearFlags = oldFlags;
            gameCamera.backgroundColor = oldBackgroundColor;

            Graphics.SetRenderTarget(Output as RenderTexture);
            Graphics.DrawTexture(new Rect(Screen.width * 0.5f, 0, Screen.width * 0.5f, Screen.height * 0.5f), quadrantTex);
        }
        void RenderFirstPerson()
        {
            Graphics.SetRenderTarget(Output as RenderTexture);
            Graphics.DrawTexture(new Rect(0, Screen.height * 0.5f, Screen.width * 0.5f, Screen.height * 0.5f), fpTex);

            //uint texId = 0;
            //Valve.VR.EVRCompositorError err = SteamVR.instance.compositor.GetMirrorTextureGL(Valve.VR.EVREye.Eye_Right, ref texId, quadrantTex.GetNativeTexturePtr());
            //if( err == Valve.VR.EVRCompositorError.None )
            //{
            //    Graphics.SetRenderTarget(Output as RenderTexture);
            //    Graphics.DrawTexture(new Rect(Screen.width * 0.5f, 0, Screen.width * 0.5f, Screen.height * 0.5f), quadrantTex);
            //}
            //else
            //{
            //    Debug.LogError("Can't access eye texture: " + err.ToString());
            //}

        }

        float CalculatePlayerDepth()
        {
            //Use depth of HMD
            return Vector3.Dot(gameCamera.transform.forward, Camera.main.transform.position - gameCamera.transform.position);
        }
    }
}