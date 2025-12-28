using UnityEngine;
using RWCustom;
using MoreSlugcats;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class FumeFruit : Weapon, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public FumeFruit? Fruit;
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, FruitPos;
        public bool Kill;

        public Stalk(FumeFruit fruit, Room room)
        {
            Fruit = fruit;
            FruitPos = fruit.firstChunk.pos;
            base.room = room;
            var tilePosition = Room.StaticGetTilePosition(fruit.firstChunk.pos);
            while (tilePosition.y >= 0 && !room.GetTile(tilePosition).Solid)
                --tilePosition.y;
            if (tilePosition.y < 0)
                Kill = true;
            RootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
            var segs = Segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(fruit.firstChunk.pos, RootPos) / 15f), 4, 60)][];
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = Vector2.Lerp(RootPos, fruit.firstChunk.pos, (float)i / segs.Length);
                segs[i] = [seg, seg, default];
            }
            Direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(FruitPos.x + FruitPos.y))));
            for (var j = 0; j < 100; j++)
                Update(false);
            fruit.ChangeCollisionLayer(0);
        }

        public override void Update(bool eu)
        {
            if (Kill)
            {
                if (Fruit is FumeFruit f)
                    f.MyStalk = null;
                Destroy();
                return;
            }
            base.Update(eu);
            var segments = Segments;
            for (var i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                seg[1] = seg[0];
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                else if (i == segments.Length - 1 && Fruit is FumeFruit fruit)
                {
                    seg[0] = fruit.firstChunk.pos;
                    seg[2] *= 0f;
                }
                else
                {
                    seg[0] += seg[2];
                    seg[2] *= .7f;
                    seg[2].y += .3f;
                    seg[2] += Direction * .4f * (1f - (i + 1f) / segments.Length);
                }
                if (i < segments.Length - 1)
                {
                    var segip1 = segments[i + 1];
                    Vector2 normalized = (seg[0] - segip1[0]).normalized;
                    float num = 15f,
                        num2 = Vector2.Distance(seg[0], segip1[0]);
                    seg[0] += normalized * (num - num2) * .5f;
                    seg[2] += normalized * (num - num2) * .5f;
                    segip1[0] -= normalized * (num - num2) * .5f;
                    segip1[2] -= normalized * (num - num2) * .5f;
                }
                if (i < Segments.Length - 2)
                {
                    var s2 = Segments[i + 2];
                    Vector2 normalized2 = (seg[0] - s2[0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    s2[2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            if (Fruit is FumeFruit fr)
            {
                var chunk = fr.firstChunk;
                if (!Custom.DistLess(FruitPos, chunk.pos, fr.grabbedBy.Count == 0 ? 100f : 20f) || fr.room != room || fr.slatedForDeletetion || chunk.vel.magnitude > 15f)
                {
                    fr.AbstrCons.Consume();
                    fr.MyStalk = null;
                    Fruit = null;
                }
                else
                {
                    chunk.vel.y += fr.gravity;
                    chunk.vel *= .6f;
                    chunk.vel += (FruitPos - chunk.pos) / 20f;
                    fr.setRotation = Custom.DirVec(segments[segments.Length - 2][0], chunk.pos);
                }
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segments.Length, false, false)];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var segments = Segments;
            var seg0 = segments[0];
            Vector2 vector = Vector2.Lerp(seg0[1], seg0[0], timeStacker);
            var s0 = (TriangleMesh)sLeaser.sprites[0];
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                Vector2 vector2 = Vector2.Lerp(segment[1], segment[0], timeStacker),
                    normalized = (vector2 - vector).normalized,
                    vector3 = Custom.PerpendicularVector(normalized) * 1.75f;
                var num = Vector2.Distance(vector2, vector) / 4f;
                s0.MoveVertice(i * 4, vector - vector3 + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 1, vector + vector3 + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 2, vector2 - vector3 - normalized * num - camPos);
                s0.MoveVertice(i * 4 + 3, vector2 + vector3 - normalized * num - camPos);
                vector = vector2;
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => ((TriangleMesh)sLeaser.sprites[0]).color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");
            var s0 = sLeaser.sprites[0];
            s0.RemoveFromContainer();
            newContainer.AddChild(s0);
            s0.MoveToBack();
        }
    }

    public Stalk? MyStalk;
    public float Darkness, LastDarkness, Swallowed;
    public bool LastModeThrown;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual bool StalkActive => MyStalk is not null;

    public override int DefaultCollLayer => 1;

    public FumeFruit(AbstractPhysicalObject obj) : base(obj, obj.world)
    {
        bodyChunks = [new(this, 0, default, 9f, .11f)];
        bodyChunkConnections = [];
        bounce = .2f;
        surfaceFriction = .3f;
        collisionLayer = 1;
        waterFriction = .98f;
        exitThrownModeSpeed = 15f;
        airFriction = .98f;
        buoyancy = 1.8f;
        gravity = .86f;
    }

    public override void Update(bool eu)
    {
        var ch = firstChunk;
        if (ch.ContactPoint.y < 0)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * .1f * ch.vel.x).normalized;
            ch.vel.x *= .8f;
        }
        base.Update(eu);
        if (room is not Room rm)
            return;
        rotationSpeed *= .1f;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
            ch.vel += Custom.DirVec(ch.pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 3f;
        if (LastModeThrown && ch.ContactPoint != default)
            Explode();
        LastModeThrown = mode == Mode.Thrown;
        var flag = false;
        if (mode == Mode.Carried && grabbedBy.Count > 0 && grabbedBy[0]?.grabber is Player p && p.swallowAndRegurgitateCounter > 50 && p.objectInStomach is null && p.input[0].pckp)
        {
            var num3 = -1;
            var grs = p.grasps;
            for (var k = 0; k < grs.Length; k++)
            {
                if (grs[k] is Creature.Grasp g && p.CanBeSwallowed(g.grabbed))
                {
                    num3 = k;
                    break;
                }
            }
            if (num3 >= 0 && p.grasps[num3] is Creature.Grasp g2 && g2.grabbed == this)
                flag = true;
        }
        Swallowed = Custom.LerpAndTick(Swallowed, flag ? 1f : 0f, .05f, .05f);
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.chunk is not BodyChunk b || !b.owner.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject))
            return false;
        b.vel += firstChunk.vel * .1f / b.mass;
        base.HitSomething(result, eu);
        Explode();
        return true;
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        if (room is Room rm)
        {
            rm.AddObject(new FumeFruitCloud(firstChunk.pos, Custom.RNV() * Random.value + throwDir.ToVector2() * 10f, .5f, null, -1, null, abstractPhysicalObject.rippleLayer));
            rm.PlaySound(SoundID.Slugcat_Throw_Puffball, firstChunk);
        }
    }

    public override void PickedUp(Creature upPicker) => room?.PlaySound(SoundID.Slugcat_Pick_Up_Puffball, firstChunk);

    public override void HitWall()
    {
        Explode();
        SetRandomSpin();
        ChangeMode(Mode.Free);
        forbiddenToPlayer = 10;
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        Explode();
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        Explode();
    }

    public virtual void Explode()
    {
        if (slatedForDeletetion)
            return;
        InsectCoordinator? smallInsects = null;
        var uads = room.updateList;
        for (var i = 0; i < uads.Count; i++)
        {
            if (uads[i] is InsectCoordinator coord)
            {
                smallInsects = coord;
                break;
            }
        }
        var fc = firstChunk;
        for (var j = 0; j < 70; j++)
            room.AddObject(new FumeFruitCloud(fc.pos, Custom.RNV() * Random.value * 10f, 1f, thrownBy?.abstractCreature, j % 20, smallInsects, abstractPhysicalObject.rippleLayer));
        room.AddObject(new FumeFruitCloud.CloudVisionObscurer(fc.pos, abstractPhysicalObject.rippleLayer));
        for (var k = 0; k < 6; k++)
            room.AddObject(new PuffBallSkin(fc.pos, Custom.RNV() * Random.value * 16f, color, color));
        room.PlaySound(SoundID.Puffball_Eplode, fc);
        Destroy();
    }

    public static void Explode(Player player)
    {
        if (player?.room is not Room room)
            return;
        InsectCoordinator? smallInsects = null;
        var uads = room.updateList;
        for (var i = 0; i < uads.Count; i++)
        {
            if (uads[i] is InsectCoordinator coord)
            {
                smallInsects = coord;
                break;
            }
        }
        var rippleLayer = player.abstractPhysicalObject.rippleLayer;
        var pos = player.mainBodyChunk.pos;
        for (var j = 0; j < 70; j++)
            room.AddObject(new FumeFruitCloud(pos, Custom.RNV() * Random.value * 10f, 1f, player.abstractCreature, j % 20, smallInsects, rippleLayer));
        room.AddObject(new FumeFruitCloud.CloudVisionObscurer(pos, rippleLayer));
        room.PlaySound(SoundID.Puffball_Eplode, pos, player.abstractPhysicalObject);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (ModManager.MMF && room.game.session is ArenaGameSession sess && (MMF.cfgSandboxItemStems.Value || sess.chMeta is not null) && sess.counter < 10)
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            placeRoom.AddObject(MyStalk = new(this, placeRoom));
        }
        else if (!AbstrCons.isConsumed && AbstrCons.placedObjectIndex >= 0 && AbstrCons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrCons.placedObjectIndex].pos);
            placeRoom.AddObject(MyStalk = new(this, placeRoom));
        }
        else
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        rotationSpeed = 0f;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("FumeFruitShape"),
            new("FumeFruitGradient")
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var fc = firstChunk;
        var pos = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var sprs = sLeaser.sprites;
        var rot = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.SetPosition(pos - camPos);
            spr.scale = .75f - Swallowed * .375f;
            spr.rotation = rot;
        }
        if (blink > 0 && Random.value < .5f)
            sprs[1].color = Color.white;
        else
            sprs[1].color = color;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Color.Lerp(new(251f / 255f, 231f / 255f, 14f / 255f), palette.blackColor, Darkness * .2f);
        sLeaser.sprites[0].color = palette.blackColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var sprite = sprs[i];
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
    }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st)
        {
            if (st.Fruit is FumeFruit f)
            {
                f.AbstrCons.Consume();
                st.Fruit = null;
            }
            MyStalk = null;
        }
    }
}