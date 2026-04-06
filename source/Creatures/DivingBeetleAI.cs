using Noise;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;

public class DivingBeetleAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
    public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
    {
        public static readonly Behavior Idle = new("Idle", true),
            Flee = new("Flee", true),
            Hunt = new("Hunt", true),
            EscapeRain = new("EscapeRain", true),
            ReturnPrey = new("ReturnPrey", true),
            GetUnstuck = new("GetUnstuck", true),
            LeaveRoom = new("LeaveRoom", true),
            Injured = new("Injured", true);
    }

    public DivingBeetle Bug;
    public float CurrentUtility;
    public AbstractCreature? TargetCreature;
    public Behavior Behav;
    public WorldCoordinate? WalkWithBug;
    public int IdlePosCounter, NoiseRectionDelay, AttackCounter;
    public WorldCoordinate TempIdlePos;

    public DivingBeetleAI(AbstractCreature creature, World world) : base(creature, world)
    {
        Bug = (creature.realizedCreature as DivingBeetle)!;
        Bug.AI = this;
        AddModule(new StandardPather(this, world, creature));
        pathFinder.stepsPerFrame = 15;
        AddModule(new Tracker(this, 10, 10, 1500, .5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new PreyTracker(this, 5, 2f, 10f, 80f, .85f));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new StuckTracker(this, true, false));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new InjuryTracker(this, .4f));
        AddModule(new DiscomfortTracker(this, tracker, 0f));
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);
        var smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, .01f);
        utilityComparer.AddComparedModule(preyTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(stuckTracker, null, 1f, 1.1f);
        smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 150f);
        utilityComparer.AddComparedModule(injuryTracker, smoother, 1f, 1.1f);
        Behav = Behavior.Idle;
    }

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost) => base.TravelPreference(coord, cost) with { resistance = !coord.destinationCoord.TileDefined || Bug?.room is not Room rm || rm.PointSubmerged(coord.destinationCoord.Tile.ToVector2() * 20f) ? cost.resistance + 1000f : cost.resistance };

    public override float CurrentPlayerAggression(AbstractCreature player) => base.CurrentPlayerAggression(player) * (1f + Bug.Submersion * .85f) * (Behav == Behavior.Hunt && preyTracker.MostAttractivePrey is Tracker.CreatureRepresentation rep && rep.representedCreature == player ? 1f : .5f);

    public override void Update()
    {
        base.Update();
        if (Bug.room is not Room rm)
            return;
        pathFinder.walkPastPointOfNoReturn = stranded || denFinder.GetDenPosition() is not WorldCoordinate w || !pathFinder.CoordinatePossibleToGetBackFrom(w) || threatTracker.Utility() > .95f;
        if (Bug.Sitting)
            noiseTracker.hearingSkill = 1f;
        else
            noiseTracker.hearingSkill = .3f;
        utilityComparer.GetUtilityTracker(preyTracker).weight = .07f + (TargetCreature is not null ? .83f : .57f) * Mathf.InverseLerp(0f, 100f, AttackCounter);
        if (AttackCounter > 0)
            --AttackCounter;
        else
            TargetCreature = null;
        var aIModule = utilityComparer.HighestUtilityModule();
        CurrentUtility = utilityComparer.HighestUtility();
        if (aIModule is not null)
        {
            if (aIModule is ThreatTracker)
                Behav = Behavior.Flee;
            else if (aIModule is PreyTracker)
                Behav = Behavior.Hunt;
            else if (aIModule is RainTracker)
                Behav = Behavior.EscapeRain;
            else if (aIModule is StuckTracker)
                Behav = Behavior.GetUnstuck;
            else if (aIModule is InjuryTracker)
                Behav = Behavior.Injured;
        }
        var crits = rm.abstractRoom.creatures;
        var trk = tracker;
        for (var i = 0; i < crits.Count; i++)
        {
            if (crits[i] is AbstractCreature cr && cr.SameRippleLayer(creature) && cr.realizedCreature?.grasps is Creature.Grasp[] ar)
            {
                for (var j = 0; j < ar.Length; j++)
                {
                    if (ar[j]?.grabbed is BubbleGrass b && b.Submersion > .1f)
                    {
                        trk.SeeCreature(cr);
                        if (trk.creatures.Count > 0)
                            preyTracker.AddPrey(trk.creatures[trk.creatures.Count - 1]);
                        if (Behav == Behavior.Hunt)
                            CurrentUtility = 1f;
                    }
                }
            }
        }
        if (CurrentUtility < .05f)
            Behav = Behavior.Idle;
        if (Behav != Behavior.Flee && Bug.grasps[0] is Creature.Grasp g && g.grabbed is Creature c && DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
        {
            Behav = Behavior.ReturnPrey;
            CurrentUtility = 1f;
        }
        if (Behav != Behavior.Idle)
            TempIdlePos = creature.pos;
        if (Behav == Behavior.Injured && preyTracker.Utility() > .4f && preyTracker.MostAttractivePrey is Tracker.CreatureRepresentation rep && creature.pos.room == rep.BestGuessForPosition().room && creature.pos.Tile.FloatDist(rep.BestGuessForPosition().Tile) < 6f)
        {
            Behav = Behavior.Hunt;
            utilityComparer.GetUtilityTracker(preyTracker).weight = 1f;
        }
        if (Behav == Behavior.GetUnstuck)
        {
            if (Random.value < .02f)
                creature.abstractAI.SetDestination(rm.GetWorldCoordinate(Bug.bodyChunks[0].pos + Custom.RNV() * 100f));
        }
        else if (Behav == Behavior.LeaveRoom)
        {
            creature.abstractAI.AbstractBehavior(1);
            if (pathFinder.GetDestination != creature.abstractAI.MigrationDestination)
                creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
        }
        else if (Behav == Behavior.Idle)
        {
            if (Random.value < .01f)
                creature.abstractAI.AbstractBehavior(1);
            if (!pathFinder.CoordinateReachableAndGetbackable(pathFinder.GetDestination))
                creature.abstractAI.SetDestination(creature.pos);
            if (WalkWithBug.HasValue)
            {
                creature.abstractAI.SetDestination(WalkWithBug.Value);
                if (Random.value < .02f || Custom.ManhattanDistance(creature.pos, WalkWithBug.Value) < 4)
                    WalkWithBug = null;
            }
            else if (!creature.abstractAI.WantToMigrate)
            {
                var coord = new WorldCoordinate(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1);
                if (IdleScore(coord) < IdleScore(TempIdlePos))
                    TempIdlePos = coord;
                if (IdleScore(TempIdlePos) < IdleScore(pathFinder.GetDestination) + Custom.LerpMap(IdlePosCounter, 0f, 300f, 100f, -300f))
                {
                    SetDestination(TempIdlePos);
                    IdlePosCounter = Random.Range(200, 800);
                    TempIdlePos = new(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1);
                }
                --IdlePosCounter;
            }
            else if (pathFinder.GetDestination != creature.abstractAI.MigrationDestination)
                creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
        }
        else if (Behav == Behavior.Hunt)
        {
            if (preyTracker.MostAttractivePrey is Tracker.CreatureRepresentation repr)
                creature.abstractAI.SetDestination(repr.BestGuessForPosition());
        }
        else if (Behav == Behavior.Flee)
            creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 6, 30, true));
        else if (Behav == Behavior.EscapeRain || Behav == Behavior.ReturnPrey || Behav == Behavior.Injured)
        {
            if (denFinder.GetDenPosition() is WorldCoordinate wc)
                creature.abstractAI.SetDestination(wc);
        }
        if (NoiseRectionDelay > 0)
            --NoiseRectionDelay;
    }

    public virtual float IdleScore(WorldCoordinate coord)
    {
        if (coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord))
            return float.MaxValue;
        var num = 1f;
        var rm = Bug.room;
        var aiTile = rm.aimap.getAItile(coord);
        if (aiTile.narrowSpace)
            num += 300f;
        var flag = coord.TileDefined && rm.PointSubmerged(coord.Tile.ToVector2() * 20f);
        if (flag)
            num += 1000f;
        if (aiTile.acc == AItile.Accessibility.Solid)
            num -= 100f;
        if (rm.GetTile(coord + new IntVector2(0, 1)).Solid)
            num -= 300f;
        num += Math.Max(0f, creature.pos.Tile.FloatDist(coord.Tile) - 80f) / 2f;
        num += aiTile.visibility / 800f;
        num -= Math.Max(aiTile.smoothedFloorAltitude, 16f) * 2f;
        if (!flag)
            num *= .1f;
        if (discomfortTracker is DiscomfortTracker trk)
            num -= 1000f * trk.DiscomfortOfTile(coord);
        return num;
    }

    public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

    public virtual void CollideWithKin(DivingBeetle otherBug)
    {
        if (Custom.ManhattanDistance(creature.pos, otherBug.AI.pathFinder.GetDestination) >= 4)
        {
            if (otherBug.abstractCreature.personality.dominance > Bug.abstractCreature.personality.dominance && !otherBug.Sitting || Bug.Sitting || otherBug.AI.pathFinder.GetDestination.room != otherBug.room.abstractRoom.index)
                WalkWithBug = otherBug.AI.pathFinder.GetDestination;
        }
    }

    public override float VisualScore(Vector2 lookAtPoint, float bonus)
    {
        var bs = Bug.bodyChunks;
        return base.VisualScore(lookAtPoint, bonus) - Mathf.Pow(Mathf.InverseLerp(.4f, -.6f, Vector2.Dot((bs[1].pos - bs[0].pos).normalized, (bs[0].pos - lookAtPoint).normalized)), 2f);
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
        if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
            return preyTracker;
        return null;
    }

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature)
            return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        if (dRelation.trackerRep.VisualContact)
            dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
        var result = StaticRelationship(dRelation.trackerRep.representedCreature);
        if (result.type != CreatureTemplate.Relationship.Type.Eats && dRelation.trackerRep.representedCreature.realizedCreature is Creature c && c.dead && c.TotalMass < Bug.TotalMass * 1.15f)
            result = new(CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(Bug.TotalMass * .3f, Bug.TotalMass * 1.15f, c.TotalMass) * .5f);
        if (result.type == CreatureTemplate.Relationship.Type.Eats)
        {
            result.intensity *= Mathf.InverseLerp(.1f, 1.5f, dRelation.trackerRep.representedCreature.creatureTemplate.bodySize);
            if (TargetCreature is not null)
                result.intensity = Mathf.Pow(result.intensity, dRelation.trackerRep.representedCreature == TargetCreature ? .1f : 3f);
            if (dRelation.trackerRep.representedCreature.realizedCreature is Creature cre && cre.abstractPhysicalObject.SameRippleLayer(creature) && cre.grasps is Creature.Grasp[] ar)
            {
                for (var i = 0; i < ar.Length; i++)
                {
                    if (ar[i]?.grabbed is BubbleGrass)
                    {
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
            result.intensity = 0f;
        if (dRelation.trackerRep.representedCreature.realizedCreature is Creature cr && cr.abstractPhysicalObject.SameRippleLayer(creature))
        {
            if (cr.grasps is Creature.Grasp[] grs)
            {
                for (var i = 0; i < grs.Length; i++)
                {
                    if (grs[i]?.grabbed is LimeMushroom)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Afraid;
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        return result;
    }

    public virtual void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        if (!Bug.Sitting || NoiseRectionDelay > 0 || Random.value < 1f)
            return;
        var bs = Bug.bodyChunks;
        bs[1].pos += Custom.RNV();
        if (Bug.graphicsModule is DivingBeetleGraphics g)
        {
            for (var i = 0; i < 2; i++)
                g.Antennae[i].pos += Custom.DirVec(bs[0].pos, noise.pos) * (Random.value * 20f);
        }
        NoiseRectionDelay = Random.Range(0, 30);
    }
}