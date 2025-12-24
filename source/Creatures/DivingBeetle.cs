using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class DivingBeetle : InsectoidCreature
{
    [AllowNull] public DivingBeetleAI AI;
    public int FootingCounter, OutOfWaterFooting, SpecialMoveCounter, GrabbedCounter;
    public IntVector2 SpecialMoveDestination;
    public MovementConnection LastFollowedConnection;
    public bool Sitting, Swimming;
    public Vector2 TravelDir, TweakedDir;
    public float RunCycle, BurstSpeed, LastBurstSpeed, CarryObjectMass, StuckShake, AirLungs = 1f, BurstCounter;
    public ChunkSoundEmitter? VoiceSound;

    public virtual Vector2 ChunkVelResult
    {
        get
        {
            var bs = bodyChunks;
            return bs[0].vel * .9f + bs[1].vel + bs[2].vel * .8f;
        }
    }

    public virtual new HealthState State
    {
        get => (HealthState)abstractCreature.state;
    }

    public virtual bool Footing
    {
        get => FootingCounter > 10 || OutOfWaterFooting > 0;
    }

    public DivingBeetle(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var bs = bodyChunks =
        [
            new(this, 0, default, 7f, 1.6f * .4f),
            new(this, 1, default, 9f, 1.6f * .4f),
            new(this, 2, default, 7f, 1.6f * .2f)
        ];
        bodyChunkConnections =
        [
            new(bs[0], bs[1], 8f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(bs[1], bs[2], 10f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(bs[0], bs[2], 6f, BodyChunkConnection.Type.Push, 1f, -1f)
        ];
        airFriction = .999f;
        gravity = .9f;
        bounce = .3f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .98f;
        buoyancy = .5f;
        waterRetardationImmunity = .1f;
        baseCurrentAffectiveness = .25f;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new DivingBeetleGraphics(this);
        graphicsModule.Reset();
    }

    public override void Update(bool eu)
    {
        var chs = bodyChunks;
        var fch = chs[0];
        if (Submersion < 1f && !safariControlled)
            fch.vel.y -= 1.75f;
        LastBurstSpeed = BurstSpeed;
        if (Consious && (Footing && Swimming || safariControlled))
        {
            BurstCounter += Mathf.Cos(BurstCounter) > 0f ? .175f : .1f;
            if (BurstCounter > Mathf.PI)
                BurstCounter = 0f;
            BurstSpeed = Mathf.Sin(BurstCounter) * 1.2f + .1f;
        }
        else
        {
            BurstCounter = 0f;
            BurstSpeed = 0f;
        }
        if (!Consious)
            buoyancy = .85f;
        else
            buoyancy = Footing && Swimming && !safariControlled ? .45f : .5f;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            fch.vel += Custom.DirVec(fch.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (!dead)
        {
            if (State.health < 0f && Random.value < 0f - State.health && Random.value < 1f / (Consious ? 80f : 800f))
                Die();
            if (Random.value < 1f / 30f && (Random.value * .2f > State.health || Random.value < 0f - State.health))
                Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * State.health), 0, 10))));
            if (State.health > 0f && State.health < 1f && Random.value < .01f && poison < .1f)
                State.health = Mathf.Min(1f, State.health + 1f / Mathf.Lerp(550f, 70f, State.health));
            if (!Consious && Random.value < .05f)
                chs[1].vel += Custom.RNV() * (Random.value * 3f);
            if (stun < 35 && grabbedBy.Count > 0 && grabbedBy[0].grabber is Creature c && c is not Vulture and not Leech)
            {
                ++GrabbedCounter;
                if (GrabbedCounter == 25 && Random.value < .85f * State.health)
                {
                    Slash(c, null);
                    if (grabbedBy.Count == 0)
                        stun = 0;
                }
                else if (GrabbedCounter < 25)
                {
                    for (var i = 0; i < chs.Length; i++)
                        chs[i].pos += Custom.RNV() * (Random.value * 6f) + Custom.RNV() * (Random.value * 6f);
                }
            }
            else
                GrabbedCounter = 0;
        }
        if (OutOfWaterFooting > 0)
            --OutOfWaterFooting;
        base.Update(eu);
        lungs = 1f;
        if (room is not Room rm)
            return;
        Sitting = false;
        if (Consious)
        {
            if (rm.aimap.TileAccessibleToCreature(fch.pos, Template) || rm.aimap.TileAccessibleToCreature(chs[1].pos, Template))
                ++FootingCounter;
            Act();
        }
        else
            FootingCounter = 0;
        if (Footing)
        {
            for (var k = 0; k < 2; k++)
            {
                var ch = chs[k];
                ch.vel *= .82f;
                ch.vel.y += gravity;
            }
            chs[2].vel.y += gravity * Mathf.Lerp(.5f, 1f, AI.stuckTracker.Utility());
        }
        TravelDir *= Sitting ? .5f : .9995f;
        if (grasps[0] is not null)
            CarryObject(eu);
        else
            CarryObjectMass = 0f;
        if (Consious && Submersion >= .5f)
        {
            chs[2].vel -= fch.vel * .16f + Custom.DirVec(chs[2].pos, fch.pos);
            fch.vel += Custom.DirVec(chs[2].pos, fch.pos);
        }
        if (Submersion < .2f)
        {
            if (AirLungs > 0f)
                AirLungs -= .002f;
            if (Consious)
            {
                fch.pos += Custom.RNV() * 2f;
                chs[1].pos += Custom.RNV() * 2f;
                chs[2].pos += Custom.RNV() * 2f;
                StuckShake += .2f;
            }
        }
        else
            AirLungs = 1f;
        lungs = 1f;
        StuckShake *= .5f;
        var ps = new Vector2(fch.pos.x, fch.pos.y + 20f);
        if (Consious && !safariControlled && (Submersion is > .1f and < 1f || Submersion >= 1f && !rm.PointSubmerged(ps)))
        {
            for (var i = 1; i < chs.Length; i++)
                chs[i].vel.y -= 4.5f;
            var wc = abstractCreature.pos;
            wc.y -= 5;
            if (wc.TileDefined)
                AI?.SetDestination(wc);
        }
        if (AirLungs <= 0f)
            Die();
    }

    public virtual void CarryObject(bool eu)
    {
        if (grasps[0].grabbed is not Creature cr || Random.value < .025f && AI.DynamicRelationship(cr.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && !safariControlled)
        {
            ReleaseGrasp(0);
            return;
        }
        var chs = bodyChunks;
        var fch = chs[0];
        CarryObjectMass = cr.TotalMass;
        if (CarryObjectMass <= TotalMass * 1.1f)
            CarryObjectMass /= 2f;
        else if (CarryObjectMass <= TotalMass / 5f)
            CarryObjectMass = 0f;
        var chGrabbed = cr.bodyChunks[grasps[0].chunkGrabbed];
        var num = fch.rad + chGrabbed.rad;
        var vector = -Custom.DirVec(fch.pos, chGrabbed.pos) * (num - Vector2.Distance(fch.pos, chGrabbed.pos));
        var num2 = chGrabbed.mass / (fch.mass + chGrabbed.mass);
        num2 *= .2f * (1f - AI.stuckTracker.Utility());
        fch.pos += vector * num2;
        fch.vel += vector * num2;
        chGrabbed.pos -= vector * (1f - num2);
        chGrabbed.vel -= vector * (1f - num2);
        Vector2 vector2 = fch.pos + Custom.DirVec(chs[1].pos, fch.pos) * num,
            vector3 = chGrabbed.vel - fch.vel;
        chGrabbed.vel = fch.vel;
        if (!enteringShortCut.HasValue && (vector3.magnitude * chGrabbed.mass > 30f || !Custom.DistLess(vector2, chGrabbed.pos, 70f + chGrabbed.rad)))
            ReleaseGrasp(0);
        else
            chGrabbed.MoveFromOutsideMyUpdate(eu, vector2);
        if (grasps[0] is Grasp g)
        {
            for (var i = 0; i < 2; i++)
                g.grabbed.PushOutOf(chs[i].pos, chs[i].rad, grasps[0].chunkGrabbed);
        }
    }

    public override Color ShortCutColor() => DivingBeetleGraphics.BugCol;

    public virtual void Swim()
    {
        var chs = bodyChunks;
        var fch = chs[0];
        fch.vel *= 1f - (safariControlled ? .3f : .6f) * fch.submersion;
        if (safariControlled)
        {
            if (inputWithDiagonals is Player.InputPackage p)
            {
                fch.vel += new Vector2(p.x * 1.3f, p.y * 1.15f);
                if (p.y == 1 || Mathf.Abs(p.x) == 1)
                    fch.vel.y += 2f;
                else if (p.y == 0 && p.x == 0 && fch.vel.y > .1f)
                {
                    fch.vel.y -= 2.1f;
                    chs[2].vel.y -= .25f;
                }
                if (p.thrw && lastInputWithDiagonals?.thrw is false or null)
                    ReleaseGrasp(0);
                ++chs[2].vel.y;
                if (p.x == 0 && p.y == 0)
                {
                    for (var i = 0; i < chs.Length; i++)
                        chs[i].vel.y -= .3f;
                }
            }
            else if (fch.vel.y > .1f)
            {
                fch.vel.y -= 2.1f;
                chs[2].vel.y += .7f;
            }
            chs[2].vel -= fch.vel * .16f + Custom.DirVec(chs[2].pos, fch.pos);
        }
        RunCycle += .125f;
        GoThroughFloors = true;
        var movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(fch.pos), true);
        if (movementConnection == default)
            movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(chs[1].pos), true);
        if (!safariControlled && (movementConnection == default || !room.PointSubmerged(movementConnection.DestTile.ToVector2() * 20f)))
        {
            movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(new Vector2(fch.pos.x, fch.pos.y - 20f)), true);
            if (movementConnection == default)
                movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(new Vector2(chs[1].pos.x, chs[1].pos.y - 20f)), true);
        }
        if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
        {
            movementConnection = default;
            if (inputWithDiagonals.HasValue)
            {
                var type = MovementConnection.MovementType.Standard;
                if (room.GetTile(fch.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    type = MovementConnection.MovementType.ShortCut;
                if (inputWithDiagonals.Value.AnyDirectionalInput)
                    movementConnection = new(type, room.GetWorldCoordinate(fch.pos), room.GetWorldCoordinate(fch.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
            }
        }
        if (movementConnection != default)
        {
            fch.vel *= .8f;
            fch.vel += Custom.DirVec(fch.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
            if (!safariControlled || Submersion < .5f)
            {
                FootingCounter = Math.Max(FootingCounter, 25);
                Run(movementConnection);
                OutOfWaterFooting = 20;
            }
            else
            {
                fch.vel *= .75f;
                FootingCounter = 0;
                Run(movementConnection);
                OutOfWaterFooting = 0;
            }
        }
        fch.vel *= BurstSpeed;
    }

    public virtual void Act()
    {
        AI.Update();
        var chs = bodyChunks;
        var fch = chs[0];
        if (Random.value < .005f && !dead)
            room.PlaySound(NewSoundID.M4R_GenericBug_Chip, fch, false, 1f, 1f);
        if (Submersion > .3f)
        {
            Swim();
            Swimming = true;
            return;
        }
        Swimming = false;
        if (AI.stuckTracker.Utility() > .9f)
            StuckShake = Custom.LerpAndTick(StuckShake, 1f, .07f, 1f / 70f);
        else if (AI.stuckTracker.Utility() < .2f)
            StuckShake = Custom.LerpAndTick(StuckShake, 0f, .07f, .05f);
        if (StuckShake > 0f && (!safariControlled || inputWithDiagonals.HasValue && inputWithDiagonals.Value.AnyDirectionalInput))
        {
            for (var k = 0; k < chs.Length; k++)
            {
                var c = chs[k];
                c.vel += Custom.RNV() * (Random.value * 5f * StuckShake);
                c.pos += Custom.RNV() * (Random.value * 5f * StuckShake);
            }
        }
        if (SpecialMoveCounter > 0)
        {
            --SpecialMoveCounter;
            MoveTowards(room.MiddleOfTile(SpecialMoveDestination));
            TravelDir = Vector2.Lerp(TravelDir, Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(SpecialMoveDestination)), .4f);
            if (Custom.DistLess(mainBodyChunk.pos, room.MiddleOfTile(SpecialMoveDestination), 5f))
                SpecialMoveCounter = 0;
        }
        else
        {
            if (!room.aimap.TileAccessibleToCreature(fch.pos, Template) && !room.aimap.TileAccessibleToCreature(chs[1].pos, Template))
                FootingCounter = Custom.IntClamp(FootingCounter - 3, 0, 35);
            else if ((room.GetWorldCoordinate(fch.pos) == AI.pathFinder.GetDestination || room.GetWorldCoordinate(chs[1].pos) == AI.pathFinder.GetDestination) && AI.threatTracker.Utility() < .5f && !safariControlled)
            {
                Sitting = true;
                GoThroughFloors = false;
            }
            else
            {
                var movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(fch.pos), true);
                if (movementConnection == default)
                    movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(chs[2].pos), true);
                if (movementConnection == default)
                    movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(chs[1].pos), true);
                if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
                {
                    movementConnection = default;
                    if (inputWithDiagonals.HasValue)
                    {
                        var type = MovementConnection.MovementType.Standard;
                        if (room.GetTile(fch.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                            type = MovementConnection.MovementType.ShortCut;
                        if (inputWithDiagonals.Value.AnyDirectionalInput && (Footing || fch.submersion != 0f))
                            movementConnection = new(type, room.GetWorldCoordinate(fch.pos), room.GetWorldCoordinate(fch.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
                        if (inputWithDiagonals.Value.thrw && lastInputWithDiagonals?.thrw is false or null)
                            ReleaseGrasp(0);
                        if (inputWithDiagonals.Value.y < 0)
                            GoThroughFloors = true;
                        else
                            GoThroughFloors = false;
                    }
                }
                if (movementConnection != default)
                    Run(movementConnection);
                else
                    GoThroughFloors = false;
            }
        }
        if (Consious && !Custom.DistLess(fch.pos, fch.lastPos, 2f))
            RunCycle += .125f;
        if (RunCycle < Mathf.Floor(RunCycle))
            room.PlaySound(SoundID.Drop_Bug_Step, fch);
        if (Footing)
        {
            for (var i = 0; i < chs.Length; i++)
                chs[i].vel *= .75f;
        }
    }

    public override void Stun(int st)
    {
        base.Stun(st);
        if (st > 4 && Random.value < .5f)
            ReleaseGrasp(0);
    }

    public virtual void Run(MovementConnection followingConnection)
    {
        if (followingConnection.type == MovementConnection.MovementType.ReachUp && Submersion == 0f)
            (AI.pathFinder as StandardPather)!.pastConnections.Clear();
        if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
        {
            enteringShortCut = followingConnection.StartTile;
            if (safariControlled)
            {
                var flag = false;
                var list = new List<IntVector2>();
                var shortcuts = room.shortcuts;
                for (var i = 0; i < shortcuts.Length; i++)
                {
                    var shortcutData = shortcuts[i];
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
                        list.Add(shortcutData.StartTile);
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
                        flag = true;
                }
                if (flag)
                {
                    if (list.Count > 0)
                    {
                        list.Shuffle();
                        NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
                    }
                    else
                        NPCTransportationDestination = followingConnection.destinationCoord;
                }
            }
            else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
                NPCTransportationDestination = followingConnection.destinationCoord;
        }
        else if ((followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach) && Submersion <= 0f)
        {
            SpecialMoveCounter = 30;
            SpecialMoveDestination = followingConnection.DestTile;
        }
        else
        {
            var movementConnection = followingConnection;
            if (AI.stuckTracker.Utility() == 0f)
            {
                var movementConnection2 = (AI.pathFinder as StandardPather)!.FollowPath(movementConnection.destinationCoord, false);
                if (movementConnection2 != default)
                {
                    if (movementConnection2.destinationCoord == followingConnection.startCoord)
                    {
                        Sitting = true;
                        return;
                    }
                    if (movementConnection2.destinationCoord.TileDefined && room.aimap.getAItile(movementConnection2.DestTile).acc < AItile.Accessibility.Solid)
                    {
                        var flag2 = false;
                        for (var j = Math.Min(followingConnection.StartTile.x, movementConnection2.DestTile.x); j < Math.Max(followingConnection.StartTile.x, movementConnection2.DestTile.x); j++)
                        {
                            if (flag2)
                                break;
                            for (var k = Math.Min(followingConnection.StartTile.y, movementConnection2.DestTile.y); k < Math.Max(followingConnection.StartTile.y, movementConnection2.DestTile.y); k++)
                            {
                                if (!room.aimap.TileAccessibleToCreature(j, k, Template))
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        if (!flag2)
                            movementConnection = movementConnection2;
                    }
                }
            }
            var vector = room.MiddleOfTile(movementConnection.DestTile);
            var fch = bodyChunks[0];
            TravelDir = Vector2.Lerp(TravelDir, Custom.DirVec(fch.pos, vector), .4f);
            if (LastFollowedConnection != default && LastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
                fch.vel += Custom.DirVec(fch.pos, vector) * 4f;
            if (Footing)
            {
                if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                    fch.vel.x += Mathf.Min((vector.x - fch.pos.x) / 8f, 1.2f);
                else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                    fch.vel.y += Mathf.Min((vector.y - fch.pos.y) / 8f, 1.2f);
            }
            if (LastFollowedConnection != default && (Footing || room.aimap.TileAccessibleToCreature(fch.pos, Template)) && (followingConnection.startCoord.x != followingConnection.destinationCoord.x && LastFollowedConnection.startCoord.x == LastFollowedConnection.destinationCoord.x || followingConnection.startCoord.y != followingConnection.destinationCoord.y && LastFollowedConnection.startCoord.y == LastFollowedConnection.destinationCoord.y))
                fch.vel *= .8f;
            if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
                FootingCounter = 0;
            MoveTowards(vector);
        }
        LastFollowedConnection = followingConnection;
    }

    public virtual void MoveTowards(Vector2 moveTo)
    {
        if (Random.value > State.health)
            return;
        var chs = bodyChunks;
        var fch = chs[0];
        var tr = Custom.DirVec(fch.pos, moveTo);
        TweakedDir = Vector2.Lerp(TweakedDir, tr, .1f * BurstSpeed);
        var vector = safariControlled ? tr : TweakedDir;
        if (!Footing)
            vector *= .3f;
        if (IsTileSolid(1, 0, -1) && (vector.x < -.5f && fch.pos.x > chs[1].pos.x + 5f || vector.x > .5f && fch.pos.x < chs[1].pos.x - 5f))
        {
            fch.vel.x -= (vector.x < 0f ? -1f : 1f) * 1.3f;
            chs[1].vel.x += (vector.x < 0f ? -1f : 1f) * .5f;
            if (!IsTileSolid(0, 0, 1))
                fch.vel.y += 3.2f;
        }
        var num = Custom.LerpMap(CarryObjectMass, 0f, 4f, 1f, .2f, .7f) * Mathf.Lerp(1f, 1.5f, StuckShake);
        fch.vel += vector * (4.5f * num);
        GoThroughFloors = moveTo.y < fch.pos.y - 5f;
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        var bs = bodyChunks;
        var otherCh = otherObject.bodyChunks[otherChunk];
        if (otherObject is DivingBeetle dvb)
        {
            AI.CollideWithKin(dvb);
            if (bs[myChunk].pos.y > otherCh.pos.y)
            {
                bs[myChunk].vel.y += 2f;
                otherCh.vel.y -= 2f;
            }
        }
        if (otherObject is not Creature c || !Consious)
            return;
        AI.tracker.SeeCreature(c.abstractCreature);
        var flag = myChunk == 0 && grasps[0] is null && Vector2.Dot(Custom.DirVec(bs[1].pos, bs[0].pos), Custom.DirVec(bs[0].pos, otherCh.pos)) > -.2f && AI.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
        if (safariControlled)
            flag = myChunk == 0 && grasps[0] is null && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && AI.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
        if (!flag || Submersion < .5f)
            return;
        for (var i = 0; i < 4; i++)
            room.AddObject(new WaterDrip(Vector2.Lerp(bs[0].pos, otherCh.pos, Random.value), Custom.RNV() * (Random.value * 14f), false));
        if ((safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp || !safariControlled) && AI?.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
        {
            if (Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, .5f, false, true))
            {
                c.Violence(bs[0], Custom.DirVec(bs[0].pos, otherCh.pos) * 4f, otherCh, null, DamageType.Bite, .4f, 0f);
                room.PlaySound(SoundID.Drop_Bug_Grab_Creature, bs[0]);
            }
            else
                Slash(c, otherCh);
        }
        else
            Slash(c, otherCh);
    }

    public virtual void Slash(Creature creature, BodyChunk? chunk)
    {
        var fch = bodyChunks[0];
        var otherChs = creature.bodyChunks;
        if (chunk is null)
        {
            chunk = otherChs[0];
            var dst = float.MaxValue;
            for (var i = 0; i < otherChs.Length; i++)
            {
                var ch = otherChs[i];
                if (Custom.DistLess(fch.pos, ch.pos, dst))
                {
                    dst = Vector2.Distance(fch.pos, ch.pos);
                    chunk = ch;
                }
            }
        }
        bool flag = Random.value < 1f / 3f, flag2 = Random.value < .2f;
        creature.Violence(fch, Custom.DirVec(fch.pos, chunk.pos) * 8f, chunk, null, DamageType.Bite, flag2 ? 1.1f : .4f, flag ? 50f : 15f);
        fch.vel = Custom.DirVec(chunk.pos, fch.pos) * 8f;
        for (var j = 0; j < 5; j++)
            room.AddObject(new WaterDrip(Vector2.Lerp(fch.pos, chunk.pos, Random.value), Custom.RNV() * (Random.value * (flag2 ? 24f : 14f)), false));
        if (AI.DynamicRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
        {
            if (flag || flag2 || creature.dead)
                AI.AttackCounter = Math.Max(70, AI.AttackCounter);
            AI.TargetCreature = creature.abstractCreature;
        }
        room.PlaySound(SoundID.Drop_Bug_Grab_Creature, fch, false, 1f, 1.1f);
    }

    public override void Die()
    {
        base.Die();
        ReleaseGrasp(0);
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (!RippleViolenceCheck(source))
            return;
        damage = hitChunk.index != 0 ? damage * Mathf.Lerp(.9f, 1.1f, Random.value) : damage * Mathf.Lerp(.975f, 1.25f, Random.value);
        if (!dead && (damage > .1f || stunBonus > 20f) && Random.value < Custom.LerpMap(damage, .3f, 1f, .4f, .95f) && (VoiceSound is null || VoiceSound.slatedForDeletetion))
            VoiceSound = room.PlaySound(NewSoundID.M4R_GenericBug_BigChip, bodyChunks[0], false, .95f, .9f);
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        if (Random.value < .5f && State.health < 0f)
            Die();
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
            ch.vel = vector * 2f;
        }
        graphicsModule?.Reset();
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}