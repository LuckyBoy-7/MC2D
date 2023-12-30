using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float speed = 1)
    {
        audioSource.PlayOneShot(clip);
        audioSource.pitch = speed;
    }
    
    public void Play(AudioClip[] clips, float speed = 1)
    {
        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
        audioSource.pitch = speed;
    }


    public AudioSource GetAudioSource(AudioClip clip, float speed = 1)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.pitch = speed;
        source.loop = true;
        source.clip = clip;
        source.Play();

        return source;
    }

    public AudioSource GetAudioSource(AudioClip[] clips, float speed = 1)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.pitch = speed;
        source.loop = true;
        StartCoroutine(ChangeClipRandomly(source, clips));
        source.Play();

        return source;
    }

    private IEnumerator ChangeClipRandomly(AudioSource source, AudioClip[] clips)
    {
        while (true)
        {
            if (source == null) // 说明被delete了
                yield break;
            // Debug.Log(source.isPlaying);
            var clip = clips[Random.Range(0, clips.Length)];
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length / source.pitch);
        }
    }
}