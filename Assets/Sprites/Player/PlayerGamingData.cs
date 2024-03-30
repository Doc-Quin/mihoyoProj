using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace mihoyoProj
{
    public class PlayerGamingData : MonoBehaviour
    {
        public static PlayerGamingData Instance { get; private set; }
        public int HP { get; set; } = 0;
        public int MaxHP { get; set; } = 5;
        public int Attack { get; set; } = 0;
        public int Defense { get; set; } = 0;
        public int WalkSpeed { get; set; } = 0;
        public int RunSpeed { get; set; } = 0;
        public int CriticalRate {get; set; } = 0;
        public int CriticalDamage {get; set; } = 0;
        public int currentPoints {get; set;} = 0;
        public int TargetPoints {get; set; } = 100;
        public int Life { get; set; } = 1;
        public int levelnumber { get; set; } = 1;
        public int FinalLeveLnumber = 2;

        public Animator anim1;
        public Animator anim2;
        public GameObject P1;
        public PlayerController P1Controller;
        public GameObject P2;
        public PlayerController P2Controller;
        public Animator currentAnim { get; set; }
        public PlayerController currentController { get; set; }
        public GameObject currentPlayerObj { get; set; }
        public Vector3 currentPos { get; set; }
        public int currentPlayerNumber { get; set; }
        public int tempPoints;
        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }

            P1Controller = P1.GetComponent<PlayerController>();
            P2Controller = P2.GetComponent<PlayerController>(); 
            
            currentPlayerNumber = 1;
            currentAnim = anim1;
            currentController = P1Controller;
            currentPlayerObj = P1;
        }

        void Update() {
            #region Swich
            if(Input.GetKeyDown(KeyCode.P) && currentAnim.GetBool("IsSwichRole") == false){
                currentPos = currentPlayerObj.transform.position;
                tempPoints = currentController.playerAttribute.currentPoints;
                currentPoints = tempPoints;
                StartCoroutine(SwitchRole());
            }
            #endregion
        }

        IEnumerator SwitchRole()
        {
            currentController.isLocked = true; // 锁定操作
            currentController.Cam01.Priority = 11;
            
            currentAnim.Rebind();
            currentAnim.ResetTrigger("Appear");
            currentAnim.SetTrigger("Refresh");
            currentAnim.SetBool("IsSwichRole", true);
            currentPlayerObj.transform.position = currentPos + Vector3.up * 3f;
            yield return new WaitForSeconds(0.5f); // 等待指定的秒数
            currentController.Cam01.Priority = 9;
            currentPlayerObj.SetActive(false);
            
            currentPlayerObj = currentPlayerObj == P1? P2 : P1;
            currentPlayerNumber = currentPlayerNumber == 1? 2 : 1;
            currentAnim = currentPlayerObj.GetComponent<Animator>();
            currentController = currentPlayerObj.GetComponent<PlayerController>();
            currentController.playerAttribute.currentPoints = tempPoints;
            currentPlayerObj.transform.position = currentPos;

            currentPlayerObj.SetActive(true);
            currentController.uiStatus.SetPoints(currentController.playerAttribute.currentPoints);

            currentAnim.Rebind();
            currentAnim.SetBool("IsSwichRole", false);
            currentController.isLocked = false; // 解锁操作
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.buildIndex == 0){
                Destroy(gameObject);
                return;
            }

            P1 = GameObject.Find("Player01");
            P2 = GameObject.Find("Player02");
            P1Controller = P1.GetComponent<PlayerController>();
            P2Controller = P2.GetComponent<PlayerController>(); 
            anim1 = P1.GetComponent<Animator>();
            anim2 = P2.GetComponent<Animator>();

            if(currentPlayerNumber == 1){
                currentPlayerNumber = 1;
                currentAnim = anim1;
                currentController = P1Controller;
                currentPlayerObj = P1;
            }else{
                currentPlayerNumber = 2;
                currentAnim = anim2;
                currentController = P2Controller;
                currentPlayerObj = P2;
            }

            currentPos = currentPlayerObj.transform.position;
            
            if(currentPlayerNumber == 1){P2.SetActive(false);}else{P1.SetActive(false);}
            
        }
    }
}
