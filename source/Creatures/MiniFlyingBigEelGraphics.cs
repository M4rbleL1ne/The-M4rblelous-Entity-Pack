using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class MiniFlyingBigEelGraphics : BigEelGraphics
{
    public MiniFlyingBigEelGraphics(MiniFlyingBigEel ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var tl = tail = new TailSegment[11];
        var chlm1 = ow.bodyChunks.Length - 1;
        for (var i = 0; i < tl.Length; i++)
        {
            var t = i / chlm1;
            tl[i] = new(this, Mathf.Lerp(ow.bodyChunks[chlm1].rad, 1f, t), 15f, i > 0 ? tl[i - 1] : null, .5f, 1f, .1f, true);
        }
        var fns = fins = new TailSegment[Random.Range(2, 3)][,];
        finsData = new float[fns.Length, 2];
        var num = Mathf.Lerp(6f, 8f, Random.value);
        if (fns.Length > 2)
            num *= .8f;
        for (var j = 0; j < fns.Length; j++)
        {
            finsData[j, 0] = (5f + 10f * Mathf.Sin(Mathf.Pow((float)j / (fns.Length - 1), .5f) * Mathf.PI)) * .9f;
            var num3 = num + num * Mathf.Sin(Mathf.Pow(j / 5f, .8f) * Mathf.PI);
            var fj = fns[j] = new TailSegment[2, Mathf.FloorToInt(finsData[j, 0] / 3f) + 1];
            for (var k = 0; k < 2; k++)
            {
                finsData[j, 1] = Random.value;
                var l1 = fj.GetLength(1);
                for (var l = 0; l < l1; l++)
                    fns[j][k, l] = new(this, 1f + FinContour((float)l / (l1 - 1)) * num3, (finsData[j, 0] / Mathf.FloorToInt(finsData[j, 0] / 16f) + 1f) * .05f, l > 0 ? fns[j][k, l - 1] : null, .5f, 1f, .2f, true);
            }
        }
        numberOfScales = Random.Range(16, 20);
        scaleSize = Mathf.Lerp(.2f, .7f, Mathf.Pow(Random.value, .5f));
        numberOfEyes = 20;
        var eyesDt = eyesData = new Vector2[numberOfEyes];
        eyeScales = new float[numberOfEyes, 3];
        for (var n = 0; n < eyesDt.Length; n++)
        {
            var eyeDn = eyesDt[n] = Custom.RNV() * (Mathf.Pow(Random.value, .6f) * .15f);
            if (eyeDn.y > .7f)
                eyeDn.y = Mathf.Lerp(eyeDn.y, .7f, .3f);
            eyeScales[n, 0] = Mathf.Lerp(.2f, 1f, Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 1.5f, .2f)));
            eyeScales[n, 1] = Mathf.Lerp(.1f, eyeScales[n, 0] * .9f, Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 2f, .1f)));
            eyeScales[n, 2] = Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 2f, .1f));
        }
        Random.state = state;
        var num4 = 0;
        for (var num5 = 0; num5 < fns.Length; num5++)
            num4 += 2 * fns[num5].GetLength(1);
        var bp = bodyParts = new BodyPart[tl.Length + num4];
        for (var num6 = 0; num6 < tl.Length; num6++)
            bp[num6] = tl[num6];
        num4 = 0;
        for (var num7 = 0; num7 < fns.Length; num7++)
        {
            for (var num8 = 0; num8 < 2; num8++)
            {
                var l = fns[num7].GetLength(1);
                for (var num9 = 0; num9 < l; num9++)
                {
                    bp[tl.Length + num4] = fns[num7][num8, num9];
                    ++num4;
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (chargedJawsSound is StaticSoundLoop s)
        {
            s.pitch *= 1.2f;
            s.volume *= .25f;
        }
        if (hydraulicsSound is StaticSoundLoop s2)
        {
            s2.pitch *= 1.2f;
            s2.volume *= .25f;
        }
    }

    public override void Reset()
    {
        base.Reset();
        if (finSound is StaticSoundLoop snd)
            snd.volume = 0f;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprites = sLeaser.sprites;
        sprites[MeshSprite].shader = Custom.rainWorld.Shaders["GRJMiniEelBody"];
        var flg = fins.Length;
        for (var k = 0; k < 2; k++)
        {
            for (var l = 0; l < flg; l++)
            {
                var fin = sprites[FinSprite(l, k)];
                fin.shader = Custom.rainWorld.Shaders["TentaclePlant"];
                fin.MoveToBack();
            }
            var s = sprites[BeakSprite(k, 0)];
            s.scaleX *= .35f;
            s.scaleY *= .4f;
            s = sprites[BeakSprite(k, 1)];
            s.scaleX *= .35f;
            s.scaleY *= .4f;
            for (var n = 0; n < 4; n++)
            {
                for (var num = 0; num < 2; num++)
                {
                    s = sprites[BeakArmSprite(n, num, k)];
                    if (n % 2 == 0)
                    {
                        s.scaleX = 2f;
                        s.scaleY = 15f;
                    }
                    else
                        s.scale = .25f;
                }
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (eel is MiniFlyingBigEel be)
        {
            Vector2 vector4 = Vector2.Lerp(be.firstChunk.lastPos, be.firstChunk.pos, timeStacker),
                  vector5 = Custom.DirVec(Vector2.Lerp(be.bodyChunks[1].lastPos, be.bodyChunks[1].pos, timeStacker), vector4),
                  vector6 = Custom.PerpendicularVector(vector5);
            float num4 = Mathf.Lerp(lastJawCharge, jawCharge, timeStacker),
                num5 = Mathf.Min(Mathf.InverseLerp(0f, .3f, num4), Mathf.InverseLerp(1f, .7f, num4)) * .6f,
                t = num4 > .35f ? Mathf.InverseLerp(1f, .65f, num4) : 0f,
                num6 = num5 * Mathf.InverseLerp(.7f, .4f, num4) * .6f,
                num7 = Mathf.Sin(be.jawChargeFatigue * Mathf.PI) * num6;
            var sprites = sLeaser.sprites;
            for (var k = 0; k < 2; k++)
            {
                var num8 = k == 0 ? -.3f : .3f;
                var vector11 = vector4 + vector5 * (65f * num6) + Custom.RNV() * (num7 * 2f);
                vector11 += vector6 * (num8 * (Mathf.Lerp(30f, 6f + be.beakGap * .05f, t) + 10f * Mathf.Sin(Mathf.Pow(num5, 2f) * Mathf.PI)));
                var num11 = Custom.VecToDeg(vector5) + Mathf.Sin(num5 * Mathf.PI) * (num4 < .35f ? -20f : -10f) * num8 + Mathf.Lerp(-2f, 2f, Random.value) * num7;
                for (var num12 = 0; num12 < 2; num12++)
                {
                    var s = sprites[BeakSprite(k, num12)];
                    s.SetPosition(vector11 - camPos);
                    s.rotation = num11;
                    var num13 = num4 >= .35f ? (num12 == 0 ? (43f * Math.Abs(Mathf.Cos(Mathf.InverseLerp(1f, .4f, num4) * Mathf.PI))) : (30f * Mathf.InverseLerp(1f, .4f, num4))) : (num12 == 0 ? Mathf.Lerp(15f, 43f, Mathf.InverseLerp(.35f, .15f, num4)) : (30f * Mathf.Pow(Mathf.InverseLerp(0f, .5f, num5), .2f)));
                    Vector2 vector12 = vector4 + vector6 * (num8 * num13) - vector5 * (num12 == 0 ? 22f : 30f),
                        vector13 = vector11 + Custom.DegToVec(num11) * (num12 == 0 ? -12f : 10f) + Custom.PerpendicularVector(Custom.DegToVec(num11)) * (2.5f * num8),
                        vector14 = Custom.InverseKinematic(vector12, vector13, 35f, 25f, -num8);
                    int num14;
                    for (num14 = 0; num14 < 2; num14++)
                    {
                        s = sprites[BeakArmSprite(num14, num12, k)];
                        s.SetPosition(vector14 - camPos);
                    }
                    sprites[BeakArmSprite(0, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector14, vector12);
                    for (num14 = 2; num14 < 4; num14++)
                    {
                        s = sprites[BeakArmSprite(num14, num12, k)];
                        s.SetPosition(vector13 - camPos);
                    }
                    sprites[BeakArmSprite(2, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector13, vector14);
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (eel is MiniFlyingBigEel be)
        {
            var state = Random.state;
            Random.InitState(be.abstractPhysicalObject.ID.RandomSeed);
            var sprites = sLeaser.sprites;
            var flg = fins.Length;
            for (var k = 0; k < 2; k++)
            {
                var beak = sprites[BeakSprite(k, 1)];
                beak.color = Color.Lerp(beak.color, RainWorld.GoldRGB, .65f);
                for (var l = 0; l < numberOfScales; l++)
                {
                    var scl = sprites[ScaleSprite(l, k)];
                    scl.color = Color.Lerp(scl.color, HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb, .8f);
                }
                for (var m = 0; m < flg; m++)
                    sprites[FinSprite(m, k)].color = HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb;
            }
            Random.state = state;
        }
    }
}