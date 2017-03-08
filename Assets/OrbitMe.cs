using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMe : MonoBehaviour {

    public List<GameObject> orbitList;
    public int level;


	// Use this for initialization
	void Start () {
		
	}


    private void OnTriggerEnter(Collider other)
    {
        {

            Orbiter orbit = other.GetComponent<Orbiter>();
            if (!orbit.isActiveAndEnabled)
            {
                orbitList.Add(other.gameObject);
                orbit.enabled = true;
                if (level == 1)
                {
                    if (orbitList.Count == 1)
                    {
                        orbit.orbitDegreesPerSec = 45;
                    //    orbit.axis = new Vector3(0f, 1f, 0f);
                    }

                    if (orbitList.Count == 2)
                    {
                        orbit.enabled = true;
                        orbit.orbitDegreesPerSec = 180;
                       // orbit.axis = new Vector3(1f, 0f, 0f);
                    }
                }
            }

        } 
    }


    // Update is called once per frame
    void Update () {
		
	}
}
