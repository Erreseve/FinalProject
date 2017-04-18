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

	void Awake()
	{
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
        Debug.Log(name + "taking immigrant...");
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
        return Vector3.zero;
    }

	void InitializeImmigrants()
	{
		immigrantQ.Clear ();
		for (int i = 0; i < maxImmigrants; i++)
		{
			GameObject instance = Instantiate (immigratePrefab, Vector3.zero, Quaternion.identity);

            float compensationY = instance.GetComponent<Collider> ().bounds.extents.y ; //compensate for the height of the mesh
			float compensationZ = instance.GetComponent<Collider> ().bounds.size.z * 1.5f; //place them next to each other

			Vector3 offset = Vector3.up*compensationY + Vector3.forward * compensationZ * i;
			instance.transform.position = immigrantStoreZone.position + offset;
			instance.transform.LookAt(Vector3.zero); //immigrants spawn looking at the map center
			instance.GetComponent<Collider>().enabled = false; //disable colliders so they don't push each other at spawn

			immigrantQ.Enqueue (instance.GetComponent<Immigrant>());
		}
		adquiredAllImmigrants = false;
	}
}
