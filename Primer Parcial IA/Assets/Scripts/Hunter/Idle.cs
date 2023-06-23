using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : IState
{
    FSM _fsm;

    public Idle(FSM fsm)
    {
        _fsm = fsm;
    }

    public void OnEnter()
    {
        Debug.Log("EMPEZO IDLE");
    }

    public void OnUpdate()
    {
        Hunter.instance.ChargeEnergy();
        if (Hunter.instance.Energy >= 10)
        {
            if (Hunter.instance._boidIsNear)
            {
                _fsm.ChangeState("Chase");
            }
            else{
                _fsm.ChangeState("Patrol");
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("TERMINO IDLE");
    }
}
