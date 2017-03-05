using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGravity : MonoBehaviour {
    public GameObject[] gravItems;

    public void Toggle()
    {
        foreach (GameObject box in gravItems)
        {
            if (box.GetComponent<Rigidbody>().useGravity == true) {
                box.GetComponent<Rigidbody>().useGravity = false;
            }
            else
            {
                box.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }

    // Use this for initialization
    void Start () {
        this.gravItems = GameObject.FindGameObjectsWithTag("Box");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}


