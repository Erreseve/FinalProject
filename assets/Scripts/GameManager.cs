using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MapGrid))]
public class GameManager : MonoBehaviour {

	MapGrid mapGrid;
	[SerializeField]
	Country[] countries;

	public static GameManager instance = null; 

	void Awake()
	{
		mapGrid = GetComponent<MapGrid> ();
		//GameManager Simpleton
		if (instance == null)
			instance = this;
		else
			Destroy (gameObject);
	}

	void Start()
	{
		StartCoroutine (SpawnInitialImmigrants());
	}

	IEnumerator SpawnInitialImmigrants()
	{
		float timeBetweenWaves = 1f;
		int[] immigrantsInWave = new int[] {1, 2 ,3 };

		for (int wave = 0; wave < immigrantsInWave.Length; wave++) //repeat for all waves
		{
			for (int immigrantsSpawnedInWave = 0; immigrantsSpawnedInWave < immigrantsInWave[wave]; immigrantsSpawnedInWave++) 
			{
				for (int countryIndex = 0; countryIndex < countries.Length ; countryIndex++ ) //launch 1 for all countries
				{
					countries [countryIndex].LaunchImmigrant( RequestRandomWorldPos(), countries [countryIndex].immigrantQ.Dequeue()); //launch

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
