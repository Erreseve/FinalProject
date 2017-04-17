using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float movementSpeed = 10f; 
	public float speedReduction = 3f;

    //each axis gets added the playerID number when utilized
    const string horizontalInputAxis = "Horizontal_P"; //name of the general horizontal input 
    const string verticalInputAxis = "Vertical_P"; //name of the general vertical input axis 
    const string actionInputAxis = "Action_P"; //name of the general action input axis 

    [HideInInspector]
    public int playerID; //the number of the player (from 1 to 4)
	public Transform immigrantHoldPosition; //the position where the player will hold the immigrant when grabbed

	Vector3 moveDir;
	Rigidbody rb;
	bool carryingImmigrant;
	Immigrant immigrantCarried; 
	float immigrantReleaseCooldown = 1f;
    float timeSinceAction;

	void Start()
	{
        playerID = 1; //remove
		rb = GetComponent<Rigidbody> ();
		carryingImmigrant = false;
	}

	void Update()
	{
		if (Input.GetButtonDown (actionInputAxis + playerID.ToString()) && timeSinceAction >= immigrantReleaseCooldown)
		{
			if (carryingImmigrant)
			{
				immigrantCarried.ReleasedByPlayer (false);
				timeSinceAction = 0;
				immigrantCarried = null;
				carryingImmigrant = false;
			}
		}
		timeSinceAction += Time.deltaTime;
		timeSinceAction = Mathf.Clamp(timeSinceAction, 0, immigrantReleaseCooldown);

        //ensure the player can't be rotated by forces applied by immigrants
        rb.velocity = Vector3.zero; 
		rb.angularVelocity = Vector3.zero; 
        //note player needs to be kinematic
	}

	void OnCollisionStay (Collision collisionInfo)
	{
		if (collisionInfo.gameObject.tag == "Immigrant") //on collision with an immigrant, enable picking up
		{
			if (Input.GetButtonDown (actionInputAxis + playerID.ToString()) && !carryingImmigrant && timeSinceAction >= immigrantReleaseCooldown) 
			{ //unless we are already carrying one or have recently released one
				Debug.Log ("Picked up immigrant!");
				carryingImmigrant = true;
				immigrantCarried = collisionInfo.gameObject.GetComponent <Immigrant> ();
				immigrantCarried.PickedByPlayer (immigrantHoldPosition);
                timeSinceAction = 0;
			}
		}
	}

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Retrieval Zone") //Player is in a retrieval zone
        {
            if (Input.GetButtonDown(actionInputAxis + playerID.ToString()) && carryingImmigrant) //Player wants to drop the current immigrant
            {
                Country country = other.GetComponentInParent<Country>();
                Debug.Log("On trigger, releasing immigrant...");
                if (country.tag == immigrantCarried.country) //the immigrant belongs to that country
                {
                    country.TakeImmigrant(immigrantCarried);//make country add immigrant to its queue
                    immigrantCarried.ReleasedByPlayer(true);
                }
                else
                    immigrantCarried.ReleasedByPlayer(false);

                timeSinceAction = 0;
                immigrantCarried = null;
                carryingImmigrant = false;
            }
        }
    }

    void FixedUpdate () 
	{
		moveDir = new Vector3 (Input.GetAxisRaw (horizontalInputAxis + playerID.ToString()), 0, Input.GetAxisRaw (verticalInputAxis + playerID.ToString()));
		if (moveDir.sqrMagnitude > 1f)
			moveDir = moveDir.normalized;

		float targetSpeed = carryingImmigrant ? movementSpeed - speedReduction : movementSpeed;
		Vector3 velocity = moveDir * targetSpeed * Time.fixedDeltaTime;
		rb.MovePosition (transform.position  + velocity);

		if (moveDir != Vector3.zero)
			transform.forward = Vector3.Normalize(moveDir);
	}
}
