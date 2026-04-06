using Noise;
using RWCustom;
using UnityEngine;
using Watcher;

namespace LBMergedMods.Creatures;

public class ThornBugAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
    public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
    {
        public static readonly Behavior Idle = new("Idle", true),
            Flee = new("Flee", true),
            Hunt = new("Hunt", true),
            EscapeRain = new("EscapeRain", true),
            ReturnPrey = new("ReturnPrey", true);
    }

    public ThornBug Bug;
    public float CurrentUtility;
    public float Fear;
    public int NoiseRectionDelay, IdlePosCounter;
    public Behavior Behav;
    public WorldCoordinate? WalkWithBug;
    public WorldCoordinate TempIdlePos;

    public ThornBugAI(AbstractCreature creature, World world) : base(creature, world)
    {
        Bug = (creature.realizedCreature as ThornBug)!;
        Bug.AI = this;
        AddModule(new StandardPather(this, world, creature) { stepsPerFrame = 50 });
        AddModule(new Tracker(this, 10, 10, 150, .5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new PreyTracker(this, 5, .9f, 3f, 70f, .5f));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new DiscomfortTracker(this, tracker, 0f));
        FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, .5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, .005f));
        utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, .15f);
        utilityComparer.AddComparedModule(preyTracker, smoother, .6f, 1f);
        Behav = Behavior.Idle;
    }

    public override void Update()
    {
        base.Update();
        if (Bug.room is not Room rm)
            return;
        if (ModManager.MSC && Bug.LickedByPlayer is Player p)
            tracker.SeeCreature(p.abstractCreature);
        pathFinder.walkPastPointOfNoReturn = stranded || denFinder.GetDenPosition() is not WorldCoordinate w || !pathFinder.CoordinatePossibleToGetBackFrom(w) || threatTracker.Utility() > .95f;
        if (Bug.Sitting)
            noiseTracker.hearingSkill = 2f;
        else
            noiseTracker.hearingSkill = .2f;
        utilityComparer.GetUtilityTracker(threatTracker).weight = Custom.LerpMap(threatTracker.ThreatOfTile(creature.pos, true), .1f, 2f, .1f, 1f, .5f);
        var aIModule = utilityComparer.HighestUtilityModule();
        CurrentUtility = utilityComparer.HighestUtility();
        if (aIModule is not null)
        {
            if (aIModule is ThreatTracker)
                Behav = Behavior.Flee;
            else if (aIModule is RainTracker)
                Behav = Behavior.EscapeRain;
            else if (aIModule is PreyTracker && preyTracker.MostAttractivePrey?.representedCreature.realizedCreature is Creature c && !c.dead && Bug.grasps[0] is null)
                Behav = Behavior.Hunt;
        }
        if (CurrentUtility < .02f)
            Behav = Behavior.Idle;
        if (Bug.grasps[0]?.grabbed is Creature)
        {
            Behav = Behavior.ReturnPrey;
            CurrentUtility = 1f;
        }
        if (Behav == Behavior.Idle)
        {
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, .5f + .5f * Mathf.Max(threatTracker.Utility(), Fear), .01f, 1f / 60f) * .88f;
            if (WalkWithBug.HasValue)
            {
                creature.abstractAI.SetDestination(WalkWithBug.Value);
                if (Random.value < .02f || Custom.ManhattanDistance(creature.pos, WalkWithBug.Value) < 4)
                    WalkWithBug = null;
            }
            else
            {
                var flag = pathFinder.GetDestination.room != rm.abstractRoom.index;
                if (!flag && IdlePosCounter <= 0)
                {
                    var abstractNode = rm.abstractRoom.RandomNodeInRoom().abstractNode;
                    if (rm.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
                    {
                        var num2 = rm.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, Bug.Template);
                        if (num2 > -1)
                        {
                            var num3 = rm.aimap.ExitDistanceForCreatureAndCheckNeighbours(Bug.abstractCreature.pos.Tile, num2, Bug.Template);
                            if (num3 > -1 && num3 < 400)
                            {
                                if (rm.game.world.GetAbstractRoom(rm.abstractRoom.connections[abstractNode]) is AbstractRoom abstractRoom)
                                {
                                    var worldCoordinate = abstractRoom.RandomNodeInRoom();
                                    if (pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                                    {
                                        creature.abstractAI.SetDestination(worldCoordinate);
                                        IdlePosCounter = Random.Range(200, 500);
                                        flag = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!flag)
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
                }
                --IdlePosCounter;
            }
        }
        else if (Behav == Behavior.Flee)
        {
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, 1f, .01f, .1f) * .8f;
            creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 10, 30, true));
            if (Random.value < threatTracker.Panic && threatTracker.mostThreateningCreature?.representedCreature.realizedCreature is Creature cr && cr.room == rm)
            {
                var ch = cr.bodyChunks[Random.Range(0, cr.bodyChunks.Length)];
                if (!Bug.safariControlled && Custom.DistLess(Bug.mainBodyChunk.pos, ch.pos, Bug.mainBodyChunk.rad + ch.rad + 40f * Fear))
                    Bug.TryJump(ch.pos, true);
            }
        }
        else if (Behav == Behavior.EscapeRain || Behav == Behavior.ReturnPrey)
        {
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, 1f, .01f, .1f) * .86f;
            if (denFinder.GetDenPosition() is WorldCoordinate wc)
                creature.abstractAI.SetDestination(wc);
        }
        else if (Behav == Behavior.Hunt)
        {
            if (preyTracker.MostAttractivePrey is Tracker.CreatureRepresentation prey)
                AggressiveBehav(prey);
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, 1f, .025f, .1f) * .8f;
        }
        if (Behav == Behavior.ReturnPrey && creature.remainInDenCounter < 30 && !creature.InDen)
            creature.remainInDenCounter = 30;
        Fear = Custom.LerpAndTick(Fear, Mathf.Max(utilityComparer.GetUtilityTracker(threatTracker).SmoothedUtility(), Mathf.Pow(threatTracker.Panic, .7f)), .07f, 1f / 30f);
        if (Bug.safariControlled)
        {
            Bug.RunSpeed = .8f;
            Fear = 1f;
        }
        if (NoiseRectionDelay > 0)
            --NoiseRectionDelay;
    }

    public virtual float IdleScore(WorldCoordinate coord)
    {
        if (coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord) || Bug.room.aimap.getAItile(coord).acc >= AItile.Accessibility.Wall)
            return float.MaxValue;
        var num = 1f;
        if (pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)))
            num += 10f;
        if (Bug.room.aimap.getAItile(coord).narrowSpace)
            num += 50f;
        num += threatTracker.ThreatOfTile(coord, true) * 1000f;
        num += threatTracker.ThreatOfTile(Bug.room.GetWorldCoordinate((Bug.room.MiddleOfTile(coord) + Bug.room.MiddleOfTile(creature.pos)) / 2f), true) * 1000f;
        var sources = noiseTracker.sources;
        for (var i = 0; i < sources.Count; i++)
            num += Custom.LerpMap(Vector2.Distance(Bug.room.MiddleOfTile(coord), sources[i].pos), 40f, 400f, 100f, 0f);
        if (discomfortTracker is DiscomfortTracker trk)
            num -= 1000f * trk.DiscomfortOfTile(coord);
        return num;
    }

    public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

    public virtual void CollideWithKin(ThornBug otherBug)
    {
        if (Custom.ManhattanDistance(creature.pos, otherBug.AI.pathFinder.GetDestination) >= 4 && (otherBug.abstractCreature.personality.dominance > Bug.abstractCreature.personality.dominance && !otherBug.Sitting || Bug.Sitting || otherBug.AI.pathFinder.GetDestination.room != otherBug.room.abstractRoom.index))
            WalkWithBug = otherBug.AI.pathFinder.GetDestination;
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.smallCreature)
            return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false);
        return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
    }

    public virtual AIModule? ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
            return preyTracker;
        if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            return threatTracker;
        return null;
    }

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.trackerRep.VisualContact)
            dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
        var result = StaticRelationship(dRelation.trackerRep.representedCreature);
        if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
            result.intensity = 0f;
        if (ModManager.Watcher)
        {
            if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
                return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog && dRelation.trackerRep.representedCreature.state is FrogState { creatureAttachedTo: var creatureAttachedTo } frogState && (creatureAttachedTo == creature.ID || frogState.bloodBank))
                return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        }
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c)
        {
            var grs = c.grasps;
            if (grs is not null)
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

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        cost = base.TravelPreference(coord, cost);
        return new(cost.resistance + Custom.LerpMap(Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, false) - threatTracker.ThreatOfTile(creature.pos, false)), 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
    }

    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
    {
        if (!firstSpot && Random.value > Fear)
            return;
        var relationship = DynamicRelationship(otherCreature);
        if (relationship.type != CreatureTemplate.Relationship.Type.Ignores)
        {
            var guessPos = Bug.room.MiddleOfTile(otherCreature.BestGuessForPosition());
            if (!Bug.safariControlled && firstSpot && relationship.type == CreatureTemplate.Relationship.Type.Afraid && relationship.intensity > .06f && Custom.DistLess(Bug.DangerPos, guessPos, Custom.LerpMap(relationship.intensity, .06f, .5f, 50f, 300f)))
                Bug.TryJump(guessPos, true);
            if (relationship.intensity > (firstSpot ? .02f : .1f))
                Bug.Suprise(guessPos);
        }
    }

    public virtual void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        Bug.AntennaDir = noise.pos;
        Bug.AntennaAttention = 1f;
        if (NoiseRectionDelay <= 0)
        {
            if (!Bug.safariControlled && noise.strength > 160f && Custom.DistLess(noise.pos, Bug.mainBodyChunk.pos, Mathf.Lerp(Bug.Sitting ? 300f : 100f, 600f, Fear)))
                Bug.TryJump(noise.pos, true);
            Bug.Suprise(noise.pos);
            NoiseRectionDelay = Random.Range(0, 30);
        }
    }

    public virtual bool UnpleasantFallRisk(IntVector2 tile)
    {
        var tl = Bug.room.aimap.getAItile(tile).fallRiskTile;
        if (!Bug.room.GetTile(tl).AnyWater && tl.y >= 0 && tl.y >= tile.y - 20)
        {
            if (tl.y < tile.y - 10)
                return !pathFinder.CoordinatePossibleToGetBackFrom(Bug.room.GetWorldCoordinate(tl));
            return false;
        }
        return true;
    }

    public virtual void AggressiveBehav(Tracker.CreatureRepresentation target)
    {
        if (Bug.safariControlled)
            return;
        var destination = target.BestGuessForPosition();
        creature.abstractAI.SetDestination(destination);
        if (target.representedCreature.realizedCreature is Creature cr && cr.room is Room crRm && crRm.abstractRoom.index == Bug.room.abstractRoom.index && !cr.inShortcut)
        {
            var mPos = Bug.mainBodyChunk.pos;
            var bugTl = Bug.room.aimap.getAItile(creature.pos);
            var targetTl = target.representedCreature.pos.Tile;
            if (target.VisualContact && Random.value < .1f && !UnpleasantFallRisk(creature.pos.Tile) && !UnpleasantFallRisk(targetTl) && Custom.DistLess(mPos, cr.mainBodyChunk.pos, 200f) && !Bug.room.aimap.getAItile(targetTl).narrowSpace && !bugTl.narrowSpace && Bug.room.GetTile(mPos + new Vector2(0f, -20f)).Solid && Bug.Footing)
                Bug.TryJump(new Vector2(destination.x, destination.y), false);
            var bs = cr.bodyChunks;
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                if (Custom.DistLess(b.pos, mPos, 15f) && Custom.DistLess(mPos + Custom.DirVec(Bug.bodyChunks[1].pos, mPos) * 20f, b.pos, b.rad + 25f) && Bug.Grab(cr, 0, i, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, Random.value, true, true))
                    break;
            }
        }
    }
}