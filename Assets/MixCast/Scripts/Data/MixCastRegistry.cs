/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastRegistry
    {
        private const string REGISTRY_NAME = "SOFTWARE\\Blueprint Reality\\MixCast VR";
        private const string REGISTRY_KEY = "DATA";

        public static MixCastData ReadData()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(REGISTRY_NAME);
            string dataStr = reg.GetValue(REGISTRY_KEY, null) as string;
            if (!string.IsNullOrEmpty(dataStr))
                return JsonUtility.FromJson<MixCastData>(dataStr);
            else
                return new MixCastData();
        }
        public static void WriteData(MixCastData data)
        {
            string dataStr = JsonUtility.ToJson(data);
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(REGISTRY_NAME);
            reg.SetValue(REGISTRY_KEY, dataStr);
        }
    }
}