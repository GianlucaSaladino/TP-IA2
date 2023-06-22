using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : IState
{
    Transform _transform;
    float _speed;
    float _maxVelocity;
    float _maxForce;
    Vector3 _velocity;
    Boid _nearestBoid;
    FSM _fsm;

    public Chase(FSM fsm, Transform transform, float speed, float maxForce, float maxVelocity)
    {
        _fsm = fsm;
        _transform = transform;
        _speed = speed;
        _maxForce = maxForce;
        _maxVelocity = maxVelocity;
    }

    public void OnEnter()
    {
        Debug.Log("Enter Chase");
        _nearestBoid = Hunter.instance.Nearestboid;
    }

    public void OnExit()
    {
        Debug.Log("Exit Chase");
    }

    public void OnUpdate()
    {
        if (!Hunter.instance._boidIsNear)
        {
            _fsm.ChangeState("Patrol");
        }
        _transform.position += _velocity * Time.deltaTime;
        Move(Pursuit(_nearestBoid));

    }

    Vector3 Pursuit(Boid target)
    {
        Vector3 finalPos = target.transform.position + target.Velocity * Time.deltaTime;
        Vector3 desired = finalPos - _transform.position;
        desired.Normalize();
        desired *= _maxVelocity;

        Vector3 steering = desired - _velocity;

        return steering;
    }
    void Move(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxForce);
        Hunter.instance.Velocity = _velocity;
    }
}
