using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Swarm.Application.Contracts;
using Swarm.Presentation.Input;
using Swarm.Presentation.Renderers;
using Swarm.Presentation.Renderers.Hud;

namespace Swarm.Presentation;

public class Swarm : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private readonly IGameSessionService _service;
    private readonly Dictionary<int, Texture2D> _circleCache = new();
    private float _moveSpeed;
    private float _width;
    private float _height;
    private int _border;
    private HudRenderer _hud = null!;
    private SpriteFont _font = null!;
    private readonly InputManager _input;
    private string? _gameConfigJson;
    private bool _manifestSaved;
    private bool _contentLoaded;
    private RenderTarget2D _renderTarget = null!;
    private Rectangle _drawDestination;
    private CrosshairRenderer _crosshairRenderer = null!;
    private readonly IGameSessionConfigSource _configSource;
    private readonly IGameSessionConfigLoader _configLoader;

    public Swarm(
        IGameSessionService service,
        IGameSessionConfigSource configSource,
        IGameSessionConfigLoader configLoader)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _service = service;
        _configSource = configSource ?? throw new ArgumentNullException(nameof(configSource));
        _configLoader = configLoader ?? throw new ArgumentNullException(nameof(configLoader));
        _input = new InputManager();
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

    }
    
    private void RecalculateDestination()
    {
        var screenW = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var screenH = GraphicsDevice.PresentationParameters.BackBufferHeight;

        // float scaleX = screenW / _width;
        // float scaleY = screenH / _height;
        // float scale = MathF.Min(scaleX, scaleY);

        // int drawW = (int)(_width * scale);
        // int drawH = (int)(_height * scale);

        int drawW = (int)_width;
        int drawH = (int)_height;

        int drawX = (screenW - drawW) / 2;
        int drawY = (screenH - drawH) / 2;

        _drawDestination = new Rectangle(drawX, drawY, drawW, drawH);
    }

    protected override void Initialize()
    {
        Window.ClientSizeChanged += (_, __) => RecalculateDestination();
        _gameConfigJson = _configSource.LoadConfigJson(Content.RootDirectory);
        ApplyPresentationConfig(_gameConfigJson);
        _manifestSaved = false;
        _service.StartNewSession(_gameConfigJson).GetAwaiter().GetResult();

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {

        _font = Content.Load<SpriteFont>("DefaultFont");

        _hud = new HudRenderer(_spriteBatch, _font, GraphicsDevice);

        _crosshairRenderer = new CrosshairRenderer(_spriteBatch, GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, (int)_width, (int)_height);
        _contentLoaded = true;

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

        if (state.Reset)
        {
            ResetManifestProgress();
            _gameConfigJson = _configSource.LoadConfigJson(Content.RootDirectory);
            ApplyPresentationConfig(_gameConfigJson);
            _manifestSaved = false;
            _service.Restart(_gameConfigJson!);
            return;
        }

        if (state.NavigateNextConfig || state.NavgatePrevConfig)
        {
            NavigateConfig(state.NavigateNextConfig ? 1 : -1);
            return;
        }
        
        if (state.Replay)
        {
            if (!string.IsNullOrWhiteSpace(_gameConfigJson))
                _service.Restart(_gameConfigJson);
            return;
        }

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
            if (snap.IsCompleted && !_manifestSaved)
            {
                var manifest = _configSource.LoadManifest(Content.RootDirectory);
                var index = _configSource.SelectEntryIndex(manifest);
                if (index >= 0 && index < manifest.Entries.Count)
                {
                    manifest.Entries[index].Completed = true;
                    _configSource.SaveManifest(Content.RootDirectory, manifest);
                }
                _manifestSaved = true;
            }
            if (state.Next)
            {
                _gameConfigJson = _configSource.LoadConfigJson(Content.RootDirectory);
                ApplyPresentationConfig(_gameConfigJson);
                _manifestSaved = false;
                _service.Restart(_gameConfigJson!);
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

        base.Update(gameTime);
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
            DrawPlayer(new Vector2(e.X, e.Y), (int)e.Radius, e.RotationAngle, GetColorForNonPlayerEntityType(e.Type));

        if (snap.IsPaused || snap.IsInterrupted || snap.IsTimeUp || snap.IsCompleted)
        {
            string mainText = "";
            string continueText = "PRESS R TO RUN TO THE NEXT SESSION";
            string replayText = "PRESS F6 TO REPLAY THIS SESSION";
            string resetText = "PRESS F8 TO RESET PROGRESS";
            string navText = "PRESS F9/F10 TO PREV/NEXT CONFIG";

            if (snap.IsPaused) mainText = "PAUSED";
            else if (snap.IsInterrupted) mainText = "GAME OVER";
            else if (snap.IsTimeUp) mainText = "TIME UP";

            if (!string.IsNullOrEmpty(mainText))
            {
                Vector2 size = _font.MeasureString(mainText);
                Vector2 pos = new(
                    (_width - size.X) / 2f,
                    (_height - size.Y) / 2f
                );
                _spriteBatch.DrawString(_font, mainText, pos, Color.White);
            }

            Vector2 mainSize = string.IsNullOrEmpty(mainText) ? Vector2.Zero : _font.MeasureString(mainText);

            Vector2 subSize = _font.MeasureString(continueText);
            Vector2 subPos = new(
                (_width - subSize.X) / 2f,
                (_height - mainSize.Y) / 2f + mainSize.Y + 10
            );
            _spriteBatch.DrawString(_font, continueText, subPos, Color.White);

            Vector2 resetSize = _font.MeasureString(resetText);
            Vector2 resetPos = new(
                (_width - resetSize.X) / 2f,
                subPos.Y + resetSize.Y + 10
            );
            _spriteBatch.DrawString(_font, resetText, resetPos, Color.White);

            Vector2 replaySize = _font.MeasureString(replayText);
            Vector2 replayPos = new(
                (_width - replaySize.X) / 2f,
                resetPos.Y + replaySize.Y + 10
            );
            _spriteBatch.DrawString(_font, replayText, replayPos, Color.White);

            Vector2 navSize = _font.MeasureString(navText);
            Vector2 navPos = new(
                (_width - navSize.X) / 2f,
                replayPos.Y + navSize.Y + 10
            );
            _spriteBatch.DrawString(_font, navText, navPos, Color.White);

        }
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, _drawDestination, Color.White);

        DrawBorder(_spriteBatch, _drawDestination, _border, Color.Black);

        _hud.Draw(snap.GameSessionData);

        _crosshairRenderer.Draw(snap.AimPositionX, snap.AimPositionY);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private static Color GetColorForNonPlayerEntityType(string type)
    {
        return type switch
        {
            "Shooter" => Color.Purple,
            "Healthy" => Color.Orange,
            _ => Color.Yellow
        };
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
        float scale = rect.Width / _width;
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

    private void ResetManifestProgress()
    {
        var manifest = _configSource.LoadManifest(Content.RootDirectory);
        manifest.ActiveIndex = null;
        for (int i = 0; i < manifest.Entries.Count; i++)
        {
            manifest.Entries[i].Completed = false;
        }
        _configSource.SaveManifest(Content.RootDirectory, manifest);
    }

    private void NavigateConfig(int delta)
    {
        var manifest = _configSource.LoadManifest(Content.RootDirectory);
        if (manifest.Entries.Count == 0) return;

        int currentIndex = manifest.ActiveIndex is int idx && idx >= 0 && idx < manifest.Entries.Count
            ? idx
            : _configSource.SelectEntryIndex(manifest);

        int nextIndex = (currentIndex + delta) % manifest.Entries.Count;
        if (nextIndex < 0) nextIndex += manifest.Entries.Count;

        manifest.ActiveIndex = nextIndex;
        _configSource.SaveManifest(Content.RootDirectory, manifest);

        _gameConfigJson = LoadConfigJsonFromManifestEntry(manifest, nextIndex);
        ApplyPresentationConfig(_gameConfigJson);
        _manifestSaved = false;
        _service.Restart(_gameConfigJson!);
    }

    private void ApplyPresentationConfig(string configJson)
    {
        var config = _configLoader.Load(configJson);
        var stage = config.StageConfig;

        _moveSpeed = config.PlayerSpeed;

        var newWidth = stage.Right + stage.Left;
        var newHeight = stage.Bottom + stage.Top;
        var newBorder = (int)stage.Left;
        if ((int)stage.Top != newBorder)
        {
            newBorder = (int)MathF.Min(stage.Left, stage.Top);
        }

        var sizeChanged = (int)newWidth != (int)_width || (int)newHeight != (int)_height;

        _width = newWidth;
        _height = newHeight;
        _border = newBorder;

        if (sizeChanged)
        {
            _graphics.PreferredBackBufferWidth = (int)_width;
            _graphics.PreferredBackBufferHeight = (int)_height;
            _graphics.ApplyChanges();

            if (_contentLoaded)
            {
                _renderTarget.Dispose();
                _renderTarget = new RenderTarget2D(GraphicsDevice, (int)_width, (int)_height);
            }
        }

        RecalculateDestination();
    }

    private string LoadConfigJsonFromManifestEntry(GameSessionConfigManifest manifest, int index)
    {
        var entry = manifest.Entries[index];
        if (string.IsNullOrWhiteSpace(entry.File))
            throw new InvalidOperationException("Manifest entry must include a file path.");

        var contentRoot = Path.Combine(AppContext.BaseDirectory, Content.RootDirectory);
        var configPath = Path.Combine(contentRoot, entry.File);
        return File.ReadAllText(configPath);
    }

}
