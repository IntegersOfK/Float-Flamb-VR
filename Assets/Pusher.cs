using UnityEngine;
using System.Collections;

public class Pusher : MonoBehaviour
{

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    private float dePadUp = 0f;
    private float dePadDown = 0f;
    public GameObject displayCyl;
    private Renderer rend;
    public Color pullColor;
    public Color pushColor;


    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        this.rend = displayCyl.GetComponent<Renderer>();
    }

    void Update()
    {
        if (controller == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }

        if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            dePadUp = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y;
            displayCyl.SetActive(true);
            rend.material.color = Color.Lerp(pullColor, pushColor, dePadUp*2);
            //print(dePadUp);
        }
        else
        {
            dePadUp = 0f;
            displayCyl.SetActive(false);
        }
    }

   
    
    private void OnTriggerStay(Collider other)
    {
        if (dePadUp!=0f && other.gameObject.tag == "Box")
        {
            //Vector3 direction = other.gameObject.GetComponent<Transform>().position - controller.transform.pos;
            //direction.Normalize();
            other.GetComponent<Rigidbody>().AddForce(transform.up * dePadUp/2*-1);
        }

       // if (dePadDown && other.gameObject.tag == "Box")
      //  {
     //       other.GetComponent<Rigidbody>().AddForce(transform.up * 100f);
     //   }

    }


}