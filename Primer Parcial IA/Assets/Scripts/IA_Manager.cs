using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_Manager : MonoBehaviour
{

    [SerializeField, Range(1, 200)] float _width;

    [SerializeField, Range(1, 200)] float _height;

    private float _timeToSpawn = 1.5f;

    private float _currentTime;

    [Range(0,5)] public float weightSeparation;

    [Range(0,5)] public float weightCohesion;

    [Range(0,5)] public float weightAlignment;

    public GameObject food;

    public static IA_Manager instance;
    
    public List<Boid> allBoids = new List<Boid>();

    private void Awake()
    {
        instance = this;
        _currentTime = _timeToSpawn;
    }

    private void Update() {
        _currentTime-=Time.deltaTime;
        if(_currentTime<=0){
            SpawnFood();
            _currentTime = _timeToSpawn;
        }
    }

    public Vector3 ApplyBound(Vector3 objectPosition)
    {
        if (objectPosition.x > _width*2)
            objectPosition.x = 0;
        if (objectPosition.x < 0)
            objectPosition.x = _width*2;

        if (objectPosition.z > _height*2)
            objectPosition.z = 0;
        if (objectPosition.z < 0)
            objectPosition.z = _height*2;

        return objectPosition;
    }

    private void SpawnFood(){
        float x = Random.Range(0,_width*2);
        float z = Random.Range(0,_height*2);
        Vector3 spawnPos = new Vector3(x,0,z);
        Instantiate(food,spawnPos,Quaternion.identity);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 topLeft = new Vector3(0, 0, _height*2);
        Vector3 topRight = new Vector3(_width*2, 0, _height*2);
        Vector3 botRight = new Vector3(_width*2, 0, 0);
        Vector3 botLeft = new Vector3(0, 0, 0);

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
