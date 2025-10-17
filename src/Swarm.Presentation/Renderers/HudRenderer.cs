
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Swarm.Application.Contracts;

namespace Swarm.Presentation.Renderers;

public class HudRenderer(
    SpriteBatch spriteBatch,
    SpriteFont font,
    GraphicsDevice graphicsDevice
)
{
    private readonly SpriteBatch _spriteBatch = spriteBatch;

    private readonly SpriteFont _font = font;

    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

    public void Draw(Hud hud)
    {
        var leftText = hud.ToDisplayString();
        var leftPos = new Vector2(10, 10);
        _spriteBatch.DrawString(_font, leftText, leftPos, Color.White);

        var levelText = hud.LevelString;
        var rightTextSize = _font.MeasureString(levelText);
        var rightPos = new Vector2(_graphicsDevice.Viewport.Width - rightTextSize.X - 10, 10);
        _spriteBatch.DrawString(_font, levelText, rightPos, Color.White);

        string pauseHint = "Press P to PAUSE";
        string exitHint = "Press ESC to EXIT";

        Vector2 pauseSize = _font.MeasureString(pauseHint);
        Vector2 exitSize = _font.MeasureString(exitHint);

        Vector2 pausePos = new Vector2(10, _graphicsDevice.Viewport.Height - pauseSize.Y - 10);

        Vector2 exitPos = new Vector2(_graphicsDevice.Viewport.Width - exitSize.X - 10,
                                    _graphicsDevice.Viewport.Height - exitSize.Y - 10);

        _spriteBatch.DrawString(_font, pauseHint, pausePos, Color.White);
        _spriteBatch.DrawString(_font, exitHint, exitPos, Color.White);

    }
    
    
}