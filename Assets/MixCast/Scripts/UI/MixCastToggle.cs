/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(Toggle))]
    public class MixCastToggle : MonoBehaviour
    {
        private const string SETUP_URL = "https://blueprinttools.com/mixcast";

        private Toggle toggle;

        private IEnumerator Start()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(HandleToggleSet);

            yield return null;

            HandleToggleSet(toggle.isOn);
        }
        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(HandleToggleSet);
        }

        void HandleToggleSet(bool val)
        {
            MixCast.SetActive(val);

            //Open MixCast webpage if the user doesn't have MixCast calibrated
            if( val && MixCast.Settings.cameras.Count == 0 )
            {
                Application.OpenURL(SETUP_URL);
            }
        }
    }
}