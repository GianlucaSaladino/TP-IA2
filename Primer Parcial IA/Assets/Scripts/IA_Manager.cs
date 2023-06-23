using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_Manager : MonoBehaviour
{
    public static IA_Manager instance;

    public List<Boid> allBoids = new List<Boid>();

    [SerializeField, Range(1, 35)] private float _width;

    [SerializeField, Range(1, 35)] private float _height;

    private float _timeToSpawn = 1.5f;

    private float _currentTime;

    [Range(0, 5)] public float weightSeparation;

    [Range(0, 5)] public float weightCohesion;

    [Range(0, 5)] public float weightAlignment;

    public GameObject food;


    private void Awake()
    {
        //instance = this;
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }

        _currentTime = _timeToSpawn;
    }

    private void Update()
    {
        _currentTime -= Time.deltaTime;
        if (_currentTime <= 0)
        {
            SpawnFood();
            _currentTime = _timeToSpawn;
        }
    }

    public Vector3 ApplyBound(Vector3 objectPosition)
    {
        if (objectPosition.x > _width)
            objectPosition.x = -_width;
        if (objectPosition.x < -_width)
            objectPosition.x = _width;

        if (objectPosition.z > _height)
            objectPosition.z = -_height;
        if (objectPosition.z < -_height)
            objectPosition.z = _height;

        return objectPosition;
    }

    private void SpawnFood()
    {
        float x = Random.Range(-_width, _width);
        float z = Random.Range(-_height, _height);
        Vector3 spawnPos = new Vector3(x, 0, z);
        Instantiate(food, spawnPos, Quaternion.identity);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 topLeft = new Vector3(-_width, 0, _height);
        Vector3 topRight = new Vector3(_width, 0, _height);
        Vector3 botRight = new Vector3(_width, 0, -_height);
        Vector3 botLeft = new Vector3(-_width, 0, -_height);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
        Gizmos.DrawLine(botLeft, topLeft);
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
