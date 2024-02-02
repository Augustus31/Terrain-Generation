using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    private GameObject player;
    private HashSet<Vector2> meshes;

    public float xDim = 10;
    public float zDim = 10;

    public GameObject meshfab;

    [SerializeField]
    int drawDistance = 3;
    // Start is called before the first frame update
    void Start()
    {
        meshes = new HashSet<Vector2>();
        player = GameObject.Find("Player");
        StartCoroutine(updateMeshes());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void draw(int dist)
    {
        Vector3 playerPos = player.transform.position;
        Vector2 playerCoord = new Vector2(Mathf.FloorToInt(playerPos.x / xDim), Mathf.FloorToInt(playerPos.z / zDim));
        //Vector2[] grid = new Vector2[Mathf.RoundToInt(Mathf.Pow((2 * dist + 1), 2))];
        for(int i = -1*dist; i <= dist; i++)
        {
            for(int j = -1*dist; j <= dist; j++)
            {
                if(!meshes.Contains(new Vector2(playerCoord.x + i, playerCoord.y + j))){
                    meshes.Add(new Vector2(playerCoord.x + i, playerCoord.y + j));
                    GameObject.Instantiate(meshfab, new Vector3((playerCoord.x + i)*xDim, 0, (playerCoord.y + j)*zDim), Quaternion.identity);
                }
            }
        }
    }

    IEnumerator updateMeshes()
    {
        while (true)
        {
            draw(drawDistance);
            yield return new WaitForSeconds(1f);
        }
    }
}
