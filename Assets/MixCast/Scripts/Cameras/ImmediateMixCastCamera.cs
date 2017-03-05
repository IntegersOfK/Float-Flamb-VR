/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    [ExecuteInEditMode]
    public class ImmediateMixCastCamera : MixCastCamera
    {
        //Simple MixedRealityCamera that renders the game camera into the Output. Additional logic can be attached to the game camera as CommandBuffers in order to insert the real feed
        private Material postBlit;
        private CommandBuffer postBuff;

        private void Awake()
        {
            postBlit = new Material(Shader.Find("Hidden/BPR/AlphaWrite"));
            postBuff = new CommandBuffer();
            postBuff.Blit(null, BuiltinRenderTextureType.CameraTarget, postBlit);

            gameCamera.enabled = false;
        }
        private void OnDestroy()
        {
            postBuff.Dispose();
            postBuff = null;
            postBlit = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            gameCamera.AddCommandBuffer(CameraEvent.AfterEverything, postBuff);
            

            BuildOutput();

            StartCoroutine(RenderLoop());
        }
        protected override void OnDisable()
        {
            ReleaseOutput();
            gameCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, postBuff);

            StopCoroutine("RenderLoop");

            base.OnDisable();
        }

        void BuildOutput()
        {
            Output = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        }
        void ReleaseOutput()
        {
            if( Output != null )
            {
                (Output as RenderTexture).Release();
                Output = null;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if( Screen.width != Output.width || Screen.height != Output.height )
            {
                ReleaseOutput();
                BuildOutput();
            }
        }

        IEnumerator RenderLoop()
        {
            while( isActiveAndEnabled )
            {
                yield return new WaitForEndOfFrame();
                RenderGameCamera(gameCamera, Output as RenderTexture);
                yield return null;
            }
        }
    }
}