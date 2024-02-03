using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] UVs;
    //public Color[] colors;
    private Gradient gradient;

    private int xSize = 200;
    private int zSize = 200;

    private float xDim = 10;
    private float zDim = 10;

    private GameObject player;
    private int deleteDistance = 5;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        xDim = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().xDim;
        zDim = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().zDim;

        xSize = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().xSize;
        zSize = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().zSize;

        CreateShape();
        UpdateMesh();
        StartCoroutine(deleteMeshes());
    }

    // Update is called once per frame
    void Update()
    {

    }

    float fbm(float a, float b, int octaves, float amp, float freq)
    {
        float result = 0;
        float ampmod = amp;
        float freqmod = freq;
        for(int i = 0; i < octaves; i++)
        {
            result += ampmod * (Mathf.PerlinNoise(Mathf.Abs(Mathf.Pow(10,3)+a*freqmod), Mathf.Abs(Mathf.Pow(10, 3) + b *freqmod)) - 0.5f);
            ampmod = ampmod / 2;
            freqmod = freqmod * 2;
        }

        //calculate max and min
        float ampcalc = amp;
        float sum = 0;
        for(int i = 0; i < octaves; i++)
        {
            sum += 0.5f * ampcalc;
            ampcalc = ampcalc / 2f;
        }
        MeshRenderer rend = GetComponent<MeshRenderer>();
        rend.material.SetFloat("_Low", -1*sum);
        rend.material.SetFloat("_High", sum);
        return result;
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int i = 0;
        for(int z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x / (xSize/xDim), 0, z / (zSize/zDim));
                vertices[i].y = 3 * fbm((transform.position.x + vertices[i].x) * 0.0373f, (transform.position.z + vertices[i].z) * 0.0373f, 7, 3f, 2f);
                i++;

            }
        }

        triangles = new int[6 * xSize * zSize];
        int trindex = 0;
        int vertadd = 0;
        for(int z = 0; z < zSize; z++)
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

        UVs = new Vector2[vertices.Length];
        for (int j = 0; j < UVs.Length; j++)
        {
            UVs[j] = new Vector2(vertices[j].x / xSize, vertices[j].z / zSize);
        }
        //colors = new Color[vertices.Length];
        mesh.vertices = vertices;
        /*float min = mesh.bounds.min.y;
        float max = mesh.bounds.max.y;
        for (int j = 0; j < colors.Length; j++)
        {
            float height = Mathf.InverseLerp(min, max, vertices[j].y);
            Debug.Log(height);
            colors[j] = gradient.Evaluate(height);
        }*/
        
        
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        //mesh.colors = colors;
        mesh.RecalculateNormals();
        Debug.Log(mesh.bounds.min.y);
        Debug.Log(mesh.bounds.max.y);
    }

    IEnumerator deleteMeshes()
    {
        while (true)
        {
            if(Mathf.Abs((transform.position.x + xDim/2 - player.transform.position.x)/xDim) >= (deleteDistance) || Mathf.Abs((transform.position.z + zDim/2 - player.transform.position.z) / zDim) >= (deleteDistance))
            {
                GameObject.Destroy(this.gameObject);
            }
            yield return new WaitForSeconds(1);
        }
    }
}
