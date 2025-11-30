using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Swarm.Application.Contracts;
using Swarm.Presentation.Renderers.Hud;

namespace Swarm.Presentation.Renderers;

public class SaveGameRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;
    private int _page = 0;
    private const int PageSize = 10;

    public SaveGameRenderer(SpriteBatch spriteBatch, SpriteFont font)
    {
        _spriteBatch = spriteBatch;
        _font = font;
    }

    public void Draw(IReadOnlyList<SaveGame> saves)
    {
        int start = _page * PageSize;
        int end = Math.Min(saves.Count, start + PageSize);

        for (int i = start; i < end; i++) 
        {
            var save = saves[i];
            string date = save.SaveTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            string text = $"{i + 1}. {date} {HudTextBuilder.BuildSaveGameString(save.HudData)}";
            Vector2 pos = new(50, 50 + (i - start) * 20);
            _spriteBatch.DrawString(_font, text, pos, Color.White);
        }
    }

    public void NextPage(IReadOnlyList<SaveGame> saves)
    {
        if ((_page + 1) * PageSize < saves.Count) _page++;
    }

    public void PrevPage()
    {
        if (_page > 0) _page--;
    }
}
