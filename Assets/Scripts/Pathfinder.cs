using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    private List<Waypoint> waypoints;
    
    void Start()
    {
        waypoints = GetComponentsInChildren<Waypoint>().ToList();

        GenerateNetwork();
    }

    public void GenerateNetwork()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].neighbors.Clear();
        }

        // auto-link neighbors based on raycast visibility
        for (int i = 0; i < waypoints.Count; i++)
        {
            for (int j = i + 1; j < waypoints.Count; j++)
            {
                //Vector3 direction = waypoints[j].transform.position - waypoints[i].transform.position;
                //float distance = direction.magnitude;
                //direction.Normalize();

                //bool hitObstacle = Physics.Raycast(waypoints[i].transform.position, direction, distance);

                //if (!hitObstacle)
                if (InLineOfSight(waypoints[i].gameObject, waypoints[j].gameObject))
                {
                    waypoints[i].neighbors.Add(waypoints[j]);
                    waypoints[j].neighbors.Add(waypoints[i]);
                }
            }
        }
    }

    public static bool InLineOfSight(Vector3 worldA, Vector3 worldB)
    {
        Vector3 direction = worldB - worldA;
        float distance = direction.magnitude - 0.2f; // make it a little smaller to not collide with the B object itself
        direction.Normalize();

        return !Physics.Raycast(worldA, direction, distance);
    }

    public static bool InLineOfSight(GameObject objectA, GameObject objectB)
    {
        Vector3 direction = objectB.transform.position - objectA.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;
        if (Physics.Raycast(objectA.transform.position, direction, out hit, distance))
        {
            // if the first object we hit was objectB, it must be in line of sight
            return hit.collider.gameObject == objectB;
        }

        // it didn't hit anything, so it must be in line of sight
        return true;
    }

    public static bool InLineOfSight(Vector3 worldA, GameObject objectB)
    {
        Vector3 direction = objectB.transform.position - worldA;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;
        if (Physics.Raycast(worldA, direction, out hit, distance))
        {
            // if the first object we hit was objectB, it must be in line of sight
            return hit.collider.gameObject == objectB;
        }

        // it didn't hit anything, so it must be in line of sight
        return true;
    }

    private float DistanceHeuristic(Waypoint pointA, Waypoint pointB)
    {
        return Vector3.Distance(pointA.transform.position, pointB.transform.position);
    }

    public Waypoint GetClosestWaypoint(Vector3 localPosition)
    {
        float closestDistance = float.MaxValue;
        Waypoint closest = null;

        Vector3 worldPosition = transform.TransformPoint(localPosition);

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (InLineOfSight(worldPosition, waypoints[i].gameObject))
            {
                float distance = Vector3.Distance(worldPosition, waypoints[i].transform.position);

                //Debug.LogFormat("{0} is in line of sight at a distance of {1}", waypoints[i].name, distance);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = waypoints[i];
                }
            }
        }

        return closest;
    }

    public List<Waypoint> GetPath(Vector3 localStartPosition, Vector3 localEndPosition)
    {
        // find closest VISIBLE waypoint to start and end
        Waypoint startWaypoint = GetClosestWaypoint(localStartPosition);
        //Debug.LogFormat("Starting waypoint = {0}", startWaypoint.name);

        Waypoint endWaypoint = GetClosestWaypoint(localEndPosition);
        //Debug.LogFormat("Ending waypoint = {0}", endWaypoint.name);

        // find shortest path connecting waypoints
        List<Waypoint> path = AStar(startWaypoint, endWaypoint);

        if (path == null)
        {
            Debug.LogErrorFormat("Path is null!");
        }
        else
        {
            //foreach (var waypoint in path)
            //{
            //    Debug.LogFormat("{0}", waypoint.name);
            //}
        }

        return path;
    }

    #region AStar
    //private List<Waypoint> totalPath = new List<Waypoint>();

    private List<Waypoint> AStar(Waypoint start, Waypoint goal)
    {
        // The set of nodes already evaluated.
        List<Waypoint> closedSet = new List<Waypoint>();

        // The set of currently discovered nodes still to be evaluated.
        // Initially, only the start node is known.
        List<Waypoint> openSet = new List<Waypoint>();
        openSet.Add(start);

        // For each node, which node it can most efficiently be reached from.
        // If a node can be reached from many nodes, cameFrom will eventually contain the
        // most efficient previous step.
        Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();

        // For each node, the cost of getting from the start node to that node.
        Dictionary<Waypoint, float> gScore = new Dictionary<Waypoint, float>();
        // TODO: default all to infinity... BELOW        

        // For each node, the total cost of getting from the start node to the goal
        // by passing by that node. That value is partly known, partly heuristic.
        Dictionary<Waypoint, float> fScore = new Dictionary<Waypoint, float>();

        // TODO: default all to infinity...
        foreach (Waypoint waypoint in waypoints)
        {
            gScore[waypoint] = float.MaxValue;
            fScore[waypoint] = float.MaxValue;
        }

        // The cost of going from start to start is zero.
        gScore[start] = 0;

        // For the first node, that value is completely heuristic.
        // For now, the heuristic will be the straight line distance between the two points.
        fScore[start] = DistanceHeuristic(start, goal);

        while (openSet.Count > 0)
        {
            // get the node in openSet having the lowest fScore[] value
            //Waypoint current = fScore.OrderBy(kvp => kvp.Value).First().Key;
            Waypoint current = null;
            float minF = float.MaxValue;
            for (int i = 0; i < openSet.Count; i++)
            {
                Waypoint openWaypoint = openSet[i];
                if (fScore[openWaypoint] < minF)
                {
                    minF = fScore[openWaypoint];
                    current = openWaypoint;
                }
            }

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Waypoint neighbor in current.neighbors)
            {
                if (closedSet.Contains(neighbor))
                {
                    // Ignore the neighbor which is already evaluated.
                    continue;
                }

                // The distance from start to a neighbor
                float tentativeGScore = gScore[current] + DistanceHeuristic(current, neighbor);

                if (!openSet.Contains(neighbor))
                {
                    // Discover a new node
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    // This is not a better path.
                    continue; 
                }

                // START PJM
                //var existingNeighbor = cameFrom.FirstOrDefault(x => x.Value == current).Key;
                //if (existingNeighbor != null)
                //{
                //    // another neighbor had already been added... compare the two
                //    if (gScore[existingNeighbor] <= tentativeGScore)
                //    {
                //        // existing neighbor is a better path
                //        continue;
                //    }
                //    else
                //    {
                //        cameFrom.Remove(existingNeighbor);
                //    }
                //}
                // END PJM

                // This path is the best until now. Record it!
                //Debug.LogFormat("Adding {0} to path.", neighbor.name);
                //Debug.LogFormat("Adding {0} --> {1}", neighbor, current);
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + DistanceHeuristic(neighbor, goal);
            }
        }

        return null;
    }

    private List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint current)
    {
        List<Waypoint> totalPath = new List<Waypoint>();
        //totalPath.Clear();
        //totalPath.Add(current);

        foreach (Waypoint waypoint in cameFrom.Keys)
        {
            Waypoint neighbor = cameFrom[waypoint];
            //Debug.LogFormat("Building {0} --> {1}", waypoint, neighbor);
            totalPath.Add(neighbor);
        }

        totalPath.Add(current);

        //totalPath.Reverse();

        return totalPath;
    }
    #endregion
}
