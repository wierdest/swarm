using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Swarm.Application.Config;
using Swarm.Application.Contracts;
using Swarm.Application.Services;
using Swarm.Presentation.Renderers;

namespace Swarm.Presentation;

public class Swarm : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly IGameSessionService _service;
    private readonly Dictionary<int, Texture2D> _circleCache = new();
    private float _moveSpeed = 220f;
    private KeyboardState _prevKb;
    private MouseState _prevMouse;

    private HudRenderer _hud;
    private SpriteFont _font;

    public Swarm()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 960;
        _graphics.PreferredBackBufferHeight = 540;

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Swarm", LogLevel.Information)
                .AddConsole();
        });

        var logger = loggerFactory.CreateLogger<GameSessionService>();
        _service = new GameSessionService(logger);
    }

    protected override void Initialize()
    {
        var gameConfig = new GameConfig(
        StageConfig: new StageConfig(
            Left: 0,
            Top: 0,
            Right: _graphics.PreferredBackBufferWidth,
            Bottom: _graphics.PreferredBackBufferHeight
        ),
        LevelConfig: new LevelConfig(
            Weapon: new WeaponConfig(
                Damage: 1,
                ProjectileSpeed: 420f,
                ProjectileRadius: 4f,
                RatePerSecond: 6f,
                ProjectileLifetimeSeconds: 2.0f
            ),
            PlayerAreaConfig: new AreaConfig(X: 50, Y: 50, Radius: 20),
            TargetAreaConfig: new AreaConfig(X: 900, Y: 500, Radius: 20),
            Walls:
            [
                new(X: 200, Y: 100, Radius: 30),
                new(X: 400, Y: 200, Radius: 30)
            ],
            Spawners:
            [
                new(X: 400, Y: 300, CooldownSeconds: 0.8f, BehaviourType: "")
            ]
        ),
        PlayerRadius: 12,
        RoundLength: 99
    );

    _service.StartNewSession(gameConfig);

    base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _font = Content.Load<SpriteFont>("DefaultFont");

        _hud = new HudRenderer(_spriteBatch, _font, GraphicsDevice);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape)) Exit();

        var mouse = Mouse.GetState();

        float dx = (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D) ? 1f : 0f)
                - (kb.IsKeyDown(Keys.Left)  || kb.IsKeyDown(Keys.A) ? 1f : 0f);
        float dy = (kb.IsKeyDown(Keys.Down)  || kb.IsKeyDown(Keys.S) ? 1f : 0f)
                - (kb.IsKeyDown(Keys.Up)    || kb.IsKeyDown(Keys.W) ? 1f : 0f);

        _service.ApplyInput(dx, dy, (dx == 0f && dy == 0f) ? 0f : _moveSpeed);

        if (kb.IsKeyDown(Keys.Space) && !_prevKb.IsKeyDown(Keys.Space)) _service.Fire();

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released) _service.Fire();

        _service.RotateTowards(mouse.X, mouse.Y);

        var dt = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.05f);
        
        if (dt > 0f) _service.Tick(dt);

        _prevKb = kb;
        _prevMouse = mouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        var snap = _service.GetSnapshot();

        _spriteBatch.Begin();

        DrawRect(new Rectangle((int)snap.Stage.Left, (int)snap.Stage.Top, (int)(snap.Stage.Right - snap.Stage.Left), (int)(snap.Stage.Bottom - snap.Stage.Top)), new Color(20, 20, 20));

        foreach (var wall in snap.Walls)
        {
            DrawRect(new Rectangle(
                (int)(wall.X - wall.Radius),
                (int)(wall.Y - wall.Radius),
                (int)(wall.Radius * 2),
                (int)(wall.Radius * 2)),
                Color.Gray
            );
        }

        var pa = snap.PlayerArea;
        DrawRect(new Rectangle(
            (int)(pa.X - pa.Radius),
            (int)(pa.Y - pa.Radius),
            (int)(pa.Radius * 2),
            (int)(pa.Radius * 2)),
            Color.Green * 0.5f
        );

        var ta = snap.TargetArea;
        DrawRect(new Rectangle(
            (int)(ta.X - ta.Radius),
            (int)(ta.Y - ta.Radius),
            (int)(ta.Radius * 2),
            (int)(ta.Radius * 2)),
            Color.OrangeRed * 0.5f
        );

        DrawPlayer(new Vector2(snap.Player.X, snap.Player.Y), (int)snap.Player.Radius, snap.Player.RotationAngle, Color.DeepSkyBlue);

        foreach (var p in snap.Projectiles)
            DrawCircle(new Vector2(p.X, p.Y), (int)p.Radius, Color.OrangeRed);
        
        foreach (var e in snap.Enemies)
            DrawPlayer(new Vector2(e.X, e.Y), (int)e.Radius, e.RotationAngle, Color.Yellow);

        _hud.Draw(snap.Hud);

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

    private void DrawPlayer(Vector2 pos, int radius, float rotation, Color color)
    {
        _spriteBatch.Draw(
            Pixel,                     
            position: pos,
            sourceRectangle: null,
            color: color,
            rotation: rotation,
            origin: new Vector2(0.5f, 0.5f),
            scale: new Vector2(radius * 2f, radius * 2f),
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
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
