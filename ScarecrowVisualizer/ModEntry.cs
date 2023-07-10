using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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

        private void RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.player != null && (Game1.player.ActiveObject?.IsScarecrow() ?? false))
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
                                    e.SpriteBatch.Draw(Game1.mouseCursors, drawPos, new Rectangle(194, 388, 16, 16), Color.White*1f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                                }   
                            }
                        }
                    }
                }
            }
        }
    }
}

