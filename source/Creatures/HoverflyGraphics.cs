using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;

public class HoverflyGraphics : GraphicsModule, ILookingAtCreatures, IMuddableGraphics
{
    public enum HeadState
    {
        FlyFastRight = -3,
        FlyRight = -2,
        LookRight = -1,
        Neutral,
        LookLeft,
        FlyLeft,
        FlyFastLeft
    }

	public enum WingState
	{
        Swarm,
        Flee,
        Flee2,
        Glide,
        Crawl,
        Dead,
		Water
	}

    public const int BODY_SPRITE = 0,
		HIGHLIGHT_SPRITE = 1,
        EYE_A_DARK = 6,
        EYE_A_SPRITE = 7,
        EYE_B_DARK = 8,
        EYE_B_SPRITE = 9,
		TOTAL_SPRITES = 10;
    public Vector2 ZRotation;
	public int BlinkCounter;
	public Vector2 LookDir;
	public float WingDeploymentGetTo, ClimbCounter, BaseScale, BaseSMScale;
	public CreatureLooker CreatureLooker;
	public BodyPart[][] Wings;
	public float[][] WingDeployment, WingDeploymentSpeed;
	public SoundID CurrentLoop = SoundID.None;
	public ChunkSoundEmitter? LoopSoundEmitter;
	public HeadState CurrentHead;
	public WingState CurrentWing;
	public int EyeFearCounter, GlideCounter;
	public float DeathWingPosition = Random.value;
	public float WingProgress;
	public float LastWingProgress;
	public bool PushWingUp;
	public int ChirpCounter;

    public virtual float DangerPosXBonus => CurrentHead switch
    {
        HeadState.FlyFastLeft => -4f,
        HeadState.FlyLeft or HeadState.LookLeft => -2f,
        HeadState.FlyFastRight => 4f,
        HeadState.FlyRight or HeadState.LookRight => 2f,
        _ => 0f
    };

    public virtual Hoverfly.IndividualVariations IVars => Fly.IVars;

    public virtual string HeadSprite => CurrentHead switch
	{
		HeadState.FlyFastLeft or HeadState.FlyFastRight => "HoverflyHead4",
        HeadState.FlyLeft or HeadState.FlyRight => "HoverflyHead3",
        HeadState.LookLeft or HeadState.LookRight => "HoverflyHead2",
		_ => "HoverflyHead1"
    };

    public virtual string HeadHighlightSprite => CurrentHead switch
    {
        HeadState.FlyFastLeft or HeadState.FlyFastRight => "HoverflyHeadHighlight4",
        HeadState.FlyLeft or HeadState.FlyRight => "HoverflyHeadHighlight3",
        HeadState.LookLeft or HeadState.LookRight => "HoverflyHeadHighlight2",
        _ => "HoverflyHeadHighlight1"
    };

	public virtual string EyeVar => EyeFearCounter > 0 ? (EyeFearCounter > 10 ? "3" : "2") : "1";

	public virtual float LeftEyeXPos => CurrentHead switch
    {
        HeadState.FlyFastLeft => -8f,
        HeadState.FlyLeft => -7f,
        HeadState.LookLeft => -6f,
        HeadState.LookRight => -4f,
        HeadState.FlyRight => -2f,
        HeadState.FlyFastRight => 0f,
        _ => -5f
    };

    public virtual float RightEyeXPos => CurrentHead switch
    {
        HeadState.FlyFastLeft => 0f,
        HeadState.FlyLeft => 2f,
        HeadState.LookLeft => 4f,
        HeadState.LookRight => 6f,
        HeadState.FlyRight => 7f,
        HeadState.FlyFastRight => 8f,
        _ => 5f
    };

    public virtual Hoverfly Fly => (owner as Hoverfly)!;

	public HoverflyGraphics(Hoverfly ow) : base(ow, false)
	{
        var wings = Wings = new BodyPart[2][];
        for (var i = 0; i < wings.Length; i++)
        {
            var wing = wings[i] = new BodyPart[2];
            for (var j = 0; j < wing.Length; j++)
                wing[j] = new(this);
        }
        var w = WingDeployment = new float[2][];
        var ws = WingDeploymentSpeed = new float[2][];
        for (var i = 0; i < ws.Length; i++)
            ws[i] = new float[2];
		for (var i = 0; i < w.Length; i++)
		{
			var wi = w[i] = new float[2];
			for (var j = 0; j < wi.Length; j++)
				wi[j] = ow.IVars.DefaultWingDeployment;
		}
        bodyParts =
        [
            Wings[0][0],
			Wings[1][0],
			Wings[0][1],
			Wings[1][1]
        ];
        CreatureLooker = new(this, ow.AI.tracker, ow, .4f, 20);
        ChirpCounter = Random.Range(150, 601);
        Reset();
	}

    public virtual int WingSprite(int side, int wing) => 2 + side + wing + wing;

    public override void Update()
	{
		base.Update();
		if (Fly is not Hoverfly f || f.room is not Room rm)
			return;
		var num = 0f;
		var wingDeps = WingDeployment;
        for (var i = 0; i < wingDeps.Length; i++)
		{
			var wi = wingDeps[i];
            for (var j = 0; j < wi.Length; j++)
			{
				if (wi[j] == 1f)
					num += .25f;
			}
		}
		var flag = f.Consious;
        if (flag)
		{
            if (ChirpCounter > 0)
                --ChirpCounter;
            else
            {
                ChirpCounter = Random.Range(400, 701);
                rm.PlaySound(NewSoundID.M4R_Hoverfly_Idle, f.firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
            }
        }
		num = Mathf.Pow(num, 1.4f) * 1.25f;
		var soundID = SoundID.None;
		if (flag && num > 0f)
			soundID = NewSoundID.M4R_Hoverfly_Fly_LOOP;
		if (soundID != CurrentLoop)
		{
			if (LoopSoundEmitter is not null)
			{
				LoopSoundEmitter.alive = false;
				LoopSoundEmitter = null;
			}
			if (soundID == SoundID.None)
				rm.PlaySound(flag ? SoundID.Fly_Caught : SoundID.Fly_Caught_Dead, f.firstChunk, false, num, 1f + IVars.SoundPitchBonus);
			CurrentLoop = soundID;
			if (CurrentLoop != SoundID.None)
			{
				LoopSoundEmitter = rm.PlaySound(CurrentLoop, f.firstChunk, true, 1.25f, 1f);
				LoopSoundEmitter.requireActiveUpkeep = true;
			}
		}
		if (LoopSoundEmitter is ChunkSoundEmitter snd)
		{
            snd.alive = true;
            snd.volume = num;
			if (CurrentLoop == NewSoundID.M4R_Hoverfly_Fly_LOOP)
                snd.pitch = (1f + IVars.SoundPitchBonus) * (.85f + .3f * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(default, ZRotation.normalized)), 2f));
			else
                snd.pitch = 1f + IVars.SoundPitchBonus;
			if (snd.room != rm)
                snd.Destroy();
			if (snd.slatedForDeletetion)
			{
				LoopSoundEmitter = null;
				CurrentLoop = SoundID.None;
			}
		}
		CreatureLooker.Update();
		ZRotation = Vector2.Lerp(ZRotation, Custom.DirVec(f.firstChunk.lastPos, f.firstChunk.pos), .15f);
		if (flag)
		{
            --BlinkCounter;
			if (BlinkCounter < -15 || (BlinkCounter < -2 && Random.value < 1f / 3f))
				BlinkCounter = Random.Range(10, 300);
			if (!f.Flying && !Custom.DistLess(f.firstChunk.pos, f.firstChunk.lastPos, 0.4f))
			{
				ClimbCounter += Vector2.Distance(f.firstChunk.pos, f.firstChunk.lastPos);
				if (ClimbCounter > 30f)
				{
					f.bodyChunks[0].vel += Custom.DegToVec(Random.value * 360f);
					rm.PlaySound(SoundID.Fly_Wing_Flap, f.firstChunk, false, 1.25f, 1f + IVars.SoundPitchBonus);
					ClimbCounter = 0f;
				}
			}
			if (f.Flying)
			{
				WingDeploymentGetTo = 1f;
                ZRotation += Custom.DegToVec(112.5f) * .3f;
            }
			else
			{
				if (f.SitDirection.x != 0)
					ZRotation += new Vector2(.4f * -f.SitDirection.x, 0f);
				else
					ZRotation += new Vector2(.4f * f.SitDirection.y, 0f);
				if (f.WaitToFlyCounter > 0)
					WingDeploymentGetTo = .9f;
				else if (WingDeploymentGetTo == 1f)
					WingDeploymentGetTo = .9f;
				else if (Random.value < 1f / 14f)
					WingDeploymentGetTo = Mathf.Max(0f, WingDeploymentGetTo - Random.value / 6f);
			}
		}
		else
			BlinkCounter = -5;
		ZRotation = ZRotation.normalized;
		Vector2 vector = Custom.DirVec(f.firstChunk.lastPos, f.firstChunk.pos),
			vector2 = Custom.PerpendicularVector(vector);
		if (flag && CreatureLooker.lookCreature is Tracker.CreatureRepresentation rep && BlinkCounter > 0)
		{
			if (rep.VisualContact)
				LookDir = Custom.DirVec(f.firstChunk.pos, rep.representedCreature.realizedCreature.DangerPos);
			else
				LookDir = Custom.DirVec(f.firstChunk.pos, rm.MiddleOfTile(rep.BestGuessForPosition()));
		}
		else
			LookDir *= .9f;
		var num2 = flag ? 1f : .5f;
		var wdeps = WingDeployment;
        for (var k = 0; k < wdeps.Length; k++)
		{
			var wk = wdeps[k];
			var wsk = WingDeploymentSpeed[k];
			for (var l = 0; l < wk.Length; l++)
			{
				if (flag)
				{
					if (Random.value < 1f / 30f)
						wsk[l] = Random.value * Random.value * .3f;
					if (WingDeploymentGetTo == 1f)
						wk[l] = 1f;
					else if (wk[l] < WingDeploymentGetTo)
					{
						wk[l] = Mathf.Min(wk[l] + wsk[l], WingDeploymentGetTo);
						if (f.WaitToFlyCounter > 0)
							wk[l] = Mathf.Min(wk[l] + wsk[l] * 2.4f, WingDeploymentGetTo);
					}
					else if (wk[l] > WingDeploymentGetTo)
					{
						if (wk[l] == 1f)
							ResetWing(k, l);
						wk[l] = Mathf.Max(wk[l] - wsk[l], WingDeploymentGetTo);
					}
				}
				else if (wk[l] == 1f)
				{
					wk[l] = .9f;
					ResetWing(k, l);
				}
				if (wk[l] < 1f)
				{
					var w = Wings[k][l];
                    w.lastPos = w.pos;
					w.pos += w.vel;
					w.vel *= .8f;
					var t = Mathf.InverseLerp(.5f, 1f, wk[l]) * f.FlyingPower;
					w.vel -= vector * ((l == 0 ? .6f : .3f) * num2 * Mathf.Lerp(1f, l == 0 ? 0f : -5.5f, t));
					w.vel += vector2 * ((k == 0 ? -.2f : .2f) * Math.Abs(ZRotation.y) * num2 * Mathf.Lerp(1f, 6f, t));
					w.vel += vector2 * (.2f * ZRotation.x * num2 * Mathf.Lerp(1f, 6f, t));
					if (!flag)
						w.vel.y -= .3f;
					if (wk[l] < .5f)
					{
						var num3 = Mathf.InverseLerp(.5f, 0f, wk[l]);
						var b = f.firstChunk.pos - vector * (l == 0 ? 20f : 15f);
						b += vector2 * ((k == 0 ? 2f : -2f) * Math.Abs(ZRotation.y));
						b += vector2 * (5f * ZRotation.x);
						w.vel *= 1f - num3;
						w.pos = Vector2.Lerp(w.pos, b, num3);
					}
					w.Update();
					w.ConnectToPoint(f.firstChunk.pos - vector * 5f, (l == 0 ? 23f : 17f) * (1f + (l == 0 ? IVars.BigWingBonus : IVars.SmallWingBonus)), true, 0f, f.firstChunk.vel, .5f, 0f);
					w.PushOutOfTerrain(rm, f.firstChunk.pos);
				}
			}
		}
		if (EyeFearCounter > 0)
            --EyeFearCounter;
		if (GlideCounter > 0)
            --GlideCounter;
		if (LookDir.x > .25f)
			CurrentHead = HeadState.LookRight;
		else if (LookDir.x < -.25f)
			CurrentHead = HeadState.LookLeft;
		else
			CurrentHead = HeadState.Neutral;
		var xspeed = f.firstChunk.vel.x;
        if (xspeed > 3f)
		{
			if (xspeed > 6f)
				CurrentHead = HeadState.FlyFastRight;
			else
				CurrentHead = HeadState.FlyRight;
		}
		else if (xspeed < -3f)
        {
            if (xspeed < -6f)
                CurrentHead = HeadState.FlyFastLeft;
            else
                CurrentHead = HeadState.FlyLeft;
        }
		WingMovementUpdate();
    }

	public virtual void ResetWing(int side, int wing)
	{
		var fc = Fly.firstChunk;
		Vector2 vector = Custom.DirVec(fc.lastPos, fc.pos), vector2 = Custom.PerpendicularVector(vector);
		var w = Wings[side][wing];
		w.vel = default;
		w.pos = fc.pos - vector * 5f;
		w.pos += (wing == 0 ? -3f : 10f) * vector;
		w.pos += vector2 * ((side == 0 ? -17f : 17f) * Math.Abs(ZRotation.y));
		w.pos += vector2 * (17f * ZRotation.x);
		w.ConnectToPoint(fc.pos - vector * 5f, wing == 0 ? 23f : 17f, true, 0f, fc.vel, 0f, 0f);
		w.PushOutOfTerrain(Fly.room, fc.pos);
	}

	public override void Reset()
	{
		base.Reset();
		var array = bodyParts;
		for (var i = 0; i < array.Length; i++)
		{
			var obj = array[i];
			obj.vel = default;
			obj.pos = Fly?.firstChunk.pos ?? default;
			obj.lastPos = obj.pos;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		BaseScale = 1f + IVars.BodyBonus;
		BaseSMScale = 1f + IVars.SmallWingBonus;
        sLeaser.sprites =
        [
            new(HeadSprite) { scale = BaseScale },
            new(HeadHighlightSprite) { scale = BaseScale },
            new($"HoverflyBigWing{IVars.WingVar}") { anchorX = 0f, anchorY = 0f, scale = 1f + IVars.BigWingBonus },
            new($"HoverflyBigWing{IVars.WingVar}") { anchorX = 0f, anchorY = 0f, scale = 1f + IVars.BigWingBonus },
            new("HoverflySmallWing1") { anchorX = 0f, anchorY = .14f, scale = BaseSMScale },
            new("HoverflySmallWing1") { anchorX = 0f, anchorY = .14f, scale = BaseSMScale },
            new($"HoverflyEye{EyeVar}11"),
            new($"HoverflyEye{EyeVar}21"),
            new($"HoverflyEye{EyeVar}11"),
            new($"HoverflyEye{EyeVar}21")
        ];
		sLeaser.sprites[2].scaleX = -sLeaser.sprites[2].scaleX;
        sLeaser.sprites[4].scaleX = -sLeaser.sprites[4].scaleX;
        AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public virtual void WingMovementUpdate()
	{
		if (Fly.Consious)
		{
            if (Fly.firstChunk.submersion > 0f)
                CurrentWing = WingState.Water;
            else if (!Fly.Flying || Fly.room?.aimap is AImap m && m.getAItile(Fly.firstChunk.pos).narrowSpace || Fly.inShortcut)
                CurrentWing = WingState.Crawl;
            else if (Fly.safariControlled)
                CurrentWing = WingState.Flee2;
			else
			{
                if (Fly.AI is HoverflyAI ai && (ai.Behavior == HoverflyAI.FlyBehavior.Flee || ai.Behavior == HoverflyAI.FlyBehavior.EscapeRain))
                    CurrentWing = WingState.Flee2;
                else if (Fly.firstChunk.vel.x < 5f && Fly.firstChunk.vel.y < 5f)
                    CurrentWing = WingState.Swarm;
                else
                    CurrentWing = WingState.Flee;
                if (GlideCounter == 0 && Random.value < .005f && (Fly.firstChunk.vel.x > 2f || Fly.firstChunk.vel.y > 2f))
                {
                    GlideCounter = 80;
                    CurrentWing = WingState.Glide;
                }
                else if (GlideCounter > 0)
                    CurrentWing = WingState.Glide;
            }
		}
		else
			CurrentWing = WingState.Dead;
        switch (CurrentWing)
		{
			case WingState.Water:
                LastWingProgress = WingProgress;
                if (WingProgress <= 0f)
                    PushWingUp = true;
                else if (WingProgress >= 1f)
                    PushWingUp = false;
                WingProgress += (PushWingUp ? .125f : -.085f) / 3f;
                break;
			case WingState.Glide:
				if (WingProgress < .3f)
					WingProgress = .3f;
				else if (WingProgress > .5f)
                    WingProgress = .5f;
                LastWingProgress = WingProgress;
                if (WingProgress <= .38f)
                    PushWingUp = true;
                else if (WingProgress >= .42f)
                    PushWingUp = false;
                WingProgress += PushWingUp ? .04f : -.02f;
				Fly.firstChunk.vel.x *= 1.02f;
                break;
			case WingState.Swarm:
                LastWingProgress = WingProgress;
                if (WingProgress <= .1f)
                    PushWingUp = true;
                else if (WingProgress >= .9f)
                    PushWingUp = false;
                WingProgress += PushWingUp ? 1f : -.5f;
                break;
			case WingState.Flee:
                LastWingProgress = WingProgress;
                if (WingProgress <= 0f)
                    PushWingUp = true;
                else if (WingProgress >= 1f)
                    PushWingUp = false;
                WingProgress += PushWingUp ? .125f : -.085f;
                break;
            case WingState.Flee2:
                LastWingProgress = WingProgress;
                if (WingProgress <= 0f)
                    PushWingUp = true;
                else if (WingProgress >= 1f)
                    PushWingUp = false;
                WingProgress += PushWingUp ? .25f : -.17f;
                break;
            case WingState.Crawl:
            case WingState.Dead:
				var fc = Fly.firstChunk;
                var y = Custom.RotateAroundOrigo(fc.pos - fc.lastPos, Custom.AimFromOneVectorToAnother(fc.lastPos, fc.pos)).y;
                LastWingProgress = WingProgress;
                WingProgress = Mathf.Clamp(Mathf.Lerp(WingProgress, DeathWingPosition - y * .1f, .3f), 0f, 1f);
                break;
		}
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled || Fly is not Hoverfly f || f.firstChunk is not BodyChunk b)
			return;
		var vector = Vector2.Lerp(b.lastPos, b.pos, timeStacker);
		var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
			var spr = sprs[i];
			if (i < TOTAL_SPRITES) // for M4RNightFly
			{
				spr.x = vector.x - camPos.x;
				spr.y = vector.y - camPos.y;
				if (i >= 6)
				{
					spr.x += i < 8 ? RightEyeXPos : LeftEyeXPos;
					spr.y -= 2f - LookDir.y * 2f;
					spr.element = Futile.atlasManager.GetElementWithName($"HoverflyEye{EyeVar}{(i is 7 or 9 ? 2 : 1)}1");
				}
				else if (i == 0)
				{
					spr.SetElementByName(HeadSprite);
					spr.scaleX = BaseScale * (CurrentHead < 0 ? -1f : 1f);
				}
				else if (i == 1)
				{
					spr.SetElementByName(HeadHighlightSprite);
					spr.scaleX = BaseScale * (CurrentHead < 0 ? -1f : 1f);
				}
			}
        }
        sprs[2].x += 1f;
        sprs[3].x -= 1f;
        sprs[4].y -= 5f;
        sprs[5].y -= 5f;
		var flag = CurrentWing == WingState.Swarm;
        for (var i = 0; i < 2; i++)
        {
			var s4i = sprs[4 + i];
            s4i.rotation = (i == 0 ? -1f : 1f) * (40f + 100f * Mathf.Lerp(LastWingProgress, WingProgress, timeStacker)) + (i == 0 ? 90f : -90f);
			if (flag)
			{
				s4i.scaleY = BaseSMScale * -1f;
				s4i.SetElementByName("HoverflySmallWing2");
				s4i.rotation *= -1f;
            }
			else
			{
                s4i.SetElementByName("HoverflySmallWing1");
                s4i.scaleY = BaseSMScale;
            }
            sprs[2 + i].rotation = (i == 0 ? -1f : 1f) * (40f + 100f * Mathf.Lerp(LastWingProgress, WingProgress, timeStacker)) + (i == 0 ? 90f : -90f);
        }
        sprs[4].x += 4f;
        sprs[5].x -= 4f;
        sprs[EYE_B_SPRITE].isVisible = sprs[EYE_A_SPRITE].isVisible = BlinkCounter >= 0;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		var sprs = sLeaser.sprites;
        sprs[BODY_SPRITE].color = palette.blackColor;
        sprs[HIGHLIGHT_SPRITE].color = Color.white;
        for (var i = 2; i < 7; i++)
            sprs[i].color = palette.blackColor;
        sprs[EYE_B_DARK].color = palette.blackColor;
        sprs[EYE_A_SPRITE].color = IVars.Color;
        sprs[EYE_B_SPRITE].color = IVars.Color;
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

	public virtual float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score) => score;

    public virtual Tracker.CreatureRepresentation? ForcedLookCreature() => Fly?.AI?.FocusCreature;

    public virtual void LookAtNothing() { }

    public virtual bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite) => sprite is 0 or 2 or 3 or 4 or 5;

    public virtual void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay) => mudOverlay.layeringMap[mudOverlay.GetMudSprite(0)] = sLeaser.sprites[1];
}