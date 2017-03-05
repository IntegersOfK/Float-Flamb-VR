using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pusher : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private void OnCollisionStay(Collision collision)

    {
        if (collision.gameObject.tag == "Box") { 

        collision.transform.Translate(Vector3.down * Time.deltaTime * 5);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
