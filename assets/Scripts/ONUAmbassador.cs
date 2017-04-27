using UnityEngine;
using System.Collections;

public class ONUAmbassador : MonoBehaviour 
{
    public int scoreValue = 5;
    public float speed = 8;
    public float playerDetectionRadius = 5f;

    PlayerController closestPlayer;
    Rigidbody rb;
    Collider coll;
    bool onGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();

        Physics.gravity = Vector3.down * 50f;
        transform.position = GameManager.instance.RequestRandomWorldPos() + Vector3.up * 10f;
    }

    private void Update()
    {
        if (onGround)
        {
            //run from the nearest player
            bool playerNearby = false;
            Collider[] collisions = Physics.OverlapSphere(transform.position, playerDetectionRadius);
            foreach (Collider c in collisions)
            {
                if (c.gameObject.tag == "Player")
                {
                    closestPlayer = c.gameObject.GetComponent<PlayerController>();
                    playerNearby = true;
                    break;
                }
            }

            if (!playerNearby)
                closestPlayer = null;

            if (closestPlayer != null)
            { //look away from the closes player if any
                transform.LookAt( new Vector3 (closestPlayer.transform.position.x, coll.bounds.extents.y, closestPlayer.transform.position.z));
                transform.Rotate(Vector3.up, 180f);
            }

            //avoid collisions
            RaycastHit hit;
            Debug.DrawRay(transform.position + transform.forward * coll.bounds.extents.z, transform.forward, Color.red);
            if (Physics.Raycast(transform.position + transform.forward* coll.bounds.extents.z, transform.forward, out hit, coll.bounds.extents.z + .1f))
            {
                if (hit.collider.gameObject.tag != "Untagged") //collision with and object
                {
                    float angleNudge = Random.Range(30, 90);
                    angleNudge = (int)Random.Range(0, 2) == 1 ? -angleNudge : angleNudge; //randomly choose right or left rotation
                    transform.Rotate(Vector3.up, angleNudge);
                }
            }
        }
    }

    public void GrabbedByPlayer()
    {
        Destroy(gameObject);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ground")
        {
            onGround = true;
        }
    }

    private void FixedUpdate()
    {
        if (onGround)
        {
            rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
        }
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, playerDetectionRadius);
    }*/


}