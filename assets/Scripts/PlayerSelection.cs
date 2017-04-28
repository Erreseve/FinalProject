using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerSelection : MonoBehaviour 
{
    public GameObject playerSelScreen;
    public PlayerController[] players;
    public Material[] playerMats;

    const string horizontalInputAxis = "Horizontal_P"; //name of the general horizontal input 
    const string verticalInputAxis = "Vertical_P"; //name of the general vertical input axis 
    const string actionInputAxis = "Action_P"; //name of the general action input axis 

    public bool[] playersJoinedGame;
    bool onMenu;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playersJoinedGame = new bool[4];
        onMenu = true;
    }

    private void Update()
    {
        if (playerSelScreen !=null)
        {
            if (playerSelScreen.activeSelf && onMenu)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Input.GetButtonDown(actionInputAxis + (i + 1).ToString()))
                    {
                        Debug.Log(Input.GetButtonDown(actionInputAxis + (i + 1).ToString()));
                        players[i].gameObject.SetActive(true);
                        playersJoinedGame[i] = true; //player has joined game
                    }
                }
            }
            else if (!playerSelScreen.activeSelf && onMenu)
            {
                for (int i = 0; i < 4; i++)
                {
                    playersJoinedGame[i] = false; //quitting the selection screen resets player selection
                    players[i].gameObject.SetActive(false);
                }
                
            } 
        }
    }

    public void BeginPlay()
    {
        onMenu = false;
        SceneManager.LoadScene("Level");   
    }

}