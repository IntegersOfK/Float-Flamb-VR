using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resize : MonoBehaviour
{

    public NVRSlider SliderSize;


    //private Renderer Result;

    private void Start()
    {
        //this.Result = GetComponent<Renderer>();
        

    }

    void OnEnable()
    {
        NVRSlider.changeEvent += ApplesChanged; // subscribing to the event. 
    }

    void ApplesChanged(float updatedNumber)
    {
        StartCoroutine(UpdateSize());
    }

    IEnumerator UpdateSize()
    {
        yield return new WaitForSeconds(0.5f); //gotta wait for the stupid slider to stop sliding after ending interaction
        iTween.ScaleTo(gameObject, iTween.Hash("x", SliderSize.CurrentValue / 2, "y", SliderSize.CurrentValue / 2, "z", SliderSize.CurrentValue / 2, "easeType", "easeInOutExpo", "speed", 0.1f));

    }



}


