using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrancePhilip : MonoBehaviour
{
    [SerializeField] private AudioSource fart;
    public AudioSource funnySong;
    public AudioSource funnySong2;
    [SerializeField] private TerrancePhilip terrancePhilip;
    public bool isPlayed;

    public void FartSound()
    {
        if (terrancePhilip.isPlayed && isPlayed)
        {
            if (funnySong2.isPlaying)
            {
                funnySong2.Stop();
            }
            funnySong.Play();
            isPlayed = false;
            terrancePhilip.isPlayed = false;
        }
        else
        {
            if (funnySong2.isPlaying || terrancePhilip.funnySong2.isPlaying)
            {
                funnySong2.Stop();
                terrancePhilip.funnySong2.Stop();
            }
            if (funnySong.isPlaying)
            {
                funnySong.Stop();
            }
            if (terrancePhilip.funnySong.isPlaying)
            {
                terrancePhilip.funnySong.Play();
            }
            fart.Play();
            isPlayed = true;
        }
        
    }

    public void KyleMomSong()
    {
        if (fart.isPlaying)
        {
            fart.Stop();
        }
        if (terrancePhilip.fart.isPlaying)
        {
            terrancePhilip.fart.Stop();
        }
        if (funnySong.isPlaying || terrancePhilip.funnySong.isPlaying)
        {
            funnySong.Stop();
            terrancePhilip.funnySong.Stop();
        }
        funnySong2.Play();
    }
}
