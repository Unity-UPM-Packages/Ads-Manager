using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public string sceneName;
    public void A()
    {
        StartCoroutine(LoadScene());
    }

    public IEnumerator LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            Debug.Log("AAAAA");
            yield return null;
        }

        Debug.Log("Done");
    }
}
