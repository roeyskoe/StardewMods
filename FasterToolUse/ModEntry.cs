using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;

namespace FasterToolUse
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            var modifier = 20;

            if (Game1.player != null)
            {
                int toolHold = Game1.player.toolHold.Value;

                if (!Game1.player.canReleaseTool || toolHold <= 0)
                    return;

                if (toolHold - modifier <= 0)
                {
                    Game1.player.toolHold.Value = 1;
                }
                else
                {
                    Game1.player.toolHold.Value -= modifier;
                }
            }
        }
    }
}

