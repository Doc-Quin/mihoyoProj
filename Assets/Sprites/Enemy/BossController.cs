using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class BossController : MonoBehaviour
    {
        public Enemy01Attribute bossAttribute;
        public Enemy01UIStatus bossUIStatus;
        private Renderer enemyRenderer;
        public float rushDistance; // 冲刺的距离
        public float rushTime = 0.5f; // 冲刺的时间
        public Rigidbody rb;
        public BoxCollider bodydamagedCollider;
        public CapsuleCollider handdamagedCollider;
        public EnemyDamagedCheck bodyDamagedCheck;
        public EnemyDamagedCheck handDamagedCheck;
        public bool isAttacking = false;
        public bool inCoolDown = false;
        public GameObject warningAreaBox;
        public MeshCollider warningCollider;
        public EnemyWarningCheck enemyWarningCheck;
        public Transform firePoint;
        public GameObject bulletPrefab1;
        public GameObject bulletPrefab2;
        public GameObject bulletPrefab3;
        public GameObject bulletPrefab4;    
        public GameObject body;
        public GameObject hand;
        public bool isTurning = false;
        public bool isMoving = false;
        public int shootCal = 0;
        public GameObject player;
        public PlayerController playerController;
        public PlayerAttribute playerAttribute;
        public bool isInvicibled = false;
        public Vector3 staticPosition;
        public int AttackingMode = 0; // 0为普通攻击，1-4为特殊攻击, 5为转手臂

        public float rotationTime; // 旋转的总时间
        private Color defaultColor;

        void Start()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
            playerAttribute = player.GetComponent<PlayerAttribute>();   
            enemyWarningCheck.player = player;
            enemyWarningCheck.playerController = playerController; 
            enemyWarningCheck = warningCollider.GetComponent<EnemyWarningCheck>();
            bodyDamagedCheck.player = player;
            bodyDamagedCheck.playerController = playerController;
            bodyDamagedCheck = bodydamagedCollider.GetComponent<EnemyDamagedCheck>();
            handDamagedCheck.player = player;
            handDamagedCheck.playerController = playerController;
            handDamagedCheck = handdamagedCollider.GetComponent<EnemyDamagedCheck>();


            enemyRenderer = body.GetComponent<Renderer>();

            bossAttribute.HP = bossAttribute.MaxHP;
            bossUIStatus.UpdateHealthBar((float)bossAttribute.HP, (float)bossAttribute.MaxHP);
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
            bodyDamagedCheck.player = player;
            bodyDamagedCheck.playerController = playerController;
            bodyDamagedCheck = bodydamagedCollider.GetComponent<EnemyDamagedCheck>();
            handDamagedCheck.player = player;
            handDamagedCheck.playerController = playerController;
            handDamagedCheck = handdamagedCollider.GetComponent<EnemyDamagedCheck>();

            
            AttackLogic();

            bossUIStatus.UIFollowObject(gameObject);
            if(bossAttribute.HP <= 0){
                playerAttribute.currentPoints += 100;
                Destroy(gameObject);
            }
            if(bodyDamagedCheck.isDamaged && bodyDamagedCheck.Damagedtag == "PlayerBullet"){
                ShootDamaged();
            }
            if(isInvicibled){return;}
        }

        void FixedUpdate(){
            if(isInvicibled){return;}
            if(bodyDamagedCheck.isDamaged && bodyDamagedCheck.Damagedtag == "PlayerAttack"){
                StartCoroutine(AttackDamaged());
            }
        }

        void ShootDamaged(){
            playerController.isHit = false;
            bodyDamagedCheck.isDamaged = false;
            enemyRenderer.material.color = Color.red;
            bossAttribute.HP -= 2;
            bossUIStatus.UpdateHealthBar((float)bossAttribute.HP, (float)bossAttribute.MaxHP);
            shootCal++;
            enemyRenderer.material.color = defaultColor;
        }

        void AttackLogic(){
            if(enemyWarningCheck.isInWarning && !isAttacking && !inCoolDown && !isTurning && playerController.anim.GetBool("IsSwichRole") == false){
                switch (AttackingMode){
                    case 0: 
                    StartCoroutine(NormalAttacking()); 
                    break;

                    case 1:
                    StartCoroutine(NormalAttacking()); 
                    break;
                    
                    case 2:
                    StartCoroutine(SpecialAttacking(2)); 
                    break;

                    case 3:
                    StartCoroutine(SpecialAttacking(3)); 
                    break;

                    case 4:
                    StartCoroutine(SpecialAttacking(4)); 
                    break;

                    case 5:
                    StartCoroutine(TurnHands()); 
                    break;

                    default:
                    break;
                }
            }
        }

        IEnumerator AttackDamaged(){
                isInvicibled = true;
                playerController.isHit = false;
                bodyDamagedCheck.isDamaged = false;
                enemyRenderer.material.color = Color.red;

                bossAttribute.HP--;
                bossUIStatus.UpdateHealthBar((float)bossAttribute.HP, (float)bossAttribute.MaxHP);

                yield return new WaitForSeconds(0.5f);
                enemyRenderer.material.color = defaultColor;
                isInvicibled = false;
        }
        IEnumerator MoveLogic(){
            while(true){
                yield return new WaitForSeconds(bossAttribute.CheckInterval);
                if(enemyWarningCheck.isInWarning && !isMoving){
                    Vector3 targetPosition = player.transform.position;
                    Vector3 startPosition = body.transform.position;

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

                        Vector3 targetPosition = staticPosition + new Vector3(randomDirection.x, 0, randomDirection.y) * bossAttribute.MoveRadius;
                        Vector3 startPosition = body.transform.position;

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
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction * bossAttribute.Speed * 100 * Time.deltaTime, bossAttribute.Speed * 50 * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction);
            yield return new WaitForSeconds(bossAttribute.CheckInterval * 2f);
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
                bulletScript.Shoot(transform.forward, bossAttribute.bulletSpeed * 2f, 10f);
                yield return new WaitForSeconds(1f);
            }
            isAttacking = false;
            yield return new WaitForSeconds(2f);
            AttackingMode = UnityEngine.Random.Range(0, 6);
            inCoolDown = false;
        }
        IEnumerator SpecialAttacking(int attackingMode){
            isAttacking = true;
            inCoolDown = true;
            GameObject tempbullet = new GameObject();

            if(attackingMode == 2){
                tempbullet = bulletPrefab2;
            }

            if(attackingMode == 3){
                tempbullet = bulletPrefab3;
            }

            if(attackingMode == 4){
                tempbullet = bulletPrefab4;
            }


            for (int i = 0; i < 3; i++)
            {
                Vector3 firePosition = firePoint.position + firePoint.forward * 1f;

                GameObject bullet = Instantiate(tempbullet, firePosition, firePoint.rotation);
                EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
                bulletScript.Shoot(transform.forward, bossAttribute.bulletSpeed, 5f);
                yield return new WaitForSeconds(1f);
            }

            isAttacking = false;
            yield return new WaitForSeconds(2f);
            AttackingMode = UnityEngine.Random.Range(0, 5);
            inCoolDown = false;
        }
        IEnumerator TurnHands(){
            isTurning = true;
            inCoolDown = true;

            StopCoroutine(MoveLogic());
            yield return new WaitForSeconds(1f);
            
            float startTime = Time.time;
            while(Time.time - startTime < rotationTime){
                float t = (Time.time - startTime) / rotationTime;
                float speed = Mathf.Lerp(0, 360 * 5, t); // 从0加速到360*5度/秒
                hand.transform.RotateAround(firePoint.position, Vector3.up, speed * Time.deltaTime);
                yield return null;
            }

            AttackingMode = UnityEngine.Random.Range(0, 5);
            yield return new WaitForSeconds(1f);
            hand.transform.rotation = Quaternion.Euler(90, 0, 0);;
            isTurning = false;
            inCoolDown = false;
            StartCoroutine(MoveLogic());
        }
    }
}
