using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour {

	public bool displayWorldGizmos;

	[SerializeField]
	Vector2 gridWorldSize;
	[SerializeField]
	float nodeRadius;

	Node[,] grid;
	Queue <Node> spawnNodeSelectorQ; //holds all nodes in the grid order randomly

	float nodeDiameter;
	int gridSizeX;
	int gridSizeY;
	float reshuffleTimer = 2f;
	float timeSinceLastShuffle;

	void Awake () {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);

		spawnNodeSelectorQ = new Queue<Node> ();

		CreateGrid ();
	}

	/*void Update()
	{
		//every reshuffleTimer seconds, reshuffle the queue
		if (timeSinceLastShuffle > reshuffleTimer)
		{
			spawnNodeSelectorQ = Util.ShuffleQueue (spawnNodeSelectorQ);
			timeSinceLastShuffle = 0;
		}
		else
			timeSinceLastShuffle += Time.deltaTime;
	}*/

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		//pick start point the bottom left point of the grid in world coordinates
		Vector3 gridBottomLeftPos = transform.position + Vector3.left * gridSizeX / 2 - Vector3.forward * gridSizeY / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				//the position of each node is its center
				Vector3 worldPoint = gridBottomLeftPos + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

				Node n = new Node (worldPoint, x, y);
				grid [x, y] = n;
				spawnNodeSelectorQ.Enqueue (n);
			}
		}
		//puts nodes in the queue, ready to select random spawn points
		spawnNodeSelectorQ = Util.ShuffleQueue(spawnNodeSelectorQ);
		timeSinceLastShuffle = 0;
	}

	public Vector3 PickRandomGridPos()
	{
		Node node;
		while (true)
		{
			node = spawnNodeSelectorQ.Dequeue (); //pick random node
			Collider[] collisions = Physics.OverlapBox (node.worldPos, new Vector3 (nodeRadius, nodeRadius, nodeRadius));

            if (collisions.Length <= 0) //no player or immigrants in this node
            {
                spawnNodeSelectorQ.Enqueue(node);
                break;
            }
            else
            {
                bool flag = false;
                foreach (Collider c in collisions)
                {
                    string cTag = c.gameObject.tag;
                    if (cTag == "Player" || cTag == "Immigrant")
                        flag = true; //immigrant or player is in this area
                }
                if (!flag) //node is good, break while and return it
                    break;
            }
			spawnNodeSelectorQ.Enqueue (node); //return node to random queue
		}
        spawnNodeSelectorQ.Enqueue(node);
        return node.worldPos;
	}

	void OnDrawGizmos()
	{
		//Map grid overlay
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null && displayWorldGizmos)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(n.worldPos, Vector3.one * nodeDiameter);
			}
		}
	}

	//values lesser than .5 move the grid due to its generation method
	//values too big cause errors
	void OnValidate()
	{
		nodeRadius = Mathf.Clamp(nodeRadius, .5f, 3) ;
	}
}

public class Node
{
	public int gridX, gridY;
	public Vector3 worldPos;

	public Node(Vector3 worldPos, int gridX, int gridY)
	{
		this.worldPos = worldPos;
		this.gridX = gridX;
		this.gridY = gridY;
	}

}
