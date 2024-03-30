using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class DamagedCheck : MonoBehaviour
    {
        public bool isEnemy;
        public String Enemytag;
        private void OnTriggerStay(Collider other){
            if(other.gameObject.tag == "Player" || other.gameObject.tag == "HighDmg" || other.gameObject.tag == "LowDmg" || other.gameObject.tag == "Catch" || other.gameObject.tag == "Stun"){
                isEnemy = true;
                Enemytag = other.gameObject.tag;
            }
        }

        private void OnTriggerExit(Collider other){
            if(other.gameObject.tag == "Enemy" || other.gameObject.tag == "HighDmg" || other.gameObject.tag == "LowDmg" || other.gameObject.tag == "Catch" || other.gameObject.tag == "Stun"){
                isEnemy = false;
            }     
        }
    }
}
