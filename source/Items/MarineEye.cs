using MoreSlugcats;
using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class MarineEye : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public MarineEye? Fruit;
        public int ReleaseCounter;
        public float StalkLength, SinSide, SinCycles, ConnRad;
        public Vector2[][] Segs;
        public Vector2 StuckPos, FruitPos, StalkDirVec, BaseDirVec;

        public Stalk(MarineEye fruit, Room room, Vector2 fruitPos)
        {
            Fruit = fruit;
            FruitPos = fruitPos;
            fruit.firstChunk.HardSetPosition(fruitPos);
            var state = Random.state;
            Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
            StalkDirVec = Custom.DegToVec(Mathf.Lerp(30f, 110f, Random.value) * (Random.value >= .5f ? 1f : -1f));
            SinSide = Random.value >= .5f ? .5f : -.5f;
            StuckPos.x = fruitPos.x;
            StalkLength = -1f;
            var tl = Room.StaticGetTilePosition(fruitPos);
            var x1 = tl.x;
            for (var y = tl.y; y >= 0; --y)
            {
                if (room.GetTile(x1, y).Solid)
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
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (StalkLength == -1f)
            {
                if (Fruit is MarineEye fruit)
                    fruit.MyStalk = null;
                Destroy();
            }
            else
            {
                var segs = Segs;
                var lastSeg = segs[segs.Length - 1];
                if (Fruit is MarineEye fruit)
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
                    seg[0] += seg[2];
                    seg[2] *= .94f;
                    seg[2].y += 3.59999990463257f * Mathf.Pow(Mathf.Clamp(1f - Mathf.Sin(num * Mathf.PI), 0f, 1f), 3f);
                    seg[2] += Custom.DirVec(seg[0], StuckPos) * (-.5f) * num;
                    seg[2] += Mathf.Sin(num * Mathf.PI * 1.25f * SinCycles) * vector2_1 * 3f * Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI);
                    seg[2] -= BaseDirVec * Mathf.Pow(Mathf.InverseLerp(.75f, 1f, num), .5f) * 1.5f;
                    seg[2].y += Mathf.InverseLerp(StuckPos.y + StalkLength / 4f, StuckPos.y, seg[0].y) * 5f;
                    if (index > 1)
                    {
                        var vector2_2 = Custom.DirVec(seg[0], Segs[index - 2][0]);
                        seg[2] -= vector2_2 * .6f;
                        segs[index - 2][2] += vector2_2 * .6f;
                    }
                }
                ConnectSegments(false);
                ConnectSegments(true);
                if (ReleaseCounter > 0)
                    --ReleaseCounter;
                if (Fruit is MarineEye fruit2)
                {
                    fruit2.SetRotation = -Custom.DirVec(lastSeg[0], fruit2.firstChunk.pos);
                    if (ReleaseCounter != 1 && Custom.DistLess(fruit2.firstChunk.pos, StuckPos, StalkLength * 1.39999997615814f + 10f) && fruit2.grabbedBy.Count == 0 && !fruit2.slatedForDeletetion && fruit2.room == room && room.VisualContact(StuckPos + new Vector2(0f, 10f), fruit2.firstChunk.pos))
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
                    if (index == segs.Length - 1 && Fruit is MarineEye fruit)
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
            var num1 = 1.25f;
            var segs = Segs;
            for (var val = 0; val < segs.Length; ++val)
            {
                var f = val / (float)(segs.Length - 1);
                var seg = segs[val];
                var num2 = Mathf.Lerp(Custom.LerpMap(val, .5f, 2.5f, 2f, .5f), Mathf.Lerp(StalkLength / 40f, 1.5f, .5f), Mathf.Sin(Mathf.Pow(f, Mathf.Lerp(3f / segs.Length, .125f, .25f)) * Mathf.PI));
                var vector2_2 = Vector2.Lerp(seg[1], seg[0], timeStacker);
                if (val == segs.Length - 1 && Fruit is MarineEye fruit)
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
    public int Bites = 3;
    public Vector2? SetRotation;
    public Color BlueCol;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => false;

    public virtual bool StalkActive => MyStalk is not null;

    public MarineEye(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 6.5f, .18f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .2f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        BlueCol = new(17f / 255f, 118f / 255f, Mathf.Lerp(200f / 255f, 240f / 255f, Random.value));
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
        if (fc.ContactPoint.y < 0)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
        }
        var crits = room.abstractRoom.creatures;
        if (fc.submersion > .5f && crits.Count > 0 && grabbedBy.Count == 0)
        {
            var abstractCreature = crits[Random.Range(0, crits.Count)];
            if (abstractPhysicalObject.SameRippleLayer(abstractCreature) && abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature is JetFish j && !abstractCreature.realizedCreature.dead && j.AI.goToFood is null && j.AI.WantToEatObject(this))
                j.AI.goToFood = this;
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
        if (weapon is Spear s && s.IsNeedle && s.Spear_NeedleCanFeed() && s.thrownBy is Player p)
        {
            p.ObjectEaten(this);
            p.AddFood(1);
            AbstrCons.Consume();
            if (PlayerData.TryGetValue(p.abstractCreature, out var props))
                props.BlueFaceDuration = 5000;
            if (room is Room rm)
            {
                rm.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk);
                for (var i = 0; i < 10; i++)
                    rm.AddObject(new Spark(firstChunk.pos, Custom.RNV(), color, null, 30, 2));
            }
            var grabbers = grabbedBy;
            for (var i = 0; i < grabbers.Count; i++)
                grabbers[i]?.Release();
            Destroy();
        }
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = Random.Range(10, 20);
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("SEATh0A")
            {
                scaleX = .4f,
                scaleY = .4f,
                anchorY = .4f
            },
            new("SEATh0B")
            {
                scaleX = .4f,
                scaleY = .4f,
                anchorY = .4f
            },
            new("SEAThC")
            {
                scaleX = .4f,
                scaleY = .4f,
                anchorY = .4f
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
            if (i < 2)
                spr.element = Futile.atlasManager.GetElementWithName("SEATh" + Custom.IntClamp(3 - Bites, 0, 2).ToString() + (i == 0 ? "A" : "B"));
        }
        if (blink > 0 && Random.value < .5f)
            sprs[1].color = blinkColor;
        else
            sprs[1].color = color;
        sprs[2].alpha = .6f - Darkness * .3f;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = palette.blackColor;
        color = Color.Lerp(BlueCol, palette.blackColor, Darkness * .5f);
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
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
            ch.MarineEyeEffectDuration = Math.Min(ch.MarineEyeEffectDuration + 1700, 5000);
        else if (grasp.grabber is Player p && PlayerData.TryGetValue(p.abstractCreature, out var props))
            props.BlueFaceDuration = Math.Min(props.BlueFaceDuration + 1700, 5000);
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

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = 2;
    }
}