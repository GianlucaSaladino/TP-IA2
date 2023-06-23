using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Vector3 _velocity;
    [SerializeField, Range(1, 10)] int _maxSpeed;
    [SerializeField, Range(1, 10)] int _maxForce;
    float _foodviewRadius = 5;
    float _allyViewRadius = 3;
    float _hunterViewRadius = 4;
    float _separationViewRadius = 1;
    float _radiusSeparation;

    [SerializeField] LayerMask _foodMask;
    [SerializeField] LayerMask _allyMask;
    [SerializeField] LayerMask _hunterMask;

    bool _isFoodNear;
    bool _isAllyNear;
    [SerializeField] bool _isHunterNear;

    public Vector3 Velocity { get => _velocity; set => _velocity = value; }

    void Start()
    {
        BoidManager.instance.AddBoid(this);
        RandomDirection();
        
    }


    void Update()
    {
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
        transform.position = IA_Manager.instance.ApplyBound(transform.position);

        var _NearFood = Physics.OverlapSphere(transform.position, _foodviewRadius, _foodMask);
        var _NearBoid = Physics.OverlapSphere(transform.position, _allyViewRadius, _allyMask);
        var _NearHunter = Physics.OverlapSphere(transform.position, _allyViewRadius, _hunterMask);

        //IA2-P1
      var _nearHunter =  _NearHunter.Select(x => x.GetComponent<Hunter>()).ToList();
        foreach (var item in _NearFood)
        {
            if (!_isHunterNear)
            {
                Move(Arrive(item.transform.position));
            }
            if ((item.transform.position - transform.position).magnitude < 1)
            {
                Destroy(item.gameObject);
                RandomDirection();
            }
        }

        foreach (var item in _NearBoid)
        {

            Move(Separation(_NearBoid) * IA_Manager.instance.weightSeparation);
            Move(Cohesion(_NearBoid) * IA_Manager.instance.weightCohesion);
            Move(Alignment(_NearBoid) * IA_Manager.instance.weightAlignment);
        }
        if (_NearHunter.Length > 0)
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
    
    public float CheckDistance(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }

    private void RandomDirection()
    {
        float x = Random.Range(-1, 1);
        float z = Random.Range(-1, 1);
        Vector3 randomDir = new Vector3(x, 0, z).normalized * _maxForce;
        Move(randomDir);
    }

    void Move(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxForce);
    }

    Vector3 Arrive(Vector3 actualTarget)
    {
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

    Vector3 Alignment(Collider[] nearBoids)
    {
       //IA2-P1

        Vector3 desired = nearBoids
    .Where(item => item != this && (item.transform.position - transform.position).magnitude <= _allyViewRadius)
    .Aggregate(Vector3.zero, (current, item) => current + item.GetComponent<Boid>()._velocity);

        int count = nearBoids
            .Count(item => item != this && (item.transform.position - transform.position).magnitude <= _allyViewRadius);

        if (count <= 0)
        {
            return desired;
        }

        desired /= count;

        desired.Normalize();

        desired *= _maxForce;

        return CalculateSteering(desired);
    }

    Vector3 Cohesion(Collider[] nearBoids)
    {
        //IA2-P1

        Vector3 desired = nearBoids
     .Where(item => item != this && (item.transform.position - transform.position).magnitude <= _allyViewRadius)
     .Aggregate(Vector3.zero, (current, item) => current + item.transform.position);

        int count = nearBoids
            .Count(item => item != this && (item.transform.position - transform.position).magnitude <= _allyViewRadius);

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

    Vector3 Separation(Collider[] nearBoids)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _foodviewRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _allyViewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _hunterViewRadius);
    }
}
