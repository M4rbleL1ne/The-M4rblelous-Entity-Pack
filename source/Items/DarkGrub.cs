using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class DarkGrub : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public const float BASESIZE = 1f;
    public const int SHAPE_SPRITE = 0, GRAD_SPRITE = 1, GLOW_SPRITE = 2;
    public Vector2 Rotation, LastRotation;
    public int Bites = 2;
    public float Darkness, LastDarkness, Hue, LightLife, LastLightLife, LastSizeLerperX = BASESIZE,
        LastSizeLerperY = BASESIZE, SizeLerperX = BASESIZE, SizeLerperY = BASESIZE, WowSpeed = .0002f;
    public Vector2? SetRotation, GrowPos, Direction;
    public bool SizeLerpUp = true;

    public virtual bool StalkActive => GrowPos is not null;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 0;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public DarkGrub(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 7.5f, .1f)];
        bodyChunkConnections = [];
        airFriction = .98f;
        gravity = .92f;
        bounce = .2f;
        surfaceFriction = .3f;
        collisionLayer = 1;
        waterFriction = .98f;
        buoyancy = .95f;
        Hue = 90f / 360f + 40f / 360f * abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed);
        GoThroughFloors = true;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        LastDarkness = Darkness;
        Darkness = room.Darkness(fc.pos);
        LastRotation = Rotation;
        if (GrowPos is Vector2 vec)
        {
            CollideWithSlopes = false;
            CollideWithTerrain = false;
            fc.mass = .4f;
            if (Direction is Vector2 dir)
                LastRotation = Rotation = dir;
            LastSizeLerperX = SizeLerperX;
            LastSizeLerperY = SizeLerperY;
            if (SizeLerperX >= BASESIZE + .06f)
                SizeLerpUp = false;
            else if (SizeLerperX <= BASESIZE - .06f)
                SizeLerpUp = true;
            if (SizeLerpUp)
                SizeLerperX += 5f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
            else
                SizeLerperX -= 8f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
            if (SizeLerpUp)
                SizeLerperY -= 5f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
            else
                SizeLerperY += 8f * WowSpeed + Random.Range(-2f * WowSpeed, 2f * WowSpeed);
            LastLightLife = LightLife = 1f;
            fc.HardSetPosition(vec);
        }
        else
        {
            fc.mass = .1f;
            LastLightLife = LightLife;
            LightLife = Math.Max(LightLife - .0025f, 0f);
            LastSizeLerperY = SizeLerperY = LastSizeLerperX = SizeLerperX = BASESIZE;
            if (SetRotation.HasValue)
            {
                Rotation = SetRotation.Value;
                SetRotation = null;
            }
            if (fc.ContactPoint.y < 0)
            {
                Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
                fc.vel.x *= .8f;
            }
        }
        if (grabbedBy.Count > 0)
        {
            CollideWithSlopes = false;
            CollideWithTerrain = false;
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Math.Abs(Rotation.y);
            DetatchStalk();
        }
        else
        {
            CollideWithSlopes = true;
            CollideWithTerrain = true;
        }
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk)
    {
        if (!GrowPos.HasValue)
            base.PushOutOf(pos, rad, exceptedChunk);
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        firstChunk.mass = .1f;
        base.Grabbed(grasp);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        var fc = firstChunk;
        var flag1 = ModManager.MMF && placeRoom.game.session is ArenaGameSession sess && (MMF.cfgSandboxItemStems.Value || sess.chMeta is not null) && sess.counter < 10;
        if ((!AbstrCons.isConsumed && AbstrCons.placedObjectIndex >= 0 && AbstrCons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count) || flag1)
        {
            Vector2 basePos;
            PlacedObject? pObj = null;
            if (flag1)
                basePos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            else
            {
                pObj = placeRoom.roomSettings.placedObjects[AbstrCons.placedObjectIndex];
                basePos = pObj.pos;
            }
            var dir = pObj?.data is DarkGrubData dt ? dt.RootDir : new IntVector2(0, -1);
            if (dir != default)
            {
                var tlPos = Room.StaticGetTilePosition(basePos);
                while (!placeRoom.GetTile(tlPos).Solid)
                {
                    tlPos += dir;
                    if (!placeRoom.IsPositionInsideBoundries(tlPos))
                    {
                        dir = default;
                        break;
                    }
                }
                if (dir != default)
                {
                    GrowPos = placeRoom.MiddleOfTile(tlPos - dir);
                    Direction = dir.ToVector2();
                }
            }
        }
        else
        {
            fc.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            Rotation = Custom.RNV();
            LastRotation = Rotation;
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        DetatchStalk();
        if (weapon is Spear s && s.IsNeedle && s.Spear_NeedleCanFeed() && s.thrownBy is Player p)
        {
            p.ObjectEaten(this);
            if (PlayerData.TryGetValue(p.abstractCreature, out var props))
                props.GrubVisionDuration = 3200;
            if (room is Room rm)
            {
                rm.PlaySound(SoundID.Slugcat_Eat_Centipede, firstChunk);
                for (var i = 0; i < 10; i++)
                    rm.AddObject(new Spark(firstChunk.pos, Custom.RNV(), color, null, 30, 2));
            }
            var grabbers = grabbedBy;
            for (var i = 0; i < grabbers.Count; i++)
                grabbers[i]?.Release();
            Destroy();
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("DarkGrubShape"),
            new("DarkGrubGrad"),
            new("Futile_White") { shader = Custom.rainWorld.Shaders["FlatLightBehindTerrain"] }
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var lightLife = Mathf.Lerp(LastLightLife, LightLife, timeStacker);
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos,
            rot = Vector3.Slerp(LastRotation, Rotation, timeStacker);
        var shape = sLeaser.sprites[SHAPE_SPRITE];
        var grad = sLeaser.sprites[GRAD_SPRITE];
        shape.SetPosition(vector);
        grad.SetPosition(vector);
        if (!GrowPos.HasValue)
        {
            shape.element = Futile.atlasManager.GetElementWithName("DarkGrubShapeHeld");
            grad.element = Futile.atlasManager.GetElementWithName("DarkGrubGradHeld");
        }
        var sizeFac = Bites < 2 ? .6f : 1f;
        grad.scaleX = shape.scaleX = Mathf.Lerp(LastSizeLerperX, SizeLerperX, timeStacker) * sizeFac;
        grad.scaleY = shape.scaleY = Mathf.Lerp(LastSizeLerperY, SizeLerperY, timeStacker) * sizeFac;
        grad.rotation = shape.rotation = Custom.VecToDeg(rot);
        var num = Mathf.Lerp(LastDarkness, Darkness, timeStacker);
        grad.color = blink > 0 && Random.value < .5f ? blinkColor : Color.Lerp(color, rCam.currentPalette.blackColor, .15f * (1f - lightLife));
        var effect = sLeaser.sprites[GLOW_SPRITE];
        effect.SetPosition(vector - (Direction ?? default) * 5f);
        effect.scale = Mathf.Lerp(15f, 30f, num) * sizeFac / 11f;
        effect.alpha = .65f * lightLife;
        effect.color = Custom.HSL2RGB(Hue, .9f + .1f * num, .4f + .2f * num);
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Custom.HSL2RGB(Hue, 1f, .5f);
        sLeaser.sprites[SHAPE_SPRITE].color = palette.blackColor;
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer = rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var sprite = sprs[i];
            sprite.RemoveFromContainer();
            if (i == GLOW_SPRITE)
                rCam.ReturnFContainer("Foreground").AddChild(sprite);
            else
                newContainer.AddChild(sprite);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        room.PlaySound(SoundID.Slugcat_Eat_Centipede, firstChunk);
        --Bites;
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (grasp.grabber is Player p)
        {
            if (PlayerData.TryGetValue(p.abstractCreature, out var props))
                props.GrubVisionDuration = Math.Min(props.GrubVisionDuration + 1610, 3200);
        }
        else if (grasp.grabber is ChipChop ch)
            ch.MushroomEffectDuration = Math.Min(ch.MushroomEffectDuration + 500, 980);
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
        if (!AbstrCons.isConsumed)
            AbstrCons.Consume();
        if (GrowPos.HasValue)
            GrowPos = null;
        if (Direction.HasValue)
            Direction = null;
    }
}