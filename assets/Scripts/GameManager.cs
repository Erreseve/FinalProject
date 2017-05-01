using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Timers;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (MapGrid))]
public class GameManager : MonoBehaviour {

	MapGrid mapGrid;

    [Header("Round Settings")]
    public float matchTime; //[10, 120] in seconds
    public float maxTimeRangeAmbassadorSpawn; //min range value is 5 seconds
	
    [Header("UI References")]
    public Text timeText;
    public Text winnerAnnouncementText;
    public Image fadePlane;
    public Text[] playerScoresUI;
    public Button goBackToMenuButton;

    [Header("Object References")]
    public GameObject ONUAmbassadorPrefab;
    public Country[] countries;
    public GameObject[] playerPrefabs;
    

	public static GameManager instance = null;

    float timeToSpawnONUAmbassador;
    bool ONUAmbassadorSpawned;
    bool matchEnded;
    PlayerSelection selectionManager;
    PlayerController[] players; //contains the controllers to all active players or null if not

    void Awake()
	{
		mapGrid = GetComponent<MapGrid> ();
		//GameManager Simpleton
		if (instance == null)
			instance = this;
		else
			Destroy (gameObject);

        //spawn players
        selectionManager = GameObject.Find("Player Selection Manager").GetComponent<PlayerSelection>();
        players = new PlayerController[4];

        for (int i = 0; i<4; i++)
        {
            if (selectionManager.playersJoinedGame[i])
            {
                PlayerController newPlayer = Instantiate(playerPrefabs[i]).GetComponent<PlayerController>();
                newPlayer.gameObject.SetActive(true);
                newPlayer.enabled = true;

                Collider newPlayerCollider = newPlayer.GetComponent<Collider>();

                newPlayer.transform.localScale = Vector3.one;

                Vector3 spawnPosition = new Vector3(-4f + 2 * i, newPlayerCollider.bounds.extents.y, 0);
                newPlayer.transform.position = spawnPosition;
                newPlayer.transform.LookAt(newPlayer.transform.position + Vector3.back * 1f);

                players[i] = newPlayer;
            }
            else
                players[i] = null;
        }
	} 

	void Start()
	{
        //select when will the ambassador will spawn
        timeToSpawnONUAmbassador = Random.Range(5, maxTimeRangeAmbassadorSpawn);
        //spawn waves of immigrants
		StartCoroutine (SpawnInitialImmigrants());
        //make cursor invisible
        Cursor.visible = false;
	}

    void Update()
    {
        if (!matchEnded)
        {
            //MATCH TIME
            matchTime -= Time.deltaTime;

            int minutes = Mathf.RoundToInt(Mathf.Floor(matchTime / 60));
            float seconds = Mathf.RoundToInt(matchTime % 60);

            if (matchTime <= 0.0)
                StartCoroutine(GameOver(Color.clear, new Color(0, 0, 0, 1f), 1.5f));

            timeText.text = minutes.ToString() + ":" + seconds.ToString();

            //SCORES
            for (int i = 0; i < 4; i++)
            {
                if (selectionManager.playersJoinedGame[i])
                {
                    playerScoresUI[i].text = players[i].score.ToString();
                }
                else
                {
                    playerScoresUI[i].text = "";
                }
            }
        }
        else
            timeText.text = "00:00";
    }

    private void LateUpdate()
    {
        //just spawn one ambassador at the specified time
        if (matchTime <= timeToSpawnONUAmbassador && !ONUAmbassadorSpawned)
        {
            Instantiate(ONUAmbassadorPrefab); //ambassador controlls his spawn position
            ONUAmbassadorSpawned = true;
        }
    }

    IEnumerator GameOver(Color from, Color to, float time)
    {
        matchEnded = true;

        //freeze units and display game over
        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0;
        winnerAnnouncementText.text = "GAME OVER";
        //PLAY GAME OVER ANNOUNCER AUDIO CLIP

        //PLAY GAME OVER MUSIC

        //fade to black screen
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += .016f * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }

        Time.timeScale = prevTimeScale;

        #region Disable Elements and Place Players

        //make cursor visible, UI elements invisible
        Cursor.visible = true;
        foreach (Text t in playerScoresUI)
        {
            t.enabled = false;
        }
        timeText.enabled = false;

        //destroy all immigrants in scene
        GameObject[] immigrantsInScene = GameObject.FindGameObjectsWithTag("Immigrant");
        foreach (GameObject i in immigrantsInScene)
        {
            Destroy(i);
        }

        //destroy embassador if he wasnt grabbed by any player
        GameObject ambassador = GameObject.FindGameObjectWithTag("ONUAmbassador");
        if (ambassador != null)
            ambassador.GetComponent<ONUAmbassador>().GrabbedByPlayer();

        //place all active players back at spawn positions
        for (int i = 0; i < 4; i++)
        {
            if (selectionManager.playersJoinedGame[i])
            {
                players[i].enabled = false; //player wont be able to move

                Collider newPlayerCollider = players[i].gameObject.GetComponent<Collider>();

                Vector3 newPosition = new Vector3(-4f + 2 * i, newPlayerCollider.bounds.extents.y, 0);
                players[i].transform.position = newPosition;
                players[i].transform.LookAt(players[i].transform.position + Vector3.back * 1f);
            }
        }
        #endregion

        int winnerIndex = GetMatchWinnerIndex();
        winnerAnnouncementText.text = "JUGADOR " + (winnerIndex + 1).ToString() + " GANA!!!\nPUNTAJE: " + players[winnerIndex].score.ToString();

        //fade to clear screen
        speed = 1 / time;
        percent = 0;

        while (percent < 1)
        {
            percent += .016f * speed;
            fadePlane.color = Color.Lerp(to, from, percent);
            yield return null;
        }

        yield return new WaitForSeconds(1f);   

        //ZOOM CAMERA ANIMATION
        //ACTIVATE WINNER CELEBRATION ANIMATION

        goBackToMenuButton.gameObject.SetActive(true);
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    int GetMatchWinnerIndex()
    {
        int maxScore = int.MinValue;
        int matchWinnerIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            if (players[i] != null) //only check active players
            {
                if (players[i].score > maxScore)
                {
                    maxScore = players[i].score;
                    matchWinnerIndex = i;
                }
            }
        }
        return matchWinnerIndex;
    }

	IEnumerator SpawnInitialImmigrants()
	{
		float timeBetweenWaves = 1f;
		int[] immigrantsInWave = new int[] {1, 2, 2};

        yield return new WaitForSeconds(1f);

		for (int wave = 0; wave < immigrantsInWave.Length; wave++) //repeat for all waves
		{
			for (int immigrantsSpawnedInWave = 0; immigrantsSpawnedInWave < immigrantsInWave[wave]; immigrantsSpawnedInWave++) 
			{
				for (int countryIndex = 0; countryIndex < countries.Length ; countryIndex++ ) //launch 1 for all countries
				{
                    countries[countryIndex].GiveImmigrant();

					yield return new WaitForSeconds (.1f);
				}
				 //time between immigrants of same wave
				yield return new WaitForSeconds(.2f);
			}

			yield return new WaitForSeconds(timeBetweenWaves);
		}
	}

	public Vector3 RequestRandomWorldPos()
	{
		return mapGrid.PickRandomGridPos ();
	}

    private void OnValidate()
    {
        if (matchTime < 10)
            matchTime = 10;
        if (matchTime > 120)
            matchTime = 120;

        if (maxTimeRangeAmbassadorSpawn < 10)
            maxTimeRangeAmbassadorSpawn = 10;
        if (maxTimeRangeAmbassadorSpawn > matchTime)
            maxTimeRangeAmbassadorSpawn = matchTime-1;
    }
}
