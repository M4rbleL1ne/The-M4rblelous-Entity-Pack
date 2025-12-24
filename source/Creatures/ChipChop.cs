using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;

namespace LBMergedMods.Creatures;

public class ChipChop : InsectoidCreature
{
	[StructLayout(LayoutKind.Sequential)]
	public struct IndividualVariations(float size, float hue, int eyeVar, float buttAlpha, float eyeAlpha)
    {
		public float Size = size, Hue = hue, ButtAlpha = buttAlpha, EyeAlpha = eyeAlpha;
		public int EyeVar = eyeVar;
    }

    public BodyChunk? AttachedChunk;
	public PhysicalObject? Prey;
    public List<MovementConnection> Path, ScratchPath;
    public MovementConnection LastFollowingConnection, FollowingConnection, LastShortCut;
    public Vector2 DragPos;
    public int OutsideAccessibleCounter, PathCount, ScratchPathCount, IdleCounter, DenMovement, SeenNoObjectCounter,
        GrabCounter, MarineEyeEffectDuration, BouncingMelonEffectDuration, MushroomEffectDuration, SpeedEffectDuration,
        HiccupsEffectDuration, Hunger = 15;
    public IndividualVariations IVars;
	public float DeathSpasms = 1f, ConnectDistance, Excitement;
    public WorldCoordinate? DenPos;
    public Vector2? MoveAwayFromPos;
	public bool Idle, InAccessibleTerrain, Glowing, Injured;

    public virtual new HealthState State => (abstractCreature.state as HealthState)!;

	public virtual float Hue => IVars.Hue + MarineEyeEffectDuration / 70000f;

    public virtual float Saturation => 1f - MushroomEffectDuration / 1600f;

    public virtual float Lightness => .5f - .2f * injectedPoison;

    public override Vector2 DangerPos => graphicsModule is ChipChopGraphics g ? g.BodyDir * ConnectDistance + firstChunk.pos : firstChunk.pos;

    public ChipChop(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
	{
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        IVars = new(Random.Range(1.1f, 1.3f), Random.Range(36f / 360f, 86f / 360f), Random.Range(1, 5), .5f + Random.value * .35f, .8f + Random.value * .2f);
        Random.state = state;
        ConnectDistance = 12f * IVars.Size;
        bodyChunks = [new(this, 0, default, IVars.Size * 3f, IVars.Size / 3f) { goThroughFloors = true }];
		bodyChunkConnections = [];
		airFriction = .99f;
		gravity = .8f;
		bounce = 0f;
		surfaceFriction = .87f;
		collisionLayer = 1;
		waterFriction = .92f;
		buoyancy = .95f;
        Path = [];
		PathCount = 0;
		ScratchPath = [];
		ScratchPathCount = 0;
		if (abstractCreature.pos.NodeDefined && world.GetNode(abstractCreature.pos).type == AbstractRoomNode.Type.Den)
			DenPos = abstractCreature.pos.WashTileData();
		if (world.rainCycle.CycleStartUp < .5f)
			DenMovement = -1;
		else if (world.rainCycle.TimeUntilRain < (world.game.IsStorySession ? 2400 : 800) || Hunger <= 0 || Injured)
			DenMovement = 1;
	}

	public override void InitiateGraphicsModule() => graphicsModule ??= new ChipChopGraphics(this);

	public override Color ShortCutColor() => Custom.HSL2RGB(Hue * 2f + 20f / 360f, Saturation, Lightness);

    public override void Update(bool eu)
	{
        bounce = BouncingMelonEffectDuration / 4750f;
        base.Update(eu);
		if (!inShortcut)
		{
            if (BouncingMelonEffectDuration > 0)
                --BouncingMelonEffectDuration;
            if (MarineEyeEffectDuration > 0)
                --MarineEyeEffectDuration;
            if (MushroomEffectDuration > 0)
                --MushroomEffectDuration;
            if (SpeedEffectDuration > 0)
                --SpeedEffectDuration;
            if (HiccupsEffectDuration > 0)
                --HiccupsEffectDuration;
        }
        if (safariControlled)
        {
            MoveAwayFromPos = null;
            Prey = null;
            DenMovement = 0;
            Hunger = 15;
            Injured = false;
        }
        AttachedChunk = null;
		var fc = firstChunk;
		var ps = fc.pos;
        DragPos = ps + Custom.DirVec(ps, DragPos) * ConnectDistance;
		if (OutsideAccessibleCounter > 0)
            --OutsideAccessibleCounter;
		if (room is not Room rm)
			return;
		if (dead)
			DeathSpasms = Mathf.Max(0f, DeathSpasms - 1f / Mathf.Lerp(150f, 300f, Random.value));
        if (Consious)
        {
            if (safariControlled && inputWithDiagonals?.thrw is true)
                ReleaseGrasp(0);
            if (Prey is not null && (Prey.slatedForDeletetion || DenMovement != 0))
                Prey = null;
            if (grasps[0] is null)
            {
                var ps2 = DangerPos;
                var objs = rm.physicalObjects;
                for (var i = 0; i < objs.Length; i++)
                {
                    var list = objs[i];
                    for (var j = 0; j < list.Count; j++)
                    {
                        if (list[j] is PhysicalObject obj && obj.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject) && grasps[0] is null && CanEat(obj) && Custom.DistLess(ps2, obj.firstChunk.pos, Math.Max(obj.firstChunk.rad * 2.1f, 10f)) && (!safariControlled || inputWithDiagonals?.pckp is true))
                        {
                            if (Grab(obj, 0, 0, Grasp.Shareability.CanNotShare, 1000f + abstractCreature.personality.dominance, true, false))
                                rm.PlaySound(NewSoundID.M4R_ChipChop_Chip, firstChunk, false, 1.4f, 1f);
                            break;
                        }
                    }
                }
            }
        }
        else
            Excitement = 0f;
        var tilePosition = Room.StaticGetTilePosition(ps);
		tilePosition.x = Custom.IntClamp(tilePosition.x, 0, rm.TileWidth - 1);
		tilePosition.y = Custom.IntClamp(tilePosition.y, 0, rm.TileHeight - 1);
		var flag = rm.aimap.TileAccessibleToCreature(tilePosition, Template);
		if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
		{
            fc.vel += Custom.DirVec(ps, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		var hlt = State.health;
        Injured = hlt < 1f;
        if (!dead && Injured)
        {
            if (hlt < 0f && Random.value < -hlt && Random.value < .025f)
                Die();
            if (Random.value * .7f > hlt && Random.value < .125f)
                Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * hlt), 0, 10))));
        }
		if (!Consious)
			return;
        LookForObjects(rm);
		if (!safariControlled && (FollowingConnection == default || FollowingConnection.DestTile != tilePosition) && rm.GetTile(ps).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			fc.vel += Custom.IntVector2ToVector2(rm.ShorcutEntranceHoleDirection(tilePosition)) * 8f;
		else if (!rm.IsPositionInsideBoundries(tilePosition) && flag)
		{
			OutsideAccessibleCounter = 5;
			FollowingConnection = new(MovementConnection.MovementType.Standard, abstractPhysicalObject.pos, rm.GetWorldCoordinate(tilePosition), 1);
		}
		if (!safariControlled && (rm.world.rainCycle.TimeUntilRain < (rm.game.IsStorySession ? 2400 : 800) || Hunger <= 0 || Injured))
			DenMovement = 1;
        var fourDirs = Custom.fourDirections;
        if (DenMovement != 0)
		{
            if (DenMovement == -1 && Random.value < 1f / Mathf.Lerp(1200f, 400f, rm.world.rainCycle.CycleStartUp))
            {
                DenMovement = 0;
                if (abstractCreature.InDen)
                {
                    abstractCreature.remainInDenCounter = 0;
                    rm.abstractRoom.MoveEntityOutOfDen(abstractCreature);
                }
            }
            if (DenPos is WorldCoordinate w)
			{
                if (DenMovement == 1)
                {
                    for (var i = 0; i < fourDirs.Length; i++)
                    {
                        var tl = abstractPhysicalObject.pos.Tile + fourDirs[i];
                        if (rm.GetTile(tl).Terrain == Room.Tile.TerrainType.ShortcutEntrance && rm.shortcutData(tl).destNode == w.abstractNode)
                        {
                            enteringShortCut = tl;
                            break;
                        }
                    }
                }
                if (w.room != rm.abstractRoom.index)
                    DenPos = null;
            }
			else
				DenMovement = 0;
		}
		var nds = rm.abstractRoom.nodes;
        if (!DenPos.HasValue && nds.Length > 0)
		{
			var num = Random.Range(0, nds.Length);
			if (nds[num].type == AbstractRoomNode.Type.Den && rm.aimap.CreatureSpecificAImap(Template).GetDistanceToExit(abstractPhysicalObject.pos.x, abstractPhysicalObject.pos.y, rm.abstractRoom.CommonToCreatureSpecificNodeIndex(num, Template)) > -1)
				DenPos = new(rm.abstractRoom.index, -1, -1, num);
		}
		if (MoveAwayFromPos is Vector2 vec && (Random.value < .0125f || !Custom.DistLess(ps, vec, 150f)))
			MoveAwayFromPos = null;
		InAccessibleTerrain = (FollowingConnection == default || FollowingConnection.type != MovementConnection.MovementType.DropToFloor) && (OutsideAccessibleCounter > 0 || flag);
		if (FollowingConnection == default && !flag)
		{
            for (var k = 0; k < fourDirs.Length; k++)
            {
				var dr = fourDirs[k];
                if (rm.aimap.TileAccessibleToCreature(tilePosition + dr, Template))
				{
					fc.vel += dr.ToVector2() * .5f;
					break;
				}
			}
		}
		if (grasps[0] is Grasp g0)
            CarryObject(rm, g0);
		if (Consious && InAccessibleTerrain)
		{
			fc.vel *= .7f;
			fc.vel.y += gravity;
			Crawl(rm);
		}
		else
		{
			FollowingConnection = default;
			if (PathCount > 0)
				PathCount = 0;
		}
        if (BouncingMelonEffectDuration > 0 && fc.ContactPoint != default)
            fc.vel *= .9f + .1f * (1f - BouncingMelonEffectDuration / 5000f);
        if (HiccupsEffectDuration > 0)
        {
            if (Random.value <= .025f)
            {
                rm.AddObject(new ShockWave(fc.pos, 60f, 5f, 5));
                rm.PlaySound(SoundID.Snail_Pop, fc, false, .8f, .75f + Random.value * .5f);
                var crits = rm.abstractRoom.creatures;
                for (var i = 0; i < crits.Count; i++)
                {
                    if (crits[i]?.realizedCreature is Creature c && c != this && Custom.DistLess(c.mainBodyChunk.pos, fc.pos, 60f + .5f * (c.mainBodyChunk.rad + fc.rad)))
                        c.Stun(40);
                }
                Stun(20);
            }
        }
    }

    public virtual void Crawl(Room rm)
    {
        if (safariControlled)
        {
            FollowingConnection = default;
            if (inputWithDiagonals is Player.InputPackage pk)
            {
                var type = MovementConnection.MovementType.Standard;
                var tl = rm.GetTile(firstChunk.pos);
                if (tl.Terrain == Room.Tile.TerrainType.ShortcutEntrance && shortcutDelay <= 0)
                {
                    enteringShortCut = new(tl.X, tl.Y);
                    type = MovementConnection.MovementType.ShortCut;
                }
                if (pk.AnyDirectionalInput)
                    FollowingConnection = new(type, room.GetWorldCoordinate(firstChunk.pos), room.GetWorldCoordinate(firstChunk.pos + new Vector2(pk.x, pk.y) * 40f), 2);
                if (FollowingConnection != default)
                {
                    Move(FollowingConnection);
                    if (FollowingConnection.type == MovementConnection.MovementType.ShortCut)
                    {
                        var flag = false;
                        var list2 = new List<IntVector2>();
                        var shortcuts = rm.shortcuts;
                        for (var i = 0; i < shortcuts.Length; i++)
                        {
                            ref readonly var shortcutData = ref shortcuts[i];
                            if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation)
                            {
                                if (shortcutData.StartTile != FollowingConnection.StartTile)
                                    list2.Add(shortcutData.StartTile);
                                else
                                    flag = true;
                            }
                        }
                        if (flag)
                        {
                            if (list2.Count > 0)
                            {
                                list2.Shuffle();
                                NPCTransportationDestination = rm.GetWorldCoordinate(list2[0]);
                            }
                            else
                                NPCTransportationDestination = FollowingConnection.destinationCoord;
                        }
                    }
                }
            }
            return;
        }
        if (DenMovement == 0 && !MoveAwayFromPos.HasValue && Prey is null)
		{
            ++IdleCounter;
			if (!Idle && IdleCounter > 10)
				Idle = true;
		}
		else if (Random.value <= .15f && (DenMovement != 0 || MoveAwayFromPos.HasValue || Prey is not null))
		{
			IdleCounter = 0;
			Idle = false;
		}
		if (Idle)
		{
			if (FollowingConnection != default)
			{
				Move(FollowingConnection);
				if (Room.StaticGetTilePosition(firstChunk.pos) == FollowingConnection.DestTile)
					FollowingConnection = default;
			}
			else
			{
				if (Random.value >= 1f / 12f)
					return;
				var aItile = rm.aimap.getAItile(firstChunk.pos);
				if (aItile.outgoingPaths.Count > 0)
				{
					var connection = aItile.outgoingPaths[Random.Range(0, aItile.outgoingPaths.Count)];
					if (connection.type != MovementConnection.MovementType.DropToFloor && rm.aimap.IsConnectionAllowedForCreature(connection, Template) && rm.LightSourceExposure(rm.MiddleOfTile(connection.DestTile)) == 0f)
						FollowingConnection = connection;
				}
			}
			return;
		}
		if (!safariControlled && (DenMovement != 0 || MoveAwayFromPos.HasValue || Prey is not null))
		{
			ScratchPathCount = CreateRandomPath(ScratchPath, rm);
			if (ScoreOfPath(ScratchPath, ScratchPathCount, rm) > ScoreOfPath(Path, PathCount, rm))
			{
				var list = Path;
				var num = PathCount;
				Path = ScratchPath;
				PathCount = ScratchPathCount;
				ScratchPath = list;
				ScratchPathCount = num;
			}
		}
		if (FollowingConnection != default && FollowingConnection.type != 0)
		{
			if (LastFollowingConnection != FollowingConnection)
				OutsideAccessibleCounter = 20;
			if (FollowingConnection != default)
				LastFollowingConnection = FollowingConnection;
			Move(FollowingConnection);
			if (Room.StaticGetTilePosition(firstChunk.pos) != FollowingConnection.DestTile)
				return;
		}
		else if (FollowingConnection != default)
			LastFollowingConnection = FollowingConnection;
		if (PathCount > 0)
		{
			FollowingConnection = default;
			for (var num2 = PathCount - 1; num2 >= 0; num2--)
			{
				var pth = Path[num2];
                if (abstractPhysicalObject.pos.Tile == pth.StartTile)
				{
					FollowingConnection = pth;
					break;
				}
			}
			if (FollowingConnection == default)
				PathCount = 0;
		}
		if (FollowingConnection == default)
			return;
		if (FollowingConnection.type is MovementConnection.MovementType.ShortCut or MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = FollowingConnection.StartTile;
			if (FollowingConnection.type == MovementConnection.MovementType.NPCTransportation)
				NPCTransportationDestination = FollowingConnection.destinationCoord;
			LastShortCut = FollowingConnection;
			FollowingConnection = default;
		}
		else if (FollowingConnection.type is MovementConnection.MovementType.Standard or MovementConnection.MovementType.DropToFloor)
			Move(FollowingConnection);
	}

	public virtual bool CanEat(PhysicalObject item) => Hunger > 0 && !item.slatedForDeletetion && item.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject) && item switch
	{
        GooieDuck gd => !gd.StalkActive() && gd.bites < 6 && gd.Edible,
        BouncingMelon bm => !bm.StalkActive,
        IHaveAStalkState st => !st.StalkActive && st is IPlayerEdible ed && ed.Edible,
        XyloWorm x => x.Edible && x.dead,
        DendriticNeuron or OracleSwarmer or JellyFish or SwollenWaterNut or EggBugEgg or FireEgg or BlobPiece => (item as IPlayerEdible)!.Edible,
        Creature cr => cr.dead && cr is IPlayerEdible ed && ed.Edible,
        BoxWorm.Larva box => box.Edible,
        SlimeMold mld => !mld.StalkActive() && mld.Edible,
        Mushroom mush => !mush.StalkActive() && mush.Edible,
        KarmaFlower flower => !flower.StalkActive() && flower.Edible,
        LillyPuck ll => !ll.StalkActive() && ll.Edible,
        GlowWeed gw => !gw.StalkActive() && gw.Edible,
        DangleFruit dangle => !dangle.StalkActive() && dangle.Edible,
        DandelionPeach dd => !dd.StalkActive() && dd.Edible,
        _ => false
	};

    public virtual void BiteItem(PhysicalObject item, Grasp g0)
	{
        if (item is Physalis or StarLemon or GummyAnther or MiniFruit or LittleBalloon or LimeMushroom or ThornyStrawberry or DendriticNeuron or BouncingMelon or MarineEye or DarkGrub)
        {
            (item.abstractPhysicalObject as AbstractConsumable)!.Consume();
            (item as IPlayerEdible)!.BitByPlayer(g0, evenUpdate);
            item.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            --Hunger;
        }
        else if (item?.abstractPhysicalObject.type?.value is string s && s.StartsWith("OY"))
        {
            (item.abstractPhysicalObject as AbstractConsumable)!.Consume();
            (item as IPlayerEdible)!.BitByPlayer(g0, evenUpdate);
            item.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            --Hunger;
        }
        else if (item is BlobPiece blb)
        {
            blb.BitByPlayer(g0, evenUpdate);
            blb.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            --Hunger;
        }
        else if (item is XyloWorm xy)
        {
            xy.BitByPlayer(g0, evenUpdate);
            xy.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            --Hunger;
        }
        else if (item is DangleFruit d)
        {
            --d.bites;
            d.AbstrConsumable.Consume();
            room.PlaySound(d.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, d.firstChunk);
            d.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (d.bites < 1)
            {
                if (d.AbstrConsumable.rotted)
                {
                    var value = Random.value;
                    if (value < .3f)
                        Stun(120);
                    else if (value < .65f)
                        Stun(60);
                }
                g0.Release();
                d.Destroy();
            }
            --Hunger;
        }
        else if (item is GooieDuck gd)
        {
            --gd.bites;
            gd.AbstrConsumable.Consume();
            room.PlaySound(gd.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, gd.firstChunk);
            gd.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (gd.bites < 1)
            {
                g0.Release();
                gd.Destroy();
            }
            --Hunger;
        }
        else if (item is GlowWeed wd)
        {
            --wd.bites;
            wd.AbstrConsumable.Consume();
            room.PlaySound(wd.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, wd.firstChunk);
            wd.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (wd.bites < 1)
            {
                g0.Release();
                wd.Destroy();
            }
            --Hunger;
        }
        else if (item is SlimeMold mld)
        {
            --mld.bites;
            mld.AbstrConsumable.Consume();
            room.PlaySound(mld.bites == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, mld.firstChunk);
            mld.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (mld.bites < 1)
            {
                g0.Release();
                mld.Destroy();
            }
            --Hunger;
        }
        else if (item is Hazer h)
        {
            --h.bites;
            room.PlaySound(SoundID.Slugcat_Eat_Centipede, h.firstChunk);
            h.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (h.bites < 1)
            {
                g0.Release();
                h.Destroy();
            }
            --Hunger;
        }
        else if (item is VultureGrub gb)
        {
            --gb.bites;
            room.PlaySound(SoundID.Slugcat_Eat_Centipede, gb.firstChunk);
            gb.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (gb.bites < 1)
            {
                g0.Release();
                gb.Destroy();
            }
            --Hunger;
        }
        else if (item is JellyFish je)
        {
            var ps = je.firstChunk.pos;
            --je.bites;
            je.AbstrConsumable.Consume();
            room.PlaySound(je.bites == 0 ? SoundID.Slugcat_Eat_Jelly_Fish : SoundID.Slugcat_Bite_Jelly_Fish, je.firstChunk);
            je.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            var tts = je.tentacles;
            for (var i = 0; i < tts.Length; i++)
            {
                var tt = tts[i];
                var l = tt.GetLength(0);
                for (var j = 0; j < l; j++)
                    tt[j, 0] = Vector2.Lerp(tt[j, 0], ps, .2f);
            }
            if (je.bites < 1)
            {
                g0.Release();
                je.Destroy();
            }
            --Hunger;
        }
        else if (item is KarmaFlower k)
        {
            --k.bites;
            k.AbstrConsumable.Consume();
            room.PlaySound(k.bites == 0 ? SoundID.Slugcat_Eat_Karma_Flower : SoundID.Slugcat_Bite_Karma_Flower, k.firstChunk);
            k.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (k.bites < 1)
            {
                g0.Release();
                k.Destroy();
            }
            --Hunger;
            Die();
        }
        else if (item is Mushroom m)
        {
            m.AbstrConsumable.Consume();
            m.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            MushroomEffectDuration = 1280;
            g0.Release();
            m.Destroy();
            --Hunger;
        }
        else if (item is SwollenWaterNut nut)
        {
            --nut.bites;
            nut.AbstrConsumable.Consume();
            room.PlaySound(nut.bites == 0 ? SoundID.Slugcat_Eat_Water_Nut : SoundID.Slugcat_Bite_Water_Nut, nut.firstChunk);
            nut.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (nut.bites < 1)
            {
                g0.Release();
                nut.Destroy();
            }
            nut.propSpeed += Mathf.Lerp(-1f, 1f, Random.value) * 7f;
            nut.firstChunk.rad = Mathf.InverseLerp(3f, 0f, nut.bites) * 9.5f;
            --Hunger;
        }
        else if (item is EggBugEgg egg)
        {
            --egg.bites;
            room.PlaySound(egg.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, egg.firstChunk);
            egg.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            egg.liquid = 1f;
            if (egg.bites < 1)
            {
                g0.Release();
                egg.Destroy();
            }
            --Hunger;
        }
        else if (item is FireEgg fgg)
        {
            --fgg.bites;
            room.PlaySound(fgg.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, fgg.firstChunk);
            fgg.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            fgg.liquid = 1f;
            if (fgg.bites < 1)
            {
                g0.Release();
                fgg.Destroy();
            }
            --Hunger;
        }
        else if (item is Fly f)
        {
            --f.bites;
            if (!f.dead)
                f.Die();
            room.PlaySound(f.bites == 0 ? SoundID.Slugcat_Final_Bite_Fly : SoundID.Slugcat_Bite_Fly, f.mainBodyChunk);
            f.mainBodyChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (f.bites < 1 && f.eaten == 0)
            {
                g0.Release();
                f.eaten = 3;
            }
            --Hunger;
        }
        else if (item is SmallNeedleWorm worm)
        {
            worm.Scream();
            worm.Die();
            var bs = worm.bodyChunks;
            var ps = DangerPos;
            for (var i = 0; i < bs.Length; i++)
                bs[i].MoveFromOutsideMyUpdate(evenUpdate, ps);
            --worm.bites;
            if (worm.bites < 1)
            {
                g0.Release();
                worm.Destroy();
            }
            --Hunger;
        }
        else if (item is OracleSwarmer sw)
        {
            --sw.bites;
            room.PlaySound(sw.bites == 0 ? SoundID.Slugcat_Eat_Swarmer : SoundID.Slugcat_Bite_Swarmer, sw.firstChunk);
            sw.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (sw.bites < 1)
            {
                Glowing = true;
                g0.Release();
                sw.Destroy();
            }
            --Hunger;
        }
        else if (item is LillyPuck lp)
        {
            --lp.AbstrLillyPuck.bites;
            lp.AbstrLillyPuck.Consume();
            room.PlaySound(lp.AbstrLillyPuck.bites != 0 ? SoundID.Slugcat_Bite_Dangle_Fruit : SoundID.Slugcat_Eat_Dangle_Fruit, lp.firstChunk);
            lp.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (lp.AbstrLillyPuck.bites < 1)
            {
                g0.Release();
                lp.Destroy();
            }
            --Hunger;
        }
        else if (item is DandelionPeach pc)
        {
            --pc.bites;
            pc.AbstrConsumable.Consume();
            room.PlaySound(pc.bites != 0 ? SoundID.Slugcat_Bite_Water_Nut : SoundID.Slugcat_Eat_Water_Nut, pc.firstChunk);
            pc.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (pc.bites < 1)
            {
                g0.Release();
                pc.Destroy();
            }
            pc.firstChunk.rad = Mathf.InverseLerp(3f, 0f, pc.bites) * 9.5f;
            --Hunger;
        }
        else if (item is Centipede ce)
        {
            --ce.bites;
            if (!ce.dead)
                ce.Die();
            room.PlaySound(ce.bites == 0 ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, ce.mainBodyChunk);
            ce.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (ce.bites < 1)
            {
                g0.Release();
                ce.Destroy();
            }
            --Hunger;
        }
        else if (item is Rat rat)
        {
            --rat.bites;
            if (!rat.dead)
                rat.Die();
            room.PlaySound(rat.bites == 0 ? SoundID.Slugcat_Final_Bite_Fly : SoundID.Slugcat_Bite_Fly, rat.mainBodyChunk);
            rat.mainBodyChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (rat.bites < 1 && rat.eaten == 0)
            {
                g0.Release();
                rat.eaten = 3;
            }
        }
        else if (item is BoxWorm.Larva larva)
        {
            --larva.bites;
            if (ModManager.Watcher)
                room.PlaySound(larva.bites == 0 ? WatcherEnums.WatcherSoundID.Slugcat_Eat_Box_Worm_Larva : WatcherEnums.WatcherSoundID.Slugcat_Bite_Box_Worm_Larva, firstChunk);
            larva.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (larva.bites < 1)
            {
                g0.Release();
                larva.Destroy();
            }
        }
        else if (item is Tardigrade td)
        {
            var vector = Custom.RGB2HSL(td.BitesLeft == 3 ? td.iVars.secondaryColor : td.iVars.bodyColor);
            room.AddObject(new PoisonInjecter(this, .22f, (10f + Random.value * 8f) * (td.BitesLeft == 3 ? 1f : 4.4f), new HSLColor(vector.x, Mathf.Lerp(vector.y, 1f, .5f), .5f).rgb));
            --(td.State as Tardigrade.TardigradeState)!.bites;
            room.PlaySound(td.BitesLeft == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, td.firstChunk);
            td.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (td.BitesLeft <= 1 && !td.dead)
                td.Die();
            if (td.BitesLeft < 1)
            {
                g0.Release();
                td.Destroy();
            }
        }
        else if (item is SandGrub gr)
        {
            gr.bodyChunks[3 - gr.BitesLeft].rad *= .5f;
            bodyChunkConnections[3].active = false;
            bodyChunkConnections[3 - gr.BitesLeft].distance *= .25f;
            --gr.BitesLeft;
            room.PlaySound(SoundID.Slugcat_Eat_Centipede, gr.firstChunk);
            var chs = gr.bodyChunks;
            for (var i = 0; i < chs.Length; i++)
                chs[i].MoveFromOutsideMyUpdate(evenUpdate, Vector2.Lerp(gr.firstChunk.pos, DangerPos, .8f - .15f * i));
            if (gr.BitesLeft < 1)
            {
                g0.Release();
                gr.abstractPhysicalObject.destroyOnAbstraction = true;
                gr.abstractCreature.saveCreature = false;
            }
        }
        else if (item is Barnacle b)
        {
            --b.bites;
            b.Die();
            b.firstChunk.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
            if (b.bites < 1)
            {
                g0.Release();
                b.Destroy();
            }
        }
    }

	public virtual void CarryObject(Room rm, Grasp g0)
	{
        if (!CanEat(g0.grabbed))
        {
			g0.Release();
            GrabCounter = 0;
            return;
        }
        if ((!safariControlled || inputWithDiagonals?.pckp is true) && GrabCounter < 60)
            ++GrabCounter;
        else
        {
            if (GrabCounter >= 60)
			    BiteItem(g0.grabbed, g0);
            GrabCounter = 0;
        }
        var dp = DangerPos;
        var gbc = AttachedChunk = g0.grabbedChunk;
        var num = Vector2.Distance(dp, gbc.pos);
        if (num > 50f)
        {
            g0.Release();
            GrabCounter = 0;
            return;
        }
        var fc = firstChunk;
        var vector = Custom.DirVec(dp, gbc.pos);
        float num2 = fc.rad + gbc.rad,
			num3 = .95f,
			num4 = 0f;
        if (g0.grabbed.TotalMass > TotalMass / 2f)
            num4 = gbc.mass / (gbc.mass + fc.mass) * .5f;
		var vec = vector * (num4 * num3 * (num2 - num));
        fc.pos -= vec;
        fc.vel -= vec;
		vec = vector * ((1f - num4) * num3 * (num2 - num));
        gbc.pos += vec;
        gbc.vel += vec;
    }

	public override void LoseAllGrasps() => ReleaseGrasp(0);

    public override void ReleaseGrasp(int grasp)
    {
        base.ReleaseGrasp(grasp);
		GrabCounter = 0;
    }

    public virtual void Move(MovementConnection con)
	{
		var dest = room.MiddleOfTile(con.DestTile);
		firstChunk.vel += (Custom.DirVec(firstChunk.pos, dest) + Custom.DegToVec(Random.value * 360f) * (.75f * Mathf.Lerp(.8f, 1.2f, IVars.Size / 1.5f))) * (1f + Excitement * .5f + SpeedEffectDuration / 2400f);
	}

    public virtual bool VisualContact(Vector2 pos)
	{
		if (!Custom.DistLess(firstChunk.pos, pos, Template.visualRadius))
			return false;
		return room.VisualContact(firstChunk.pos, pos);
	}

    public virtual void LookForObjects(Room room)
    {
        ++SeenNoObjectCounter;
        var items = room.abstractRoom.entities;
        if (safariControlled || items.Count == 0)
        {
            Excitement = Math.Min(Excitement + MushroomEffectDuration / 1280f + (safariControlled ? .5f : 0f), 1f);
            if (safariControlled)
            {
                MoveAwayFromPos = null;
                Prey = null;
                DenMovement = 0;
                Hunger = 15;
                Injured = false;
            }
            return;
        }
        var abstractItem = items[Random.Range(0, items.Count)];
        if (abstractItem is AbstractPhysicalObject aobj && aobj.SameRippleLayer(abstractPhysicalObject) && aobj.realizedObject is PhysicalObject obj && !abstractItem.slatedForDeletion)
		{
            if (DenMovement == 0 && Prey is null && grasps[0] is null && CanEat(obj) && VisualContact(obj.firstChunk.pos))
            {
                Excitement = Mathf.Clamp(Excitement + 1f / 10f, 0f, 1f);
                Prey = obj;
                SeenNoObjectCounter = 0;
            }
            else if (obj is Creature cr)
			{
				if (ConsiderDanger(cr) && VisualContact(cr.mainBodyChunk.pos))
				{
                    Excitement = Mathf.Clamp(Excitement + Template.CreatureRelationship(cr).intensity / 10f, 0f, 1f);
                    SeenNoObjectCounter = 0;
                }
                if (Template.CreatureRelationship(cr.Template).type != CreatureTemplate.Relationship.Type.Eats && Custom.DistLess(firstChunk.pos, cr.DangerPos, 30f + cr.TotalMass * 8f))
                    MoveAwayFromPos = cr.DangerPos;
            }
			else
            {
                if (SeenNoObjectCounter > 100 + items.Count)
                    Excitement = Mathf.Clamp(Excitement - 1f / 60f, 0f, 1f);
                else
                    Excitement = Mathf.Clamp(Excitement - 1f / (60f * items.Count), 0f, 1f);
            }
        }
        Excitement = Math.Min(Excitement + MushroomEffectDuration / 1280f, 1f);
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (!RippleViolenceCheck(source))
            return;
        var bounceEffect = BouncingMelonEffectDuration / 18000f;
        damage = Math.Max(0f, damage - bounceEffect);
        stunBonus = Math.Max(0f, stunBonus - bounceEffect);
        room?.PlaySound(NewSoundID.M4R_ChipChop_Chip, firstChunk, false, 1.5f, 1f);
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public virtual float ScoreOfPath(List<MovementConnection> testPath, int testPathCount, Room room)
	{
		if (testPathCount == 0)
			return float.MinValue;
		var num = TileScore(testPath[testPathCount - 1].DestTile, room);
		for (var i = 0; i < PathCount; i++)
		{
			if (Path[i] == LastFollowingConnection)
				num -= 1000f;
		}
		return num;
	}

	public virtual float TileScore(IntVector2 tile, Room room)
	{
		var num = 0f;
		if (MoveAwayFromPos is Vector2 ps)
			num += Vector2.Distance(room.MiddleOfTile(tile), ps);
        if (Prey is PhysicalObject obj)
            num -= Vector2.Distance(room.MiddleOfTile(tile), obj.firstChunk.pos);
        if (DenMovement > 0 && DenPos is WorldCoordinate w && w.room == room.abstractRoom.index)
		{
			var distanceToExit = room.aimap.CreatureSpecificAImap(Template).GetDistanceToExit(tile.x, tile.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(w.abstractNode, Template));
			num = distanceToExit != -1 ? (num - distanceToExit * DenMovement * 10f) : (num - 100f);
		}
		var dirs = Custom.fourDirectionsAndZero;
        for (var i = 0; i < dirs.Length; i++)
        {
            var t = tile + dirs[i];
            if (room.GetTile(t).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(t).shortCutType == ShortcutData.Type.RoomExit)
                return float.MinValue;
        }
		var tl = room.aimap.getAItile(tile);
        num += tl.visibility / 800f;
        if (tl.narrowSpace)
            num -= .01f;
        num -= room.aimap.getTerrainProximity(tile) * .01f;
        if (LastShortCut != default)
        {
            num -= 10f / LastShortCut.StartTile.FloatDist(tile);
            num -= 10f / LastShortCut.DestTile.FloatDist(tile);
        }
        return num;
	}

    public virtual int CreateRandomPath(List<MovementConnection> pth, Room room)
	{
		var worldCoord = abstractCreature.pos;
		if (!room.aimap.TileAccessibleToCreature(worldCoord.Tile, Template))
		{
			var dirs = Custom.fourDirections;
            for (var i = 0; i < dirs.Length; i++)
			{
				var wp = worldCoord.Tile + dirs[i];
                if (room.aimap.TileAccessibleToCreature(wp, Template) && room.GetTile(wp).Terrain != Room.Tile.TerrainType.Slope)
				{
					worldCoord.Tile += dirs[i];
					break;
				}
			}
		}
		if (!room.aimap.TileAccessibleToCreature(worldCoord.Tile, Template))
			return 0;
		var worldCoord2 = abstractCreature.pos;
		var num = 0;
        for (var j = 0; j < Random.Range(8, 12); j++)
		{
            var pths = room.aimap.getAItile(worldCoord).outgoingPaths;
            if (pths.Count == 0)
				continue;
			var opt = pths[Random.Range(0, pths.Count)];
			if (!room.aimap.IsConnectionAllowedForCreature(opt, Template) || !(LastShortCut != opt) || !(worldCoord2 != opt.destinationCoord))
				continue;
			var flag = true;
			for (var k = 0; k < num; k++)
			{
				var pthk = pth[k];
                if (pthk.startCoord == opt.destinationCoord || pthk.destinationCoord == opt.destinationCoord)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				worldCoord2 = worldCoord;
				if (pth.Count <= num)
					pth.Add(opt);
				else
					pth[num] = opt;
                ++num;
				worldCoord = opt.destinationCoord;
			}
		}
		return num;
	}

    public virtual bool ConsiderDanger(Creature crit)
    {
        if (Hunger > 0 && crit.grasps is Grasp[] ar)
        {
            for (var i = 0; i < ar.Length; i++)
            {
                if (ar[i]?.grabbed is PhysicalObject obj && CanEat(obj))
                    return false;
            }
        }
        return Template.CreatureRelationship(crit.Template).type == CreatureTemplate.Relationship.Type.Afraid;
    }

	public override void Stun(int st)
	{
		ReleaseGrasp(0);
		base.Stun(st);
	}

	public override void Die()
	{
		surfaceFriction = .4f;
        base.Die();
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 20;
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		firstChunk.HardSetPosition(newRoom.MiddleOfTile(pos) - vector * 5f);
		firstChunk.vel = vector * (safariControlled ? 25f : 12.5f);
		graphicsModule?.Reset();
	}
}