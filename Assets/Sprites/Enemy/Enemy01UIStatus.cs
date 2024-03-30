using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace mihoyoProj
{
    public class Enemy01UIStatus : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Canvas UI;

        public void UIFollowObject(GameObject target){
            Vector3 targetPosition = target.transform.position;
            UI.transform.position = new Vector3(targetPosition.x, targetPosition.y + 1f, targetPosition.z);
        }
        public void UpdateHealthBar(float currentValue, float maxValue){
            slider.value = currentValue / maxValue;
        }
    }
}
