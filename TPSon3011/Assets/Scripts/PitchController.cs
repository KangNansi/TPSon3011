using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The pitch is controlling the direction of the character
// It goes from 440 to 880Hz
// 440 = backward
// 550 = left
// 660 = forward
// 770 = right
// ...etc



[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PitchController : AnalyseAudioACompleter {

    protected Rigidbody rigid;
    public float speed = 1000;
    public float minDB = 20;

    List<float> pitches = new List<float>();
    public int lissage = 50;

    // Use this for initialization
    new void Start () {
        base.Start();
        rigid = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        float pitch = GetPitch(trame);
        pitches.Add(pitch);
        while (pitches.Count > lissage)
        {
            pitches.RemoveAt(0);
        }
        pitch = 0;
        foreach(float p in pitches)
        {
            pitch += p;
        }
        pitch /= (float)pitches.Count;
        float DB = GetDB(GetRMS(trame));
        pitch = (DB > minDB) ? pitch : 0;
        Debug.Log(pitch);
        if (pitch < 440 || pitch > 880)
            return;
        float final = Mathf.Clamp((pitch - 440) / 440, 0, 1);
        rigid.AddForce(Quaternion.Euler(0, final * 360, 0) * -transform.forward * speed);

	}
}
