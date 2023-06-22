using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : IState
{
    Transform _transform;
    float _speed;
    FSM _fsm;
    int _actualWaypoint = 1;
    int _lastWaypoint = 0;
    Transform[] _wayPoints;
    public float minDetectWaypoint = 0.5f;

    public Patrol(FSM fsm, Transform transform, float speed, Transform[] wayPoints)
    {
        _fsm = fsm;
        _transform = transform;
        _speed = speed;
        _wayPoints = wayPoints;
    }

    public void OnEnter()
    {
        Debug.Log("ENTER Patrol");
    }

    public void OnExit()
    {
        Debug.Log("Exit Patrol");
    }

    public void OnUpdate()
    {
        WaypointPatrol();

        if (Hunter.instance._boidIsNear)
        {
            _fsm.ChangeState("Chase");
        }

        if (Hunter.instance.Energy <= 0)
        {
            _fsm.ChangeState("Idle");
        }
    }

    private void WaypointPatrol()
    {
        var dir = _wayPoints[_actualWaypoint].position - _transform.position;
        _transform.position += dir.normalized * _speed * Time.deltaTime;
        Hunter.instance.DecreaseEnergy();

        if (dir.magnitude <= minDetectWaypoint)
        {
            if (_actualWaypoint > _lastWaypoint)
            {
                _actualWaypoint++;
                _lastWaypoint = _actualWaypoint - 1;
            }
            else
            {
                _actualWaypoint--;
                _lastWaypoint = _actualWaypoint + 1;
            }

            if (_actualWaypoint >= _wayPoints.Length)
            {
                _actualWaypoint = _wayPoints.Length - 1;
                _lastWaypoint = _wayPoints.Length - 1;
            }
            else if (_actualWaypoint == 0)
            {
                _actualWaypoint = 1;
                _lastWaypoint = 0;
            }
        }
    }
}
