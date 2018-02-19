using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioplay : MonoBehaviour {


    public AudioClip idle;
    public AudioClip happy;
    public AudioClip sad;
    public AudioClip fear;
    public AudioClip disgust;
    public AudioClip anger;


    public void playIdle()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = idle;
        play();
    }

    public void playHappy()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = happy;
        play();
    }

    public void PlaySad()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = sad;
        play();
    }

    public void PlayFear()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = fear;
        play();
    }


    public void playDisgust()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = disgust;
        play();
    }

    public void playAnger()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = anger;
        play();
    }


    private void play() {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
        audio.Play(44100);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
