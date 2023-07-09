using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class Food : MonoBehaviour
{
    private void Start()
    {
        FoodManager.instance.AddFood(this);
    }
} 


