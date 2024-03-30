using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace mihoyoProj
{
    public class PlayerUIStatus : MonoBehaviour
    {
        #region UI Variables
        public Canvas UI;
        public TextMeshProUGUI Points;
        public TextMeshProUGUI Skillcd;
        public TextMeshProUGUI SixShotscd;
        public TextMeshProUGUI TargetPoints;
        public TextMeshProUGUI Health;
        public TextMeshProUGUI RestartCount;
        public Slider QTE;
        public GameObject TryAgainText;
        public GameObject DiedText;
        public GameObject QTEText;
        public GameObject VictoryText;
        public GameObject RestartButtonObj;
        public GameObject ExitButtonObj;
        public GameObject BacktoMenuObj;
        public GameObject QTESliderObj;
        public UnityEngine.Color defaultColor;
        public float currentSkillcd;
        public float currentSixShotscd;
        public bool SkillCDRunning;
        public bool SixShotsRunning;
        #endregion

        void Start(){
            #region UI Initialization
            TryAgainText.SetActive(false);

            DiedText.SetActive(false);

            QTEText.SetActive(false);
            
            VictoryText.SetActive(false);

            QTESliderObj.SetActive(false);

            RestartButtonObj.SetActive(false);

            ExitButtonObj.SetActive(false);

            BacktoMenuObj.SetActive(false);

            defaultColor =QTE.fillRect.GetComponent<Image>().color;
            #endregion
        }

        void Update(){

            if(currentSkillcd > 0 && !SkillCDRunning){
                SkillCDRunning = true;
                StartCoroutine(SkillCDreading());
            }

            if(currentSixShotscd > 0 && !SixShotsRunning){
                SixShotsRunning = true;
                StartCoroutine(SixShotsCDreading());
            }
        }

        #region UI Functions
        public void SetPoints(int points){
            Points.text = points.ToString();
        }

        public void SetSkillcd(float skillcd){
            Skillcd.text = skillcd.ToString();
        }

        public void SetSixShotcd(float sixshotcd){
            SixShotscd.text = sixshotcd.ToString();
        }
        public void SetTargetPoints(int targetPoints){
            TargetPoints.text = targetPoints.ToString();
        }

        public void SetHealth(int health){
            Health.text = health.ToString();
        }

        public void SetRestartCount(int restartCount){
            RestartCount.text = restartCount.ToString();
        }

        public void SetTryAgainTextStatus(bool status){
            TryAgainText.SetActive(status);
        }

        public void SetDiedTextStatus(bool status){
            DiedText.SetActive(status);
        }

        public void SetQTE(float qte){
            if(qte <= 10) {QTE.value = qte / 10f;}
        }

        public void SetQTEcolor(UnityEngine.Color color){
            var fill = QTE.fillRect.GetComponent<Image>();
            fill.color = color;
        }
        #endregion

        IEnumerator SkillCDreading(){
            while(currentSkillcd > 0){
                currentSkillcd = (float)(Math.Floor(currentSkillcd * 10f) / 10f);
                currentSkillcd -= 0.1f;
                SetSkillcd(currentSkillcd);
                yield return new WaitForSeconds(0.1f);
            }
            currentSkillcd = 0;
            SkillCDRunning = false;
        }

        IEnumerator SixShotsCDreading(){
            while(currentSixShotscd > 0){
                currentSixShotscd = (float)(Math.Floor(currentSixShotscd * 10f) / 10f);
                currentSixShotscd -= 0.1f;
                SetSixShotcd(currentSixShotscd);
                yield return new WaitForSeconds(0.1f);
            }
            currentSixShotscd = 0;
            SixShotsRunning = false;
        }
    }
}
