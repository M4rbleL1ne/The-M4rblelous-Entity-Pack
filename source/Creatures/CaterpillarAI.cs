using RWCustom;
using UnityEngine;
using Watcher;

namespace LBMergedMods.Creatures;

public class CaterpillarAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
	{
		public static readonly Behavior Idle = new(nameof(Idle), true),
			Flee = new(nameof(Flee), true),
			Hunt = new(nameof(Hunt), true),
            ReturnPrey = new(nameof(ReturnPrey), true),
            EscapeRain = new(nameof(EscapeRain), true),
			Injured = new(nameof(Injured), true),
            GetUnstuck = new(nameof(GetUnstuck), true),
            InvestigateSound = new(nameof(InvestigateSound), true);
    }

    public class CaterpillarTrackState : RelationshipTracker.TrackedCreatureState
	{
		public int AnnoyingCollisions;
	}

    public Behavior Behav = Behavior.Idle;
    public Caterpillar Crit;
	public int AnnoyingCollisions, IdleCounter;
    public float CurrentUtility, Excitement, Run;
	public WorldCoordinate ForbiddenIdlePos, TempIdlePos;
	public WorldCoordinate? FleeRoom;

	public CaterpillarAI(AbstractCreature creature, World world) : base(creature, world)
	{
		Crit = (creature.realizedCreature as Caterpillar)!;
		Crit.AI = this;
		AddModule(new StandardPather(this, world, creature) { accessibilityStepsPerFrame = 40 });
		AddModule(new Tracker(this, 10, 10, -1, .5f, 5, 5, 20));
		AddModule(new NoiseTracker(this, tracker));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, .05f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new InjuryTracker(this, .6f));
		AddModule(new StuckTracker(this, true, true));
        AddModule(new DiscomfortTracker(this, tracker, 0f));
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);
		utilityComparer.AddComparedModule(preyTracker, null, .9f, 1.1f);
		utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
		utilityComparer.AddComparedModule(injuryTracker, null, .7f, 1.1f);
		utilityComparer.AddComparedModule(noiseTracker, null, .2f, 1.2f);
        utilityComparer.AddComparedModule(stuckTracker, null, .4f, 1.1f);
		stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(stuckTracker));
    }

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		ForbiddenIdlePos = creature.pos;
		TempIdlePos = creature.pos;
	}

	public override void Update()
	{
		base.Update();
		if (Crit.LickedByPlayer?.abstractCreature is AbstractCreature crit)
		{
			tracker.SeeCreature(crit);
			AnnoyingCollision(crit);
		}
		if (AnnoyingCollisions > 0)
            --AnnoyingCollisions;
		noiseTracker.hearingSkill = Crit.Moving ? 1f : 1.5f;
		if (preyTracker.MostAttractivePrey is Tracker.CreatureRepresentation rep)
			utilityComparer.GetUtilityTracker(preyTracker).weight = Mathf.InverseLerp(50f, 10f, rep.TicksSinceSeen);
		if (threatTracker.mostThreateningCreature is Tracker.CreatureRepresentation repTh)
			utilityComparer.GetUtilityTracker(threatTracker).weight = Mathf.InverseLerp(500f, 100f, repTh.TicksSinceSeen);
		if (Crit.grasps[0]?.grabbed is Creature c && DoIWantToEatCreature(c.abstractCreature))
			Behav = Behavior.ReturnPrey;
		else
		{
            CurrentUtility = utilityComparer.HighestUtility();
            if (CurrentUtility < .1f)
                Behav = Behavior.Idle;
			else if (utilityComparer.HighestUtilityModule() is AIModule aIModule)
            {
                if (aIModule is ThreatTracker)
                    Behav = Behavior.Flee;
                else if (aIModule is RainTracker)
                    Behav = Behavior.EscapeRain;
                else if (aIModule is PreyTracker)
                    Behav = Behavior.Hunt;
                else if (aIModule is NoiseTracker)
                    Behav = Behavior.InvestigateSound;
                else if (aIModule is InjuryTracker)
                    Behav = Behavior.Injured;
                else if (aIModule is StuckTracker)
                    Behav = Behavior.GetUnstuck;
            }
        }
        var b = .5f;
        if (Behav == Behavior.Idle)
		{
			var testPos = Random.value < .01f ? new(creature.pos.room, Random.Range(0, Crit.room.TileWidth), Random.Range(0, Crit.room.TileHeight), -1) : creature.pos + new IntVector2(Random.Range(-10, 11), Random.Range(-10, 11));
			if (IdleScore(testPos) > IdleScore(TempIdlePos))
			{
				TempIdlePos = testPos;
				IdleCounter = 0;
			}
			else
			{
                ++IdleCounter;
				if (IdleCounter > 1400)
				{
					IdleCounter = 0;
					ForbiddenIdlePos = TempIdlePos;
				}
			}
			var dest = pathFinder.GetDestination;
            if (TempIdlePos != dest && IdleScore(TempIdlePos) > IdleScore(dest) + 100f)
				creature.abstractAI.SetDestination(TempIdlePos);
		}
		else if (Behav == Behavior.Flee)
		{
			b = 1f;
			creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 1, 30, CurrentUtility > .3f));
		}
        else if (Behav == Behavior.GetUnstuck)
        {
            b = .8f;
            creature.abstractAI.SetDestination(stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
        }
        else if (Behav == Behavior.EscapeRain || Behav == Behavior.ReturnPrey)
		{
			b = .9f;
			if (denFinder.GetDenPosition() is WorldCoordinate coord)
				creature.abstractAI.SetDestination(coord);
		}
		else if (Behav == Behavior.Injured)
		{
			b = 1f;
			if (denFinder.GetDenPosition() is WorldCoordinate coord)
				creature.abstractAI.SetDestination(coord);
		}
        else if (Behav == Behavior.Hunt)
		{
			var prey = preyTracker.MostAttractivePrey;
			b = DynamicRelationship(prey).intensity * .5f + .4f;
			creature.abstractAI.SetDestination(prey.BestGuessForPosition());
		}
		else if (Behav == Behavior.InvestigateSound)
		{
			b = .6f;
			creature.abstractAI.SetDestination(noiseTracker.ExaminePos);
		}
		if (Crit.safariControlled)
			b = 1f;
		else if (creature.world is World w && !w.singleRoomWorld && w.region is Region r && w.GetAbstractRoom(creature.pos.room) is AbstractRoom arm)
		{
			var attract = arm.AttractionValueForCreature(creature);
			if (attract <= .25f)
			{
				Crit.Moving = true;
				b = 1f - attract;
				preyTracker.ForgetAllPrey();
				if (!FleeRoom.HasValue)
				{
					var numb = r.firstRoomIndex + r.numberOfRooms;
					for (var i = r.firstRoomIndex; i < numb; i++)
					{
						if (w.GetAbstractRoom(i) is AbstractRoom rm && rm.AttractionValueForCreature(creature) > .25f)
						{
							var nd = rm.RandomRelevantNode(creature.creatureTemplate);
							if (nd >= 0)
							{
								var coord = new WorldCoordinate(rm.index, -1, -1, nd);
								FleeRoom = coord;
								SetDestination(coord);
								break;
							}
						}
					}
				}
				else
					SetDestination(FleeRoom.Value);
			}
			else
				FleeRoom = null;
        }
		Excitement = Mathf.Lerp(Excitement, b, .1f);
		Run -= .25f;
        if (Run < Mathf.Lerp(-10f, -2f, Excitement))
            Run = Mathf.Lerp(30f, 50f, Excitement);
        var num = 0;
		var num2 = 0f;
		var cnt = tracker.CreaturesCount;
        for (var i = 0; i < cnt; i++)
		{
			if (tracker.GetRep(i).representedCreature.realizedCreature is Caterpillar ctp && ctp.abstractPhysicalObject.SameRippleLayer(creature) && ctp.abstractCreature.Room == creature.Room && ctp.AI is CaterpillarAI ai && ai.Run > 0f == Run > 0f)
			{
				num2 += ai.Run;
				++num;
			}
		}
		if (num > 0)
			Run = Mathf.Lerp(Run, num2 / num, .1f);
	}

	public virtual float IdleScore(WorldCoordinate testPos)
	{
		if (!testPos.TileDefined || testPos.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(testPos))
			return float.MinValue;
		var num = 1000f;
		num /= Mathf.Max(1f, Crit.room.aimap.getTerrainProximity(testPos) - 1f);
		num -= Custom.LerpMap(testPos.Tile.FloatDist(ForbiddenIdlePos.Tile), 0f, 10f, 1000f, 0f);
		if (Crit.room.aimap.getAItile(testPos).fallRiskTile.y < 0)
			num -= Custom.LerpMap(testPos.y, 10f, 30f, 1000f, 0f);
        if (discomfortTracker is DiscomfortTracker trk)
            num -= 1000f * trk.DiscomfortOfTile(testPos);
        return num;
	}

	public virtual void AnnoyingCollision(AbstractCreature critter)
	{
		if (!critter.state.dead)
		{
			AnnoyingCollisions += 10;
			if (AnnoyingCollisions >= 150 && tracker.RepresentationForCreature(critter, false)?.dynamicRelationship.state is CaterpillarTrackState st)
                ++st.AnnoyingCollisions;
		}
	}

	public virtual bool DoIWantToEatCreature(AbstractCreature critter)
	{
		if (!critter.SameRippleLayer(creature) || !critter.NoCamo())
			return false;
		if (Crit.safariControlled)
			return StaticRelationship(critter).type == CreatureTemplate.Relationship.Type.Eats;
		if (AnnoyingCollisions < 150 && (Behav == Behavior.Flee || Behav == Behavior.EscapeRain) && CurrentUtility > .1f)
			return false;
		if (StaticRelationship(critter).type == CreatureTemplate.Relationship.Type.Eats)
		{
            if (critter.realizedCreature is Creature c)
            {
                var creatureRepresentation = tracker.RepresentationForObject(c, false);
                if (AnnoyingCollisions > 150 && (creatureRepresentation?.dynamicRelationship.state is not CaterpillarTrackState st || st.AnnoyingCollisions > (int)Crit.State.health))
                    return true;
                if (creatureRepresentation is not null)
                    return creatureRepresentation.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats;
            }
			return true;
        }
		return false;
	}

	public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

	public override float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		Vector2 pos = Crit.bodyChunks[1].pos,
			pos2 = Crit.firstChunk.pos;
        return base.VisualScore(lookAtPoint, bonus) - Mathf.InverseLerp(1f, .7f, Vector2.Dot((pos - pos2).normalized, (pos2 - lookAtPoint).normalized)) - (Crit.Moving ? .25f : 0f);
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
			return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false);
		return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
	}

	public virtual AIModule? ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
			return threatTracker;
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
			return preyTracker;
		return null;
	}

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new CaterpillarTrackState();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
        if (ModManager.Watcher && dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
            return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        var result = StaticRelationship(dRelation.trackerRep.representedCreature);
        if (result.type == CreatureTemplate.Relationship.Type.Ignores)
			return result;
		if (dRelation.trackerRep.representedCreature.realizedCreature is Creature c)
		{
			var totMass = Crit.TotalMass;
			var cMass = c.TotalMass;
            if (result.type == CreatureTemplate.Relationship.Type.Eats && cMass < totMass)
				result.intensity *= Mathf.InverseLerp(0f, totMass, cMass);
			else
			{
				result.type = CreatureTemplate.Relationship.Type.Afraid;
				result.intensity = .2f + .8f * Mathf.InverseLerp(totMass, totMass * 1.5f, cMass);
            }
            var grs = c.grasps;
			var glowing = Crit.Glowing;
            if (grs is not null && c.abstractPhysicalObject.SameRippleLayer(creature))
            {
                for (var i = 0; i < grs.Length; i++)
                {
					if (grs[i] is Creature.Grasp gr)
					{
                        if (gr.grabbed is LimeMushroom)
                        {
                            result.type = CreatureTemplate.Relationship.Type.Afraid;
                            result.intensity = 1f;
                            break;
                        }
                        else if (glowing && gr.grabbed is StarLemon)
                        {
                            result.type = CreatureTemplate.Relationship.Type.Eats;
                            result.intensity = 1f;
                            break;
                        }
                    }
                }
            }
        }
		return result;
	}
}