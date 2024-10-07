using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class IntroControl : MonoBehaviour
{
    // Intro scene
    [SerializeField] SceneReference initialScene;
    [SerializeField] bool manual = false;
    [SerializeField] float minDelay = 1.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(minDelay);
        if (manual)
        {
            yield return new WaitUntil(ReceivedInput);
        }
        LoadNext();
    }

    private bool ReceivedInput()
    {
        return InputUtils.IsAnyKeyPressed()
            || InputUtils.IsAnyGamepadButtonPressed();
    }

    private void LoadNext()
    {
        SceneManager.LoadScene(initialScene.ScenePath);
    }
}
