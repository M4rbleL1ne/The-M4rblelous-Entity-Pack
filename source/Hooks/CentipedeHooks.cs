global using static LBMergedMods.Hooks.CentipedeHooks;
using MonoMod.Cil;
using System;
using UnityEngine;
using Mono.Cecil.Cil;
using RWCustom;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class CentipedeHooks
{
    internal static bool On_Centipede_get_Centiwing(Func<Centipede, bool> orig, Centipede self) => self is RedHorror || orig(self);

    internal static bool On_Centipede_get_Red(Func<Centipede, bool> orig, Centipede self) => self is RedHorror || orig(self);

    internal static bool On_Centipede_get_Small(Func<Centipede, bool> orig, Centipede self) => self is MiniScutigera || orig(self);

    internal static void IL_Centipede_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_AbstractCreature_state,
            s_MatchLdcR4_2_3,
            s_MatchLdcR4_7,
            s_MatchLdarg_0,
            s_MatchLdfld_Centipede_size,
            s_MatchCall_Mathf_Lerp,
            s_MatchCall_Mathf_RoundToInt,
            s_MatchStfld_CreatureState_meatLeft))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((Centipede self, AbstractCreature abstractCreature) =>
             {
                 if (self is Scutigera)
                     abstractCreature.state.meatLeft = 5;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.ctor!");
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && self is not RedHorror);
            }
        }
    }

    internal static void IL_Centipede_Crawl(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCallOrCallvirt_HealthState_get_ClampedHealth,
            s_MatchMul,
            s_MatchCall_Mathf_Lerp,
            s_MatchMul,
            s_MatchLdarg_0,
            s_MatchCallOrCallvirt_Centipede_get_Red))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Centipede self) => flag || self is Scutigera);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Crawl!");
    }

    internal static float On_Centipede_GenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit)
    {
        var tp = abstrCrit.creatureTemplate.type;
        if (tp == CreatureTemplateType.Scutigera || tp == CreatureTemplateType.RedHorrorCenti)
            return 1f;
        if (tp == CreatureTemplateType.MiniScutigera)
            return 0f;
        return orig(abstrCrit);
    }

    internal static void IL_Centipede_Shock(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_7,
            s_MatchLdcR4_0_7,
            s_MatchLdcR4_1,
            s_MatchNewobj_Color))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Color color, Centipede self) => self is Scutigera or MiniScutigera ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (part 1)");
        if (!c.TryGotoNext(MoveType.After,
            s_MatchStfld_CentipedeAI_annoyingCollisions))
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (from 1 to 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_7,
            s_MatchLdcR4_0_7,
            s_MatchLdcR4_1,
            s_MatchNewobj_Color))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Color color, Centipede self) => self is Scutigera or MiniScutigera ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (part 2)");
    }

    internal static void IL_Centipede_Stun(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && self is not RedHorror);
            }
        }
    }

    internal static void IL_Centipede_Violence(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewobj_CentipedeShell))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CentipedeShell shell, Centipede self) => self is Scutigera ? new ScutigeraShell(shell.pos, shell.vel, shell.hue, shell.saturation, shell.scaleX, shell.scaleY) : (self is RedHorror ? new RedHorrorShell(shell.pos, shell.vel, shell.hue, shell.saturation, shell.scaleX, shell.scaleY) : shell));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Violence! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchCall_Creature_Violence))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Centipede self) =>
             {
                 if (self is Scutigera)
                     self.stun = 0;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Violence! (part 2)");
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag || self is Scutigera);
            }
            else if (instr.MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && self is not RedHorror);
            }
        }
    }

    internal static void IL_CentipedeAI_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, CentipedeAI self) => flag && self is not RedHorrorAI);
            }
        }
    }

    internal static bool On_CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
    {
        var result = orig(self, critter);
        if (self.centipede is Scutigera ce && critter.realizedCreature is Creature c && c is not Centipede && c.abstractPhysicalObject.SameRippleLayer(self.creature) && c.NoCamo())
        {
            if (c.dead || c.grasps is not Creature.Grasp[] ar)
                result = false;
            else if (ce.CentiState?.health < .4f || self.StaticRelationship(critter).type == CreatureTemplate.Relationship.Type.Afraid)
                result = true;
            else
            {
                for (var i = 0; i < ar.Length; i++)
                {
                    if (ar[i]?.grabbed is Weapon)
                    {
                        result = true;
                        break;
                    }
                    else
                        result = false;
                }
            }
        }
        return result;
    }

    internal static void IL_CentipedeAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchStfld_NoiseTracker_hearingSkill))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CentipedeAI self) =>
             {
                 if (self is ScutigeraAI && self.noiseTracker is NoiseTracker t)
                     t.hearingSkill = 1.5f;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (part 1)");
        if (!c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_1,
            s_MatchCall_Mathf_Lerp,
            s_MatchStfld_CentipedeAI_excitement))
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (from 1 to 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_CentipedeAI_centipede,
            s_MatchCallOrCallvirt_Centipede_get_Centiwing))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, CentipedeAI self) => flag || self is ScutigeraAI);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (part 2)");
    }

    internal static void IL_CentipedeAI_VisualScore(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, CentipedeAI self) => flag || self is ScutigeraAI);
            }
        }
    }

    internal static CreatureTemplate.Relationship On_CentipedeAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (self is ScutigeraAI)
        {
            if (dRelation?.trackerRep is Tracker.CreatureRepresentation rep && rep.representedCreature is AbstractCreature acrit && (result.type == CreatureTemplate.Relationship.Type.Attacks || result.type == CreatureTemplate.Relationship.Type.Eats))
            {
                if (self.DoIWantToShockCreature(acrit))
                {
                    result.type = CreatureTemplate.Relationship.Type.Eats;
                    result.intensity = 1f;
                    if (self.preyTracker is PreyTracker tr)
                        tr.currentPrey = new(tr, rep);
                }
                else
                {
                    result.type = CreatureTemplate.Relationship.Type.Ignores;
                    result.intensity = 0f;
                }
            }
        }
        return result;
    }

    internal static Color On_CentipedeGraphics_get_SecondaryShellColor(Func<CentipedeGraphics, Color> orig, CentipedeGraphics self)
    {
        var res = orig(self);
        if (self.centipede is Centipede c && c is Scutigera or MiniScutigera)
        {
            var state = Random.state;
            Random.InitState(c.abstractPhysicalObject.ID.RandomSeed);
            res = Color.Lerp(res, new(res.r + .2f, res.g + .2f, res.b + .2f), Mathf.Lerp(.1f, .2f, Random.value));
            Random.state = state;
        }
        return res;
    }

    internal static void IL_CentipedeGraphics_DrawSprites(ILContext il)
    {
        var c = new ILCursor(il);
        var cnt = 0;
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                ++cnt;
                if (cnt == 2)
                {
                    c.Goto(i, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool flag, CentipedeGraphics self) => flag && self is not RedHorrorGraphics);
                    break;
                }
            }
        }
    }

    internal static void IL_CentipedeGraphics_InitiateSprites(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchLdarg_0,
            s_MatchLdarg_1,
            s_MatchLdarg_2,
            s_MatchLdnull,
            s_MatchCallOrCallvirt_GraphicsModule_AddToContainer))
        {
            ++c.Index;
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser) =>
             {
                 if (self.centipede is Centipede c)
                 {
                     if (self is ScutigeraGraphics or MiniScutigeraGraphics)
                     {
                         var sprs = sLeaser.sprites;
                         if (self is not MiniScutigeraGraphics)
                         {
                             for (var l = 0; l < 2; l++)
                             {
                                 for (var num = 0; num < self.wingPairs; num++)
                                     sprs[self.WingSprite(l, num)] = new CustomFSprite("ScutigeraWing");
                             }
                         }
                         var lg = c.bodyChunks.Length;
                         for (var i = 0; i < lg; i++)
                         {
                             sprs[self.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("ScutigeraSegment");
                             for (var j = 0; j < 2; j++)
                                 sprs[self.LegSprite(i, j, 1)].element = Futile.atlasManager.GetElementWithName("ScutigeraLegB");
                         }
                     }
                     else if (self is RedHorrorGraphics)
                     {
                         var sprs = sLeaser.sprites;
                         for (var l = 0; l < 2; l++)
                         {
                             for (var num = 0; num < self.wingPairs; num++)
                                 sprs[self.WingSprite(l, num)] = new CustomFSprite("CentipedeWing") { shader = Custom.rainWorld.Shaders["CicadaWing"] };
                         }
                         var lg = c.bodyChunks.Length;
                         for (var i = 0; i < lg; i++)
                             sprs[self.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("RedHorrorSegment");
                     }
                 }
             });
            c.Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeGraphics.InitiatesSprites!");
    }

    internal static void IL_CentipedeGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_CentipedeGraphics_lightSource,
            s_MatchLdloc_OutLoc1,
            s_MatchLdloc_InLoc1,
            s_MatchLdcR4_1,
            s_MatchNewobj_Color))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Color color, CentipedeGraphics self) => self is ScutigeraGraphics or MiniScutigeraGraphics ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeGraphics.Update!");
    }

    internal static float On_CentipedeGraphics_WhiskerLength(On.CentipedeGraphics.orig_WhiskerLength orig, CentipedeGraphics self, int part) => self is ScutigeraGraphics ? (part != 0 ? 48f : 44f) : orig(self, part);
}