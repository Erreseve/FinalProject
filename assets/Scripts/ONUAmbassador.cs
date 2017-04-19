using UnityEngine;
using System;
using System.Collections;

public class ONUAmbassador : MonoBehaviour 
{
    public int scoreValue = 5;
    public float speed = 8;
    public float playerDetectionRadius = 5f;

    PlayerController closestPlayer;
    Rigidbody rb;
    Collider coll;
    bool grabbed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();

        Physics.gravity = Vector3.down * 50f;
        transform.position = Vector3.zero + Vector3.up * 10f;
    }

    private void Update()
    {
        if (!grabbed)
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, playerDetectionRadius);
            foreach (Collider c in collisions)
            {
                if (c.gameObject.tag == "Player")
                {
                    closestPlayer = c.gameObject.GetComponent<PlayerController>();
                    break;
                }
            }
            if (closestPlayer != null)
            {
                transform.LookAt( new Vector3 (closestPlayer.transform.position.x, coll.bounds.extents.y, closestPlayer.transform.position.z));
                transform.Rotate(Vector3.up, 180f);
            }
        }
    }

    private void LateUpdate()
    {
        if (!grabbed && closestPlayer != null)
        {
            transform.Translate( transform.forward * speed * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, playerDetectionRadius);
    }


}