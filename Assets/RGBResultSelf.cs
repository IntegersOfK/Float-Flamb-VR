using NewtonVR;
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
        StartCoroutine(UpdateColor());
    }

    IEnumerator UpdateColor()
    {

        yield return new WaitForSeconds(0.5f); //gotta wait for the stupid slider to stop sliding after ending interaction
        //if (SliderRed.CurrentValue != Result.material.color.r || SliderGreen.CurrentValue != Result.material.color.g || SliderBlue.CurrentValue != Result.material.color.b) {
        float start = Time.time;
        float elapsed = 0;
        Color oldColor = new Color(Result.material.color.r, Result.material.color.g, Result.material.color.b);
        Color newColor = new Color(SliderRed.CurrentValue, SliderGreen.CurrentValue, SliderBlue.CurrentValue);
        float duration = 0.8f;
        while (elapsed < duration)
        {
            // calculate how far through we are
            elapsed = Time.time - start;
            float normalisedTime = Mathf.Clamp(elapsed / duration, 0, 1);
            Result.material.color = Color.Lerp(oldColor, newColor, normalisedTime);
            // wait for the next frame
            yield return null;
        }

         
        
        
    }



}


