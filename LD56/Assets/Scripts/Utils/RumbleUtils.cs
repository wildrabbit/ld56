using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleUtils : MonoBehaviour
{
    [SerializeField] float smolLow;
    [SerializeField] float smolHigh;
    [SerializeField] float smolDuration;

    [SerializeField] float stronkLow;
    [SerializeField] float stronkHigh;
    [SerializeField] float stronkDuration;
    
    float rumbleElapsed = -1f;
    float rumbleDuration = -1f;
    Gamepad gp = null;

    private void Awake()
    {
        gp = Gamepad.current;
        if(gp != null)
        {
            gp.SetMotorSpeeds(0f, 0f);
        }
    }

    public void PlaySmol()
    {
        TryPlay(smolLow, smolHigh, smolDuration);
    }

    private void TryPlay(float low, float high, float duration)
    {
        if (rumbleElapsed >= 0f || gp == null)
        {
            return;
        }
        rumbleElapsed = 0f;
        rumbleDuration = duration;
        gp.SetMotorSpeeds(low, high);
    }

    public void Stop() 
    { 
        if(rumbleElapsed < 0)
        {
            return;
        }
        ForceStop();
    }

    public void PlayStronk()
    {
        TryPlay(stronkLow, stronkHigh, stronkDuration);
    }

    

    private void Update()
    {
        gp = Gamepad.current;
        if (rumbleElapsed >= 0f)
        {
            rumbleElapsed += Time.deltaTime;
            if(rumbleElapsed >= rumbleDuration)
            {
                Stop();
            }
        }
    }

    internal void ForceStop()
    {
        if (gp != null)
        {
            gp.SetMotorSpeeds(0f, 0f);
        }

        rumbleElapsed = -1f;
        rumbleDuration = -1f;
    }

    
    void OnApplicationLostFocus()
    {
        ForceStop();
    }

    private void OnApplicationQuit()
    {
        ForceStop();
    }
}
