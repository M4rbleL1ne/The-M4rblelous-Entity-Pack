using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class M4RMamaBugGraphics : EggBugGraphics, IMuddableGraphics
{
    public const int MESH_SPRITE = 61, HEAD_SPRITE = 62, TOTAL_SPRITES = 134;
    public Color EyeCol;

    public M4RMamaBugGraphics(M4RMamaBug ow) : base(ow)
    {
        legLength *= 1.15f;
        tailEnd.rad = 16f;
        int i, j;
        var ants = antennas = new GenericBodyPart[2, 3];
        var l0 = ants.GetLength(0);
        var l1 = ants.GetLength(1);
        var con = ow.firstChunk;
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                ants[i, j] = new(this, 1.2f, .5f, .9f, con);
        }
        var lgs = legs;
        l0 = lgs.GetLength(0);
        l1 = lgs.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                lgs[i, j].rad = .25f;
        }
        var es = eggs = new GenericBodyPart[4, 5];
        l0 = es.GetLength(0);
        l1 = es.GetLength(1);
        con = ow.bodyChunks[1];
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                es[i, j] = new(this, 4f, .5f, .99f, con);
        }
        var bps = bodyParts = new BodyPart[31];
        bps[0] = tailEnd;
        var num = 1;
        l0 = lgs.GetLength(0);
        l1 = lgs.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
            {
                bps[num] = lgs[i, j];
                ++num;
            }
        }
        l0 = ants.GetLength(0);
        l1 = ants.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
            {
                bps[num] = ants[i, j];
                ++num;
            }
        }
        l0 = es.GetLength(0);
        l1 = es.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
            {
                bps[num] = es[i, j];
                ++num;
            }
        }
    }

    public static new int BackEggSprite(int side, int egg, int part) => 1 + side * 15 + (4 - egg) * 3 + part;

    public static new int AntennaSprite(int side) => 63 + side;

    public static new int EyeSprite(int eye) => eye * 65;

    public static new int LegSprite(int leg, int side, int part) => 66 + side * 4 + leg * 2 + part;

    public static new int FrontEggSprite(int side, int egg, int part) => 74 + side * 15 + (4 - egg) * 3 + part;

    public static float EggAngle(int side) => -45f + 30f * side;

    public static float EggSpacing(int egg) => egg switch
    {
        4 => 7f,
        3 => 12f,
        2 => 17f,
        1 => 20f,
        _ => 22f
    };

    public new Vector2 EggAttachPos(int side, int egg, float timeStacker)
    {
        var t = Mathf.InverseLerp(0f, 4f, egg);
        var b1 = bug.bodyChunks[1];
        var b0 = bug.firstChunk;
        var b1pos = Vector2.Lerp(b1.lastPos, b1.pos, timeStacker);
        var bodyRelCenter = Vector2.Lerp(b1pos, Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker), t);
        var bodyDir = Custom.DirVec(bodyRelCenter, Vector2.Lerp(b0.lastPos, b0.pos, timeStacker));
        var eggAngleX = Custom.DegToVec(Custom.VecToDeg(Vector3.Slerp(lastZRotat, zRotat, timeStacker)) + EggAngle(side)).x;
        if (ShowEggs)
            eggAngleX *= Mathf.Lerp(1.5f, 1f, Math.Abs(Mathf.Lerp(lastFlip, flip, timeStacker)));
        return bodyRelCenter + bodyDir * Mathf.Lerp(8f, -2f, t) + Custom.PerpendicularVector(bodyDir) * (eggAngleX * EggSpacing(egg) * (.45f + .55f * t - (side is 0 or 3 ? .05f : .15f)));
    }

    public override void Update()
    {
        base.Update();
        if (!culled && ShowEggs)
        {
            var b0 = bug.firstChunk;
            var b1 = bug.bodyChunks[1];
            var es = eggs;
            var ps = Vector2.Lerp(b0.pos, b1.pos, .5f);
            var flpPerp = Custom.PerpendicularVector(b1.pos, b0.pos) * (flip * .4f);
            var dripCol = Color.Lerp(eggColors[1], blackColor, .4f);
            for (var i = 2; i < 4; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var egg = es[i, j];
                    egg.Update();
                    var attachPos = EggAttachPos(i, j, 1f);
                    egg.ConnectToPoint(attachPos, 5f, true, 0f, b1.vel, .1f, 0f);
                    egg.vel.y -= .9f;
                    egg.vel += Custom.DirVec(ps, attachPos) * .7f + flpPerp;
                    if (Random.value < .05f && (bug.shake > 0 || bug.noJumps > 60) && Random.value < bug.AI.fear && bug.room.ViewedByAnyCamera(egg.pos, 50f))
                        bug.room.AddObject(new EggBugEgg.LiquidDrip(egg.pos, Custom.DirVec(attachPos, egg.pos) * Mathf.Lerp(3f, 7f, Random.value) + Custom.RNV() * (Random.value * 4f), dripCol));
                }
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprites[MESH_SPRITE] = TriangleMesh.MakeLongMesh(14, false, false);
        sprites[HEAD_SPRITE] = new("Circle20");
        var antLgt = antennas.GetLength(1);
        int i;
        for (i = 0; i < 2; i++)
        {
            sprites[EyeSprite(i)] = new("JetFishEyeB") { scale = .75f };
            for (var j = 0; j < 2; j++)
            {
                sprites[LegSprite(i, j, 0)] = new("CentipedeLegA");
                sprites[LegSprite(i, j, 1)] = new("CentipedeLegB");
            }
            sprites[AntennaSprite(i)] = TriangleMesh.MakeLongMesh(antLgt, true, true);
        }
        for (i = 0; i < 4; i++)
        {
            for (var l = 0; l < 5; l++)
            {
                sprites[BackEggSprite(i, l, 0)] = new("DangleFruit0A") { scaleX = .7f, scaleY = .75f, anchorY = .3f };
                sprites[FrontEggSprite(i, l, 0)] = new("EggBugEggColor") { scaleX = .7f, scaleY = .75f, anchorY = .3f };
                sprites[BackEggSprite(i, l, 1)] = new("DangleFruit0B") { scaleX = .7f, scaleY = .75f, anchorY = .3f };
                sprites[FrontEggSprite(i, l, 1)] = new("EggBugEggColor") { scaleX = .7f, scaleY = .75f, anchorY = .3f };
                sprites[BackEggSprite(i, l, 2)] = new("JetFishEyeA") { scale = .45f, anchorY = .7f };
                sprites[FrontEggSprite(i, l, 2)] = new("JetFishEyeA") { scale = .45f, anchorY = .7f };
            }
        }
        AddToContainer(sLeaser, rCam, null);
        if (DEBUGLABELS is DebugLabel[] labels && labels.Length != 0)
        {
            var cont = rCam.ReturnFContainer("HUD");
            for (i = 0; i < labels.Length; i++)
                cont.AddChild(labels[i].label);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        int i, j;
        var b0 = bug.mainBodyChunk;
        if (DEBUGLABELS is DebugLabel[] labels && labels.Length != 0)
        {
            for (i = 0; i < labels.Length; i++)
            {
                var debugLabel = labels[i];
                if (debugLabel.relativePos)
                    debugLabel.label.SetPosition(b0.pos + debugLabel.pos - camPos);
                else
                    debugLabel.label.SetPosition(debugLabel.pos);
            }
        }
        if (bug.muddy > 0 && mudOverlay is null && CanApplyMudOverlay)
            mudOverlay = new(bug, sLeaser);
        UpdateRippleHybrid(sLeaser, owner.room);
        if (owner.slatedForDeletetion || owner.room != rCam.room || dispose)
        {
            sLeaser.CleanSpritesAndRemove();
            RemoveRippleHybrid();
        }
        var sprites = sLeaser.sprites;
        if (sprites[0].isVisible == culled)
        {
            for (i = 0; i < sprites.Length; i++)
                sprites[i].isVisible = !culled;
        }
        if (culled)
            return;
        ++drawTicker;
        var flp = Mathf.Lerp(lastFlip, flip, timeStacker);
        var zrot = Vector3.Slerp(lastZRotat, zRotat, timeStacker);
        lastDarkness = darkness;
        var pos = Vector2.Lerp(b0.lastPos, b0.pos, timeStacker);
        darkness = rCam.room.Darkness(pos);
        if (darkness > .5f)
            darkness = Mathf.Lerp(darkness, .5f, rCam.room.LightSourceExposure(pos));
        if (lastDarkness != darkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var b1 = bug.bodyChunks[1];
        var headPos = Vector2.Lerp(b1.lastPos, b1.pos, timeStacker);
        if (bug.shake > 0)
        {
            pos += Custom.RNV() * (Random.value * 3f);
            headPos += Custom.RNV() * (Random.value * 3f);
        }
        var tailDir = Custom.DirVec(headPos, pos);
        var perp = Custom.PerpendicularVector(tailDir);
        var tailPos = Vector2.Lerp(tailEnd.lastPos, tailEnd.pos, timeStacker);
        tailPos += Custom.DirVec(headPos, tailPos) * 5f;
        var head = sprites[HEAD_SPRITE];
        head.SetPosition(pos - camPos);
        head.scaleX = .5f;
        head.scaleY = .6f;
        head.rotation = Custom.VecToDeg(tailDir);
        for (i = 0; i < 2; i++)
        {
            var eye = sprites[EyeSprite(i)];
            eye.SetPosition(pos + tailDir * 4f + perp * ((i == 0 == flp < 0f ? -3f : 3f) * (1f - Math.Abs(flp))) - camPos);
            eye.color = !bug.Consious ? blackColor : EyeCol;
        }
        var tailVec = pos + tailDir;
        var lastDynRad = 0f;
        var bodyRad = Mathf.Lerp(20f, 7f, Math.Abs(flp));
        var mesh = (sprites[MESH_SPRITE] as TriangleMesh)!;
        for (i = 0; i < 14; i++)
        {
            var f = Mathf.InverseLerp(0f, 6f, i);
            var bodyCurve = Custom.Bezier(pos + tailDir * 3f, headPos, tailPos, headPos, f);
            var dynRad = Mathf.Lerp(1.5f, bodyRad, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(f, .75f) * Mathf.PI)), .3f));
            var curvePerp = Custom.PerpendicularVector(bodyCurve, tailVec);
            var chgPerp1 = curvePerp * ((dynRad + lastDynRad) * .5f);
            var i4 = i * 4;
            mesh.MoveVertice(i4, (tailVec + bodyCurve) * .5f - chgPerp1 - camPos);
            mesh.MoveVertice(i4 + 1, (tailVec + bodyCurve) * .5f + chgPerp1 - camPos);
            var chgPerp2 = curvePerp * dynRad;
            mesh.MoveVertice(i4 + 2, bodyCurve - chgPerp2 - camPos);
            mesh.MoveVertice(i4 + 3, bodyCurve + chgPerp2 - camPos);
            tailVec = bodyCurve;
            lastDynRad = dynRad;
        }
        var lgs = legs;
        for (i = 0; i < 2; i++)
        {
            for (j = 0; j < 2; j++)
            {
                var leg = lgs[i, j];
                var legPosA = Vector2.Lerp(pos, headPos, .3f) + perp * ((i == 0 ? -3f : 3f) * (1f - Math.Abs(flp))) + tailDir * (j == 0 ? -4f : 4f);
                var legPartPos = Vector2.Lerp(leg.lastPos, leg.pos, timeStacker);
                if (Custom.DistLess(legPosA, legPartPos, 6f))
                    legPartPos = legPosA + Custom.DirVec(legPosA, legPartPos) * 6f;
                var f2 = Mathf.Lerp(i == 0 ? -1f : 1f, flp * Mathf.Clamp(Custom.DistanceToLine(legPartPos, headPos - tailDir * 20f, headPos - tailDir * 20f + perp) * -.05f, -1f, 1f), Math.Abs(flp));
                var legPosB = Custom.InverseKinematic(legPosA, legPartPos, legLength / 3f, legLength * (2f / 3f), f2);
                var legA = sprites[LegSprite(j, i, 0)];
                legA.SetPosition(legPosA - camPos);
                legA.rotation = Custom.AimFromOneVectorToAnother(legPosA, legPosB);
                legA.scaleY = Vector2.Distance(legPosA, legPosB) / 27f;
                legA.anchorY = .1f;
                legA.scaleX = -Mathf.Sign(flip) * .8f;
                var legB = sprites[LegSprite(j, i, 1)];
                legB.anchorY = .1f;
                legB.scaleX = -Mathf.Sign(f2);
                legB.SetPosition(legPosB - camPos);
                legB.rotation = Custom.AimFromOneVectorToAnother(legPosB, legPartPos);
                legB.scaleY = (Vector2.Distance(legPosB, legPartPos) + 1f) * .04f;
            }
        }
        var ants = antennas;
        var antLgt = ants.GetLength(1);
        for (i = 0; i < 2; i++)
        {
            var antMesh = (sprites[AntennaSprite(i)] as TriangleMesh)!;
            tailVec = pos;
            for (j = 0; j < antLgt; j++)
            {
                var ant = ants[i, j];
                var antPartPos = Vector2.Lerp(ant.lastPos, ant.pos, timeStacker);
                if (j > 0)
                {
                    var pastAnt = ants[i, j - 1];
                    var pastPos = Vector2.Lerp(pastAnt.lastPos, pastAnt.pos, timeStacker);
                    var ang = Custom.AimFromOneVectorToAnother(pastPos, antPartPos) + (i == 1 ? 45f : -45f) * (1f + (float)j / antLgt);
                    antPartPos = pastPos + Custom.DegToVec(ang) * Custom.Dist(pastPos, antPartPos);
                }
                else
                    antPartPos += Custom.DegToVec(i == 1 ? -45f : 45f);
                var antTailDir = (antPartPos - tailVec).normalized;
                var prp = Custom.PerpendicularVector(antTailDir);
                var n4 = j * 4;
                if (j == 0)
                {
                    antMesh.MoveVertice(n4, tailVec - prp - camPos);
                    antMesh.MoveVertice(n4 + 1, tailVec + prp - camPos);
                    antMesh.MoveVertice(n4 + 2, (antPartPos + tailVec) * .5f - prp - camPos);
                    antMesh.MoveVertice(n4 + 3, (antPartPos + tailVec) * .5f + prp - camPos);
                }
                else
                {
                    var antTailDistFac = Vector2.Distance(antPartPos, tailVec) * .2f;
                    antMesh.MoveVertice(n4, tailVec - prp + antTailDir * antTailDistFac - camPos);
                    antMesh.MoveVertice(n4 + 1, tailVec + prp + antTailDir * antTailDistFac - camPos);
                    if (j < antLgt - 1)
                    {
                        antMesh.MoveVertice(n4 + 2, antPartPos - prp - antTailDir * antTailDistFac - camPos);
                        antMesh.MoveVertice(n4 + 3, antPartPos + prp - antTailDir * antTailDistFac - camPos);
                    }
                    else
                        antMesh.MoveVertice(n4 + 2, antPartPos - camPos);
                }
                tailVec = antPartPos;
            }
        }
        var egs = eggs;
        int k;
        if (ShowEggs)
        {
            for (i = 0; i < 4; i++)
            {
                var isVisible = Custom.DegToVec(Custom.VecToDeg(zrot) + EggAngle(i)).y > 0f;
                for (j = 0; j < 5; j++)
                {
                    var attachPos = EggAttachPos(i, j, timeStacker);
                    var egg = egs[i, j];
                    var eggPartPos = Vector2.Lerp(egg.lastPos, egg.pos, timeStacker);
                    var rt = Custom.AimFromOneVectorToAnother(eggPartPos, attachPos);
                    for (k = 0; k < 3; k++)
                    {
                        var back = sprites[BackEggSprite(i, j, k)];
                        var front = sprites[FrontEggSprite(i, j, k)];
                        back.SetPosition(eggPartPos - camPos);
                        front.SetPosition(eggPartPos - camPos);
                        back.rotation = front.rotation = rt;
                        front.isVisible = isVisible;
                    }
                }
            }
        }
        else
        {
            var newCol = Color.Lerp(antennaTipColor, eggColors[2], .5f);
            for (i = 0; i < 4; i++)
            {
                var isVisible = Custom.DegToVec(Custom.VecToDeg(zrot) + EggAngle(i)).y > 0f;
                for (j = 0; j < 5; j++)
                {
                    for (k = 0; k < 2; k++)
                    {
                        sprites[BackEggSprite(i, j, k)].isVisible = false;
                        sprites[FrontEggSprite(i, j, k)].isVisible = false;
                    }
                    var attachPos = EggAttachPos(i, j, timeStacker);
                    var dirAngle = Custom.AimFromOneVectorToAnother(attachPos, pos);
                    var back = sprites[BackEggSprite(i, j, 2)];
                    var front = sprites[FrontEggSprite(i, j, 2)];
                    front.SetPosition(attachPos -  camPos);
                    front.scaleY = .35f;
                    front.scaleX = .2f;
                    front.color = newCol;
                    front.rotation = dirAngle;
                    back.SetPosition(attachPos - camPos);
                    back.scaleY = .35f;
                    back.scaleX = .2f;
                    back.color = newCol;
                    back.rotation = dirAngle;
                    front.isVisible = isVisible;
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var color = Custom.HSL2RGB(Custom.Decimal(bug.hue + 1.5f), 1f, .5f);
        var color2 = Custom.HSL2RGB(Custom.Decimal(bug.hue + 1f), 1f, .5f);
        blackColor = Color.Lerp(palette.blackColor, Color.Lerp(color2, palette.fogColor, .3f), .1f * (1f - darkness));
        var sprites = sLeaser.sprites;
        int i, j;
        for (i = 0; i < sprites.Length; i++)
            sprites[i].color = blackColor;
        eggColors = EggColors(palette, bug.hue, darkness);
        for (i = 0; i < 4; i++)
        {
            for (j = 0; j < 5; j++)
            {
                for (var k = 0; k < 3; k++)
                    sprites[FrontEggSprite(i, j, k)].color = sprites[BackEggSprite(i, j, k)].color = eggColors[k];
            }
        }
        EyeCol = Color.Lerp(Color.Lerp(palette.fogColor, color, .5f), blackColor, Mathf.InverseLerp(.75f, 1f, darkness));
        sprites[EyeSprite(0)].color = sprites[EyeSprite(1)].color = !bug.Consious ? blackColor : EyeCol;
        antennaTipColor = Color.Lerp(Color.Lerp(blackColor, color2, .2f * Mathf.Pow(1f - darkness, .2f)), Custom.HSL2RGB(bug.hue, 1f, .5f), .25f);
        for (i = 0; i < 2; i++)
        {
            var verts = (sprites[AntennaSprite(i)] as TriangleMesh)!.verticeColors;
            for (j = 0; j < verts.Length; j++)
                verts[j] = Color.Lerp(blackColor, antennaTipColor, (float)j / (verts.Length - 1));
        }
        if (!ShowEggs)
        {
            var newCol = Color.Lerp(antennaTipColor, eggColors[2], .5f);
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 5; j++)
                {
                    sprites[FrontEggSprite(i, j, 2)].color = newCol;
                    sprites[BackEggSprite(i, j, 2)].color = newCol;
                }
            }
        }
    }


    public new void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) { }
}