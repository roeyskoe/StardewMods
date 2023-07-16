using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System;

namespace FasterToolUse
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedWorld += RenderedWorld;
        }

        float hovertime = 0;

        private void RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.player != null)
            {
                if ((Game1.player.ActiveObject?.IsScarecrow() ?? false) || IsHoveringOverScarecrow())
                {
                    hovertime += 0.02f;
                    hovertime = MathHelper.Clamp(hovertime, 0, 1);
                   
                }
                else
                {
                    hovertime -= 0.02f;
                    hovertime = MathHelper.Clamp(hovertime, 0, 1);
                }
                if(hovertime > 0.01)
                {
                    RenderRanges(e.SpriteBatch);
                }
            }
        }

        public bool IsHoveringOverScarecrow()
        {
            ICursorPosition cursorPos = Helper.Input.GetCursorPosition();
            var pos = cursorPos.Tile;
            var obj = Game1.player.currentLocation?.getObjectAtTile((int)pos.X, (int)pos.Y);

            if (this.Helper.Input.IsDown(SButton.LeftShift))
                Debugger.Break();

            return obj?.IsScarecrow() ?? false;
        }

        // https://easings.net/#easeInQuart
        private float Ease(float x)
        {
            return x * x * x * x;
        }

        private void RenderRanges(SpriteBatch batch)
        {
            foreach (var obj in Game1.player.currentLocation?.Objects.Values)
            {
                if (obj.IsScarecrow())
                {
                    var radius = obj.GetRadiusForScarecrow();
                    var v = obj.TileLocation;
                    for (var i = -radius; i < radius; i++)
                    {
                        for (int j = -radius; j < radius; j++)
                        {
                            var tilePos = new Vector2(v.X + i, v.Y + j);
                            if (Vector2.Distance(tilePos, v) < radius)
                            {
                                var drawPos = Game1.GlobalToLocal(new Vector2(tilePos.X * 64, tilePos.Y * 64));
                                batch.Draw(Game1.mouseCursors, drawPos, new Rectangle(194, 388, 16, 16), Color.White * Ease(hovertime), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                            }
                        }
                    }
                }
            }
        }
    }
}

