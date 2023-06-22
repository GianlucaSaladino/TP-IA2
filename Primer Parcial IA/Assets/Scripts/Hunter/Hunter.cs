using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    
    private Renderer _renderer;
    public SpatialGrid myGrid;
    
    [SerializeField] private List<GridEntity> targets;
    
    [SerializeField] private Material idleMat;
    [SerializeField] private Material patrolMat;
    [SerializeField] private Material chaseMat;
    //[SerializeField] private Material attackMat;
    [SerializeField] private Material restMat;
    
    private List<GridEntity> acceptableBoids;
    
    private GridEntity currentTarget;
    
    [SerializeField] private float queryLenght;
    [SerializeField] private Queries targetRange;
    [SerializeField] private Queries evadeRange;
    
    private Vector3 lowEnd, highEnd;


    
    
    
    public float speed;
    private float _maxForce = 4;
    private float _maxVelocity = 6;
    private float _chargeSpeed = 2.5f;
    [Range(1, 10), SerializeField] private float _boidviewRadius;
    public bool _boidIsNear;
    private Boid nearestboid;
    private Vector3 velocity;
    [SerializeField] LayerMask _boidMask;
    FSM _fsm;
    [SerializeField] private float energy;
    [SerializeField] Transform[] _wayPoints;
    public static Hunter instance;

    int _actualWaypoint = 1;
    int _lastWaypoint = 0;
    public float minDetectWaypoint = 0.5f;

    [SerializeField] private Collider[] _NearBoid;
    [SerializeField] private List<Boid> pogspog = new List<Boid>();
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
        
        
        var idle = new State<HunterActions>("IDLE");
        var chase = new State<HunterActions>("CHASE");
        var patrol = new State<HunterActions>("PATROL");

        StateConfigurer.Create(idle).SetTransition(HunterActions.Chase, chase).SetTransition(HunterActions.Patrol, patrol).Done();
        StateConfigurer.Create(chase).SetTransition(HunterActions.Idle, idle).SetTransition(HunterActions.Patrol, patrol).Done();
        StateConfigurer.Create(patrol).SetTransition(HunterActions.Idle, idle).SetTransition(HunterActions.Chase, chase).Done();

        idle.OnUpdate += () =>
        {
            ChargeEnergy();
            if (Energy >= 10)
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

        chase.OnUpdate += () =>
        {
            if (!_boidIsNear)
            {
                SendInputToFSM(HunterActions.Patrol);
            }

            transform.position += velocity * Time.deltaTime;
            //  transform.forward = velocity;
            Move(Pursuit(nearestboid));
        };

        patrol.OnUpdate += () =>
        {
            WaypointPatrol();

            if (_boidIsNear)
            {
                SendInputToFSM(HunterActions.Chase);
            }

            if (Energy <= 0)
            {
                SendInputToFSM(HunterActions.Idle);
            }
        };

        _fsmEvent = new FSMEvent<HunterActions>(idle);
    }

    private void Start()
    {
        // _fsm = new FSM();
        // _fsm.CreateState("Idle", new Idle(_fsm));
        // _fsm.CreateState("Chase", new Chase(_fsm, transform, speed, _maxVelocity, _maxForce));
        // _fsm.CreateState("Patrol", new Patrol(_fsm, transform, speed * 2, _wayPoints));
        //
        // _fsm.ChangeState("Idle");
    }

    void Update()
    {
        // _fsm.Execute();
        _fsmEvent.Update(); //IA2-P3
        _NearBoid = Physics.OverlapSphere(transform.position, _boidviewRadius);
        if (_NearBoid.Length > 0)
        {
            pogspog = _NearBoid.Aggregate(new List<Boid>(), (x, y) =>
            {
                if (y.TryGetComponent<Boid>(out var boid))
                {
                    x.Add(boid);
                }

                return x;
            }).Where((x) => Vector3.Distance(x.transform.position, transform.position) < 5).OrderBy((x) => Vector3.Distance(x.transform.position, transform.position)).ToList(); //_NearBoid.Where((x)=>x.GetComponent<Boid>()!=null).OrderBy((x) => Vector3.Distance(x.transform.position, transform.position)).First();
            _boidIsNear = pogspog.Count > 0;
            if (_boidIsNear)
                nearestboid = pogspog[0]; //[0].GetComponent<Boid>();
        }
        else
        {
            _boidIsNear = false;
        }
    }

    public void ChargeEnergy()
    {
        if (energy < 10)
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
        Gizmos.DrawWireSphere(transform.position, _boidviewRadius);
    }

    //IA2-P3
    private void SendInputToFSM(HunterActions inp)
    {
        _fsmEvent.SendInput(inp);
    }

    Vector3 Pursuit(Boid target)
    {
        Vector3 finalPos = target.transform.position + target.Velocity * Time.deltaTime;
        Vector3 desired = finalPos - transform.position;
        desired.Normalize();
        desired *= _maxVelocity;

        Vector3 steering = desired - velocity;

        return steering;
    }

    void Move(Vector3 force)
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
