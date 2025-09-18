using Microsoft.Xna.Framework.Input;

namespace Swarm.Presentation.Input;

public sealed class InputManager
{
    private KeyboardState _prevKb;
    private MouseState _prevMouse;
    public InputState Update()
    {
        var kb = Keyboard.GetState();
        var mouse = Mouse.GetState();

        float dx = (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D) ? 1f : 0f)
                 - (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A) ? 1f : 0f);

        float dy = (kb.IsKeyDown(Keys.Down) || kb.IsKeyDown(Keys.S) ? 1f : 0f)
                 - (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W) ? 1f : 0f);

        bool fire = (kb.IsKeyDown(Keys.Space) && !_prevKb.IsKeyDown(Keys.Space))
                 || (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released);

        bool reload = kb.IsKeyDown(Keys.E) && ! _prevKb.IsKeyDown(Keys.E);

        bool pause = kb.IsKeyDown(Keys.P) && !_prevKb.IsKeyDown(Keys.P);

        bool save = kb.IsKeyDown(Keys.F5) && !_prevKb.IsKeyDown(Keys.F5);
        bool load = kb.IsKeyDown(Keys.F9) && !_prevKb.IsKeyDown(Keys.F9);

        var state = new InputState(dx, dy, mouse.X, mouse.Y, fire, reload, pause, save, load);

        _prevKb = kb;
        _prevMouse = mouse;

        return state;
    }
}
