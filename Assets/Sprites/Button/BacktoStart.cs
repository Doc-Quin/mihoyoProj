using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace mihoyoProj
{
    public class BacktoStart : MonoBehaviour
    {
        public void BackToStart()
        {
            SceneManager.LoadScene(0);
        }
    }
}
