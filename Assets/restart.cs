using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class restart : MonoBehaviour
{

    public GameObject ResetColidingWith;
    void OnCollisionEnter(Collision collision) {
        Debug.Log("no reset" + collision.gameObject.name);
        if (collision.gameObject == ResetColidingWith) {

            Debug.Log("reset !! " + collision.gameObject.name);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
