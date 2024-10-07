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
    [SerializeField] float delay = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        if(!manual)
        {
            yield return new WaitForSeconds(delay);
            LoadNext();
        }
    }

    private void LoadNext()
    {
        SceneManager.LoadScene(initialScene.ScenePath);
    }

    // Update is called once per frame
    void Update()
    {
        if(!manual)
        {
            return;
        }

        if(InputUtils.IsAnyKeyPressed()
            || InputUtils.IsAnyGamepadButtonPressed())
        {
            LoadNext();
        }
    }
}
