using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace mihoyoProj
{
    public class EnemyCheck : MonoBehaviour
    {
        public bool isEnemy;
        public bool isDamaged;

        public bool isInWarningArea = false;
        public String Enemytag;
        void Start(){
        }
        private void OnTriggerEnter(Collider other){
            if(other.gameObject.tag == "HighDmg" || other.gameObject.tag == "LowDmg" || other.gameObject.tag == "Catch" || other.gameObject.tag == "Stun"){
                isEnemy = true;
                Enemytag = other.gameObject.tag;
            }

            if(other.gameObject.tag == "Enemy"){
                isEnemy = true;
                isDamaged = true;
                Enemytag = other.gameObject.tag;
            }

            if(other.gameObject.tag == "EnemyWarningArea"){
                isInWarningArea = true;
            }
        }

        private void OnTriggerExit(Collider other){
            if(other.gameObject.tag == "HighDmg" || other.gameObject.tag == "LowDmg" || other.gameObject.tag == "Catch" || other.gameObject.tag == "Stun"){
                isEnemy = false;
            }  

            if(other.gameObject.tag == "Enemy"){
                isEnemy = false;
                isDamaged = false;
                Enemytag = other.gameObject.tag;
            } 

            if(other.gameObject.tag == "EnemyWarningArea"){
                isInWarningArea = false;
            }  
        }
    }
}

