using RWCustom;
using Smoke;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class SporantulaGraphics : BigSpiderGraphics, IMuddableGraphics
{
    public SporantulaScale[] Scales;
    public SporantulaDots[] Dots;
    public int OrigNumOfSprites;

    public SporantulaGraphics(Sporantula ow) : base(ow)
    {
        var dots = Dots = new SporantulaDots[scaleStuckPositions.Length / 2 - 2];
        var scls = Scales = new SporantulaScale[scaleStuckPositions.Length / 2];
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var sclRnd = Random.value * 4f - 2f;
        for (var i = 0; i < scls.Length; i++)
            scls[i] = new(this, sclRnd);
        for (var i = 0; i < dots.Length; i++)
            dots[i] = new(this, (Random.value - .5f) * i);
        Random.state = state;
        bodyThickness *= 1.5f;
        legLength *= 1.15f;
    }

    public override void Update()
    {
        base.Update();
        var scls = Scales;
        var b1 = bug.bodyChunks[1];
        var b1pos = b1.pos;
        var b1lastPos = b1.pos;
        for (var j = 0; j < scls.Length; j++)
        {
            if (scls[j] is SporantulaScale s)
            {
                var scly = scaleStuckPositions[j * 2].y;
                var bse = new Vector2(s.BaseY, s.BaseY);
                s.LastPos = Vector2.Lerp(tailEnd.lastPos, b1lastPos, scly) + bse;
                s.Pos = Vector2.Lerp(tailEnd.pos, b1pos, scly) + bse;
                s.Update();
            }
        }
        var dots = Dots;
        for (var j = 0; j < dots.Length; j++)
        {
            if (dots[j] is SporantulaDots dot)
            {
                var scly = scaleStuckPositions[(j + 2) * 2].y * 1.4f;
                var bse = new Vector2(dot.BaseY, dot.BaseY);
                dot.LastPos = Vector2.Lerp(tailEnd.lastPos, b1lastPos, scly) + bse;
                dot.Pos = Vector2.Lerp(tailEnd.pos, b1pos, scly) + bse;
            }
        }
        if (bug is Sporantula spo && spo.dead is false && bug.AI?.preyTracker?.currentPrey?.critRep?.representedCreature is AbstractCreature crit && crit.SameRippleLayer(spo.abstractPhysicalObject) && crit.NoCamo() && Random.value < .6f && SporeMemory.TryGetValue(bug.abstractCreature, out var mem) && mem.Contains(crit))
            spo.Angry();
    }

    public override void Reset()
    {
        base.Reset();
        if (Scales is SporantulaScale[] ar)
        {
            for (var i = 0; i < ar.Length; i++)
                ar[i]?.Reset();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var cont = rCam.ReturnFContainer("Midground");
        var mesh = sLeaser.sprites[MeshSprite] = TriangleMesh.MakeLongMesh(7, false, true);
        var sh = mesh.shader = Custom.rainWorld.Shaders["JaggedSquare"];
        mesh.alpha = .75f;
        mesh.RemoveFromContainer();
        cont.AddChild(mesh);
        OrigNumOfSprites = sLeaser.sprites.Length;
        var dots = Dots;
        var scls = Scales;
        int k = OrigNumOfSprites + 1, sclLgt = scls.Length, num = k + sclLgt, n = k + sclLgt;
        for (var i = 0; i < dots.Length; i++)
            num += dots[i].Dots.Length * 2;
        Array.Resize(ref sLeaser.sprites, num);
        var spr = sLeaser.sprites[OrigNumOfSprites] = TriangleMesh.MakeLongMesh(7, false, true);
        spr.shader = sh;
        spr.RemoveFromContainer();
        rCam.ReturnFContainer("Midground").AddChild(spr);
        for (var j = 0; j < scls.Length; j++)
            scls[j]?.InitiateSprites(k + j, sLeaser, rCam);
        for (var j = 0; j < dots.Length; j++)
        {
            if (dots[j] is SporantulaDots dot)
                dot.InitiateSprites(n + j * dot.Dots.Length * 2, sLeaser, rCam);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        var num2 = Mathf.Lerp(lastMandiblesCharge, mandiblesCharge, timeStacker);
        var vector2 = Vector2.Lerp(bug.bodyChunks[1].lastPos, bug.bodyChunks[1].pos, timeStacker) + Custom.RNV() * Random.value * 3.5f * num2;
        var scls = Scales;
        for (var m = 0; m < scls.Length; m++)
            scls[m]?.DrawSprites(OrigNumOfSprites + 1 + m, sLeaser, timeStacker, camPos);
        var dots = Dots;
        for (var j = 0; j < dots.Length; j++)
        {
            if (dots[j] is SporantulaDots dot)
                dot.DrawSprites(OrigNumOfSprites + 1 + scls.Length + j * dot.Dots.Length * 2, sLeaser, timeStacker, camPos);
        }
        var scals = scales;
        for (var m = 0; m < scals.Length; m++)
        {
            var scalsm = scals[m];
            for (var n = 0; n < scalsm.GetLength(0); n++)
                sLeaser.sprites[FirstScaleSprite + (int)scalsm[n, 3].x].isVisible = false;
        }
        for (var l = 0; l < 2; l++)
        {
            var spr = (sLeaser.sprites[MandibleSprite(l, 1)] as CustomFSprite)!;
            spr.verticeColors[2] = spr.verticeColors[3] = blackColor;
            spr.verticeColors[0] = spr.verticeColors[1] = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), rCam.currentPalette.texture.GetPixel(11, 4), .5f), blackColor, .6f * (1f - num2) + .4f * darkness + .2f);
        }
        var colr = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), rCam.currentPalette.texture.GetPixel(11, 4), .5f), rCam.currentPalette.blackColor, rCam.currentPalette.darkness * .5f);
        var mesh = (sLeaser.sprites[MeshSprite] as TriangleMesh)!;
        mesh.color = colr;
        var mesh2 = (sLeaser.sprites[OrigNumOfSprites] as TriangleMesh)!;
        var vector22 = Vector2.Lerp(bug.mainBodyChunk.lastPos, bug.mainBodyChunk.pos, timeStacker);
        var vector3 = Custom.DirVec(vector2, vector22);
        var b = Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker);
        var vector5 = vector22 + vector3;
        var num3 = 0f;
        for (var i = 0; i < 7; i++)
        {
            var f = Mathf.InverseLerp(0f, 6f, i);
            var vector6 = Custom.Bezier(vector22 + vector3 * 3f, vector2, b, vector2, f);
            var num4 = Mathf.Lerp(2.5f, 10f + Mathf.Sin(Mathf.Lerp(lastBreathCounter, breathCounter, timeStacker) * .1f), Mathf.Sin(Mathf.Pow(f, .75f) * Mathf.PI)) * bodyThickness * .675f;
            var vector7 = Custom.PerpendicularVector(vector6, vector5);
            mesh2.MoveVertice(i * 4, (vector5 + vector6) * .5f - vector7 * (num4 + num3) * .5f - camPos);
            mesh2.MoveVertice(i * 4 + 1, (vector5 + vector6) * .5f + vector7 * (num4 + num3) * .5f - camPos);
            mesh2.MoveVertice(i * 4 + 2, vector6 - vector7 * num4 - camPos);
            mesh2.MoveVertice(i * 4 + 3, vector6 + vector7 * num4 - camPos);
            vector5 = vector6;
            num3 = num4;
        }
        mesh2.color = Color.Lerp(colr, Color.Lerp(Color.white, rCam.currentPalette.blackColor, rCam.currentPalette.darkness * .25f), .3f);
        mesh2.alpha = .5f;
        if (bug is BigSpider bs && bs.dead)
            mesh.isVisible = mesh2.isVisible = false;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        var clr = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), palette.texture.GetPixel(11, 4), .5f), palette.blackColor, palette.darkness * .5f);
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
            sprites[i].color = clr;
        for (var l = 0; l < 2; l++)
        {
            var spr = (sprites[MandibleSprite(l, 1)] as CustomFSprite)!;
            spr.verticeColors[2] = spr.verticeColors[3] = blackColor;
            spr.verticeColors[0] = spr.verticeColors[1] = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), palette.texture.GetPixel(11, 4), .5f), blackColor, .6f * (1f - mandiblesCharge) + .4f * darkness + .2f);
        }
        (sprites[OrigNumOfSprites] as TriangleMesh)!.color = Color.Lerp(clr, Color.Lerp(Color.white, palette.blackColor, palette.darkness * .25f), .3f);
        var scls = Scales;
        var dots = Dots;
        for (var j = 0; j < scls.Length; j++)
            scls[j]?.ApplyPalette(OrigNumOfSprites + 1 + j, sLeaser, palette);
        for (var j = 0; j < dots.Length; j++)
        {
            if (dots[j] is SporantulaDots dot)
                dot.ApplyPalette(OrigNumOfSprites + 1 + scls.Length + j * dot.Dots.Length * 2, sLeaser, palette);
        }
    }

    public virtual new bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => false;

    [StructLayout(LayoutKind.Sequential)]
    public class SporantulaScale
    {
        public Vector2[][] Segments;
        public SporesSmoke? Smoke;
        public BigSpiderGraphics Graphics;
        public Color SporeColor;
        public Vector2 Pos, LastPos;
        public float BaseY;

        public virtual Room? Room => Graphics?.bug?.room;

        public SporantulaScale(BigSpiderGraphics graphicsModule, float baseY)
        {
            LastPos = Pos = graphicsModule.bug.mainBodyChunk.pos;
            Graphics = graphicsModule;
            Segments = new Vector2[(int)Mathf.Lerp(3f, 15f, Random.value)][];
            var segs = Segments;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i] = new Vector2[3];
                seg[0] = Pos + new Vector2(0f, 5f * i);
                seg[1] = seg[0];
            }
            BaseY = baseY;
        }

        public virtual void Reset()
        {
            if (Graphics?.bug?.mainBodyChunk is BodyChunk b)
                LastPos = Pos = b.pos;
            var segs = Segments;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                seg[1] = seg[0] = Pos + new Vector2(0f, 5f * i);
                seg[2] *= 0f;
            }
        }

        public virtual void Update()
        {
            var segs = Segments;
            var l = segs.Length;
            for (var j = 0; j < l; j++)
            {
                var seg = segs[j];
                var num = (float)j / (l - 1);
                seg[1] = seg[0];
                seg[0] += seg[2];
                seg[2] *= Mathf.Lerp(1f, .85f, num);
                seg[2] += Vector2.Lerp(default, new Vector2(Mathf.Clamp(Pos.x - seg[0].x, -2f, 2f) * .0025f, .25f) * (1f - num), Mathf.Pow(num, .01f));
                seg[2].y += .01f;
                ConnectSegment(j);
            }
            for (var num2 = l - 1; num2 >= 0; num2--)
                ConnectSegment(num2);
            if (Room is not Room rm || Graphics?.bug is not BigSpider b)
                return;
            if (Smoke is SporesSmoke smoke)
            {
                var seg = Segments[l - 1];
                if (rm.ViewedByAnyCamera(Pos, 300f) && !b.dead)
                    smoke.EmitSmoke(seg[0], Custom.DirVec(Segments[l - 2][0], seg[0]) + Custom.RNV() + seg[2], SporeColor);
                if (b.dead || smoke.slatedForDeletetion || smoke.room != rm || b.inShortcut)
                    Smoke = null;
            }
            else
                rm.AddObject(Smoke = new(rm));
        }

        public virtual void ConnectSegment(int i)
        {
            var seg = Segments[i];
            if (i == 0)
            {
                var vec = Custom.DirVec(seg[0], Pos) * (5f - Vector2.Distance(seg[0], Pos));
                seg[0] -= vec;
                seg[2] -= vec;
            }
            else
            {
                var seg2 = Segments[i - 1];
                var vector2 = Custom.DirVec(seg[0], seg2[0]);
                float num2 = Vector2.Distance(seg[0], seg2[0]), num3 = .52f;
                var vec = vector2 * (5f - num2);
                seg[0] -= vec * num3;
                seg[2] -= vec * num3;
                seg2[0] += vec * (1f - num3);
                seg2[2] += vec * (1f - num3);
            }
        }

        public virtual void InitiateSprites(int startIndex, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var spr = sLeaser.sprites[startIndex] = TriangleMesh.MakeLongMesh(Segments.Length, false, false);
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Midground").AddChild(spr);
        }

        public virtual void DrawSprites(int startIndex, RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
        {
            var vector3 = Vector2.Lerp(LastPos, Pos, timeStacker);
            var mesh = (sLeaser.sprites[startIndex] as TriangleMesh)!;
            var segs = Segments;
            for (var j = 0; j < segs.Length; j++)
            {
                var seg = segs[j];
                Vector2 vector4 = Vector2.Lerp(seg[1], seg[0], timeStacker), normalized = (vector4 - vector3).normalized, vector5 = Custom.PerpendicularVector(normalized);
                var num2 = Vector2.Distance(vector4, vector3) / 5f;
                if (j == 0)
                {
                    mesh.MoveVertice(j * 4, vector3 - vector5 * .5f - camPos);
                    mesh.MoveVertice(j * 4 + 1, vector3 + vector5 * .5f - camPos);
                }
                else
                {
                    mesh.MoveVertice(j * 4, vector3 - vector5 * .5f + normalized * num2 - camPos);
                    mesh.MoveVertice(j * 4 + 1, vector3 + vector5 * .5f + normalized * num2 - camPos);
                }
                mesh.MoveVertice(j * 4 + 2, vector4 - vector5 * .5f - normalized * num2 - camPos);
                mesh.MoveVertice(j * 4 + 3, vector4 + vector5 * .5f - normalized * num2 - camPos);
                vector3 = vector4;
            }
            mesh.isVisible = Graphics?.bug is BigSpider b && !b.dead;
        }

        public virtual void ApplyPalette(int startIndex, RoomCamera.SpriteLeaser sLeaser, RoomPalette palette)
        {
            var color = Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), palette.texture.GetPixel(11, 4), .5f), palette.blackColor, palette.darkness * .5f);
            sLeaser.sprites[startIndex].color = color;
            SporeColor = Color.Lerp(color, new(.02f, .1f, .08f), .85f);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SporantulaDots
    {
        public Vector2[] Dots;
        public BigSpiderGraphics Graphics;
        public Color SporeColor;
        public Vector2 Pos, LastPos;
        public float BaseY;

        public SporantulaDots(BigSpiderGraphics graphicsModule, float baseY)
        {
            BaseY = baseY;
            Graphics = graphicsModule;
            var dots = Dots = new Vector2[5];
            for (var i = 0; i < dots.Length; i++)
                dots[i] = Custom.DegToVec((float)i / dots.Length * 360f) * Random.value + Custom.RNV() * .2f;
            for (var j = 0; j < 3; j++)
            {
                for (var k = 0; k < dots.Length; k++)
                {
                    for (var l = 0; l < dots.Length; l++)
                    {
                        ref Vector2 dotk = ref dots[k], dotl = ref dots[l];
                        if (Custom.DistLess(dotk, dotl, 1.4f))
                        {
                            var vector = Custom.DirVec(dotk, dotl) * (Vector2.Distance(dotk, dotl) - 1.4f);
                            var num = k / ((float)k + l);
                            dotk += vector * num;
                            dotl -= vector * (1f - num);
                        }
                    }
                }
            }
            float num2 = 1f, num3 = -1f, num4 = 1f, num5 = -1f, num6 = 0f;
            // don't change Mathf here !!!
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
                var dot = dots[n];
                dot.x = -1f + 2f * Mathf.InverseLerp(num2, num3, dot.x);
                dot.y = -1f + 2f * Mathf.InverseLerp(num4, num5, dot.y);
                num6 = Mathf.Max(num6, dot.magnitude);
            }
            for (var num8 = 0; num8 < dots.Length; num8++)
                dots[num8] /= num6;
        }

        public virtual void InitiateSprites(int startIndex, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var cont = rCam.ReturnFContainer("Midground");
            var lth = Dots.Length;
            for (var i = 0; i < lth; i++)
            {
                var spr1 = sLeaser.sprites[startIndex + i] = new("JetFishEyeB");
                var spr2 = sLeaser.sprites[startIndex + lth + i] = new("pixel");
                spr1.RemoveFromContainer();
                spr2.RemoveFromContainer();
                cont.AddChild(spr1);
                cont.AddChild(spr2);
            }
        }

        public virtual void DrawSprites(int startIndex, RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
        {
            var vector = Vector2.Lerp(LastPos, Pos, timeStacker);
            var num = 1f;
            var dots = Dots;
            for (var i = 0; i < dots.Length; i++)
            {
                FSprite spr1 = sLeaser.sprites[startIndex + i], spr2 = sLeaser.sprites[startIndex + dots.Length + i];
                Vector2 dot = dots[i], vector2 = vector + new Vector2(dot.x * 7f, dot.y * 8.5f) * num;
                spr1.x = spr2.x = vector2.x - camPos.x;
                spr1.y = spr2.y = vector2.y - camPos.y;
                spr1.scaleX = num;
                spr1.scaleY = Custom.LerpMap(dot.magnitude, 0f, 1f, 1f, .25f, 4f);
                spr1.isVisible = spr2.isVisible = Graphics?.bug is BigSpider b && !b.dead;
            }
        }

        public virtual void ApplyPalette(int startIndex, RoomCamera.SpriteLeaser sLeaser, RoomPalette palette)
        {
            SporeColor = Color.Lerp(Color.Lerp(Color.Lerp(new(.9f, 1f, .8f), palette.blackColor, palette.darkness * .5f), palette.texture.GetPixel(11, 4), .5f), new(.02f, .1f, .08f), .85f);
            var color = Color.Lerp(Color.Lerp(Color.Lerp(new(.8f, 1f, .5f), palette.texture.GetPixel(11, 4), .2f), palette.blackColor, .5f), palette.blackColor, palette.darkness * .5f);
            var lth = Dots.Length;
            var sprites = sLeaser.sprites;
            for (var j = 0; j < lth; j++)
            {
                sprites[startIndex + j].color = color;
                sprites[startIndex + lth + j].color = SporeColor;
            }
        }
    }
}