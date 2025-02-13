using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public string sceneName;
    private string _currentLoadRequestId;
    private Coroutine _loadTimeoutCoroutine;

    private int amout = 0;

    public void A()
    {
        amout = 0;
        Load();
    }

    private IEnumerator CallBack(float time, Action callBack)
    {
        yield return new WaitForSeconds(time);

        callBack?.Invoke();
    }

    private void Load()
    {
        if (amout >= 3)
        {
            _currentLoadRequestId = "";
            return;
        }

        Debug.Log("Load");

        _currentLoadRequestId = Guid.NewGuid().ToString();
        string loadRequestId = _currentLoadRequestId;

        amout++;

        StartCoroutine(CallBack(3, () =>
        {
            if (loadRequestId != _currentLoadRequestId)
            {
                return;
            }

            if (_loadTimeoutCoroutine != null)
            {
                StopCoroutine(_loadTimeoutCoroutine);
                _loadTimeoutCoroutine = null;
            }
            LoadResult();
        }));

        _loadTimeoutCoroutine = StartCoroutine(LoadAdTimeout(2));
    }

    private IEnumerator LoadAdTimeout(float timeout)
    {


        yield return new WaitForSeconds(timeout);

        Debug.Log("Time out");

        Load();
    }

    private void LoadResult()
    {
        Debug.Log("1");
    }
}
