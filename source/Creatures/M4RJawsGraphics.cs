using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;
using TubeSegment = (UnityEngine.Vector2 Pos, UnityEngine.Vector2 LastPos, UnityEngine.Vector2 Vel);

namespace LBMergedMods.Creatures;

public class M4RJawsGraphics : GraphicsModule, IMuddableGraphics
{
	[StructLayout(LayoutKind.Sequential)]
	public class LegGraphic(M4RJawsGraphics owner, M4RJaws.Leg leg, int firstSprite)
    {
		public const int SPRITES = 3;
		public M4RJawsGraphics Ow = owner;
        public M4RJaws.Leg Leg = leg;
        public int FirstSprite = firstSprite;

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			var sprites = sLeaser.sprites;
			var fs = FirstSprite;
            sprites[fs] = new("M4RDoubleJawLegA")
            {
                anchorX = 73f / 83f,
                anchorY = 6.5f / 17f
            };
            sprites[fs + 1] = new("M4RDoubleJawLegB1")
            {
                anchorY = 60f / 67f,
                anchorX = 12f / 18f
            };
            sprites[fs + 2] = new("M4RDoubleJawLegB2")
            {
                anchorY = 60f / 67f,
                anchorX = 12f / 18f
            };
        }

		public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
            var ow = Ow.Ow;
			var mbc = ow.firstChunk;
			var mbcPos = Vector2.Lerp(mbc.lastPos, mbc.pos, timeStacker);
            ref var foot = ref Leg.Foot;
            ref var hip = ref Leg.Hip;
            ref var knee = ref Leg.Knee;
            var hipPos = Vector2.Lerp(hip.LastPos, hip.Pos, timeStacker);
			if (!Custom.DistLess(hipPos, mbcPos, 20f))
				hipPos = mbcPos + Custom.DirVec(mbcPos, hipPos) * 20f;
			var kneePos = Vector2.Lerp(knee.LastPos, knee.Pos, timeStacker);
            var footPos = Vector2.Lerp(foot.LastPos, foot.Pos, timeStacker);
            var sprites = sLeaser.sprites;
            var fs = FirstSprite;
			var sprite = sprites[fs];
            sprite.SetPosition(hipPos - camPos);
            sprite.rotation = Custom.AimFromOneVectorToAnother(hipPos, kneePos) + 90f;
            var wdth = -Math.Sign(Leg.Flip) * Ow.LegWidth;
            sprite.scaleY = wdth;
			var calfPerp = Custom.PerpendicularVector(Custom.DirVec(kneePos, footPos)) * Leg.Flip;
            sprite = sprites[fs + 1];
            var grad = sprites[fs + 2];
            kneePos += calfPerp * 5f;
            var pos = kneePos - (calfPerp * 7f) - camPos;
            sprite.SetPosition(pos);
            grad.scaleX = sprite.scaleX = wdth;
            grad.scaleY = sprite.scaleY = Custom.Dist(kneePos, footPos) / 50f;
            grad.rotation = sprite.rotation = Custom.AimFromOneVectorToAnother(footPos, kneePos);
            grad.SetPosition(pos);
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, in RoomPalette palette)
        {
            var grad = sLeaser.sprites[FirstSprite + 2];
            grad.color = Ow.EyeCol;
            grad.alpha = .25f - palette.darkness * .1f;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class BeakGraphic(M4RJawsGraphics owner, int index, int sprite)
    {
        public M4RJawsGraphics Ow = owner;
        public int Sprite = sprite, Index = index;

        public virtual int JawSide => Index % 2;

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) => sLeaser.sprites[Sprite] = new("DentureJawPart")
        {
            anchorX = 23f / 66f,
            anchorY = 13f / 201f
        };

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir, Vector2 headPerp, float headAng, float useFlip)
        {
            var ownerCrit = Ow.Ow;
            var jawOpen = Mathf.Lerp(ownerCrit.LastJawOpen, ownerCrit.JawOpen, timeStacker);
            var spr = sLeaser.sprites[Sprite];
            spr.SetPosition(headPos - camPos);
            var fatness = Ow.BeakFatness;
            if (Index < 2)
            {
                jawOpen = Math.Max(jawOpen - .3f, 0f);
                fatness *= .85f;
            }
            jawOpen = Math.Max(jawOpen, .025f);
            spr.scaleX = fatness * Mathf.Lerp(1f, 1.05f, jawOpen);
            spr.scaleY = fatness * Mathf.Lerp(Mathf.Lerp(1f, .6f, jawOpen), 1f, Math.Abs(useFlip));
            spr.rotation = headAng + 60f * jawOpen * (-1f + 2f * JawSide) * Mathf.Pow(Math.Abs(useFlip), 1.5f);
            if (Index is 1 or 3)
                spr.scaleX *= -1f;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class EyeTrail(M4RJawsGraphics owner, int sprite)
    {
        public const int SAVE_POSITIONS = 15;
        public M4RJawsGraphics Ow = owner;
        public List<Vector2> PositionsList = [owner.Ow.VisionPoint];
        public int Sprite = sprite, UpdateTicker;

        public virtual void Reset() => PositionsList = [Ow.Ow.VisionPoint];

        public virtual Vector2 GetSmoothPos(int i, float timeStacker) => Vector2.Lerp(GetPos(i + 1), GetPos(i), timeStacker);

        public virtual Vector2 GetPos(int i) => PositionsList[Custom.IntClamp(PositionsList.Count - i - 1, 0, PositionsList.Count - 1)];

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) => sLeaser.sprites[Sprite] = TriangleMesh.MakeLongMesh(SAVE_POSITIONS - 1, false, true);

        public virtual void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var eyePos = Ow.EyePos(timeStacker);
            var rad = 2f * Ow.EyeSize;
            var mesh = (sLeaser.sprites[Sprite] as TriangleMesh)!;
            if (Ow.Ow is M4RJaws g && (g.dead || g.room?.PointSubmerged(eyePos) is true))
                mesh.isVisible = false;
            else
            {
                mesh.isVisible = true;
                int i;
                for (i = 0; i < SAVE_POSITIONS - 1; i++)
                {
                    Vector2 smoothPos = GetSmoothPos(i, timeStacker),
                        smoothPos2 = GetSmoothPos(i + 1, timeStacker),
                        dir = (eyePos - smoothPos).normalized,
                        perp = Custom.PerpendicularVector(dir);
                    dir *= Vector2.Distance(eyePos, smoothPos2) * .2f;
                    mesh.MoveVertice(i * 4, eyePos - perp * rad - dir - camPos);
                    mesh.MoveVertice(i * 4 + 1, eyePos + perp * rad - dir - camPos);
                    mesh.MoveVertice(i * 4 + 2, smoothPos - perp * rad + dir - camPos);
                    mesh.MoveVertice(i * 4 + 3, smoothPos + perp * rad + dir - camPos);
                    eyePos = smoothPos;
                }
                var vertCols = mesh.verticeColors;
                for (i = 0; i < vertCols.Length; i++)
                    vertCols[i] = Ow.EyeCol with { a = Math.Min(1f, Mathf.Pow(1f - i / (float)(vertCols.Length - 1), 1.5f)) };
            }
        }

        public virtual void UpdatePosition(Vector2 pos)
        {
            UpdateTicker += 1;
            if (UpdateTicker % 2 == 0)
            {
                PositionsList.Add(pos);
                if (PositionsList.Count > SAVE_POSITIONS)
                    PositionsList.RemoveAt(0);
            }
        }

        public virtual void Update()
        {
            PositionsList.Add(Ow.EyePos(1f));
            if (PositionsList.Count > SAVE_POSITIONS)
                PositionsList.RemoveAt(0);
        }
    }

    public const int NECK = 0, BODY = 1, FIRST_LEG_PART = 2, LAST_LEG_PART = 1 + 4 * LegGraphic.SPRITES,
		TUBE = LAST_LEG_PART + 1, FIRST_BEAK_PART = TUBE + 1, LAST_BEAK_PART = FIRST_BEAK_PART + 3, HEAD = LAST_BEAK_PART + 1,
		EYE = HEAD + 1, TRAIL = EYE + 1, TOTAL_SPRITES = TRAIL + 1;
    public M4RJaws Ow;
	public LegGraphic[] Legs;
	public BeakGraphic[] Beak;
	public EyeTrail Trail;
    public LightSource?[] LightSources;
    public TubeSegment[] Tube;
    public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData;
    public Color EyeCol;
	public float NeckFatness, BeakFatness, EyeSize, LegWidth, HeadFlip, LastHeadFlip;
    public bool MoveJawsAbove;

	public M4RJawsGraphics(M4RJaws ow) : base(ow, false)
	{
		Ow = ow;
		cullRange = 1400f;
		var state = Random.state;
		Random.InitState(Ow.abstractPhysicalObject.ID.RandomSeed);
		NeckFatness = Mathf.Lerp(2f, 2.15f, Random.value);
		BeakFatness = 1f / 3f + Random.Range(-.01f, .01f);
		EyeSize = Random.Range(.9f, 1f);
        LegWidth = Mathf.Lerp(.9f, 1.1f, Random.value);
        var owLegs = Ow.Legs;
        var legs = Legs = new LegGraphic[owLegs.Length];
		var fs = FIRST_LEG_PART;
        for (var i = 0; i < legs.Length; i++)
		{
			legs[i] = new(this, owLegs[i], fs);
			fs += LegGraphic.SPRITES;
		}
		var beak = Beak = new BeakGraphic[4];
		fs = FIRST_BEAK_PART;
		for (var j = 0; j < beak.Length; j++)
		{
			beak[j] = new(this, j, fs);
			++fs;
		}
		EyeCol = Custom.HSL2RGB(Mathf.Lerp(-.02f, .02f, Random.value), 1f, .5f);
        Random.state = state;
		LightSources = new LightSource[legs.Length];
		Trail = new(this, TRAIL);
        Tube = new TubeSegment[15];
        ScratchTerrainCollisionData.goThroughFloors = true;
        ScratchTerrainCollisionData.rad = 1f;
    }

    public virtual Vector2 EyePos(float timeStacker)
    {
        var flip = Mathf.Lerp(LastHeadFlip, HeadFlip, timeStacker);
		var head = Ow.Head;
		var neckTip = Ow.Neck.Tip;
        var headPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
        return headPos + Custom.PerpendicularVector(Custom.DirVec(Vector2.Lerp(neckTip.lastPos, neckTip.pos, timeStacker), headPos)) * (Math.Sign(flip) * 10f * (1f - Math.Abs(flip)));
    }

    public override void Reset()
	{
		base.Reset();
		Trail.Reset();
        var tube = Tube;
        var mbcPos = Ow.firstChunk.pos;
        for (var i = 0; i < tube.Length; i++)
        {
            ref var t = ref tube[i];
            t.LastPos = t.Pos = mbcPos + Custom.RNV() * Random.value;
            t.Vel = default;
        }
    }

	public override void Update()
	{
		LastHeadFlip = HeadFlip;
		var chs = Ow.bodyChunks;
		var b0 = chs[0];
        var b1 = chs[1];
        var head = chs[4];
		if (Custom.DistanceToLine(head.pos, b1.pos, b0.pos) < 0f)
			HeadFlip = Math.Min(1f, HeadFlip + 1f / 6f);
		else
			HeadFlip = Math.Max(-1f, HeadFlip - 1f / 6f);
		base.Update();
		Trail.Update();
		var lhs = LightSources;
		var rm = Ow.room;
		for (var l = 0; l < lhs.Length; l++)
		{
			if (lhs[l] is LightSource lh)
			{
                lh.stayAlive = true;
				var leg = Ow.Legs[l];
                if (leg.LightUp > 0)
                {
                    lh.setAlpha = Mathf.Pow(Random.value, .5f) * .7f;
                    lh.setRad = Mathf.Lerp(30f, 50f, Random.value);
                    lh.setPos = Vector2.Lerp(leg.LightUpPos1, leg.LightUpPos2, Random.value);
                }
                else
                    lh.setAlpha = 0f;
                if (lh.slatedForDeletetion || rm.Darkness(b0.pos) == 0f)
					lhs[l] = null;
			}
			else if (rm.Darkness(b0.pos) > 0f)
                rm.AddObject(LightSources[l] = new(Ow.Legs[l].Foot.Pos, false, new(1f, 1f, .8f), Ow)
                {
                    requireUpKeep = true
                });
        }
        var posVarMean = default(Vector2);
        for (var i = 0; i < chs.Length; i++)
        {
            var ch = chs[i];
            posVarMean += ch.pos - ch.lastPos;
        }
        posVarMean /= chs.Length;
        var neckTip = Ow.Neck.Tip;
        var neckDir = Custom.DirVec(neckTip.pos, head.pos);
        var rt = Custom.AimFromOneVectorToAnother(neckTip.pos, head.pos) - Custom.AimFromOneVectorToAnother(b0.pos, b1.pos);
        if (rt > 180f)
            rt -= 360f;
        else if (rt < -180f)
            rt += 360f;
        var neckRt = Custom.PerpendicularVector(neckDir) * (Mathf.Pow(Mathf.Abs(Mathf.Cos(Mathf.PI * rt / 180f)), .5f) * (Math.Abs(rt) > 90f ? -1f : 1f));
        var bodyDir = Custom.DirVec(b1.pos, b0.pos);
        ConnectNeckTube(bodyDir, b0.pos, head.pos, neckDir, neckRt);
        var tube = Tube;
        for (var i = 0; i < tube.Length; i++)
        {
            var value = Mathf.InverseLerp(0f, tube.Length - 1, i);
            ref var t = ref tube[i];
            t.Vel += (bodyDir * Mathf.InverseLerp(.6f, 1f, value) - neckDir * Mathf.InverseLerp(.25f, 0f, value)) * 2f;
            t.LastPos = t.Pos;
            t.Pos += t.Vel;
            t.Vel = Vector2.Lerp(t.Vel, posVarMean, .05f);
            t.Vel.y -= .9f;
            if (i > 1)
            {
                ref var tm2 = ref tube[i - 2];
                var prevChInfl = Custom.DirVec(t.Pos, tm2.Pos);
                t.Vel -= prevChInfl;
                tm2.Vel += prevChInfl;
            }
            if (i > 2 && i < tube.Length - 3)
            {
                ref var coll = ref ScratchTerrainCollisionData;
                coll.pos = t.Pos;
                coll.lastPos = t.LastPos;
                coll.vel = t.Vel;
                coll = SharedPhysics.VerticalCollision(rm, coll);
                coll = SharedPhysics.HorizontalCollision(rm, coll);
                t.Pos = coll.pos;
                t.Vel = coll.vel;
            }
        }
        ConnectNeckTube(bodyDir, b0.pos, head.pos, neckDir, neckRt);
    }

    public virtual void ConnectNeckTube(Vector2 bodyDir, Vector2 bodyPos, Vector2 headPos, Vector2 neckDir, Vector2 neckRt)
    {
        var tube = Tube;
        var connPos = bodyPos + bodyDir * 11f;
        ref var t = ref tube[0];
        var origPos = headPos + neckDir * 5f - neckRt * 10f;
        t.Pos = origPos;
        t.Vel = default;
        t = ref tube[tube.Length - 1];
        t.Pos = connPos;
        t.Vel = default;
        for (var i = 0; i < tube.Length - 1; i++)
        {
            t = ref tube[i];
            ref var tp1 = ref tube[i + 1];
            var pastChAff = Custom.DirVec(t.Pos, tp1.Pos) * ((Vector2.Distance(t.Pos, tp1.Pos) - 5f) * .5f);
            t.Pos += pastChAff;
            t.Vel += pastChAff;
            tp1.Pos -= pastChAff;
            tp1.Vel -= pastChAff;
        }
        t = ref tube[0];
        t.Pos = origPos;
        t.Vel = default;
        t = ref tube[tube.Length - 1];
        t.Pos = connPos;
        t.Vel = default;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		var sprites = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprites[HEAD] = new("Circle20")
        {
            scaleX = 1.15f,
            scaleY = 1.5f
        };
        sprites[EYE] = new("M4RDoubleJawEye")
        {
            scale = .5f * EyeSize
        };
        sprites[BODY] = new("M4RDoubleJawBody")
        {
            anchorY = .4f
        };
        sprites[TUBE] = TriangleMesh.MakeLongMesh(Tube.Length, false, false);
        var legs = Legs;
		var beak = Beak;
        for (var i = 0; i < legs.Length; i++)
			legs[i].InitiateSprites(sLeaser, rCam);
		for (var j = 0; j < beak.Length; j++)
			beak[j].InitiateSprites(sLeaser, rCam);
		Trail.InitiateSprites(sLeaser, rCam);
		(sprites[NECK] = TriangleMesh.MakeLongMesh(Ow.Neck.tChunks.Length, false, false)).element = Futile.atlasManager.GetElementWithName("M4RDoubleJawNeck");
        AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		newContainer ??= rCam.ReturnFContainer("Midground");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
		{
			if (i != TRAIL)
				newContainer.AddChild(sprites[i]);
		}
		rCam.ReturnFContainer("Water").AddChild(sprites[TRAIL]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
			return;
		var flip = Mathf.Lerp(LastHeadFlip, HeadFlip, timeStacker);
		var head = Ow.Head;
		var neckTip = Ow.Neck.Tip;
		Vector2 headPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker),
			neckDir = Custom.DirVec(Vector2.Lerp(neckTip.lastPos, neckTip.pos, timeStacker), headPos),
			neckPerp = Custom.PerpendicularVector(neckDir);
		Trail.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		var neckRot = Custom.VecToDeg(neckDir);
		var beak = Beak;
		for (var i = 0; i < beak.Length; i++)
			beak[i].DrawSprites(sLeaser, rCam, timeStacker, camPos, headPos, neckDir, neckPerp, neckRot, flip);
        var sprites = sLeaser.sprites;
        if (MoveJawsAbove)
        {
            for (var i = FIRST_BEAK_PART; i < sprites.Length; i++)
                sprites[i].MoveToFront();
            MoveJawsAbove = false;
        }
        var sprite = sprites[HEAD];
        sprite.SetPosition(headPos - camPos);
		sprite.rotation = neckRot;
		var eyePos = EyePos(timeStacker);
		sprite = sprites[EYE];
        sprite.SetPosition(eyePos - camPos);
        sprite.rotation = neckRot;
        sprite.isVisible = !Ow.dead && (Ow.room is not Room rm || !rm.PointSubmerged(eyePos));
        sprite.color = EyeCol;
        sprite.scaleX = .3f * Math.Abs(flip) * EyeSize;
		var fc = Ow.firstChunk;
		var fcPos = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
        sprite = sprites[BODY];
        sprite.SetPosition(fcPos - camPos);
		fc = Ow.bodyChunks[1];
		var ch1Pos = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
        var chunksDir = Custom.DirVec(fcPos, ch1Pos);
		sprite.rotation = Custom.AimFromOneVectorToAnother(fcPos, ch1Pos);
		var scaleFac = 0f;
		var owLegs = Ow.Legs;
		for (var n = 0; n <= 2; n += 2)
		{
            ref var hip0 = ref owLegs[n].Hip;
            ref var hip1 = ref owLegs[n + 1].Hip;
			scaleFac += Custom.LerpMap(Vector2.Distance(Vector2.Lerp(hip0.LastPos, hip0.Pos, timeStacker), Vector2.Lerp(hip1.LastPos, hip1.Pos, timeStacker)), 5f, 50f, .75f, 1.1f);
        }
		sprite.scaleX = scaleFac * 2.2f / owLegs.Length;
        sprite.scaleY = 1.2f;
		var legs = Legs;
		for (var k = 0; k < legs.Length; k++)
			legs[k].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		var neckConnCh = Ow.Neck.connectedChunk;
        var connPos = Vector2.Lerp(neckConnCh.lastPos, neckConnCh.pos, timeStacker);
		var strchRad = 8f;
		var tchs = Ow.Neck.tChunks;
		var neck = (sprites[NECK] as TriangleMesh)!;
        for (var l = 0; l < tchs.Length; l++)
		{
			var tch = tchs[l];
            var tchPos = Vector2.Lerp(tch.lastPos, tch.pos, timeStacker);
			if (l == tchs.Length - 1)
				tchPos = Vector2.Lerp(tchPos, headPos, .5f);
			else if (l == 0)
				tchPos = Vector2.Lerp(tchPos, fcPos + chunksDir * 40f, .3f);
			var relativeDir = (tchPos - connPos).normalized;
			var perp = Custom.PerpendicularVector(relativeDir);
			var connDist = Vector2.Distance(tchPos, connPos) * .2f;
			var sradTemp = tch.stretchedRad;
            neck.MoveVertice(l * 4, connPos - perp * ((sradTemp + strchRad) * .5f * NeckFatness) + relativeDir * (connDist * (l == 0 ? 0f : 1f)) - camPos);
            neck.MoveVertice(l * 4 + 1, connPos + perp * ((sradTemp + strchRad) * .5f * NeckFatness) + relativeDir * (connDist * (l == 0 ? 0f : 1f)) - camPos);
            neck.MoveVertice(l * 4 + 2, tchPos - perp * (sradTemp * NeckFatness) - relativeDir * (connDist * (l == tchs.Length - 1 ? 0f : 1f)) - camPos);
			neck.MoveVertice(l * 4 + 3, tchPos + perp * (sradTemp * NeckFatness) - relativeDir * (connDist * (l == tchs.Length - 1 ? 0f : 1f)) - camPos);
			strchRad = tch.stretchedRad;
			connPos = tchPos;
		}
        var pastWdth = 0f;
        var tube = Tube;
        var mesh = (sprites[TUBE] as TriangleMesh)!;
        for (var l = 0; l < tube.Length; l++)
        {
            ref var t = ref tube[l];
            var segPos = l == tube.Length - 1 ? fcPos : Vector2.Lerp(t.LastPos, t.Pos, timeStacker);
            var segDir = (segPos - headPos).normalized;
            var segPerp = Custom.PerpendicularVector(segDir);
            var segDist = Vector2.Distance(segPos, headPos) * .2f;
            var wdth = l % 3 == 0 ? 3f : 2f;
            mesh.MoveVertice(l * 4, headPos - segPerp * ((wdth + pastWdth) * .5f) + segDir * segDist - camPos);
            mesh.MoveVertice(l * 4 + 1, headPos + segPerp * ((wdth + pastWdth) * .5f) + segDir * segDist - camPos);
            mesh.MoveVertice(l * 4 + 2, segPos - segPerp * wdth - segDir * segDist - camPos);
            mesh.MoveVertice(l * 4 + 3, segPos + segPerp * wdth - segDir * segDist - camPos);
            pastWdth = wdth;
            headPos = segPos;
        }
    }

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
		{
			if (i is not TRAIL and not EYE)
				sprites[i].color = palette.blackColor;
		}
        var legs = Legs;
        for (var k = 0; k < legs.Length; k++)
            legs[k].ApplyPalette(sLeaser, rCam, in palette);
    }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite is not EYE and not TRAIL and not (FIRST_LEG_PART + 2) and not (FIRST_LEG_PART + LegGraphic.SPRITES + 2) and not (FIRST_LEG_PART + 2 * LegGraphic.SPRITES + 2) and not LAST_LEG_PART;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
    {
        var sprites = sLeaser.sprites;
        var lMap = mudOverlay.layeringMap;
        lMap[mudOverlay.GetMudSprite(FIRST_LEG_PART + 1)] = sprites[FIRST_LEG_PART + 2];
        lMap[mudOverlay.GetMudSprite(FIRST_LEG_PART + LegGraphic.SPRITES + 1)] = sprites[FIRST_LEG_PART + LegGraphic.SPRITES + 2];
        lMap[mudOverlay.GetMudSprite(FIRST_LEG_PART + 2 * LegGraphic.SPRITES + 1)] = sprites[FIRST_LEG_PART + 2 * LegGraphic.SPRITES + 2];
        lMap[mudOverlay.GetMudSprite(FIRST_LEG_PART + 3 * LegGraphic.SPRITES + 1)] = sprites[LAST_LEG_PART];
    }
}