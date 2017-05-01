using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour 
{
    public enum AudioChannel { Master, Sfx, Music};

    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    AudioSource sfx2DSource;
    AudioSource[] musicSources; //create a couple audio sources to cross fade
    int activeMusicSourceIndex; //which audio source has music currently playing

    public static AudioManager instance;
    SoundLibrary library;

    void Awake()
    {
        if (instance != null) //singleton, avoid duplicates
            Destroy(gameObject);
        else
        {
            instance = this;
            library = GetComponent<SoundLibrary>();

            DontDestroyOnLoad(gameObject); //audio manager should persist through scene changes

            //create audio sources and parent them to this object
            musicSources = new AudioSource[2];
            for (int i = 0; i < musicSources.Length; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1).ToString());
                newMusicSource.transform.parent = transform;
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            }

            //create another audio source for playing 2D sounds (they have no position so the volume isn't affected by the distance to the listener)
            GameObject newSfxSource = new GameObject("SFX Source");
            newSfxSource.transform.parent = transform;
            sfx2DSource = newSfxSource.AddComponent<AudioSource>();

            //retrieve previous volume values
            //if a preference value hasn't been set, the method overload returns the default value
            masterVolumePercent =  PlayerPrefs.GetFloat("Master Volume", 1f);
            musicVolumePercent = PlayerPrefs.GetFloat("Music Volume", 1f);
            sfxVolumePercent = PlayerPrefs.GetFloat("SFX Volume", 1f); 

        }
        
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        volumePercent = Mathf.Clamp01(volumePercent);
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
        }
        //update music sources' volume
        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        //save values for future game sessions
        PlayerPrefs.SetFloat("Master Volume", masterVolumePercent);
        PlayerPrefs.SetFloat("Music Volume", musicVolumePercent);
        PlayerPrefs.SetFloat("SFX Volume", sfxVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = .7f)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //alternate between music sources
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));

    }

    //play a specific clip
    public void PlaySound (AudioClip clip, Vector3 pos) //so all sfx are played by the manager, volume preferences can be considered
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }

    //play a random clip from a type
    public void PlaySound (string soundName /*the type*/, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    //play a 2D sound
    public void PlaySound (string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }
    IEnumerator AnimateMusicCrossFade(float fadeDuration)
    {
        float percent = 0; 

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent); //one source's volume goes up
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent); //the other down
            yield return null;
        }
        musicSources[1 - activeMusicSourceIndex].Stop(); //stop secondary music source to avoid overlaps later
    }
}