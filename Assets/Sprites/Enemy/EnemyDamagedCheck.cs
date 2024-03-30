using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using Unity.Profiling.LowLevel.Unsafe;

namespace mihoyoProj
{
    public class EnemyDamagedCheck : MonoBehaviour
    {
        public bool isDamaged = false;

        public bool InAssassinArea = false;
        public String Damagedtag;
        public GameObject player;
        public PlayerController playerController;

        private void OnTriggerEnter(Collider other){
            if(playerController.isHit && other.gameObject.tag == "PlayerAttack" && playerController.isAttacking){
                isDamaged = true;
                Damagedtag = other.gameObject.tag;
            }

            if(playerController.isHit && other.gameObject.tag == "PlayerBullet"){
                isDamaged = true;
                Damagedtag = other.gameObject.tag;
            }

            if(other.gameObject.tag == "PlayerAssassin"){
                InAssassinArea = true;
            }
        }

        private void OnTriggerExit(Collider other){
            if(other.gameObject.tag == "PlayerAssassin"){
                InAssassinArea = false;
            }
        }
    }
}
