using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Swarm.Application.Contracts;
using Swarm.Application.Services;

namespace Swarm.Presentation;

public class Swarm : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly IGameSessionService _service = new GameSessionService();
    private readonly Dictionary<int, Texture2D> _circleCache = new();
    private float _moveSpeed = 220f;
    private KeyboardState _prev;

    public Swarm()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 960;
        _graphics.PreferredBackBufferHeight = 540;
    }

    protected override void Initialize()
    {
        var cfg = new StageConfig(
            Left: 0, Top: 0, Right: _graphics.PreferredBackBufferWidth, Bottom: _graphics.PreferredBackBufferHeight,
            PlayerStartX: 100, PlayerStartY: _graphics.PreferredBackBufferHeight / 2f, PlayerRadius: 12,
            Weapon: new WeaponConfig(
                Damage: 1,
                ProjectileSpeed: 420f,
                ProjectileRadius: 4f,
                RatePerSecond: 6f,
                ProjectileLifetimeSeconds: 2.0f
            )
        );
        _service.StartNewSession(cfg);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape)) Exit();

        float dx = (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D) ? 1f : 0f)
                - (kb.IsKeyDown(Keys.Left)  || kb.IsKeyDown(Keys.A) ? 1f : 0f);
        float dy = (kb.IsKeyDown(Keys.Down)  || kb.IsKeyDown(Keys.S) ? 1f : 0f)
                - (kb.IsKeyDown(Keys.Up)    || kb.IsKeyDown(Keys.W) ? 1f : 0f);

        _service.ApplyInput(dx, dy, (dx == 0f && dy == 0f) ? 0f : _moveSpeed);

        if (kb.IsKeyDown(Keys.Space) && !_prev.IsKeyDown(Keys.Space)) _service.Fire();

        var dt = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.05f);
        
        if (dt > 0f) _service.Tick(dt);

        _prev = kb;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        var snap = _service.GetSnapshot();

        _spriteBatch.Begin();

        DrawRect(new Rectangle((int)snap.Stage.Left, (int)snap.Stage.Top, (int)(snap.Stage.Right - snap.Stage.Left), (int)(snap.Stage.Bottom - snap.Stage.Top)), new Color(20, 20, 20));

        DrawCircle(new Vector2(snap.Player.X, snap.Player.Y), (int)snap.Player.Radius, Color.DeepSkyBlue);

        foreach (var p in snap.Projectiles)
            DrawCircle(new Vector2(p.X, p.Y), (int)p.Radius, Color.OrangeRed);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Texture2D GetCircle(int radius)
    {
        if (_circleCache.TryGetValue(radius, out var tex)) return tex;

        int diam = radius * 2 + 1;
        var data = new Color[diam * diam];
        var r2 = radius * radius;
        for (int y = 0; y < diam; y++)
            for (int x = 0; x < diam; x++)
            {
                int dx = x - radius;
                int dy = y - radius;
                data[y * diam + x] = (dx * dx + dy * dy) <= r2 ? Color.White : Color.Transparent;
            }

        var t = new Texture2D(GraphicsDevice, diam, diam);
        t.SetData(data);
        _circleCache[radius] = t;
        return t;
    }

    private void DrawCircle(Vector2 center, int radius, Color color)
    {
        var tex = GetCircle(radius);
        var pos = new Vector2(center.X - radius, center.Y - radius);
        _spriteBatch.Draw(tex, pos, color);
    }
    
    private static Texture2D? _pixel;
    private Texture2D Pixel
    {
        get
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            return _pixel;
        }
    }

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(Pixel, rect, color);
    }
}
