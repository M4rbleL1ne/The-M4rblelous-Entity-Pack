﻿using System;
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
        private static readonly Regex entityID = new(@"^ID\.-?\d+\.-?\d+(\.-?\d+)?$");
        private static readonly string[] allTags = ["Voidsea", "Winter", "Ignorecycle", "TentacleImmune", "Lavasafe", "AlternateForm", "PreCycle", "Night", "DestroyOnAbstract", "DontSave"];
        private const string hintPrefix = "help-";

        private static HashSet<ObjType> objectTypes = [];
        private static HashSet<CritType> critterTypes = [];
        private static HashSet<SandboxUnlock> sandboxUnlocks = [];

        public static Vector2 GetMiddleOfTile(this IntVector2 vector)
        {
            return new Vector2(10f + vector.x * 20f, 10f + vector.y * 20f);
        }

        public static IntVector2 GetTilePosition(this Vector2 pos)
        {
            return new((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
        }

        public static WorldCoordinate GetWorldCoordinate(this AbstractRoom self, Vector2 pos)
        {
            return Custom.MakeWorldCoordinate(pos.GetTilePosition(), self.index);
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
                objectTypes = GetEnumTypes<ObjType>(typeof(AbstractObjectType));
                critterTypes = GetEnumTypes<CritType>(typeof(CreatureTemplateType));
                sandboxUnlocks = GetEnumTypes<SandboxUnlock>(typeof(SandboxUnlockID));

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
                if (args.Length > 1 && entityID.IsMatch(args[1]))
                {
                    startIndex = 2;
                    ID = ParseExtendedID(args[1]);
                }
                else
                {
                    ID = game.GetNewID();
                }

                List<int> foundIntegers = [];
                List<float> foundFloats = [];
                for (int i = startIndex; i < args.Length; i++)
                {
                    if (float.TryParse(args[i], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var _f)) foundFloats.Add(_f);
                    if (int.TryParse(args[i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var _i)) foundIntegers.Add(_i);
                }

                AbstractPhysicalObject entity = null!;
                var pos = GameConsole.TargetPos.Room.GetWorldCoordinate(GameConsole.TargetPos.Pos);

                if (objectTypes.Contains(objType))
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
                else if (critterTypes.Contains(critType))
                {
                    var template = StaticWorld.GetCreatureTemplate(critType);
                    var crit = new AbstractCreature(game.world, template, null, pos, ID);
                    entity = crit;
                    crit.Move(pos);

                    // Get tags to apply from arguments
                    HashSet<string> tagSet = [];
                    foreach (var tag in allTags)
                    {
                        if (args.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                        {
                            tagSet.Add(tag);
                        }
                    }
                    // Special tags
                    if (tagSet.Remove("DestroyOnAbstract")) crit.destroyOnAbstraction = true;
                    if (tagSet.Remove("DontSave")) crit.saveCreature = false;

                    var tags = tagSet.ToList();

                    // Apply creature-specific tags
                    if (StaticWorld.GetCreatureTemplate(critType).TopAncestor().type == CritType.LizardTemplate && foundFloats.Count > 0)
                    {
                        // Lizard meanness
                        tags.Add("Mean:" + foundFloats[0].ToString(NumberFormatInfo.InvariantInfo));
                    }

                    // Actually apply the tags
                    crit.spawnData = $"{{{string.Join(",", tags)}}}";
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
                foreach (var key in objectTypes)
                    yield return (key.ToString());
                foreach (var key in critterTypes)
                    yield return (key.ToString());
            }
            else
            {
                var objType = new ObjType(args[0], false);
                var critType = new CritType(args[0], false);

                var startIndex = 1;
                if (args.Length > 1 && entityID.IsMatch(args[1]))
                {
                    startIndex = 2;
                }

                // Search for ints and floats
                int foundIntegers = 0;
                int foundFloats = 0;
                for (int i = startIndex; i < args.Length; i++)
                {
                    if (float.TryParse(args[i], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _)) foundFloats++;
                    if (int.TryParse(args[i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out _)) foundIntegers++;
                }

                if (objectTypes.Contains(objType))
                {
                    // Show arguments specific to object
                    if (objType == AbstractObjectType.BlobPiece && foundFloats == 0)
                    {
                        yield return "color: float";
                    }
                }
                else if (critterTypes.Contains(critType))
                {

                    // Show generic creature tags
                    var tags = allTags.ToHashSet();
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
                        yield return hintPrefix + "mean: float";
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
                        foreach (var unlock in sandboxUnlocks)
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
                        if (sandboxUnlocks.Contains(unlock))
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
                    foreach (var unlock in sandboxUnlocks)
                    {
                        yield return (unlock.ToString());
                    }
                }
            }
            yield break;
        }

    }
}
