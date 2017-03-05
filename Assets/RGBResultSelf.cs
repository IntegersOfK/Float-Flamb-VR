﻿using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBResultSelf : MonoBehaviour {

    public NVRSlider SliderRed;
    public NVRSlider SliderGreen;
    public NVRSlider SliderBlue;

    private Renderer Result;

    private void Start()
    {
        this.Result = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        NVRSlider.changeEvent += ApplesChanged; // subscribing to the event. 
    }

    void ApplesChanged(float updatedNumber)
    {
        UpdateColor();

    }

    private void UpdateColor()
    {
        //if (SliderRed.CurrentValue != Result.material.color.r || SliderGreen.CurrentValue != Result.material.color.g || SliderBlue.CurrentValue != Result.material.color.b) {
            Result.material.color = new Color(SliderRed.CurrentValue, SliderGreen.CurrentValue, SliderBlue.CurrentValue);
        //}
        
    }
}


