﻿global using static LBMergedMods.Hooks.ScavengerHooks;
using MonoMod.Cil;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class ScavengerHooks
{
    internal static void On_Scavenger_MeleeGetFree(On.Scavenger.orig_MeleeGetFree orig, Scavenger self, Creature target, bool eu)
    {
        if (self.grabbedBy is List<Creature.Grasp> l && l.Count > 0 && l[0].grabber is FatFireFly c && c == target && self.grabbedAttackCounter != 25 && !(self.Elite && self.grabbedAttackCounter % 75 == 0))
            return;
        orig(self, target, eu);
    }

    internal static int On_ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var res = orig(self, obj, weaponFiltered);
        if (obj is ThornyStrawberry st)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(st, true);
            else
                res = st.SpikesRemoved() ? 1 : 3;
        }
        else if (obj is BlobPiece or Physalis or LimeMushroom or MarineEye)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (!(weaponFiltered && self.NeedAWeapon))
                res = obj is Physalis ? 5 : (obj is LimeMushroom ? 3 : 2);
        }
        return res;
    }

    internal static int On_ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
    {
        var res = orig(self, obj, pickupDropInsteadOfWeaponSelection);
        if (obj is ThornyStrawberry st)
        {
            if (st.SpikesRemoved())
                res = 0;
            else
            {
                if (!pickupDropInsteadOfWeaponSelection && (self.currentViolenceType == ScavengerAI.ViolenceType.NonLethal || self.currentViolenceType == ScavengerAI.ViolenceType.ForFun))
                    res = 2;
                else
                    res = 3;
            }
        }
        return res;
    }

    internal static void IL_ScavengerAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("trackerRep"),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("PoleMimic"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.Scutigera || tp == CreatureTemplateType.RedHorrorCenti || tp == CreatureTemplateType.Sporantula));
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 1)");
        ILLabel? label2 = null;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("trackerRep"),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("MirosBird"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label2))
        && label2 is not null)
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type == CreatureTemplateType.Blizzor);
            c.Emit(OpCodes.Brtrue, label2);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 2)");
    }
}