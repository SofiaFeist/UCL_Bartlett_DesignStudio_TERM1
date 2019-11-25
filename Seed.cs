using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed 
{
    public Vector3 Position = new Vector3(0,0,0);



    public Seed(GameObject prefab, Vector3 position, Material material)
    {
        this.Position = position;
        GameObject seed = Object.Instantiate(prefab, Position, Quaternion.identity);
        seed.GetComponent<Renderer>().material = material;
    }
}
