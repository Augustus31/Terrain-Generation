using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGen : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter filter;
    private GameObject tg;
    private TerrainGen tgs;
    private GameObject player;

    int xDim;
    int zDim;

    int xSize;
    int zSize;

    float lo;
    float hi;

    public float waterLevel;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        filter = GetComponent<MeshFilter>();
        tg = GameObject.Find("TerrainGen");
        tgs = tg.GetComponent<TerrainGen>();
        player = GameObject.Find("Player");
        xDim = Mathf.RoundToInt(tgs.xDim * (2 * tgs.drawDistance + 5));
        zDim = Mathf.RoundToInt(tgs.zDim * (2 * tgs.drawDistance + 5));

        xSize = xDim * 10* 2;
        zSize = zDim * 10 * 2;

        gameObject.transform.position = new Vector3(player.transform.position.x - xDim / 2, 0, player.transform.position.z - zDim / 2);

        GameObject mg = GameObject.Find("TerrainGen");
        TerrainGen mgs = tg.GetComponent<TerrainGen>();

        StartCoroutine(setWater());
    }

    // Update is called once per frame
    void Update()
    {
        float xFloor = Mathf.Floor((player.transform.position.x - xDim / 2) / 10) * 10;
        float zFloor = Mathf.Floor((player.transform.position.z - zDim / 2) / 10) * 10;
        gameObject.transform.position = new Vector3(xFloor, 0, zFloor);
    }

    void CreateMesh()
    {
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int i = 0;
        float low = tgs.lo;
        float high = tgs.hi;
        float cutoff = waterLevel * (high - low) + low;
        gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Cutoff", cutoff);
        Debug.Log(cutoff);
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3((x) / (xSize / xDim), 0, (z) / (zSize / zDim));
                vertices[i].y = cutoff;
                i++;

            }
        }

        int[] triangles = new int[6 * (xSize) * (zSize)];
        int trindex = 0;
        int vertadd = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[0 + trindex] = vertadd + 0;
                triangles[1 + trindex] = vertadd + xSize + 1;
                triangles[2 + trindex] = vertadd + 1;
                triangles[3 + trindex] = vertadd + xSize + 1;
                triangles[4 + trindex] = vertadd + xSize + 2;
                triangles[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
        }

        Vector2[] UVs = new Vector2[0];
        for (int j = 0; j < UVs.Length; j++)
        {
            UVs[j] = new Vector2(vertices[j].x / (xDim), vertices[j].z / (zDim));
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        filter.mesh = mesh;
    }

    IEnumerator setWater()
    {
        yield return new WaitForSeconds(1);
        CreateMesh();
    }
}
