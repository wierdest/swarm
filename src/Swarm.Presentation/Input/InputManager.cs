using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Swarm.Application.DTOs;

namespace Swarm.Presentation.Input;

public sealed class InputManager
{
    private KeyboardState _prevKb;
    private MouseState _prevMouse;
    private GamePadState _prevPad;
    private float? _aimAngleRadians = null;
    public float _aimMagnitude = 0f;
    private Vector2 _smoothedRightStick = Vector2.Zero;
    private const float _aimSmoothness = 0.2f;

    public InputStateDTO Update()
    {
        var kb = Keyboard.GetState();
        var mouse = Mouse.GetState();
        var pad = GamePad.GetState(PlayerIndex.One);

        float dx = (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D) ? 1f : 0f)
                 - (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A) ? 1f : 0f);

        float dy = (kb.IsKeyDown(Keys.Down) || kb.IsKeyDown(Keys.S) ? 1f : 0f)
                 - (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W) ? 1f : 0f);

        dx += pad.ThumbSticks.Left.X;
        dy -= pad.ThumbSticks.Left.Y;

        bool firePressed =
            JustPressed(Keys.Space, kb, _prevKb) ||
            (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released) ||
            JustPressed(Buttons.RightTrigger, pad, _prevPad);

        bool fireHeld =
            kb.IsKeyDown(Keys.Space) ||
            mouse.LeftButton == ButtonState.Pressed ||
            pad.Triggers.Right > 0.3f;

        bool reload =
              JustPressed(Keys.E, kb, _prevKb) ||
              JustPressed(Buttons.X, pad, _prevPad);

        bool pause =
            JustPressed(Keys.P, kb, _prevKb) ||
            JustPressed(Buttons.Start, pad, _prevPad);

        bool restart =
            JustPressed(Keys.R, kb, _prevKb) ||
            JustPressed(Buttons.Y, pad, _prevPad);

        bool view =
            JustPressed(Keys.V, kb, _prevKb) ||
            JustPressed(Buttons.Back, pad, _prevPad);

        bool left =
            JustPressed(Keys.Left, kb, _prevKb) ||
            JustPressed(Buttons.DPadLeft, pad, _prevPad);

        bool right =
            JustPressed(Keys.Right, kb, _prevKb) ||
            JustPressed(Buttons.DPadRight, pad, _prevPad);

        if (pad.IsConnected)
        {
            Vector2 rightStick = pad.ThumbSticks.Right;
            rightStick.Y *= -1f; // invert Y for screen coordinates

            _smoothedRightStick = Vector2.Lerp(_smoothedRightStick, rightStick, _aimSmoothness);

            if (_smoothedRightStick.LengthSquared() > 0.15f)
            {
                _aimMagnitude = MathHelper.Clamp(_smoothedRightStick.Length(), 0f, 1f);
                _aimAngleRadians = (float)Math.Atan2(_smoothedRightStick.Y, _smoothedRightStick.X);
            }
            else
            {
                _aimMagnitude = 0f;
                _aimAngleRadians = null;
            }
        }

        var state = new InputStateDTO(
            dx,
            dy,
            mouse.X,
            mouse.Y,
            _aimAngleRadians,
            _aimMagnitude,
            firePressed,
            fireHeld,
            reload,
            pause,
            false,
            false,
            restart,
            view,
            left,
            right
        );

        _prevKb = kb;
        _prevMouse = mouse;
        _prevPad = pad;

        return state;
    }
    private static bool JustPressed(Keys key, KeyboardState current, KeyboardState prev)  => current.IsKeyDown(key) && !prev.IsKeyDown(key);
    private static bool JustPressed(Buttons button, GamePadState current, GamePadState prev) => current.IsButtonDown(button) && !prev.IsButtonDown(button);

}
