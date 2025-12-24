using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class ScutigeraGraphics : CentipedeGraphics
{
    public ScutigeraGraphics(Scutigera ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        hue = Mathf.Lerp(.1527777777777778f, .1861111111111111f, Random.value);
        saturation = Mathf.Lerp(.294f, .339f, Random.value);
        wingPairs = ow.bodyChunks.Length;
        var tot = totSegs;
        var wl = wingLengths = new float[tot];
        for (var j = 0; j < wl.Length; j++)
        {
            var num = j / (tot - 1f);
            var num2 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(.5f, 0f, num), .75f) * Mathf.PI);
            num2 *= 1f - num;
            var num3 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(1f, .5f, num), .75f) * Mathf.PI);
            num3 *= num;
            num2 = .5f + .5f * num2;
            num3 = .5f + .5f * num3;
            wl[j] = Mathf.Lerp(3f, Custom.LerpMap(centipede.size, .5f, 1f, 60f, 80f), Mathf.Max(num2, num3) - Mathf.Sin(num * Mathf.PI) * .25f);
        }
        Random.state = state;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var ar = new Color[centipede.bodyChunks.Length];
        for (var i = 0; i < ar.Length; i++)
            ar[i] = sLeaser.sprites[ShellSprite(i, 0)].color;
        base.ApplyPalette(sLeaser, rCam, palette);
        for (var i = 0; i < ar.Length; i++)
            sLeaser.sprites[ShellSprite(i, 0)].color = ar[i];
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (centipede is Scutigera c)
        {
            var chs = c.bodyChunks;
            var sprs = sLeaser.sprites;
            for (var i = 0; i < chs.Length; i++)
            {
                var spr = sprs[ShellSprite(i, 0)];
                if (spr.element.name is "CentipedeBackShell")
                    spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
                else if (spr.element.name is "CentipedeBellyShell")
                    spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBellyShell");
            }
            for (var k = 0; k < 2; k++)
            {
                for (var num15 = 0; num15 < wingPairs; num15++)
                {
                    if (sprs[WingSprite(k, num15)] is CustomFSprite cSpr)
                    {
                        var vector1 = num15 != 0 ? Custom.DirVec(ChunkDrawPos(num15 - 1, timeStacker), ChunkDrawPos(num15, timeStacker)) : Custom.DirVec(ChunkDrawPos(0, timeStacker), ChunkDrawPos(1, timeStacker));
                        var vector2 = Custom.PerpendicularVector(vector1);
                        var vector3 = RotatAtChunk(num15, timeStacker);
                        var vector4 = WingPos(k, num15, vector1, vector2, vector3, timeStacker);
                        var vector5 = ChunkDrawPos(num15, timeStacker) + vector2 * (chs[num15].rad * (k != 0 ? 1f : -1f) * vector3.y);
                        cSpr.MoveVertice(1, vector4 + vector1 * 2f - camPos);
                        cSpr.MoveVertice(0, vector4 - vector1 * 2f - camPos);
                        cSpr.MoveVertice(2, vector5 + vector1 * 2f - camPos);
                        cSpr.MoveVertice(3, vector5 - vector1 * 2f - camPos);
                        cSpr.verticeColors[1] = cSpr.verticeColors[0] = SecondaryShellColor;
                        cSpr.verticeColors[3] = cSpr.verticeColors[2] = blackColor;
                    }
                }
            }
        }
    }
}