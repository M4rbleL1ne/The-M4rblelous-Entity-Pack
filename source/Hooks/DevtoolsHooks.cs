global using static LBMergedMods.Hooks.DevtoolsHooks;
using DevInterface;
using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LBMergedMods.Hooks;

public static class DevtoolsHooks
{
    internal static void On_ConsumableObjectData_ctor(On.PlacedObject.ConsumableObjectData.orig_ctor orig, PlacedObject.ConsumableObjectData self, PlacedObject owner)
    {
        orig(self, owner);
        if (owner.type == PlacedObjectType.HazerMom || owner.type == PlacedObjectType.AlbinoHazerMom || owner.type == PlacedObjectType.DeadHazerMom || owner.type == PlacedObjectType.DeadAlbinoHazerMom || owner.type == PlacedObjectType.DeadAlbinoFormHazer || owner.type == PlacedObjectType.AlbinoFormHazer || owner.type == PlacedObjectType.XyloNest || owner.type == PlacedObjectType.AltXyloNest || owner.type == PlacedObjectType.YellowXyloNest || owner.type == PlacedObjectType.XyloWorm || owner.type == PlacedObjectType.BigXyloWorm || owner.type == PlacedObjectType.DeadXyloWorm || owner.type == PlacedObjectType.DeadBigXyloWorm)
        {
            self.minRegen = 7;
            self.maxRegen = 10;
        }
    }

    internal static Color On_CreatureVis_CritCol(On.DevInterface.MapPage.CreatureVis.orig_CritCol orig, AbstractCreature crit)
    {
        var res = orig(crit);
        if (crit.creatureTemplate.type == CreatureTemplate.Type.TubeWorm && crit.IsBig())
            res = Color.green;
        return res;
    }

    internal static string On_CreatureVis_CritString(On.DevInterface.MapPage.CreatureVis.orig_CritString orig, AbstractCreature crit)
    {
        var res = orig(crit);
        if (crit.creatureTemplate.type == CreatureTemplate.Type.Fly && crit.IsSeed())
            res = "sdb";
        else if (crit.creatureTemplate.type == CreatureTemplate.Type.TubeWorm && crit.IsBig())
            res = "bgr";
        return res;
    }

    internal static void IL_ObjectsPage_AssembleObjectPages(ILContext il)
    {
        ILLabel? label = null;
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcI4_1,
            s_MatchAdd,
            s_MatchStloc_OutLoc1)
         && c.TryGotoNext(
            s_MatchLdloca_Any))
        {
            label = c.MarkLabel();
            c.Index = 0;
            if (c.TryGotoNext(MoveType.After,
                s_MatchLdcI4_1,
                s_MatchAdd,
                s_MatchStloc_OutLoc1))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
                 .EmitDelegate((ObjectsPage self, int num3) => num3 >= self.placedObjectTypes.Length);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ObjectsPage.AssembleObjectPages! (part 2)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ObjectsPage.AssembleObjectPages! (part 1)");
    }

    internal static void On_ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
    {
        if (tp == PlacedObjectType.ThornyStrawberry || tp == PlacedObjectType.LittleBalloon || tp == PlacedObjectType.BouncingMelon || tp == PlacedObjectType.HazerMom || tp == PlacedObjectType.AlbinoHazerMom || tp == PlacedObjectType.DeadHazerMom || tp == PlacedObjectType.DeadAlbinoHazerMom || tp == PlacedObjectType.DeadAlbinoFormHazer || tp == PlacedObjectType.AlbinoFormHazer || tp == PlacedObjectType.Physalis || tp == PlacedObjectType.LimeMushroom || tp == PlacedObjectType.MarineEye || tp == PlacedObjectType.StarLemon || tp == PlacedObjectType.DendriticNeuron || tp == PlacedObjectType.SporeProjectile || tp == PlacedObjectType.XyloNest || tp == PlacedObjectType.AltXyloNest || tp == PlacedObjectType.YellowXyloNest || tp == PlacedObjectType.XyloWorm || tp == PlacedObjectType.BigXyloWorm || tp == PlacedObjectType.DeadXyloWorm || tp == PlacedObjectType.DeadBigXyloWorm || tp == PlacedObjectType.FumeFruit || tp == PlacedObjectType.Durian)
        {
            if (pObj is null)
                self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                });
            var pObjRep = new ConsumableRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
            self.tempNodes.Add(pObjRep);
            self.subNodes.Add(pObjRep);
        }
        else if (tp == PlacedObjectType.RubberBlossom)
        {
            if (pObj is null)
                self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                });
            var rep = new RubberBlossomRepresentation(self.owner, self, pObj);
            self.tempNodes.Add(rep);
            self.subNodes.Add(rep);
        }
        else if (tp == PlacedObjectType.DarkGrub)
        {
            if (pObj is null)
                self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                });
            var rep = new DarkGrubRepresentation(self.owner, self, pObj);
            self.tempNodes.Add(rep);
            self.subNodes.Add(rep);
        }
        else if (tp == PlacedObjectType.MiniFruitBranch)
        {
            if (pObj is null)
                self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                });
            var rep = new MiniFruitSpawnerRepresentation(self.owner, self, pObj);
            self.tempNodes.Add(rep);
            self.subNodes.Add(rep);
        }
        else if (tp == PlacedObjectType.BonusScoreToken)
        {
            if (pObj is null)
                self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                });
            var pObjRep = new ScoreTokenRepresentation(self.owner, "BonusScoreToken_Rep", self, pObj);
            self.tempNodes.Add(pObjRep);
            self.subNodes.Add(pObjRep);
        }
        else
            orig(self, tp, pObj);
    }

    internal static ObjectsPage.DevObjectCategories On_ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
    {
        if (type == PlacedObjectType.ThornyStrawberry || type == PlacedObjectType.LittleBalloon || type == PlacedObjectType.BouncingMelon || type == PlacedObjectType.AlbinoHazerMom || type == PlacedObjectType.DeadAlbinoHazerMom || type == PlacedObjectType.DeadHazerMom || type == PlacedObjectType.HazerMom || type == PlacedObjectType.DeadAlbinoFormHazer || type == PlacedObjectType.AlbinoFormHazer || type == PlacedObjectType.Physalis || type == PlacedObjectType.LimeMushroom || type == PlacedObjectType.RubberBlossom || type == PlacedObjectType.MarineEye || type == PlacedObjectType.StarLemon || type == PlacedObjectType.DendriticNeuron || type == PlacedObjectType.MiniFruitBranch || type == PlacedObjectType.SporeProjectile || type == PlacedObjectType.BonusScoreToken || type == PlacedObjectType.XyloNest || type == PlacedObjectType.AltXyloNest || type == PlacedObjectType.YellowXyloNest || type == PlacedObjectType.XyloWorm || type == PlacedObjectType.BigXyloWorm || type == PlacedObjectType.DeadXyloWorm || type == PlacedObjectType.DeadBigXyloWorm || type == PlacedObjectType.FumeFruit || type == PlacedObjectType.Durian || type == PlacedObjectType.DarkGrub)
            return DevObjectCategories.M4rblelousEntities;
        return orig(self, type);
    }

    internal static void On_PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
    {
        if (self.type == PlacedObjectType.ThornyStrawberry || self.type == PlacedObjectType.LittleBalloon || self.type == PlacedObjectType.BouncingMelon || self.type == PlacedObjectType.HazerMom || self.type == PlacedObjectType.AlbinoHazerMom || self.type == PlacedObjectType.DeadHazerMom || self.type == PlacedObjectType.DeadAlbinoHazerMom || self.type == PlacedObjectType.DeadAlbinoFormHazer || self.type == PlacedObjectType.AlbinoFormHazer || self.type == PlacedObjectType.Physalis || self.type == PlacedObjectType.LimeMushroom || self.type == PlacedObjectType.MarineEye || self.type == PlacedObjectType.StarLemon || self.type == PlacedObjectType.DendriticNeuron || self.type == PlacedObjectType.SporeProjectile || self.type == PlacedObjectType.XyloNest || self.type == PlacedObjectType.AltXyloNest || self.type == PlacedObjectType.YellowXyloNest || self.type == PlacedObjectType.XyloWorm || self.type == PlacedObjectType.BigXyloWorm || self.type == PlacedObjectType.DeadXyloWorm || self.type == PlacedObjectType.DeadBigXyloWorm || self.type == PlacedObjectType.FumeFruit || self.type == PlacedObjectType.Durian)
            self.data = new PlacedObject.ConsumableObjectData(self);
        else if (self.type == PlacedObjectType.DarkGrub)
            self.data = new DarkGrubData(self);
        else if (self.type == PlacedObjectType.RubberBlossom)
            self.data = new RubberBlossomData(self);
        else if (self.type == PlacedObjectType.MiniFruitBranch)
            self.data = new MiniFruitSpawnerData(self);
        else if (self.type == PlacedObjectType.BonusScoreToken)
            self.data = new ScoreTokenData(self);
        else
            orig(self);
    }

    internal static RoomSettingsPage.DevEffectsCategories On_RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        if (type == RoomEffectType.SeedBats || type == RoomEffectType.Bigrubs)
            return DevEffectsCategories.M4rblelousEntities;
        return orig(self, type);
    }
}