using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace mihoyoProj
{
    public class Enemy01Controller : MonoBehaviour
    {
        public Enemy01Attribute enemy01Attribute;
        public Enemy01UIStatus enemy01UIStatus;
        private Renderer enemyRenderer;
        public float rushDistance; // 冲刺的距离
        public float rushTime = 0.5f; // 冲刺的时间
        public Rigidbody rb;
        public BoxCollider damagedCollider;
        public EnemyDamagedCheck enemyDamagedCheck;
        public int shootCal = 0;
        public GameObject player;
        public PlayerController playerController;
        public PlayerAttribute playerAttribute;
        public bool isInvicibled = false;

        public GameObject body;

        private Color defaultColor;

        void Start()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            playerAttribute = player.GetComponent<PlayerAttribute>();
            enemyDamagedCheck.player = player;
            enemyDamagedCheck.playerController = playerController;
            enemyDamagedCheck = damagedCollider.GetComponent<EnemyDamagedCheck>();  

            enemyRenderer = body.GetComponent<Renderer>();
            enemy01Attribute.HP = enemy01Attribute.MaxHP;
            enemy01UIStatus.UpdateHealthBar((float)enemy01Attribute.HP, (float)enemy01Attribute.MaxHP);
            defaultColor = enemyRenderer.material.color;
        }

        // Update is called once per frame
        void Update()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            playerAttribute = player.GetComponent<PlayerAttribute>();
            enemyDamagedCheck.player = player;
            enemyDamagedCheck.playerController = playerController;
            enemyDamagedCheck = damagedCollider.GetComponent<EnemyDamagedCheck>();
            
            if(enemyDamagedCheck.InAssassinArea && playerController.AssassinSuccess){
                StartCoroutine(Kill());
            }

            enemy01UIStatus.UIFollowObject(gameObject);
            
            if(enemy01Attribute.HP <= 0){
                playerAttribute.currentPoints += 10;
                Destroy(gameObject);
            }
            if(enemyDamagedCheck.isDamaged && enemyDamagedCheck.Damagedtag == "PlayerBullet"){
                ShootDamaged();
            }
            if(isInvicibled){return;}
        }

        void FixedUpdate(){
            if(isInvicibled){return;}
            if(enemyDamagedCheck.isDamaged && enemyDamagedCheck.Damagedtag == "PlayerAttack"){
                StartCoroutine(AttackDamaged());
            }
        }
        
        void ShootDamaged(){
            playerController.isHit = false;
            enemyDamagedCheck.isDamaged = false;
            enemyRenderer.material.color = Color.red;
            enemy01Attribute.HP -= 2;
            enemy01UIStatus.UpdateHealthBar((float)enemy01Attribute.HP, (float)enemy01Attribute.MaxHP);
            shootCal++;
            enemyRenderer.material.color = defaultColor;
        }
        
        IEnumerator Kill(){
            yield return new WaitForSeconds(1.5f);
            playerAttribute.currentPoints += 10;
            Destroy(gameObject);
        }

        IEnumerator AttackDamaged(){
                isInvicibled = true;
                playerController.isHit = false;
                enemyDamagedCheck.isDamaged = false;
                enemyRenderer.material.color = Color.red;

                enemy01Attribute.HP--;
                enemy01UIStatus.UpdateHealthBar((float)enemy01Attribute.HP, (float)enemy01Attribute.MaxHP);

                yield return new WaitForSeconds(0.5f);
                enemyRenderer.material.color = defaultColor;

                yield return new WaitForSeconds(0.7f);
                isInvicibled = false;
        }
    }
}
