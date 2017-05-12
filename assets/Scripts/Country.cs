using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Country : MonoBehaviour {

	[SerializeField]
	int maxImmigrants = 6; 
	[SerializeField]
	float timeBetweenImmigrantsLaunches = 2.5f;
	[SerializeField]
	Transform retrievalZone; 
	[SerializeField]
	Transform immigrantStoreZone;
	[SerializeField]
	GameObject immigratePrefab; //containing the visuals and the instance of Immigrant

	public Queue<Immigrant> immigrantQ; //holds all the immigrants the country will have inside it
	float timeSinceLastLaunch;
	bool adquiredAllImmigrants = false; //a country only throws immigrants once they have all been recolected
	int immigrantsAdquired;
    Collider storeZoneCollider;
    Vector3 leftTopMostPoint;

    const float separation = .6f; //how far apart are we spawning the immigrants 

    void Awake()
	{
        storeZoneCollider = immigrantStoreZone.gameObject.GetComponent<Collider>();
        //store the lefttopmost zone of the collider (notice the walkable zone of the navmesh is slightly smaller)
        leftTopMostPoint = immigrantStoreZone.position - Vector3.right * storeZoneCollider.bounds.extents.x + Vector3.forward * storeZoneCollider.bounds.extents.z;

        immigrantQ = new Queue<Immigrant> ();
		immigrantsAdquired = 0;
		InitializeImmigrants ();
    }

	void Update()
	{
		//launch condition
		if (adquiredAllImmigrants)
		{
			if (timeSinceLastLaunch >= timeBetweenImmigrantsLaunches && immigrantQ.Count > 0)
			{
                GiveImmigrant();
				timeSinceLastLaunch = 0;
			}
			else
				timeSinceLastLaunch += Time.deltaTime;
		}
	}

	//Triggered by player, when colliding with retrievalZone's collider and presing the appropiate button
	public void TakeImmigrant(Immigrant immigrant)
	{
		immigrantsAdquired++;
		if (immigrantsAdquired == maxImmigrants)
			adquiredAllImmigrants = true;

		immigrantQ.Enqueue (immigrant);
        
	}

    public void GiveImmigrant()
    {
        if (immigrantQ.Count > 0)
        {
            Immigrant immigrant = immigrantQ.Dequeue();
            immigrant.SetDestinationOnField();
        }
    }

    public Vector3 RequestRandomCountryPosition()
    {
        float offsetZ = Random.Range(0, storeZoneCollider.bounds.size.z - separation * 2);
        float offsetX = Random.Range(0, storeZoneCollider.bounds.size.x - separation * 2);

        Vector3 returnWorldPoint = leftTopMostPoint + Vector3.right * offsetX + Vector3.back * offsetZ;

        return returnWorldPoint;
    }

	void InitializeImmigrants()
	{
        Collider instanceColl = immigratePrefab.GetComponent<Collider>();
        if (instanceColl == null)
            instanceColl = immigratePrefab.GetComponentInChildren<Collider>();

        float compensationY = instanceColl.bounds.extents.y; //compensate for the height of the mesh
        float compensationX = separation;
        float compensationZ = 0; //place them below each other in the store zone

        int immigrantsPerColumn = Mathf.FloorToInt((storeZoneCollider.bounds.size.z - 2 * separation) / separation);
        int immigrantsPlacedInCurrentColumn = 0;

        immigrantQ.Clear ();
		for (int i = 1; i <= maxImmigrants; i++)
		{
			GameObject instance = Instantiate (immigratePrefab, Vector3.zero, Quaternion.identity);

            immigrantsPlacedInCurrentColumn++; 

            //align immigrants in the top of a new row if they no longer fit in the column
            if ( immigrantsPlacedInCurrentColumn / immigrantsPerColumn > 0 )
            {
                compensationX += separation;
                compensationZ = - separation;
                immigrantsPlacedInCurrentColumn = 0;
            }
            else
                compensationZ -= separation;

            Vector3 offset = new Vector3(compensationX, compensationY, compensationZ);
			instance.transform.position = leftTopMostPoint + offset;
			instance.transform.LookAt(Vector3.zero); //immigrants spawn looking at the map center

			immigrantQ.Enqueue (instance.GetComponent<Immigrant>());
		}
		adquiredAllImmigrants = false;

        Invoke("AllowImmigrantsToStartMoving", 1f);
	}

    //immigrants are all set in formation and enough time has passed to see it, they can start moving
    void AllowImmigrantsToStartMoving()
    {
        foreach (Immigrant i in immigrantQ)
        {
            i.justSpawned = false;
        }
    }

    private void OnDrawGizmos()
    {
        //leftmost point inside the country 
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(leftTopMostPoint, Vector3.one * .3f);

        //retrieval zone
        Gizmos.color = Color.green;
        Collider temp = retrievalZone.GetComponent<Collider>();
        Gizmos.DrawWireCube(retrievalZone.position, new Vector3(temp.bounds.size.x, temp.bounds.size.y, temp.bounds.size.z));
    }
}
