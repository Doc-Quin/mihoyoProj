using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class GroundCheck : MonoBehaviour
    {
        public bool isGrounded;
        private void OnTriggerStay(Collider other){
            if(other.gameObject.tag == "Environment"){
                isGrounded = true;
            } 
        }

        private void OnTriggerExit(Collider other){
            if(other.gameObject.tag == "Environment"){
                isGrounded = false;
            } 
        }
    }
}
