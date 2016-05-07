using UnityEngine;
using System.Collections;

public class MusicPlaylist : MonoBehaviour
{
    public bool ActivateOnAwake = true;
    public AudioClip[] MusicList;

    void Awake()
    {
        if (ActivateOnAwake && MusicManager.Instance)
            MusicManager.Instance.ChangePlaylist(this);
    }

    void Start()
    {
        
    }

}