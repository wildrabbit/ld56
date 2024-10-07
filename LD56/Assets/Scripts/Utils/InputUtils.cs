using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class InputUtils
{
    public const float Deadzone = 0.15f;
    public const int LStick = 0;
    public const int RStick = 1;
    public static bool IsAnyKeyPressed()
    {
        return ValidKeyboard(out Keyboard keyboard) && keyboard.anyKey.isPressed;
    }

    public static bool IsAnyKeyPressed(Key[] keys)
    {
        if (!ValidKeyboard(out var keyboard))
        {
            return false;
        }

        foreach (var k in keys)
        {
            if(keyboard[k].isPressed)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsKeyPressed(Key key)
    {
        return ValidKeyboard(out var keyboard) && keyboard[key].isPressed;
    }

    public static bool ValidKeyboard(out Keyboard keyboard)
    {
        keyboard = Keyboard.current;
        return keyboard != null;
    }

    public static bool IsGamepadButtonPressed(GamepadButton button)
    {
        return ValidGamepad(out Gamepad gamepad) && gamepad[button].isPressed;
    }

    public static bool IsAnyGamepadButtonPressed(bool includeSticks = true)
    {        
        if (!ValidGamepad(out var gamepad))
        {
            return false;
        }
        GamepadButton[] converted = (GamepadButton[])Enum.GetValues(typeof(GamepadButton));
        bool any = IsAnyGamepadButtonPressed(converted);
        return any 
            || !includeSticks 
            || HasGamepadStickValue(gamepad, LStick, Deadzone)
            || HasGamepadStickValue(gamepad, RStick, Deadzone);
    }

    public static bool HasGamepadStickValue(Gamepad gamepad, int stickID, float deadzone = 0f)
    {
        return GetGamepadStickValue(gamepad, stickID).magnitude > deadzone;
    }

    public static Vector2 GetGamepadStickValue(Gamepad gamepad, int stickID)
    {
        if(stickID == LStick)
        {
            return gamepad.leftStick.value;
        }
        else if (stickID == RStick)
        {
            return gamepad.rightStick.value;
        }
        return Vector2.zero;
    }

    public static bool IsAnyGamepadButtonPressed(GamepadButton[] buttons)
    {
        if (!ValidGamepad(out var gamepad))
        {
            return false;
        }

        foreach (var b in buttons)
        {
            if (gamepad[b].isPressed)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ValidGamepad(out Gamepad gamepad)
    {
        gamepad = Gamepad.current;
        return gamepad != null;
    }

    public static bool TryGetGamepadStickValue(int stick, out Vector2 stickValue)
    {
        stickValue = Vector2.zero;
        if(!ValidGamepad(out Gamepad gamepad))
        {
            return false;
        }
        stickValue = GetGamepadStickValue(gamepad, stick);
        return true;
    }
}
