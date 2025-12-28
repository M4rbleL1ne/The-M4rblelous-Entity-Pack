using UnityEngine;
using RWCustom;
using Noise;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class LittleBalloon : Rock, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public LittleBalloon? Balloon;
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, BalloonPos;
        public float ColorAdd;
        public bool Kill;

        public Stalk(LittleBalloon balloon, Room room)
        {
            Balloon = balloon;
            BalloonPos = balloon.firstChunk.pos;
            ColorAdd = balloon.ColorAdd;
            base.room = room;
            var tilePosition = Room.StaticGetTilePosition(balloon.firstChunk.pos);
            while (tilePosition.y >= 0 && !room.GetTile(tilePosition).Solid)
                --tilePosition.y;
            if (tilePosition.y < 0)
                Kill = true;
            RootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
            var segs = Segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(balloon.firstChunk.pos, RootPos) / 15f), 4, 60)][];
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = Vector2.Lerp(RootPos, balloon.firstChunk.pos, (float)i / segs.Length);
                segs[i] = [seg, seg, default];
            }
            Direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(BalloonPos.x + BalloonPos.y))));
            for (var j = 0; j < 100; j++)
                Update(false);
            balloon.ChangeCollisionLayer(0);
        }

        public override void Update(bool eu)
        {
            if (Kill)
            {
                if (Balloon is LittleBalloon ba)
                    ba.MyStalk = null;
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
                else if (i == segments.Length - 1 && Balloon is LittleBalloon balloon)
                {
                    seg[0] = balloon.firstChunk.pos;
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
                    Vector2 normalized2 = (seg[0] - Segments[i + 2][0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    Segments[i + 2][2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            if (Balloon is LittleBalloon b)
            {
                var chunk = b.firstChunk;
                if (!Custom.DistLess(BalloonPos, chunk.pos, b.grabbedBy.Count == 0 ? 100f : 20f) || b.room != room || b.slatedForDeletetion || chunk.vel.magnitude > 15f)
                {
                    b.AbstrCons.Consume();
                    b.MyStalk = null;
                    Balloon = null;
                }
                else
                {
                    chunk.vel.y += b.gravity;
                    chunk.vel *= .6f;
                    chunk.vel += (BalloonPos - chunk.pos) / 20f;
                    b.setRotation = Custom.DirVec(segments[segments.Length - 2][0], chunk.pos);
                }
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segments.Length, false, true)];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var segments = Segments;
            Vector2 vector = Vector2.Lerp(segments[0][1], segments[0][0], timeStacker);
            var s0 = (TriangleMesh)sLeaser.sprites[0];
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                Vector2 vector2 = Vector2.Lerp(segment[1], segment[0], timeStacker),
                    normalized = (vector2 - vector).normalized,
                    vector3 = Custom.PerpendicularVector(normalized);
                var num = Vector2.Distance(vector2, vector) / 4f;
                s0.MoveVertice(i * 4, vector - vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 1, vector + vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 2, vector2 - vector3 * .8f - normalized * num - camPos);
                s0.MoveVertice(i * 4 + 3, vector2 + vector3 * .8f - normalized * num - camPos);
                if (i > segments.Length - segments.Length * .3f)
                    s0.verticeColors[i * 4] = s0.verticeColors[i * 4 + 1] = s0.verticeColors[i * 4 + 2] = s0.verticeColors[i * 4 + 3] = Color.Lerp(BalloonColor, rCam.currentPalette.blackColor, .7f - ColorAdd);
                vector = vector2;
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var segmentsL = Segments.Length;
            var s0 = (TriangleMesh)sLeaser.sprites[0];
            s0.color = palette.blackColor;
            for (var i = 0; i < segmentsL; i++)
            {
                if (i > segmentsL - segmentsL * .3f)
                    s0.verticeColors[i * 4] = s0.verticeColors[i * 4 + 1] = s0.verticeColors[i * 4 + 2] = s0.verticeColors[i * 4 + 3] = Color.Lerp(BalloonColor, palette.blackColor, .7f - ColorAdd);
            }
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
        {
            var s0 = sLeaser.sprites[0];
            s0.RemoveFromContainer();
            rCam.ReturnFContainer("Items").AddChild(s0);
        }
    }

    public Stalk? MyStalk;
    public int Bites = 2, BounceCounter;
    public float Prop, LastProp, PropSpeed, Darkness, LastDarkness, Plop, LastPlop;
    public float ColorAdd, AlphaRemove;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public override bool HeavyWeapon => false;

    public virtual bool StalkActive => MyStalk is not null;

    public LittleBalloon(AbstractPhysicalObject obj) : base(obj, obj.world)
    {
        firstChunk.rad = .34f;
        bounce = .85f;
        surfaceFriction = .9f;
        collisionLayer = 1;
        waterFriction = .91f;
        airFriction = .95f;
        buoyancy = 1.2f;
        Plop = 1f;
        LastPlop = 1f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        ColorAdd = Random.value / 4f;
        AlphaRemove = Random.value / 3f;
        Random.state = state;
        gravity = .86f;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var ch = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            ch.vel += Custom.DirVec(ch.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        lastRotation = rotation;
        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(ch.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = Math.Abs(rotation.y);
        }
        if (setRotation.HasValue)
        {
            rotation = setRotation.Value;
            setRotation = null;
        }
        if (ch.ContactPoint.y < 0)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * .1f * ch.vel.x).normalized;
            ch.vel.x *= .8f;
        }
        LastProp = Prop;
        Prop += PropSpeed;
        PropSpeed *= .85f;
        PropSpeed -= Prop / 10f;
        Prop = Mathf.Clamp(Prop, -15f, 15f);
        if (grabbedBy.Count == 0)
        {
            Prop += (ch.lastPos.x - ch.pos.x) / 15f;
            Prop -= (ch.lastPos.y - ch.pos.y) / 15f;
        }
        LastPlop = Plop;
        if (Plop > 0f && Plop < 1f)
            Plop = Mathf.Min(1f, Plop + .1f);
        if (ch.submersion > 0f)
            PopHard();
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        PopHard();
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (weapon is not LittleBalloon)
            PopHard();
        else
            Pop();
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (direction.y != 0)
        {
            Prop += speed;
            PropSpeed += speed / 10f;
        }
        else
        {
            Prop -= speed;
            PropSpeed -= speed / 10f;
        }
        if (speed > 1.25f && firstContact)
        {
            var pos = firstChunk.pos + direction.ToVector2() * firstChunk.rad;
            for (var i = 0; i < Mathf.RoundToInt(Custom.LerpMap(speed, 1.2f, 6f, 2f, 5f, 1.2f)); i++)
                room.AddObject(new WaterDrip(pos, Custom.RNV() * (2f + speed) * Random.value * .5f + -direction.ToVector2() * (3f + speed) * .35f, false));
            room.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, firstChunk, false, Custom.LerpMap(speed, 1.2f, 6f, .2f, 1f), 1.3f);
            Pop();
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta is not null) && room.game.GetArenaGameSession.counter < 10)
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

    public override void HitWall()
    {
        if (room.BeingViewed)
        {
            for (var i = 0; i < 7; i++)
                room.AddObject(new Spark(firstChunk.pos + throwDir.ToVector2() * (firstChunk.rad - 1f), Custom.DegToVec(Random.value * 360f) * 10f * Random.value + -throwDir.ToVector2() * 10f, Color.white, null, 2, 4));
        }
        SetRandomSpin();
        ChangeMode(Mode.Free);
        forbiddenToPlayer = 10;
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        var flag = result.obj is Creature && result.chunk is BodyChunk b && (b.vel.magnitude > .8f || firstChunk.vel.magnitude > .8f);
        var res = base.HitSomething(result, eu);
        if (flag && res)
            PopHard();
        return res;
    }

    public virtual void Pop()
    {
        if (BounceCounter >= 6)
            PopHard();
        else
        {
            ++BounceCounter;
            var fs = firstChunk;
            var vector = Vector2.Lerp(fs.pos, fs.lastPos, .35f);
            room.AddObject(new ExplosionSpikes(room, vector, 14, 20f, 9f, 6f, 20f, BalloonColor));
            room.AddObject(new ShockWave(vector, 150f, .005f, 5));
            room.PlaySound(SoundID.Water_Nut_Swell, fs, false, 1.2f, 1.3f);
            var stu = abstractPhysicalObject.stuckObjects;
            for (var m = 0; m < stu.Count; m++)
                stu[m].Deactivate();
            room.InGameNoise(new InGameNoise(vector, 200f, this, 1f));
            if (grabbedBy.Count > 0)
                grabbedBy[0].grabber?.Stun(28);
        }
    }

    public virtual void PopHard()
    {
        var fs = firstChunk;
        var vector = Vector2.Lerp(fs.pos, fs.lastPos, .35f);
        room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 40f, BalloonColor));
        room.AddObject(new ShockWave(vector, 200f, .01f, 5));
        for (var l = 0; l < 4; l++)
            room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
        room.PlaySound(SoundID.Water_Nut_Swell, fs, false, 1.2f, 1.3f);
        var stu = abstractPhysicalObject.stuckObjects;
        for (var m = 0; m < stu.Count; m++)
            stu[m].Deactivate();
        room.InGameNoise(new InGameNoise(vector, 400f, this, 1f));
        if (grabbedBy.Count > 0)
            grabbedBy[0].grabber?.Stun(50);
        Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("lbulb_base"),
            new("lbulb_detail")
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.x = pos.x - camPos.x;
            spr.y = pos.y - camPos.y;
        }
        sprs[1].alpha = (1f - Darkness / 2f) * (1f - firstChunk.submersion) * (.7f - AlphaRemove);
        var num = Mathf.Lerp(LastPlop, Plop, timeStacker);
        num = Mathf.Lerp(0f, 1f + Mathf.Sin(num * Mathf.PI), num);
        sprs[0].scale = sprs[1].scale = (1.2f * Custom.LerpMap(Bites, 3f, 1f, 1f, .5f) + Mathf.Lerp(LastProp, Prop, timeStacker) / 20f) * num * .4f;
        if (blink > 0 && Random.value < .5f)
            sprs[0].color = Color.white;
        else
            sprs[0].color = color;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => color = Color.Lerp(BalloonColor, palette.blackColor, .7f - ColorAdd);

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

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        room.PlaySound(Bites == 0 ? SoundID.Slugcat_Eat_Water_Nut : SoundID.Slugcat_Bite_Water_Nut, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            PopHard();
        }
        PropSpeed += Mathf.Lerp(-1f, 1f, Random.value) * 7f;
        firstChunk.rad *= .5f;
    }

    public virtual void ThrowByPlayer() { }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st)
        {
            if (st.Balloon is LittleBalloon b)
            {
                b.AbstrCons.Consume();
                st.Balloon = null;
            }
            MyStalk = null;
        }
    }
}