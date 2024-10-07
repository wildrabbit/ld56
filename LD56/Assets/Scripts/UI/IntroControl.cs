using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroControl : MonoBehaviour
{
    // Intro scene
    [SerializeField] SceneReference initialScene;
    [SerializeField] bool manual = false;
    [SerializeField] float minDelay = 1.5f;

    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(minDelay);
        if (manual)
        {
            yield return new WaitUntil(ReceivedInput);
        }
        yield return new WaitForSeconds(0.3f);
        LoadNext();
    }

    private bool ReceivedInput()
    {
        bool inputs = InputUtils.IsAnyKeyPressed()
            || InputUtils.IsAnyGamepadButtonPressed();
        if (inputs && audioSource != null)
        {
            audioSource.Play();
        }
        return inputs;
    }

    private void LoadNext()
    {
        SceneManager.LoadScene(initialScene.ScenePath);
    }
}
