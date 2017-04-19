using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Timers;

[RequireComponent (typeof (MapGrid))]
public class GameManager : MonoBehaviour {

	MapGrid mapGrid;
	[SerializeField]
	Country[] countries;

    public float matchTime = 30; //secondes

    public Text timeText;
    public GameObject[] playerPrefabs;
    public GameObject ONUAmbassadorPrefab;

	public static GameManager instance = null;

    float timeToSpawnONUAmbassador;
    bool ONUAmbassadorSpawned;

	void Awake()
	{
		mapGrid = GetComponent<MapGrid> ();
		//GameManager Simpleton
		if (instance == null)
			instance = this;
		else
			Destroy (gameObject);

        PlayerSelection selectionManager = GameObject.Find("Player Selection Manager").GetComponent<PlayerSelection>();
        
        for (int i = 0; i<4; i++)
        {
            if(selectionManager.playersJoinedGame[i])
            {
                PlayerController newPlayer = Instantiate(playerPrefabs[i]).GetComponent<PlayerController>();
                newPlayer.gameObject.SetActive(true);
                newPlayer.enabled = true;
     
                Collider newPlayerCollider = newPlayer.GetComponent<Collider>();

                newPlayer.transform.localScale = Vector3.one;

                Vector3 spawnPosition = new Vector3(-4f + 2*i, newPlayerCollider.bounds.extents.y, 0);
                newPlayer.transform.position = spawnPosition;
                newPlayer.transform.LookAt(newPlayer.transform.position + Vector3.back * 1f);
                
            }
        }
	}

	void Start()
	{
        timeToSpawnONUAmbassador = Random.Range(5, matchTime);
		StartCoroutine (SpawnInitialImmigrants());
	}

    void Update()
    {
        matchTime -= Time.deltaTime;

        int minutes = Mathf.RoundToInt(Mathf.Floor(matchTime / 60));
        float seconds = Mathf.RoundToInt(matchTime % 60);

        if (matchTime <= 0.0)
            MatchEnded();

        timeText.text = minutes.ToString() + ":"+ seconds.ToString();   
    }

    private void LateUpdate()
    {
        if (matchTime <= timeToSpawnONUAmbassador && !ONUAmbassadorSpawned)
        {
            Instantiate(ONUAmbassadorPrefab);
            ONUAmbassadorSpawned = true;
        }
    }

    void MatchEnded()
    {
        Application.Quit();
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
}
