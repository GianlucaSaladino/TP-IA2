using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Hunter : MonoBehaviour
{

    public static Hunter instance;
    public float speed;
    private float _maxForce = 4;
    private float _maxVelocity = 6;
    private float _chargeSpeed = 2.5f;
    private Vector3 velocity;
    FSM _fsm;
    [SerializeField] private float energy;
    [SerializeField] Transform[] _wayPoints;

    int _actualWaypoint = 1;
    int _lastWaypoint = 0;
    public float minDetectWaypoint = 0.5f;

    [Range(1, 10), SerializeField] private float _boidViewRadius;
    public bool _boidIsNear;
    private Boid nearestboid;


    //###########################################################################

    public SpatialGrid myGrid;
    
    private Material _material;
    
    
    private Boid currentTarget;
    [SerializeField] private List<Boid> _directTargets;
    [SerializeField] private List<Boid> _boidsInRange;

    [SerializeField] private Queries _boidRange;

    //###########################################################################

    public float Energy { get => energy; set => energy = value; }

    public Boid Nearestboid { get => nearestboid; set => nearestboid = value; }

    public Vector3 Velocity { get => velocity; set => velocity = value; }

    //###########################################################################
    // FSM IA 2
    public enum HunterActions
    {
        Chase,
        Idle,
        Patrol
    }

    private FSMEvent<HunterActions> _fsmEvent;
    //###########################################################################

   

    //IA2-P3
    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        

        var idle = new State<HunterActions>("IDLE");
        var chase = new State<HunterActions>("CHASE");
        var patrol = new State<HunterActions>("PATROL");

        StateConfigurer.Create(idle).SetTransition(HunterActions.Chase, chase).SetTransition(HunterActions.Patrol, patrol).Done();
        StateConfigurer.Create(chase).SetTransition(HunterActions.Idle, idle).SetTransition(HunterActions.Patrol, patrol).Done();
        StateConfigurer.Create(patrol).SetTransition(HunterActions.Idle, idle).SetTransition(HunterActions.Chase, chase).Done();

        idle.OnEnter += x =>
        {
            _material.color = Color.yellow;
            Debug.Log("IDLE");
        };
        
        idle.OnUpdate += () =>
        {
            ChargeEnergy();
            if (Energy >= 25)
            {
                if (_boidIsNear)
                {
                    SendInputToFSM(HunterActions.Chase);
                }
                else
                {
                    SendInputToFSM(HunterActions.Patrol);
                }
            }
        };
        
        patrol.OnEnter += x =>
        {
            _material.color = Color.green;
            Debug.Log("PATROL");
        };
        
        patrol.OnUpdate += () =>
        {
            //IA2-P2
            _boidsInRange = IA_Manager.instance.allBoids.Aggregate(FList.Create<Boid>(), (flist, boid) =>
            {
                Tuple<int, int> pos = myGrid.GetPositionInGrid(boid.transform.position);
                flist = boid.CheckDistance(transform.position) <= 15 && myGrid.IsInsideGrid(pos) ? flist + boid : flist;
                return flist;
            }).OrderBy(b => b.CheckDistance(transform.position)).ToList();

            
            var nearestTarget = _boidRange.Query();

            if (nearestTarget.Any())
            {
                foreach (var boid in _boidsInRange)
                {
                    if (GetDistance(transform.position, boid.transform.position) <= 10)
                    {
                        _directTargets.Add(boid);
                        _boidIsNear = true;
                        nearestboid = boid;
                        SendInputToFSM(HunterActions.Chase);
                    } 
                }
            }
            
            
            WaypointPatrol();

            if (Energy <= 0)
            {
                SendInputToFSM(HunterActions.Idle);
            }
        };
        
        chase.OnEnter += x =>
        {
            _material.color = Color.red;
            Debug.Log("CHASE");
        };

        chase.OnUpdate += () =>
        {
            if (GetDistance(transform.position, nearestboid.transform.position) > 10)
            {
                _boidIsNear = false;
                _directTargets.Clear();
                SendInputToFSM(HunterActions.Patrol);
            }

            transform.position += velocity * Time.deltaTime;
            //  transform.forward = velocity;
            Move(Pursuit(nearestboid));
            DecreaseEnergy();
        };

       

        _fsmEvent = new FSMEvent<HunterActions>(idle);
    }

    
    private float GetDistance(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(pos1, pos2);
    }

    private void Update()
    {
        
        _fsmEvent.Update(); //IA2-P3
        //_NearBoid = Physics.OverlapSphere(transform.position, _boidViewRadius);
        /*
        if (_NearBoid.Length > 0)
        {
            boidsList = _NearBoid.Aggregate(new List<Boid>(), (x, y) =>
            {
                if (y.TryGetComponent<Boid>(out var boid))
                {
                    x.Add(boid);
                }

                return x;
            }).Where((x) => Vector3.Distance(x.transform.position, transform.position) < 5).OrderBy((x) => Vector3.Distance(x.transform.position, transform.position)).Take(1).ToList();
            _boidIsNear = boidsList.Count > 0;
            if (_boidIsNear)
                nearestboid = boidsList[0];
        }
        else
        {
            _boidIsNear = false;
        }
        */
    }

    public void ChargeEnergy()
    {
        if (energy < 25)
        {
            energy += Time.deltaTime * _chargeSpeed;
        }
    }

    public void DecreaseEnergy()
    {
        energy -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _boidViewRadius);
    }

    //IA2-P3
    private void SendInputToFSM(HunterActions inp)
    {
        _fsmEvent.SendInput(inp);
    }

    private Vector3 Pursuit(Boid target)
    {
        Vector3 finalPos = target.transform.position + target.Velocity * Time.deltaTime;
        Vector3 desired = finalPos - transform.position;
        desired.Normalize();
        desired *= _maxVelocity;

        Vector3 steering = desired - velocity;

        return steering;
    }

    private void Move(Vector3 force)
    {
        velocity = Vector3.ClampMagnitude(velocity + force, _maxForce);
        Velocity = velocity;
    }

    private void WaypointPatrol()
    {
        var dir = _wayPoints[_actualWaypoint].position - transform.position;
        transform.position += dir.normalized * speed * 2 * Time.deltaTime;
        DecreaseEnergy();

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
