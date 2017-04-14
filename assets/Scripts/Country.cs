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
				LaunchImmigrant (GameManager.instance.RequestRandomWorldPos(), immigrantQ.Dequeue());// the Immigrant we are launching);
				timeSinceLastLaunch = 0;
			}
			else
				timeSinceLastLaunch += Time.deltaTime;
		}
	}

	//Triggered by player, when colliding with retrievalZone's collider and presing the appropiate button
	void TakeImmigrant(Immigrant immigrant)
	{
		//check before if the immigrant belongs to this country or is wild card
		immigrantsAdquired++;
		if (immigrantsAdquired == maxImmigrants)
			adquiredAllImmigrants = true;

		immigrantQ.Enqueue (immigrant);
		immigrant.GetComponent<Collider> ().enabled = false;
		//LaunchImmigrant(immigrantStoreZone.position, immigrant)
	}

	public void LaunchImmigrant(Vector3 target, Immigrant immigrant) 
	{
		LaunchData launchData = CalculateTrajectory(target, immigrant);//launchHeight: the starting height of the parabolic motion

		//decide whether the immigrant is heading to the store zone or to the map, allow movement based on that
		bool canMoveAfterLanding = Vector3.Distance (target, immigrantStoreZone.position) < 1f ? false : true;

		//immigrant manages his own velocity, we pass the calculated data
		immigrant.Launch (launchData.initialVelocity, launchData.timeToTarget, canMoveAfterLanding);
	}

	LaunchData CalculateTrajectory(Vector3 target, Immigrant immigrant)
	{
		float gravity = -18f; //custom gravity value
		Physics.gravity = Vector3.up * gravity;
		float h = 5f; //h is the highest point in the parabolic motion

		float displacementY = target.y - immigrant.transform.position.y;
		Vector3 displacementXZ = new Vector3 (target.x - immigrant.transform.position.x, 0, target.z - immigrant.transform.position.z);

		float time = Mathf.Sqrt (-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h)/ gravity);
		Vector3 velocityY = Vector3.up * Mathf.Sqrt (-2 * gravity * h);
		Vector3 velocityXZ = displacementXZ / time; 

		return new LaunchData (velocityXZ + velocityY * - Mathf.Sign(gravity), time);
	}

	void InitializeImmigrants()
	{
		immigrantQ.Clear ();
		for (int i = 0; i < maxImmigrants; i++)
		{
			GameObject instance = Instantiate (immigratePrefab, Vector3.zero, Quaternion.identity);
		
			float compensationY = instance.GetComponent<Collider> ().bounds.extents.y; //compensate for the height of the mesh
			float compensationZ = instance.GetComponent<Collider> ().bounds.size.z * 1.5f; //place them next to each other

			Vector3 offset = Vector3.up*compensationY + Vector3.forward * compensationZ * i;
			instance.transform.position = immigrantStoreZone.position + offset;
			instance.transform.LookAt(Vector3.zero); //immigrants spawn looking at the map center
			instance.GetComponent<Collider>().enabled = false; //disable colliders so they don't push each other at spawn

			immigrantQ.Enqueue (instance.GetComponent<Immigrant>());
		}
		adquiredAllImmigrants = false;
	}

	struct LaunchData
	{
		public readonly Vector2 initialVelocity;
		public readonly float timeToTarget;

		public LaunchData (Vector3 u, float t)
		{
			initialVelocity = u;
			timeToTarget = t;
		}
	}
}
