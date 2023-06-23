using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public static BoidManager instance;
    public List<Boid> allBoids = new List<Boid>();

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddBoid(Boid boid)
    {
        if (!allBoids.Contains(boid))
            allBoids.Add(boid);
    }

    public void RemoveBoid(Boid boid)
    {
        allBoids.Remove(boid);
    }

}
