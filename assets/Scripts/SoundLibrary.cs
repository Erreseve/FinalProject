using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SoundLibrary : MonoBehaviour 
{
    //this class will contain variants of sounds for many purposes
    //i.e. different foot steps, immigrants entry dialogues, etc

    public SoundGroup[] soundGroups;
    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();    

    void Awake()
    {
        foreach (SoundGroup sg in soundGroups) //store groups in a dictionary for better management
        {
            groupDictionary.Add(sg.groupID, sg.group);
        }
    }

    public AudioClip GetClipFromName (string name)
    {
        if (groupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds = groupDictionary[name];
            return sounds[Random.Range(0, sounds.Length)]; //return a random sound 
        }
        else
            return null; //if the sound type doesn't exists
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupID; //name of the type of sound
        public AudioClip[] group; //contains variants of the sound type
    }

}