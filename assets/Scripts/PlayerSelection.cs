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
                        DontDestroyOnLoad(players[i].gameObject);
                        players[i].gameObject.SetActive(true);
                        playersJoinedGame[i] = true; //player has joined game
                    }
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