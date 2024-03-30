using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class EnemyBullet : MonoBehaviour
    {
        Rigidbody rb;
    
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
    
        public void Shoot(Vector3 direction, float force, float destroyTime)
        {   
            rb.AddForce(direction * force);
            Destroy(gameObject, destroyTime);
        }
    
        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "Player"){
                Destroy(gameObject);
            }

            if(other.gameObject.tag == "Environment"){
                Destroy(gameObject);
            }
        }
    }
}
