using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class BouncingMelon : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public BouncingMelon? Fruit;
        public int ReleaseCounter;
        public float StalkLength, SinSide, SinCycles, ConnRad;
        public Vector2[][] Segs;
        public Vector2 StuckPos, FruitPos, StalkDirVec, BaseDirVec;
        public float Coil, CoilGoal, CoilGoalGoal, CoilSin, CoilSinMode, CoilSinModeGoal, CoilSinSpeed;

        public Stalk(BouncingMelon fruit, Room room, Vector2 fruitPos)
        {
            Fruit = fruit;
            FruitPos = fruitPos;
            fruit.firstChunk.HardSetPosition(fruitPos);
            var state = Random.state;
            Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
            StalkDirVec = Custom.DegToVec(Mathf.Lerp(30f, 110f, Random.value) * (Random.value >= .5f ? 1f : -1f));
            SinSide = Random.value >= .5f ? 1f : -1f;
            StuckPos.x = fruitPos.x;
            StalkLength = -1f;
            var stalkList = new List<Stalk>();
            var ul = room.updateList;
            for (var i = 0; i < ul.Count; i++)
            {
                if (ul[i] is BouncingMelon fru && fru.MyStalk is Stalk sta && sta != this)
                    stalkList.Add(sta);
            }
            var tl = Room.StaticGetTilePosition(fruitPos);
            var x1 = tl.x;
            for (var y = tl.y; y >= 0; --y)
            {
                if (room.GetTile(x1, y).Solid)
                {
                    var flag = false;
                    if (stalkList.Count > 0)
                    {
                        for (var index1 = -1; index1 < 2 && !flag; index1 += 2)
                        {
                            for (var index2 = Mathf.Min(20, Mathf.Abs(tl.y - y)); index2 >= 0; --index2)
                            {
                                if (room.GetTile(x1 + index1, y).Solid && !room.GetTile(x1 + index1, y + 1).Solid)
                                {
                                    for (var i = 0; i < stalkList.Count; i++)
                                    {
                                        var stalk = stalkList[i];
                                        if (Room.StaticGetTilePosition(stalk.StuckPos) == new IntVector2(x1, y))
                                        {
                                            flag = true;
                                            StuckPos = stalk.StuckPos;
                                            StalkLength = Math.Abs(StuckPos.y - fruitPos.y) * 1.10000002384186f + 30f;
                                            break;
                                        }
                                    }
                                    if (!flag)
                                        x1 += index1;
                                    else
                                        break;
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        var x2 = tl.x;
                        for (var index = Random.Range(0, Mathf.Min(20, Mathf.Abs(Room.StaticGetTilePosition(fruitPos).y - y))); index >= 0; --index)
                        {
                            var intSn = (int)SinSide;
                            if (room.GetTile(x2 + intSn, y).Solid && !room.GetTile(x2 + intSn, y + 1).Solid)
                                x2 += intSn;
                        }
                        StuckPos = room.MiddleOfTile(x2, y) + new Vector2(Mathf.Lerp(-5f, 5f, Random.value), 5f);
                        StalkLength = Math.Abs(StuckPos.y - fruitPos.y) * 1.10000002384186f + 30f;
                    }
                    break;
                }
            }
            BaseDirVec = Custom.DirVec(StuckPos, fruitPos);
            var segs = Segs = new Vector2[Mathf.Max(1, (int)(StalkLength / 8f))][];
            for (var index = 0; index < segs.Length; ++index)
            {
                var seg = segs[index] = new Vector2[3];
                var t = index / (float)(Segs.Length - 1);
                seg[0] = Vector2.Lerp(StuckPos, fruitPos, t);
                seg[1] = seg[0];
            }
            ConnRad = StalkLength / Mathf.Pow(segs.Length, 1.1f);
            SinCycles = StalkLength / 40f * Mathf.Lerp(.75f, 1.25f, Random.value);
            Random.state = state;
            CoilGoalGoal = Random.value;
            CoilGoal = CoilGoalGoal;
            CoilSinSpeed = Random.value;
            CoilSinModeGoal = Random.value;
            CoilSinMode = CoilSinModeGoal;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (StalkLength == -1f)
            {
                if (Fruit is BouncingMelon mel)
                    mel.MyStalk = null;
                Destroy();
            }
            else
            {
                var segs = Segs;
                var lastSeg = segs[segs.Length - 1];
                if (Fruit is BouncingMelon fruit)
                {
                    fruit.firstChunk.vel.y += Fruit.gravity;
                    fruit.firstChunk.vel *= .8f;
                    fruit.firstChunk.vel -= (fruit.firstChunk.pos - FruitPos) / 30f;
                    fruit.firstChunk.vel.y += 1.05f;
                    lastSeg[2].y -= 1.95f;
                }
                else
                {
                    lastSeg[2] -= (BaseDirVec + StalkDirVec) * 2f;
                    lastSeg[2] -= (lastSeg[0] - FruitPos) / 30f;
                }
                for (var index = 0; index < segs.Length; ++index)
                    segs[index][1] = segs[index][0];
                ConnectSegments(true);
                ConnectSegments(false);
                var vector2_1 = Custom.PerpendicularVector(BaseDirVec) * SinSide;
                for (var index = 0; index < segs.Length; ++index)
                {
                    var seg = segs[index];
                    var num = index / (float)(segs.Length - 1);
                    var t = Mathf.Pow(Coil, Mathf.Lerp(.5f, 3.5f, num));
                    seg[0] += seg[2];
                    seg[2] *= .94f;
                    seg[2].y += 3.59999990463257f * Mathf.Pow(Mathf.Clamp(1f - Mathf.Sin(num * Mathf.PI), 0f, 1f), 3f) * (1f - t);
                    seg[2] += Custom.DirVec(seg[0], StuckPos) * Mathf.Lerp(-.5f, .7f, t) * num;
                    seg[2] += Mathf.Sin(num * Mathf.PI * Mathf.Lerp(1.25f, .75f, t) * SinCycles) * vector2_1 * Mathf.Lerp(3f, 5f, t) * Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI);
                    seg[2] -= BaseDirVec * Mathf.Pow(Mathf.InverseLerp(.75f, 1f, num), .5f) * 1.5f;
                    seg[2].y += Mathf.InverseLerp(StuckPos.y + StalkLength / 4f, StuckPos.y, seg[0].y) * 5f;
                    if (index > 1)
                    {
                        var vector2_2 = Custom.DirVec(seg[0], Segs[index - 2][0]);
                        seg[2] -= vector2_2 * .6f;
                        segs[index - 2][2] += vector2_2 * .6f;
                    }
                }
                Coil = Custom.LerpAndTick(Coil, Mathf.Lerp(CoilGoal, .5f + .5f * Mathf.Sin(CoilSin), Custom.SCurve(CoilSinMode, .2f)), .06f, 1f / Mathf.Lerp(StalkLength, 40f, .75f));
                CoilSin += Mathf.Lerp(.007f, .013f, CoilSinSpeed);
                CoilGoal = Custom.LerpAndTick(CoilGoal, CoilGoalGoal, 1f / 1000f, .008333334f);
                CoilSinMode = Custom.LerpAndTick(CoilSinMode, CoilSinModeGoal, 1f / 1000f, 1f / 720f);
                if (Random.value < 1f / 300f)
                    CoilGoalGoal = Fruit is null ? 1f : Random.value;
                if (Random.value < 1f / 300f)
                    CoilSinSpeed = Fruit is null ? 0f : Random.value;
                if (Random.value < 1f / 300f)
                    CoilSinModeGoal = Fruit is null ? 0f : Random.value;
                ConnectSegments(false);
                ConnectSegments(true);
                if (ReleaseCounter > 0)
                    --ReleaseCounter;
                if (Fruit is BouncingMelon fruit2)
                {
                    fruit2.SetRotation = -Custom.DirVec(lastSeg[0], fruit2.firstChunk.pos);
                    if (ReleaseCounter != 1 && Custom.DistLess(fruit2.firstChunk.pos, StuckPos, StalkLength * 1.39999997615814f + 10f) && fruit2.grabbedBy.Count <= 0 && !fruit2.slatedForDeletetion && fruit2.room == room && room.VisualContact(StuckPos + new Vector2(0f, 10f), fruit2.firstChunk.pos))
                        return;
                    fruit2.AbstrCons.Consume();
                    fruit2.MyStalk = null;
                    Fruit = null;
                }
            }
        }

        public virtual void ConnectSegments(bool dir)
        {
            var segs = Segs;
            var index = !dir ? segs.Length - 1 : 0;
            var flag = false;
            while (!flag)
            {
                var seg = segs[index];
                if (index == 0)
                {
                    if (!Custom.DistLess(seg[0], StuckPos, ConnRad))
                    {
                        var vector2 = Custom.DirVec(seg[0], StuckPos) * (Vector2.Distance(seg[0], StuckPos) - ConnRad);
                        seg[0] += vector2;
                        seg[2] += vector2;
                    }
                }
                else
                {
                    var segm1 = Segs[index - 1];
                    if (!Custom.DistLess(seg[0], segm1[0], ConnRad))
                    {
                        var vector2 = Custom.DirVec(seg[0], segm1[0]) * (Vector2.Distance(seg[0], segm1[0]) - ConnRad);
                        seg[0] += vector2 * .5f;
                        seg[2] += vector2 * .5f;
                        segm1[0] -= vector2 * .5f;
                        segm1[2] -= vector2 * .5f;
                    }
                    if (index == segs.Length - 1 && Fruit is BouncingMelon fruit)
                    {
                        var vector2 = Custom.DirVec(seg[0], fruit.firstChunk.pos) * (Vector2.Distance(seg[0], fruit.firstChunk.pos) - 12f);
                        seg[0] += vector2 * .65f;
                        seg[2] += vector2 * .65f;
                        fruit.firstChunk.vel -= vector2 * .35f;
                    }
                }
                index += !dir ? -1 : 1;
                if (dir && index >= segs.Length)
                    flag = true;
                else if (!dir && index < 0)
                    flag = true;
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segs.Length, false, true)];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var vector2_1 = StuckPos;
            var num1 = 1.5f;
            var segs = Segs;
            for (var val = 0; val < segs.Length; ++val)
            {
                var f = val / (float)(segs.Length - 1);
                var seg = segs[val];
                var num2 = Mathf.Lerp(Custom.LerpMap(val, 1f, 5f, 4f, 1f), Mathf.Lerp(StalkLength / 40f, 3f, .5f), Mathf.Sin(Mathf.Pow(f, Mathf.Lerp(3f / segs.Length, .125f, .5f)) * Mathf.PI));
                var vector2_2 = Vector2.Lerp(seg[1], seg[0], timeStacker);
                if (val == segs.Length - 1 && Fruit is BouncingMelon fruit)
                    vector2_2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker) - fruit.GetRotat(timeStacker) * 6f;
                var vector2_3 = Custom.PerpendicularVector((vector2_1 - vector2_2).normalized);
                vector2_2 = new(Mathf.Floor(vector2_2.x) + .5f, Mathf.Floor(vector2_2.y) + .5f);
                var mesh = (TriangleMesh)sLeaser.sprites[0];
                mesh.MoveVertice(val * 4, vector2_1 - vector2_3 * num1 - camPos);
                mesh.MoveVertice(val * 4 + 1, vector2_1 + vector2_3 * num1 - camPos);
                mesh.MoveVertice(val * 4 + 2, vector2_2 - vector2_3 * num2 - camPos);
                mesh.MoveVertice(val * 4 + 3, vector2_2 + vector2_3 * num2 - camPos);
                vector2_1 = vector2_2;
                num1 = num2;
            }
            if (!slatedForDeletetion && room == rCam.room)
                return;
            sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[0].color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            var spr = sLeaser.sprites[0];
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Background").AddChild(spr);
        }
    }

    public Stalk? MyStalk;
    public Vector2 Rotation, LastRotation;
    public float Darkness, LastDarkness;
    public Vector2? SetRotation;
    public Color GreenCol;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual int BitesLeft => 1;

    public virtual int FoodPoints => 0;

    public virtual bool Edible => false;

    public virtual bool AutomaticPickUp => false;

    public override float ThrowPowerFactor => .8f;

    public virtual bool StalkActive => MyStalk is not null;

    public BouncingMelon(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 9f, .25f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .8f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1.1f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        GreenCol = new(Mathf.Lerp(39f / 255f, 29f / 255f, Random.value), 130f / 255f, Mathf.Lerp(29f / 255f, 59f / 255f, Random.value));
        Random.state = state;
    }

    public virtual Vector2 GetRotat(float timeStacker) => Vector3.Slerp(LastRotation, Rotation, timeStacker);

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fc.vel += Custom.DirVec(fc.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        LastRotation = Rotation;
        if (grabbedBy.Count > 0)
        {
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Math.Abs(Rotation.y);
        }
        if (SetRotation is Vector2 vec)
        {
            Rotation = vec;
            SetRotation = null;
        }
        if (fc.ContactPoint != default)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
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
            Rotation = Custom.RNV();
            LastRotation = Rotation;
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = Random.Range(10, 20);
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("ThornyStrawberry0A")
            {
                scaleX = 1.2f,
                scaleY = 1.3f
            },
            new("ThornyStrawberry0B")
            {
                scaleX = 1.2f,
                scaleY = 1.3f
            }
        ];
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
            v = Vector3.Slerp(LastRotation, Rotation, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.SetPosition(pos - camPos);
            spr.rotation = Custom.VecToDeg(v);
        }
        if (blink > 0 && Random.value < .5f)
            sprs[1].color = blinkColor;
        else
            sprs[1].color = color;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = palette.blackColor;
        color = Color.Lerp(GreenCol, palette.blackColor, Darkness);
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        if (grasp.grabber is ChipChop ch)
        {
            ch.BouncingMelonEffectDuration = 5000;
            room.PlaySound(SoundID.Slugcat_Swallow_Item, firstChunk);
            room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk);
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer() { }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = 2;
    }
}