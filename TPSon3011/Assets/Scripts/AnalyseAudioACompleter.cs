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
    public int pitchLissage = 10;
    public int finalComp = 100;
    private int current = 0;

    public float strength = 0.1f;
    public float correlStrength = 3f;

    public float correlThreshold = 0.3f;

    public bool drawCorrelation = false;
    public bool drawSpectre = false;

    protected float[] trame;
    private float[] spectre;
    private float[] lastCorrel;
    private float spmax = 0;
    private List<float> volt = new List<float>();
    private List<float> med = new List<float>();
    private List<float> pitchBuffer = new List<float>();
    private List<float> finalBuffer = new List<float>();

    protected struct localMaximum
    {
        public float value;
        public int index;
        public localMaximum(int ind, float val)
        {
            value = val;
            index = ind;
        }
    }
	
	// ===============================================
	// =========== METHODES START ET UPDATE ==========
	// ===============================================
	
	// Use this for initialization
	// ----------------------------
	protected void Start () {
		trame = new float[NB_SAMPLES];
        lastCorrel = new float[NB_SAMPLES * 2];
        for(int i = 0; i < lastCorrel.Length; i++)
        {
            lastCorrel[i] = 0;
        }
		this.audio = GetComponent<AudioSource> (); 
		StartMicListener();   
	}
	
	// Update is called once per frame
	// -------------------------------
	protected void Update () {
        // If the audio has stopped playing, this will restart the mic play the clip.
        if (!audio.isPlaying) {
			StartMicListener();
		}
        audio.GetOutputData(trame, 0);
        if (drawCorrelation)
            DrawOnScreen(trame);
        if (drawSpectre)
            DrawSpectrum(trame, GetMax(trame));
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

    protected float GetRMS(float[] tr)
    {
        float RMS = 0;
        for (int i = 0; i < tr.Length; i++)
        {
            RMS += (tr[i] * tr[i]);
        }
        RMS = Mathf.Sqrt(RMS / NB_SAMPLES);
        return RMS;
    }

    protected float GetDB(float rms)
    {
        return 20 * Mathf.Log10(rms / REFVALUE);
    }

    protected float GetFundamental(float n)
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
        //DrawSpectrum(spectre, max);

        //DrawOnScreen(trame);
        return ind * (AudioSettings.outputSampleRate / 2) / n;

    }

    protected void DrawSpectrum(float[] t, float max)
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

    protected float getCorrelPoint(float[] t, int lag)
    {
        float res = 0;
        for(int i = Mathf.Max(0,0-lag); i < Mathf.Min(t.Length, t.Length-lag); i++)
        {
            res += t[i] * t[i + lag];
        }
        return res;
    }

    protected float[] GetCorrel(float[] t)
    {
        float[] res = new float[t.Length * 2];
        float corref = getCorrelPoint(t, 0);
        for(int i = -t.Length; i < t.Length; i++)
        {
            res[i + t.Length] = getCorrelPoint(t, i)/corref;
        }
        return res;
    }

    protected float GetMax(float[] t)
    {
        float max = 0;
        foreach(float f in t)
        {
            if (f > max)
                max = f;
        }
        return max;
    }

    protected float GetMoyenne(float[] t)
    {
        float moyenne = 0;
        foreach(float f in t)
        {
            moyenne += f;
        }
        return moyenne / (float)t.Length;
    }

    protected List<localMaximum> GetLocalMaximum(float[] correl)
    {
        List<localMaximum> maxs = new List<localMaximum>();
        for(int i = 0; i < correl.Length; i++)
        {
            if (i > 0 && correl[i - 1] > correl[i])
                continue;
            if (i < correl.Length-1 && correl[i + 1] > correl[i])
                continue;
            maxs.Add(new localMaximum(i, correl[i]));
        }
        return maxs;
    }

    protected void DrawCorrel(float[] t)
    {
        Vector3 last = Camera.main.ScreenToWorldPoint(Vector3.zero);
        last.z = 0;
        float mx = Mathf.Log(t.Length);
        float[] correl = GetCorrel(t);
        for (int i = 0; i < correl.Length; i++)
        {
            float x = i/(float)(correl.Length);
            float y = correl[i]*correlStrength;
            x = Mathf.Clamp(x, 0, 1);
            y = Mathf.Clamp(y, 0, 1);
            Vector3 n = Camera.main.ScreenToWorldPoint(new Vector3(x * Camera.main.pixelWidth, y * Camera.main.pixelHeight));
            n.z = 0;
            Debug.DrawLine(last, n, new Color((1 - (n.y / 10f)), (n.y / 10f), 0));
            last = n;
        }
    }

    protected void DrawOnScreen(float[] t)
    {
        Vector3 last = Camera.main.ScreenToWorldPoint(Vector3.zero);
        last.z = 0;
        float mx = Mathf.Log(t.Length);
        float[] correl = GetCorrel(t);
        for (int i = 0; i < Camera.main.pixelWidth; i++)
        {
            float x = i;
            float y = correl[(int)((i/ (float)Camera.main.pixelWidth)*correl.Length)] * correlStrength;
            //x = Mathf.Clamp(x, 0, 1);
            y = Mathf.Clamp(y, 0, 1);
            Vector3 n = Camera.main.ScreenToWorldPoint(new Vector3(x, y * Camera.main.pixelHeight));
            n.z = 0;
            Vector3 n2 = Camera.main.ScreenToWorldPoint(new Vector3(x, 0));
            n2.z = 0;
            Debug.DrawLine(n2, n, new Color((1 - (n.y / 10f)), (n.y / 10f), 0));
            last = n;
        }
    }

    protected float GetPitch(float[] t)
    {
        float[] correl = GetCorrel(t);
        List<localMaximum> localMax = GetLocalMaximum(correl);
        float correlOrigin = correl[correl.Length/2];
        float nextMax = 0;
        int nextMaxInd = -1;
        for(int i = 0; i < localMax.Count; i++)
        {
            if(localMax[i].index!=correl.Length/2 && localMax[i].value > nextMax && localMax[i].value > correlThreshold)
            {
                nextMax = localMax[i].value;
                nextMaxInd = localMax[i].index;
            }
        }
        int dist = Mathf.Abs(correl.Length / 2 - nextMaxInd);
        if (nextMaxInd == -1)
            return 0;
        return (AudioSettings.outputSampleRate / (float)dist);
    }
	
	
}
