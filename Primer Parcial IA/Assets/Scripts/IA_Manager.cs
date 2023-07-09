using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_Manager : MonoBehaviour
{
    public static IA_Manager instance;
    public List<Boid> allBoids = new List<Boid>();

    [SerializeField, Range(1, 200)] float _width;

    [SerializeField, Range(1, 200)] float _height;

    private float _timeToSpawn = 1f;

    private float _currentTime;

    [Range(0, 50)] public float weightSeparation;

    [Range(0, 50)] public float weightCohesion;

    [Range(0, 50)] public float weightAlignment;
    

    [SerializeField] private SpatialGrid _grid;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
    }

    public Vector3 ApplyBound(Vector3 objectPosition)
    {
        if (objectPosition.x > _width * 2)
            objectPosition.x = 0;
        if (objectPosition.x < 0)
            objectPosition.x = _width * 2;

        if (objectPosition.z > _height * 2)
            objectPosition.z = 0;
        if (objectPosition.z < 0)
            objectPosition.z = _height * 2;

        return objectPosition;
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
    
    public IEnumerator EnableBoids(GameObject boid)
    {
        yield return new WaitForSeconds(3f);
        boid.SetActive(true);
    }
}
