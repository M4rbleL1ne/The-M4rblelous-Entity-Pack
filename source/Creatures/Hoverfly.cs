using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class Hoverfly : InsectoidCreature, Weapon.INotifyOfFlyingWeapons
{
    public record struct IndividualVariations(float BodyBonus, float SoundPitchBonus, float DefaultWingDeployment, float SmallWingBonus, float BigWingBonus, int WingVar, Color Color);

    [AllowNull] public HoverflyAI AI;
    public float SinCounter, FlyingPower, Stamina = 1f;
    public bool Flying;
    public int WaitToFlyCounter;
    public int FlipH;
    public IndividualVariations IVars;
    public IntVector2 SitDirection;
    public int DodgeDelay;

    public virtual bool WantToSitDownAtDestination
    {
        get
        {
            if (AI.Behavior == HoverflyAI.FlyBehavior.Idle && AI.pathFinder.GetDestination.room == room.abstractRoom.index)
                return Climbable(AI.pathFinder.GetDestination.Tile);
            return false;
        }
    }

    public virtual bool AtSitDestination
    {
        get
        {
            if (WantToSitDownAtDestination && Custom.ManhattanDistance(abstractCreature.pos, AI.pathFinder.GetDestination) < 2)
                return Climbable(AI.pathFinder.GetDestination.Tile);
            return false;
        }
    }

    public virtual new HealthState State => (base.State as HealthState)!;

    public virtual void GenerateIVars()
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        IVars = new(Random.value * .2f, Random.value * .2f, Random.value, Random.value * .2f, Random.value * .2f, Random.Range(1, 4), abstractCreature.superSizeMe ? Color.Lerp(new(235f / 255f, 57f / 255f, 78f / 255f), new(104f / 255f, 3f / 255f, 7f / 255f), Random.value) : Color.Lerp(new(0f, 251f / 255f, 1f), new(0f, 1f, 55f / 255f), Random.value));
        Random.state = state;
    }

    public override Vector2 DangerPos
    {
        get
        {
            var res = base.DangerPos;
            res.y -= 13.5f;
            if (graphicsModule is HoverflyGraphics grs)
                res.x += grs.DangerPosXBonus;
            return res;
        }
    }

    public Hoverfly(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = [new(this, 0, default, 7.5f, .5f)];
        bodyChunkConnections = [];
        GoThroughFloors = true;
        airFriction = .988f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .9f;
        buoyancy = .98f;
        GenerateIVars();
        SinCounter = Random.value;
        FlipH = Random.value >= .5f ? 1 : -1;
        Flying = true;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new HoverflyGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is null)
            return;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            firstChunk.vel += Custom.DirVec(firstChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (State.health < .5f && Random.value > State.health && Random.value < 1f / 3f)
        {
            Stun(4);
            if (State.health <= 0f && Random.value < .25f)
                Die();
        }
        if (Consious)
        {
            if (firstChunk.submersion > .5f)
                firstChunk.vel.y += .5f;
            else
                Act();
        }
        else
            Stamina = 0f;
        if (grasps[0] is not null)
            CarryObject();
    }

    public override void ReleaseGrasp(int grasp)
    {
        base.ReleaseGrasp(grasp);
        if (HoverflyData.TryGetValue(abstractCreature, out var d))
            d.BiteWait = 1000;
    }

    public virtual void SafariPickup()
    {
        if (inputWithDiagonals!.Value.pckp && grasps.Length > 0)
        {
            if (grasps[0]?.grabbed is null)
            {
                var physobs = room.physicalObjects;
                for (var j = 0; j < physobs.Length; j++)
                {
                    var objs = physobs[j];
                    for (var k = 0; k < objs.Count; k++)
                    {
                        if (objs[k] is DangleFruit f && f.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject) && Custom.DistLess(firstChunk.pos, f.firstChunk.pos, f.firstChunk.rad * 2f))
                            TryToGrabPrey(f);
                    }
                }
            }
            else if (lastInputWithDiagonals.HasValue && !lastInputWithDiagonals.Value.pckp && grasps[0]?.grabbed is DangleFruit da)
            {
                --da.bites;
                room.PlaySound(da.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, da.firstChunk, false, 1.25f, 1f);
                if (da.bites < 1)
                {
                    grasps[0].Release();
                    AI.FoodTracker.ForgetItem(da.abstractPhysicalObject);
                    da.Destroy();
                }
            }
        }
    }

    public virtual void Act()
    {
        var fc = firstChunk;
        if (DodgeDelay > 0)
            --DodgeDelay;
        AI.Update();
        if (grabbedBy.Count == 0)
            Stamina = Mathf.Min(Stamina + 1f / 70f, 1f);
        MovementConnection movementConnection = default;
        if ((Flying || !AtSitDestination) && !AI.SwooshToPos.HasValue)
            movementConnection = (AI.pathFinder as HoverflyPather)!.FollowPath(room.GetWorldCoordinate(fc.pos), true);
        if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
        {
            movementConnection = default;
            if (inputWithDiagonals.HasValue)
            {
                var type = MovementConnection.MovementType.Standard;
                if (room.GetTile(fc.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    type = MovementConnection.MovementType.ShortCut;
                if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
                {
                    if (Flying)
                        fc.vel += new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * .5f;
                    movementConnection = new(type, room.GetWorldCoordinate(fc.pos), room.GetWorldCoordinate(fc.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
                }
                if (inputWithDiagonals.Value.thrw && (lastInputWithDiagonals is not Player.InputPackage p || !p.thrw))
                    ReleaseGrasp(0);
                SafariPickup();
                GoThroughFloors = inputWithDiagonals.Value.y < 0;
            }
        }
        if (Flying)
        {
            SinCounter += 1f / Mathf.Lerp(45f, 85f, Random.value);
            if (SinCounter > 1f)
                SinCounter -= 1f;
            fc.vel.y += Mathf.Sin(SinCounter * Mathf.PI * 2f) * .05f * FlyingPower * Stamina;
            fc.vel *= Mathf.Lerp(1f, .98f, FlyingPower * Stamina);
            fc.vel.y += .8f * FlyingPower * Stamina;
            fc.vel *= AI.Behavior == HoverflyAI.FlyBehavior.Idle || AI.Behavior == HoverflyAI.FlyBehavior.Hunt ? .7f : 1f;
            var flag = false;
            if (movementConnection == default || Climbable(movementConnection.DestTile) || Climbable(Room.StaticGetTilePosition(fc.pos)))
            {
                var aiTile = room.aimap.getAItile(fc.pos);
                if (aiTile.narrowSpace || room.aimap.getTerrainProximity(fc.pos) == 1 && (movementConnection == default || room.aimap.getTerrainProximity(movementConnection.destinationCoord) == 1) || AtSitDestination)
                    flag = true;
            }
            if (safariControlled && (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp))
                flag = false;
            var flag2 = true;
            if (flag)
            {
                var tl = abstractCreature.pos.Tile + Custom.fourDirections[1];
                if (room.GetTile(tl).Solid)
                {
                    fc.vel += Custom.fourDirections[1].ToVector2() * 3f;
                    Land();
                }
                else if (movementConnection != default && movementConnection.destinationCoord.y < abstractCreature.pos.y)
                    flag2 = false;
            }
            else if (fc.ContactPoint.x != 0 || fc.ContactPoint.y != 0)
                fc.vel -= fc.ContactPoint.ToVector2() * (8f * FlyingPower * Stamina * Random.value);
            FlyingPower = Mathf.Lerp(FlyingPower, flag2 ? 1f : 0f, .1f);
        }
        else
        {
            FlyingPower = Mathf.Lerp(FlyingPower, 0f, .05f);
            if (Climbable(Room.StaticGetTilePosition(fc.pos)))
            {
                fc.vel *= .8f;
                fc.vel.y += gravity;
            }
            else
                Flying = true;
        }
        if (AtSitDestination)
            fc.vel += Vector2.ClampMagnitude(BodySitPosOffset(room, AI.pathFinder.GetDestination.Tile) - fc.pos, 10f) / 10f * .5f;
        if (movementConnection != default)
        {
            if (movementConnection.destinationCoord.x < movementConnection.startCoord.x)
                FlipH = -1;
            else if (movementConnection.destinationCoord.x > movementConnection.startCoord.x)
                FlipH = 1;
            GoThroughFloors = movementConnection.destinationCoord.y < movementConnection.startCoord.y;
            if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                enteringShortCut = movementConnection.StartTile;
                if (safariControlled)
                {
                    var flag3 = false;
                    var list = new List<IntVector2>();
                    var shortcuts = room.shortcuts;
                    for (var n = 0; n < shortcuts.Length; n++)
                    {
                        var shortcutData = shortcuts[n];
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
                            list.Add(shortcutData.StartTile);
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
                            flag3 = true;
                    }
                    if (flag3)
                    {
                        if (list.Count > 0)
                        {
                            list.Shuffle();
                            NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
                        }
                        else
                            NPCTransportationDestination = movementConnection.destinationCoord;
                    }
                }
                else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                    NPCTransportationDestination = movementConnection.destinationCoord;
            }
            else if (Flying)
            {
                var vector3 = room.MiddleOfTile(movementConnection.destinationCoord);
                MovementConnection movementConnection2;
                var num6 = 1;
                for (var num7 = 0; num7 < 3; num7++)
                {
                    movementConnection2 = (AI.pathFinder as HoverflyPather)!.FollowPath(movementConnection.destinationCoord, false);
                    if (movementConnection2 == default)
                        break;
                    vector3 += room.MiddleOfTile(movementConnection2.destinationCoord);
                    num6++;
                }
                vector3 /= num6;
                if (room.aimap is AImap map)
                {
                    var a = map.getTerrainProximity(fc.pos) / Mathf.Max(map.getTerrainProximity(fc.pos + Custom.DirVec(fc.pos, vector3) * Mathf.Clamp(fc.vel.magnitude * 5f, 5f, 15f)), 1f);
                    a = Mathf.Pow(Mathf.Min(a, 1f), 3f);
                    if (WantToSitDownAtDestination && AI.pathFinder.GetDestination.room == room.abstractRoom.index && Custom.DistLess(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), fc.pos, 200f) && AI.VisualContact(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), 0f))
                        a *= Mathf.Lerp(.2f, 1f, Mathf.InverseLerp(0f, 300f, Vector2.Distance(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), fc.pos)));
                    fc.vel += Vector2.ClampMagnitude(vector3 - fc.pos, 40f) * (.0275f * a * FlyingPower * Stamina);
                }
            }
            else
            {
                if (!movementConnection.destinationCoord.TileDefined)
                    return;
                if (room.GetTile(movementConnection.DestTile).Terrain == Room.Tile.TerrainType.Slope)
                    TakeOff(Custom.DegToVec(Random.value * 360f));
                if (Climbable(movementConnection.DestTile))
                {
                    fc.vel += Custom.DirVec(fc.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * Mathf.Lerp(.4f, 1.8f, AI.stuckTracker.Utility());
                    return;
                }
                ++WaitToFlyCounter;
                if (WaitToFlyCounter > 30)
                    TakeOff(Custom.DirVec(fc.pos, room.MiddleOfTile(movementConnection.destinationCoord)));
            }
        }
        else if (AI.SwooshToPos.HasValue)
        {
            fc.vel += Vector2.ClampMagnitude(AI.SwooshToPos.Value - fc.pos, 20f) * (.09f * FlyingPower * Stamina);
            Flying = true;
        }
    }

    public virtual void CarryObject()
    {
        if (grasps[0].grabbed is not DangleFruit d)
        {
            ReleaseGrasp(0);
            return;
        }
        var fch = firstChunk;
        var dfch = d.firstChunk;
        var dst = Vector2.Distance(fch.pos, dfch.pos);
        if (dst > 50f)
        {
            ReleaseGrasp(0);
            return;
        }
        var adjPos = Custom.DirVec(fch.pos, dfch.pos) * (dst - (fch.rad + dfch.rad));
        var relativeMass = dfch.mass / (fch.mass + dfch.mass) * .2f * (1f - AI?.stuckTracker.Utility() ?? 0f);
        fch.pos += adjPos * relativeMass;
        fch.vel += adjPos * relativeMass;
        dfch.pos -= adjPos * (1f - relativeMass);
        dfch.vel -= adjPos * (1f - relativeMass);
        dfch.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
        PushOutOf(fch.pos, fch.rad, 0);
        if (HoverflyData.TryGetValue(abstractCreature, out var data) && data.BiteWait == 0)
        {
            --d.bites;
            room.PlaySound(d.bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, dfch, false, 1.25f, 1f);
            if (d.bites < 1)
            {
                grasps[0].Release();
                AI?.FoodTracker.ForgetItem(d.abstractPhysicalObject);
                d.Destroy();
                --data.Hunger;
            }
            data.BiteWait = 1000;
        }
    }

    public virtual bool Climbable(IntVector2 tile)
    {
        if (safariControlled)
        {
            if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp || room.aimap.getTerrainProximity(tile) != 1)
                return room.aimap.getAItile(tile).acc is AItile.Accessibility.Corridor or AItile.Accessibility.Floor;
            return true;
        }
        if (room.aimap.getTerrainProximity(tile) != 1)
            return room.aimap.getAItile(tile).acc is AItile.Accessibility.Corridor or AItile.Accessibility.Floor;
        return true;
    }

    public virtual bool TryToGrabPrey(PhysicalObject prey)
    {
        var res = Grab(prey, 0, 0, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, false);
        if (res)
        {
            if (HoverflyData.TryGetValue(abstractCreature, out var d))
                d.BiteWait = 1000;
            if (AI is HoverflyAI ai && ai.SwooshToPos.HasValue)
            {
                if (!safariControlled)
                    firstChunk.vel += Custom.DirVec(ai.SwooshToPos.Value, firstChunk.pos) * 40f;
                ai.SwooshToPos = null;
            }
        }
        return res;
    }

    public virtual void TakeOff(Vector2 dir)
    {
        WaitToFlyCounter = 0;
        Flying = true;
        var num = 0;
        var b = default(Vector2);
        for (var i = 0; i < 8; i++)
        {
            var eightDir = Custom.eightDirections[i];
            for (var j = 0; j < 3; j++)
            {
                var terrainProximity = room.aimap.getTerrainProximity(abstractPhysicalObject.pos.Tile + eightDir * j);
                num += terrainProximity;
                b = eightDir.ToVector2() * terrainProximity;
            }
        }
        b /= num;
        var value = Random.value;
        firstChunk.vel += Vector2.Lerp(dir, b, .5f).normalized * (9f * value);
        FlyingPower = .5f;
        room.PlaySound(SoundID.Fly_Wing_Flap, firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
    }

    public virtual void Land()
    {
        WaitToFlyCounter = 0;
        Flying = false;
        room.PlaySound(SoundID.Fly_Wing_Flap, firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
    }

    public virtual Vector2 BodySitPosOffset(Room rm, IntVector2 pos)
    {
        if (rm.GetTile(pos + new IntVector2(FlipH, 0)).Solid)
        {
            SitDirection = new IntVector2(FlipH, 0);
            return rm.MiddleOfTile(pos) + new Vector2(FlipH * -2f, 0f);
        }
        if (rm.GetTile(pos + new IntVector2(-FlipH, 0)).Solid)
        {
            SitDirection = new IntVector2(-FlipH, 0);
            return rm.MiddleOfTile(pos) + new Vector2(-FlipH * -2f, 0f);
        }
        if (rm.GetTile(pos + new IntVector2(0, 1)).Solid)
        {
            SitDirection = new IntVector2(0, 1);
            return rm.MiddleOfTile(pos) + new Vector2(0f, -2f);
        }
        if (rm.GetTile(pos + new IntVector2(0, -1)).Solid)
        {
            SitDirection = new IntVector2(0, -1);
            return rm.MiddleOfTile(pos) + new Vector2(0f, 2f);
        }
        if (rm.GetTile(pos).verticalBeam)
        {
            if (!rm.GetTile(pos + new IntVector2(FlipH, 0)).Solid)
            {
                SitDirection = new IntVector2(-FlipH, 0);
                return rm.MiddleOfTile(pos) + new Vector2(FlipH * 7f, 0f);
            }
            if (!rm.GetTile(pos + new IntVector2(-FlipH, 0)).Solid)
            {
                SitDirection = new IntVector2(FlipH, 0);
                return rm.MiddleOfTile(pos) + new Vector2(-FlipH * 7f, 0f);
            }
        }
        return rm.MiddleOfTile(pos);
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (!Consious)
            return;
        if (otherObject is Creature)
            room.PlaySound(SoundID.Fly_Wing_Flap, firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (speed > 1.5f && firstContact)
            room.PlaySound(SoundID.Fly_Wing_Flap, firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
    }

    public override Color ShortCutColor() => IVars.Color;

    public override void Stun(int st)
    {
        Flying = false;
        ReleaseGrasp(0);
        base.Stun(st);
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = newRoom.ShorcutEntranceHoleDirection(pos).ToVector2();
        firstChunk.pos = newRoom.MiddleOfTile(pos) - vector * (-22.5f);
        firstChunk.lastPos = newRoom.MiddleOfTile(pos);
        firstChunk.vel = vector * 8f;
        graphicsModule?.Reset();
    }

    public override void RecreateSticksFromAbstract()
    {
        var sts = abstractCreature.stuckObjects;
        for (var i = 0; i < sts.Count; i++)
        {
            if (sts[i] is AbstractPhysicalObject.CreatureGripStick s && s.A == abstractCreature && s.B.realizedObject is PhysicalObject obj)
            {
                grasps[s.grasp] = new(this, obj, s.grasp, Random.Range(0, obj.bodyChunks.Length), Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, obj.TotalMass < TotalMass);
                obj.Grabbed(grasps[s.grasp]);
            }
        }
    }

    public virtual void FlyingWeapon(Weapon weapon)
    {
        if (weapon?.firstChunk is not BodyChunk b)
            return;
        if (AI?.VisualContact(b.pos, 2.5f) is true)
        {
            var vecto = firstChunk.pos - (b.pos + b.vel.normalized * 200f);
            vecto.y *= 2f;
            if (vecto.magnitude <= 400f)
            {
                var dir = b.vel.normalized;
                var projPos = b.pos;
                var vector = Custom.PerpendicularVector(dir) * Mathf.Sign(Custom.DistanceToLine(firstChunk.pos, projPos, projPos + dir));
                if (graphicsModule is HoverflyGraphics g)
                {
                    g.LookDir = Custom.DirVec(firstChunk.pos + vector * 20f, projPos);
                    g.EyeFearCounter = 30;
                }
                if (Random.value > .5f)
                    room?.PlaySound(NewSoundID.M4R_Hoverfly_Startle, firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
                if (!Flying || DodgeDelay > 0)
                    return;
                DodgeDelay = 10;
                if (!Custom.DistLess(projPos, firstChunk.pos, firstChunk.rad))
                    firstChunk.vel += vector * 12f;
            }
        }
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}