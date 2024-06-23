using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

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
                    Remind(locations);
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
            public bool IsKeg;
        }

        public enum ObjectLevel
        {
            None = 0,
            Silver = 2,
            Gold = 3,
            Iridium = 4,
        }

        private static List<Location> GetStatuses()
        {
            var locations = new List<Location>();
            foreach (GameLocation location in Game1.locations)
            {
                var loc = new Location
                {
                    LocationName = location.Name
                };

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
                                loc.IsKeg = false;
                                break;
                            }
                            // If the quality increased today.
                            if (c.GetDaysForQuality(c.GetNextQuality(heldObject.Quality - 1)) == val)
                            {
                                // Only care about the highest quality one.
                                // If we already have a Keg ready in the location, disregard casks that are not ready.
                                if(val > (int)loc.ObjectLevel && !loc.IsKeg)
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
                    else if(obj.Name.Equals("Keg"))
                    {
                        if(obj.heldObject.Value != null && obj.MinutesUntilReady == 0)
                        {
                            loc.ObjectName = obj.heldObject.Value.Name;
                            loc.IsKeg = true;
                        }
                    }
                }
                // Dont care if nothing to report of.
                if(loc.ObjectLevel != 0 || loc.IsKeg)
                {
                    locations.Add(loc);
                }
            }

            return locations;
        }

        private static void Remind(List<Location> locations)
        {
            foreach (Location location in locations)
            {
                HUDMessage msg;

                if (location.ObjectLevel == ObjectLevel.Iridium)
                {
                    msg = new HUDMessage($"{location.LocationName}: {location.ObjectName} is ready!", 1);
                }
                else if (location.IsKeg)
                {
                    msg = new HUDMessage($"{location.LocationName}: Keg of {location.ObjectName} is ready!", 2);
                }
                else
                {
                    msg = new HUDMessage($"{location.LocationName}: {location.ObjectName} at {location.ObjectLevel}", 2);
                }

                Game1.addHUDMessage(msg);
            }
        }
    }
}

