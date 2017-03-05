/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class SetRenderingForMixCast : MonoBehaviour
    {
        public List<Renderer> targets = new List<Renderer>();
        public bool renderForMixCast = false;

        private void OnEnable()
        {
            MixCastCamera.GameRenderStarted += HandleMixCastRenderStarted;
            MixCastCamera.GameRenderEnded += HandleMixCastRenderEnded;

            if (targets.Count == 0)
                GetComponentsInChildren<Renderer>(targets);

            if( renderForMixCast )
            {
                //Disable Renderers that draw in mixed reality only
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].enabled = !renderForMixCast;
                }
            }
        }
        private void OnDisable()
        {
            MixCastCamera.GameRenderStarted -= HandleMixCastRenderStarted;
            MixCastCamera.GameRenderEnded -= HandleMixCastRenderEnded;
        }


        private void HandleMixCastRenderStarted(MixCastCamera cam)
        {
            for( int i = 0; i < targets.Count; i++ )
            {
                targets[i].enabled = renderForMixCast;
            }
        }
        private void HandleMixCastRenderEnded(MixCastCamera cam)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].enabled = !renderForMixCast;
            }
        }


        private void Reset()
        {
            targets.Clear();
            GetComponentsInChildren<Renderer>(targets);
        }
    }
}