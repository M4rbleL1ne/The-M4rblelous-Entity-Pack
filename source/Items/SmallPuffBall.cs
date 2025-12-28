using UnityEngine;
using Smoke;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class SmallPuffBall : Rock
{
    public Color SporeColor;
    public SporesSmoke? Smoke;
    public bool LastModeThrown;
    public Vector2[] Dots;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public SmallPuffBall(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        exitThrownModeSpeed = 15f;
        var dots = Dots = new Vector2[Random.Range(6, 11)];
        for (var i = 0; i < dots.Length; i++)
            dots[i] = Custom.DegToVec((float)i / dots.Length * 360f) * Random.value + Custom.RNV() * .2f;
        Random.state = state;
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < dots.Length; k++)
            {
                for (var l = 0; l < dots.Length; l++)
                {
                    if (Custom.DistLess(dots[k], dots[l], 1.4f))
                    {
                        var vector = Custom.DirVec(dots[k], dots[l]) * (Vector2.Distance(dots[k], dots[l]) - 1.4f) * .5f;
                        var num = k / (k + (float)l);
                        dots[k] += vector * num * .5f;
                        dots[l] -= vector * (1f - num) * .5f;
                    }
                }
            }
        }
        var num2 = 1f;
        var num3 = -1f;
        var num4 = 1f;
        var num5 = -1f;
        for (var m = 0; m < dots.Length; m++)
        {
            var dot = dots[m];
            num2 = Mathf.Min(num2, dot.x);
            num3 = Mathf.Max(num3, dot.x);
            num4 = Mathf.Min(num4, dot.y);
            num5 = Mathf.Max(num5, dot.y);
        }
        for (var n = 0; n < dots.Length; n++)
        {
            ref var dot = ref dots[n];
            dot.x = -.5f + Mathf.InverseLerp(num2, num3, dot.x);
            dot.y = -.5f + Mathf.InverseLerp(num4, num5, dot.y);
        }
        var num6 = 0f;
        for (var num7 = 0; num7 < dots.Length; num7++)
            num6 = Mathf.Max(num6, dots[num7].magnitude);
        for (var num8 = 0; num8 < dots.Length; num8++)
            dots[num8] /= num6;
    }

    public override void Update(bool eu)
    {
        if (LastModeThrown && (firstChunk.ContactPoint.x != 0 || firstChunk.ContactPoint.y != 0))
            Explode();
        LastModeThrown = mode == Mode.Thrown;
        if (Smoke is SporesSmoke smoke)
        {
            if (room.ViewedByAnyCamera(firstChunk.pos, 300f))
                smoke.EmitSmoke(firstChunk.pos, Custom.DirVec(firstChunk.pos, tailPos) + Custom.RNV(), SporeColor);
            if (smoke.slatedForDeletetion || smoke.room != room)
                Smoke = null;
        }
        else
            room.AddObject(Smoke = new(room));
        base.Update(eu);
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.chunk is not BodyChunk ch)
            return false;
        if (!abstractPhysicalObject.SameRippleLayer(ch.owner.abstractPhysicalObject))
            return false;
        base.HitSomething(result, eu);
        Explode();
        if (result.obj is Creature cr && cr is not Sporantula)
        {
            cr.Stun(8);
            if (cr is InsectoidCreature ic)
                ic.poison += .35f;
        }
        return true;
    }


    public override void PlaceInRoom(Room placeRoom)
    {
        if (!AbstrCons.isConsumed)
        {
            var index = AbstrCons.placedObjectIndex;
            if (index >= 0 && index < placeRoom.roomSettings.placedObjects.Count)
                firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[index].pos);
        }
        base.PlaceInRoom(placeRoom);
    }


    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        room.AddObject(new SporeCloud(firstChunk.pos, Custom.RNV() * Random.value + throwDir.ToVector2() * 10f, SporeColor, .25f, null, -1, null, abstractPhysicalObject.rippleLayer));
        room.PlaySound(SoundID.Slugcat_Throw_Puffball, firstChunk);
    }

    public virtual void Shoot(Vector2 pos, Vector2 dir, BigSpider thrownBy)
    {
        if (room?.game is not RainWorldGame g)
            return;
        this.thrownBy = thrownBy;
        thrownPos = pos;
        firstChunk.HardSetPosition(thrownPos);
        tailPos = thrownPos;
        throwDir = new(Math.Sign(dir.x), 0);
        firstChunk.vel = dir * 50f;
        for (var num = Random.Range(0, 5); num >= 0; num--)
            room.AddObject(new WaterDrip(pos, dir * Random.value * 15f + Custom.RNV() * Random.value * 5f, false));
        color = Color.Lerp(new(.9f, 1f, .8f), g.cameras[0].currentPalette.texture.GetPixel(11, 4), .5f);
        SporeColor = Color.Lerp(color, new(.02f, .1f, .08f), .85f);
        room.AddObject(new SporeCloud(firstChunk.pos, Custom.RNV() * Random.value + dir * 10f, SporeColor, 1f, null, -1, null, abstractPhysicalObject.rippleLayer));
        room.PlaySound(SoundID.Slugcat_Throw_Puffball, firstChunk);
        changeDirCounter = 3;
        ChangeOverlap(true);
        ChangeMode(Mode.Thrown);
        setRotation = dir;
        rotationSpeed = 0f;
        meleeHitChunk = null;
    }

    public override void HitWall()
    {
        AbstrCons.Consume();
        Explode();
        SetRandomSpin();
        ChangeMode(Mode.Free);
        forbiddenToPlayer = 10;
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        AbstrCons.Consume();
        Explode();
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        AbstrCons.Consume();
        Explode();
    }

    public virtual void Explode()
    {
        if (slatedForDeletetion || thrownBy is null)
            return;
        InsectCoordinator? smallInsects = null;
        var udlist = room.updateList;
        for (var i = 0; i < udlist.Count; i++)
        {
            if (udlist[i] is InsectCoordinator coord)
            {
                smallInsects = coord;
                break;
            }
        }
        var ps = firstChunk.pos;
        for (var j = 0; j < 40; j++)
            room.AddObject(new SporeCloud(ps, Custom.RNV() * Random.value * 10f, SporeColor, 1f, thrownBy?.abstractCreature, j % 20, smallInsects, abstractPhysicalObject.rippleLayer));
        room.AddObject(new SporePuffVisionObscurer(ps, abstractPhysicalObject.rippleLayer));
        for (var k = 0; k < 4; k++)
            room.AddObject(new PuffBallSkin(ps, Custom.RNV() * Random.value * 16f, color, Color.Lerp(color, SporeColor, .5f)));
        room.PlaySound(SoundID.Puffball_Eplode, firstChunk);
        Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var dotl = Dots.Length;
        var sprs = sLeaser.sprites = new FSprite[2 + dotl * 2];
        sprs[0] = new("BodyA");
        sprs[1] = new("BodyA");
        for (var i = 0; i < dotl; i++)
        {
            sprs[2 + i] = new("JetFishEyeB");
            sprs[2 + dotl + i] = new("pixel");
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        if (grabbedBy.Count > 0 && AbstrCons.originRoom >= 0 && AbstrCons.originRoom != newRoom.abstractRoom.index)
            AbstrCons.Consume();
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        var degAng = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
        if (vibrate > 0)
            vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        var dots = Dots;
        var sprites = sLeaser.sprites;
        var s0 = sprites[0];
        var s1 = sprites[1];
        s0.x = vector.x - camPos.x;
        s0.y = vector.y - camPos.y;
        s1.x = vector.x - camPos.x - 1.25f;
        s1.y = vector.y - camPos.y + 1.25f;
        s0.rotation = degAng;
        s1.rotation = degAng;
        s0.scaleY = .6f;
        s0.scaleX = .66f;
        s1.scaleY = .3f;
        s1.scaleX = .33f;
        for (var i = 0; i < dots.Length; i++)
        {
            var dot = dots[i];
            var vector2 = vector + Custom.RotateAroundOrigo(new Vector2(dot.x * 4f, dot.y * 5.5f), degAng);
            var s2i = sprites[2 + i];
            s2i.x = vector2.x - camPos.x;
            s2i.y = vector2.y - camPos.y;
            s2i.rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(dot, degAng).normalized);
            s2i.scaleY = Custom.LerpMap(dot.magnitude, 0f, 1f, 1f, .25f, 4f);
            var s2idotl = sprites[2 + dots.Length + i];
            s2idotl.x = vector2.x - camPos.x;
            s2idotl.y = vector2.y - camPos.y;
        }
        if (blink > 0 && mode != Mode.Thrown)
            s0.color = (blink > 1 && Random.value < .5f) ? blinkColor : color;
        else
            s0.color = color;
        s1.color = Color.white;
        s1.alpha = .5f - rCam.currentPalette.darkness / 5f;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), palette.texture.GetPixel(11, 4), .5f), palette.blackColor, palette.darkness / 2f);
        var sprs = sLeaser.sprites;
        sprs[0].color = color;
        sprs[1].color = Color.white;
        SporeColor = Color.Lerp(color, new(.02f, .1f, .08f), .85f);
        var clr = Color.Lerp(Color.Lerp(new(.8f, 1f, .5f), palette.texture.GetPixel(11, 4), .2f), palette.blackColor, .5f + palette.darkness / 5f);
        var dotl = Dots.Length;
        for (var j = 0; j < dotl; j++)
        {
            sprs[2 + j].color = clr;
            sprs[2 + dotl + j].color = SporeColor;
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        var specialIndex = 2 + Dots.Length;
        for (var i = 0; i < sprs.Length; i++)
        {
            FSprite spr;
            if (i == specialIndex)
            {
                spr = sprs[1];
                spr.RemoveFromContainer();
                newContainer.AddChild(spr);
            }
            else if (i != 1)
            {
                spr = sprs[i];
                spr.RemoveFromContainer();
                newContainer.AddChild(spr);
            }
        }
    }
}