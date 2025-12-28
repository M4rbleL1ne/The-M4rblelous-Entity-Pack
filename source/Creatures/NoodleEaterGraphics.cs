using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class NoodleEaterGraphics : LizardGraphics
{
    public int PupilSprite = -1;

    public NoodleEaterGraphics(NoodleEater ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        overrideHeadGraphic = -1;
        bodyLength *= .75f;
        iVars.fatness = .65f + Random.value * .1f;
        iVars.tailColor = 4f;
        Random.state = state;
    }

    public override void Update()
    {
        base.Update();
        if (lizard is NoodleEater l)
        {
            if (!l.Consious)
            {
                l.bubble = 0;
                l.bubbleIntensity = 0f;
            }
            else
                l.bubbleIntensity /= 2f;
        }
        if (lightSource is LightSource ls)
            ls.setAlpha = 0f;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        if (debugVisualization)
            return;
        if (tongue is GenericBodyPart[] t)
        {
            var tg = sLeaser.sprites[SpriteTongueStart];
            tg.isVisible = false;
            var cont = tg.container;
            var tlm1 = t.Length - 1;
            var array = new TriangleMesh.Triangle[tlm1 * 4 + 1];
            for (var n = 0; n < tlm1; n++)
            {
                var num = n * 4;
                for (var num2 = 0; num2 < 4; num2++)
                    array[num + num2] = new(num + num2, num + num2 + 1, num + num2 + 2);
            }
            array[tlm1 * 4] = new(tlm1 * 4, tlm1 * 4 + 1, tlm1 * 4 + 2);
            cont.AddChild(sLeaser.sprites[SpriteTongueStart] = new TriangleMesh("Futile_White", array, true));
            sLeaser.sprites[SpriteTongueStart].MoveBehindOtherNode(tg);
            tg.RemoveFromContainer();
        }
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
        sLeaser.sprites[SpriteHeadStart + 4].container.AddChild(sLeaser.sprites[PupilSprite = sLeaser.sprites.Length - 1] = new("pixel")
        {
            anchorY = .7f
        });
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
        if (!debugVisualization && PupilSprite >= 0 && PupilSprite < sLeaser.sprites.Length)
        {
            if (sLeaser.sprites[PupilSprite] is FSprite spr)
            {
                spr.RemoveFromContainer();
                (newContainer ?? rCam.ReturnFContainer("Midground")).AddChild(spr);
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization && lizard is NoodleEater liz)
        {
            var ef = effectColor;
            var sprites = sLeaser.sprites;
            var eye = sprites[SpriteHeadStart + 4];
            var stre = (3 - (int)(Math.Abs(Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker)) * 3.9f)).ToString();
            eye.element = Futile.atlasManager.GetElementWithName("NoodleEaterEyeDead" + stre);
            eye.color = liz.Consious ? ef : palette.blackColor;
            var num8 = SpriteLimbsColorStart - SpriteLimbsStart;
            for (var l = SpriteLimbsStart; l < SpriteLimbsEnd; l++)
                sprites[l + num8].color = palette.blackColor;
            if (liz.tongue is LizardTongue t && t.Out)
            {
                sprites[SpriteTongueStart + 1].color = ef;
                var verts = (sprites[SpriteTongueStart] as TriangleMesh)!.verticeColors;
                for (var num18 = 0; num18 < verts.Length; num18++)
                    verts[num18] = ef;
            }
            if (PupilSprite >= 0 && PupilSprite < sprites.Length)
            {
                var pupil = sprites[PupilSprite];
                pupil.MoveInFrontOfOtherNode(eye);
                pupil.SetPosition(eye.GetPosition());
                pupil.rotation = eye.rotation;
                pupil.scaleX = eye.scaleX;
                pupil.scaleY = eye.scaleY;
                pupil.color = Color.black;
                pupil.alpha = .7490196f; // 1f - 64f / 255f
                pupil.isVisible = liz.Consious;
                pupil.element = Futile.atlasManager.GetElementWithName("WB64NoodleEaterEye" + stre);
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (!debugVisualization && lizard is NoodleEater liz)
        {
            var sprites = sLeaser.sprites;
            var ef = effectColor;
            sprites[SpriteHeadStart + 4].color = liz.Consious ? ef : palette.blackColor;
            var num8 = SpriteLimbsColorStart - SpriteLimbsStart;
            for (var l = SpriteLimbsStart; l < SpriteLimbsEnd; l++)
                sprites[l + num8].color = palette.blackColor;
            if (liz.tongue is LizardTongue t && t.Out)
            {
                sprites[SpriteTongueStart + 1].color = ef;
                var verts = (sprites[SpriteTongueStart] as TriangleMesh)!.verticeColors;
                for (var num18 = 0; num18 < verts.Length; num18++)
                    verts[num18] = ef;
            }
            if (PupilSprite >= 0 && PupilSprite < sprites.Length)
                sprites[PupilSprite].color = Color.black;
        }
    }
}