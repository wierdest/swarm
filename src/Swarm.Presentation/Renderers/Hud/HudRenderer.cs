using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Swarm.Application.Contracts;

namespace Swarm.Presentation.Renderers.Hud;

public class HudRenderer(
    SpriteBatch spriteBatch,
    SpriteFont font,
    GraphicsDevice graphicsDevice
)
{
    private readonly SpriteBatch _spriteBatch = spriteBatch;
    private readonly SpriteFont _font = font;
    private readonly static string _pauseHint = "Press P to PAUSE";
    private readonly static string _exitHint = "Press ESC to EXIT";
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

    public void Draw(HudData hudData)
    {
        var leftText = HudTextBuilder.BuildTopLine(hudData);
        var leftPos = new Vector2(10, 10);
        _spriteBatch.DrawString(_font, leftText, leftPos, Color.White);

        var levelText = HudTextBuilder.BuildMissionText(hudData);
        var rightTextSize = _font.MeasureString(levelText);
        var rightPos = new Vector2(_graphicsDevice.Viewport.Width - rightTextSize.X - 10, 10);
        _spriteBatch.DrawString(_font, levelText, rightPos, Color.White);


        var bombText = HudTextBuilder.BuildBombText(hudData.BombCount);
        var bombTextSize = _font.MeasureString(bombText);
        float screenCenterX = _graphicsDevice.Viewport.Width / 2f;
        float bottomY = _graphicsDevice.Viewport.Height - bombTextSize.Y - 10;
        Vector2 bombPos = new(screenCenterX - bombTextSize.X / 2f, bottomY);
        _spriteBatch.DrawString(_font, bombText, bombPos, Color.White);

        Vector2 pauseSize = _font.MeasureString(_pauseHint);
        Vector2 pausePos = new(10, _graphicsDevice.Viewport.Height - pauseSize.Y - 10);
        _spriteBatch.DrawString(_font, _pauseHint, pausePos, Color.White);
        
        Vector2 exitSize = _font.MeasureString(_exitHint);
        Vector2 exitPos = new(
            _graphicsDevice.Viewport.Width - exitSize.X - 10,
            _graphicsDevice.Viewport.Height - exitSize.Y - 10);
        _spriteBatch.DrawString(_font, _exitHint, exitPos, Color.White);

    }
    
    
}