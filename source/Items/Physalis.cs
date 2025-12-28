using UnityEngine;
using RWCustom;
using MoreSlugcats;

namespace LBMergedMods.Items;

public class Physalis : PlayerCarryableItem, IPlayerEdible, IDrawable, IHaveAStalkState, IHaveAStalk
{
    public class Stem : UpdatableAndDeletable, IDrawable
    {
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, FruitPos, Rotation;
        public bool Closed;

        public Stem(Vector2 fruitPos, Room room, bool closed)
        {
            FruitPos = fruitPos with { y = fruitPos.y - 10f };
            Closed = closed;
            base.room = room;
            var tilePosition = Room.StaticGetTilePosition(fruitPos);
            while (tilePosition.y < room.TileHeight && !room.GetTile(tilePosition).Solid)
                ++tilePosition.y;
            if (tilePosition.y == room.TileHeight)
            {
                Segments = [];
                Destroy();
            }
            else
            {
                RootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, 10f);
                var segs = Segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(fruitPos, RootPos) / 15f), 4, 60)][];
                for (var i = 0; i < segs.Length; i++)
                {
                    var seg = Vector2.Lerp(RootPos, fruitPos, (float)i / segs.Length);
                    segs[i] = [seg, seg, default];
                }
                Direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(fruitPos.x + fruitPos.y))));
                for (var j = 0; j < 100; j++)
                    Update(false);
            }
        }

        public override void Update(bool eu)
        {
            if (slatedForDeletetion)
                return;
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
                else if (i == segments.Length - 1)
                {
                    seg[0] = FruitPos;
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
                    var normalized = (seg[0] - segip1[0]).normalized;
                    float num = 15f,
                        num2 = Vector2.Distance(seg[0], segip1[0]);
                    seg[0] += normalized * (num - num2) * .5f;
                    seg[2] += normalized * (num - num2) * .5f;
                    segip1[0] -= normalized * (num - num2) * .5f;
                    segip1[2] -= normalized * (num - num2) * .5f;
                }
                if (i < segments.Length - 2)
                {
                    Vector2 normalized2 = (seg[0] - segments[i + 2][0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    segments[i + 2][2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            Rotation = Custom.DirVec(segments[segments.Length - 2][0], FruitPos);
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segments.Length, false, false), new(Closed ? "M4RClosedPhysalisGraf" : "M4ROpenPhysalisGraf") { scaleY = -1f }, new(Closed ? "M4RClosedPhysalisGrad" : "M4ROpenPhysalisGrad") { scaleY = -1f }];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var sprites = sLeaser.sprites;
            var segments = Segments;
            var vector = Vector2.Lerp(segments[0][1], segments[0][0], timeStacker);
            var s0 = (TriangleMesh)sprites[0];
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                Vector2 vector2 = Vector2.Lerp(segment[1], segment[0], timeStacker), normalized = (vector2 - vector).normalized, vector3 = Custom.PerpendicularVector(normalized);
                var num = Vector2.Distance(vector2, vector) / 4f;
                s0.MoveVertice(i * 4, vector - vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 1, vector + vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 2, vector2 - vector3 * .8f - normalized * num - camPos);
                s0.MoveVertice(i * 4 + 3, vector2 + vector3 * .8f - normalized * num - camPos);
                vector = vector2;
            }
            for (var i = 1; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                sprite.SetPosition(FruitPos - camPos);
                sprite.rotation = Custom.VecToDeg(Rotation);
            }
            sprites[2].alpha = 1f - rCam.currentPalette.darkness * .25f;
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var sprites = sLeaser.sprites;
            sprites[1].color = ((TriangleMesh)sprites[0]).color = palette.blackColor;
            sprites[2].color = new(Closed ? 245f / 255f : 195f / 255f, 151f / 255f, 20f / 255f);
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer = rCam.ReturnFContainer("Items");
            var sprites = sLeaser.sprites;
            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                sprite.RemoveFromContainer();
                newContainer.AddChild(sprite);
            }
        }
    }

    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public Physalis? Fruit;
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, FruitPos, Rotation;
        public bool Kill;

        public Stalk(Physalis fruit, Room room)
        {
            Fruit = fruit;
            FruitPos = fruit.firstChunk.pos;
            FruitPos.y -= 10f;
            base.room = room;
            var tilePosition = Room.StaticGetTilePosition(fruit.firstChunk.pos);
            while (tilePosition.y < room.TileHeight && !room.GetTile(tilePosition).Solid)
                ++tilePosition.y;
            if (tilePosition.y == room.TileHeight)
                Kill = true;
            RootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, 10f);
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
                if (Fruit is Physalis ph)
                    ph.MyStalk = null;
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
                else if (i == segments.Length - 1)
                {
                    seg[0] = FruitPos;
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
                    var normalized = (seg[0] - segip1[0]).normalized;
                    float num = 15f,
                        num2 = Vector2.Distance(seg[0], segip1[0]);
                    seg[0] += normalized * (num - num2) * .5f;
                    seg[2] += normalized * (num - num2) * .5f;
                    segip1[0] -= normalized * (num - num2) * .5f;
                    segip1[2] -= normalized * (num - num2) * .5f;
                }
                if (i < segments.Length - 2)
                {
                    Vector2 normalized2 = (seg[0] - segments[i + 2][0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    segments[i + 2][2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            if (Fruit is Physalis f)
            {
                var chunk = f.firstChunk;
                if (!Custom.DistLess(FruitPos with { y = FruitPos.y + 10f }, chunk.pos, f.grabbedBy.Count == 0 ? 40f : 4f) || f.room != room || f.slatedForDeletetion || chunk.vel.magnitude > 15f)
                {
                    chunk.mass = .07f;
                    f.AbstrCons.Consume();
                    f.MyStalk = null;
                    Fruit = null;
                }
                else
                {
                    chunk.vel.y += f.gravity;
                    chunk.vel *= .6f;
                    chunk.vel += (FruitPos with { y = FruitPos.y + 10f } - chunk.pos) / 20f;
                    chunk.mass = 50f;
                }
            }
            Rotation = Custom.DirVec(segments[segments.Length - 2][0], FruitPos);
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segments.Length, false, false), new("M4ROpenPhysalisGraf") { scaleY = - 1f }, new("M4ROpenPhysalisGrad") { scaleY = -1f }];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var sprites = sLeaser.sprites;
            var segments = Segments;
            var vector = Vector2.Lerp(segments[0][1], segments[0][0], timeStacker);
            var s0 = (TriangleMesh)sprites[0];
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                Vector2 vector2 = Vector2.Lerp(segment[1], segment[0], timeStacker), normalized = (vector2 - vector).normalized, vector3 = Custom.PerpendicularVector(normalized);
                var num = Vector2.Distance(vector2, vector) / 4f;
                s0.MoveVertice(i * 4, vector - vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 1, vector + vector3 * .8f + normalized * num - camPos);
                s0.MoveVertice(i * 4 + 2, vector2 - vector3 * .8f - normalized * num - camPos);
                s0.MoveVertice(i * 4 + 3, vector2 + vector3 * .8f - normalized * num - camPos);
                vector = vector2;
            }
            for (var i = 1; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                sprite.SetPosition(FruitPos - camPos);
                sprite.rotation = Custom.VecToDeg(Rotation);
            }
            sprites[2].alpha = 1f - rCam.currentPalette.darkness * .25f;
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var sprites = sLeaser.sprites;
            sprites[1].color = sprites[0].color = palette.blackColor;
            sprites[2].color = new(195f / 255f, 151f / 255f, 20f / 255f);
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer = rCam.ReturnFContainer("Items");
            var sprites = sLeaser.sprites;
            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                sprite.RemoveFromContainer();
                newContainer.AddChild(sprite);
            }
        }
    }

    public Stalk? MyStalk;
    public float Darkness, ColorAdd, AlphaRemove;

    public virtual int BitesLeft => 1;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public override float ThrowPowerFactor => .5f;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual bool StalkActive => MyStalk is not null;

    public Physalis(AbstractPhysicalObject obj) : base(obj)
    {
        bodyChunks = [new(this, 0, default, 5f, .07f) { loudness = .8f }];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .4f;
        surfaceFriction = .4f;
        collisionLayer = 2;
        waterFriction = .98f;
        buoyancy = 1.05f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        ColorAdd = Random.value / 5f;
        AlphaRemove = 1f - Random.value / 5f;
        Random.state = state;
    }

    public override void HitByWeapon(Weapon weapon)
    {
        firstChunk.mass = .07f;
        base.HitByWeapon(weapon);
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
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var ch = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            ch.vel += Custom.DirVec(ch.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        var flag = grabbedBy.Count < 1;
        ch.collideWithTerrain = ch.collideWithObjects = flag;
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("JetFishEyeA"), new("tinyStar")];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprs = sLeaser.sprites;
        color = Color.Lerp(PhysalisColor, Color.red, ColorAdd);
        var vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
        sprs[1].x = vector.x - camPos.x;
        sprs[1].y = vector.y - camPos.y + 1.5f;
        sprs[0].x = vector.x - camPos.x;
        sprs[0].y = vector.y - camPos.y;
        if (blink > 0 && Random.value < .5f)
            sprs[0].color = sprs[1].color = blinkColor;
        else
        {
            sprs[0].color = Custom.RGB2RGBA(color * Mathf.Lerp(1f, .2f, Darkness), 1f);
            sprs[1].color = Color.Lerp(Custom.RGB2RGBA(color * Mathf.Lerp(1.3f, .5f, Darkness), 1f), Color.white, Mathf.Lerp(.5f, .2f, Darkness) * AlphaRemove);
        }
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer = rCam.ReturnFContainer("Items");
        var spr = sLeaser.sprites[0];
        spr.RemoveFromContainer();
        newContainer.AddChild(spr);
        spr = sLeaser.sprites[1];
        spr.RemoveFromContainer();
        newContainer.AddChild(spr);
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk, false, 1f, 1.2f);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        (grasp.grabber as Player)?.ObjectEaten(this);
        grasp.Release();
        Destroy();
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        firstChunk.mass = .07f;
        base.Grabbed(grasp);
    }

    public virtual void ThrowByPlayer() { }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st)
        {
            if (st.Fruit is Physalis ph)
            {
                ph.AbstrCons.Consume();
                st.Fruit = null;
            }
            MyStalk = null;
        }
    }
}