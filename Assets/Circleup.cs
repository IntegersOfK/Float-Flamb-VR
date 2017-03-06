using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circleup : MonoBehaviour {

    public List<GameObject> spheres;
    public List<GameObject> cubes;

	// Use this for initialization
	void Start () {
        //Mesh mesh = GetComponent<Mesh>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

    }

    public void CircleUp()
    {

        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                cube.SetActive(false);
            }
        }

        foreach (GameObject sphere in spheres)
        {
            if (sphere != null)
            {
                sphere.SetActive(true);
            }
        }
    }
	
    public void SquareUp()
    {

        foreach (GameObject sphere in spheres)
        {
            if (sphere != null)
            {
                sphere.SetActive(false);
            }
        }

        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                cube.SetActive(true);
            }
        }

        
    }

	// Update is called once per frame
	void Update () {
		
	}
}
