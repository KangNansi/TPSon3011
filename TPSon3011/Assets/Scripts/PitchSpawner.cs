using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchSpawner : AnalyseAudioACompleter
{
    float timer = 0;
    public float tempo = 1;
    public GameObject enemy;
    // Use this for initialization
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        timer += Time.deltaTime;
        if (timer > tempo)
        {
            float pitch = GetPitch(trame);
            float DB = GetDB(GetRMS(trame));
            float normalizedPitch = (pitch - 440) / 110f;
            if (DB > 20)
            {
                GameObject e = Instantiate(enemy);
                e.transform.position = transform.position + (Vector3.up * normalizedPitch);
            }
            timer = 0;
        }
    }
}