using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace mihoyoProj
{
    public class StarttoPrelevel : MonoBehaviour
    {
        public void Jump(){
            SceneManager.LoadScene(1);
        }
    }
}
