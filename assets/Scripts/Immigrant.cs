using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class Immigrant : MonoBehaviour {

    public string country = "";
    Country countryBelongedTo;

	Rigidbody rb;
	NavMeshAgent nav;
	Collider coll;
	Transform playerHold;

	bool followingPath;
	bool grabbedByPlayer;
    bool onCountry;

	void Awake () 
	{
		rb = GetComponent<Rigidbody> ();
		nav = GetComponent<NavMeshAgent> ();
		coll = GetComponent<Collider> ();

        GameObject tempObjRef = GameObject.FindWithTag(country);
        if (tempObjRef != null) //if the country exists (which it always should)
            countryBelongedTo = tempObjRef.GetComponent<Country>();

        rb.useGravity = true; //gravity should always be on for the immigrant
        nav.autoTraverseOffMeshLink = true; //so the immigrant can jump from and to the country
    }

	void Update()
	{
		if (!grabbedByPlayer) //simulate immigrant's wandering
		{
			if (!followingPath) //if we don't a destination, get one
			{
                if (onCountry) //request only paths within country navmesh
                    SetDestinationOnCountry();
                else //request only paths within field navmesh
                    SetDestinationOnField();    
			}
			else
			{
				if (Vector3.Distance (nav.destination, transform.position) < 1f) //pick another waypoint if too close to the current one
				{
					followingPath = false;
				}
			}
		}
		else
		{
			if (nav.enabled) //we are not to be looking for a path when grabbed by player
				nav.ResetPath();
		}
	}

	void LateUpdate()
	{
		if (grabbedByPlayer) //keep immigrant at where the player is holding him
		{
			transform.position = playerHold.position;
		}
        else if (!onCountry) //if immigrant isn't grabbed by player but is in scenario, make sure he is always ground leveled
        {
            transform.position = new Vector3(transform.position.x, 0 + coll.bounds.extents.y, transform.position.z);
        }
	}


    public void SetDestinationOnCountry()
    {
        followingPath = true; 
        onCountry = true;
        nav.SetDestination(countryBelongedTo.RequestRandomCountryPosition());
    }

    public void SetDestinationOnField()
    {
        followingPath = true;
        onCountry = false;
        nav.SetDestination(GameManager.instance.RequestRandomWorldPos());
    }

	public void PickedByPlayer(Transform playerHold)
	{
		this.playerHold = playerHold;
		grabbedByPlayer = true;
		nav.ResetPath ();

        //so immigrant doesn't push player while being hold, only place where collider is deactivated
        coll.enabled = false; 
	}

    public void ReleasedByPlayer(bool releasedOnCountry)
    {
        coll.enabled = true;
        playerHold = null;
        grabbedByPlayer = false;
        followingPath = false;

        if (releasedOnCountry)
            onCountry = true;

    }
}
