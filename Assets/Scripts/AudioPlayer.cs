﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

    public static AudioPlayer Instance;
    public AudioSource audioSource;
    [SerializeField]
    private AudioClip[] audios;

    //private static float vol = 1;

    void Awake()
    {
        Instance = this;
    }
    public void PlayAudio(int id)
    {
        audioSource.PlayOneShot(audios[id]);
    }
    public void PlayAudio(int id, float vol)
    {
        audioSource.PlayOneShot(audios[id], vol);
    }

}
