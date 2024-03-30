using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace mihoyoProj
{
    public class Enemy02Controller : MonoBehaviour
    {
        public Enemy01Attribute enemy02Attribute;
        public Enemy01UIStatus enemy02UIStatus;
        private Renderer enemyRenderer;
        public float rushDistance; // 冲刺的距离
        public float rushTime = 0.5f; // 冲刺的时间
        public Rigidbody rb;
        public SphereCollider damagedCollider;
        public EnemyDamagedCheck enemyDamagedCheck;
        public bool isAttacking = false;
        public bool inCoolDown = false;
        public GameObject warningAreaBox;
        public MeshCollider warningCollider;
        public EnemyWarningCheck enemyWarningCheck;
        public Transform firePoint;
        public GameObject bulletPrefab1;
        public GameObject bulletPrefab2;
        public GameObject body;
        public bool isMoving = false;
        public int shootCal = 0;
        public GameObject player;
        public PlayerController playerController;
        public PlayerAttribute playerAttribute;
        public bool isInvicibled = false;
        public Vector3 staticPosition;
        public int AttackingMode = 0; // 0为普通攻击，1为特殊攻击
        private Color defaultColor;

        void Start()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            playerAttribute = player.GetComponent<PlayerAttribute>();
            enemyWarningCheck.player = player;
            enemyWarningCheck.playerController = playerController; 
            enemyWarningCheck = warningCollider.GetComponent<EnemyWarningCheck>();
            enemyDamagedCheck.player = player;
            enemyDamagedCheck.playerController = playerController;
            enemyDamagedCheck = damagedCollider.GetComponent<EnemyDamagedCheck>();
            
            enemyRenderer = body.GetComponent<Renderer>();
            enemy02Attribute.HP = enemy02Attribute.MaxHP;
            enemy02UIStatus.UpdateHealthBar((float)enemy02Attribute.HP, (float)enemy02Attribute.MaxHP);
            staticPosition = transform.position;
            defaultColor = enemyRenderer.material.color;
            StartCoroutine(MoveLogic());
        }

        // Update is called once per frame
        void Update()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            playerAttribute = player.GetComponent<PlayerAttribute>();
            enemyWarningCheck.player = player;
            enemyWarningCheck.playerController = playerController; 
            enemyWarningCheck = warningCollider.GetComponent<EnemyWarningCheck>();
            enemyDamagedCheck.player = player;
            enemyDamagedCheck.playerController = playerController;
            enemyDamagedCheck = damagedCollider.GetComponent<EnemyDamagedCheck>();

            AttackLogic();

            if(enemyDamagedCheck.InAssassinArea && playerController.AssassinSuccess){
                StartCoroutine(Kill());
            }

            enemy02UIStatus.UIFollowObject(gameObject);
            if(enemy02Attribute.HP <= 0){
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
            enemy02Attribute.HP -= 2;
            enemy02UIStatus.UpdateHealthBar((float)enemy02Attribute.HP, (float)enemy02Attribute.MaxHP);
            shootCal++;
            enemyRenderer.material.color = defaultColor;
        }

        void AttackLogic(){
            if(enemyWarningCheck.isInWarning && !isAttacking && !inCoolDown && playerController.anim.GetBool("IsSwichRole") == false){
                switch (AttackingMode){
                    case 0: 
                    StartCoroutine(NormalAttacking()); 
                    break;

                    case 1:
                    StartCoroutine(NormalAttacking()); 
                    break;
                    
                    case 2:
                    StartCoroutine(SpecialAttacking()); 
                    break;

                    case 3:
                    StartCoroutine(SpecialAttacking()); 
                    break;

                    default:
                    break;
                }
            }
        }

        IEnumerator AttackDamaged(){
                isInvicibled = true;
                playerController.isHit = false;
                enemyDamagedCheck.isDamaged = false;
                enemyRenderer.material.color = Color.red;

                enemy02Attribute.HP--;
                enemy02UIStatus.UpdateHealthBar((float)enemy02Attribute.HP, (float)enemy02Attribute.MaxHP);

                yield return new WaitForSeconds(0.5f);
                enemyRenderer.material.color = defaultColor;
                isInvicibled = false;
        }
        IEnumerator Kill(){
            yield return new WaitForSeconds(1.5f);
            playerAttribute.currentPoints += 10;
            Destroy(gameObject);
        }
        IEnumerator MoveLogic(){
            while(true){
                yield return new WaitForSeconds(enemy02Attribute.CheckInterval);
                if(enemyWarningCheck.isInWarning && !isMoving){
                    Vector3 targetPosition = player.transform.position;
                    Vector3 startPosition = transform.position;

                    // 保持y轴的高度不变
                    targetPosition.y = startPosition.y;

                    // 计算新的位置
                    Vector3 direction = targetPosition - startPosition;
                    direction.Normalize();

                    // 更新位置
                    staticPosition = transform.position; //更新绑定位置
                    StartCoroutine(SolveMoving(direction));

                }else if(!enemyWarningCheck.isInWarning && !isMoving){
                        // 生成一个随机的方向
                        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

                        Vector3 targetPosition = staticPosition + new Vector3(randomDirection.x, 0, randomDirection.y) * enemy02Attribute.MoveRadius;
                        Vector3 startPosition = transform.position;

                        // 保持y轴的高度不变
                        targetPosition.y = startPosition.y;

                        // 计算新的位置
                        Vector3 direction = targetPosition - startPosition;
                        direction.Normalize();

                        // 更新位置
                        StartCoroutine(SolveMoving(direction));

                }
            }
        }
        IEnumerator SolveMoving(Vector3 direction){
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction * enemy02Attribute.Speed * 100 * Time.deltaTime, enemy02Attribute.Speed * 50 * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);
            yield return new WaitForSeconds(enemy02Attribute.CheckInterval * 4f);
            isMoving = false;
        }
        IEnumerator NormalAttacking(){
            isAttacking = true;
            inCoolDown = true;
            for (int i = 0; i < 3; i++)
            {
                Vector3 firePosition = firePoint.position + firePoint.forward * 1f;

                GameObject bullet = Instantiate(bulletPrefab1, firePosition, firePoint.rotation);
                EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
                bulletScript.Shoot(transform.forward, enemy02Attribute.bulletSpeed, 10f);
                yield return new WaitForSeconds(1f);
            }
            isAttacking = false;
            yield return new WaitForSeconds(2f);
            AttackingMode = UnityEngine.Random.Range(0, 3);
            inCoolDown = false;
        }

        IEnumerator SpecialAttacking(){
            isAttacking = true;

            for (int i = 0; i < 3; i++)
            {
                Vector3 firePosition = firePoint.position + firePoint.forward * 1f;

                GameObject bullet = Instantiate(bulletPrefab2, firePosition, firePoint.rotation);
                EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
                bulletScript.Shoot(transform.forward, enemy02Attribute.bulletSpeed, 5f);
                yield return new WaitForSeconds(1f);
            }

            isAttacking = false;
            yield return new WaitForSeconds(2f);
            AttackingMode = UnityEngine.Random.Range(0, 3);
            inCoolDown = false;
        }
    }
}
