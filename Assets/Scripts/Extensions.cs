using UnityEngine;

public static class Extensions
{
    public static bool Within(this Vector3 positionA, Vector3 positionB, float distance)
    {
        return Vector3.Distance(positionA, positionB) <= distance;
    }
}
