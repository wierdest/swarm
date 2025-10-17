using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Swarm.Presentation.Renderers;

public class CrosshairRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;
    private readonly int _size;
    private readonly int _thickness;
    private readonly Color _color;

    public CrosshairRenderer(
        SpriteBatch spriteBatch, 
        GraphicsDevice graphicsDevice, 
        int size = 6, 
        int thickness = 2, 
        Color? color = null)
    {
        _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        _size = size;
        _thickness = thickness;
        _color = color ?? Color.White;

        _pixel = new Texture2D(graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice)), 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(float x, float y)
    {
        _spriteBatch.Draw(
            _pixel, 
            new Rectangle((int)(x - _size), (int)(y - _thickness / 2f), _size * 2, _thickness), 
            _color
        );

        _spriteBatch.Draw(
            _pixel, 
            new Rectangle((int)(x - _thickness / 2f), (int)(y - _size), _thickness, _size * 2), 
            _color
        );
    }
}
