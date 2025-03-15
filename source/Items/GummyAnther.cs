using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Items;

public class GummyAnther : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public GummyAnther? Fruit;
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, FruitPos;

        public Stalk(GummyAnther fruit, Room room)
        {
            Fruit = fruit;
            FruitPos = fruit.firstChunk.pos;
            base.room = room;
            RootPos = Fruit.PObjPos;
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
            base.Update(eu);
            var segs = Segments;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                seg[1] = seg[0];
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                else if (i == segs.Length - 1 && Fruit is not null)
                {
                    seg[0] = Fruit.firstChunk.pos;
                    seg[2] *= 0f;
                }
                else
                {
                    seg[0] += seg[2];
                    seg[2] *= .7f;
                    seg[2].y += .3f;
                    seg[2] += Direction * .4f * (1f - (i + 1f) / segs.Length);
                }
                if (i < segs.Length - 1)
                {
                    var normalized = (seg[0] - segs[i + 1][0]).normalized;
                    float num = 15f, num2 = Vector2.Distance(seg[0], segs[i + 1][0]);
                    seg[0] += normalized * (num - num2) * .5f;
                    seg[2] += normalized * (num - num2) * .5f;
                    segs[i + 1][0] -= normalized * (num - num2) * .5f;
                    segs[i + 1][2] -= normalized * (num - num2) * .5f;
                }
                if (i < segs.Length - 2)
                {
                    var normalized2 = (seg[0] - segs[i + 2][0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    segs[i + 2][2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            if (Fruit is GummyAnther b)
            {
                var chunk = b.firstChunk;
                if (!Custom.DistLess(FruitPos, chunk.pos, b.grabbedBy.Count == 0 ? 100f : 20f) || b.room != room || b.slatedForDeletetion || chunk.vel.magnitude > 15f)
                {
                    b.AbstrCons.Consume();
                    b.MyStalk = null;
                    Fruit = null;
                }
                else
                {
                    chunk.vel.y += b.gravity;
                    chunk.vel *= .6f;
                    chunk.vel += (FruitPos - chunk.pos) / 20f;
                    b.SetRotation = Custom.DirVec(segs[segs.Length - 2][0], chunk.pos);
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
            var segs = Segments;
            var vector = Vector2.Lerp(segs[0][1], segs[0][0], timeStacker);
            var s0 = (TriangleMesh)sLeaser.sprites[0];
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                Vector2 vector2 = Vector2.Lerp(seg[1], seg[0], timeStacker), normalized = (vector2 - vector).normalized, vector3 = Custom.PerpendicularVector(normalized);
                var num = Vector2.Distance(vector2, vector) / 4f;
                s0.MoveVertice(i * 4, vector - vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 1, vector + vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 2, vector2 - vector3 * .8f - normalized * num - camPos);
                s0.MoveVertice(i * 4 + 3, vector2 + vector3 * .8f - normalized * num - camPos);
                if (i > Segments.Length - Segments.Length * .3f)
                    s0.verticeColors[i * 4] = s0.verticeColors[i * 4 + 1] = s0.verticeColors[i * 4 + 2] = s0.verticeColors[i * 4 + 3] = Color.Lerp(AntherCol, rCam.currentPalette.blackColor, rCam.currentPalette.darkness * .4f);
                vector = vector2;
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => ((TriangleMesh)sLeaser.sprites[0]).color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
        {
            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[0]);
        }
    }

    public Stalk? MyStalk;
    public Vector2 Rotation, LastRotation, PObjPos;
    public Vector2? SetRotation;
    public float Darkness, LastDarkness;
    public int Bites = 2;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public virtual bool StalkActive => MyStalk is not null;

    public GummyAnther(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 5f, .2f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .2f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1.1f;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fs = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fs.vel += Custom.DirVec(fs.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        LastRotation = Rotation;
        if (grabbedBy.Count > 0)
        {
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fs.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Math.Abs(Rotation.y);
        }
        if (SetRotation.HasValue)
        {
            Rotation = SetRotation.Value;
            SetRotation = null;
        }
        if (fs.ContactPoint.y < 0)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fs.vel.x).normalized;
            fs.vel.x *= .8f;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        Rotation = Custom.RNV();
        LastRotation = Rotation;
        if (!AbstrCons.isConsumed && StationFruit.TryGetValue(AbstrCons, out var fprops) && fprops.Plant is AbstractConsumable cons)
        {
            if (cons.placedObjectIndex >= 0 && cons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
            {
                var obj = placeRoom.roomSettings.placedObjects[cons.placedObjectIndex];
                PObjPos = obj.pos;
                firstChunk.HardSetPosition(new(PObjPos.x + (Random.value - Random.value) * 15f, PObjPos.y + 70f + Random.value * 40f - Random.value * 20f));
                var i = 0;
                while (placeRoom.GetTile(firstChunk.pos).Solid && i < 3)
                    firstChunk.HardSetPosition(firstChunk.pos with { y = firstChunk.pos.y - 20f });
            }
            else if (StationPlant.TryGetValue(cons, out var flowerData) && flowerData.DevSpawn)
            {
                // Extra shenanigans since dev console integration throws things for a loop
                PObjPos = room.MiddleOfTile(cons.pos);
                firstChunk.HardSetPosition(new(PObjPos.x + (Random.value - Random.value) * 15f, PObjPos.y + 70f + Random.value * 40f - Random.value * 20f));
                var i = 0;
                while (placeRoom.GetTile(firstChunk.pos).Solid && i < 3)
                    firstChunk.HardSetPosition(firstChunk.pos with { y = firstChunk.pos.y - 20f });
            }
            else
                firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            placeRoom.AddObject(MyStalk = new(this, placeRoom));
        }
        else
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("BigStationPlantFruit2")];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
            rotat = Vector3.Slerp(LastRotation, Rotation, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var spr = sLeaser.sprites[0];
        spr.x = pos.x - camPos.x;
        spr.y = pos.y - camPos.y;
        spr.rotation = Custom.VecToDeg(rotat);
        if (Bites > 0)
            spr.element = Futile.atlasManager.GetElementWithName("BigStationPlantFruit" + Bites.ToString());
        if (blink > 0 && Random.value < .5f)
            spr.color = blinkColor;
        else
            spr.color = color;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => color = Color.Lerp(AntherCol, palette.blackColor, Darkness * .4f);

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        sLeaser.sprites[0].RemoveFromContainer();
        newContainer.AddChild(sLeaser.sprites[0]);
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        room.PlaySound(Bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer() { }
}