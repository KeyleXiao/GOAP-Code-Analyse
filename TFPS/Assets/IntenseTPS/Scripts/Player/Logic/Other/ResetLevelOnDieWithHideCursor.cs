using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Player
{

    public class ResetLevelOnDieWithHideCursor : MonoBehaviour
    {
        public bool hideCursorOnStart = true;

        void Start()
        {
            if (GameObject.FindGameObjectWithTag("Player") && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>())
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>().onPlayerDead += OnPlayerDead;
            }

            if (hideCursorOnStart)
                Cursor.visible = false;
        }

        private void OnPlayerDead()
        {
            StartCoroutine(RestartLevel());
        }

        private IEnumerator RestartLevel()
        {
            yield return new WaitForSeconds(4);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}