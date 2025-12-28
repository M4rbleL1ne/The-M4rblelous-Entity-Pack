using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class ThornyStrawberry : Weapon, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public ThornyStrawberry? Fruit;
        public Vector2[] Displacements;
        public Vector2[][] Segs;
        public Vector2 StuckPos;
        public float RopeLength, ConnRad;
        public int ReleaseCounter;

        public Stalk(ThornyStrawberry fruit, Room room, Vector2 fruitPos)
        {
            Fruit = fruit;
            fruit.firstChunk.HardSetPosition(fruitPos);
            StuckPos.x = fruitPos.x;
            RopeLength = -1f;
            var fpos = Room.StaticGetTilePosition(fruitPos);
            var x = fpos.x;
            int i, height = room.TileHeight;
            for (i = fpos.y; i < height; i++)
            {
                if (room.GetTile(x, i).Solid)
                {
                    StuckPos.y = room.MiddleOfTile(x, i).y - 10f;
                    RopeLength = Math.Abs(StuckPos.y - fruitPos.y);
                    break;
                }
            }
            var segs = Segs = new Vector2[Math.Max(1, (int)(RopeLength / 7.5f))][];
            height = segs.Length;
            var lf = (float)(height - 1);
            var sp = StuckPos;
            for (i = 0; i < segs.Length; i++)
            {
                var seg = segs[i] = new Vector2[3];
                seg[0] = Vector2.Lerp(sp, fruitPos, i / lf);
                seg[1] = seg[0];
            }
            ConnRad = RopeLength / Mathf.Pow(height, 1.1f);
            var disps = Displacements = new Vector2[height];
            var state = Random.state;
            Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
            for (i = 0; i < disps.Length; i++)
                disps[i] = Custom.RNV();
            Random.state = state;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (RopeLength == -1f)
            {
                if (Fruit is ThornyStrawberry fruit1)
                    fruit1.MyStalk = null;
                Destroy();
                return;
            }
            ConnectSegments(true);
            ConnectSegments(false);
            var segs = Segs;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                seg[1] = seg[0];
                seg[0] += seg[2];
                seg[2] *= .99f;
                seg[2].y -= .9f;
            }
            ConnectSegments(false);
            ConnectSegments(true);
            if (ReleaseCounter > 0)
                --ReleaseCounter;
            if (Fruit is ThornyStrawberry fruit)
            {
                var fpos = fruit.firstChunk.pos;
                fruit.setRotation = Custom.DirVec(fpos, segs[segs.Length - 1][0]);
                if (!Custom.DistLess(fpos, StuckPos, RopeLength * 1.4f + 10f) || fruit.slatedForDeletetion || fruit.Bites < 3 || fruit.room != room || ReleaseCounter == 1)
                {
                    fruit.AbstrCons.Consume();
                    fruit.MyStalk = null;
                    Fruit = null;
                }
            }
        }

        public virtual void ConnectSegments(bool dir)
        {
            var segs = Segs;
            var sp = StuckPos;
            var crad = ConnRad;
            var l = segs.Length;
            var num = dir ? 0 : l - 1;
            var flag = false;
            while (!flag)
            {
                var seg = segs[num];
                if (num == 0)
                {
                    if (!Custom.DistLess(seg[0], sp, crad))
                    {
                        var vector = Custom.DirVec(seg[0], sp) * (Vector2.Distance(seg[0], sp) - crad);
                        seg[0] += vector;
                        seg[2] += vector;
                    }
                }
                else
                {
                    var segm1 = segs[num - 1];
                    if (!Custom.DistLess(seg[0], segm1[0], crad))
                    {
                        var vector2 = Custom.DirVec(seg[0], segm1[0]) * (Vector2.Distance(seg[0], segm1[0]) - crad) * .5f;
                        seg[0] += vector2;
                        seg[2] += vector2;
                        segm1[0] -= vector2;
                        segm1[2] -= vector2;
                    }
                    if (num == l - 1 && Fruit is ThornyStrawberry fruit && !Custom.DistLess(segm1[0], fruit.firstChunk.pos, crad))
                    {
                        var fc = fruit.firstChunk;
                        Vector2 vector3 = Custom.DirVec(seg[0], fc.pos) * (Vector2.Distance(seg[0], fc.pos) - crad);
                        seg[0] += vector3 * .75f;
                        seg[2] += vector3 * .75f;
                        fc.vel -= vector3 * .25f;
                    }
                }
                num += dir ? 1 : -1;
                if (dir && num >= l)
                    flag = true;
                else if (!dir && num < 0)
                    flag = true;
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segs.Length, false, false)];
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var vector = StuckPos;
            var num = 3f;
            var mesh = (TriangleMesh)sLeaser.sprites[0];
            var segs = Segs;
            var l = segs.Length - 1;
            var crad = ConnRad;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                float num2 = i / (float)l,
                    num3 = Custom.LerpMap(num2, 0f, .5f, 1f, 0f) + Mathf.Lerp(1f, .5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * Mathf.PI)) * 2f;
                Vector2 vector2;
                if (i == l && Fruit is ThornyStrawberry fruit)
                    vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
                else
                    vector2 = Vector2.Lerp(seg[1], seg[0], timeStacker);
                Vector2 normalized = (vector - vector2).normalized,
                    vector3 = Custom.PerpendicularVector(normalized);
                if (i < l)
                {
                    var disp = Displacements[i];
                    vector2 += (normalized * disp.y + vector3 * disp.x) * Custom.LerpMap(Vector2.Distance(vector, vector2), crad, crad * 5f, 4f, 0f);
                }
                vector2 = new Vector2(Mathf.Floor(vector2.x) + .5f, Mathf.Floor(vector2.y) + .5f);
                var i4 = i * 4;
                mesh.MoveVertice(i4, vector - vector3 * num - camPos);
                mesh.MoveVertice(i4 + 1, vector + vector3 * num - camPos);
                mesh.MoveVertice(i4 + 2, vector2 - vector3 * num3 - camPos);
                mesh.MoveVertice(i4 + 3, vector2 + vector3 * num3 - camPos);
                vector = vector2;
                num = num3;
            }
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[0].color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            var spr = sLeaser.sprites[0];
            spr.RemoveFromContainer();
            (newContainer ?? rCam.ReturnFContainer("Background")).AddChild(spr);
        }
    }

    public Stalk? MyStalk;
    public float Darkness, LastDarkness;
    public int Bites = 3;
    public Color RedCol, YellowCol;
    public Color Color2;
    public bool Side;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => SpikesRemoved();

    public virtual bool AutomaticPickUp => SpikesRemoved();

    public override int DefaultCollLayer => 1;

    public virtual bool StalkActive => MyStalk is not null;

    public ThornyStrawberry(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bodyChunks = [new(this, 0, default, 8.2f, .205f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .2f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1.1f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        YellowCol = new(1f, Mathf.Lerp(191f / 255f, 242f / 255f, Random.value), 0f);
        RedCol = new(189f / 255f, 0f, Mathf.Lerp(50f / 255f, 0f, Random.value));
        Side = Random.value > .5f;
        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fc.vel += Custom.DirVec(fc.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        if (fc.ContactPoint.y != 0 || fc.submersion > .5f)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
            rotationSpeed *= .8f;
        }
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj is not PhysicalObject obj || SpikesRemoved(out var data))
            return false;
        if (result.chunk is BodyChunk b && !abstractPhysicalObject.SameRippleLayer(b.owner.abstractPhysicalObject))
            return false;
        if (thrownBy is Scavenger scav && scav.AI is ScavengerAI AI)
            AI.HitAnObjectWithWeapon(this, obj);
        var fc = firstChunk;
        if (obj is Creature c)
        {
            var stunBonus = 45f;
            if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (obj is Cicada || obj is LanternMouse || ModManager.MSC && obj is Yeek) || ModManager.MSC && room.game.GetArenaGameSession?.chMeta is not null)
                stunBonus = 90f;
            if (result.chunk is BodyChunk ch)
            {
                c.Violence(fc, fc.vel * fc.mass * 2f, ch, result.onAppendagePos, Creature.DamageType.Stab, .85f, stunBonus);
                if (obj is Player player)
                {
                    player.playerState.permanentDamageTracking += .85f / player.Template.baseDamageResistance;
                    if (player.playerState.permanentDamageTracking >= 1.0)
                        player.Die();
                }
            }
            data.SpikesRemoved = true;
            if (room is Room rm)
                rm.AddObject(new ExplosionSpikes(room, fc.pos, 6, 2.5f, 4.5f, 6f, 35f, YellowCol with { a = .7f }));
        }
        else if (result.chunk is BodyChunk ch)
        {
            ch.vel += fc.vel * fc.mass / ch.mass;
            room.AddObject(new ExplosionSpikes(room, ch.pos + Custom.DirVec(ch.pos, result.collisionPoint) * ch.rad, 5, 2f, 4f, 4.5f, 30f, new(1f, 1f, 1f, .5f)));
        }
        else if (result.onAppendagePos is not null)
            (obj as IHaveAppendages)!.ApplyForceOnAppendage(result.onAppendagePos, fc.vel * fc.mass);
        vibrate = 20;
        ChangeMode(Mode.Free);
        fc.vel = fc.vel * -.1f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(.1f, .4f, Random.value) * fc.vel.magnitude * .2f;
        room.PlaySound(SoundID.Rock_Hit_Creature, fc);
        SetRandomSpin();
        return true;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        placeRoom.AddObject(this);
        if (ModManager.MMF && placeRoom.game.GetArenaGameSession is ArenaGameSession arena && (MMF.cfgSandboxItemStems.Value || arena.chMeta is not null) && arena.counter < 10)
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            placeRoom.AddObject(MyStalk = new(this, placeRoom, firstChunk.pos));
        }
        else if (!AbstrCons.isConsumed && AbstrCons.placedObjectIndex >= 0 && AbstrCons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrCons.placedObjectIndex].pos);
            placeRoom.AddObject(MyStalk = new(this, placeRoom, firstChunk.pos));
        }
        else
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            rotation = Custom.RNV();
            lastRotation = rotation;
        }
        SetRandomSpin();
        inFrontOfObjects = -1;
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = Random.Range(30, 50);
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        if (!SpikesRemoved(out var data))
        {
            var fc = firstChunk;
            data.SpikesRemoved = true;
            if (room is Room rm)
            {
                rm.PlaySound(SoundID.Rock_Hit_Creature, fc);
                rm.AddObject(new ExplosionSpikes(room, fc.pos, 6, 2.5f, 4.5f, 6f, 35f, YellowCol with { a = .7f }));
                SetRandomSpin();
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var scaleX = Side ? 1f : -1f;
        sLeaser.sprites = [new("ThornyStrawberry0A") { scaleX = scaleX }, new("ThornyStrawberry0B") { scaleX = scaleX }, new("ThornyStrawberry0ASP") { scaleX = scaleX }, new("ThornyStrawberry0BSP") { scaleX = scaleX }];
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
            v = Vector3.Slerp(lastRotation, rotation, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var sprs = sLeaser.sprites;
        var spk = !SpikesRemoved() && Bites == 3;
        var biteNm = Custom.IntClamp(3 - Bites, 0, 2).ToString();
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.SetPosition(pos - camPos);
            spr.rotation = Custom.VecToDeg(v);
            var nm = "ThornyStrawberry" + biteNm + (i % 2 == 0 ? "A" : "B");
            if (i > 1)
                nm += spk ? "SP" : "SN";
            spr.element = Futile.atlasManager.GetElementWithName(nm);
        }
        if (blink > 0 && Random.value < .5f)
            sprs[3].color = sprs[1].color = blinkColor;
        else
        {
            sprs[1].color = color;
            sprs[3].color = Color2;
        }
        sprs[3].alpha = spk ? 1f : .5f;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[2].color = sLeaser.sprites[0].color = palette.blackColor;
        color = Color.Lerp(RedCol, palette.blackColor, Darkness);
        Color2 = Color.Lerp(YellowCol, palette.blackColor, Darkness);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        var sprs = sLeaser.sprites;
        newContainer ??= rCam.ReturnFContainer("Items");
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        room.PlaySound(Bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer() { }

    public virtual bool SpikesRemoved() => StrawberryData.TryGetValue(AbstrCons, out var data) && data.SpikesRemoved;

    public virtual bool SpikesRemoved(out ThornyStrawberryData data) => StrawberryData.TryGetValue(AbstrCons, out data) && data.SpikesRemoved;

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = 2;
    }
}