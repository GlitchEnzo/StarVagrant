using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
    public bool drawNetworkGizmo = true;
    public List<Waypoint> neighbors;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);

        Handles.color = Color.black;
        //Vector3 direction = (Camera.main.transform.position - transform.position);
        //direction.Normalize();
        Handles.Label(transform.position + new Vector3(0, 1.0f, 0) /*+ direction * 10 */, name);

        if (drawNetworkGizmo && neighbors != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < neighbors.Count; i++)
            {
                Gizmos.DrawLine(transform.position, neighbors[i].transform.position);
            }
        }
    }
}
