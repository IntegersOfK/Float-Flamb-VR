/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    //Place at the root of the camera hierarchy. 
    public class BufferedMixCastCamera : MixCastCamera
    {
        public const string DEPTH_WRITE_SHADER = "Hidden/BPR/DepthWrite";
        public const string FIX_FG_SHADOW_SHADER = "Hidden/BPR/ScreenSpaceShadows NoFade";
        public const string BLIT_FOREGROUND_SHADER = "Hidden/BPR/Foreground Blit";

        [System.Serializable]
        public class GameFrame
        {
            public float playerDepth;

            public RenderTexture foregroundBuffer;
            public RenderTexture backgroundBuffer;

            //public Matrix4x4 cameraTransform;
        }

        public float layerOverlap = 0f;
        public LayerMask forceToBackground = 0;

        public Texture inputTexture;
        public Material inputMaterial;
        public float cameraLag;

        private float gameNearClipPlane;
        private float gameFarClipPlane;
        private List<GameFrame> gameFrames = new List<GameFrame>();
        private int gameFrameIndex = 0;

        private Shader blockoutBackgroundShader;
        private Shader noForegroundShadowFadeShader;
        

        private Material blitBackgroundMat;
        private Material blitForegroundMat;

        private void Awake()
        {
            gameNearClipPlane = gameCamera.nearClipPlane;
            gameFarClipPlane = gameCamera.farClipPlane;

            blitBackgroundMat = new Material(Shader.Find("Unlit/Texture"));
            blockoutBackgroundShader = Shader.Find(DEPTH_WRITE_SHADER);
            noForegroundShadowFadeShader = Shader.Find(FIX_FG_SHADOW_SHADER);
            blitForegroundMat = new Material(Shader.Find(BLIT_FOREGROUND_SHADER));

            gameCamera.enabled = false;
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            gameFrameIndex = 0;

            Output = new RenderTexture(Screen.width, Screen.height, 16);

            GenerateFrames((int)(cameraLag * 30));

        }

        protected override void OnDisable()
        {
            base.OnDisable();
        
            (Output as RenderTexture).Release();
            Output = null;

            DisposeOfFrames();
        }
        void GenerateFrames(int count)
        {
            DisposeOfFrames();

            for( int i = 0; i < count; i++ )
            {
                GameFrame newFrame = new GameFrame();
                newFrame.backgroundBuffer = new RenderTexture(Screen.width, Screen.height, 16);
                newFrame.foregroundBuffer = new RenderTexture(Screen.width, Screen.height, 16);
                gameFrames.Add(newFrame);
            }

            gameFrameIndex = 0;
        }
        void DisposeOfFrames()
        {
            for (int i = 0; i < gameFrames.Count; i++)
            {
                gameFrames[i].foregroundBuffer.Release();
                gameFrames[i].backgroundBuffer.Release();
            }
            gameFrames.Clear();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            CollectGameFrame();
            gameFrameIndex++;
            CompositeCurrentFrame();    //Frame at gameFrameIndex now oldest game frame
        }

        void CollectGameFrame()
        {
            GameFrame newFrame = gameFrames[gameFrameIndex % gameFrames.Count];

            newFrame.playerDepth = CalculatePlayerDepth();

            GrabBackground(newFrame);
            GrabForeground(newFrame);
        }

        void GrabBackground(GameFrame targetFrame)
        {
            
            gameCamera.farClipPlane = gameFarClipPlane;
            gameCamera.nearClipPlane = targetFrame.playerDepth - 0.5f * layerOverlap;
            //Start on black background
            gameCamera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth | CameraClearFlags.Skybox;
            gameCamera.backgroundColor = new Color(0, 0, 0, 1);

            RenderGameCamera(gameCamera, targetFrame.backgroundBuffer);

            if( forceToBackground > 0 )
            {
                LayerMask oldCull = gameCamera.cullingMask;
                gameCamera.farClipPlane = targetFrame.playerDepth + 0.5f * layerOverlap;
                gameCamera.nearClipPlane = gameNearClipPlane;
                gameCamera.cullingMask = forceToBackground;
                gameCamera.clearFlags = CameraClearFlags.Depth;
                RenderGameCamera(gameCamera, targetFrame.backgroundBuffer);
                gameCamera.cullingMask = oldCull;
            }
        }
        void GrabForeground(GameFrame targetFrame)
        {
            gameCamera.farClipPlane = targetFrame.playerDepth + 0.5f * layerOverlap;
            gameCamera.nearClipPlane = gameNearClipPlane;
            //
            gameCamera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
            gameCamera.backgroundColor = new Color(0, 0, 0, 0);
            
            Shader originalScreenSpaceShadowShader = GraphicsSettings.GetCustomShader(BuiltinShaderType.ScreenSpaceShadows);
            GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, noForegroundShadowFadeShader);

            LayerMask oldCull = gameCamera.cullingMask;
            if ( forceToBackground > 0 )
            {
                gameCamera.cullingMask = forceToBackground;
                gameCamera.RenderWithShader(blockoutBackgroundShader, null);
                gameCamera.clearFlags = CameraClearFlags.Nothing;       //Whole point: next Render() respects this depth buffer
            }
            
            gameCamera.cullingMask = oldCull ^ forceToBackground;
            RenderGameCamera(gameCamera, targetFrame.foregroundBuffer);
            gameCamera.cullingMask = oldCull;

            GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, originalScreenSpaceShadowShader);

            gameCamera.nearClipPlane = gameNearClipPlane;
            gameCamera.farClipPlane = gameFarClipPlane;
        }

        float CalculatePlayerDepth()
        {
            //Use depth of HMD
            return Mathf.Max(gameNearClipPlane, Vector3.Dot(gameCamera.transform.forward, Camera.main.transform.position - gameCamera.transform.position));
        }

        void CompositeCurrentFrame()
        {
            GameFrame currentFrame = gameFrames[gameFrameIndex % gameFrames.Count];
            RenderTexture outputRt = (Output as RenderTexture);
            outputRt.DiscardContents(true, false);

            Rect fullscreenRect = new Rect(0, 0, Output.width, Output.height);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Output.width, Output.height, 0);
            Graphics.SetRenderTarget(outputRt);
            Graphics.DrawTexture(fullscreenRect, currentFrame.backgroundBuffer, blitBackgroundMat);
            GL.Clear(true, false, Color.black);
            if( inputTexture != null && inputMaterial != null )
                Graphics.DrawTexture(fullscreenRect, inputTexture, inputMaterial);
            Graphics.DrawTexture(fullscreenRect, currentFrame.foregroundBuffer, blitForegroundMat);
            //Graphics.Blit(currentFrame.backgroundBuffer, outputRt);
            //Graphics.Blit(inputTexture, outputRt, inputMaterial);   //Insert real world
            //Graphics.Blit(currentFrame.foregroundBuffer, outputRt);

            GL.PopMatrix();
        }
    }
}