using System;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    public bool onGrid;
    public Vector3 velocity = new Vector3(0, 0, 0);

    private void Update()
    {
        if (velocity != Vector3.zero)
        {
            OnMove(this);
        }
    }
}
