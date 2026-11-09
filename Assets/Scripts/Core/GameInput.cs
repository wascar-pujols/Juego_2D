using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lectura del teclado. Funciona con el Input Manager clásico o con el nuevo Input System,
/// según cómo esté configurado el proyecto.
/// Controles: A/D o flechas = mover, Shift = correr, Espacio/W/Flecha arriba/Z = saltar,
/// J/X/K = golpear, Esc/P = pausa, Enter = aceptar.
/// </summary>
public static class GameInput
{
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
    static bool Held(Key k) { var kb = Keyboard.current; return kb != null && kb[k].isPressed; }
    static bool Down(Key k) { var kb = Keyboard.current; return kb != null && kb[k].wasPressedThisFrame; }

    public static float Horizontal
    {
        get
        {
            float h = 0;
            if (Held(Key.A) || Held(Key.LeftArrow)) h -= 1;
            if (Held(Key.D) || Held(Key.RightArrow)) h += 1;
            var gp = Gamepad.current;
            if (gp != null && Mathf.Abs(gp.leftStick.x.ReadValue()) > 0.3f) h = Mathf.Sign(gp.leftStick.x.ReadValue());
            if (gp != null && gp.dpad.left.isPressed) h = -1;
            if (gp != null && gp.dpad.right.isPressed) h = 1;
            return h;
        }
    }
    public static bool JumpDown => Down(Key.Space) || Down(Key.W) || Down(Key.UpArrow) || Down(Key.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    public static bool JumpHeld => Held(Key.Space) || Held(Key.W) || Held(Key.UpArrow) || Held(Key.Z) || (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed);
    public static bool AttackDown => Down(Key.J) || Down(Key.X) || Down(Key.K) || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
    public static bool RunHeld => Held(Key.LeftShift) || Held(Key.RightShift) || (Gamepad.current != null && Gamepad.current.rightTrigger.isPressed);
    public static bool PauseDown => Down(Key.Escape) || Down(Key.P) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
    public static bool SubmitDown => Down(Key.Enter) || Down(Key.NumpadEnter) || Down(Key.Space) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    public static bool UpDown => Down(Key.W) || Down(Key.UpArrow);
    public static bool DownDown => Down(Key.S) || Down(Key.DownArrow);
    public static bool MenuKeyDown => Down(Key.M);
#else
    public static float Horizontal
    {
        get
        {
            float h = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1;
            return h;
        }
    }
    public static bool JumpDown => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton0);
    public static bool JumpHeld => Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.JoystickButton0);
    public static bool AttackDown => Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.JoystickButton2);
    public static bool RunHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    public static bool PauseDown => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7);
    public static bool SubmitDown => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
    public static bool UpDown => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    public static bool DownDown => Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
    public static bool MenuKeyDown => Input.GetKeyDown(KeyCode.M);
#endif
}
