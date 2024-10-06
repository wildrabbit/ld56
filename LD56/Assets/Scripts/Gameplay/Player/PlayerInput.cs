using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    
    public float xAxis;
    public bool shootReleased;
    

    bool active = false;
    bool shootPressed = false;
    bool inputReady = false;

    public void Activate()
    {
        active = true;
        xAxis = 0f;
        shootPressed = shootReleased = false;
    }

    private bool ReadShoot()
    {
        return (Keyboard.current != null && (Keyboard.current.xKey.isPressed || Keyboard.current.zKey.isPressed || Keyboard.current.spaceKey.isPressed))
            || (Gamepad.current != null && Gamepad.current.aButton.isPressed);
    }

    public void Deactivate() 
    {
        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

#if UNITY_WEBGL
        if(!inputReady)
        {
            if(Keyboard.current != null|| Gamepad.current != null)
            {
                inputReady = true;
            }
            return;
        }
#endif

        xAxis = ReadMovement();
        bool wasPressed = shootPressed;
        shootPressed = ReadShoot();
        shootReleased = !shootPressed && wasPressed;
    }

    private float ReadMovement()
    {
        float motion = 0f;
        float gamepadAxis = 0f;
        if (Gamepad.current != null)
        {
            gamepadAxis = Gamepad.current.leftStick.x.value;
        }

        float keyboardAxis = 0f;
        if ((Keyboard.current !=null))
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                keyboardAxis = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                keyboardAxis = 1f;
            }
        }
        
        if (gamepadAxis != 0f)
        {
            motion = gamepadAxis;
        }
        else if (keyboardAxis != 0f)
        {
            motion = keyboardAxis;
        }
        return motion;
    }
}
