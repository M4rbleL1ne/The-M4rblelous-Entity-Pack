global using static LBMergedMods.Hooks.AbstractPhysicalObjectHooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class AbstractPhysicalObjectHooks
{
    public static ConditionalWeakTable<AbstractCreature, BigrubProperties> Big = new();
    public static ConditionalWeakTable<AbstractCreature, AbstractProperties> AbsProps = new();
    public static ConditionalWeakTable<AbstractCreature, HVFlyData> HoverflyData = new();
    public static ConditionalWeakTable<AbstractCreature, JellyProperties> Jelly = new();
    public static ConditionalWeakTable<AbstractCreature, PlayerCustomData> PlayerData = new();
    public static ConditionalWeakTable<AbstractCreature, FlyProperties> Seed = new();
    public static ConditionalWeakTable<AbstractCreature, HashSet<AbstractCreature>> SporeMemory = new();
    public static ConditionalWeakTable<AbstractConsumable, ThornyStrawberryData> StrawberryData = new();
    public static ConditionalWeakTable<AbstractConsumable, RubberBlossomProperties> StationPlant = new();
    public static ConditionalWeakTable<AbstractConsumable, GummyAntherProperties> StationFruit = new();
    public static ConditionalWeakTable<AbstractConsumable, MiniFruitSpawnerProperties> MiniFruitSpawners = new();
    public static ConditionalWeakTable<AbstractConsumable, MiniFruitProperties> MiniFruits = new();

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
        else if (type == AbstractObjectType.MiniBlueFruit && !MiniFruits.TryGetValue(self, out _))
            MiniFruits.Add(self, new());
        else if (type == AbstractObjectType.MiniFruitSpawner && !MiniFruitSpawners.TryGetValue(self, out _))
            MiniFruitSpawners.Add(self, new());
    }

    internal static void On_AbstractConsumable_Consume(On.AbstractConsumable.orig_Consume orig, AbstractConsumable self)
    {
        if (MiniFruits.TryGetValue(self, out var fprops) && fprops.Spawner is AbstractConsumable cons && MiniFruitSpawners.TryGetValue(cons, out var prps))
        {
            if (!self.isConsumed)
            {
                if (prps.NumberOfFruits > 0)
                    --prps.NumberOfFruits;
                self.isConsumed = true;
                if (self.world.game.session is StoryGameSession sess)
                    sess.saveState.ReportConsumedFruit(self.world, self.originRoom, self.placedObjectIndex);
            }
        }
        /*else if (StationFruit.TryGetValue(self, out var pprops) && !self.isConsumed && pprops.Plant is not null) // removal intended
        {
            self.isConsumed = true;
            if (self.world.game.session is StoryGameSession sess)
                sess.saveState.ReportConsumedFruit(self.world, self.originRoom, self.placedObjectIndex);
        }*/
        else if (StationPlant.TryGetValue(self, out var props) && !props.AlwaysClosed && !props.AlwaysOpen)
        {
            if (!self.isConsumed)
            {
                if (props.RemainingOpenCycles > 0)
                    --props.RemainingOpenCycles;
                self.isConsumed = props.RemainingOpenCycles == 0;
                if (self.world.game.session is StoryGameSession session)
                {
                    /*if (self.isConsumed)
                        session.saveState.ClearConsumedFruits(self.world, self.originRoom, self.placedObjectIndex);*/ // removal intended
                    session.saveState.ReportConsumedItem(self.world, false, self.originRoom, self.placedObjectIndex, self.minCycles > 0 ? Random.Range(self.minCycles, self.maxCycles + 1) + props.RemainingOpenCycles * 100 : -1);
                }
            }
        }
        else if (self.type == AbstractObjectType.MiniFruitSpawner)
        {
            if (!self.isConsumed)
            {
                self.isConsumed = true;
                if (self.world.game.session is StoryGameSession sess)
                {
                    sess.saveState.ClearConsumedFruits(self.world, self.originRoom, self.placedObjectIndex);
                    sess.saveState.ReportConsumedItem(self.world, false, self.originRoom, self.placedObjectIndex, self.minCycles > 0 ? Random.Range(self.minCycles, self.maxCycles + 1) : -1);
                }
            }
        }
        else
            orig(self);
    }

    internal static bool On_AbstractCreature_AllowedToExistInRoom(On.AbstractCreature.orig_AllowedToExistInRoom orig, AbstractCreature self, Room room)
    {
        var res = orig(self, room);
        if (res && self.creatureTemplate.type == CreatureTemplateType.ChipChop && !room.readyForAI)
            return false;
        return res;
    }

    internal static int On_AbstractCreature_get_karmicPotential(Func<AbstractCreature, int> orig, AbstractCreature self)
    {
        if ((!ModManager.MSC || !MoreSlugcats.MoreSlugcats.cfgArtificerCorpseMaxKarma.Value) && self.realizedCreature is ScavengerSentinel)
            return Mathf.Clamp(1 + Math.Min(4, (int)Mathf.Floor(self.personality.bravery * 2f + self.personality.energy * 2f + self.personality.sympathy * 2f + self.personality.dominance * 2f)) - self.RemovedKarma, 0, 5);
        return orig(self);
    }

    internal static void IL_AbstractCreature_OpportunityToEnterDen(ILContext il)
    {
        var c = new ILCursor(il);
        var locs = il.Body.Variables;
        s_loc1 = 0;
        for (var i = 0; i < locs.Count; i++)
        {
            if (locs[i].VariableType.Name.Contains("Boolean"))
                s_loc1 = i;
        }
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_InLoc1))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AbstractCreature self) =>
             {
                 var tp = self.creatureTemplate.type;
                 var hv = tp == CreatureTemplateType.Hoverfly;
                 var chch = tp == CreatureTemplateType.ChipChop;
                 if (hv || chch || tp == CreatureTemplateType.TintedBeetle)
                 {
                     var obj = hv ? AbstractPhysicalObject.AbstractObjectType.DangleFruit : AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant;
                     var stuckObjs = self.stuckObjects;
                     for (var i = 0; i < stuckObjs.Count; i++)
                     {
                         if (stuckObjs[i] is AbstractPhysicalObject.CreatureGripStick st && st.A == self && (chch || st.B?.type == obj))
                             flag = true;
                     }
                 }
                 return flag;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook AbstractCreature.OpportunityToEnterDen!");
    }

    internal static void IL_AbstractCreatureAI_AbstractBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchStloc_OutLoc1))
        {
            var loc = il.Body.Variables[s_loc1];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, loc)
             .EmitDelegate((AbstractCreatureAI self, bool flag) =>
             {
                 if (self.parent.creatureTemplate.type == CreatureTemplateType.TintedBeetle && self.denPosition is WorldCoordinate w && self.destination != w)
                 {
                     var stuckObjs = self.parent.stuckObjects;
                     for (var i = 0; i < stuckObjs.Count; i++)
                     {
                         if (stuckObjs[i].B?.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
                         {
                             self.GoToDen();
                             flag = true;
                         }
                     }
                 }
                 return flag;
             });
            c.Emit(OpCodes.Stloc, loc);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook AbstractCreature.AbstractBehavior!");
    }

    internal static bool On_AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type) => type == AbstractObjectType.BouncingMelon || type == AbstractObjectType.ThornyStrawberry || type == AbstractObjectType.LittleBalloon || type == AbstractObjectType.Physalis || type == AbstractObjectType.LimeMushroom || type == AbstractObjectType.RubberBlossom || type == AbstractObjectType.GummyAnther || type == AbstractObjectType.MarineEye || type == AbstractObjectType.StarLemon || type == AbstractObjectType.DendriticNeuron || type == AbstractObjectType.MiniBlueFruit || type == AbstractObjectType.MiniFruitSpawner || type == AbstractObjectType.SporeProjectile || type == AbstractObjectType.FumeFruit || type == AbstractObjectType.Durian || type == AbstractObjectType.DarkGrub || orig(type);

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
        else if ((tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplateType.Xylo || tp == CreatureTemplate.Type.JetFish || tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Glowpillar || tp == CreatureTemplateType.FatFireFly || tp == CreatureTemplateType.XyloWorm || tp == CreatureTemplate.Type.TentaclePlant) && !AbsProps.TryGetValue(self, out _))
            AbsProps.Add(self, new());
        if (tp == CreatureTemplateType.Denture)
            self.remainInDenCounter = 0;
        else if (tp == CreatureTemplateType.ScavengerSentinel)
        {
            self.personality.dominance = Mathf.Lerp(self.personality.dominance, 1f, .5f);
            self.personality.bravery = Mathf.Lerp(self.personality.bravery, 1f, .25f);
        }
        else if (tp == CreatureTemplateType.AlphaOrange)
            self.personality.dominance = 1f;
    }

    internal static AbstractRoomNode.Type On_AbstractCreature_get_GetNodeType(Func<AbstractCreature, AbstractRoomNode.Type> orig, AbstractCreature self)
    {
        if (!self.pos.NodeDefined || self.pos.abstractNode >= self.Room.nodes.Length)
            return UnregisteredNodeType;
        return orig(self);
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
        if (!ModManager.MSC || rm.world.game.session is not ArenaGameSession sess || sess.arenaSitting.gameTypeSetup.gameType != DLCSharedEnums.GameTypeID.Challenge)
        {
            var list = new List<string>();
            if (rm.world.region is Region reg)
            {
                var prms = reg.regionParams;
                list.AddRange(prms.globalCreatureFlags_All);
                if (prms.globalCreatureFlags_Specific.TryGetValue(self.creatureTemplate.type, out var value))
                    list.AddRange(value);
            }
            if (self.spawnData is string s && s.Length > 1 && s[0] == '{')
                list.AddRange(s.Substring(1, s.Length - 2).Split(',', '|'));
            for (var i = 0; i < list.Count; i++)
            {
                var ari = list[i];
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
                        else if (string.Equals(nm, "bigworm", StringComparison.OrdinalIgnoreCase))
                        {
                            props2.IsBig = true;
                            props2.NormalLook = true;
                        }
                    }
                    else if (string.Equals(nm, "albinoform", StringComparison.OrdinalIgnoreCase) && AbsProps.TryGetValue(self, out var props4))
                        props4.Albino = true;
                    else if (string.Equals(nm, "rottenmode", StringComparison.OrdinalIgnoreCase) && AbsProps.TryGetValue(self, out var props5))
                        props5.RottenMode = true;
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
        if ((self.realizedCreature is MiniLeech l && l.fleeFromRain) || (self.realizedCreature is ChipChop c && c.DenMovement == 1))
            return true;
        return orig(self);
    }

    internal static void On_AbstractCreature_WatcherInitiateAI(On.AbstractCreature.orig_WatcherInitiateAI orig, AbstractCreature self)
    {
        if (!CreatureTemplateType.M4RCreatureList.Contains(self.creatureTemplate.type))
            orig(self);
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
            else if (type == AbstractObjectType.MiniBlueFruit)
                self.realizedObject = new MiniFruit(self);
            else if (type == AbstractObjectType.MiniFruitSpawner)
                self.realizedObject = new MiniFruitSpawner(self);
            else if (type == AbstractObjectType.FumeFruit)
                self.realizedObject = new FumeFruit(self);
            else if (type == AbstractObjectType.Durian)
                self.realizedObject = new Durian(self);
            else if (type == AbstractObjectType.DarkGrub)
                self.realizedObject = new DarkGrub(self);
        }
    }

    internal static void On_PlayerCarryableItem_NewRoom(On.PlayerCarryableItem.orig_NewRoom orig, PlayerCarryableItem self, Room newRoom)
    {
        orig(self, newRoom);
        if (self is VultureMask mask && mask.maskGfx is M4RScavMaskGraphics g)
            g.Reset();
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

    public static (bool Consumed, int WaitCycles) PlantConsumed(this SaveState self, World world, int originRoom, int placedObjectIndex)
    {
        if (world.singleRoomWorld || originRoom < 0 || placedObjectIndex < 0 || originRoom < world.firstRoomIndex || originRoom >= world.firstRoomIndex + world.NumberOfRooms || self.regionStates[world.region.regionNumber] is not RegionState st)
            return (false, 101);
        return st.PlantConsumed(originRoom, placedObjectIndex);
    }

    public static void ReportConsumedFruit(this SaveState self, World world, int originRoom, int placedObjectFruitIndex)
    {
        if (!world.singleRoomWorld && originRoom >= 0 && placedObjectFruitIndex <= -10 && originRoom >= world.firstRoomIndex && originRoom < world.firstRoomIndex + world.NumberOfRooms && self.regionStates[world.region.regionNumber] is RegionState st)
            st.consumedItems.Add(new(originRoom, placedObjectFruitIndex, -1));
    }

    public static int ConsumedFruits(this SaveState self, World world, int originRoom, int placedObjectOwnerIndex)
    {
        if (world.singleRoomWorld || originRoom < 0 || placedObjectOwnerIndex < 0 || originRoom < world.firstRoomIndex || originRoom >= world.firstRoomIndex + world.NumberOfRooms || self.regionStates[world.region.regionNumber] is not RegionState st)
            return 0;
        var cons = st.consumedItems;
        var index = -10 - placedObjectOwnerIndex;
        var res = 0;
        for (var num = cons.Count - 1; num >= 0; num--)
        {
            var item = cons[num];
            if (item.originRoom == originRoom && item.placedObjectIndex == index)
                ++res;
        }
        return res;
    }

    public static void ClearConsumedFruits(this SaveState self, World world, int originRoom, int placedObjectOwnerIndex)
    {
        if (world.singleRoomWorld || originRoom < 0 || placedObjectOwnerIndex < 0 || originRoom < world.firstRoomIndex || originRoom >= world.firstRoomIndex + world.NumberOfRooms || self.regionStates[world.region.regionNumber] is not RegionState st)
            return;
        var index = -10 - placedObjectOwnerIndex;
        var cons = st.consumedItems;
        for (var num = cons.Count - 1; num >= 0; num--)
        {
            var item = cons[num];
            if (item.originRoom == originRoom && item.placedObjectIndex == index)
                cons.RemoveAt(num);
        }
    }

    public static bool Albino(this AbstractCreature self) => AbsProps.TryGetValue(self, out var props) && props.Albino;

    public static bool RottenMode(this AbstractCreature self) => AbsProps.TryGetValue(self, out var props) && props.RottenMode;

    public static bool SameRippleLayer(this AbstractPhysicalObject self, AbstractPhysicalObject other) => self.IsSameRippleLayer(other.rippleLayer) || other.rippleBothSides;

    public static bool NoCamo(this AbstractPhysicalObject self) => self.realizedObject is not PhysicalObject robj || robj.NoCamo();

    public static bool NoCamo(this PhysicalObject self) => (self is not Player p || !p.isCamo) &&
        (self is not Lizard l || !l.Camouflaged()) &&
        (self is not Hazer h || !h.Camouflaged()) &&
        (self is not PoleMimic po || !po.Camouflaged()) &&
        (self is not Denture d || !d.Camouflaged);
}