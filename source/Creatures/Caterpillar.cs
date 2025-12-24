using LBMergedMods.Hooks;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class Caterpillar : InsectoidCreature, IProvideWarmth
{
    public CaterpillarAI? AI;
	public bool Moving, Glowing;
    public Rope[]? ConnectionRopes;
	public Vector2 MoveToPos;
	public int NoFollowConCounter, StunCounter;
	public float BodyWave;

    public virtual Room loadedRoom => room;

    public virtual float warmth => Consious && Glowing && graphicsModule is CaterpillarGraphics gr ? RainWorldGame.DefaultHeatSourceWarmth * 1.5f * gr.LightAlpha : 0f;

    public virtual float range => 170f;

    public override Vector2 VisionPoint => firstChunk.pos;

    public new virtual HealthState State => (abstractCreature.state as HealthState)!;

	public Caterpillar(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
	{
		Glowing = abstractCreature.creatureTemplate.type == CreatureTemplateType.Glowpillar;
        var state = Random.state;
		Random.InitState(abstractCreature.ID.RandomSeed);
		var chs = bodyChunks = new BodyChunk[Random.Range(8, 11)];
		for (var i = 0; i < chs.Length; i++)
			chs[i] = new(this, i, default, Mathf.Lerp(Mathf.Lerp(2f, 3.5f, 1f), Mathf.Lerp(4f, 6.5f, 1f), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * i / (chs.Length - 1)), 0f, 1f), Mathf.Lerp(.7f, .3f, 1f))) + .45f, Mathf.Lerp(3f / 70f, 11f / 34f, Mathf.Pow(1f, 1.4f)) + .01f)
			{
				loudness = .05f
			};
		mainBodyChunkIndex = chs.Length / 2;
		bodyChunkConnections = new BodyChunkConnection[chs.Length * (chs.Length - 1) / 2];
		var num3 = 0;
		for (var l = 0; l < chs.Length; l++)
		{
			var chl = chs[l];
			for (var m = l + 1; m < chs.Length; m++)
			{
				var chm = chs[m];
				bodyChunkConnections[num3] = new(chl, chm, chl.rad + chm.rad, BodyChunkConnection.Type.Push, .9f, -1f);
                ++num3;
			}
		}
		airFriction = .999f;
		gravity = .9f;
		bounce = .1f;
		surfaceFriction = .4f;
		collisionLayer = 1;
		waterFriction = .96f;
		buoyancy = 1.05f;
		collisionRange = 150f;
		Random.state = state;
	}

	public override void InitiateGraphicsModule() => graphicsModule ??= new CaterpillarGraphics(this);

    public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		var chs = bodyChunks;
		var rps = ConnectionRopes = new Rope[chs.Length - 1];
		for (var i = 0; i < rps.Length; i++)
			rps[i] = new Rope(newRoom, chs[i].pos, chs[i + 1].pos, 1f);
	}

	public override void Update(bool eu)
	{
        var chs = bodyChunks;
		base.Update(eu);
		if (room is not Room rm)
			return;
        if (grasps[0] is not null)
            UpdateGrasp();
        if (!enteringShortCut.HasValue && ConnectionRopes is Rope[] cons)
		{
			for (var l = 0; l < cons.Length; l++)
			{
				var chl = chs[l];
				var chlp1 = chs[l + 1];
                var con = cons[l];
                con.Update(chl.pos, chlp1.pos);
				var totalLength = con.totalLength;
				var num2 = chl.rad + chlp1.rad;
				if (totalLength > num2)
				{
					var num3 = chl.mass / (chl.mass + chlp1.mass);
					var vector = Custom.DirVec(chl.pos, con.AConnect);
					chl.vel += vector * ((totalLength - num2) * num3);
					chl.pos += vector * ((totalLength - num2) * num3);
					vector = Custom.DirVec(chlp1.pos, con.BConnect);
                    chlp1.vel += vector * ((totalLength - num2) * (1f - num3));
                    chlp1.pos += vector * ((totalLength - num2) * (1f - num3));
				}
			}
			for (var num5 = cons.Length - 2; num5 >= 0; num5--)
			{
				var ch5 = chs[num5];
				var ch5p1 = chs[num5 + 1];
                var con = ConnectionRopes[num5];
                con.Update(ch5.pos, ch5p1.pos);
				float totalLength2 = con.totalLength,
					num6 = ch5.rad + ch5p1.rad;
				if (totalLength2 > num6)
				{
					var num7 = ch5.mass / (ch5.mass + ch5p1.mass);
					var vector2 = Custom.DirVec(ch5.pos, con.AConnect);
                    ch5.vel += vector2 * ((totalLength2 - num6) * num7);
                    ch5.pos += vector2 * ((totalLength2 - num6) * num7);
					vector2 = Custom.DirVec(ch5p1.pos, con.BConnect);
                    ch5p1.vel += vector2 * ((totalLength2 - num6) * (1f - num7));
                    ch5p1.pos += vector2 * ((totalLength2 - num6) * (1f - num7));
				}
			}
			for (var m = 0; m < chs.Length - 2; m++)
			{
				var chm = chs[m];
				var chmp2 = chs[m + 2];
                chm.vel += Custom.DirVec(chmp2.pos, chm.pos) * 2f;
                chmp2.vel += Custom.DirVec(chm.pos, chmp2.pos) * 2f;
			}
		}
        if (grasps[0] is not null)
            UpdateGrasp();
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
		{
			chs[0].vel += Custom.DirVec(chs[0].pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (dead && grabbedBy.Count == 0)
		{
			for (var num9 = 0; num9 < chs.Length; num9++)
			{
				var ch9 = chs[num9];
                if (ch9.ContactPoint.y != 0)
                    ch9.vel.x *= .1f;
			}
		}
		else if (State.health < .75f)
		{
			if (Random.value * .75f > State.health && stun > 0)
                --stun;
			if (Random.value > State.health && Random.value < 1f / 3f)
			{
				Stun(4);
				if (State.health <= 0f && Random.value < 1f / Mathf.Lerp(500f, 10f, -State.health))
					Die();
			}
			if (!dead)
			{
				for (var num10 = 0; num10 < chs.Length; num10++)
				{
					if (Random.value > State.health * 2f)
						chs[num10].vel += Custom.RNV() * (Mathf.Pow(Random.value, Custom.LerpMap(State.health, .75f, 0f, 3f, .1f, 2f)) * 4f * Mathf.InverseLerp(.75f, 0f, State.health));
				}
			}
		}
		if (Consious)
		{
			if (Submersion > .5f)
                Swim();
            else
				Act();
		}
	}

	public virtual void Swim()
	{
		var chs = bodyChunks;
		var fch = chs[0];
        var worldCoordinate = room.GetWorldCoordinate(fch.pos);
		worldCoordinate.y = Math.Max(worldCoordinate.y, room.DefaultWaterLevel(worldCoordinate.Tile));
		MovementConnection movementConnection = default;
		if (safariControlled)
		{
			if (inputWithoutDiagonals.HasValue)
			{
				var type = MovementConnection.MovementType.Standard;
				if (room.GetTile(mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					type = MovementConnection.MovementType.ShortCut;
				if (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0)
					movementConnection = new(type, room.GetWorldCoordinate(mainBodyChunk.pos), room.GetWorldCoordinate(mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * Mathf.Max(80f, 240f)), 2);
			}
		}
		else
			movementConnection = (AI!.pathFinder as StandardPather)!.FollowPath(worldCoordinate, true);
		if (movementConnection == default)
			return;
		if (movementConnection.destinationCoord.y > movementConnection.startCoord.y)
		{
			if (room.aimap.TileAccessibleToCreature(fch.pos, Template))
                Act();
            ++fch.vel.y;
			return;
		}
        fch.vel += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2()) * 1.2f;
		if (room.aimap.TileAccessibleToCreature(fch.pos, Template))
		{
			for (var i = 0; i < chs.Length; i++)
			{
				var chi = chs[i];
				chi.vel += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2()) * .05f + Custom.RNV() * (Random.value * 4f);
                chi.vel.y += Mathf.Clamp(room.WaterLevelDisplacement(chi.pos), -5f, 5f) * .05f;
            }
		}
		if (!Moving)
			return;
		for (var j = 1; j < chs.Length; j++)
            chs[j].vel += Custom.DirVec(chs[j].pos, chs[j - 1].pos) * .3f;
    }

    public virtual void Act()
	{
		AI!.Update();
		var grasps = this.grasps;
        var bodyChunks = this.bodyChunks;
        var fch = bodyChunks[0];
        Moving = AI.Run > 0f && Custom.ManhattanDistance(room.GetWorldCoordinate(fch.pos), AI.pathFinder.GetDestination) > 2;
        if (safariControlled)
		{
			Moving = false;
			if (inputWithoutDiagonals is Player.InputPackage input && (input.x != 0 || input.y != 0))
				Moving = true;
			if ((inputWithoutDiagonals?.thrw is true || grabbedBy.Count > 0) && grasps[0] is not null)
				ReleaseGrasp(0);
			else if (inputWithoutDiagonals?.pckp is true && grabbedBy.Count == 0 && Consious && grasps[0] is null)
			{
                var crits = abstractCreature.Room.creatures;
                for (var l = 0; l < crits.Count; l++)
                {
					var absc = crits[l];
                    if (!AI.DoIWantToEatCreature(absc) || absc.realizedCreature is not Creature cre || cre == this)
                        continue;
                    var chs = cre.bodyChunks;
                    for (var n = 0; n < chs.Length; n++)
                    {
                        var chn = chs[n];
                        if (Custom.DistLess(fch.pos, chn.pos, 50f + chn.rad))
                        {
                            Grab(cre, 0, n, Grasp.Shareability.CanNotShare, abstractCreature.personality.dominance, false, false);
                            room.PlaySound(SoundID.Centipede_Attach, fch);
                            break;
                        }
                    }
                }
            }
		}
		if (Moving)
		{
            var movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(room.GetWorldCoordinate(fch.pos), true);
			if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)) && inputWithoutDiagonals is Player.InputPackage inp)
			{
				var type = MovementConnection.MovementType.Standard;
				if (room.GetTile(mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					type = MovementConnection.MovementType.ShortCut;
				if (inp.x != 0 || inp.y != 0)
					movementConnection = new(type, room.GetWorldCoordinate(mainBodyChunk.pos), room.GetWorldCoordinate(mainBodyChunk.pos + new Vector2(inp.x, inp.y) * Mathf.Max(80f, 240f)), 2);
				if (inp.y < 0)
					GoThroughFloors = true;
				else
					GoThroughFloors = false;
			}
			Moving = movementConnection != default;
			if (Moving)
			{
				if (shortcutDelay < 1 && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation))
				{
					enteringShortCut = movementConnection.StartTile;
					if (safariControlled)
					{
						var flag2 = false;
						var list = new List<IntVector2>();
						var shortcuts = room.shortcuts;
						for (var num4 = 0; num4 < shortcuts.Length; num4++)
						{
							ref readonly ShortcutData shortcutData = ref shortcuts[num4];
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
								list.Add(shortcutData.StartTile);
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
								flag2 = true;
						}
						if (flag2)
						{
							if (list.Count > 0)
							{
								list.Shuffle();
								NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
							}
							else
								NPCTransportationDestination = movementConnection.destinationCoord;
						}
					}
					else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
						NPCTransportationDestination = movementConnection.destinationCoord;
					return;
				}
				if (movementConnection.destinationCoord.TileDefined)
				{
					GoThroughFloors = movementConnection.DestTile.y < movementConnection.StartTile.y;
					MoveToPos = room.MiddleOfTile(movementConnection.DestTile);
					if (movementConnection.DestTile.x != movementConnection.StartTile.x)
						MoveToPos.y += VerticalSitSurface(MoveToPos) * 5f;
					if (movementConnection.DestTile.y != movementConnection.StartTile.y)
						MoveToPos.x += HorizontalSitSurface(MoveToPos) * 5f;
				}
				NoFollowConCounter = 0;
			}
			else
			{
                ++NoFollowConCounter;
				if (NoFollowConCounter > 40)
				{
					var ps = bodyChunks[Random.Range(0, bodyChunks.Length)].pos;
                    if (AccessibleTile(Room.StaticGetTilePosition(ps)))
						MoveToPos = ps;
				}
			}
		}
		Crawl(bodyChunks);
		if (AI.preyTracker.MostAttractivePrey?.representedCreature.realizedCreature is not Creature cr || cr.collisionLayer == collisionLayer)
			return;
		var crChs = cr.bodyChunks;
        for (var num10 = 0; num10 < crChs.Length; num10++)
		{
			var crCh = crChs[num10];
			for (var num11 = 0; num11 < bodyChunks.Length; num11++)
			{
				var ch = bodyChunks[num11];
				if (Custom.DistLess(crCh.pos, ch.pos, crCh.rad + ch.rad))
					Collide(cr, num11, num10);
			}
		}
	}

	public virtual void Crawl(BodyChunk[] bodyChunks)
	{
		var ex = AI?.Excitement ?? 1f;
        var num = 0;
		for (var i = 0; i < bodyChunks.Length; i++)
		{
			var ch = bodyChunks[i];
			if (!AccessibleTile(Room.StaticGetTilePosition(ch.pos)))
				continue;
            ++num;
            ch.vel *= .7f;
            ch.vel.y += gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(State.ClampedHealth, 1f, Random.value)), .25f) * (1f - ex * .25f);
			if (i > 0 && !AccessibleTile(Room.StaticGetTilePosition(bodyChunks[i - 1].pos)))
			{
                ch.vel *= .3f;
                ch.vel.y += gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(State.ClampedHealth, 1f, Random.value)), .25f) * (1f - ex * .25f);
			}
			if (i < bodyChunks.Length - 1 && !AccessibleTile(Room.StaticGetTilePosition(bodyChunks[i + 1].pos)))
			{
                ch.vel *= .3f;
                ch.vel.y += gravity * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(State.ClampedHealth, 1f, Random.value)), .25f) * (1f - ex * .25f);
			}
			if (i == 0 || i == bodyChunks.Length - 1)
				continue;
            if (Moving)
            {
                if (AccessibleTile(Room.StaticGetTilePosition(bodyChunks[i - 1].pos)))
                    ch.vel += Custom.DirVec(ch.pos, bodyChunks[i - 1].pos) * (1.5f * Mathf.Lerp(.5f, 1.5f, State.ClampedHealth) * 1.25f * ex);
                ch.vel -= Custom.DirVec(ch.pos, bodyChunks[i + 1].pos) * (.8f * Mathf.Lerp(.7f, 1.3f, State.ClampedHealth) * ex);
                continue;
            }
            var vector = ((ch.pos - bodyChunks[i - 1].pos).normalized + (bodyChunks[i + 1].pos - ch.pos).normalized) * .5f;
            if (Math.Abs(vector.x) > .5f)
                ch.vel.y -= (ch.pos.y - (room.MiddleOfTile(ch.pos).y + VerticalSitSurface(ch.pos) * (10f - ch.rad))) * Mathf.Lerp(.01f, .6f, Mathf.Pow(State.ClampedHealth, 1.2f));
            if (Math.Abs(vector.y) > .5f)
                ch.vel.x -= (ch.pos.x - (room.MiddleOfTile(ch.pos).x + HorizontalSitSurface(ch.pos) * (10f - ch.rad))) * Mathf.Lerp(.01f, .6f, Mathf.Pow(State.ClampedHealth, 1.2f));
        }
		if (num > 0 && !Custom.DistLess(bodyChunks[0].pos, MoveToPos, 10f))
            bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, MoveToPos) * (Custom.LerpMap(num, 0f, bodyChunks.Length, 6f, 3f) * Mathf.Lerp(.7f, 1.3f, State.health) * ex);
    }

    public virtual int VerticalSitSurface(Vector2 pos)
	{
		if (room.GetTile(pos with { y = pos.y - 20f }).Solid)
			return -1;
		if (room.GetTile(pos with { y = pos.y + 20f }).Solid)
			return 1;
		return 0;
	}

    public virtual int HorizontalSitSurface(Vector2 pos)
	{
		if (room.GetTile(pos with { x = pos.x - 20f }).Solid && !room.GetTile(pos with { x = pos.x + 20f }).Solid)
			return -1;
		if (room.GetTile(pos with { x = pos.x + 20f }).Solid && !room.GetTile(pos with { x = pos.x - 20f }).Solid)
			return 1;
		return 0;
	}

    public virtual bool AccessibleTile(IntVector2 testPos)
	{
		if (testPos.y != room.DefaultWaterLevel(testPos))
			return room.aimap.TileAccessibleToCreature(testPos, Template);
		return ClimbableTile(testPos);
	}

    public virtual bool ClimbableTile(IntVector2 testPos)
	{
		if (!room.GetTile(testPos).wallbehind && !room.GetTile(testPos).verticalBeam && !room.GetTile(testPos).horizontalBeam)
			return room.aimap.getTerrainProximity(testPos) < 2;
		return true;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (damage >= .2f && Consious && RippleViolenceCheck(source))
		{
            ReleaseGrasp(0);
            room.PlaySound(NewSoundID.M4R_GenericBug_Chip, firstChunk, false, 1f, .85f);
        }
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage * .075f, stunBonus * (1f - State.health));
	}

    public override void ReleaseGrasp(int grasp)
    {
		StunCounter = 0;
        base.ReleaseGrasp(grasp);
    }
    public virtual void UpdateGrasp()
    {
		if (!Consious || grasps[0].grabbed is not Creature c || !AI!.DoIWantToEatCreature(c.abstractCreature))
		{
			ReleaseGrasp(0);
			return;
		}
		var fch = firstChunk;
		var grabbedCh = grasps[0].grabbedChunk;
        var num = Vector2.Distance(fch.pos, grabbedCh.pos);
		if (num > 80f + grabbedCh.rad)
		{
			ReleaseGrasp(0);
			return;
		}
		if (c.Stunned || !c.Consious)
			StunCounter = -1;
		else if (StunCounter == -1)
            c.Stun(60);
        else if (StunCounter < 100)
			++StunCounter;
		else
		{
            c.Stun(60);
            room?.PlaySound(SoundID.Spear_Stick_In_Creature, fch, false, .6f, .8f);
			StunCounter = -1;
        }
		var vector = Custom.DirVec(fch.pos, grabbedCh.pos);
		float rad = grabbedCh.rad,
			num3 = grabbedCh.mass / (grabbedCh.mass + fch.mass);
		fch.pos -= vector * ((rad - num) * num3 * .95f);
		fch.vel -= vector * ((rad - num) * num3 * .95f);
		grabbedCh.pos += vector * ((rad - num) * (1f - num3) * .95f);
        grabbedCh.vel += vector * ((rad - num) * (1f - num3) * .95f);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!Consious || otherObject is not Creature cr || safariControlled)
			return;
		if (AI is CaterpillarAI ai)
		{
            ai.tracker.SeeCreature(cr.abstractCreature);
            if (otherObject is not Caterpillar)
                ai.AnnoyingCollision(cr.abstractCreature);
            if (ai.DoIWantToEatCreature(cr.abstractCreature) && myChunk == 0)
            {
                var flag = true;
                var grb = grabbedBy;
                for (var j = 0; j < grb.Count && flag; j++)
                {
                    if (grb[j].grabber == otherObject)
                        flag = false;
                }
                if (flag)
                {
                    room.PlaySound(SoundID.Centipede_Attach, bodyChunks[myChunk]);
                    Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, abstractCreature.personality.dominance, false, false);
                }
            }
        }
	}

	public override void Stun(int st)
	{
		ReleaseGrasp(0);
		st *= 2;
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos)) * 8f;
		var chs = bodyChunks;
		for (var i = 0; i < chs.Length; i++)
		{
			var chi = chs[i];
            chi.pos = newRoom.MiddleOfTile(pos) + Custom.RNV() * (Random.value * 2f);
            chi.lastPos = newRoom.MiddleOfTile(pos);
            chi.vel = vector;
		}
		chs[0].vel += vector;
        graphicsModule?.Reset();
	}

	public override Color ShortCutColor() => graphicsModule is CaterpillarGraphics gr ? (Glowing ? gr.GlowColor : Custom.HSL2RGB(gr.Hue, gr.Saturation, .4f)) : Color.white;

    public override void Die()
    {
		if (!dead)
            room?.PlaySound(NewSoundID.M4R_GenericBug_BigChip, firstChunk, false, 1f, .85f);
        base.Die();
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

	public virtual Vector2 TailPosition() => bodyChunks[bodyChunks.Length - 1].pos;

	Vector2 IProvideWarmth.Position() => TailPosition();
}