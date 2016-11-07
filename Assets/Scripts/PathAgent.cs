using UnityEngine;
using System.Collections.Generic;

public class PathAgent : MonoBehaviour
{
    public float timeToLook = 0.5f;
    private float accumulatedTimeToLook;

    private GameObject clickedArea;
    private Waypoint clickedAreaWaypoint;
    private List<Waypoint> path;

    private int currentWaypointIndex;    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentWaypointIndex = 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                if (clickedArea == null)
                {
                    clickedArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(clickedArea.GetComponent<SphereCollider>());
                    clickedArea.name = "Clicked";

                    MeshRenderer renderer = clickedArea.GetComponent<MeshRenderer>();
                    renderer.material.color = Color.red;

                    clickedAreaWaypoint = clickedArea.AddComponent<Waypoint>();
                }

                clickedArea.transform.position = hit.point;
                clickedArea.transform.Translate(0, 0.5f, 0);

                Vector3 localPoint = hit.collider.gameObject.transform.root.InverseTransformPoint(hit.point);
                //Debug.LogFormat("Hit {0} at {1}", hit.collider.gameObject.transform.root.name, localPoint);

                Pathfinder pathfinder = hit.collider.gameObject.transform.root.GetComponent<Pathfinder>();
                //Waypoint endWaypoint = pathfinder.GetClosestWaypoint(localPoint);
                //Debug.LogFormat("Closest waypoint to clicked area is {0}", endWaypoint.name);

                //Waypoint startWaypoint = pathfinder.GetClosestWaypoint(transform.localPosition);
                //Debug.LogFormat("Closest waypoint to Agent is {0}", startWaypoint.name);

                path = pathfinder.GetPath(transform.localPosition, localPoint);
                //Debug.LogFormat("Path has {0} waypoints", path.Count);

                path.Add(clickedAreaWaypoint);
            }
        }

        if (path != null && currentWaypointIndex < path.Count)
        {
            Vector3 direction = path[currentWaypointIndex].transform.position - transform.position;
            direction.Normalize();
            transform.position += direction * 2 * Time.deltaTime;
            if (transform.position.Within(path[currentWaypointIndex].transform.position, 0.5f))
            {
                currentWaypointIndex++;
            }
        }

        accumulatedTimeToLook += Time.deltaTime;

        if (clickedArea != null && accumulatedTimeToLook >= timeToLook)
        {
            accumulatedTimeToLook = 0;

            if (Pathfinder.InLineOfSight(transform.position, clickedArea.transform.position))
            {
                // move the index to the end to prevent following the path
                //currentWaypointIndex = path.Count;

                currentWaypointIndex = 0;
                path.Clear();
                path.Add(clickedAreaWaypoint);
            }
        }


    }

    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, path[0].transform.position);

            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
            }

            Gizmos.DrawLine(clickedArea.transform.position, path[path.Count - 1].transform.position);
        }
    }
}
