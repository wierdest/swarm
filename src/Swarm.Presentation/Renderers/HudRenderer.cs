
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Swarm.Application.DTOs;

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

    public void Draw(HudDTO hud)
    {
        var scoreText = $"Score: {hud.Score}";
        var hpText = $"HP: {hud.HP}";
        var enemiesText = $"Enemies alive: {hud.NumberOfEnemiesAlive}";
        
        var leftText = $"{hpText} {scoreText} {enemiesText}";
        var leftPos = new Vector2(10, 10);

        var levelText = $"{hud.GameLevel}";
        var rightTextSize = _font.MeasureString(levelText);
        var rightPos = new Vector2(_graphicsDevice.Viewport.Width - rightTextSize.X - 10, 10);

        _spriteBatch.DrawString(_font, leftText, leftPos, Color.White);
        _spriteBatch.DrawString(_font, levelText, rightPos, Color.White);

    }
}