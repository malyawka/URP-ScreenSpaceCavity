using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    public float speed = 5.0f;
    public Transform target = default;

    private Transform _self = default;
    
    private void LateUpdate()
    {
        if (!_self)
            _self = transform;
        
        if (target)
        {
            _self.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);   
        }
    }

    private void OnDrawGizmos()
    {
        if (target)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(target.transform.position, 1.0f);   
        }
    }
}
