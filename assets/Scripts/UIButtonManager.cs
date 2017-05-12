using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonManager : MonoBehaviour 
{
    public float buttonHolderXEndPosition = -850f;
    public float timeToXEndPosition = .6f;
    public Transform buttonHolder;
    public Animator buttonHolderAnimator;

    public GameObject playerSelectionBackground;
    public GameObject menuBackground;
    public GameObject[] MenuLayouts;
    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths; //can't use Resolution type here as it doesn't show up in inspector

    int activeScreenResolutionIndex;

    void Start()
    {
        //Retrieve preferences

        //last active resolution
        activeScreenResolutionIndex = PlayerPrefs.GetInt("Screen Resolution Index", 2);
        for (int i =0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = i == activeScreenResolutionIndex; //turn on toggle for last resolution, turn off for the others
        }

        //fullscreen option
        bool isFullScreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1 ? true : false;
        fullscreenToggle.isOn = isFullScreen; //turn toggle on or off, this calls the method SetFullScreen() automatically

        //set slider values
        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;
    }

    public void ToggleOffAllMenusBut (int i)
    {
        if (i == 1) //To player selection screen, activate new background
        {
            playerSelectionBackground.SetActive(true);
            menuBackground.SetActive(false);
        }
        else
        {
            playerSelectionBackground.SetActive(false);
            menuBackground.SetActive(true);
        }


        OnButtonClicked(); //for sound
        foreach (GameObject menu in MenuLayouts)
        {
            menu.SetActive(false);
        }
        MenuLayouts[i].SetActive(true);

        AudioManager.instance.gameObject.GetComponent<MusicManager>().ChangeMenuMusic(); //every transition between menus changes music randomly
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn) //this particular resolution toggle has been enabled
        {
            activeScreenResolutionIndex = i;
            float aspectRatio = 16f / 9;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);

            //save active resolution in preferences for future game sessions
            PlayerPrefs.SetInt("Screen Resolution Index", activeScreenResolutionIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullscreen; //if fullscreen option enabled, disable interactivity for resolution toggles and viceversa
        }
        if (isFullscreen) //option to set fullscreen is on
        {
            Resolution[] allResolutions = Screen.resolutions; //all availables fullscreen resolutionss by the display
            Resolution maxResolution = allResolutions[allResolutions.Length - 1]; //last resolution is the highest, set that on fullscreen
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
            SetScreenResolution(activeScreenResolutionIndex); //return screen resolution back to its last state and enable its toggle

        //save fullscreen preference 
        PlayerPrefs.SetInt ("Fullscreen", isFullscreen ? 1: 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }

    public void GoToScene (string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void QuitGame()
    {
        OnButtonClicked();
        Application.Quit();
    }

    public void Initiate()
    {
        OnButtonClicked();
        buttonHolderAnimator.SetTrigger("TriggerButtonAnimations");
    }

    public void OnButtonClicked()
    {
        AudioManager.instance.PlaySound("ButtonClick");
    }

    public void OnButtonHover()
    {
        AudioManager.instance.PlaySound("ButtonHover");
    }
}