using RWCustom;
using System;
using UnityEngine;
using static LBMergedMods.Items.Physalis;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class XyloWorm : Creature, IPlayerEdible
{
    public PlacedObject? PlacedObj;
    public Vector2 LookDir, BodyDir;
    public IntVector2 LastAirTile;
    public float Lungs, Wiggle, Swallowed;
    public int Bites, StunCounter;
    public bool Big, Rotten;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => Rotten ? 0 : (Big ? 2 : 1);

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public override Vector2 DangerPos
    {
        get
        {
            var ch0 = ChunkInOrder(0);
            var ch1 = ChunkInOrder(1);
            return Vector2.Lerp(ch0.pos + Custom.DirVec(ch1.pos, ch0.pos) * 5f, ch1.pos, Swallowed * .9f);
        }
    }

    public virtual new VultureGrub.VultureGrubState State => (base.State as VultureGrub.VultureGrubState)!;

    public XyloWorm(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        Big = abstractCreature.superSizeMe;
        Bites = Big ? 3 : 2;
        var fac = Big ? 2.5f : 1f;
        var chs = bodyChunks =
        [
            new(this, 1, default, 3f * fac, .0625f + .001f * (fac - 1f)),
            new(this, 0, default, 3.5f * fac, .0625f + .001f * (fac - 1f)),
            new(this, 2, default, 3f * fac, .0625f + .001f * (fac - 1f))
        ];
        bodyChunkConnections =
        [
            new(chs[1], chs[0], 7f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(chs[0], chs[2], 7f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(chs[1], chs[2], 3.5f, BodyChunkConnection.Type.Push, 1f, -1f)
        ];
        airFriction = .995f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .1f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = .95f;
        LookDir = Custom.RNV() * Random.value;
        BodyDir = Custom.RNV() * Random.value;
        Rotten = abstractCreature.RottenMode();
    }

    public BodyChunk ChunkInOrder(int i) => bodyChunks[i switch
    {
        1 => 0,
        0 => 1,
        _ => 2,
    }];

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        var state = State;
        if (state.origRoom > -1 && state.origRoom == placeRoom.abstractRoom.index && state.placedObjectIndex >= 0 && state.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
            PlacedObj = placeRoom.roomSettings.placedObjects[state.placedObjectIndex];
    }

    public override void LoseAllGrasps()
    {
        ReleaseGrasp(0);
        StunCounter = 0;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new XyloWormGraphics(this);

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (grasps[0] is null && Consious && otherObject is Creature c && Template.CreatureRelationship(c).type == CreatureTemplate.Relationship.Type.Eats && Grab(otherObject, 0, otherChunk, Grasp.Shareability.NonExclusive, 1f, false, false))
            room?.PlaySound(SoundID.Rock_Hit_Creature, otherObject.bodyChunks[otherChunk], false, .55f, .85f);
    }

    public override void Update(bool eu)
    {
        if (Rotten && !dead)
            Die();
        var chs = bodyChunks;
        var ch0 = chs[0];
        if (Consious && grasps[0]?.grabbedChunk is BodyChunk b && b.owner is Creature obj && !obj.inShortcut && obj.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject))
        {
            var odr0 = ChunkInOrder(0);
            var vector2 = b.pos;
            if (obj.evenUpdate != evenUpdate)
                vector2 += b.vel;
            var vector3 = Custom.DirVec(odr0.pos, vector2);
            var fvec = vector3 * Vector2.Distance(odr0.pos, vector2) * (1f - odr0.mass / (odr0.mass + b.mass));
            odr0.vel += fvec;
            odr0.pos += fvec - b.vel.normalized * 7f;
            for (var i = 0; i < chs.Length; i++)
            {
                var ch = chs[i];
                ch.vel = Vector2.Lerp(ch.vel, b.vel * .5f, .4f);
            }
            if (StunCounter < (Big ? 160 : 200))
                ++StunCounter;
            else
            {
                StunCounter = 0;
                obj.Violence(odr0, null, b, null, DamageType.Bite, .01f, 0f);
                obj.Stun(Big ? 70 : 60);
                room.PlaySound(SoundID.Rock_Hit_Creature, b, false, .55f, .75f);
            }
            if ((obj is Player pl && pl.animation == Player.AnimationIndex.Roll) || !Custom.DistLess(b.pos, odr0.pos, 30f))
            {
                Stun(40);
                LoseAllGrasps();
            }
        }
        else
            LoseAllGrasps();
        Grasp? grasp = null;
        var gbs = grabbedBy;
        for (var i = 0; i < gbs.Count; i++)
        {
            var item = gbs[i];
            if (item.grabber is not Watcher.Frog)
            {
                grasp = item;
                break;
            }
        }
        var flag1 = grasp is null;
        if (!flag1)
            LoseAllGrasps();
        CollideWithTerrain = flag1;
        GoThroughFloors = !flag1;
        CollideWithObjects = flag1;
        CollideWithSlopes = flag1;
        WeightedPush(1, 2, Custom.DirVec(chs[2].pos, chs[1].pos), Custom.LerpMap(Vector2.Distance(chs[2].pos, chs[1].pos), 3.5f, 8f, 1f, 0f));
        if (!room.GetTile(ch0.pos).Solid)
            LastAirTile = room.GetTilePosition(ch0.pos);
        else
        {
            for (var i = 0; i < chs.Length; i++)
                chs[i].HardSetPosition(room.MiddleOfTile(LastAirTile) + Custom.RNV());
        }
        if (PlacedObj is PlacedObject pObj)
        {
            if (flag1 && Mathf.Abs(ch0.pos.x - pObj.pos.x) > 10f)
                ch0.vel.x += (Mathf.Abs(ch0.pos.x - pObj.pos.x) - 10f) / (4f * (ch0.pos.x < pObj.pos.x ? 1f : -1f));
            if (!Custom.DistLess(ch0.pos, pObj.pos, 50f) || !flag1)
            {
                if (room.game.session is StoryGameSession sess)
                {
                    var data = (pObj.data as PlacedObject.ConsumableObjectData)!;
                    sess.saveState.ReportConsumedItem(room.world, false, State.origRoom, State.placedObjectIndex, Random.Range(data.minRegen, data.maxRegen));
                }
                PlacedObj = null;
            }
        }
        base.Update(eu);
        if (room is not Room rm)
            return;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
        {
            ch0.vel += Custom.DirVec(ch0.pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (!flag1)
        {
            var dir = Custom.PerpendicularVector(Custom.DirVec(ch0.pos, grasp!.grabber.mainBodyChunk.pos));
            WeightedPush(1, 2, dir with { y = Mathf.Abs(dir.y) }, 4f);
            Stun(10);
        }
        if (!dead)
            Act();
        if (!dead && ch0.submersion > .5f)
        {
            Lungs = Mathf.Max(Lungs - 1f / 180f, 0f);
            if (Lungs == 0f)
                Die();
        }
        else
            Lungs = Mathf.Min(Lungs + .02f, 1f);
        var flag = false;
        if (!flag1 && grasp!.grabber is Player p && p.swallowAndRegurgitateCounter > 50 && p.objectInStomach is null && p.input[0].pckp)
        {
            var num = -1;
            for (var j = 0; j < 2; j++)
            {
                if (p.grasps[j] is Grasp g && p.CanBeSwallowed(g.grabbed))
                {
                    num = j;
                    break;
                }
            }
            if (num > -1 && p.grasps[num] is Grasp g1 && g1.grabbed == this)
                flag = true;
        }
        Swallowed = Custom.LerpAndTick(Swallowed, flag ? 1f : 0f, .05f, .05f);
    }

    public virtual void Act()
    {
        var chs = bodyChunks;
        if (!safariControlled || (safariControlled && inputWithoutDiagonals is Player.InputPackage pack && (pack.x != 0 || pack.y != 0)))
        {
            LookDir += Custom.RNV() * Random.value * .25f + new Vector2(0f, .025f * room.gravity * Random.value * Random.value);
            BodyDir += Custom.RNV() * Random.value * .25f;
        }
        else
            BodyDir = default;
        LookDir = Vector2.ClampMagnitude(LookDir, 1f);
        BodyDir = Vector2.ClampMagnitude(BodyDir, 1f);
        var norm = LookDir.normalized;
        var mag = LookDir.magnitude;
        WeightedPush(1, 0, norm, mag);
        WeightedPush(1, 2, norm, mag);
        norm = BodyDir * .2f;
        chs[0].vel += norm;
        chs[1].vel -= norm;
        chs[2].vel -= norm;
        norm = BodyDir.normalized;
        mag = BodyDir.magnitude;
        WeightedPush(0, 1, norm, mag);
        WeightedPush(0, 2, norm, mag);
        if (safariControlled)
        {
            if (inputWithoutDiagonals is not Player.InputPackage pack2)
                return;
            if (pack2.y != 0 || pack2.x != 0)
            {
                Wiggle = Mathf.Max(Random.Range(0f, .001f), Custom.LerpAndTick(Wiggle, 0f, .05f, .025f));
                if (Random.value < 1f / 60f)
                    Wiggle = Mathf.Max(Wiggle, Random.value);
                Vector2 vector = Custom.RNV(),
                    vector2 = Custom.RNV();
                var wiggle = Mathf.Pow(Wiggle, .1f) * 5f;
                var sgn = Math.Sign(pack2.x);
                chs[1].vel += new Vector2(Mathf.Abs(vector.x) * sgn * Math.Abs(pack2.x), vector.y) * Random.value * wiggle;
                chs[2].vel += new Vector2(Mathf.Abs(vector2.x) * sgn * Mathf.Abs(pack2.x), vector2.y) * Random.value * wiggle;
                if (pack2.x != 0)
                {
                    LookDir.x = Mathf.Abs(LookDir.x) * sgn;
                    BodyDir.x = Mathf.Abs(BodyDir.x) * sgn;
                }
            }
            else
                Wiggle = 0f;
        }
        else
        {
            Wiggle = Custom.LerpAndTick(Wiggle, 0f, .05f, .025f);
            if (Random.value < 1f / 60f)
                Wiggle = Mathf.Max(Wiggle, Random.value);
            if (Wiggle > 0f)
            {
                var wiggle = Mathf.Pow(Wiggle, .1f) * 5f;
                chs[1].vel += Custom.RNV() * Random.value * wiggle;
                chs[2].vel += Custom.RNV() * Random.value * wiggle;
            }
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
        {
            chs[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + i) * 15f;
            chs[i].lastPos = newRoom.MiddleOfTile(pos);
            chs[i].vel = vector * 8f;
        }
        graphicsModule?.Reset();
    }

    public virtual void BitByPlayer(Grasp grasp, bool eu)
    {
        --Bites;
        if (!dead)
            Die();
        LoseAllGrasps();
        room.PlaySound(SoundID.Slugcat_Eat_Centipede, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public override void Stun(int st)
    {
        base.Stun(st);
        LoseAllGrasps();
    }

    public virtual void ThrowByPlayer() { }
}