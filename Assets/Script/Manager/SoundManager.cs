using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum SoundType
{
    None,
    A,
    B
}

[System.Serializable]
public class Sound
{
    public SoundType type;
    public AudioClip audioClip;
}

public class SoundManager : Singleton<SoundManager>
{
    public List<Sound> audioList;

    private Dictionary<SoundType, AudioClip> audios = new Dictionary<SoundType, AudioClip>();
    private AudioSource audioSource;
    public AudioSource bgmSource; // 배경음악 AudioSource Lee 추가 2024.10.22
    public AudioSource sfxSource; // 효과음 AudioSource Lee 추가 2024.10.22
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        foreach (var sound in audioList)
        {
            if (!audios.ContainsKey(sound.type))
            {
                audios.Add(sound.type, sound.audioClip);
            }
        }
    }

    public void PlaySound(SoundType type)
    {
        if (audios.ContainsKey(type))
        {
            audioSource.PlayOneShot(audios[type]);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + type);
        }
    }

    public void PlayRandomSound(SoundType[] soundTypes) // 입력된 타입 중에서 무작위로 사운드를 재생
    {
        if (soundTypes.Length == 0)
        {
            Debug.LogWarning("No SoundType provided");
            return;
        }

        // 무작위로 SoundType 선택
        SoundType randomType = soundTypes[UnityEngine.Random.Range(0, soundTypes.Length)];

        // 선택된 SoundType의 사운드 재생
        PlaySound(randomType);
    }
    
    public void SetBGMVolume(float volume) //Lee 추가 2024.10.22
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }
    
    public void SetSFXVolume(float volume) //Lee 추가 2024.10.22
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}
