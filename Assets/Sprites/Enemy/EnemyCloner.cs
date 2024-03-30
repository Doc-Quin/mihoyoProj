using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mihoyoProj
{
    public class EnemyCloner : MonoBehaviour
    {
        public List<GameObject> enemyPoints;
        public List<GameObject> enemyPrefab;
        public GameObject enemyClone;
        public float cloneWaitTime;
        public float radius;
        public float someMinimumDistance;
        private bool isSpawning = false;

        void Start(){
            StartCoroutine(CloneEnemies());
            StartCoroutine(CheckAndCloneEnemies());
        }


        IEnumerator CheckAndCloneEnemies(){
            while(true){
                if(enemyClone.transform.childCount <= 4 && !isSpawning){
                    StartCoroutine(CloneEnemies());
                }
                yield return new WaitForSeconds(cloneWaitTime);
            }
        }

        IEnumerator CloneEnemies(){
            isSpawning = true;
            while(enemyClone.transform.childCount <= 4){
                Vector3 spawnPosition;
                bool positionIsValid;
                do{
                    spawnPosition = enemyClone.transform.position + Random.insideUnitSphere * radius; // 在enemyClone的位置的半径内随机生成位置
                    positionIsValid = true;
                    foreach(Transform child in enemyClone.transform){
                        if(Vector3.Distance(spawnPosition, child.position) < someMinimumDistance){
                            positionIsValid = false;
                            break;
                        }
                    }
                }while(!positionIsValid);
                
                GameObject e = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Count)], spawnPosition, Quaternion.identity);
                e.transform.SetParent(enemyClone.transform);
                yield return new WaitForSeconds(cloneWaitTime);
            }
            isSpawning = false;
        }

    }
}
