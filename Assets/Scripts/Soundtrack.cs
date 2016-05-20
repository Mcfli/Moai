using UnityEngine;
using System.Collections;

public class Soundtrack : MonoBehaviour
{

    private static Soundtrack instance = null;
    public AudioClip[] soundtracks;
    private int i;

    public static Soundtrack Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        i = Random.Range(0, soundtracks.Length);
        StartCoroutine("Playlist");
    }

    IEnumerator Playlist()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (!GetComponent<AudioSource>().isPlaying)
            {
                if (i != (soundtracks.Length - 1))
                {
                    i++;
                    
                    GetComponent<AudioSource>().clip = soundtracks[i];
                    GetComponent<AudioSource>().Play();
                }
                else
                {
                    i = 0;
                    GetComponent<AudioSource>().clip = soundtracks[i];
                    GetComponent<AudioSource>().Play();
                }
            }
        }
    }
}