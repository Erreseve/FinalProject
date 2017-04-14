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
	float immigrantReleaseCooldown;

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
				immigrantReleaseCooldown = .5f;
				immigrantCarried = null;
				carryingImmigrant = false;
			}
		}
		immigrantReleaseCooldown -= Time.deltaTime;
		immigrantReleaseCooldown = Mathf.Clamp01(immigrantReleaseCooldown);

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	void OnCollisionStay (Collision collisionInfo)
	{
		if (collisionInfo.gameObject.tag == "Immigrant")
		{
			if (Input.GetKeyUp (KeyCode.Space) && !carryingImmigrant && immigrantReleaseCooldown <= 0)
			{
				Debug.Log ("Picked up immigrant!");
				carryingImmigrant = true;
				immigrantCarried = collisionInfo.gameObject.GetComponent < Immigrant> ();
				immigrantCarried.PickedByPlayer (immigrantHoldPosition);
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
