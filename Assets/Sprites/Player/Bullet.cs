using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class Bullet : MonoBehaviour
    {
        Rigidbody rb;
        public GameObject player;
        public PlayerController playerController;
    
        void Awake()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody>();
        }
    
        public void Shoot(Vector3 direction, float force, float destroyTime)
        {   
            rb.AddForce(direction * force);
            Destroy(gameObject, destroyTime);
        }
    
        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "Enemy"){
                playerController.isHit = true;
                Destroy(gameObject);
            }

            if(other.gameObject.tag == "Environment"){
                Destroy(gameObject);
            }
        }
    }
}
