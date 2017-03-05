using UnityEngine;
using System.Collections;

public class Pusher : MonoBehaviour
{

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    private bool dePadUp = false;
    private bool dePadDown = false;

    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {
        if (controller == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }

        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y > 0.5f)
            {
                
                Debug.Log(trackedObj.index + " Movement Dpad Up");
                this.dePadDown = false;
                this.dePadUp = !this.dePadUp;
            }


            if (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y < -0.5f)
            {
                Debug.Log(trackedObj.index + " Movement Dpad Down");
                this.dePadUp = false;
                this.dePadDown = !this.dePadDown;
            }

            if (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x > 0.5f)
            {
                Debug.Log(trackedObj.index + " Movement Dpad Right");
            }

            if (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x < -0.5f)
            {
                Debug.Log(trackedObj.index + " Movement Dpad Left");
            }
        }
    }

   
    
    private void OnTriggerStay(Collider other)
    {
        if (dePadUp && other.gameObject.tag == "Box")
        {
            //Vector3 direction = other.gameObject.GetComponent<Transform>().position - controller.transform.pos;
            //direction.Normalize();
            other.GetComponent<Rigidbody>().AddForce(transform.up * -1f);
        }

        if (dePadDown && other.gameObject.tag == "Box")
        {
            other.GetComponent<Rigidbody>().AddForce(transform.up * 1f);
        }

    }


}