using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVPasser : MonoBehaviour
{
    public MeshRenderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        rend.material.SetFloat("_Height", GetComponent<MeshFilter>().mesh.bounds.size.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
