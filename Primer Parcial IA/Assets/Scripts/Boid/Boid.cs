using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour {
    private Vector3 _velocity;
    [SerializeField, Range(1, 10)] int _maxSpeed;
    [SerializeField, Range(1, 10)] int _maxForce;
    float _foodviewRadius = 5;
    float _allyViewRadius = 3;
    float _hunterViewRadius = 4;
    float _separationViewRadius = 1;
    float _radiusSeparation;

    [SerializeField] private List<Boid> _NearBoids;
    [SerializeField] private List<Hunter> _nearHunter;
    [SerializeField] private GridEntity _NearFood;

    bool _isFoodNear;
    bool _isAllyNear;
    [SerializeField] bool _isHunterNear;
    private GridEntity _gridEntity;
    private Queries _queries;

    public Vector3 Velocity {
        get => _velocity;
        set => _velocity = value;
    }

    private void Awake(){
        _gridEntity = GetComponent<GridEntity>();
        _queries = GetComponent<Queries>();
    }

    void Start(){
        IA_Manager.instance.AddBoid(this);
        RandomDirection();
    }


    void Update(){
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        transform.position = IA_Manager.instance.ApplyBound(transform.position);

        // var _NearFood = Physics.OverlapSphere(transform.position, _foodviewRadius, _foodMask);
        // var _NearBoid = Physics.OverlapSphere(transform.position, _allyViewRadius, _allyMask);
        // var _NearHunter = Physics.OverlapSphere(transform.position, _allyViewRadius, _hunterMask);
        // var _nearHunter = _NearHunter.Select(x => x.GetComponent<Hunter>()).ToList();
        _queries.selected = _queries.Query().Where(x => x.gameObject != gameObject);
        _NearBoids = _queries.selected
            .Select(x => x.GetComponent<Boid>()).Where(x => x != null).ToList();
        // _nearHunter = _queries.selected
        //     .Select(x => x.GetComponent<Hunter>())
        //     .Where(x => x != null).ToList();
        _NearFood = _queries.selected.FirstOrDefault(x => x.gameObject.CompareTag("Food"));


        Move(Separation(_NearBoids) * IA_Manager.instance.weightSeparation);
        Move(Cohesion(_NearBoids) * IA_Manager.instance.weightCohesion);
        Move(Alignment(_NearBoids) * IA_Manager.instance.weightAlignment);


        if (_NearFood != null)
        {
            if (!_isHunterNear)
            {
                Move(Arrive(_NearFood.transform.position));
                if (Vector3.Distance(_NearFood.transform.position, transform.position) < 1f)
                {
                    _NearFood.gameObject.SetActive(false);
                    _NearFood.transform.position = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
                    _NearFood.gameObject.SetActive(true);
                    _NearFood = null;
                }
            }
        }


        if (_nearHunter.Any())
        {
            _isHunterNear = true;
        }
        else
        {
            _isHunterNear = false;
        }

        if (_isHunterNear)
        {
            Move(Evade(_nearHunter[0]));
        }
    }

    private void RandomDirection(){
        float x = Random.Range(-1, 1);
        float z = Random.Range(-1, 1);
        Vector3 randomDir = new Vector3(x, 0, z).normalized * _maxForce;
        Move(randomDir);
    }

    void Move(Vector3 force){
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxForce);
        _gridEntity.DispatchOnMove();
    }

    Vector3 Arrive(Vector3 actualTarget){
        Vector3 desired = actualTarget - transform.position;
        float dist = desired.magnitude;
        desired.Normalize();
        if (dist <= _foodviewRadius)
        {
            desired *= _maxSpeed * (dist / _foodviewRadius);
        }
        else
        {
            desired *= _maxSpeed;
        }

        Vector3 steering = desired - _velocity;

        return steering;
    }

    Vector3 Alignment(IEnumerable<Boid> nearBoids){
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

    Vector3 Cohesion(IEnumerable<Boid> nearBoids){
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

    Vector3 Separation(IEnumerable<Boid> nearBoids){
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

    Vector3 Evade(Hunter target){
        Vector3 finalPos = target.transform.position + target.Velocity * Time.deltaTime;
        Vector3 desired = transform.position - finalPos;
        desired.Normalize();
        desired *= _maxForce;
        Vector3 steering = desired - _velocity;
        return steering;
    }

    Vector3 CalculateSteering(Vector3 desired){
        return Vector3.ClampMagnitude(desired - _velocity, _maxSpeed);
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _foodviewRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _allyViewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _hunterViewRadius);
    }

    public float CheckDistance(Vector3 position){
        return Vector3.Distance(transform.position, position);
    }
}