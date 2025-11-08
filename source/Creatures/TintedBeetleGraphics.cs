using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class TintedBeetleGraphics : GraphicsModule, IMuddableGraphics
{
    public const float LEG_LENGTH = 30f;
    public const int TOTAL_SPRITES = 15, HEAD_SPRITE = 3, MESH_SPRITE = 1, MESH_2 = 2;
    public TintedBeetle Bug;
    public float LastFlip, Darkness, LastDarkness, Flip;
    public Color BlackColor;
    public Limb[][] Legs;
    public int LegsDangleCounter;
    public GenericBodyPart[][] Antennas;
    public GenericBodyPart TailEnd;
    public bool AltForm;

    public TintedBeetleGraphics(TintedBeetle ow) : base(ow, false)
    {
        Bug = ow;
        AltForm = ow.abstractCreature.superSizeMe;
        TailEnd = new(this, 7f, .51f, .98f, ow.bodyChunks[1]);
        LastDarkness = -1f;
        var fch = ow.mainBodyChunk;
        var ant = Antennas = new GenericBodyPart[2][];
        for (var i = 0; i < ant.Length; i++)
        {
            var anti = ant[i] = new GenericBodyPart[4];
            for (var j = 0; j < anti.Length; j++)
                anti[j] = new(this, 1f, .5f, .9f, fch);
        }
        var legs = Legs = new Limb[2][];
        for (var k = 0; k < legs.Length; k++)
        {
            var legsk = legs[k] = new Limb[2];
            for (var l = 0; l < legsk.Length; l++)
                legsk[l] = new(this, fch, k * 2 + l, .1f, .7f, .99f, 17f, .8f);
        }
        var bp = bodyParts = new BodyPart[13];
        bp[0] = TailEnd;
        var num = 1;
        for (var num2 = 0; num2 < legs.Length; num2++)
        {
            var legsk = legs[num2];
            for (var num3 = 0; num3 < legsk.Length; num3++)
            {
                bp[num] = legsk[num3];
                ++num;
            }
        }
        for (var num4 = 0; num4 < ant.Length; num4++)
        {
            var anti = ant[num4];
            for (var num5 = 0; num5 < anti.Length; num5++)
            {
                bp[num] = anti[num5];
                ++num;
            }
        }
    }

    public virtual int AntennaSprite(int side) => 4 + side;

    public virtual int EyeSprite(int eye) => eye != 0 ? 6 : 0;

    public virtual int LegSprite(int leg, int side, int part) => 7 + side * 4 + leg * 2 + part;

    public override void Update()
    {
        base.Update();
        if (culled)
            return;
        var bs = Bug.bodyChunks;
        var b0 = bs[0];
        if (!Bug.Consious || !Bug.Footing)
            LegsDangleCounter = 30;
        else if (LegsDangleCounter > 0)
        {
            --LegsDangleCounter;
            if (Bug.Footing)
            {
                for (var i = 0; i < bs.Length; i++)
                {
                    if (Bug.room.aimap.TileAccessibleToCreature(bs[i].pos, Bug.Template))
                        LegsDangleCounter = 0;
                }
            }
        }
        LastFlip = Flip;
        TailEnd.Update();
        TailEnd.ConnectToPoint(bs[1].pos + Custom.DirVec(b0.pos, bs[1].pos) * 7f + Custom.PerpendicularVector(b0.pos, bs[1].pos) * Flip * 7f, 6f, false, .2f, bs[1].vel, .5f, .1f);
        TailEnd.vel.y -= .4f;
        TailEnd.vel += Custom.DirVec(b0.pos, bs[1].pos) * .8f;
        float num = Custom.AimFromOneVectorToAnother(bs[1].pos, b0.pos), num2 = 0f, num3 = 0f;
        var legs = Legs;
        for (var j = 0; j < legs.Length; j++)
        {
            var legsj = legs[j];
            for (var k = 0; k < legsj.Length; k++)
            {
                var leg = legsj[k];
                num3 += Custom.LerpMap(Vector2.Dot(Custom.DirVec(b0.pos, leg.absoluteHuntPos), Bug.TravelDir.normalized), -.6f, .6f, 0f, .25f);
                num2 += Custom.DistanceToLine(leg.pos, bs[1].pos, b0.pos);
            }
        }
        num3 *= Mathf.InverseLerp(0f, .1f, Bug.TravelDir.magnitude);
        Flip = Custom.LerpAndTick(Flip, Mathf.Clamp(num2 / 40f, -1f, 1f), .07f, .1f);
        if (LegsDangleCounter > 0)
            num3 = 1f;
        var num4 = 0f;
        if (Bug.room.GetTile(b0.pos + Custom.PerpendicularVector(b0.pos, bs[1].pos) * 20f).Solid)
            num4 += 1f;
        if (Bug.room.GetTile(b0.pos - Custom.PerpendicularVector(b0.pos, bs[1].pos) * 20f).Solid)
            num4 -= 1f;
        if (num4 != 0f)
            Flip = Custom.LerpAndTick(Flip, num4, .07f, .05f);
        var num5 = 0;
        for (var l = 0; l < legs.Length; l++)
        {
            var legsl = legs[l];
            for (var m = 0; m < legsl.Length; m++)
            {
                var leg = legsl[m];
                leg.Update();
                var num6 = .5f + .5f * Mathf.Sin((Bug.RunCycle + num5 * .25f) * Mathf.PI);
                if (leg.mode == Limb.Mode.HuntRelativePosition || LegsDangleCounter > 0)
                    leg.mode = Limb.Mode.Dangle;
                var vector = Custom.DegToVec(num + (m == 1 ? 45f : 135f) * (num4 != 0f ? 0f - num4 : l == 0 ? 1f : -1f));
                if (Bug.Consious)
                {
                    vector += Bug.TravelDir * Custom.LerpMap(num3, 0f, .6f, 3f, 0f) * Mathf.Pow(num6, .5f);
                    vector.Normalize();
                }
                var vector2 = b0.pos + vector * LEG_LENGTH * .2f + Custom.DegToVec(num + (m == 1 ? 45f : 135f) * (l == 0 ? 1f : -1f)) * LEG_LENGTH * .1f;
                leg.ConnectToPoint(vector2, LEG_LENGTH, false, 0f, b0.vel, .1f, 0f);
                leg.ConnectToPoint(b0.pos, LEG_LENGTH, false, 0f, b0.vel, .1f, 0f);
                if (Custom.DistLess(leg.pos, b0.pos, 6f))
                    leg.pos = b0.pos + Custom.DirVec(b0.pos, leg.pos) * 6f;
                if (LegsDangleCounter > 0)
                {
                    var vector3 = vector2 + vector * LEG_LENGTH * .7f;
                    leg.vel = Vector2.Lerp(leg.vel, vector3 - leg.pos, .3f);
                    leg.vel.y -= .4f;
                    if (Bug.Consious)
                        leg.vel += Custom.RNV() * 3f;
                }
                else
                {
                    var vector4 = vector2 + vector * LEG_LENGTH;
                    for (var n = 0; n < legs.Length; n++)
                    {
                        var legsn = legs[n];
                        for (var num7 = 0; num7 < legsn.Length; num7++)
                        {
                            var leg2 = legsn[num7];
                            if (n != l && num7 != m && Custom.DistLess(vector4, leg2.absoluteHuntPos, LEG_LENGTH * .3f))
                                vector4 = leg2.absoluteHuntPos + Custom.DirVec(leg2.absoluteHuntPos, vector4) * LEG_LENGTH * .3f;
                        }
                    }
                    var num8 = 1.5f;
                    if (!leg.reachedSnapPosition)
                    {
                        leg.FindGrip(Bug.room, vector2, vector2, LEG_LENGTH * num8, vector4, -2, -2, true);
                        if (leg.mode != Limb.Mode.HuntAbsolutePosition)
                            leg.FindGrip(Bug.room, vector2, vector2 + vector * LEG_LENGTH * .5f, LEG_LENGTH * num8, vector4, -2, -2, true);
                    }
                    if (!Custom.DistLess(leg.pos, leg.absoluteHuntPos, LEG_LENGTH * num8 * Mathf.Pow(1f - num6, .2f)))
                    {
                        leg.mode = Limb.Mode.Dangle;
                        leg.vel += vector * 7f;
                        leg.vel = Vector2.Lerp(leg.vel, vector4 - leg.pos, .5f);
                    }
                    else
                        leg.vel += vector * 2f;
                }
                ++num5;
            }
        }
        var ant = Antennas;
        for (var num9 = 0; num9 < ant.Length; num9++)
        {
            var anti = ant[num9];
            for (var num10 = 0; num10 < anti.Length; num10++)
            {
                var antenna = anti[num10];
                var num11 = Mathf.InverseLerp(0f, anti.Length - 1, num10);
                antenna.Update();
                antenna.vel *= .9f;
                if (num10 == 0)
                    antenna.ConnectToPoint(b0.pos + AntennaDir(0, 1f, 1f) * 4f, 4f, false, 0f, default, 0f, 0f);
                else if (!Custom.DistLess(antenna.pos, anti[num10 - 1].pos, 6f))
                {
                    var vector5 = -Custom.DirVec(antenna.pos, anti[num10 - 1].pos) * (6f - Vector2.Distance(antenna.pos, anti[num10 - 1].pos)) * .5f;
                    antenna.pos += vector5;
                    antenna.vel += vector5;
                    anti[num10 - 1].pos -= vector5;
                    anti[num10 - 1].vel -= vector5;
                }
                if (num10 > 1)
                {
                    antenna.vel += Custom.DirVec(anti[num10 - 2].pos, antenna.pos) * .8f;
                    anti[num10 - 2].vel -= Custom.DirVec(anti[num10 - 2].pos, antenna.pos) * .8f;
                }
                antenna.vel += (AntennaDir(num9, Mathf.Pow(1f - num11, 0.5f), 1f) + Bug.AwayFromTerrainDir * Mathf.Sin(num11 * Mathf.PI) * 1.7f) * Mathf.Lerp(6f, 2f, num11);
                antenna.vel.y -= .3f;
                if (Bug.Consious)
                {
                    antenna.pos += Custom.RNV() * num11 * Random.value * (1f + Bug.AI.Fear + Bug.AntennaAttention);
                    if (Bug.Sitting)
                        antenna.vel += Custom.DirVec(antenna.pos, Bug.AntennaDir) * 11f * Mathf.Pow(num11, 0.5f) * Random.value * Bug.AntennaAttention;
                }
            }
        }
    }

    public virtual Vector2 AntennaDir(int s, float sideFac, float timeStacker)
    {
        var bs = Bug.bodyChunks;
        var vector = Custom.DirVec(Vector2.Lerp(bs[1].lastPos, bs[1].pos, timeStacker), Vector2.Lerp(bs[0].lastPos, bs[0].pos, timeStacker));
        return (vector + Custom.PerpendicularVector(vector) * Mathf.Lerp(s == 0 ? -.7f : .7f, Mathf.Lerp(LastFlip, Flip, timeStacker) * -1.4f, Math.Abs(Mathf.Lerp(LastFlip, Flip, timeStacker) * .7f)) * sideFac).normalized;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprs = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprs[MESH_SPRITE] = TriangleMesh.MakeLongMesh(11, false, true);
        sprs[MESH_2] = TriangleMesh.MakeLongMesh(11, false, true);
        sprs[HEAD_SPRITE] = new("Circle20") { scaleX = .3f };
        for (var i = 0; i < 2; i++)
            sprs[EyeSprite(i)] = new("TintedBeetleEye") { scale = .2f };
        for (var j = 0; j < 2; j++)
        {
            sprs[AntennaSprite(j)] = TriangleMesh.MakeLongMesh(Antennas[j].Length, true, false);
            for (int k = 0; k < 2; k++)
            {
                sprs[LegSprite(j, k, 0)] = new("TintedBeetleLegA");
                sprs[LegSprite(j, k, 1)] = new("TintedBeetleLegB");
            }
        }
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (culled)
            return;
        var bs = Bug.bodyChunks;
        var b0 = bs[0];
        var num = Mathf.Lerp(LastFlip, Flip, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(Vector2.Lerp(b0.lastPos, b0.pos, timeStacker));
        if (Darkness > .5f)
            Darkness = Mathf.Lerp(Darkness, .5f, rCam.room.LightSourceExposure(Vector2.Lerp(b0.lastPos, b0.pos, timeStacker)));
        if (LastDarkness != Darkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var vector = Vector2.Lerp(b0.lastPos, b0.pos, timeStacker);
        if (Bug.Shake > 0)
            vector += Custom.RNV() * Random.value * 4f;
        var vector2 = Vector2.Lerp(bs[1].lastPos, bs[1].pos, timeStacker);
        if (Bug.Shake > 0)
            vector2 += Custom.RNV() * Random.value * 4f;
        Vector2 vector3 = Custom.DirVec(vector2, vector), vector4 = Custom.PerpendicularVector(vector3), vector5 = Vector2.Lerp(TailEnd.lastPos, TailEnd.pos, timeStacker);
        vector5 += Custom.DirVec(vector2, vector5) * 3f;
        var sprs = sLeaser.sprites;
        var head = sprs[HEAD_SPRITE];
        head.SetPosition(vector - camPos);
        head.rotation = Custom.VecToDeg(vector3);
        var col = Bug.Consious ? Color.Lerp(Color.Lerp(rCam.currentPalette.fogColor, AltForm ? Custom.HSL2RGB(348f / 360f, .77f, .81f) : Custom.HSL2RGB(Bug.Hue, 1f, .5f), .75f), BlackColor, Mathf.InverseLerp(.75f, 1f, Darkness) * .4f) : BlackColor;
        for (var i = 0; i < 2; i++)
        {
            var num2 = (i == 0 == num < 0f ? -1f : 1f) * (1f - Math.Abs(num));
            var vector6 = vector + vector3 * 4f + vector4 * num2 * 3f;
            var eye = sprs[EyeSprite(i)];
            eye.SetPosition(vector6 - camPos);
            eye.color = col;
        }
        var vector7 = vector + vector3;
        float num3 = 0f, b = Mathf.Lerp(7f, 5f, Math.Abs(num)), num32 = num3;
        var mes = (sprs[MESH_SPRITE] as TriangleMesh)!;
        var mesh2 = (sprs[MESH_2] as TriangleMesh)!;
        for (var j = 0; j < 11; j++)
        {
            var f = Mathf.InverseLerp(0f, 6f, j);
            var vector8 = Custom.Bezier(vector + vector3 * 3f, vector2, vector5, vector2, f);
            var num4 = Mathf.Lerp(1.5f, b, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(f, .75f) * Mathf.PI)), .3f));
            var num42 = (j % 2 == 0 ? 2f : -2f) * num4;
            var vector9 = Custom.PerpendicularVector(vector8, vector7);
            mes.MoveVertice(j * 4, (vector7 + vector8) / 2f - vector9 * (num4 + num3) * .5f - camPos);
            mes.MoveVertice(j * 4 + 1, (vector7 + vector8) / 2f + vector9 * (num4 + num3) * .5f - camPos);
            mes.MoveVertice(j * 4 + 2, vector8 - vector9 * num4 - camPos);
            mes.MoveVertice(j * 4 + 3, vector8 + vector9 * num4 - camPos);
            mesh2.MoveVertice(j * 4, (vector7 + vector8) / 2f - vector9 * (num42 + num32) * .5f - camPos);
            mesh2.MoveVertice(j * 4 + 1, (vector7 + vector8) / 2f + vector9 * (num42 + num32) * .5f - camPos);
            mesh2.MoveVertice(j * 4 + 2, vector8 - vector9 * num42 - camPos);
            mesh2.MoveVertice(j * 4 + 3, vector8 + vector9 * num42 - camPos);
            vector7 = vector8;
            num3 = num4;
            num32 = num42;
        }
        for (var k = 0; k < 2; k++)
        {
            for (var l = 0; l < 2; l++)
            {
                var vector10 = Vector2.Lerp(vector, vector2, .3f);
                vector10 += vector4 * (k == 0 ? -1f : 1f) * 3f * (1f - Math.Abs(num));
                vector10 += vector3 * (l == 0 ? -1f : 1f) * 4f;
                var leg = Legs[k][l];
                var vector11 = Vector2.Lerp(leg.lastPos, leg.pos, timeStacker);
                if (Custom.DistLess(vector10, vector11, 6f))
                    vector11 = vector10 + Custom.DirVec(vector10, vector11) * 6f;
                var f2 = Mathf.Lerp(k == 0 ? -1f : 1f, num * Mathf.Clamp(Custom.DistanceToLine(vector11, vector2 - vector3 * 20f, vector2 - vector3 * 20f + vector4) / -20f, -1f, 1f), Math.Abs(num));
                var vector12 = Custom.InverseKinematic(vector10, vector11, LEG_LENGTH / 3f, LEG_LENGTH * (2f / 3f), f2);
                var l0 = sprs[LegSprite(l, k, 0)];
                var l1 = sprs[LegSprite(l, k, 1)];
                l0.x = vector10.x - camPos.x;
                l0.y = vector10.y - camPos.y;
                l0.rotation = Custom.AimFromOneVectorToAnother(vector10, vector12);
                l0.scaleY = Vector2.Distance(vector10, vector12) / 17f;
                l0.anchorY = .1f;
                l1.anchorY = .1f;
                l0.scaleX = -Mathf.Sign(Flip) * .8f;
                l1.scaleX = -Mathf.Sign(f2);
                l1.x = vector12.x - camPos.x;
                l1.y = vector12.y - camPos.y;
                l1.rotation = Custom.AimFromOneVectorToAnother(vector12, vector11);
                l1.scaleY = (Vector2.Distance(vector12, vector11) + 1f) / 14f;
            }
        }
        var ant = Antennas;
        for (var m = 0; m < ant.Length; m++)
        {
            vector7 = vector;
            var num5 = .75f;
            var anti = ant[m];
            for (var n = 0; n < anti.Length; n++)
            {
                var antenna = anti[n];
                Vector2 vector13 = Vector2.Lerp(antenna.lastPos, antenna.pos, timeStacker), normalized = (vector13 - vector7).normalized, vector15 = Custom.PerpendicularVector(normalized);
                var mesh = (sprs[AntennaSprite(m)] as TriangleMesh)!;
                if (n == 0)
                {
                    mesh.MoveVertice(n * 4, vector7 - vector15 * num5 - camPos);
                    mesh.MoveVertice(n * 4 + 1, vector7 + vector15 * num5 - camPos);
                    mesh.MoveVertice(n * 4 + 2, (vector13 + vector7) / 2f - vector15 * num5 - camPos);
                    mesh.MoveVertice(n * 4 + 3, (vector13 + vector7) / 2f + vector15 * num5 - camPos);
                }
                else
                {
                    var num7 = Vector2.Distance(vector13, vector7) / 2f;
                    mesh.MoveVertice(n * 4, vector7 - vector15 * num5 + normalized * num7 - camPos);
                    mesh.MoveVertice(n * 4 + 1, vector7 + vector15 * num5 + normalized * num7 - camPos);
                    if (n < anti.Length - 1)
                    {
                        mesh.MoveVertice(n * 4 + 2, vector13 - vector15 * num5 - normalized * num7 - camPos);
                        mesh.MoveVertice(n * 4 + 3, vector13 + vector15 * num5 - normalized * num7 - camPos);
                    }
                    else
                        mesh.MoveVertice(n * 4 + 2, vector13 - camPos);
                }
                vector7 = vector13;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var color2 = Custom.HSL2RGB(Custom.Decimal(Bug.Hue), 1f, .5f);
        BlackColor = Color.Lerp(palette.blackColor, Color.Lerp(color2, palette.fogColor, .3f), .1f * (1f - Darkness));
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
            sprs[i].color = BlackColor;
        var bodyCol = Color.Lerp(Custom.HSL2RGB(Bug.Hue, 1f, .5f), BlackColor, Darkness * .4f);
        var colors = (sprs[MESH_SPRITE] as TriangleMesh)!.verticeColors;
        var colors2 = (sprs[MESH_2] as TriangleMesh)!.verticeColors;
        for (var num = 0; num < colors.Length; num++)
            colors2[num] = colors[num] = Color.Lerp(BlackColor, bodyCol, (float)num / (colors.Length - 1) - .1f);
        var col = Bug.Consious ? Color.Lerp(Color.Lerp(palette.fogColor, AltForm ? Custom.HSL2RGB(348f / 360f, .77f, .81f) : Custom.HSL2RGB(Bug.Hue, 1f, .5f), .75f), BlackColor, Mathf.InverseLerp(.75f, 1f, Darkness) * .4f) : BlackColor;
        for (var n = 0; n < 2; n++)
            sprs[EyeSprite(n)].color = col;
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite != EyeSprite(0) && sprite != EyeSprite(1);

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
    {
        /*MudUtils.SetUpTube(mudOverlay.GetMudSprite(MESH_SPRITE), 30f, 6f);
        for (var i = 0; i < 2; i++)
            MudUtils.SetUpTube(mudOverlay.GetMudSprite(AntennaSprite(i)), 60f, 2f);*/
    }
}