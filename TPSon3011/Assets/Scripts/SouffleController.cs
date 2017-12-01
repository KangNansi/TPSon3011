using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SouffleController : AnalyseAudioACompleter {

    protected Rigidbody rigid;
    public float speed = 1000;
    public float minDB = 20;

	// Use this for initialization
	new void Start () {
        base.Start();
        rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        float pitch = GetPitch(trame);
        float db = GetDB(GetRMS(trame));
        Debug.Log("Pitch: " + pitch + "|DB: " + db);
        if(db>minDB && pitch < 0.1f)
        {
            rigid.AddForce(transform.forward * db * speed);
        }
	}
}
