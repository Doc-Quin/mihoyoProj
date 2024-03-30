using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Cinemachine;
using TMPro;
using System.Linq.Expressions;
using System.Diagnostics.Tracing;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace mihoyoProj
{
    public class PlayerController : MonoBehaviour
    {
        #region Variables
        public bool isLocked = false;
        public Collider playerCollider; // reference to the player's collider
        public float speedlevel;
        public bool useCharacterForward = false;
        public bool lockToCameraForward = false;
        public float turnSpeed = 8f;
        public KeyCode sprintJoystick = KeyCode.JoystickButton2;
        public KeyCode sprintKeyboard = KeyCode.LeftShift;
        private float turnSpeedMultiplier;
        private float speed = 0f;
        private float direction = 0f;
        private bool isSprinting = false;
        public Animator anim;
        private Vector3 targetDirection;
        private Vector3 currentDirection;
        private Vector3 StartPosition;
        private Vector2 input;
        private Quaternion freeRotation;
        private Camera mainCamera;
        private float velocity;
        public CinemachineVirtualCamera Cam01;
        private Vector3 lastPosition;
        private Rigidbody rb;
        public float jumpForce = 2f;
        public float groundCheckDistance = 0.1f;
        public float fallThresholdTime = 0.5f; // 设置下落阈值时间
        public float fallTimer = 0; // 初始化下落计时器
        public GameObject gc; //地面检测
        public GroundCheck gcScript; //地面检测脚本
        public GameObject sa; //前方检测
        public EnemyCheck saScript; //前方检测脚本
        public bool StartFallingClock = false; // 是否开始下落计时器
        public int RunoutClick = 0; // QTE次数
 
        #region Player Status Variables

        public bool IsBacktoMenuActive = false; // 是否返回菜单激活
        public bool inCatching = false;
        public bool isCatched = false; // 是否被抓住
        public CinemachineFreeLook cfl; // 自由视角组件
        public bool AssassinSuccess = false; // 是否成功突袭
        public float defaultYAxis; // 默认的Y轴位置
        public float defaultXAxis; // 默认的X轴位置 
        public PlayerAttribute playerAttribute; // 玩家属性脚本
        public int RandomStandnumber;
        public PlayerUIStatus uiStatus; // 玩家状态脚本
        public bool IsInvincibled = false;
        public bool isFalling = false;
        public bool isJumping = false;
        public bool IsRushJumping = false;
        #endregion

        #region Controller Variables
        public float rushDistance; // 冲刺的距离
        public float rushTime = 0.5f; // 冲刺的时间
        private float rushSpeed; // 冲刺的速度
        public float jumpCooldown; // 跳跃的冷却时间
        private float jumpTimer = 0; // 跳跃的计时器
        public int jumpCount = 0; // 跳跃的次数
        public int attackClick = 0;
        private int previousAttack;
        public bool isAttacking = false;
        public GameObject bulletPrefab;
        private float lastSixShotTime = 0f; // 记录上一次触发SixShot的时间
        public float SixShotCooldown = 4f; // SixShot的冷却时间
        private float lastSkillTime = 0f; // 记录上一次触发E技能的时间
        public float SkillCooldown = 4f; // 技能的冷却时间
        public float lastAttackTime = 0f; // 记录上一次攻击的时间
        public float AttackCooldown = 1f; // 攻击的冷却时间
        public float lastEvadeTime = 0f; // 记录上一次回避的时间
        public float EvadeCooldown = 3f; // 回避的冷却时间
        public bool Skilling = false;

        #endregion

        #region judge box binder
        public GameObject ps;
        public Transform firePoint;
        public GameObject leftlegbox;
        public GameObject rightlegbox;
        public GameObject righthandbox;
        public GameObject lefthandbox;
        public GameObject damagedBox;
        public EnemyCheck leftlegScript; //左腿攻击脚本
        public EnemyCheck rightlegScript; //右腿攻击脚本
        public EnemyCheck righthandScript; //右手攻击脚本
        public EnemyCheck lefthandScript; //左手攻击脚本
        public EnemyCheck damagedScript; //受伤脚本

        public bool isHit = false;
        #endregion

        #endregion
        void Start()
        {
            ReadPlayerData();
            defaultXAxis = cfl.m_XAxis.Value;
            defaultYAxis = cfl.m_YAxis.Value;
            rb = GetComponent<Rigidbody>();
            rushSpeed = rushDistance / rushTime;
            anim = GetComponent<Animator>();
	        mainCamera = Camera.main;
            StartPosition = transform.position;

            ps.SetActive(false);
            gcScript = gc.GetComponent<GroundCheck>();
            saScript = sa.GetComponent<EnemyCheck>();
            leftlegScript = leftlegbox.GetComponent<EnemyCheck>();
            rightlegScript = rightlegbox.GetComponent<EnemyCheck>();
            righthandScript = righthandbox.GetComponent<EnemyCheck>();
            lefthandScript = lefthandbox.GetComponent<EnemyCheck>();
            damagedScript = damagedBox.GetComponent<EnemyCheck>();
            
            playerAttribute.HP = playerAttribute.MaxHP;
            uiStatus.SetHealth(playerAttribute.HP);
            uiStatus.SetRestartCount(playerAttribute.Life);

            StartCoroutine(GenerateRandomNumber());
            StartCoroutine(CheckValueChange());
        }
        void Update(){

            #region QuitGame
            if (Input.GetKeyDown(KeyCode.Escape)){
                if(IsBacktoMenuActive == true){
                    isLocked = false;
                    IsBacktoMenuActive = false;
                    uiStatus.BacktoMenuObj.SetActive(false);
                }else{
                    IsBacktoMenuActive = true;
                    isLocked = true;
                    uiStatus.BacktoMenuObj.SetActive(true);
                }
            }
            #endregion

            if(inCatching){
                if(Input.GetKeyDown(KeyCode.Space)){
                    RunoutClick++; 
                    uiStatus.SetQTE((float)RunoutClick);
                }
            }

            if(IsInvincibled){damagedScript.isEnemy = false;}

            #region Restart
            if (Input.GetKeyDown(KeyCode.R) && isLocked){
                StartCoroutine(Restart());
            }
            #endregion

            if(playerAttribute.HP <= 0 && !isLocked){StartCoroutine(SetFail(playerAttribute.Life));}
            if(isLocked){return;}

            #region Victory Sending PlayerInfo
            if(playerAttribute.currentPoints >= playerAttribute.TargetPoints){
                cfl.m_XAxis.Value = defaultXAxis;
                cfl.m_YAxis.Value = defaultYAxis;
                anim.Rebind();
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsVictory");
                isLocked = true;
                SaveCurrentData();
                StartCoroutine(GotoNextLevel());
            }
            #endregion

            if(damagedScript.isEnemy && !IsInvincibled && damagedScript.Enemytag != "Catch"){StartCoroutine(Damaged(damagedScript.Enemytag));}
            if(damagedScript.isEnemy && !IsInvincibled && damagedScript.Enemytag == "Catch"){StartCoroutine(CatchPlayer());}

            if(isAttacking && (lefthandScript.isDamaged || righthandScript.isDamaged || leftlegScript.isDamaged || rightlegScript.isDamaged)){isHit = true;}
            if(!Input.anyKey){anim.SetBool("IsStop", true);}
            anim.SetInteger("JumpCount", jumpCount);

            EnterCheck();
        }
        void FixedUpdate()
        {
            #region Refresh Status
            uiStatus.SetPoints(playerAttribute.currentPoints);
            uiStatus.SetTargetPoints(playerAttribute.TargetPoints);
            #endregion

            if(isLocked){return;}

            #region Falling

            if (transform.position.y < lastPosition.y && !anim.GetCurrentAnimatorStateInfo(0).IsName("Appear")) {
            if (fallTimer > fallThresholdTime) {
                isFalling = true; 
            } else {
                fallTimer += Time.fixedDeltaTime; // 增加计时器
            }
            } else {
                isFalling = false;
                fallTimer = 0; // 重置计时器
            }

            lastPosition = transform.position;

            if(isFalling && jumpTimer <= 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Appear")){
                anim.SetBool("IsFreeFall", true);
                anim.SetTrigger("Refresh");
                if(!StartFallingClock) StartCoroutine(FallandDie(3f));
            } else {
                anim.SetBool("IsFreeFall", false);
            }
            #endregion
            
            FixEnterCheck();

            #if ENABLE_LEGACY_INPUT_MANAGER
            input.x = Input.GetAxis("Horizontal");
	        input.y = Input.GetAxis("Vertical");

            if (useCharacterForward)
                speed = Mathf.Abs(input.x) + input.y;
            else
                speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

            speed = Mathf.Clamp(speed, 0f, 1f);
            speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
            anim.SetFloat("Speed", speed);

            if (input.y < 0f && useCharacterForward)
                direction = input.y;
	        else
                direction = 0f;

            anim.SetFloat("Direction", direction);

            # region Running
            isSprinting = (Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard) && input != Vector2.zero && direction >= 0f);
            anim.SetBool("IsRun", isSprinting);  
            if(isSprinting){
                speedlevel = playerAttribute.RunSpeed;
                anim.SetInteger("RandomRun", Random.Range(0, 1));
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsMoving");
                anim.SetBool("IsStop", false);       
            }else{
                speedlevel = playerAttribute.WalkSpeed;
            }
            #endregion

            UpdateTargetDirection();
            if (input != Vector2.zero && targetDirection.magnitude > 0.1f)
            {
                Vector3 lookDirection = targetDirection.normalized;
                freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
                var eulerY = transform.eulerAngles.y;

                if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
                var euler = new Vector3(0, eulerY, 0);

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), turnSpeed * turnSpeedMultiplier * Time.deltaTime);
            }
            #else
            InputSystemHelper.EnableBackendsWarningMessage();
            #endif
        }
        public virtual void UpdateTargetDirection()
        {
            if (!useCharacterForward)
                {
                turnSpeedMultiplier = 1f;
                var forward = mainCamera.transform.TransformDirection(Vector3.forward);
                forward.y = 0;

                //get the right-facing direction of the referenceTransform
                var right = mainCamera.transform.TransformDirection(Vector3.right);

                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                targetDirection = input.sqrMagnitude > 0 ? input.x * right + input.y * forward : forward;
            }
            else
            {
                turnSpeedMultiplier = 0.2f;
                var forward = transform.TransformDirection(Vector3.forward);
                forward.y = 0;

                //get the right-facing direction of the referenceTransform
                var right = transform.TransformDirection(Vector3.right);
                targetDirection = input.sqrMagnitude > 0 ? input.x * right + Mathf.Abs(input.y) * forward : forward;
            }
        }
        public void FixEnterCheck(){
            #region move
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && moveCheck()){
                anim.SetBool("IsStop", false);
                anim.SetTrigger("IsMoving");
                anim.SetTrigger("Refresh");
                currentDirection = transform.forward;
                transform.position += transform.forward * speed * Time.deltaTime * speedlevel;
                defaultXAxis = 0f;
            }
            if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && moveCheck()){
                anim.SetBool("IsStop", false);
                anim.SetTrigger("IsMoving");
                anim.SetTrigger("Refresh");
                currentDirection = -transform.forward;
                transform.position += transform.forward * speed * Time.deltaTime * speedlevel;
                defaultXAxis = 180f;
            }
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && moveCheck()){
                anim.SetBool("IsStop", false);
                anim.SetTrigger("IsMoving");
                anim.SetTrigger("Refresh");
                currentDirection = transform.right;
                transform.position += transform.forward * speed * Time.deltaTime * speedlevel;
            }
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && moveCheck()){
                anim.SetBool("IsStop", false);
                anim.SetTrigger("IsMoving");
                anim.SetTrigger("Refresh");
                currentDirection = -transform.right;
                transform.position += transform.forward * speed * Time.deltaTime * speedlevel;
            }
            #endregion

            #region Attack
            if (Input.GetKey(KeyCode.J) && Time.time >= lastAttackTime + AttackCooldown){
                lastAttackTime = Time.time;
                attackClick++;
                Attack(attackClick);
                if(attackClick > 0) {isAttacking = true;}
            }
            #endregion
        }
        public void EnterCheck(){
            #region JumpCheck
            // 如果跳跃计时器大于0，那么减小计时器
            if (jumpTimer > 0){
            jumpTimer -= Time.deltaTime;
            }

            // 如果角色在地面上，重置跳跃次数
            if (gcScript.isGrounded && !Input.GetKey(KeyCode.K)){
                jumpCount = 0;
                anim.SetInteger("JumpCount", 0);
                isJumping = false;
                anim.SetBool("IsJump", false);
            }
            #endregion

            #region Jump
            if (Input.GetKeyDown(KeyCode.K))
            {
                //一段跳
                if (!isJumping && gcScript.isGrounded)
                {
                    isJumping = true;
                    jumpCount++;
                    anim.SetInteger("JumpCount", jumpCount);
                    anim.Rebind();
                    anim.ResetTrigger("Appear");
                    anim.SetTrigger("Refresh");
                    anim.SetTrigger("IsMoving");
                    anim.SetTrigger("IsJump");
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                }
                //二段跳
                else if (isJumping && jumpCount == 1)
                {
                    jumpCount++;
                    anim.SetInteger("JumpCount", jumpCount);
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce * 2, rb.velocity.z);
                    // 跳跃后，设置跳跃计时器为冷却时间
                    jumpTimer = jumpCooldown;
                }else{
                    jumpTimer = jumpCooldown;
                }
            }
            #endregion

            #region RushJump
            if (Input.GetKeyDown(KeyCode.L) && moveCheck()){
                StartCoroutine(LockForSecondsRushJump(0.5f));
            }
            #endregion

            #region EvadeForward
            if (Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.Space) && Time.time >= lastEvadeTime + EvadeCooldown){
                StartCoroutine(GetEvadingForwardInvisible(1f));
                LockForSecondsEvadeForward();
            }
            #endregion

            #region EvadeBackward
            if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Space) && Time.time >= lastEvadeTime + EvadeCooldown){
                StartCoroutine(GetEvadingBackwardInvisible(1f));
                LockForSecondsEvadeBackward();
            }
            #endregion

            #region Skill
            if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastSkillTime + SkillCooldown){
                lastSkillTime = Time.time;
                Skilling = true;
                StartCoroutine(Skill());
            }
            #endregion

            #region SixShot
            if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastSixShotTime + SixShotCooldown){
                lastSixShotTime = Time.time;
                StartCoroutine(Shoot());
            }
            #endregion

            #region Assassin
            if (Input.GetKeyDown(KeyCode.F) && saScript.isEnemy && !saScript.isInWarningArea){
                StartCoroutine(Assassin());
            }
            #endregion

            #region Reset Camera
            if(Input.GetKeyDown(KeyCode.Y) && !isLocked){
                StartCoroutine(ResetCamera());
            }
            #endregion

            #region Died
            if (Input.GetKeyDown(KeyCode.KeypadPlus)){
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsDead");
                isLocked = true;
            }
            #endregion

            #region Victory
            if (Input.GetKeyDown(KeyCode.KeypadMinus)){
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsVictory");
                isLocked = true;
            }
            #endregion

            #region Fail
            if (Input.GetKeyDown(KeyCode.KeypadMultiply)){
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsFail");
                isLocked = true;
            }
            #endregion

            #region Cheat
            if (Input.GetKeyDown(KeyCode.C)){
                playerAttribute.currentPoints += 50;
            }
            #endregion
        }
        public bool moveCheck(){
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("RunStopLeft") || anim.GetCurrentAnimatorStateInfo(0).IsName("RunStopRight") 
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Appear") || anim.GetCurrentAnimatorStateInfo(0).IsName("SwitchRole")  
            || anim.GetCurrentAnimatorStateInfo(0).IsName("RushJump") || anim.GetCurrentAnimatorStateInfo(0).IsName("EvadeForward") 
            || anim.GetCurrentAnimatorStateInfo(0).IsName("EvadeBackward")) {
                return false;
            }else{
                return true;
            }
        }
        public void Attack(int AttackClick){      
            switch(AttackClick){
                case 1:
                    anim.Rebind();
                    anim.ResetTrigger("Appear");
                    anim.SetBool("IsStop", false);
                    anim.SetTrigger("Refresh");
                    anim.SetTrigger("IsAttack");
                break;

                case 2:
                    anim.Rebind();
                    anim.SetBool("IsStop", false);
                    anim.ResetTrigger("Appear");
                    anim.SetTrigger("Refresh");
                    anim.SetTrigger("IsAttack");
                    anim.SetTrigger("Attack-2");
                break;

                case 3:
                    anim.Rebind();
                    anim.SetBool("IsStop", false);
                    anim.ResetTrigger("Appear");
                    anim.SetTrigger("Refresh");
                    anim.SetTrigger("IsAttack");
                    anim.SetTrigger("Attack-3");
                break;

                case 4:
                    anim.Rebind();
                    anim.SetBool("IsStop", false);
                    anim.ResetTrigger("Appear");
                    anim.SetTrigger("Refresh");
                    anim.SetTrigger("IsAttack");
                    anim.SetTrigger("Attack-4");
                break;
                
                case 5:
                        anim.Rebind();
                        anim.SetBool("IsStop", false);
                        anim.ResetTrigger("Appear");
                        anim.SetTrigger("Refresh");
                        anim.SetTrigger("IsAttack");
                        anim.SetTrigger("Attack-5");
                        isAttacking = false;
                break;

                default:
                attackClick = 0;
                break;
            }
        }
        public void LockForSecondsEvadeForward()
        {
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsEvadeForward");
        }
        public void LockForSecondsEvadeBackward()
        {
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsEvadeBackward");
        }
        public void SaveCurrentData(){
            PlayerGamingData.Instance.HP= playerAttribute.HP;
            PlayerGamingData.Instance.Life = playerAttribute.Life;
            PlayerGamingData.Instance.currentPoints = playerAttribute.currentPoints;
            PlayerGamingData.Instance.TargetPoints = playerAttribute.TargetPoints + 100;
        }
        public void ReadPlayerData(){ 
            playerAttribute.HP = PlayerGamingData.Instance.HP;
            playerAttribute.Life = PlayerGamingData.Instance.Life;
            playerAttribute.currentPoints = PlayerGamingData.Instance.currentPoints;
            playerAttribute.TargetPoints = PlayerGamingData.Instance.TargetPoints;
        }

        IEnumerator GenerateRandomNumber()
        {
            while (true)
            {
                RandomStandnumber = Random.Range(0, 3);
                yield return new WaitForSecondsRealtime(3);
                if(RandomStandnumber != 0){
                    anim.SetInteger("RandomStand", RandomStandnumber);
                    yield return new WaitForSecondsRealtime(2); 
                    RandomStandnumber = 0;
                    anim.SetInteger("RandomStand", RandomStandnumber); 
                    yield return new WaitForSecondsRealtime(4); 
                }else if(RandomStandnumber == 0){
                    anim.SetInteger("RandomStand", RandomStandnumber); 
                }
            }
        }

        IEnumerator LockForSecondsRushJump(float seconds)
        {
            if(IsRushJumping){yield return null;}
            IsRushJumping = true;
            IsInvincibled = true;
            isLocked = true; // 锁定操作
            rb.useGravity = false;
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsMoving");
            anim.SetTrigger("IsRushJump");
            yield return new WaitForSeconds(seconds); // 等待指定的秒数
            Vector3 startPosition = transform.position; // 开始位置
            Vector3 endPosition = transform.position + transform.forward * rushDistance * 1.5f; // 结束位置

            float elapsedTime = 0; // 已经过去的时间

            while (elapsedTime < rushTime)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rushTime * 1.5f); // 在开始位置和结束位置之间插值
                elapsedTime += Time.deltaTime; // 增加已经过去的时间
                yield return null; // 等待下一帧
            }

            transform.position = endPosition; // 确保物体到达结束位置
            IsRushJumping = false;
            IsInvincibled = false;
            rb.useGravity = true;
            isLocked = false; // 解锁操作
        }
        IEnumerator CheckValueChange()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (Mathf.Abs(attackClick - previousAttack) > Mathf.Epsilon)
                {
                    previousAttack = attackClick;
                }else if(!isLocked){
                    isAttacking = false;
                    anim.SetTrigger("IsStopAttack");
                    anim.ResetTrigger("IsDamaging");
                    isHit = false;
                }

                if(Time.time >= lastAttackTime + 2f * AttackCooldown){
                    attackClick = 0;
                    isAttacking = false;
                }
            }
        }
        IEnumerator Shoot()
        {
            isLocked = true; // 锁定操作
            uiStatus.SetSixShotcd(SixShotCooldown);
            uiStatus.currentSixShotscd = (float)SixShotCooldown;
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("IsAttack");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsShot");
            UpdateTargetDirection();

            // 连续发射6次子弹
            for (int i = 0; i < 6; i++)
            {
                Vector3 firePosition = firePoint.position + firePoint.up * 1.2f;
                GameObject bullet = Instantiate(bulletPrefab, firePosition, firePoint.rotation);
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                bulletScript.Shoot(transform.forward, 300f, 2f);
                yield return new WaitForSeconds(0.2f);
            }

            isLocked = false; // 解锁操作
        }
        IEnumerator Skill()
        {
            isLocked = true; // 锁定操作
            uiStatus.SetSkillcd(SkillCooldown);
            uiStatus.currentSkillcd = (float)SkillCooldown;
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsAttack");
            anim.SetTrigger("IsSkill");
            UpdateTargetDirection();

            float[] angles = new float[10]; // 希望偏离的角度
            for (int i = 0; i < 10; i++)
            {
                angles[i] = Random.Range(-50, 50); // 为数组的每一个元素生成一个随机角度
            }
            Vector3 axis = Vector3.up; // 旋转的轴

            // 创建一个绕Y轴旋转angle度的四元数
            Quaternion[] rotation = new Quaternion[10];
            for(int i = 0; i < 10; i++){
                rotation[i] = Quaternion.AngleAxis(angles[i], axis);
            }

            // 用这个四元数来旋转前向向量
            Vector3[] forwardRound = new Vector3[10];
            for(int i = 0; i < 10; i++){
                forwardRound[i] = rotation[i] * transform.forward;
                forwardRound[i].Normalize(); // 归一化，使其长度为1
            }

            for(int i = 0; i < 10; i++){
                Vector3 firePosition = firePoint.position + firePoint.up * 1.2f;
                GameObject bullet = Instantiate(bulletPrefab, firePosition, firePoint.rotation);
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                bulletScript.Shoot(forwardRound[i], 300f, 2f);
                yield return new WaitForSeconds(0.1f);
            }
            isLocked = false; // 解锁操作
        }
        IEnumerator GetEvadingForwardInvisible(float seconds){ 
            IsInvincibled = true;
            yield return new WaitForSeconds(0.3f);
            transform.position = transform.position + transform.forward * 1f;
            yield return new WaitForSeconds(seconds - 0.3f);
            IsInvincibled = false;
        }
        IEnumerator GetEvadingBackwardInvisible(float seconds){ 
            IsInvincibled = true;
            yield return new WaitForSeconds(0.3f);
            transform.position = transform.position - transform.forward * 1f;
            yield return new WaitForSeconds(seconds - 0.3f);
            IsInvincibled = false;
        }
        IEnumerator Assassin()
        {
            AssassinSuccess = true;
            isLocked = true; // 锁定操作
            IsInvincibled = true;
            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsAttack");
            anim.SetTrigger("IsAssassin");
            UpdateTargetDirection();
            yield return new WaitForSeconds(4f);
            AssassinSuccess = false;
            IsInvincibled = false;
            isLocked = false; // 解锁操作
        }
        IEnumerator Damaged(System.String damagetag)
        {
            isLocked = true; // 锁定操作
            IsInvincibled = true;

            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("IsDamaging");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsDamaged");

            if(damagetag == "LowDmg"){
                anim.SetTrigger("IsLowDmg");
                playerAttribute.HP--;
                uiStatus.SetHealth(playerAttribute.HP);
                ps.SetActive(true);
            }else if(damagetag == "HighDmg"){
                anim.SetTrigger("IsHighDmg");
                playerAttribute.HP -= 2;
                uiStatus.SetHealth(playerAttribute.HP);
                ps.SetActive(true);
            }else if(damagetag == "Stun"){
                anim.SetBool("IsStun", true);
                playerAttribute.HP--;
                uiStatus.SetHealth(playerAttribute.HP);
                yield return new WaitForSeconds(3f);
                anim.SetBool("IsStun", false);
                ps.SetActive(true);
            }else if(damagetag == "Enemy"){
                anim.SetTrigger("IsLowDmg");
                playerAttribute.HP--;
                uiStatus.SetHealth(playerAttribute.HP);
                ps.SetActive(true);
            }

            yield return new WaitForSeconds(0.3f);

            if(damagetag == "LowDmg"){
                //击退
                Vector3 startPosition = transform.position; // 开始位置
                Vector3 endPosition = transform.position - transform.forward * rushDistance * 0.3f; // 结束位置
                
                float elapsedTime = 0; // 已经过去的时间

                while (elapsedTime < rushTime)
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rushTime * 0.3f); // 在开始位置和结束位置之间插值
                    elapsedTime += Time.deltaTime; // 增加已经过去的时间
                    yield return null; // 等待下一帧
                }
                transform.position = endPosition; // 确保物体到达结束位置
            }

            if(damagetag == "HighDmg"){
                //击退
                Vector3 startPosition = transform.position; // 开始位置
                Vector3 endPosition = transform.position - transform.forward * rushDistance * 0.6f; // 结束位置
                
                float elapsedTime = 0; // 已经过去的时间

                while (elapsedTime < rushTime)
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rushTime * 0.6f); // 在开始位置和结束位置之间插值
                    elapsedTime += Time.deltaTime; // 增加已经过去的时间
                    yield return null; // 等待下一帧
                }
                transform.position = endPosition; // 确保物体到达结束位置
            }

            isLocked = false; // 解锁操作
            yield return new WaitForSeconds(2f);
            ps.SetActive(false);
            IsInvincibled = false;
        }
        IEnumerator SetFail(int restartCount){
            if(restartCount <= 0){
                isLocked = true;
                anim.Rebind();
                anim.ResetTrigger("Appear");
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsDead");
                rb.useGravity = false;
                if(isFalling){
                    rb.transform.position = StartPosition;
                }
                playerAttribute.HP = 0;
                uiStatus.SetHealth(0);
                yield return new WaitForSeconds(3f);
                uiStatus.SetDiedTextStatus(true);
                uiStatus.RestartButtonObj.SetActive(true);
                uiStatus.ExitButtonObj.SetActive(true);

            }else{
                anim.Rebind();
                anim.ResetTrigger("Appear");
                anim.SetTrigger("Refresh");
                anim.SetTrigger("IsFail");
                playerAttribute.HP = 0;
                uiStatus.SetHealth(0);
                isLocked = true;
                yield return new WaitForSeconds(2f);
                uiStatus.SetTryAgainTextStatus(true);
                rb.useGravity = true;
                if(isFalling){
                    rb.transform.position = StartPosition;
                }
            }
        }
        IEnumerator FallandDie(float seconds){
            StartFallingClock = true;
            yield return new WaitForSeconds(seconds);
            if(isFalling){
                rb.useGravity = false;
                UpdateTargetDirection();
                StartCoroutine(SetFail(playerAttribute.Life));
            }else if(!isFalling){
                StartFallingClock = false;
            }
        }
        IEnumerator ResetCamera(){
            isLocked = true;
            UpdateTargetDirection();
            cfl.m_XAxis.Value = defaultXAxis;
            cfl.m_YAxis.Value = defaultYAxis;
            yield return new WaitForSeconds(0.5f);
            isLocked = false;
        }
        IEnumerator Restart(){
            IsInvincibled = true;
            ps.SetActive(true);
            playerAttribute.HP = playerAttribute.MaxHP;
            playerAttribute.Life--;
            uiStatus.SetRestartCount(playerAttribute.Life);
            uiStatus.SetHealth(playerAttribute.HP);
            uiStatus.SetTryAgainTextStatus(false);
            anim.SetTrigger("IsRestart");
            StartFallingClock = false;
            isLocked = false;
            yield return new WaitForSeconds(5f);
            ps.SetActive(false);
            IsInvincibled = false;
        }
        IEnumerator CatchPlayer(){
            isLocked = true;
            IsInvincibled = true;
            inCatching = true;

            anim.Rebind();
            anim.ResetTrigger("Appear");
            anim.SetTrigger("IsDamaging");
            anim.SetTrigger("Refresh");
            anim.SetTrigger("IsDamaged");
            anim.SetTrigger("IsCatched");

            int WaitSeconds = 3 + Random.Range(1, 3);

            uiStatus.QTEText.SetActive(true);
            uiStatus.QTESliderObj.SetActive(true);
            uiStatus.SetQTE(0);

            yield return new WaitForSeconds(2f);
            if(RunoutClick >= 10){
                uiStatus.SetQTEcolor(Color.blue);
                yield return new WaitForSeconds(0.2f);
                RunoutClick = 0;
                uiStatus.SetQTE(0);
                uiStatus.SetQTEcolor(uiStatus.defaultColor);
                uiStatus.QTEText.SetActive(false);
                uiStatus.QTESliderObj.SetActive(false);
                anim.Rebind();
                anim.ResetTrigger("Appear");
                anim.SetTrigger("Refresh");
                anim.SetTrigger("QTE");

                yield return new WaitForSeconds(1f);

                yield return new WaitForSeconds(0.3f);
                //击退
                Vector3 startPosition = transform.position; // 开始位置
                Vector3 endPosition = transform.position - transform.forward * rushDistance * 0.3f; // 结束位置
                
                float elapsedTime = 0; // 已经过去的时间

                while (elapsedTime < rushTime)
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rushTime * 0.3f); // 在开始位置和结束位置之间插值
                    elapsedTime += Time.deltaTime; // 增加已经过去的时间
                    yield return null; // 等待下一帧
                }
                transform.position = endPosition; // 确保物体到达结束位置

            }else{
                yield return new WaitForSeconds(0.2f);
                RunoutClick = 0;
                uiStatus.SetQTE(0);
                uiStatus.SetQTEcolor(uiStatus.defaultColor);
                uiStatus.QTEText.SetActive(false);
                uiStatus.QTESliderObj.SetActive(false);

                yield return new WaitForSeconds(WaitSeconds);
                anim.SetTrigger("IsThrow");

                yield return new WaitForSeconds(0.3f);
                //击退
                Vector3 startPosition = transform.position; // 开始位置
                Vector3 endPosition = transform.position - transform.forward * rushDistance * 2f; // 结束位置
                
                float elapsedTime = 0; // 已经过去的时间

                while (elapsedTime < rushTime)
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / rushTime * 2f); // 在开始位置和结束位置之间插值
                    elapsedTime += Time.deltaTime; // 增加已经过去的时间
                    yield return null; // 等待下一帧
                }
                transform.position = endPosition; // 确保物体到达结束位置

                playerAttribute.HP -= 3;
                uiStatus.SetHealth(playerAttribute.HP);
            }

            inCatching = false;
            isLocked = false;
            ps.SetActive(true);
            yield return new WaitForSeconds(2f);
            IsInvincibled = false;
        }
        IEnumerator GotoNextLevel(){
            if(PlayerGamingData.Instance.levelnumber < PlayerGamingData.Instance.FinalLeveLnumber){
                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene(++PlayerGamingData.Instance.levelnumber);
            }

            if(PlayerGamingData.Instance.levelnumber == PlayerGamingData.Instance.FinalLeveLnumber){
                yield return new WaitForSeconds(3f);
                uiStatus.VictoryText.SetActive(true);
                uiStatus.RestartButtonObj.SetActive(true);
                uiStatus.ExitButtonObj.SetActive(true);
            }
        }
    }
}

