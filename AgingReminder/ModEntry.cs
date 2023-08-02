using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FasterToolUse
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.GameLoop.DayStarted += DayStarted;
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            if (Context.IsMultiplayer)
            {
                if (Context.IsMainPlayer)
                {
                    var locations = GetStatuses();
                    Helper.Multiplayer.SendMessage(locations, "CaskNotification", modIDs: new[] { this.ModManifest.UniqueID });
                }
            }
            else
            {
                var locations = GetStatuses();
                Remind(locations);
            }
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == "CaskNotification")
            {
                List<Location> locations = e.ReadAs<List<Location>>();
                Remind(locations);
            }
        }

        public struct Location
        {
            public string LocationName;
            public string ObjectName;
            public ObjectLevel ObjectLevel;
        }

        public enum ObjectLevel
        {
            None = 0,
            Silver = 2,
            Gold = 3,
            Iridium = 4,
        }

        private List<Location> GetStatuses()
        {
            List<Location> locations = new List<Location>();
            foreach (GameLocation location in Game1.locations)
            {
                var loc = new Location();
                loc.LocationName = location.Name;

                foreach (var obj in location.Objects.Values)
                {
                    if (obj is Cask c)
                    {
                        var heldObject = c.heldObject.Value;
                        if (heldObject != null)
                        {
                            var val = c.daysToMature.Value;
                            // if anything at iridium, we can stop going through other objects.
                            if (heldObject.Quality == 4)
                            {
                                loc.ObjectLevel = ObjectLevel.Iridium;
                                loc.ObjectName = heldObject.Name;
                                break;
                            }
                            // If the quality increased today.
                            if (c.GetDaysForQuality(c.GetNextQuality(heldObject.Quality - 1)) == val)
                            {
                                // Only care about the highest quality one.
                                if(val > (int)loc.ObjectLevel)
                                {
                                    loc.ObjectName = heldObject.Name;
                                    loc.ObjectLevel = val switch
                                    {
                                        42 => ObjectLevel.Silver,
                                        28 => ObjectLevel.Gold,
                                        _ => loc.ObjectLevel
                                    };
                                }
                                
                            }
                        }
                    }
                }
                // Dont care if nothing to report of.
                if(loc.ObjectLevel != 0)
                {
                    locations.Add(loc);
                }
            }

            return locations;
        }

        private void Remind(List<Location> locations)
        {
            foreach (Location location in locations)
            {
                if (location.ObjectLevel == ObjectLevel.Iridium)
                {
                    Game1.addHUDMessage(new HUDMessage($"{location.LocationName}: {location.ObjectName} ready!", 1));
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage($"{location.LocationName}: {location.ObjectName} at {location.ObjectLevel}", 2));
                }
            }
        }
    }
}

