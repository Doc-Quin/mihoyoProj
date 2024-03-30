using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class EnemyWarningCheck : MonoBehaviour
    {
        public bool isInWarning = false;
        public GameObject player;
        public PlayerController playerController;

        private void OnTriggerEnter(Collider other){
            if(other.gameObject == player){
                isInWarning = true;
            }
        }

        private void OnTriggerExit(Collider other){
            if(other.gameObject == player){
                isInWarning = false;
            }
        }
    }
}
