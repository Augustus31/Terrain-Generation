using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    private GameObject player;
    private Dictionary<Vector2, Mesh> meshes;

    public float xDim = 10;
    public float zDim = 10;

    public int xSize = 200;
    public int zSize = 200;

    public GameObject meshfab;

    [SerializeField]
    int drawDistance = 0;
    // Start is called before the first frame update
    void Start()
    {
        meshes = new Dictionary<Vector2, Mesh>();
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
                if(!meshes.ContainsKey(new Vector2(playerCoord.x + i, playerCoord.y + j))){
                    GameObject newMeshObj = GameObject.Instantiate(meshfab, new Vector3((playerCoord.x + i)*xDim, 0, (playerCoord.y + j)*zDim), Quaternion.identity);
                    meshes.Add(new Vector2(playerCoord.x + i, playerCoord.y + j), newMeshObj.GetComponent<MeshFilter>().mesh);
                }
            }
        }
    }

    void newNormals()
    {
        foreach (var item in meshes)
        {
            Mesh W = null;
            Mesh E = null;
            Mesh S = null;
            Mesh N = null;
            Vector2 loc = item.Key;
            Mesh mesh = item.Value;
            Vector3[] normals = mesh.normals;
            if(meshes.ContainsKey(new Vector2(loc.x, loc.y + 1)))
            {
                N = meshes[new Vector2(loc.x, loc.y + 1)];
            }
            if (meshes.ContainsKey(new Vector2(loc.x, loc.y - 1)))
            {
                S = meshes[new Vector2(loc.x, loc.y - 1)];
            }
            if (meshes.ContainsKey(new Vector2(loc.x, loc.y + 1)))
            {
                E = meshes[new Vector2(loc.x + 1, loc.y)];
            }
            if (meshes.ContainsKey(new Vector2(loc.x, loc.y + 1)))
            {
                W = meshes[new Vector2(loc.x - 1, loc.y)];
            }

            if(N != null)
            {
                for(int i = (xSize+1)*(zSize+1) - 1 -1; i > (xSize + 1) * (zSize) - 1 + 1; i--)
                {
                    float h = mesh.vertices[i].y;
                    float hx = (h - mesh.vertices[i+1].y) - (h - mesh.vertices[i -1].y);
                    float hy = (h - mesh.vertices[i - xSize-1].y) - (h - N.vertices[i%(xSize+1)].y);
                    normals[i] = Vector3.Cross(new Vector3(1, hx, 0), new Vector3(0, hx, 1)).normalized;
                }
            }

            if (S != null)
            {
                for (int i = (xSize + 1) - 1 - 1; i > 0; i--)
                {
                    float h = mesh.vertices[i].y;
                    float hx = (h - mesh.vertices[i + 1].y) - (h - mesh.vertices[i - 1].y);
                    float hy = (h - N.vertices[(xSize+1)*zSize + i].y) - (h - mesh.vertices[i + xSize + 1].y);
                    normals[i] = Vector3.Cross(new Vector3(1, hx, 0), new Vector3(0, hx, 1)).normalized;
                }
            }

            if (E != null)
            {
                for (int i = (xSize + 1) * (zSize + 1) - 1 - 1; i > (xSize + 1) * (zSize) - 1 + 1; i--)
                {
                    float h = mesh.vertices[i].y;
                    float hx = (h - mesh.vertices[i + 1].y) - (h - mesh.vertices[i - 1].y);
                    float hy = (h - mesh.vertices[i - xSize - 1].y) - (h - N.vertices[i % (xSize + 1)].y);
                    normals[i] = Vector3.Cross(new Vector3(1, hx, 0), new Vector3(0, hx, 1)).normalized;
                }
            }

            if (W != null)
            {
                for (int i = (xSize + 1) * (zSize + 1) - 1 - 1; i > (xSize + 1) * (zSize) - 1 + 1; i--)
                {
                    float h = mesh.vertices[i].y;
                    float hx = (h - mesh.vertices[i + 1].y) - (h - mesh.vertices[i - 1].y);
                    float hy = (h - mesh.vertices[i - xSize - 1].y) - (h - N.vertices[i % (xSize + 1)].y);
                    normals[i] = Vector3.Cross(new Vector3(1, hx, 0), new Vector3(0, hx, 1)).normalized;
                }
            }

        }
    }

    IEnumerator updateMeshes()
    {
        while (true)
        {
            draw(drawDistance);
            foreach (var item in meshes)
            {
                if(item.Value == null)
                {
                    meshes.Remove(item.Key);
                }
            }
            newNormals();
            yield return new WaitForSeconds(1f);
        }
    }
}
