using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class SurfaceSwimmerGraphics : EggBugGraphics, IMuddableGraphics
{
    public SurfaceSwimmerGraphics(SurfaceSwimmer ow) : base(ow) => legLength *= 2f;

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled)
        {
            var head = sLeaser.sprites[HeadSprite];
            head.scaleX *= 1.15f;
            head.scaleY *= 2f;
            var ants = antennas;
            var lgt = ants.GetLength(1);
            var mbc = bug.mainBodyChunk;
            var vector = Vector2.Lerp(mbc.lastPos, mbc.pos, timeStacker);
            for (var m = 0; m < 2; m++)
            {
                var vector7 = vector;
                var num5 = 1f;
                for (var n = 0; n < lgt; n++)
                {
                    var vector13 = Vector2.Lerp(ants[m, n].lastPos, ants[m, n].pos, timeStacker);
                    if (n > 0)
                    {
                        var vector14 = Vector2.Lerp(ants[m, n - 1].lastPos, ants[m, n - 1].pos, timeStacker);
                        var v2 = Custom.DirVec(vector14, vector13);
                        var num6 = Custom.Dist(vector14, vector13);
                        var ang = Custom.VecToDeg(v2) + (m == 1 ? -90 : 90) * (1 + n / lgt);
                        vector13 = vector14 + Custom.DegToVec(ang) * num6;
                    }
                    else
                        vector13 += Custom.DegToVec(m == 1 ? -90 : 90);
                    var normalized = (vector13 - vector7).normalized;
                    var vector15 = Custom.PerpendicularVector(normalized);
                    var ant = (sLeaser.sprites[AntennaSprite(m)] as TriangleMesh)!;
                    if (n == 0)
                    {
                        ant.MoveVertice(n * 4, vector7 - vector15 * num5 - camPos);
                        ant.MoveVertice(n * 4 + 1, vector7 + vector15 * num5 - camPos);
                        ant.MoveVertice(n * 4 + 2, (vector13 + vector7) * .5f - vector15 * num5 - camPos);
                        ant.MoveVertice(n * 4 + 3, (vector13 + vector7) * .5f + vector15 * num5 - camPos);
                    }
                    else
                    {
                        var num7 = Vector2.Distance(vector13, vector7) * (n == 0 ? 1f : .2f);
                        ant.MoveVertice(n * 4, vector7 - vector15 * num5 + normalized * num7 - camPos);
                        ant.MoveVertice(n * 4 + 1, vector7 + vector15 * num5 + normalized * num7 - camPos);
                        if (n < lgt - 1)
                        {
                            ant.MoveVertice(n * 4 + 2, vector13 - vector15 * num5 - normalized * num7 - camPos);
                            ant.MoveVertice(n * 4 + 3, vector13 + vector15 * num5 - normalized * num7 - camPos);
                        }
                        else
                            ant.MoveVertice(n * 4 + 2, vector13 - camPos);
                    }
                    vector7 = vector13;
                }
                for (var num18 = 0; num18 < 3; num18++)
                {
                    var spr = sLeaser.sprites[FrontEggSprite(m, num18, 2)];
                    spr.scaleY = .95f;
                    spr.scaleX = .275f;
                }
                for (var k = 0; k < 2; k++)
                {
                    var leg = sLeaser.sprites[LegSprite(m, k, 1)];
                    leg.scaleX *= .5f;
                    leg.scaleY *= .7f;
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        var col = Color.Lerp(antennaTipColor, eggColors[2], .5f);
        for (var num17 = 0; num17 < 2; num17++)
        {
            for (var num18 = 0; num18 < 3; num18++)
                sLeaser.sprites[BackEggSprite(num17, num18, 2)].color = sLeaser.sprites[FrontEggSprite(num17, num18, 2)].color = col;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        for (var i = 0; i < 2; i++)
        {
            var eye = sLeaser.sprites[EyeSprite(i)];
            eye.element = Futile.atlasManager.GetElementWithName("deerEyeA");
            eye.scale = .5f;
        }
        for (var j = 0; j < 2; j++)
        {
            for (var k = 0; k < 2; k++)
            {
                var leg = sLeaser.sprites[LegSprite(j, k, 1)];
                leg.element = Futile.atlasManager.GetElementWithName("ScutigeraWing");//same sprite as BHEggBugLeg
                leg.scale = .5f;
            }
        }
    }

    public virtual new void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) { }
}