using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Swarm.Application.Config;
using Swarm.Application.Contracts;
using Swarm.Application.Primitives;
using Swarm.Presentation.Input;
using Swarm.Presentation.Renderers;
using Swarm.Presentation.Renderers.Hud;

namespace Swarm.Presentation;

public class Swarm : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private readonly IGameSessionService _service;
    private readonly ILogger<Swarm> _logger;
    private readonly Dictionary<int, Texture2D> _circleCache = new();
    private readonly float _moveSpeed = 360f;
    private HudRenderer _hud = null!;
    private SpriteFont _font = null!;
    private readonly InputManager _input;
    private GameConfig? gameConfig;
    private RenderTarget2D _renderTarget = null!;
    private Rectangle _drawDestination;
    private readonly float WIDTH = 960f;
    private readonly float HEIGHT = 540f;
    private readonly int BORDER = 40;
    private readonly SaveName SAVE = new("Progression");
    private bool _iShowingSaveGames = false;
    private SaveGameRenderer _saveGameRenderer = null!;

    private CrosshairRenderer _crosshairRenderer = null!;
    private bool _prevViewStatsKey = false;

    public Swarm(IGameSessionService service, ILogger<Swarm> logger)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _graphics.PreferredBackBufferWidth = (int)WIDTH;
        _graphics.PreferredBackBufferHeight = (int)HEIGHT;
        _service = service;
        _logger = logger;
        _input = new InputManager();
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

    }
    
    private void RecalculateDestination()
    {
        var screenW = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var screenH = GraphicsDevice.PresentationParameters.BackBufferHeight;

        // float scaleX = screenW / WIDTH;
        // float scaleY = screenH / HEIGHT;
        // float scale = MathF.Min(scaleX, scaleY);

        // int drawW = (int)(WIDTH * scale);
        // int drawH = (int)(HEIGHT * scale);

        int drawW = (int)WIDTH;
        int drawH = (int)HEIGHT;

        int drawX = (screenW - drawW) / 2;
        int drawY = (screenH - drawH) / 2;

        _drawDestination = new Rectangle(drawX, drawY, drawW, drawH);
    }

    protected override void Initialize()
    {
        Window.ClientSizeChanged += (_, __) => RecalculateDestination();

        var outer = new Rectangle(0, 0, (int)WIDTH, (int)HEIGHT);
        var inner = GetInnerGameRect(outer, BORDER);

        gameConfig = new GameConfig(
            StageConfig: new StageConfig(
                Left: inner.Left,
                Top: inner.Top,
                Right: inner.Right,
                Bottom: inner.Bottom
            ),
            LevelConfig: new LevelConfig(
                Weapon: new WeaponConfig(
                    Name: "",
                    Damage: 1,
                    ProjectileSpeed: 840f,
                    ProjectileRadius: 16f,
                    RatePerSecond: 60f,
                    ProjectileLifetimeSeconds: 10f,
                    MaxAmmo: 1000),
                PlayerAreaConfig: new AreaConfig(X: inner.Left + BORDER, Y: inner.Top + BORDER, Radius: 40),
                TargetAreaConfig: new AreaConfig(X: inner.Right - BORDER, Y: inner.Bottom - BORDER, Radius: 40),
                Walls:
                [
                    new(X: 480, Y: 270, Radius:80),

                ],
                Spawners:
                [
                    new(
                        CooldownSeconds: 0.2f,
                        BehaviourType: "",
                        SpawnObjectType: "Zombie",
                        BatchSize: 10
                    ),

                    new(
                        CooldownSeconds: 0.2f,
                        BehaviourType: "",
                        SpawnObjectType: "Zombie",
                        BatchSize: 10
                    ),

                    new(
                        CooldownSeconds: 0.2f,
                        BehaviourType: "",
                        SpawnObjectType: "Zombie",
                        BatchSize: 10
                    ),

                     new(
                        CooldownSeconds: 0.2f,
                        BehaviourType: "",
                        SpawnObjectType: "Zombie",
                        BatchSize: 10
                    ),

                    new(
                        CooldownSeconds: 12f,
                        BehaviourType: "",
                        SpawnObjectType: "Shooter",
                        BatchSize: 1
                    )
                ],
                BossConfig: new BossConfig(
                    Waypoints: new List<PointConfig>
                    {
                        new PointConfig(600, 300),
                        new PointConfig(600, 100),
                        new PointConfig(700, 100),
                        new PointConfig(700, 400)
                    },
                    Speed: 100f,
                    ShootRange: 600f,
                    Cooldown: 1f,
                    Damage: 10,
                    ProjectileSpeed: 900f,
                    ProjectileRadius: 6f,
                    ProjectileRatePerSecond: 4f,
                    ProjectileLifetimeSeconds: 2f),
                InitialTargetScore: 250
            ),
            PlayerRadius: 12,
            RoundLength: 45
        );

        
        _service.StartNewSession(gameConfig).GetAwaiter().GetResult();

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {

        _font = Content.Load<SpriteFont>("DefaultFont");

        _hud = new HudRenderer(_spriteBatch, _font, GraphicsDevice);

        _saveGameRenderer = new SaveGameRenderer(_spriteBatch, _font);

        _crosshairRenderer = new CrosshairRenderer(_spriteBatch, GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, (int) WIDTH, (int) HEIGHT);

        RecalculateDestination();

        base.LoadContent();
    }

    private bool IsSessionReady() => _service != null && _service.HasSession;
    
    protected override void Update(GameTime gameTime)
    {
        if (_service is null) return;

        if (!IsSessionReady())
            return;

        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape)) Exit();

        var snap = _service.GetSnapshot();

        var state = _input.Update();

        if (state.Pause)
        {
            if (snap.IsPaused)
                _service.Resume();
            else
                _service.Pause();

            snap = _service.GetSnapshot();
        }

        if (snap.IsPaused || snap.IsTimeUp || snap.IsCompleted || snap.IsInterrupted)
        {
            if (state.Restart) _service.Restart(gameConfig!);

            if (state.ViewStats && !_prevViewStatsKey)
            {
                _iShowingSaveGames = !_iShowingSaveGames;
            }
            _prevViewStatsKey = state.ViewStats;

            if (_iShowingSaveGames)
            {
                if (state.Left)
                {
                    _saveGameRenderer.PrevPage();

                }
                else if (state.Right)
                {
                    _saveGameRenderer.NextPage(_service.GetSaveGames());
                }
            }

            return;
        }

        _service.ApplyInput(state.DirX, state.DirY, (state.DirX == 0f && state.DirY == 0f) ? 0f : _moveSpeed);

        _service.Fire(state.FirePressed, state.FireHeld);

        if (state.DropBomb) _service.DropBomb();

        if (state.Reload) _service.Reload();

        _service.RotateTowards(state.MouseX, state.MouseY, state.AimRadians, state.AimMagnitude);

        var dt = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.05f);

        if (dt > 0f) _service.Tick(dt);

        // if (state.Save) _ = SaveGameAsync(new SaveName("QuickSave"));
        // if (state.Load) _ = LoadGameAsync(new SaveName("QuickSave"));

        base.Update(gameTime);
    }

    private async Task SaveGameAsync()
    {
        await _service.SaveAsync(SAVE);
        _logger.LogInformation("Game saved to {SaveName}",  SAVE.Value);
    }


    protected override void Draw(GameTime gameTime)
    {
        if (!IsSessionReady()) 
        {
            GraphicsDevice.Clear(Color.Black);
            return;
        }

        GraphicsDevice.SetRenderTarget(_renderTarget);
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
            Color.Blue
        );

        var ta = snap.TargetArea;
        DrawRect(new Rectangle(
            (int)(ta.X - ta.Radius),
            (int)(ta.Y - ta.Radius),
            (int)(ta.Radius * 2),
            (int)(ta.Radius * 2)),
            snap.TargetAreaIsOpenToPlayer ? Color.SeaGreen : Color.OrangeRed
        );

        DrawPlayer(new Vector2(snap.Player.X, snap.Player.Y), (int)snap.Player.Radius, snap.Player.RotationAngle, Color.IndianRed);

        foreach (var p in snap.Projectiles)
            DrawCircle(new Vector2(p.X, p.Y), (int)p.Radius, Color.OrangeRed);

        foreach (var e in snap.Enemies)
            DrawPlayer(new Vector2(e.X, e.Y), (int)e.Radius, e.RotationAngle, e.IsBoss ? Color.Purple : Color.Yellow);

        if (snap.IsPaused || snap.IsInterrupted || snap.IsTimeUp || snap.IsCompleted)
        {
            string mainText = "";
            string subText = "PRESS R TO RERUN";
            string subSubText = "PRESS V TO VIEW SAVE GAMES";

            if (snap.IsPaused) mainText = "PAUSED";
            else if (snap.IsInterrupted) mainText = "GAME OVER";
            else if (snap.IsTimeUp) mainText = "TIME UP";

            if (!string.IsNullOrEmpty(mainText))
            {
                Vector2 size = _font.MeasureString(mainText);
                Vector2 pos = new Vector2(
                    (WIDTH - size.X) / 2f,
                    (HEIGHT - size.Y) / 2f
                );
                _spriteBatch.DrawString(_font, mainText, pos, Color.White);
            }

            Vector2 mainSize = string.IsNullOrEmpty(mainText) ? Vector2.Zero : _font.MeasureString(mainText);

            Vector2 subSize = _font.MeasureString(subText);
            Vector2 subPos = new Vector2(
                (WIDTH - subSize.X) / 2f,
                (HEIGHT - mainSize.Y) / 2f + mainSize.Y + 10
            );
            _spriteBatch.DrawString(_font, subText, subPos, Color.White);

            Vector2 subSubSize = _font.MeasureString(subSubText);
            Vector2 subSubPos = new(
                (WIDTH - subSubSize.X) / 2f,
                subPos.Y + subSubSize.Y + 10
            );
            _spriteBatch.DrawString(_font, subSubText, subSubPos, Color.White);

            if (_iShowingSaveGames)
            {
                var saves = _service.GetSaveGames();
                _saveGameRenderer.Draw(saves);
            }

        }
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp); // PointClamp to avoid blur
        _spriteBatch.Draw(_renderTarget, _drawDestination, Color.White);

        DrawBorder(_spriteBatch, _drawDestination, BORDER, Color.Black );

        _hud.Draw(snap.HudData);

        _crosshairRenderer.Draw(snap.AimPositionX, snap.AimPositionY);
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

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, int baseThickness, Color color)
    {
        // Scale thickness proportional to how much the render target was scaled
        float scale = rect.Width / WIDTH; // since WIDTH = 960, this gives your draw scale
        int thickness = Math.Max(1, (int)(baseThickness * scale));

        // Top
        spriteBatch.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(Pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(Pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    private Rectangle GetInnerGameRect(Rectangle outer, int baseThickness)
    {
        float scale = outer.Width / WIDTH; // proportional to scaling
        int thickness = (int)(baseThickness * scale);

        return new Rectangle(
            outer.X + thickness,
            outer.Y + thickness,
            outer.Width - thickness * 2,
            outer.Height - thickness * 2
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
