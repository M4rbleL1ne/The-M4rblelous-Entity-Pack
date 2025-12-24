using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using System;
using Smoke;

namespace LBMergedMods.Creatures;

public class ChipChopGraphics : GraphicsModule, IMuddableGraphics
{
    public class SpeedSmoke(Room room, Vector2 pos, BodyChunk chunk, Color fireColor) : BombSmoke(room, pos, chunk, fireColor)
    {
        public class SpeedSmokeSegment : ThickSmokeSegment
        {
            public override Color MyColor(float timeStacker) => Color.Lerp(base.MyColor(timeStacker), (owner as BombSmoke)!.fireColor, .5f);
        }

        public override SmokeSystemParticle CreateParticle() => new SpeedSmokeSegment();
    }

    public const int TOTAL_SPRITES = 11;
    public SpeedSmoke? Smoke;
    public Limb[][] Limbs;
    public LightSource? LightSource;
    public float[][] LimbGoalDistances;
    public Vector2[][] DeathLegPositions;
    public Vector2 LastBodyDir, BodyDir;
    public float WalkCycle, LimbLength, LastLightLife, LightLife;
    public bool LegsPosition, LastLegsPosition;

    public virtual ChipChop Bug => (owner as ChipChop)!;

    public ChipChopGraphics(PhysicalObject ow) : base(ow, false)
    {
        BodyDir = Custom.DegToVec(Random.value * 360f);
        var lbs = Limbs = new Limb[2][];
        LimbGoalDistances = [new float[2], new float[2]];
        DeathLegPositions = [[Custom.DegToVec(Random.value * 360f), Custom.DegToVec(Random.value * 360f)], [Custom.DegToVec(Random.value * 360f), Custom.DegToVec(Random.value * 360f)]];
        LimbLength = Mathf.Lerp(20f, 25f, Bug.IVars.Size / 1.5f);
        var fc = ow.firstChunk;
        for (var i = 0; i < lbs.Length; i++)
        {
            var lb = lbs[i] = new Limb[2];
            for (var j = 0; j < lb.Length; j++)
                lb[j] = new(this, fc, i + j * 2, 1f, .5f, .98f, 15f, .95f)
                {
                    mode = Limb.Mode.Dangle,
                    pushOutOfTerrain = false
                };
        }
        LegsPosition = Random.value < .5f;
    }

    public virtual int LimbSprite(int limb, int side, int segment) => 3 + limb + segment * 2 + side * 4;

    public override void Reset()
    {
        base.Reset();
        var lbs = Limbs;
        var ps = owner.firstChunk.pos;
        for (var i = 0; i < lbs.Length; i++)
        {
            var lb = lbs[i];
            for (var j = 0; j < lb.Length; j++)
                lb[j].Reset(ps);
        }
        LastLightLife = LightLife;
    }

    public override void Update()
    {
        base.Update();
        var fc = owner.firstChunk;
        if (Smoke is SpeedSmoke sm)
        {
            sm.setPos = fc.pos;
            if (sm.slatedForDeletetion || sm.room != Bug.room || Bug.SpeedEffectDuration == 0)
            {
                if (!sm.slatedForDeletetion)
                    sm.Destroy();
                Smoke = null;
            }
        }
        else if (Bug.SpeedEffectDuration > 0 && Bug.room is Room rm)
            rm.AddObject(Smoke = new(rm, fc.pos, fc, Color.red));
        if (Bug.Glowing)
        {
            LastLightLife = LightLife;
            LightLife = Mathf.Clamp01(LightLife + (Bug.dead ? -.00125f : .00125f));
            var darkness = Bug.room?.Darkness(fc.pos) ?? 0f;
            if (LightSource is LightSource lh)
            {
                lh.stayAlive = true;
                lh.setPos = fc.pos;
                lh.setRad = 200f * LightLife;
                lh.setAlpha = .8f * Mathf.Pow(LightLife, .5f);
                lh.color = Custom.HSL2RGB(Bug.Hue * 2f + 20f / 360f, Bug.Saturation, Bug.Lightness);
                if (lh.slatedForDeletetion || darkness == 0f)
                    LightSource = null;
            }
            else if (darkness > 0f)
            {
                LastLightLife = LightLife = Bug.dead ? 0f : 1f;
                Bug.room?.AddObject(LightSource = new(fc.pos, false, Color.white, Bug) { requireUpKeep = true });
            }
        }
        else
        {
            LastLightLife = LightLife = 0f;
            if (LightSource is not null)
            {
                LightSource.Destroy();
                LightSource = null;
            }
        }
        LastBodyDir = BodyDir;
        if (Bug.AttachedChunk is BodyChunk ch)
            BodyDir = Custom.DirVec(fc.pos, ch.pos);
        else
        {
            BodyDir -= Custom.DirVec(fc.pos, Bug.DragPos);
            BodyDir += fc.vel * .2f;
            if (!Bug.Consious)
                BodyDir += Custom.DegToVec(Random.value * 360f) * Bug.DeathSpasms;
            BodyDir.Normalize();
        }
        var magnitude = fc.vel.magnitude;
        if (magnitude > 1f)
        {
            WalkCycle += Mathf.Max(0f, (magnitude - 1f) / 30f);
            if (WalkCycle > 1f)
                --WalkCycle;
        }
        LastLegsPosition = LegsPosition;
        LegsPosition = WalkCycle > .5f;
        var vector = Custom.PerpendicularVector(BodyDir);
        var lbs = Limbs;
        for (var i = 0; i < lbs.Length; i++)
        {
            var lb = lbs[i];
            for (var j = 0; j < lb.Length; j++)
            {
                var legP = lb[j];
                var vector2 = BodyDir;
                var flag2 = i % 2 == j == LegsPosition;
                vector2 = Custom.DegToVec(Custom.VecToDeg(vector2) + (Mathf.Lerp(30f, 140f, i * 1f / 3f) + (i == 3 ? 20f : 0f) + 15f * (flag2 ? -1f : 1f) * Mathf.InverseLerp(.5f, 5f, magnitude)) * (-1 + 2 * j));
                var num = i == 0 ? LimbLength : (.95f * LimbLength);
                var vector3 = fc.pos + vector2 * (num * .85f) + fc.vel.normalized * (num * .4f * Mathf.InverseLerp(.5f, 5f, magnitude));
                if (i == 0 && !Bug.dead && !Bug.Idle)
                    legP.pos += Custom.DegToVec(Random.value * 360f) * Random.value;
                var flag3 = false;
                if (Bug.Consious)
                {
                    legP.mode = Limb.Mode.HuntAbsolutePosition;
                    if ((Bug.FollowingConnection != default && Bug.FollowingConnection.type == MovementConnection.MovementType.DropToFloor) || !Bug.InAccessibleTerrain)
                    {
                        flag3 = true;
                        legP.mode = Limb.Mode.Dangle;
                        legP.vel += Custom.DegToVec(Random.value * 360f) * (Random.value * 3f);
                    }
                    else if (i == 0 && Bug.AttachedChunk is BodyChunk b)
                    {
                        flag3 = true;
                        legP.absoluteHuntPos = b.pos + vector * ((-1 + 2 * j) * b.rad * .5f);
                        legP.pos = legP.absoluteHuntPos;
                    }
                }
                else
                    legP.mode = Limb.Mode.Dangle;
                if (legP.mode == Limb.Mode.HuntAbsolutePosition)
                {
                    if (!flag3)
                    {
                        if (magnitude < 1f)
                        {
                            if (Random.value < .05f && !Custom.DistLess(legP.pos, vector3, num / 6f))
                                FindGrip(i, j, vector3, num, magnitude, legP);
                        }
                        else if (flag2 && (LastLegsPosition != LegsPosition || i == 3) && !Custom.DistLess(legP.pos, vector3, num * .5f))
                            FindGrip(i, j, vector3, num, magnitude, legP);
                    }
                }
                else
                {
                    legP.vel += Custom.RotateAroundOrigo(DeathLegPositions[i][j], Custom.AimFromOneVectorToAnother(-BodyDir, BodyDir)) * .65f + Custom.DegToVec(Random.value * 360f) * (Bug.DeathSpasms * 5f) + vector2 * .7f;
                    legP.vel.y -= .8f;
                    LimbGoalDistances[i][j] = 0f;
                }
                legP.huntSpeed = 15f * Mathf.InverseLerp(-0.05f, 2f, magnitude);
                legP.Update();
                legP.ConnectToPoint(fc.pos, num, false, 0f, fc.vel, 1f, .5f);
            }
        }
    }

    public virtual void FindGrip(int l, int s, Vector2 idealPos, float rad, float moveSpeed, Limb legPart)
    {
        if (owner.room.GetTile(idealPos).wallbehind)
            legPart.absoluteHuntPos = idealPos;
        else
            legPart.FindGrip(owner.room, owner.firstChunk.pos, idealPos, rad, idealPos + BodyDir * Mathf.Lerp(moveSpeed * 2f, rad / 2f, .5f), 2, 2, true);
        LimbGoalDistances[l][s] = Vector2.Distance(legPart.pos, legPart.absoluteHuntPos);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        ref readonly var iVars = ref Bug.IVars;
        sprites[0] = new("ChipChopBody")
        {
            scale = iVars.Size / 1.8f
        };
        sprites[1] = new("ChipChopBodyGrad")
        {
            scale = iVars.Size / 1.8f,
            alpha = iVars.ButtAlpha
        };
        sprites[2] = new("ChipChopBodyEye" + iVars.EyeVar)
        {
            scale = iVars.Size / 1.8f
        };
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                var sz = (j == 0 ? 1f : -1f) * .85f * iVars.Size;
                var leg = "SpiderLeg" + (i + 1);
                sprites[LimbSprite(i, j, 0)] = new(leg + "A")
                {
                    anchorY = 1f / (i == 0 ? 26f : 21f),
                    scaleX = sz,
                    scaleY = (i == 0 ? 1f : .95f) * (i == 0 ? .6f : .5f) * LimbLength / (i == 0 ? 26f : 21f)
                };
                sprites[LimbSprite(i, j, 1)] = new(leg + "B")
                {
                    anchorY = 1f / (i == 0 ? 20f : 23f),
                    scaleX = sz
                };
            }
        }
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        ref readonly var iVars = ref Bug.IVars;
        var fc = owner.firstChunk;
        var sprites = sLeaser.sprites;
        var vector = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
        var ps = vector - camPos;
        sprites[0].SetPosition(ps);
        sprites[1].SetPosition(ps);
        sprites[2].SetPosition(ps);
        Vector2 vector2 = Vector3.Slerp(LastBodyDir, BodyDir, timeStacker),
            vector3 = -Custom.PerpendicularVector(vector2);
        sprites[2].rotation = sprites[1].rotation = sprites[0].rotation = Custom.AimFromOneVectorToAnother(-vector2, vector2);
        sprites[2].alpha = Bug.dead ? 0f : iVars.EyeAlpha;
        var sz = (iVars.Size + Bug.BouncingMelonEffectDuration / 18000f) / 1.8f;
        for (var i = 0; i < 3; i++)
            sprites[i].scale = sz;
        var lbs = Limbs;
        for (var l = 0; l < lbs.Length; l++)
        {
            var lb = lbs[l];
            var lgd = LimbGoalDistances[l];
            for (var m = 0; m < lb.Length; m++)
            {
                var legP = lb[m];
                Vector2 vector4 = vector + (vector2 * (7f - (l + 1) * .5f) + vector3 * ((3f + (l + 1) * .5f) * (-1 + 2 * m))) * iVars.Size,
                    a = Vector2.Lerp(legP.lastPos, legP.pos, timeStacker);
                a = Vector2.Lerp(a, vector4 + vector2 * (LimbLength * .1f), Mathf.Sin(Mathf.InverseLerp(0f, lgd[m], Vector2.Distance(a, legP.absoluteHuntPos)) * Mathf.PI) * .4f);
                float fl = (l == 0 ? 1f : .95f) * LimbLength,
                    num = fl * (l == 0 ? .6f : .5f),
                    num2 = fl * (l == 0 ? .4f : .5f),
                    num3 = Vector2.Distance(vector4, a),
                    num4 = l == 1 ? .7f : 1f;
                num4 *= -1f + 2f * m;
                var num5 = Mathf.Acos(Mathf.Clamp((num3 * num3 + num * num - num2 * num2) / (2f * num3 * num), .2f, .98f)) * (180f / Mathf.PI) * num4;
                var vector5 = vector4 + Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector4, a) + num5) * num;
                FSprite l0 = sprites[LimbSprite(l, m, 0)],
                    l1 = sprites[LimbSprite(l, m, 1)];
                l0.SetPosition(vector4 - camPos);
                l1.SetPosition(vector5 - camPos);
                l0.rotation = Custom.AimFromOneVectorToAnother(vector4, vector5);
                l1.rotation = Custom.AimFromOneVectorToAnother(vector5, a);
                l1.scaleY = fl * (l == 0 ? .6f : .5f) / (l == 0 ? 20f : 23f);
            }
        }
        if (Bug.MarineEyeEffectDuration > 0 || Bug.Glowing || Bug.MushroomEffectDuration > 0)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var h = Bug.Hue;
        var sat = Bug.Saturation;
        var light = Math.Max(0f, Bug.Lightness - palette.darkness * (1f - LightLife) * .3f);
        var eyeColor = Custom.HSL2RGB(h, sat, light);
        var bodyColor = Color.Lerp(eyeColor, palette.blackColor, .92f);
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
            sprites[i].color = bodyColor;
        sprites[1].color = Custom.HSL2RGB(h * 2f + 20f / 360f, sat, light);
        sprites[2].color = eyeColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite is not 1 and not 2;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) => mudOverlay.layeringMap[mudOverlay.GetMudSprite(0)] = sLeaser.sprites[1];
}