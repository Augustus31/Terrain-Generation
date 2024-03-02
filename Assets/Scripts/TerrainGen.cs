using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    private GameObject player;
    private Dictionary<Vector2Int, GameObject> meshes;

    public float xDim = 10;
    public float zDim = 10;

    public int xSize = 100;
    public int zSize = 100;

    public GameObject meshfab;

    [SerializeField]
    int drawDistance;
    // Start is called before the first frame update
    void Start()
    {
        meshes = new Dictionary<Vector2Int, GameObject>();
        player = GameObject.Find("Player");
        StartCoroutine(updateMeshes());
        player.GetComponent<Rigidbody>().velocity = new Vector3(1, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void draw(int dist)
    {
        Vector3 playerPos = player.transform.position;
        Vector2Int playerCoord = new Vector2Int(Mathf.FloorToInt(playerPos.x / xDim), Mathf.FloorToInt(playerPos.z / zDim));
        //Vector2[] grid = new Vector2[Mathf.RoundToInt(Mathf.Pow((2 * dist + 1), 2))];
        for(int i = -1*dist; i <= dist; i++)
        {
            for(int j = -1*dist; j <= dist; j++)
            {
                if(!meshes.ContainsKey(new Vector2Int(playerCoord.x + i, playerCoord.y + j))){
                    GameObject newMeshObj = GameObject.Instantiate(meshfab, new Vector3((playerCoord.x + i)*xDim, 0, (playerCoord.y + j)*zDim), Quaternion.identity);
                    meshes.Add(new Vector2Int(playerCoord.x + i, playerCoord.y + j), newMeshObj);
                }
            }
        }
    }

    IEnumerator updateMeshes()
    {
        while (true)
        {
            draw(drawDistance);
            foreach (var key in new List<Vector2Int>(meshes.Keys))
            {
                if(meshes[key] == null)
                {
                    meshes.Remove(key);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
