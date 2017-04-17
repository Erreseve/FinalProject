using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class Immigrant : MonoBehaviour {

    public bool freeToMove = false;
    public string country = "";

	Rigidbody rb;
	NavMeshAgent nav;
	Collider coll;
	Transform playerHold;

	bool followingPath;
	bool grabbed;

	void Awake () 
	{
		rb = GetComponent<Rigidbody> ();
		nav = GetComponent<NavMeshAgent> ();
		coll = GetComponent<Collider> ();
        rb.useGravity = true;
    }

	void Update()
	{
		if (freeToMove)
		{
			if (!followingPath)
			{
				nav.SetDestination (GameManager.instance.RequestRandomWorldPos ());
				followingPath = true;
			}
			else
			{
				if (Vector3.Distance (nav.destination, transform.position) < 1f)
				{
					followingPath = false;
				}
			}
		}
		else
		{
			if (nav.enabled)
				nav.ResetPath();
		}
	}

	void LateUpdate()
	{
		if (grabbed)
		{
			transform.position = playerHold.position;
		}
        else
        {
            transform.position = new Vector3(transform.position.x, 0 + coll.bounds.extents.y, transform.position.z);
        }
	}

	public void Launch(Vector3 v0, float time, bool canMoveAfterLanding)
	{
        rb.useGravity = true;
		rb.velocity = v0;
		GetComponent<Collider>().enabled = true;

		StartCoroutine (Restore(canMoveAfterLanding, time));
	}

	public void ReleasedByPlayer(bool releaseToRetrieve)
	{
		if (!releaseToRetrieve) //the immigrant was released to walk freely on the field again
		{
            StartCoroutine(Restore(true, .5f));
			coll.enabled = true;
		}
		playerHold = null;
		grabbed = false;
		followingPath = false;
	}

	public void PickedByPlayer(Transform playerHold)
	{
		this.playerHold = playerHold;
		freeToMove = false;
		grabbed = true;
		nav.ResetPath ();
        nav.enabled = false;
		coll.enabled = false;
	}

	IEnumerator Restore(bool canMoveAfterLanding, float time)
	{
		//wait for the trayectory to end to restore control to the immigrant
		yield return new WaitForSeconds (time);

		transform.LookAt(Vector3.zero); //look at map center after landing
		rb.velocity = Vector3.zero;
		nav.enabled = true;
		freeToMove = canMoveAfterLanding;
	}
}
