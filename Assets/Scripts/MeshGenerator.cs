using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] UVs;
    //public Color[] colors;
    private Gradient gradient;
    private Vector3[] normals;

    private Mesh simp;
    private Vector3[] sVertices;
    private int[] sTriangles;
    private Vector2[] sUVs;
    private Vector3[] sNormals;
    private Vector4[] sTangents;

    private Vector3[] tempNorms;
    private Vector4[] tempTangs;

    private int xSize = 100;
    private int zSize = 100;

    private float xDim = 10;
    private float zDim = 10;

    private GameObject player;
    private MeshCollider mc;
    private int deleteDistance = 5;
    private MeshRenderer rend;
    private float low;
    private float high;

    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        simp = new Mesh();
        rend = GetComponent<MeshRenderer>();
        player = GameObject.Find("Player");
        pos = transform.position;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mc = GetComponent<MeshCollider>();

        xDim = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().xDim;
        zDim = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().zDim;

        xSize = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().xSize;
        zSize = GameObject.Find("TerrainGen").GetComponent<TerrainGen>().zSize;

        low = -0.5f;
        high = 0.5f;

        MeshA();
        //StartCoroutine(MeshC());
        /*UpdateMesh();
        mesh.RecalculateNormals();
        normals = mesh.normals;

        StartCoroutine(RemoveEdgesC());
        UpdateMesh();

        mesh.normals = normals; // normals have been changed in RemoveEdges()
        mesh.RecalculateTangents();*/
        
        //StartCoroutine(MakeMesh());

        /*for(int i = 0; i < vertices.Length; i++)
        {
            Debug.DrawRay(transform.position + vertices[i], 0.1f * normals[i], new Color(1, 0, 0), 10);
        }*/

        StartCoroutine(deleteMeshes());
    }

    // Update is called once per frame
    void Update()
    {
        pos = transform.position;
        rend.material.SetFloat("_Low", low);
        rend.material.SetFloat("_High", high);
    }

    float fbm(float a, float b, int octaves, float amp, float freq)
    {
        float result = 0;
        float ampmod = amp;
        float freqmod = freq;
        for (int i = 0; i < octaves; i++)
        {
            result += ampmod * (Mathf.PerlinNoise(Mathf.Abs(Mathf.Pow(10, 3) + a * freqmod), Mathf.Abs(Mathf.Pow(10, 3) + b * freqmod)) - 0.5f);
            ampmod = ampmod / 2;
            freqmod = freqmod * 2;
        }

        //calculate max and min
        float ampcalc = amp;
        float sum = 0;
        for (int i = 0; i < octaves; i++)
        {
            sum += 0.5f * ampcalc;
            ampcalc = ampcalc / 2f;
        }
        low = -1 * sum;
        high = sum;
        return result;
    }
    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1 + 2) * (zSize + 1 + 2)];
        int i = 0;
        for(int z = 0; z <= zSize + 2; z++)
        {
            for(int x = 0; x <= xSize + 2; x++)
            {
                vertices[i] = new Vector3((x-1) / (xSize/xDim), 0, (z-1) / (zSize/zDim));
                vertices[i].y = 3 * fbm((pos.x + vertices[i].x) * 0.0373f, (pos.z + vertices[i].z) * 0.0373f, 7, 3f, 2f);
                i++;

            }
        }

        triangles = new int[6 * (xSize+2) * (zSize+2)];
        int trindex = 0;
        int vertadd = 0;
        for(int z = 0; z < zSize+2; z++)
        {
            for (int x = 0; x < xSize+2; x++)
            {
                triangles[0 + trindex] = vertadd + 0;
                triangles[1 + trindex] = vertadd + xSize + 2 + 1;
                triangles[2 + trindex] = vertadd + 1;
                triangles[3 + trindex] = vertadd + xSize + 2 + 1;
                triangles[4 + trindex] = vertadd + xSize + 2 + 2;
                triangles[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
        }

        UVs = new Vector2[0];
        
        
    }

    void RemoveEdges()
    {
        //MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
        //Vector4[] vert4 = rend.material.GetVectorArray("vertices");
        //int xs = rend.material.GetInt("xSize");
        //print(vert4);

        Vector3[] newVerts = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector3[] newNormals = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 1; z < zSize + 2; z++)
        {
            for(int x = 1; x < xSize + 2; x++)
            {
                //Debug.Log(zSize);
                newVerts[(z - 1) * (zSize + 1) + (x - 1)] = vertices[(zSize + 3) * z + x];
                newNormals[(z - 1) * (zSize + 1) + (x - 1)] = normals[(zSize + 3) * z + x];
            }
        }
        int[] newTriangles = new int[6 * (xSize) * (zSize)];
        int trindex = 0;
        int vertadd = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                newTriangles[0 + trindex] = vertadd + 0;
                newTriangles[1 + trindex] = vertadd + xSize + 1;
                newTriangles[2 + trindex] = vertadd + 1;
                newTriangles[3 + trindex] = vertadd + xSize + 1;
                newTriangles[4 + trindex] = vertadd + xSize + 2;
                newTriangles[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
        }

        Vector2[] newUVs = new Vector2[newVerts.Length];
        for (int j = 0; j < newUVs.Length; j++)
        {
            newUVs[j] = new Vector2(newVerts[j].x / (xDim), newVerts[j].z / (zDim));
        }
        vertices = newVerts;
        triangles = newTriangles;
        normals = newNormals;
        UVs = newUVs;
    }

    /*IEnumerator MeshC()
    {
        vertices = new Vector3[(xSize + 1 + 2) * (zSize + 1 + 2)];
        int i = 0;
        for (int z = 0; z <= zSize + 2; z++)
        {
            for (int x = 0; x <= xSize + 2; x++)
            {
                vertices[i] = new Vector3((x - 1) / (xSize / xDim), 0, (z - 1) / (zSize / zDim));
                vertices[i].y = 3 * fbm((transform.position.x + vertices[i].x) * 0.0373f, (transform.position.z + vertices[i].z) * 0.0373f, 7, 3f, 2f);
                i++;

            }
            yield return null;
        }

        triangles = new int[6 * (xSize + 2) * (zSize + 2)];
        int trindex = 0;
        int vertadd = 0;
        for (int z = 0; z < zSize + 2; z++)
        {
            for (int x = 0; x < xSize + 2; x++)
            {
                triangles[0 + trindex] = vertadd + 0;
                triangles[1 + trindex] = vertadd + xSize + 2 + 1;
                triangles[2 + trindex] = vertadd + 1;
                triangles[3 + trindex] = vertadd + xSize + 2 + 1;
                triangles[4 + trindex] = vertadd + xSize + 2 + 2;
                triangles[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
            if(z % (int)((zSize+2)/20) == 0)
            {
                yield return null;
            }
        }

        UVs = new Vector2[0];

        UpdateMesh();
        mesh.RecalculateNormals();
        normals = mesh.normals;

        Vector3[] newVerts = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector3[] newNormals = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 1; z < zSize + 2; z++)
        {
            for (int x = 1; x < xSize + 2; x++)
            {
                //Debug.Log(zSize);
                newVerts[(z - 1) * (zSize + 1) + (x - 1)] = vertices[(zSize + 3) * z + x];
                newNormals[(z - 1) * (zSize + 1) + (x - 1)] = normals[(zSize + 3) * z + x];
            }
            if (z % (int)((zSize + 2) / 20) == 0)
            {
                yield return null;
            }
        }
        yield return null;
        int[] newTriangles = new int[6 * (xSize) * (zSize)];
        trindex = 0;
        vertadd = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                newTriangles[0 + trindex] = vertadd + 0;
                newTriangles[1 + trindex] = vertadd + xSize + 1;
                newTriangles[2 + trindex] = vertadd + 1;
                newTriangles[3 + trindex] = vertadd + xSize + 1;
                newTriangles[4 + trindex] = vertadd + xSize + 2;
                newTriangles[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
            if (z % (int)((zSize + 2) / 20) == 0)
            {
                yield return null;
            }
        }
        Vector2[] newUVs = new Vector2[newVerts.Length];
        for (int j = 0; j < newUVs.Length; j++)
        {
            newUVs[j] = new Vector2(newVerts[j].x / (xDim), newVerts[j].z / (zDim));
            if (j % (int)(newUVs.Length / 20) == 0)
            {
                yield return null;
            }
        }
        vertices = newVerts;
        triangles = newTriangles;
        normals = newNormals;
        UVs = newUVs;

        UpdateMesh();

        mesh.normals = normals; 
        mesh.RecalculateTangents();
    }*/

    async void MeshA()
    {
        await CS();
        UpdateMesh();
        await RN();
        mesh.normals = tempNorms;
        normals = mesh.normals;
        await RE();
        UpdateMesh();
        mesh.normals = normals;
        await RT();
        mesh.tangents = tempTangs;
        await SM();
        assignSimp();

        //mesh.RecalculateTangents();
    }
    async Task CS()
    {
        await Task.Run(() => CreateShape());
    }
    async Task RN()
    {
        await Task.Run(() => tempNorms = RNormals(mesh, 0));
    }
    async Task RE()
    {
        await Task.Run(() => RemoveEdges());
    }
    async Task RT()
    {
        await Task.Run(() => tempTangs = RTangents(mesh));
    }
    async Task SM()
    {
        await Task.Run(() => simplifiedMesh(2));
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        if(UVs.Length > 0)
        {
            mesh.uv = UVs;
        }
    }

    void simplifiedMesh(int simplificationCoeff)
    {
        int sC = simplificationCoeff;
        Vector3[] nV = new Vector3[(xSize / sC + 1) * (zSize / sC + 1)];
        Vector3[] nN = new Vector3[(xSize / sC + 1) * (zSize / sC + 1)];
        Vector4[] nTg = new Vector4[(xSize / sC + 1) * (zSize / sC + 1)];
        Vector2[] nUV = new Vector2[(xSize / sC + 1) * (zSize / sC + 1)];
        int count = 0;
        for(int z = 0; z <= zSize; z += sC)
        {
            for (int x = 0; x <= xSize; x += sC)
            {
                nV[count] = vertices[z * (xSize + 1) + x];
                nN[count] = normals[z * (xSize + 1) + x];
                nTg[count] = tempTangs[z * (xSize + 1) + x];
                nUV[count] = UVs[z * (xSize + 1) + x];
                count++;
            }
        }
        int[] nT = new int[6 * (xSize/sC) * (zSize/sC)];
        int trindex = 0;
        int vertadd = 0;
        for (int z = 0; z < (zSize/sC); z++)
        {
            for (int x = 0; x < (xSize / sC); x++)
            {
                nT[0 + trindex] = vertadd + 0;
                nT[1 + trindex] = vertadd + (xSize / sC) + 1;
                nT[2 + trindex] = vertadd + 1;
                nT[3 + trindex] = vertadd + (xSize / sC) + 1;
                nT[4 + trindex] = vertadd + (xSize / sC) + 2;
                nT[5 + trindex] = vertadd + 1;
                trindex += 6;
                vertadd++;
            }
            vertadd++;
        }
        sVertices = nV;
        sTriangles = nT;
        sNormals = nN;
        sTangents = nTg;
        sUVs = nUV;
    }
    void assignSimp()
    {
        simp.vertices = sVertices;
        simp.triangles = sTriangles;
        simp.normals = sNormals;
        simp.tangents = sTangents;
        simp.uv = sUVs;

        mc.sharedMesh = simp;
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
    Vector3[] RNormals(Mesh mesh, float angle)
    {
        var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);

        var verts = vertices;
        var normals = new Vector3[verts.Length];
        

        var dictionary = new Dictionary<VertexKey, List<VertexEntry>>(verts.Length);

        var trigs = triangles;

        var triNormals = new Vector3[trigs.Length / 3];

        for (var i = 0; i < triangles.Length; i += 3)
        {
            int i1 = trigs[i];
            int i2 = trigs[i + 1];
            int i3 = trigs[i + 2];

            // Calculate the normal of the triangle
            Vector3 p1 = verts[i2] - verts[i1];
            Vector3 p2 = verts[i3] - verts[i1];
            Vector3 normal = Vector3.Cross(p1, p2).normalized;
            int triIndex = i / 3;
            triNormals[triIndex] = normal;

            List<VertexEntry> entry;
            VertexKey key;

            if (!dictionary.TryGetValue(key = new VertexKey(verts[i1]), out entry))
            {
                entry = new List<VertexEntry>(4);
                dictionary.Add(key, entry);
            }
            entry.Add(new VertexEntry(0, triIndex, i1));

            if (!dictionary.TryGetValue(key = new VertexKey(verts[i2]), out entry))
            {
                entry = new List<VertexEntry>();
                dictionary.Add(key, entry);
            }
            entry.Add(new VertexEntry(0, triIndex, i2));

            if (!dictionary.TryGetValue(key = new VertexKey(verts[i3]), out entry))
            {
                entry = new List<VertexEntry>();
                dictionary.Add(key, entry);
            }
            entry.Add(new VertexEntry(0, triIndex, i3));
            }

        // Each entry in the dictionary represents a unique vertex position.

        foreach (var vertList in dictionary.Values)
        {
            for (var i = 0; i < vertList.Count; ++i)
            {

                var sum = new Vector3();
                var lhsEntry = vertList[i];

                for (var j = 0; j < vertList.Count; ++j)
                {
                    var rhsEntry = vertList[j];

                    if (lhsEntry.VertexIndex == rhsEntry.VertexIndex)
                    {
                        sum += triNormals[rhsEntry.TriangleIndex];
                    }
                    else
                    {
                        // The dot product is the cosine of the angle between the two triangles.
                        // A larger cosine means a smaller angle.
                        var dot = Vector3.Dot(
                            triNormals[lhsEntry.TriangleIndex],
                            triNormals[rhsEntry.TriangleIndex]);
                        if (dot >= cosineThreshold)
                        {
                            sum += triNormals[rhsEntry.TriangleIndex];
                        }
                    }
                }

                normals[lhsEntry.VertexIndex] = sum.normalized;
            }
        }

        return normals;
    }

    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        // Change this if you require a different precision.
        private const int Tolerance = 100000;

        // Magic FNV values. Do not change these.
        private const long FNV32Init = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;

        public VertexKey(Vector3 position)
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }

        public override int GetHashCode()
        {
            long rv = FNV32Init;
            rv ^= _x;
            rv *= FNV32Prime;
            rv ^= _y;
            rv *= FNV32Prime;
            rv ^= _z;
            rv *= FNV32Prime;

            return rv.GetHashCode();
        }
    }

    private struct VertexEntry
    {
        public int MeshIndex;
        public int TriangleIndex;
        public int VertexIndex;

        public VertexEntry(int meshIndex, int triIndex, int vertIndex)
        {
            MeshIndex = meshIndex;
            TriangleIndex = triIndex;
            VertexIndex = vertIndex;
        }
    }

    Vector4[] RTangents(Mesh mesh)
    {
        int triangleCount = triangles.Length;
        int vertexCount = vertices.Length;

        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        Vector4[] tangents = new Vector4[vertexCount];

        for (long a = 0; a < triangleCount; a += 3)
        {
            long i1 = triangles[a + 0];
            long i2 = triangles[a + 1];
            long i3 = triangles[a + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = UVs[i1];
            Vector2 w2 = UVs[i2];
            Vector2 w3 = UVs[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        for (long a = 0; a < vertexCount; ++a)
        {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];

            Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
            tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);

            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
        }

        return tangents;
    }
}
