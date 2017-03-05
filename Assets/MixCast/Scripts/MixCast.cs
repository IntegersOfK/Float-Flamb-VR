/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCast
    {
        public static bool Active { get; protected set; }

        public static event System.Action MixCastEnabled;
        public static event System.Action MixCastDisabled;


        public static MixCastData Settings { get; protected set; }

        static MixCast()
        {
            Settings = MixCastRegistry.ReadData();
        }

        public static void SetActive(bool active)
        {
            if (Active == active)
                return;

            Active = active;
            if (Active)
                MixCastEnabled();
            else
                MixCastDisabled();
        }
    }
}