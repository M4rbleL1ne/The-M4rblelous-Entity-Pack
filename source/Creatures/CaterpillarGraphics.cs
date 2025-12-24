using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class CaterpillarGraphics : GraphicsModule, IMuddableGraphics
{
    public const int TUBE_SPRITE = 0;
    public Caterpillar Crit;
    public LightSource? TailLight, FlatTailLight;
    public ChunkDynamicSoundLoop SoundLoop;
    public GenericBodyPart[][][] Whiskers;
    public Vector2[][] BodyRotations;
    public Limb[][] Legs;
    public float[] LegLengths;
    public int[] BodyVars;
    public int TotSegs, TotalSecondarySegments, EffectColor;
    public float Darkness, LastDarkness, Hue, Saturation, WalkCycle, BodyDir, LastBodyDir, DefaultRotat, LightAlpha;
    public Color BlackColor, GlowColor;

    public CaterpillarGraphics(Caterpillar ow) : base(ow, false)
    {
        var chs = ow.bodyChunks;
        cullRange = 400f;
        Crit = ow;
        if (ow.Glowing)
        {
            if (ow.abstractCreature.Albino())
                EffectColor = 2;
            else if (ow.abstractCreature.superSizeMe)
                EffectColor = 1;
        }
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        TotSegs = chs.Length;
        TotalSecondarySegments = TotSegs - 1;
        DefaultRotat = Mathf.Lerp(-5f, 5f, Random.value);
        var rots = BodyRotations = new Vector2[3][];
        var defVec = Custom.DegToVec(DefaultRotat);
        for (var i = 0; i < rots.Length; i++)
            rots[i] = [defVec, defVec];
        Hue = Mathf.Lerp(.04f, .1f, Random.value);
        Saturation = .9f;
        var legs = Legs = new Limb[TotSegs][];
        LegLengths = new float[TotSegs];
        BodyVars = new int[TotSegs];
        for (var l = 0; l < legs.Length; l++)
        {
            BodyVars[l] = Random.Range(1, 5);
            LegLengths[l] = Mathf.Lerp(10f, 25f, Mathf.Sin((float)l / (TotSegs - 1) * Mathf.PI)) * .6f;
            var ch = chs[l];
            legs[l] = [new(this, ch, l * 2, 2f, .5f, .9f, 7f, .8f), new(this, ch, l * 2 + 1, 2f, .5f, .9f, 7f, .8f)];
        }
        LastDarkness = -1f;
        var wh = Whiskers = new GenericBodyPart[2][][];
        bodyParts = new BodyPart[TotSegs * 2 + 8];
        var num8 = 0;
        for (var n = 0; n < legs.Length; n++)
        {
            var ln = legs[n];
            for (var num9 = 0; num9 < ln.Length; num9++)
            {
                bodyParts[num8] = ln[num9];
                ++num8;
            }
        }
        for (var num10 = 0; num10 < wh.Length; num10++)
        {
            var wh1 = wh[num10] = new GenericBodyPart[2][];
            for (var num11 = 0; num11 < wh1.Length; num11++)
            {
                var wh2 = wh1[num11] = new GenericBodyPart[2];
                for (var num12 = 0; num12 < wh2.Length; num12++)
                {
                    bodyParts[num8] = wh2[num12] = new(this, 1f, .5f, .9f, num10 == 0 ? chs[0] : chs[chs.Length - 1]);
                    ++num8;
                }
            }
        }
        SoundLoop = new(ow.mainBodyChunk);
        Random.state = state;
    }

    public virtual int SecondarySegmentSprite(int s) => 1 + s;

    public virtual int SegmentSprite(int s) => 1 + TotalSecondarySegments + s;

    public virtual int WhiskerSprite(int end, int side, int pos) => 1 + TotalSecondarySegments + TotSegs + end * 4 + side * 2 + pos;

    public virtual int LegSprite(int segment, int side, int part) => 9 + TotalSecondarySegments + TotSegs + segment * 4 + side * 2 + part;

    public virtual int DotsSprite(int s) => 9 + TotalSecondarySegments + TotSegs * 5 + s;

    public override void Update()
    {
        base.Update();
        SoundLoop.Update();
        var chs = Crit.bodyChunks;
        if (Crit.Glowing)
        {
            if (TailLight is LightSource lh)
            {
                lh.stayAlive = true;
                lh.HardSetPos(chs[chs.Length - 1].pos);
                var rd = Crit.Consious ? Math.Min(lh.Rad + 11f, 300f) : Math.Max(lh.Rad - 5f, 0f);
                lh.setRad = rd;
                LightAlpha = rd / 300f;
                lh.color = GlowColor;
                if (lh.slatedForDeletetion)
                    TailLight = null;
            }
            else if (Crit.room is Room rm)
                rm.AddObject(TailLight = new(chs[chs.Length - 1].pos, false, Color.white, Crit, true)
                {
                    requireUpKeep = true,
                    lastAlpha = .5f,
                    alpha = .5f,
                    lastRad = 0f,
                    rad = 0f
                });
            if (FlatTailLight is LightSource flh)
            {
                flh.stayAlive = true;
                flh.HardSetPos(chs[chs.Length - 1].pos);
                flh.setRad = Crit.Consious ? Math.Min(flh.Rad + (11f / 15f), 20f) : Math.Max(flh.Rad - (1f / 3f), 0f);
                flh.color = GlowColor;
                if (flh.slatedForDeletetion)
                    FlatTailLight = null;
            }
            else if (Crit.room is Room rm)
                rm.AddObject(FlatTailLight = new(chs[chs.Length - 1].pos, false, Color.white, Crit, true)
                {
                    flat = true,
                    requireUpKeep = true,
                    lastAlpha = .5f,
                    alpha = .5f,
                    lastRad = 0f,
                    rad = 0f
                });
        }
        if (Crit.dead)
            SoundLoop.Volume = 0f;
        else
        {
            SoundLoop.sound = NewSoundID.M4R_Caterpillar_Crawl_LOOP;
            var mc = Crit.mainBodyChunk;
            SoundLoop.Volume = !Crit.Moving ? 0f : Mathf.InverseLerp(Vector2.Distance(mc.lastPos, mc.pos), .5f, 2f) * 1.1f;
            SoundLoop.Pitch = .95f;
        }
        if (Crit.Moving && Crit.Consious)
            WalkCycle -= 1f / 10f;
        LastBodyDir = BodyDir;
        BodyDir = Mathf.Lerp(BodyDir, -1f, .1f);
        var rots = BodyRotations;
        for (var i = 0; i < rots.Length; i++)
        {
            var rot = rots[i];
            rot[1] = rot[0];
            var num = i switch
            {
                1 => chs.Length / 2,
                0 => 0,
                _ => chs.Length - 1,
            };
            rot[0] = Vector3.Slerp(rot[0], BestBodyRotatAtChunk(num), Crit.Moving ? .4f : .01f);
        }
        var p = chs[0].pos + Custom.DirVec(chs[1].pos, chs[0].pos);
        var legs = Legs;
        for (var j = 0; j < legs.Length; j++)
        {
            var num2 = (float)j / (TotSegs - 1);
            var ch = chs[j];
            Vector2 pos = ch.pos,
                vector2 = Custom.DirVec(p, pos),
                vector3 = Custom.PerpendicularVector(vector2),
                vector4 = RotatAtChunk(j, 1f);
            var num3 = .5f + .5f * Mathf.Sin((WalkCycle + j / 10f) * (Mathf.PI * 2f));
            var leg = legs[j];
            for (var k = 0; k < leg.Length; k++)
            {
                var legPart = leg[k];
                legPart.Update();
                var vector5 = pos + vector3 * ((k == 0 ? -1f : 1f) * vector4.y * ch.rad);
                var lgt = LegLengths[j];
                Vector2 vector6 = Vector3.Slerp(vector2 * Mathf.Lerp(Mathf.Lerp(-1f, 1f, num2), -1f, Math.Abs(num3 - .5f)), Vector3.Slerp(vector3 * ((k == 0 ? -1f : 1f) * vector4.y), vector3 * vector4.x, Math.Abs(vector4.x)), Mathf.Lerp(.5f + .5f * Mathf.Sin(num2 * Mathf.PI), 0f, Math.Abs(num3 - .5f) * 2f)).normalized,
                    vector7 = vector5 + vector6 * lgt;
                legPart.ConnectToPoint(vector5, lgt, false, 0f, ch.vel, .1f, 0f);
                if (Crit.Consious && !legPart.reachedSnapPosition)
                    legPart.FindGrip(Crit.room, vector5, vector5, lgt * 1.5f, vector7, -2, -2, true);
                if (!Crit.Consious || !Custom.DistLess(legPart.pos, legPart.absoluteHuntPos, LegLengths[j] * 1.5f))
                {
                    legPart.mode = Limb.Mode.Dangle;
                    legPart.vel += vector6 * 13f;
                    legPart.vel = Vector2.Lerp(legPart.vel, vector7 - legPart.pos, .5f);
                }
                else
                    legPart.vel += vector6 * 5f;
            }
        }
        var whs = Whiskers;
        for (var l = 0; l < whs.Length; l++)
        {
            var vector8 = l != 0 ? chs[chs.Length - 1].pos : chs[0].pos;
            var whl = whs[l];
            for (var m = 0; m < whl.Length; m++)
            {
                var whm = whl[m];
                for (var n = 0; n < whm.Length; n++)
                {
                    var wh = whm[n];
                    wh.Update();
                    var lg = n == 0 ? 8f : 14f;
                    var dir = WhiskerDir(l, m, n, 1f);
                    wh.ConnectToPoint(vector8, lg, false, 0f, default, 0f, 0f);
                    wh.vel += (vector8 + dir * lg - wh.pos) / 30f + dir;
                    wh.vel.y -= .3f;
                    if (Crit.Consious && !Crit.Moving)
                        wh.pos += Custom.RNV() * (.25f * (l == 0 ? 2f : .8f));
                }
            }
        }
    }

    public virtual Vector2 BestBodyRotatAtChunk(int chunk)
    {
        var chs = Crit.bodyChunks;
        var ch = chs[chunk];
        var v = Custom.PerpendicularVector(chunk == 0 ? Custom.DirVec(ch.pos, chs[1].pos) : (chunk != chs.Length - 1 ? Custom.DirVec(chs[chs.Length / 2 - 1].pos, chs[chs.Length / 2 + 1].pos) : Custom.DirVec(chs[chs.Length - 2].pos, chs[chs.Length - 1].pos)));
        var num = 0f;
        if (Crit.room.GetTile(ch.pos + v * 20f).Solid)
            num += 1f;
        if (Crit.room.GetTile(ch.pos - v * 20f).Solid)
            num -= 1f;
        if (num == 0f && (Crit.room.GetTile(ch.pos).verticalBeam || Crit.room.GetTile(ch.pos).horizontalBeam))
            return new Vector2(DefaultRotat * .01f, -1f).normalized;
        return Vector3.Slerp(new Vector2(-1f, .15f).normalized, new Vector2(1f, .15f).normalized, Mathf.InverseLerp(-1f, 1f, num + DefaultRotat * .01f));
    }

    public virtual Vector2 RotatAtChunk(int chunk, float timeStacker)
    {
        var lg = Crit.bodyChunks.Length;
        var b1 = BodyRotations[1];
        if (chunk <= lg / 2)
        {
            var b0 = BodyRotations[0];
            return Vector3.Slerp(Vector3.Slerp(b0[1], b0[0], timeStacker), Vector3.Slerp(b1[1], b1[0], timeStacker), Mathf.InverseLerp(0f, lg / 2, chunk));
        }
        var b2 = BodyRotations[2];
        return Vector3.Slerp(Vector3.Slerp(b1[1], b1[0], timeStacker), Vector3.Slerp(b2[1], b2[0], timeStacker), Mathf.InverseLerp(lg / 2, lg - 1, chunk));
    }

    public virtual Vector2 WhiskerDir(int end, int side, int part, float timeStacker)
    {
        BodyChunk ch0, ch1;
        var chs = Crit.bodyChunks;
        Vector2 vector, vector2;
        if (end == 0)
        {
            ch1 = chs[1];
            ch0 = chs[0];
            vector = Custom.DirVec(Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker), Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker));
            vector2 = RotatAtChunk(0, timeStacker);
        }
        else
        {
            ch1 = chs[chs.Length - 2];
            ch0 = chs[chs.Length - 1];
            vector = Custom.DirVec(Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker), Vector2.Lerp(ch0.lastPos, ch0.pos, timeStacker));
            vector2 = RotatAtChunk(chs.Length - 1, timeStacker);
        }
        var vector3 = Custom.PerpendicularVector(vector) * (end == 0 ? -1f : 1f);
        return (vector + (Vector2)Vector3.Slerp(vector3 * ((side == 0 ? -1f : 1f) * vector2.y * (part == 0 ? .4f : 1.4f)), vector3 * (vector2.x * (part == 0 ? .25f : -.5f)), Math.Abs(vector2.x))).normalized;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var chs = Crit.bodyChunks;
        var sprites = sLeaser.sprites = new FSprite[9 + TotalSecondarySegments + TotSegs * 6];
        sprites[TUBE_SPRITE] = TriangleMesh.MakeLongMesh(chs.Length, false, false);
        var hairy = !Crit.Glowing && Crit.abstractCreature.superSizeMe;
        for (var i = 0; i < chs.Length; i++)
        {
            var vari = BodyVars[i].ToString();
            if (hairy)
                vari += "Alt";
            sprites[SegmentSprite(i)] = new("CTPSegment" + vari)
            {
                scaleY = chs[i].rad * (1.8f * (1f / 12f))
            };
            sprites[DotsSprite(i)] = new(i == 0 ? "CTPEyes" : (hairy ? "pixel" : ("CTPDots" + vari)));
            for (var k = 0; k < 2; k++)
            {
                sprites[LegSprite(i, k, 0)] = new("CentipedeLegA");
                sprites[LegSprite(i, k, 1)] = new("CentipedeLegB");
            }
        }
        for (var l = 0; l < TotalSecondarySegments; l++)
            sprites[SecondarySegmentSprite(l)] = new("pixel")
            {
                scaleY = chs[l].rad
            };
        for (var m = 0; m < 2; m++)
        {
            for (var n = 0; n < 2; n++)
            {
                for (var num = 0; num < 2; num++)
                    sprites[WhiskerSprite(m, n, num)] = TriangleMesh.MakeLongMesh(4, true, false);
            }
        }
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
        {
            var node = sprites[i];
            node.RemoveFromContainer();
            newContainer.AddChild(node);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (culled)
            return;
        var sprites = sLeaser.sprites;
        var mbc = Crit.mainBodyChunk;
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(Vector2.Lerp(mbc.lastPos, mbc.pos, timeStacker));
        Darkness *= 1f - .5f * rCam.room.LightSourceExposure(Vector2.Lerp(mbc.lastPos, mbc.pos, timeStacker));
        if (LastDarkness != Darkness || (Crit.Glowing && GlowColor == default))
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var chs = Crit.bodyChunks;
        Vector2 vector = default,
            vector2 = Vector2.Lerp(chs[0].lastPos, chs[0].pos, timeStacker);
        vector2 += Custom.DirVec(Vector2.Lerp(chs[1].lastPos, chs[1].pos, timeStacker), vector2) * 10f;
        var glowing = Crit.Glowing;
        var hairy = !glowing && Crit.abstractCreature.superSizeMe;
        var col = Color.Lerp(BlackColor, GlowColor, LightAlpha);
        var clr = Custom.HSL2RGB(Hue, Saturation, .5f);
        for (var i = 0; i < chs.Length; i++)
        {
            var num = (float)i / (chs.Length - 1);
            var ch = chs[i];
            Vector2 normalized = RotatAtChunk(i, timeStacker).normalized,
                vector3 = Vector2.Lerp(ch.lastPos, ch.pos, timeStacker),
                vector4 = i < chs.Length - 1 ? Vector2.Lerp(chs[i + 1].lastPos, chs[i + 1].pos, timeStacker) : (vector3 + Custom.DirVec(vector2, vector3) * 10f),
                normalized2 = (vector2 - vector4).normalized,
                vector5 = Custom.PerpendicularVector(normalized2);
            float num2 = Vector2.Distance(vector3, vector2) / 4f,
                num3 = Vector2.Distance(vector3, vector4) / 4f,
                num5 = i == 0 ? 1f : 3f;
            var tube = (sprites[TUBE_SPRITE] as TriangleMesh)!;
            tube.MoveVertice(i * 4, vector3 - vector5 * 3f + normalized2 * num2 - camPos);
            tube.MoveVertice(i * 4 + 1, vector3 + vector5 * 3f + normalized2 * num2 - camPos);
            tube.MoveVertice(i * 4 + 2, vector3 - vector5 * num5 - normalized2 * num3 - camPos);
            tube.MoveVertice(i * 4 + 3, vector3 + vector5 * num5 - normalized2 * num3 - camPos);
            var num6 = Mathf.Clamp(Mathf.Sin(num * Mathf.PI), 0f, 1f) * .5f;
            var seg = sprites[SegmentSprite(i)];
            seg.SetPosition(vector3 - camPos);
            seg.rotation = Custom.VecToDeg((vector2 - vector4).normalized);
            seg.scaleX = ch.rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, .9f, Math.Abs(normalized.x)), num6) * (2f * .0625f);
            if (glowing && i == chs.Length - 1)
                seg.color = col;
            var dots = sprites[DotsSprite(i)];
            if (i == 0 || (!hairy && normalized.y > 0f && (!glowing || i != chs.Length - 1)))
            {
                dots.isVisible = true;
                dots.scaleX = ch.rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, .9f, Math.Abs(normalized.x)), num6) * normalized.y * (1.8f * (1f / 14f));
                dots.scaleY = ch.rad * (1.5f * (1f / 11f));
                dots.SetPosition(vector3 + Custom.PerpendicularVector(normalized2) * (normalized.x * ch.rad * 1.1f) - camPos);
                dots.rotation = Custom.VecToDeg((vector2 - vector4).normalized);
                if (i == 0)
                    dots.color = !Crit.Consious ? BlackColor : (glowing ? Color.Lerp(GlowColor, Color.white, .33f * LightAlpha) : Color.Lerp(Custom.HSL2RGB(Hue, Saturation, .7f), BlackColor, Darkness * .2f + .5f));
                else
                    dots.color = glowing ? col : Color.Lerp(clr, BlackColor, Darkness * .2f + .8f);
            }
            else
                dots.isVisible = false;
            if (i > 0)
            {
                var seg2 = sprites[SecondarySegmentSprite(i - 1)];
                seg2.SetPosition(Vector2.Lerp(vector2, vector3, .5f) - camPos);
                seg2.rotation = Custom.VecToDeg(Vector3.Slerp(vector, normalized2, .5f));
                seg2.scaleX = ch.rad * Mathf.Lerp(.9f, Mathf.Lerp(1.1f, .8f, Math.Abs(normalized.x)), num6) * 2f;
            }
            vector2 = vector3;
            vector = normalized2;
            var leg = Legs[i];
            var lgt = LegLengths[i];
            for (var l = 0; l < leg.Length; l++)
            {
                var legPart = leg[l];
                Vector2 vector7 = vector3 - vector5 * ((l == 0 ? -1f : 1f) * normalized.y * ch.rad),
                    vector8 = Vector2.Lerp(legPart.lastPos, legPart.pos, timeStacker);
                var f = Mathf.Lerp(-1f, 1f, Mathf.Clamp(num - BodyDir * .4f, 0f, 1f)) * Mathf.Lerp(l == 0 ? 1f : -1f, 0f - normalized.x, Math.Abs(normalized.x));
                f = Mathf.Pow(Math.Abs(f), .2f) * Mathf.Sign(f);
                var vector9 = Custom.InverseKinematic(vector7, vector8, lgt / 2f, lgt / 2f, f);
                var legPart1 = sprites[LegSprite(i, l, 0)];
                var legPart2 = sprites[LegSprite(i, l, 1)];
                legPart1.SetPosition(vector7 - camPos);
                legPart1.rotation = Custom.AimFromOneVectorToAnother(vector7, vector9);
                legPart1.scaleY = Vector2.Distance(vector7, vector9) / 27f;
                legPart2.anchorY = legPart1.anchorY = .1f;
                legPart2.scaleX = legPart1.scaleX = -Mathf.Sign(f) * 1.5f;
                legPart2.SetPosition(vector9 - camPos);
                legPart2.rotation = Custom.AimFromOneVectorToAnother(vector9, vector8);
                legPart2.scaleY = Vector2.Distance(vector9, vector8) / 25f;
                if (glowing && i == chs.Length - 1)
                    legPart1.color = legPart2.color = col;
            }
        }
        if (hairy)
        {
            var mbci = mbc.index;
            var mbcip1 = mbci + 1;
            var mbcim1 = mbci - 1;
            sprites[LegSprite(mbcip1, 0, 0)].color = sprites[LegSprite(mbcip1, 0, 1)].color = sprites[LegSprite(mbcip1, 1, 0)].color = sprites[LegSprite(mbcip1, 1, 1)].color = sprites[SegmentSprite(mbcip1)].color = sprites[LegSprite(mbcim1, 0, 0)].color = sprites[LegSprite(mbcim1, 0, 1)].color = sprites[LegSprite(mbcim1, 1, 0)].color = sprites[LegSprite(mbcim1, 1, 1)].color = sprites[SegmentSprite(mbcim1)].color = Color.Lerp(clr, BlackColor, Darkness * .06f + .94f);
            sprites[LegSprite(mbci, 0, 0)].color = sprites[LegSprite(mbci, 0, 1)].color = sprites[LegSprite(mbci, 1, 0)].color = sprites[LegSprite(mbci, 1, 1)].color = sprites[SegmentSprite(mbci)].color = Color.Lerp(clr, BlackColor, Darkness * .12f + .88f);
        }
        for (var m = 0; m < 2; m++)
        {
            BodyChunk ch1, ch2;
            Vector2 vector10, vector11;
            if (m == 0)
            {
                ch1 = chs[0];
                ch2 = chs[1];
                vector10 = Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker);
                vector11 = Custom.DirVec(Vector2.Lerp(ch2.lastPos, ch2.pos, timeStacker), vector10);
            }
            else
            {
                ch1 = chs[chs.Length - 1];
                ch2 = chs[chs.Length - 2];
                vector10 = Vector2.Lerp(ch1.lastPos, ch1.pos, timeStacker);
                vector11 = Custom.DirVec(Vector2.Lerp(ch2.lastPos, ch2.pos, timeStacker), vector10);
            }
            var whm = Whiskers[m];
            for (var n = 0; n < 2; n++)
            {
                for (var num11 = 0; num11 < 2; num11++)
                {
                    var wh = whm[num11][n];
                    var vector12 = Vector2.Lerp(wh.lastPos, wh.pos, timeStacker);
                    vector2 = vector10;
                    var whisker = (sprites[WhiskerSprite(m, n, num11)] as TriangleMesh)!;
                    if (glowing && m != 0)
                        whisker.color = col;
                    for (var num14 = 0; num14 < 4; num14++)
                    {
                        Vector2 vector13 = Custom.Bezier(vector10, vector10 + vector11 * (Vector2.Distance(vector10, vector12) * .7f), vector12, vector12, num14 / 3f),
                            normalized3 = (vector13 - vector2).normalized,
                            vector14 = Custom.PerpendicularVector(normalized3);
                        var num15 = Vector2.Distance(vector13, vector2) / (num14 == 0 ? 1f : 5f);
                        whisker.MoveVertice(num14 * 4, vector2 - vector14 + normalized3 * num15 - camPos);
                        whisker.MoveVertice(num14 * 4 + 1, vector2 + vector14 + normalized3 * num15 - camPos);
                        if (num14 < 3)
                        {
                            whisker.MoveVertice(num14 * 4 + 2, vector13 - vector14 - normalized3 * num15 - camPos);
                            whisker.MoveVertice(num14 * 4 + 3, vector13 + vector14 - normalized3 * num15 - camPos);
                        }
                        else
                            whisker.MoveVertice(num14 * 4 + 2, vector13 + normalized3 * 2.1f - camPos);
                        vector2 = vector13;
                    }
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var glowing = Crit.Glowing;
        if (glowing)
            GlowColor = EffectColor == 2 ? Color.white : Color.Lerp(palette.texture.GetPixel(30, 5 - EffectColor * 2), Color.white, .05f);
        BlackColor = palette.blackColor;
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
            sprites[i].color = BlackColor;
        var hairy = !glowing && Crit.abstractCreature.superSizeMe;
        var col = Color.Lerp(BlackColor, GlowColor, LightAlpha);
        var clr = Custom.HSL2RGB(Hue, Saturation, .5f);
        var chsL = Crit.bodyChunks.Length;
        var lt = chsL - 1;
        var mbc = Crit.mainBodyChunk;
        for (var i = 0; i < chsL; i++)
        {
            var dots = sprites[DotsSprite(i)];
            if (i == 0 || (!hairy && RotatAtChunk(i, 1f).normalized.y > 0f && (!glowing || i != lt)))
            {
                if (i == 0)
                    dots.color = !Crit.Consious ? BlackColor : (glowing ? Color.Lerp(GlowColor, Color.white, .33f * LightAlpha) : Color.Lerp(Custom.HSL2RGB(Hue, Saturation, .7f), BlackColor, Darkness * .2f + .5f));
                else
                    dots.color = glowing ? col : Color.Lerp(clr, BlackColor, Darkness * .2f + .8f);
            }
            if (glowing && i == lt)
            {
                sprites[SegmentSprite(i)].color = col;
                var leg = Legs[i];
                for (var l = 0; l < leg.Length; l++)
                    sprites[LegSprite(i, l, 0)].color = sprites[LegSprite(i, l, 1)].color = col;
            }
        }
        if (hairy)
        {
            var mbci = mbc.index;
            var mbcip1 = mbci + 1;
            var mbcim1 = mbci - 1;
            sprites[LegSprite(mbcip1, 0, 0)].color = sprites[LegSprite(mbcip1, 0, 1)].color = sprites[LegSprite(mbcip1, 1, 0)].color = sprites[LegSprite(mbcip1, 1, 1)].color = sprites[SegmentSprite(mbcip1)].color = sprites[LegSprite(mbcim1, 0, 0)].color = sprites[LegSprite(mbcim1, 0, 1)].color = sprites[LegSprite(mbcim1, 1, 0)].color = sprites[LegSprite(mbcim1, 1, 1)].color = sprites[SegmentSprite(mbcim1)].color = Color.Lerp(clr, BlackColor, Darkness * .06f + .94f);
            sprites[LegSprite(mbci, 0, 0)].color = sprites[LegSprite(mbci, 0, 1)].color = sprites[LegSprite(mbci, 1, 0)].color = sprites[LegSprite(mbci, 1, 1)].color = sprites[SegmentSprite(mbci)].color = Color.Lerp(clr, BlackColor, Darkness * .12f + .88f);
        }
        if (glowing)
        {
            for (var n = 0; n < 2; n++)
            {
                for (var num11 = 0; num11 < 2; num11++)
                    (sprites[WhiskerSprite(1, n, num11)] as TriangleMesh)!.color = col;
            }
        }
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite < DotsSprite(0);

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
    {
        if (Crit.bodyChunks is not BodyChunk[] chs)
            return;
        var sprites = sLeaser.sprites;
        for (var i = 1; i < chs.Length; i++)
            mudOverlay.layeringMap[mudOverlay.GetMudSprite(SegmentSprite(i))] = sprites[DotsSprite(i)];
    }
}