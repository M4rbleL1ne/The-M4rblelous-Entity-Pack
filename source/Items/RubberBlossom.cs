using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Items;

public class RubberBlossom : PhysicalObject, IDrawable
{
    public const float BASESIZE = .5f;
    public float LastSizeLerperX = BASESIZE, LastSizeLerperY = BASESIZE, SizeLerperX = BASESIZE, SizeLerperY = BASESIZE, WowSpeed = .0001f, MaxUpwardVel, RndColorAlpha;
    public bool SizeLerpUp = true, SpritesDirty, Open;
    public int MoveToFrontCounter = 5;
    public float[] PetalVars;
    public Vector2 RootPos;
    public Color Color;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public RubberBlossom(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        RndColorAlpha = Random.Range(-.2f, 0f);
        PetalVars = [Random.value > .5f ? -1f : 1f, Random.value > .5f ? -1f : 1f, Random.value > .5f ? -1f : 1f, Random.value > .5f ? -1f : 1f];
        Random.state = state;
        bodyChunks = [new(this, 0, default, 15f, .5f)];
        bodyChunkConnections = [];
        CollideWithObjects = false;
        CollideWithSlopes = false;
        CollideWithTerrain = false;
        GoThroughFloors = true;
        airFriction = 0f;
        gravity = 0f;
        bounce = 0f;
        surfaceFriction = 0f;
        collisionLayer = 0;
        waterFriction = 0f;
        buoyancy = 0f;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        LastSizeLerperX = SizeLerperX;
        LastSizeLerperY = SizeLerperY;
        if (SizeLerperX >= BASESIZE + .03f)
            SizeLerpUp = false;
        else if (SizeLerperX <= BASESIZE - .03f)
            SizeLerpUp = true;
        if (SizeLerpUp)
            SizeLerperX += 5f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
        else
            SizeLerperX -= 8f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
        if (SizeLerpUp)
            SizeLerperY -= 5f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
        else
            SizeLerperY += 8f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
        var i = Open ? 0 : 1;
        if (collisionLayer != i)
            ChangeCollisionLayer(i);
        if (firstChunk.pos != RootPos)
            firstChunk.pos = RootPos;
        CollideWithObjects = !Open;
        if (WowSpeed > .0001f)
            WowSpeed = Mathf.Lerp(WowSpeed, .0001f, .025f);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;
        var ind = AbstrCons.placedObjectIndex;
        var ftrl = StationPlant.TryGetValue(AbstrCons, out var props) && props.FirstTimeRealized;
        if (ind >= 0 && ind < placeRoom.roomSettings.placedObjects.Count)
        {
            var obj = placeRoom.roomSettings.placedObjects[ind];
            firstChunk.HardSetPosition(obj.pos);
            if (obj.data is RubberBlossomData data)
            {
                if (ftrl)
                {
                    if (data.AlwaysOpen)
                        props.Open = true;
                    else if (data.AlwaysClosed)
                        props.Open = false;
                    else if (!placeRoom.game.IsStorySession)
                        props.Open = data.StartsOpen;
                    else
                        props.Open = data.StartsOpen ? !AbstrCons.isConsumed : AbstrCons.isConsumed;
                }
                Color = data.Color;
                MaxUpwardVel = data.MaxUpwardVel;
            }
            else
            {
                if (ftrl)
                    props.Open = !AbstrCons.isConsumed;
                Color = StationPlantCol;
                MaxUpwardVel = 20f;
            }
        }
        else if (props != null && props.DevSpawn)
        {
            // Have to do some extra shenanigans since we're not placing in as a placed object
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            Color = props.forceColor;
            MaxUpwardVel = props.forceMaxVel;
            Open = props.Open;

            // That includes manually spawning our anthers
            if (Open)
            {
                for (int i = 0; i < props.NumberOfFruits; i++)
                {
                    var anther = new AbstractConsumable(room.world, AbstractObjectType.GummyAnther, null, abstractPhysicalObject.pos, room.game.GetNewID(), -1, -1, null)
                    {
                        isConsumed = false
                    };
                    if (StationFruit.TryGetValue(anther, out var antherProps))
                    {
                        antherProps.Plant = AbstrCons;
                    }
                    room.abstractRoom.AddEntity(anther);
                    anther.RealizeInRoom();
                }
            }
        }
        else
        {
            if (ftrl)
                props!.Open = !AbstrCons.isConsumed;
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            Color = StationPlantCol;
            MaxUpwardVel = 20f;
        }
        SpritesDirty = true;
        RootPos = firstChunk.pos;
        if (ftrl)
        {
            props!.FirstTimeRealized = false;
            AbstrCons.Consume();
        }
        if (props is not null)
            Open = props.Open;
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk) { }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (otherObject is Player p && p.bodyChunks is BodyChunk[] bs)
        {
            var intVector = bs[1].ContactPoint;
            if (intVector.y == -1)
            {
                WowSpeed = .0025f;
                for (var i = 0; i < bs.Length; i++)
                {
                    var b = bs[i];
                    b.vel.y = Mathf.Min(b.vel.y + Math.Abs(b.vel.y) * 5f, MaxUpwardVel);
                }
            }
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        const string NM = "BigStationPlant", GR = "GR", LR = "LR";
        var cl = Open ? string.Empty : "CL";
        var sprs = sLeaser.sprites =
            [new($"{NM}{LR}4") { scaleX = .5f * PetalVars[0] },
            new($"{NM}{LR}3{cl}") { scaleX = PetalVars[1] * .5f },
            new($"{NM}{GR}3{cl}") { scaleX = PetalVars[1] * .5f },
            new($"{NM}{LR}2{cl}") { scaleX = PetalVars[1] * .5f },
            new($"{NM}{GR}2{cl}") { scaleX = PetalVars[1] * .5f },
            new($"{NM}{LR}1{cl}") { scaleX = PetalVars[2] * .5f },
            new($"{NM}{GR}1{cl}") { scaleX = PetalVars[2] * .5f },
            new($"{NM}{LR}5{cl}") { scaleX = PetalVars[3] * .5f },
            new($"{NM}{GR}5{cl}") { scaleX = PetalVars[3] * .5f }];
        for (var i = 0; i < sprs.Length; i++)
        {
            var sprite = sprs[i];
            sprite.scaleY = -.5f;
            sprite.anchorX = .5f;
            sprite.anchorY = .4f;
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void ChangeDirtySprites(RoomCamera.SpriteLeaser sLeaser)
    {
        const string NM = "BigStationPlant", GR = "GR", LR = "LR";
        var cl = Open ? string.Empty : "CL";
        var sprs = sLeaser.sprites;
        sprs[1].element = Futile.atlasManager.GetElementWithName($"{NM}{LR}3{cl}");
        sprs[2].element = Futile.atlasManager.GetElementWithName($"{NM}{GR}3{cl}");
        sprs[3].element = Futile.atlasManager.GetElementWithName($"{NM}{LR}2{cl}");
        sprs[4].element = Futile.atlasManager.GetElementWithName($"{NM}{GR}2{cl}");
        sprs[5].element = Futile.atlasManager.GetElementWithName($"{NM}{LR}1{cl}");
        sprs[6].element = Futile.atlasManager.GetElementWithName($"{NM}{GR}1{cl}");
        sprs[7].element = Futile.atlasManager.GetElementWithName($"{NM}{LR}5{cl}");
        sprs[8].element = Futile.atlasManager.GetElementWithName($"{NM}{GR}5{cl}");
        SpritesDirty = false;
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprites = sLeaser.sprites;
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        var darkness = .8f + rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)) * .1f;
        float lX = Mathf.Lerp(LastSizeLerperX, SizeLerperX, timeStacker), lY = Mathf.Lerp(LastSizeLerperY, SizeLerperY, timeStacker);
        for (var i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i];
            sprite.SetPosition(pos - camPos);
            sprite.scaleX = lX;
            sprite.scaleY = lY;
        }
        sprites[0].scaleX *= PetalVars[0];
        sprites[1].scaleX *= PetalVars[1];
        sprites[2].scaleX *= PetalVars[1];
        sprites[3].scaleX *= PetalVars[1];
        sprites[4].scaleX *= PetalVars[1];
        sprites[5].scaleX *= PetalVars[2];
        sprites[6].scaleX *= PetalVars[2];
        sprites[7].scaleX *= PetalVars[3];
        sprites[8].scaleX *= PetalVars[3];
        var tex = rCam.currentPalette.texture;
        sprites[6].color = Color;
        sprites[5].color = tex.GetPixel(0, 0);
        sprites[4].color = Color;
        sprites[3].color = tex.GetPixel(2, 0);
        sprites[2].color = Color;
        sprites[1].color = tex.GetPixel(4, 0);
        sprites[0].color = tex.GetPixel(5, 0);
        sprites[7].color = tex.GetPixel(6, 0);
        sprites[8].color = Color;
        var bn = Open ? 0f : .35f;
        sprites[8].alpha = sprites[2].alpha = sprites[4].alpha = sprites[6].alpha = Mathf.Lerp(1f + RndColorAlpha - bn, .5f - bn, darkness);
        if (SpritesDirty)
            ChangeDirtySprites(sLeaser);
        if (MoveToFrontCounter == 0)
        {
            for (var i = 0; i < 7; i++)
                sprites[i].MoveToFront();
            MoveToFrontCounter = 5;
        }
        else
            --MoveToFrontCounter;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < 7; i++)
        {
            var sprite = sprites[i];
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
        newContainer = rCam.ReturnFContainer("Background");
        for (var i = 7; i < 9; i++)
        {
            var sprite = sprites[i];
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
    }

    public override void Grabbed(Creature.Grasp grasp) { }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk) { }

    public override void HitByWeapon(Weapon weapon) { }
}