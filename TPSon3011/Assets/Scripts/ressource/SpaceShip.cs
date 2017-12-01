using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : MonoBehaviour {

    public float speed;
    public GameObject proj;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float v = Input.GetAxis("Vertical");
        transform.position = transform.position + new Vector3(0, v * speed * Time.deltaTime, 0);
        if (Input.GetButtonDown("Fire1"))
        {
            GameObject o = Instantiate(proj);
            proj.transform.position = transform.position + Vector3.right;
        }
	}
}
