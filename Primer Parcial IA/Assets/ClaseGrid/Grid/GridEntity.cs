using System;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
    public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    public event Action<GridEntity> OnMove = delegate { };

    public void DispatchOnMove()
    {
        OnMove(this);
    }
}
