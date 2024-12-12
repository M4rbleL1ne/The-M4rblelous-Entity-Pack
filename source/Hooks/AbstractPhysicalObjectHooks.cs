﻿global using static LBMergedMods.Hooks.AbstractPhysicalObjectHooks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class AbstractPhysicalObjectHooks
{
    public static ConditionalWeakTable<AbstractCreature, BigrubProperties> Big = new();
    public static ConditionalWeakTable<AbstractCreature, StrongBox<bool>> Albino = new();
    public static ConditionalWeakTable<AbstractCreature, HVFlyData> HoverflyData = new();
    public static ConditionalWeakTable<AbstractCreature, JellyProperties> Jelly = new();
    public static ConditionalWeakTable<AbstractCreature, PlayerCustomData> PlayerData = new();
    public static ConditionalWeakTable<AbstractCreature, FlyProperties> Seed = new();
    public static ConditionalWeakTable<AbstractCreature, HashSet<AbstractCreature>> SporeMemory = new();
    public static ConditionalWeakTable<AbstractConsumable, ThornyStrawberryData> StrawberryData = new();
    public static ConditionalWeakTable<AbstractConsumable, RubberBlossomProperties> StationPlant = new();
    public static ConditionalWeakTable<AbstractConsumable, GummyAntherProperties> StationFruit = new();

    internal static void On_AbstractConsumable_ctor(On.AbstractConsumable.orig_ctor orig, AbstractConsumable self, World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
    {
        orig(self, world, type, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData);
        if (type == AbstractObjectType.ThornyStrawberry && !StrawberryData.TryGetValue(self, out _))
            StrawberryData.Add(self, new());
        else if (type == AbstractObjectType.GummyAnther && !StationFruit.TryGetValue(self, out _))
            StationFruit.Add(self, new());
        else if (type == AbstractObjectType.RubberBlossom && !StationPlant.TryGetValue(self, out _))
        {
            var state = Random.state;
            Random.InitState(self.ID.RandomSeed);
            RubberBlossomProperties dt;
            if (consumableData is RubberBlossomData data)
            {
                dt = new(data.StartsOpen, data.FoodChance ? Random.Range(0, data.FoodAmount + 1) : data.FoodAmount, data.StartsOpen ? (data.RandomOpen ? Random.Range(1, data.CyclesOpen + 1) : data.CyclesOpen) : (data.RandomClosed ? Random.Range(1, data.CyclesClosed + 1) : data.CyclesClosed), data.AlwaysOpen && !data.AlwaysClosed, data.AlwaysClosed && !data.AlwaysOpen);
                if (data.StartsOpen)
                {
                    self.maxCycles = data.CyclesClosed + 1;
                    self.minCycles = data.RandomClosed ? 2 : self.maxCycles;
                }
                else
                {
                    self.maxCycles = data.CyclesOpen + 1;
                    self.minCycles = data.RandomOpen ? 2 : self.maxCycles;
                }
            }
            else
                dt = new(true, Random.Range(0, 4), Random.Range(1, 11), false, false);
            StationPlant.Add(self, dt);
            Random.state = state;
        }
    }

    internal static void On_AbstractConsumable_Consume(On.AbstractConsumable.orig_Consume orig, AbstractConsumable self)
    {
        if (StationPlant.TryGetValue(self, out var props) && !props.AlwaysClosed && !props.AlwaysOpen)
        {
            if (!self.isConsumed)
            {
                if (props.RemainingOpenCycles > 0)
                    --props.RemainingOpenCycles;
                self.isConsumed = props.RemainingOpenCycles == 0;
                if (self.world.game.session is StoryGameSession session)
                    session.saveState.ReportConsumedItem(self.world, false, self.originRoom, self.placedObjectIndex, self.minCycles > 0 ? Random.Range(self.minCycles, self.maxCycles + 1) + props.RemainingOpenCycles * 100 : -1);
            }
        }
        else
            orig(self);
    }

    internal static void IL_AbstractCreature_InitiateAI(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_Slugcat,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AbstractCreature self) => flag || CreatureTemplateType.s_M4RCreatureList.Contains(self.creatureTemplate.type));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook AbstractCreature.InitiateAI!");
    }

    internal static void IL_AbstractCreature_MSCInitiateAI(On.AbstractCreature.orig_MSCInitiateAI orig, AbstractCreature self)
    {
        if (!CreatureTemplateType.s_M4RCreatureList.Contains(self.creatureTemplate.type))
            orig(self);
    }

    internal static bool On_AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type) => type == AbstractObjectType.BouncingMelon || type == AbstractObjectType.ThornyStrawberry || type == AbstractObjectType.LittleBalloon || type == AbstractObjectType.Physalis || type == AbstractObjectType.LimeMushroom || type == AbstractObjectType.RubberBlossom || type == AbstractObjectType.GummyAnther || type == AbstractObjectType.MarineEye || type == AbstractObjectType.StarLemon || type == AbstractObjectType.DendriticNeuron || orig(type);

    internal static void On_AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);
        if (world is null)
            return;
        var tp = creatureTemplate.type;
        if (tp == CreatureTemplate.Type.Fly && !Seed.TryGetValue(self, out _))
            Seed.Add(self, new() { IsSeed = self.Room is AbstractRoom rm && rm.SeedBatRooms() });
        else if (tp == CreatureTemplate.Type.TubeWorm && !Big.TryGetValue(self, out _))
            Big.Add(self, new());
        else if (tp == CreatureTemplateType.Hoverfly && !HoverflyData.TryGetValue(self, out _))
            HoverflyData.Add(self, new());
        else if ((tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplate.Type.JetFish || tp == CreatureTemplateType.Denture) && !Albino.TryGetValue(self, out _))
            Albino.Add(self, new());
        if (tp == CreatureTemplateType.Denture)
            self.remainInDenCounter = 0;
    }

    internal static void On_AbstractCreature_IsEnteringDen(On.AbstractCreature.orig_IsEnteringDen orig, AbstractCreature self, WorldCoordinate den)
    {
        var tp = self.creatureTemplate.type;
        if ((tp == CreatureTemplateType.Hoverfly || tp == CreatureTemplateType.TintedBeetle) && self.stuckObjects is List<AbstractPhysicalObject.AbstractObjectStick> list)
        {
            for (var num = list.Count - 1; num >= 0; num--)
            {
                if (list[num] is AbstractPhysicalObject.CreatureGripStick stick && stick.A == self && stick.B is AbstractPhysicalObject obj)
                {
                    var abai = self.abstractAI;
                    var ai = abai?.RealAI;
                    if (ai is HoverflyAI hvai)
                        hvai.FoodTracker?.ForgetItem(obj);
                    else if (ai is TintedBeetleAI tbai)
                        tbai.FoodTracker?.ForgetItem(obj);
                    obj.Destroy();
                    obj.realizedObject?.Destroy();
                    if (self.remainInDenCounter > -1 && self.remainInDenCounter < 200 && !self.WantToStayInDenUntilEndOfCycle())
                        self.remainInDenCounter = 200;
                    if (abai is null || abai.DoIwantToDropThisItemInDen(obj))
                        self.DropCarriedObject(stick.grasp);
                }
            }
        }
        orig(self, den);
    }

    internal static bool On_AbstractCreature_IsVoided(On.AbstractCreature.orig_IsVoided orig, AbstractCreature self)
    {
        var res = orig(self);
        var tp = self.creatureTemplate.type;
        return res || (self.voidCreature && (tp == CreatureTemplate.Type.RedLizard || tp == CreatureTemplate.Type.RedCentipede || tp == CreatureTemplate.Type.CyanLizard || tp == CreatureTemplate.Type.BigSpider || tp == CreatureTemplate.Type.DaddyLongLegs || tp == CreatureTemplate.Type.BrotherLongLegs || tp == CreatureTemplate.Type.BigEel || tp == CreatureTemplateType.RedHorrorCenti));
    }

    internal static void On_AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
    {
        orig(self);
        if (self.Room is not AbstractRoom rm)
            return;
        if ((!ModManager.MSC || rm.world.game.session is not ArenaGameSession sess || sess.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge) && self.spawnData is string s && s.Length > 1 && s[0] == '{')
        {
            var array = s.Substring(1, s.Length - 2).Split(',');
            for (var i = 0; i < array.Length; i++)
            {
                var ari = array[i];
                if (ari.Length > 0)
                {
                    var nm = ari.Split(':')[0];
                    if (string.Equals(nm, "seedbat", StringComparison.OrdinalIgnoreCase) && Seed.TryGetValue(self, out var props))
                        props.IsSeed = true;
                    else if (Big.TryGetValue(self, out var props2))
                    {
                        if (string.Equals(nm, "bigrub", StringComparison.OrdinalIgnoreCase))
                            props2.IsBig = true;
                        else if (string.Equals(nm, "altbigrub", StringComparison.OrdinalIgnoreCase))
                        {
                            props2.IsBig = true;
                            self.superSizeMe = true;
                        }
                    }
                    else if (string.Equals(nm, "albinoform", StringComparison.OrdinalIgnoreCase) && Albino.TryGetValue(self, out var props4))
                        props4.Value = true;
                }
            }
        }
    }

    internal static void IL_AbstractCreature_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_PoleMimic,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AbstractCreature self) => flag || self.creatureTemplate.type == CreatureTemplateType.Denture);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook AbstractCreature.Update!");
    }

    internal static void On_AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);
        if (!self.slatedForDeletion && self.state is CreatureState st && !st.dead && HoverflyData.TryGetValue(self, out var d))
        {
            if (d.CanEatRootDelay > 0)
                --d.CanEatRootDelay;
            if (d.BiteWait > 0 && self.realizedCreature is Hoverfly f && f.grasps[0]?.grabbed is DangleFruit)
                --d.BiteWait;
        }
    }

    internal static void IL_AbstractCreature_WantToStayInDenUntilEndOfCycle(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_PoleMimic,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AbstractCreature self) => flag || self.creatureTemplate.type == CreatureTemplateType.Denture);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook AbstractCreature.WantToStayInDenUntilEndOfCycle!");
    }

    internal static bool On_AbstractCreature_WantToStayInDenUntilEndOfCycle(On.AbstractCreature.orig_WantToStayInDenUntilEndOfCycle orig, AbstractCreature self)
    {
        if (self.realizedCreature is MiniLeech l && l.fleeFromRain)
            return true;
        return orig(self);
    }

    internal static void On_AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.realizedObject is null)
        {
            var type = self.type;
            if (type == AbstractObjectType.ThornyStrawberry)
                self.realizedObject = new ThornyStrawberry(self, self.world);
            else if (type == AbstractObjectType.SporeProjectile)
                self.realizedObject = new SmallPuffBall(self, self.world);
            else if (type == AbstractObjectType.BlobPiece)
                self.realizedObject = new BlobPiece(self);
            else if (type == AbstractObjectType.LittleBalloon)
                self.realizedObject = new LittleBalloon(self);
            else if (type == AbstractObjectType.BouncingMelon)
                self.realizedObject = new BouncingMelon(self);
            else if (type == AbstractObjectType.Physalis)
                self.realizedObject = new Physalis(self);
            else if (type == AbstractObjectType.LimeMushroom)
                self.realizedObject = new LimeMushroom(self);
            else if (type == AbstractObjectType.GummyAnther)
                self.realizedObject = new GummyAnther(self);
            else if (type == AbstractObjectType.RubberBlossom)
                self.realizedObject = new RubberBlossom(self);
            else if (type == AbstractObjectType.MarineEye)
                self.realizedObject = new MarineEye(self);
            else if (type == AbstractObjectType.StarLemon)
                self.realizedObject = new StarLemon(self);
            else if (type == AbstractObjectType.DendriticNeuron)
                self.realizedObject = new DendriticNeuron(self);
        }
    }

    public static (bool Consumed, int WaitCycles) PlantConsumed(this RegionState self, int originRoom, int placedObjectIndex)
    {
        var consumedItems = self.consumedItems;
        for (var num = consumedItems.Count - 1; num >= 0; num--)
        {
            var item = consumedItems[num];
            if (item.originRoom == originRoom && item.placedObjectIndex == placedObjectIndex)
                return (true, item.waitCycles);
        }
        return (false, 101);
    }

    public static (bool Consumed, int WaitCycles) PlantConsumed(this SaveState self, World world, int originroom, int placedObjectIndex)
    {
        if (world.singleRoomWorld || originroom < 0 || placedObjectIndex < 0 || originroom < world.firstRoomIndex || originroom >= world.firstRoomIndex + world.NumberOfRooms || self.regionStates[world.region.regionNumber] is not RegionState st)
            return (false, 101);
        return st.PlantConsumed(originroom, placedObjectIndex);
    }
}