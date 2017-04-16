using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour 
{
    public AudioClip[] menuThemes;
    int currentThemeIndex;

    bool OnMenuScene;
    int lastMenuThemeIndex;

    void Start()
    {
        OnMenuScene = true; //the first scene is always the main menu
        currentThemeIndex = 0;
        AudioManager.instance.PlayMusic(menuThemes[0], 2); //start with first theme
        lastMenuThemeIndex = 0;
    }

    public void ChangeMenuMusic()
    {
        Random.InitState((int)System.DateTime.Now.Ticks); //change the Random object seed
        int randomIndex = Random.Range(0, menuThemes.Length);
        while (randomIndex == lastMenuThemeIndex && menuThemes.Length > 1) //ensure the new music isn't the one being played
        { //if there is only one menu theme track, it will always get picked, so check for that too
            randomIndex = Random.Range(0, menuThemes.Length);
        }

        AudioClip randomMenuTheme = menuThemes[randomIndex];
        lastMenuThemeIndex = randomIndex;
        AudioManager.instance.PlayMusic(randomMenuTheme, 1.5f);
    }
}