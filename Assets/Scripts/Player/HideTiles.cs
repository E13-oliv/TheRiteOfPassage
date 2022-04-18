using UnityEngine;

public class HideTiles : MonoBehaviour
{
    [Header("Tiles management")]
	[SerializeField]
	private string tileTag;

	[SerializeField]
	private Vector3 tileSize;

	[SerializeField]
	private int maxDistance;

	[SerializeField]
	private GameObject depa;

	private GameObject[] tiles;

	private Terrain playerActiveTerrain;
	private Terrain depaActiveTerrain;

	private void Start()
	{
        // get the terrain tiles
		this.tiles = GameObject.FindGameObjectsWithTag(tileTag);
		ManageTiles();
	}

	private void Update()
	{
		ManageTiles();
	}

    // deactivate distant tiles and set active tile (activeTerrain)
	private void ManageTiles()
	{
		Vector3 playerPosition = this.gameObject.transform.position;
		Vector3 depaPosition = depa.transform.position;

		foreach (GameObject tile in tiles)
		{
			Vector3 tilePosition = tile.gameObject.transform.position + (tileSize / 2f);

			float xPlayerDistance = Mathf.Abs(tilePosition.x - playerPosition.x);
			float zPlayerDistance = Mathf.Abs(tilePosition.z - playerPosition.z);

			float xDepaDistance = Mathf.Abs(tilePosition.x - depaPosition.x);
			float zDepaDistance = Mathf.Abs(tilePosition.z - depaPosition.z);

			// if the tile is too far away
			if (xPlayerDistance + zPlayerDistance > maxDistance)
			{
				tile.SetActive(false);
			} else
			{
				tile.SetActive(true);
			}

            // set the nearest tile from player as player active tile
            if (xPlayerDistance < tileSize.x / 2 && zPlayerDistance < tileSize.z / 2)
            {
				playerActiveTerrain = tile.GetComponent<Terrain>();
            }

			// set the nearest tile from Depa as depa active tile
			if (xDepaDistance < tileSize.x / 2 && zDepaDistance < tileSize.z / 2)
			{
				depaActiveTerrain = tile.GetComponent<Terrain>();
			}
		}
	}

    // public get methods
	public Terrain GetPlayerActiveTerrain()
    {
		return playerActiveTerrain;
    }

	public Terrain GetDepaActiveTerrain()
	{
		return depaActiveTerrain;
	}
}