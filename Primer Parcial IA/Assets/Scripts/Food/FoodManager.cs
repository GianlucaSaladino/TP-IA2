using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodManager : MonoBehaviour
{
    public GameObject food;
    public static FoodManager instance;
    public List<Food> allFoods = new List<Food>();

    [SerializeField, Range(1, 200)] float _width;

    [SerializeField, Range(1, 200)] float _height;

    private float _timeToSpawn = 1f;

    private float _currentTime;


    [SerializeField] private SpatialGrid _grid;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }

        _currentTime = _timeToSpawn;
        for (int i = 0; i < 10; i++)
        {
            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        float x = Random.Range(0, _width * 2);
        float z = Random.Range(0, _height * 2);
        Vector3 spawnPos = new Vector3(x, 0, z);
        Instantiate(food, spawnPos, Quaternion.identity, _grid.transform);
    }

    public void AddFood(Food food)
    {
        if (!allFoods.Contains(food))
            allFoods.Add(food);
    }

    public void RemoveFood(Food food)
    {
        allFoods.Remove(food);
    }
}
