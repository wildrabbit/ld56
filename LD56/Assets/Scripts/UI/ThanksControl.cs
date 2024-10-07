using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

class ThanksControl: MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] float autoOut;
    [SerializeField] SceneReference reference;

    private float elapsed = 0f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(duration);
        elapsed = 0f;
        while (elapsed < autoOut)
        {
            elapsed += Time.deltaTime;
            if(elapsed >= autoOut || InputUtils.IsAnyGamepadButtonPressed() || InputUtils.IsAnyKeyPressed())
            {
                break;
            }
            yield return null;
        }        
        SceneManager.LoadScene(reference.ScenePath);
    }
}
