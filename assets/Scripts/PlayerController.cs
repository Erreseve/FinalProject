using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float movementSpeed = 10f; 
	public float speedReduction = 3f;
	public Transform immigrantHoldPosition;
	Vector3 moveDir;
	Vector3 prevMoveDir;
	Rigidbody rb;
	bool carryingImmigrant;
	Immigrant immigrantCarried; 
	float immigrantReleaseCooldown = 1f;
    float timeSinceRelease;

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
		carryingImmigrant = false;
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space))
		{
			if (carryingImmigrant)
			{
				immigrantCarried.ReleasedByPlayer (false);
				timeSinceRelease = 0;
				immigrantCarried = null;
				carryingImmigrant = false;
			}
		}
		timeSinceRelease += Time.deltaTime;
		timeSinceRelease = Mathf.Clamp(timeSinceRelease, 0, immigrantReleaseCooldown);

        //ensure the player can't be rotated by forces applied by immigrants
        rb.velocity = Vector3.zero; 
		rb.angularVelocity = Vector3.zero; 
        //note player needs to be kinematic
	}

	void OnCollisionStay (Collision collisionInfo)
	{
		if (collisionInfo.gameObject.tag == "Immigrant") //on collision with an immigrant, enable picking up
		{
			if (Input.GetKeyUp (KeyCode.Space) && !carryingImmigrant && timeSinceRelease >= immigrantReleaseCooldown) 
			{ //unless we are already carrying one or have recently released one
				Debug.Log ("Picked up immigrant!");
				carryingImmigrant = true;
				immigrantCarried = collisionInfo.gameObject.GetComponent < Immigrant> ();
				immigrantCarried.PickedByPlayer (immigrantHoldPosition);
			}
		}
	}

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Retrieval Zone") //Player is in a retrieval zone
        {
            Debug.Log("On " + other.transform.parent.name+ "'s retrieval zone...");
            if (Input.GetKeyUp(KeyCode.Space) && carryingImmigrant) //Player wants to drop the current immigrant
            {
                Country country = other.GetComponentInParent<Country>();
                Debug.Log("On trigger, releasing immigrant...");
                if (country.tag == immigrantCarried.country) //the immigrant belongs to that country
                {
                    country.SendMessage("TakeImmigrant", immigrantCarried); //make country add immigrant to its queue
                    immigrantCarried.ReleasedByPlayer(true);
                }
                else
                    immigrantCarried.ReleasedByPlayer(false);

                timeSinceRelease = 0;
                immigrantCarried = null;
                carryingImmigrant = false;
            }
        }
    }

    void FixedUpdate () 
	{
		moveDir = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		if (moveDir.sqrMagnitude > 1f)
			moveDir = moveDir.normalized;

		float targetSpeed = carryingImmigrant ? movementSpeed - speedReduction : movementSpeed;
		Vector3 velocity = moveDir * targetSpeed * Time.fixedDeltaTime;
		rb.MovePosition (transform.position  + velocity);

		if (moveDir != Vector3.zero)
			transform.forward = Vector3.Normalize(moveDir);
	}
}
