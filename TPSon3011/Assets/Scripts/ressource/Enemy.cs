using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    float timer = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += Vector3.left * 10 * Time.deltaTime;
        timer += Time.deltaTime;
        if (timer > 5)
            Destroy(gameObject);
	}
}
