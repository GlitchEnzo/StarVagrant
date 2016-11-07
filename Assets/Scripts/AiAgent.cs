namespace StarVagrant
{
    using UnityEngine;
    using System.Collections;

    public class AiAgent : MonoBehaviour
    {
        public Transform target;

        public float velocity;

        void Start()
        {

        }
        
        void Update()
        {
            Vector3 directionToTarget = target.position - transform.position;

            transform.position += directionToTarget * velocity * Time.deltaTime;
        }
    }
}