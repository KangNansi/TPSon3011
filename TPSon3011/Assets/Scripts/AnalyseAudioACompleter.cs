using UnityEngine;  
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;

public class AnalyseAudioACompleter : MonoBehaviour {
	
	// VARIABLES GLOBALES
	private AudioSource audio; 				
	private const int FREQUENCY = 44100;    // ??
	private const int NB_SAMPLES = 512;     // ??
    private const float REFVALUE = 0.00045f;
    public int NB_TRAMES = 50;
    private int current = 0;

    public float strength = 0.1f;

    private float[] trame;
    private float[] spectre;
    private float spmax = 0;
    private List<float> volt = new List<float>();
    private List<float> med = new List<float>();
	
	// ===============================================
	// =========== METHODES START ET UPDATE ==========
	// ===============================================
	
	// Use this for initialization
	// ----------------------------
	void Start () {
		trame = new float[NB_SAMPLES];
		this.audio = GetComponent<AudioSource> (); 
		StartMicListener();   
	}
	
	// Update is called once per frame
	// -------------------------------
	void Update () {
		// If the audio has stopped playing, this will restart the mic play the clip.
		if (!audio.isPlaying) {
			StartMicListener();
		}
		
		votreFonction ();

	}

	// ===============================================
	// ============== AUTRES METHODES ================
	// ===============================================

	// Starts the Mic, and plays the audio back in (near) real-time.
	// --------------------------------------------------------------
	private void StartMicListener() {
		if (audio.clip == null) {
			audio.clip = Microphone.Start ("Built-in Microphone", true, 999, FREQUENCY);
			// HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
			while (!(Microphone.GetPosition("Built-in Microphone") > 0)) {
			} audio.Play ();
		}
	}
	
	// Votre Fonction
	// -------------------------------
	private void votreFonction(){
		audio.GetOutputData (trame, 0);

        float res = GetDB(GetRMS(trame, NB_SAMPLES));
        volt.Add(res);
        while(volt.Count >= NB_TRAMES)
        {
            volt.RemoveAt(0);
        }
        float fres = 0;
        med = new List<float>();
        foreach(float a in volt)
        {
            med.Add(a);
            //fres += a;
        }
        med.Sort();
        if(med.Count % 2 == 0)
        {
            fres = (med[(med.Count/2)-1] + med[(med.Count/2)])/2f;
        }
        else
        {
            fres = med[(med.Count / 2)];
        }
        float r = GetFundamental(NB_SAMPLES*16);
        transform.position = new Vector3(0, r * strength, 0);
        Debug.Log(r);
	}

    private float GetRMS(float[] tr, int nb_samples)
    {
        float RMS = 0;
        for (int i = 0; i < nb_samples; i++)
        {
            RMS += (tr[i] * tr[i]);
        }
        RMS = Mathf.Sqrt(RMS / NB_SAMPLES);
        return RMS;
    }

    private float GetDB(float rms)
    {
        return 20 * Mathf.Log10(rms / REFVALUE);
    }

    private float GetFundamental(float n)
    {
        spectre = new float[(int)n];
        audio.GetSpectrumData(spectre, 0, FFTWindow.Hamming);
        float max = 0;
        float ind = 0;
        for(int i = 0; i < n; i++)
        {
            if (spectre[i] > max)
            {
                max = spectre[i];
                ind = i;
            }
        }
        spmax = max;
        DrawSpectrum(spectre, max);
        return ind * (AudioSettings.outputSampleRate / 2) / n;

    }

    private void OnPostRender()
    {
        DrawSpectrum(spectre, spmax);
    }

    private void DrawSpectrum(float[] t, float max)
    {
        if (max < 0) return;
        Vector3 last = Camera.main.ScreenToWorldPoint(Vector3.zero);
        last.z = 0;
        float mx = Mathf.Log(t.Length);
        for(int i = 0; i < t.Length; i++)
        {
            float x = Mathf.Log(i)/mx;
            float y = (t[i] / max);
            x = Mathf.Clamp(x, 0, 1);
            y = Mathf.Clamp(y, 0, 1);
            Vector3 n = Camera.main.ScreenToWorldPoint(new Vector3(x*Camera.main.pixelWidth, y*Camera.main.pixelHeight));
            n.z = 0;
            Debug.DrawLine(last, n, new Color((1-(n.y/10f)), (n.y / 10f), 0));
            last = n;
        }
    }
	
	
}
