using UnityEngine;

public class PlayerInput: MonoBehaviour {
    public static FixedInputFrame Frame { get; private set; }
    
    // Next frame really means the current frame we are currently collecting input for
    // This is done so FixedFrame is not reset before it can be used
    private FixedInputFrame _nextFrame;

    void Start() {
        Frame = new FixedInputFrame();
        _nextFrame = new FixedInputFrame();
    }

    private void FixedUpdate() {
        Frame = _nextFrame;
        _nextFrame = new FixedInputFrame();
    }

    void Update() {
        _nextFrame.Joystick = Joystick();
        if (IsJumpKeyDown()) {
            _nextFrame.IsJumpKeyDown = true;
        }

        if (IsJumpKeyHeld()) {
            _nextFrame.IsJumpKeyHeld = true;
        }

        if (IsRewindKeyHeld()) {
            _nextFrame.IsRewindKeyHeld = true;
        }

        if (IsDashKeyDown()) {
            _nextFrame.IsDashKeyDown = true;
        }

        if (IsMenuKeyPressed()) {
            _nextFrame.IsMenuKeyPressed = true;
            
            // Doing this here until menu is made
            Application.Quit();
        }

        if (IsResetKeyHeld()) {
            _nextFrame.IsResetKeyHeld = true;
        }
    }

    private static Vector2 Joystick() {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private static bool IsJumpKeyDown() {
        return Input.GetKeyDown(KeyCode.Space) || 
               Input.GetKeyDown(ControllerA());
    }

    private static bool IsJumpKeyHeld() {
        return Input.GetKey(KeyCode.Space) || 
               Input.GetKey(ControllerA());
    }

    private static bool IsRewindKeyHeld() {
        return Input.GetKey(KeyCode.Backspace) ||
               Input.GetKey(ControllerLeftBumper()) ||
               Input.GetKey(ControllerRightBumper());
    }

    private static bool IsDashKeyDown() {
        return Input.GetKeyDown(KeyCode.LeftShift) || 
               Input.GetKeyDown(KeyCode.RightShift) ||
               Input.GetKeyDown(ControllerX());
    }

    private static bool IsMenuKeyPressed() {
        return Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetKeyDown(ControllerStart());
    }

    private static bool IsResetKeyHeld() {
        return Input.GetKey(KeyCode.Delete) ||
               Input.GetKey(ControllerBack());
    }
    
    // Controller Key Code
    
    private static KeyCode ControllerA() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton0;
#elif UNITY_STANDALONE_OSX 
        return KeyCode.JoystickButton16;
#endif
    }
    
    private static KeyCode ControllerX() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton2;
#elif UNITY_STANDALONE_OSX
        return KeyCode.JoystickButton18;
#endif
    }
    
    private static KeyCode ControllerLeftBumper() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton4;
#elif UNITY_STANDALONE_OSX
        return KeyCode.JoystickButton13;
#endif
    }
    
    private static KeyCode ControllerRightBumper() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton5;
#elif UNITY_STANDALONE_OSX
        return KeyCode.JoystickButton14;
#endif
    }

    private static KeyCode ControllerStart() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton7;
#elif UNITY_STANDALONE_OSX
        return KeyCode.JoystickButton9;
#endif
    }

    private static KeyCode ControllerBack() {
#if UNITY_STANDALONE_WIN
        return KeyCode.JoystickButton6;
#elif UNITY_STANDALONE_OSX
        return KeyCode.JoystickButton10;
#endif
    }
}

public class FixedInputFrame {
    public Vector2 Joystick;
    public bool IsJumpKeyDown;
    public bool IsJumpKeyHeld;
    public bool IsRewindKeyHeld;
    public bool IsDashKeyDown;
    public bool IsMenuKeyPressed;
    public bool IsResetKeyHeld;

    public bool IsTiltedRight() {
        return Joystick.x > .1f;
    }

    public bool IsTiltingDown() {
        return Joystick.y < -.3f;
    }
    
    public bool JoystickNotReceivingInput() {
        return Util.ValueInRange(Joystick.x, -.1f, .1f) && Util.ValueInRange(Joystick.y, -.1f, .1f);
    }
    
    public bool JoystickIsReceivingInput() {
        return !JoystickNotReceivingInput();
    }
}