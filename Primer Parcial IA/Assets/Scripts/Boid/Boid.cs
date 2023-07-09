using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour
{
    [Header("Targets")]
    public Hunter hunter;
    
    [Header("Attrbutes")]
    private Vector3 _velocity;
    [SerializeField, Range(1, 10)] private int _maxSpeed;
    [SerializeField, Range(1, 10)] private int _maxForce;
    
    [Header("HunterEvade")] 
    [SerializeField] private float _hunterViewRadius = 4;
    
    [Header("Food")] 
    [SerializeField] private float _foodviewRadius = 5;
    
    [Header("Ally")]
    private float _allyViewRadius = 3;
    private float _separationViewRadius = 1;
    private float _radiusSeparation;

    [SerializeField] private List<Boid> _NearBoids;
    [SerializeField] private List<Food> _NearFoods;
    
    [SerializeField] private Food _NearFood;

    private bool _isFoodNear;
    private bool _isAllyNear;
    [SerializeField] private bool _isHunterNear;
    private GridEntity _gridEntity;
    private Queries _queries;

    public Vector3 Velocity { get => _velocity; set => _velocity = value; }

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
        _queries = GetComponent<Queries>();
        
        RandomDirection();
    }

    private void Start()
    {
        IA_Manager.instance.AddBoid(this);
        RandomDirection();
    }


    void Update()
    {
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        transform.position = IA_Manager.instance.ApplyBound(transform.position);

        _queries.selected = _queries.Query().Where(x => x.gameObject != gameObject);
        _NearBoids = _queries.selected.Select(x => x.GetComponent<Boid>()).Where(x => x != null).ToList();
        
        _NearFoods = _queries.selected.Select(x => x.GetComponent<Food>()).Where(x => x != null).ToList();
        
        _NearFood = _NearFoods.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
        
        
        Move(Separation(_NearBoids) * IA_Manager.instance.weightSeparation);
        Move(Cohesion(_NearBoids) * IA_Manager.instance.weightCohesion);
        Move(Alignment(_NearBoids) * IA_Manager.instance.weightAlignment);
        

        if (Vector3.Distance(hunter.transform.position, transform.position) < _hunterViewRadius)
        {
            _isHunterNear = true;
            Move(Evade(hunter));
            _maxForce = 3;
        }
        else
        {
            _isHunterNear = false;
            _maxForce = 10;
        }
        
        
        if (_NearFood && !_isHunterNear)
        {
            Move(Arrive(_NearFood.transform.position));
            Move(Separation(_NearBoids) * IA_Manager.instance.weightSeparation);
            Move(Cohesion(_NearBoids) * IA_Manager.instance.weightCohesion);
            Move(Alignment(_NearBoids) * IA_Manager.instance.weightAlignment);
            if (Vector3.Distance(_NearFood.transform.position, transform.position) < 1f)
            {
                _NearFood.gameObject.SetActive(false);
                _NearFood.transform.position = new Vector3(Random.Range(0, 60), 0, Random.Range(0, 60));
                _NearFood.gameObject.SetActive(true);
                _NearFoods.Remove(_NearFood);
                _NearFood = null;
            }
        }
        else
        {
            RandomDirection();
        }
        
        
      

        
    }

    private void RandomDirection()
    {
        float x = Random.Range(-1, 1);
        float z = Random.Range(-1, 1);
        Vector3 randomDir = new Vector3(x, 0, z).normalized * _maxForce;
        Move(randomDir);
    }

    private void Move(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxForce);
        _gridEntity.DispatchOnMove();
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        float dist = desired.magnitude;
        if (dist <= _foodviewRadius)
        {
            float speed = _maxSpeed * (dist / _foodviewRadius);
            desired.Normalize();
            desired *= speed;
        }
        else
        {
            desired.Normalize();
            desired *= _maxSpeed;
        }

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    Vector3 Alignment(IEnumerable<Boid> nearBoids)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;
        foreach (var item in nearBoids)
        {
            if (item == this)
            {
                continue;
            }

            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= _allyViewRadius)
            {
                desired += item.GetComponent<Boid>()._velocity;
                count++;
            }
        }

        if (count <= 0)
        {
            return desired;
        }

        desired /= count;

        desired.Normalize();

        desired *= _maxForce;

        return CalculateSteering(desired);
    }

    Vector3 Cohesion(IEnumerable<Boid> nearBoids)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var item in nearBoids)
        {
            if (item == this)
            {
                continue;
            }

            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= _allyViewRadius)
            {
                desired += item.transform.position;
                count++;
            }
        }

        if (count <= 0)
        {
            return desired;
        }

        desired /= count;
        desired -= transform.position;

        desired.Normalize();
        desired *= _maxForce;

        return CalculateSteering(desired);
    }

    Vector3 Separation(IEnumerable<Boid> nearBoids)
    {
        Vector3 desired = Vector3.zero;

        foreach (var item in nearBoids)
        {
            Vector3 dist = item.transform.position - transform.position;

            if (dist.magnitude <= _radiusSeparation)
            {
                desired += dist;
            }
        }

        if (desired == Vector3.zero)
        {
            return desired;
        }

        desired = -desired;

        desired.Normalize();

        desired *= _maxForce;

        return CalculateSteering(desired);
    }

    Vector3 Evade(Hunter target)
    {
        Vector3 finalPos = target.transform.position + target.Velocity * Time.deltaTime;
        Vector3 desired = transform.position - finalPos;
        desired.Normalize();
        desired *= _maxForce;
        Vector3 steering = desired - _velocity;
        return steering;
    }

    Vector3 CalculateSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude(desired - _velocity, _maxSpeed);
    }
    

    public float CheckDistance(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }

    private void OnDisable()
    {
        if (!IA_Manager.instance) return;
        IA_Manager.instance.StartCoroutine(IA_Manager.instance.EnableBoids(gameObject));
    }
}
