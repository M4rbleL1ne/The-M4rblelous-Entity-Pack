using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DevConsole;
using DevConsole.Commands;
using RWCustom;
using UnityEngine;
using CritType = CreatureTemplate.Type;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using Random = UnityEngine.Random;
using SandboxUnlock = MultiplayerUnlocks.SandboxUnlockID;

namespace LBMergedMods
{
    internal static class DevConsoleCommands
    {
        private static readonly Regex s_EntityID = new(@"^ID\.-?\d+\.-?\d+(\.-?\d+)?$");
        private static readonly string[] s_AllTags = ["Voidsea", "Winter", "Ignorecycle", "TentacleImmune", "Lavasafe", "AlternateForm", "PreCycle", "Night", "AlbinoForm", "DestroyOnAbstract", "DontSave"];
        private const string s_HintPrefix = "help-";

        private static HashSet<ObjType> s_ObjectTypes = [];
        private static HashSet<CritType> s_CritterTypes = [];
        private static HashSet<SandboxUnlock> s_SandboxUnlocks = [];

        public static WorldCoordinate GetWorldCoordinate(this AbstractRoom self, Vector2 pos)
        {
            return Custom.MakeWorldCoordinate(Room.StaticGetTilePosition(pos), self.index);
        }

        public static HashSet<T> GetEnumTypes<T>(Type C)
        {
            return C.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(T) && x.GetValue(null) != null)
                .Select(x => x.GetValue(null))
                .Cast<T>()
                .ToHashSet();
        }

        public static void RegisterDevConsole()
        {
            try
            {
                // Collect information using reflection so we don't have to worry about collecting it later when it inevitably changes lmao
                s_ObjectTypes = GetEnumTypes<ObjType>(typeof(AbstractObjectType));
                s_CritterTypes = CreatureTemplateType.s_M4RCreatureList.Union([new CritType("Bigrub", false), new CritType("Bigworm", false), new CritType("SeedBat", false), new CritType("JellyLongLegs", false)]).ToHashSet();
                s_SandboxUnlocks = GetEnumTypes<SandboxUnlock>(typeof(SandboxUnlockID));

                // Shrembly
                if (ModManager.ActiveMods.Any(x => x.id == "com.rainworldgame.shroudedassembly.plugin"))
                {
                    s_CritterTypes.Add(new CritType("BabyCroaker", false));
                    s_CritterTypes.Add(new CritType("Gecko", false));
                    s_CritterTypes.Add(new CritType("MaracaSpider", false));
                    s_ObjectTypes.Add(new ObjType("PureCrystal", false));
                    s_ObjectTypes.Add(new ObjType("RockFruit", false));
                }

                // Actually register commands
                new CommandBuilder("spawn_lb")
                    .Help("spawn_lb [type] [ID?] [args...]")
                    .RunGame(SpawnLB)
                    .AutoComplete(SpawnLBAutoComplete)
                    .Register();
                new CommandBuilder("unlock_lb")
                    .Help("unlock_lb enable|disable all|[tokens...]")
                    .Run(SandboxLB)
                    .AutoComplete(SandboxLBAutoComplete)
                    .Register();
            }
            catch (Exception e)
            {
                LBMergedModsPlugin.s_logger.LogError(e);
                GameConsole.WriteLine(e.ToString(), Color.red);
            }
        }

        private static EntityID ParseExtendedID(string id)
        {
            // From Dev Console
            EntityID outID = EntityID.FromString(id);
            string[] split = id.Split('.');
            if (split.Length > 3 && int.TryParse(split[3], out int altSeed))
            {
                outID.setAltSeed(altSeed);
            }
            return outID;
        }

        private static void SpawnLB(RainWorldGame game, string[] args)
        {
            if (args.Length == 0)
            {
                GameConsole.WriteLine("No object type specified!");
                return;
            }

            try
            {
                var objType = new ObjType(args[0], false);
                var critType = new CritType(args[0], false);

                var startIndex = 1;
                EntityID ID;
                if (args.Length > 1 && s_EntityID.IsMatch(args[1]))
                {
                    startIndex = 2;
                    ID = ParseExtendedID(args[1]);
                }
                else
                {
                    ID = game.GetNewID();
                }

                List<float> foundFloats = [];
                List<bool> foundBools = [];
                for (int i = startIndex; i < args.Length; i++)
                {
                    if (float.TryParse(args[i], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var _f)) foundFloats.Add(_f);
                    else if (bool.TryParse(args[i], out var _b)) foundBools.Add(_b);
                }

                AbstractPhysicalObject entity = null!;
                var pos = GameConsole.TargetPos.Room.GetWorldCoordinate(GameConsole.TargetPos.Pos);

                // Generic cases
                if (s_ObjectTypes.Contains(objType))
                {
                    // Special cases
                    if (objType == AbstractObjectType.BlobPiece)
                    {
                        float c = Mathf.Lerp(Random.Range(0f, 1f), .5f, Mathf.Pow(Random.value, 2f));
                        if (foundFloats.Count != 0)
                        {
                            c = foundFloats[0];
                        }
                        entity = new BlobPiece.AbstractBlobPiece(game.world, null, pos, ID, c);
                    }
                    else if (objType == AbstractObjectType.ThornyStrawberry)
                    {
                        var strawb = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                        entity = strawb;
                        if (foundBools.Count > 0 && StrawberryData.TryGetValue(strawb, out var data))
                        {
                            data.SpikesRemoved = foundBools[0];
                        }
                    }
                    else if (objType == AbstractObjectType.RubberBlossom)
                    {
                        if (foundBools.Count == 0 || (foundFloats.Count != 1 && foundFloats.Count != 4))
                        {
                            GameConsole.WriteLine("Invalid spawn line for rubber blossom!");
                            return;
                        }
                        bool open = foundBools[0];
                        GameConsole.WriteLine("OPEN IS " + open);
                        Color color = StationPlantCol;
                        if (foundFloats.Count > 1)
                        {
                            color = new Color(foundFloats[1], foundFloats[2], foundFloats[3]);
                        }

                        var flower = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                        StationPlant.Remove(flower);
                        StationPlant.Add(flower, new RubberBlossomProperties(open, (int)foundFloats[0], 999, open, !open) { Open = open, DevSpawn = true, forceMaxVel = foundFloats[0], forceColor = color });
                        entity = flower;
                    }

                    // Generic cases
                    else if (AbstractConsumable.IsTypeConsumable(objType))
                    {
                        entity = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                    }
                    else
                    {
                        entity = new AbstractPhysicalObject(game.world, objType, null, pos, ID);
                    }
                }
                else if (s_CritterTypes.Contains(critType))
                {
                    var actualCritType = critType;
                    if (critType.value == "Bigrub" || critType.value == "Bigworm") actualCritType = CritType.TubeWorm;
                    else if (critType.value == "SeedBat") actualCritType = CritType.Fly;
                    else if (critType.value == "JellyLongLegs") actualCritType = CritType.BrotherLongLegs;

                    var template = StaticWorld.GetCreatureTemplate(actualCritType);
                    var crit = new AbstractCreature(game.world, template, null, pos, ID);
                    entity = crit;
                    crit.Move(pos);

                    // Get tags to apply from arguments
                    HashSet<string> tagSet = [];
                    foreach (var tag in s_AllTags)
                    {
                        if (args.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                        {
                            tagSet.Add(tag);
                        }
                    }
                    // Special tags
                    if (tagSet.Remove("DestroyOnAbstract")) crit.destroyOnAbstraction = true;
                    if (tagSet.Remove("DontSave")) crit.saveCreature = false;

                    if (critType.value == "Bigrub")
                        tagSet.Add(foundBools.Count > 0 && foundBools[0] ? "altbigrub" : "bigrub");
                    if (critType.value == "Bigworm")
                        tagSet.Add("bigworm");
                    else if (critType.value == "SeedBat")
                        tagSet.Add("seedbat");
                    else if (critType.value == "JellyLongLegs")
                        Jelly.Add(crit, new JellyProperties() { Born = false, IsJelly = true });
                    
                    var tags = tagSet.ToList();

                    // Apply creature-specific tags
                    if (critType.Index > -1 && StaticWorld.GetCreatureTemplate(critType).TopAncestor().type == CritType.LizardTemplate && foundFloats.Count > 0)
                    {
                        // Lizard meanness
                        tags.Add("Mean:" + foundFloats[0].ToString(NumberFormatInfo.InvariantInfo));
                    }

                    // Actually apply the tags
                    crit.spawnData = $"{{{string.Join(",", tags)}}}";
                    crit.setCustomFlags();
                }
                else if (objType.index > -1 || critType.index > -1)
                {
                    GameConsole.WriteLine("Use spawn or spawn_raw for non-M4rblelous object types!");
                    return;
                }
                else
                {
                    GameConsole.WriteLine("Invalid object type!");
                    return;
                }

                // Realize created object, if there is one
                if (entity != null)
                {
                    GameConsole.TargetPos.Room.AddEntity(entity);
                    entity.RealizeInRoom();
                }
            }
            catch (Exception ex)
            {
                LBMergedModsPlugin.s_logger.LogError(ex);
                GameConsole.WriteLine(ex.ToString(), Color.red);
            }
        }

        private static IEnumerable<string> SpawnLBAutoComplete(string[] args)
        {
            if (args.Length == 0)
            {
                // All objects and creatures
                foreach (var key in s_ObjectTypes)
                    yield return (key.ToString());
                foreach (var key in s_CritterTypes)
                    yield return (key.ToString());
            }
            else
            {
                var objType = new ObjType(args[0], false);
                var critType = new CritType(args[0], false);

                var startIndex = 1;
                if (args.Length > 1 && s_EntityID.IsMatch(args[1]))
                {
                    startIndex = 2;
                }

                // Search for ints and floats
                int foundFloats = 0;
                int foundBools = 0;
                for (int i = startIndex; i < args.Length; i++)
                {
                    if (float.TryParse(args[i], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _)) foundFloats++;
                    else if (bool.TryParse(args[i], out _)) foundBools++;
                }

                if (s_ObjectTypes.Contains(objType))
                {
                    // Show arguments specific to object
                    if (objType == AbstractObjectType.BlobPiece && foundFloats == 0)
                    {
                        yield return s_HintPrefix + "color: float";
                    }
                    else if (objType == AbstractObjectType.ThornyStrawberry && foundBools == 0)
                    {
                        yield return s_HintPrefix + "thorns: bool";
                    }
                    else if (objType == AbstractObjectType.RubberBlossom)
                    {
                        if (foundBools == 0)
                            yield return "open: bool";
                        else
                        {
                            bool open = false;
                            args.FirstOrDefault(x => bool.TryParse(x, out open));

                            string? hint = foundFloats switch
                            {
                                0 when open => s_HintPrefix + "food: int",
                                0 when !open => s_HintPrefix + "boost: float",
                                1 => s_HintPrefix + "r: float",
                                2 => s_HintPrefix + "g: float",
                                3 => s_HintPrefix + "b: float",
                                _ => null
                            };
                            if (hint != null)
                            {
                                yield return hint;
                            }
                        }
                    }
                }
                else if (s_CritterTypes.Contains(critType))
                {

                    // Show generic creature tags
                    var tags = s_AllTags.ToHashSet();
                    for (int i = startIndex; i < args.Length; i++)
                    {
                        tags.Remove(args[i]);
                    }
                    foreach (var tag in tags)
                    {
                        yield return tag;
                    }

                    // Show arguments specific to creature
                    if (StaticWorld.GetCreatureTemplate(critType).TopAncestor().type == CritType.LizardTemplate && foundFloats == 0)
                    {
                        // Lizard-specific
                        yield return s_HintPrefix + "mean: float";
                    }
                    else if (critType.value == "Bigrub" && foundBools == 0)
                    {
                        yield return s_HintPrefix + "alt: bool";
                    }
                }
            }
        }

        private static void SandboxLB(string[] args)
        {
            if (args.Length == 0)
            {
                GameConsole.WriteLine("Missing arguments!");
            }
            else if (args[0] == "enable" || args[0] == "disable")
            {
                bool enable = args[0] == "enable";

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "ALL")
                    {
                        // Unlock all
                        foreach (var unlock in s_SandboxUnlocks)
                        {
                            if (enable)
                            {
                                bool result = Custom.rainWorld.progression.miscProgressionData.SetTokenCollected(unlock);
                                if (result)
                                {
                                    GameConsole.WriteLine("Unlocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                                }
                            }
                            else
                            {
                                bool result = Custom.rainWorld.progression.miscProgressionData.sandboxTokens.Remove(unlock);
                                if (result)
                                {
                                    GameConsole.WriteLine("Relocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        var unlock = new SandboxUnlock(args[i], false);
                        if (s_SandboxUnlocks.Contains(unlock))
                        {
                            if (enable)
                            {
                                bool result = Custom.rainWorld.progression.miscProgressionData.SetTokenCollected(unlock);
                                if (result)
                                {
                                    GameConsole.WriteLine("Unlocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                                }
                            }
                            else
                            {
                                bool result = Custom.rainWorld.progression.miscProgressionData.sandboxTokens.Remove(unlock);
                                if (result)
                                {
                                    GameConsole.WriteLine("Relocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                                }
                            }
                        }
                        else if (unlock.index > -1)
                        {
                            GameConsole.WriteLine("Cannot use unlock '" + unlock.ToString() + "' with unlock_fl!");
                        }
                        else
                        {
                            GameConsole.WriteLine("Invalid unlock token '" + unlock.ToString() + "!");
                        }
                    }
                }
            }
            else
            {
                GameConsole.WriteLine("Invalid argument! Must have 'enable' or 'disable'.");
            }
        }

        private static IEnumerable<string> SandboxLBAutoComplete(string[] args)
        {
            if (args.Length == 0)
            {
                yield return "enable";
                yield return "disable";
            }
            else
            {
                if (args[0] == "enable" || args[0] == "disable")
                {
                    yield return "ALL";
                    foreach (var unlock in s_SandboxUnlocks)
                    {
                        yield return (unlock.ToString());
                    }
                }
            }
            yield break;
        }

    }
}
