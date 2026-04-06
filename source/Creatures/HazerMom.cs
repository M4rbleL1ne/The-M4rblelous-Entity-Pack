using LBMergedMods.Hooks;
using RWCustom;
using Smoke;
using System;
using UnityEngine;
using Watcher;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class HazerMom : Creature
{
    public BlackHaze? Smoke;
    public PlacedObject? PlacedObj;
    public IntVector2 LastAirTile;
    public int HopDir, MoveCounter, Clds;
    public Vector2 SprayDir, GetToSprayDir, SwimDir;
    public float InkLeft, SwimCycle, Swim, FloatHeight;
    public Vector2? SprayStuckPos;
    public bool HasSprayed, Spraying;

    public virtual BodyChunk ChunkInOrder0 => bodyChunks[1];

    public virtual BodyChunk ChunkInOrder1 => bodyChunks[0];

    public virtual BodyChunk ChunkInOrder2 => bodyChunks[2];

    public virtual new HazerMomState State => (base.State as HazerMomState)!;

    public HazerMom(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var alive = abstractCreature.state.alive;
        InkLeft = alive ? 5f : 0f;
        HasSprayed = !alive;
        bodyChunks =
        [
            new(this, 1, default, alive ? 12f : 8f, .27f),
            new(this, 0, default, alive ? 11f : 8f, .27f * .6f),
            new(this, 2, default, 3f, .27f)
        ];
        bodyChunkConnections =
        [
            new(bodyChunks[1], firstChunk, 12f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(firstChunk, bodyChunks[2], 12f, BodyChunkConnection.Type.Normal, 1f, -1f)
        ];
        airFriction = .995f;
        gravity = .92f;
        bounce = .1f;
        surfaceFriction = .1f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = dead ? .92f : .8f;
        HopDir = Random.value >= .5f ? 1 : -1;
        MoveCounter = -Random.Range(120, 2500);
        GetToSprayDir = Custom.RNV();
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        var state = State;
        if (state.OrigRoom > -1 && state.OrigRoom == placeRoom.abstractRoom.index && state.PlacedObjectIndex >= 0 && state.PlacedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
            PlacedObj = placeRoom.roomSettings.placedObjects[state.PlacedObjectIndex];
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (type == DamageType.Explosion)
            damage *= 4f;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new HazerMomGraphics(this);

    public override void Update(bool eu)
    {
        buoyancy = dead ? .92f : .8f;
        var chs = bodyChunks;
        var nds = abstractPhysicalObject.Room.nodes;
        if ((!abstractPhysicalObject.pos.NodeDefined || abstractPhysicalObject.pos.abstractNode >= nds.Length) && nds.Length > 0)
            abstractPhysicalObject.pos.abstractNode = Random.Range(0, nds.Length);
        WeightedPush(1, 2, Custom.DirVec(chs[2].pos, chs[1].pos), Custom.LerpMap(Vector2.Distance(chs[2].pos, chs[1].pos), 3.5f, 8f, 1f, 0f));
        if (!room.GetTile(chs[0].pos).Solid)
            LastAirTile = Room.StaticGetTilePosition(chs[0].pos);
        else
        {
            for (var i = 0; i < chs.Length; i++)
                chs[i].HardSetPosition(room.MiddleOfTile(LastAirTile) + Custom.RNV());
        }
        if (Spraying)
        {
            InkLeft = Mathf.Max(0f, InkLeft - .0045454544f);
            if (!dead)
            {
                SprayDir = Vector3.Slerp(SprayDir, GetToSprayDir, .2f);
                if (Random.value < .1f)
                    GetToSprayDir = Custom.DegToVec(-90f + 180f * Random.value);
                WeightedPush(1, 2, SprayDir, 5f);
            }
            if (InkLeft <= 0f)
            {
                for (var j = 0; j < chs.Length; j++)
                    chs[j].rad = 8f;
                Spraying = false;
                Die();
            }
            if (SprayStuckPos.HasValue)
            {
                if (Custom.DistLess(SprayStuckPos.Value, chs[2].pos, 15f) && grabbedBy.Count == 0)
                    chs[2].pos = SprayStuckPos.Value;
                else
                    SprayStuckPos = null;
            }
            else if (chs[2].ContactPoint.y < 0 && grabbedBy.Count == 0)
                SprayStuckPos = chs[2].pos;
        }
        else
            SprayStuckPos = null;
        if (Smoke is BlackHaze haze)
        {
            if (haze.room != room || haze.slatedForDeletetion)
                Smoke = null;
            else
            {
                haze.MoveTo(ChunkInOrder0.pos, eu);
                if (Spraying)
                {
                    haze.EmitSmoke(Vector3.Slerp(Custom.DirVec(ChunkInOrder1.pos, ChunkInOrder0.pos), SprayDir, .4f) * 20f, 1f);
                    if ((1f - InkLeft * .2f) * 3f > Clds)
                    {
                        haze.EmitBigSmoke(Mathf.InverseLerp(4f, 0f, Clds));
                        ++Clds;
                    }
                }
            }
        }
        else if (Spraying)
            room.AddObject(Smoke = new(room, ChunkInOrder0.pos) { rippleLayer = abstractPhysicalObject.rippleLayer });
        if (PlacedObj?.data is PlacedObject.ConsumableObjectData dt && grabbedBy.Count > 0)
        {
            if (room.game.session is StoryGameSession sess)
                sess.saveState.ReportConsumedItem(room.world, false, State.OrigRoom, State.PlacedObjectIndex, Random.Range(dt.minRegen, dt.maxRegen));
            PlacedObj = null;
        }
        base.Update(eu);
        if (room is not Room rm)
            return;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
        {
            chs[0].vel += Custom.DirVec(chs[0].pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        var grabbed = false;
        var grabs = grabbedBy;
        for (var i = 0; i < grabs.Count; i++)
        {
            if (grabs[i].grabber is not Frog)
            {
                grabbed = true;
                break;
            }
        }
        if (grabbed)
        {
            MoveCounter = -Math.Abs(MoveCounter);
            GoThroughFloors = true;
        }
        else
        {
            GoThroughFloors = false;
            if (!dead && !Spraying)
                Act();
            else if (safariControlled && !dead && Spraying && (!inputWithoutDiagonals.HasValue || !inputWithoutDiagonals.Value.thrw))
                Spraying = false;
            else
                Swim = Mathf.Max(0f, Swim - 1f / 30f);
        }
    }

    public virtual void Act()
    {
        var grabbed = false;
        var grabs = grabbedBy;
        for (var i = 0; i < grabs.Count; i++)
        {
            if (grabs[i].grabber is not Frog)
            {
                grabbed = true;
                break;
            }
        }
        if (grabbed)
            return;
        var chs = bodyChunks;
        if (safariControlled)
        {
            if (inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0))
            {
                if (MoveCounter < 0)
                    MoveCounter = 0;
                ++MoveCounter;
                SwimDir = new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y);
                HopDir = (int)Mathf.Sign(inputWithoutDiagonals.Value.x);
            }
            else
                MoveCounter = -10;
            if (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.thrw && InkLeft > 0f)
                Spraying = true;
        }
        else
            ++MoveCounter;
        var tl = room.GetTile(chs[0].pos);
        if (Submersion < .8f || (tl.WaterSurface && room.GetTile(chs[0].pos + new Vector2(0f, -20f)).Solid))
        {
            Swim = Mathf.Max(0f, Swim - 1f / 30f);
            if (tl.AnyWater)
            {
                for (var i = 0; i < chs.Length; i++)
                    chs[i].vel.y -= .3f;
            }
            if (chs[1].ContactPoint.y > -1 && chs[0].ContactPoint.y > -1)
                return;
            if (MoveCounter > 0 && MoveCounter % 6 == 0)
            {
                if (room.readyForAI && (room.aimap.getAItile(Room.StaticGetTilePosition(chs[0].pos) + new IntVector2(HopDir, 0)).floorAltitude > 2 || room.aimap.getAItile(Room.StaticGetTilePosition(chs[0].pos) + new IntVector2(HopDir * 2, 0)).floorAltitude > 2))
                {
                    HopDir = -HopDir;
                    MoveCounter = -Random.Range(60, 120);
                }
                else
                {
                    WeightedPush(2, 1, new Vector2(HopDir, 0f), 4f);
                    chs[1].vel += new Vector2(-HopDir, 0f);
                    chs[0].vel += new Vector2(HopDir * 3f, 4f);
                    chs[2].vel += new Vector2(HopDir * 3f, 4f);
                    room.PlaySound(SoundID.Hazer_Shuffle, chs[0]);
                    if (MoveCounter > Random.Range(30, 400))
                    {
                        if (Random.value < 1f / 3f)
                            HopDir = -HopDir;
                        MoveCounter = -Random.Range(120, 2500);
                    }
                }
            }
            if (Random.value < .1f && chs[1].ContactPoint.x != 0)
                HopDir = -chs[1].ContactPoint.x;
            if (Random.value < .1f && chs[2].ContactPoint.x != 0)
                HopDir = -chs[1].ContactPoint.x;
        }
        else if (MoveCounter > 0)
        {
            Swim = Mathf.Min(1f, Swim + 1f / 30f);
            SwimCycle += Swim / 18f;
            SwimDir = (SwimDir + Custom.RNV() * (Random.value * .1f)).normalized;
            if (room.readyForAI)
            {
                Vector2 vector = default;
                var tilePosition = Room.StaticGetTilePosition(chs[2].pos);
                var terrainProximity = room.aimap.getTerrainProximity(chs[2].pos);
                for (var j = 1; j < 3; j++)
                {
                    for (var k = 0; k < 8; k++)
                    {
                        var dir = Custom.eightDirections[k];
                        if (terrainProximity < 3 && room.aimap.getTerrainProximity(tilePosition + dir * j) > terrainProximity)
                            vector += dir.ToVector2() * (Random.value / j);
                        else if (!room.GetTile(tilePosition + dir * j).AnyWater)
                            vector -= dir.ToVector2() * (.1f * Random.value / j);
                    }
                }
                SwimDir = (SwimDir + Vector2.ClampMagnitude(vector, 1f) * Random.value).normalized;
            }
            chs[2].vel += SwimDir;
            if (MoveCounter > Random.Range(120, 8000))
                MoveCounter = -Random.Range(120, 800);
            FloatHeight = Mathf.Max(30f, Math.Abs(room.DefaultWaterLevel(new(tl.X, tl.Y)) * 20f - chs[2].pos.y));
        }
        else
        {
            Swim = Mathf.Max(0f, Swim - 1f / 130f);
            SwimCycle += 1f / 160f;
            var num = room.DefaultWaterLevel(new(tl.X, tl.Y)) * 20f - FloatHeight + Mathf.Sin(SwimCycle * Mathf.PI * 2f) * 10f;
            ChunkInOrder0.vel.y += .25f * (1f - Swim);
            ChunkInOrder1.vel.y -= Mathf.Clamp(chs[0].pos.y - num, -.6f, .6f) * (1f - Swim);
            ChunkInOrder2.vel.y -= .25f * (1f - Swim);
            if (SwimDir.y < .5f)
                SwimDir = (SwimDir + new Vector2(0f, .1f)).normalized;
        }
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (MoveCounter < 0 && otherObject is Creature cr && !cr.dead && (cr is not HazerMom and not Hazer))
        {
            MoveCounter /= 2;
            if (Random.value < .2f)
                HopDir = (int)Mathf.Sign(mainBodyChunk.pos.x - otherObject.bodyChunks[otherChunk].pos.x);
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override void Die()
    {
        base.Die();
        if (!HasSprayed && InkLeft > 2.5f)
        {
            Spraying = true;
            HasSprayed = true;
        }
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
        {
            var ch = chs[i];
            ch.pos = newRoom.MiddleOfTile(pos) - vector * ((-1.5f + i) * 15f);
            ch.lastPos = newRoom.MiddleOfTile(pos);
            ch.vel = vector * 8f;
        }
        graphicsModule?.Reset();
    }

    public override void LoseAllGrasps() { }
}