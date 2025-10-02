using Noise;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class TintedBeetleAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction, FriendTracker.IHaveFriendTracker, IReactToSocialEvents
{
    public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
    {
        public static readonly Behavior Idle = new("Idle", true),
            Flee = new("Flee", true),
            Hunt = new("Hunt", true),
            EscapeRain = new("EscapeRain", true),
            ReturnPrey = new("ReturnPrey", true),
            FollowFriend = new("FollowFriend", true);
    }

    public TintedBeetle Bug;
    public float CurrentUtility, Fear;
    public int NoiseRectionDelay, IdlePosCounter;
    public Behavior Behav;
    public WorldCoordinate? WalkWithBug;
    public WorldCoordinate TempIdlePos;
    public FoodItemTracker FoodTracker;

    public TintedBeetleAI(AbstractCreature creature, World world) : base(creature, world)
    {
        Bug = (creature.realizedCreature as TintedBeetle)!;
        Bug.AI = this;
        AddModule(new StandardPather(this, world, creature) { stepsPerFrame = 50 });
        AddModule(new Tracker(this, 10, 10, 150, .5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(FoodTracker = new FoodItemTracker(this, 5, .9f, 3f, 70f));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new FriendTracker(this)
        {
            tamingDifficlty = Mathf.Lerp(.6f, .65f, creature.personality.dominance),
            desiredCloseness = 4f
        });
        FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, .5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, .005f));
        utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, .15f);
        utilityComparer.AddComparedModule(FoodTracker, smoother, .6f, 1f);
        utilityComparer.AddComparedModule(friendTracker, null, 1.01f, 1.2f);
        Behav = Behavior.Idle;
    }

    public override void Update()
    {
        base.Update();
        if (Bug.room is not Room rm)
            return;
        if (ModManager.MSC && Bug.LickedByPlayer is Player p)
            tracker.SeeCreature(p.abstractCreature);
        if (rm.abstractRoom.entities is List<AbstractWorldEntity> l)
        {
            for (var i = 0; i < l.Count; i++)
            {
                if (l[i] is AbstractPhysicalObject obj && obj.SameRippleLayer(creature) && obj.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant && obj.realizedObject is not null && !obj.InDen)
                    FoodTracker.AddItem(this.CreateTrackerRepresentationForItem(obj));
            }
        }
        var itms = FoodTracker.Items;
        for (var i = 0; i < itms.Count; i++)
        {
            if (itms[i]?.ItemRep?.RepresentedItem is AbstractPhysicalObject itm && itm.InDen)
                FoodTracker.ForgetItem(itm);
        }
        pathFinder.walkPastPointOfNoReturn = stranded || denFinder.GetDenPosition() is not WorldCoordinate w || !pathFinder.CoordinatePossibleToGetBackFrom(w) || threatTracker.Utility() > .95f;
        if (Bug.Sitting)
            noiseTracker.hearingSkill = 2f;
        else
            noiseTracker.hearingSkill = .2f;
        utilityComparer.GetUtilityTracker(threatTracker).weight = friendTracker.friend is not null ? .35f : Custom.LerpMap(threatTracker.ThreatOfTile(creature.pos, true), .1f, 2f, .1f, 1f, .5f);
        utilityComparer.GetUtilityTracker(rainTracker).weight = friendTracker.CareAboutRain() ? 1f : .1f;
        var aIModule = utilityComparer.HighestUtilityModule();
        CurrentUtility = utilityComparer.HighestUtility();
        if (aIModule is not null)
        {
            if (aIModule is ThreatTracker)
                Behav = Behavior.Flee;
            else if (aIModule is RainTracker)
                Behav = Behavior.EscapeRain;
            else if (aIModule is FoodItemTracker && FoodTracker.MostAttractiveItem?.RepresentedItem.realizedObject is FirecrackerPlant fp && fp.grabbedBy?.Count is 0 or null && Bug.grasps[0] is null)
                Behav = Behavior.Hunt;
            else if (aIModule is FriendTracker)
                Behav = Behavior.FollowFriend;
        }
        if (CurrentUtility < .02f)
            Behav = Behavior.Idle;
        if (Bug.grasps[0]?.grabbed is FirecrackerPlant)
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
                if (!flag && IdlePosCounter <= 0 && Random.value >= .1f)
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
        }
        else if (Behav == Behavior.EscapeRain || Behav == Behavior.ReturnPrey)
        {
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, 1f, .01f, .1f) * .86f;
            if (denFinder.GetDenPosition() is WorldCoordinate wc)
                creature.abstractAI.SetDestination(wc);
        }
        else if (Behav == Behavior.FollowFriend)
        {
            creature.abstractAI.SetDestination(friendTracker.friendDest);
            Bug.RunSpeed = Mathf.Lerp(Bug.RunSpeed, friendTracker.RunSpeed(), .7f);
        }
        else if (Behav == Behavior.Hunt)
        {
            if (FoodTracker.MostAttractiveItem is FoodItemRepresentation prey)
                AggressiveBehav(prey);
            Bug.RunSpeed = Custom.LerpAndTick(Bug.RunSpeed, 1f, .025f, .1f) * .8f;
        }
        if (Behav == Behavior.ReturnPrey && creature.remainInDenCounter < 30 && !creature.InDen)
            creature.remainInDenCounter = 30;
        Fear = Custom.LerpAndTick(Fear, Mathf.Max(utilityComparer.GetUtilityTracker(threatTracker).SmoothedUtility(), Mathf.Pow(threatTracker.Panic, .7f)), .07f, 1f / 30f);
        if (Bug.safariControlled)
        {
            Bug.RunSpeed = .6f;
            Fear = 1f;
        }
        if (NoiseRectionDelay > 0)
            --NoiseRectionDelay;
    }

    public virtual float LikeOfPlayer(Tracker.CreatureRepresentation player)
    {
        if (player is null)
            return 0f;
        var a = creature.world.game.session.creatureCommunities.LikeOfPlayer(CommunityID.TintedBeetles, creature.world.RegionNumber, (player.representedCreature.state as PlayerState)!.playerNumber);
        var tempLike = creature.state.socialMemory.GetTempLike(player.representedCreature.ID);
        a = Mathf.Lerp(a, tempLike, Math.Abs(tempLike));
        if (friendTracker.giftOfferedToMe?.owner == player.representedCreature.realizedCreature)
            a = Custom.LerpMap(a, -.5f, 1f, 0f, 1f, .8f);
        return a;
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
        return num;
    }

    public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

    public virtual void CollideWithKin(TintedBeetle otherBug)
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
        if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            return threatTracker;
        return null;
    }

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        var rep = dRelation.trackerRep.representedCreature;
        if (dRelation.trackerRep.VisualContact)
            dRelation.state.alive = rep.state.alive;
        var result = StaticRelationship(rep);
        if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
            result.intensity = 0f;
        if (rep.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
        {
            result.intensity = 1f - LikeOfPlayer(dRelation.trackerRep);
            if (result.type != CreatureTemplate.Relationship.Type.Pack && result.intensity <= .2f)
            {
                result.intensity = 1f;
                result.type = CreatureTemplate.Relationship.Type.Pack;
            }
        }
        else if (ModManager.MSC && friendTracker.friend is Creature friend && rep.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && rep.abstractAI.RealAI is SlugNPCAI ai && ai.friendTracker.friend == friend)
        {
            result.intensity = 1f;
            result.type = CreatureTemplate.Relationship.Type.Pack;
        }
        if (rep.realizedCreature is Creature c && c.abstractPhysicalObject.SameRippleLayer(creature))
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

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost) => new(cost.resistance + Custom.LerpMap(Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, false) - threatTracker.ThreatOfTile(creature.pos, false)), 0f, 1.5f, 0f, 10000f, 5f), cost.legality);

    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
    {
        if (!firstSpot && Random.value > Fear)
            return;
        var relationship = DynamicRelationship(otherCreature);
        if (relationship.type != CreatureTemplate.Relationship.Type.Ignores && relationship.type != CreatureTemplate.Relationship.Type.Pack)
        {
            var guessPos = Bug.room.MiddleOfTile(otherCreature.BestGuessForPosition());
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

    public virtual void AggressiveBehav(FoodItemRepresentation target)
    {
        if (Bug.safariControlled)
            return;
        var destination = target.BestGuessForPosition();
        creature.abstractAI.SetDestination(destination);
        if (target.RepresentedItem.realizedObject is FirecrackerPlant pl && pl.grabbedBy?.Count is null or 0 && pl.room is Room crRm && crRm.abstractRoom.index == Bug.room.abstractRoom.index)
        {
            var mPos = Bug.mainBodyChunk.pos;
            var bs = pl.bodyChunks;
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                if (Custom.DistLess(b.pos, mPos, 10f) && Custom.DistLess(mPos + Custom.DirVec(Bug.bodyChunks[1].pos, mPos) * 10f, b.pos, b.rad + 12.5f) && Bug.Grab(pl, 0, i, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, Random.value, true, true))
                {
                    Behav = Behavior.ReturnPrey;
                    FoodTracker.ForgetItem(pl.abstractPhysicalObject);
                    break;
                }
            }
        }
    }

    public virtual void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
    {
        if (Bug is not TintedBeetle bug || subjectCrit is not Player || tracker is not Tracker trk || (objectCrit is not null && subjectCrit.abstractPhysicalObject.SameRippleLayer(objectCrit.abstractPhysicalObject)))
            return;
        var creatureRepresentation = trk.RepresentationForObject(subjectCrit, false);
        if (creatureRepresentation is null)
            return;
        Tracker.CreatureRepresentation? creatureRepresentation2 = null;
        var flag = objectCrit == bug;
        if (!flag)
        {
            if (objectCrit is null)
                return;
            creatureRepresentation2 = trk.RepresentationForObject(objectCrit, false);
            if (creatureRepresentation2 is null)
                return;
        }
        if ((!flag && bug.dead) || (creatureRepresentation2 is not null && creatureRepresentation.TicksSinceSeen > 40 && creatureRepresentation2.TicksSinceSeen > 40))
            return;
        if (ID == SocialEventRecognizer.EventID.ItemOffering)
        {
            if (flag)
                friendTracker?.ItemOffered(creatureRepresentation, involvedItem);
            return;
        }
        var num = 0f;
        if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt)
            num = .05f;
        else if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
            num = .15f;
        else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
            num = .35f;
        else if (ID == SocialEventRecognizer.EventID.LethalAttack)
            num = 1f;
        if (objectCrit!.dead)
            num /= 3f;
        if (flag)
        {
            if (friendTracker?.friend is Creature c && subjectCrit == c)
            {
                if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt || ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
                    return;
                num /= 2f;
            }
            PlayerRelationChange(-num, subjectCrit.abstractCreature);
        }
        else if (creatureRepresentation2?.dynamicRelationship?.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
        {
            var num2 = .1f;
            if (threatTracker?.GetThreatCreature(objectCrit.abstractCreature) is not null && bug.mainBodyChunk is BodyChunk b)
                num2 += .7f * Custom.LerpMap(Vector2.Distance(b.pos, objectCrit.DangerPos), 120f, 320f, 1f, .1f);
            var flag2 = false;
            var grs = objectCrit.grasps;
            for (var i = 0; i < grs.Length; i++)
            {
                if (flag2)
                    break;
                if (grs[i]?.grabbed == bug)
                    flag2 = true;
            }
            if (flag2)
            {
                if (ID == SocialEventRecognizer.EventID.NonLethalAttack || ID == SocialEventRecognizer.EventID.LethalAttack)
                    num = 1f;
                num2 = 2f;
            }
            PlayerRelationChange(Mathf.Pow(num, .5f) * num2, subjectCrit.abstractCreature);
        }
        else if (creatureRepresentation2?.dynamicRelationship?.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
            PlayerRelationChange(num * -.75f, subjectCrit.abstractCreature);
    }

    public virtual void PlayerRelationChange(float change, AbstractCreature player)
    {
        var orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(player.ID);
        orInitiateRelationship.InfluenceTempLike(change * 1.5f);
        orInitiateRelationship.InfluenceLike(change * .75f);
        orInitiateRelationship.InfluenceKnow(Math.Abs(change) * .25f);
        creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(CommunityID.TintedBeetles, creature.world.RegionNumber, (player.state as PlayerState)!.playerNumber, change * .05f, .25f, .3f);
    }

    public virtual void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe)
    {
        if (Bug is TintedBeetle tb && tb.room is Room rm && giftOfferedToMe.item is FirecrackerPlant plt)
        {
            var orInitiateRelationship = tb.State.socialMemory.GetOrInitiateRelationship(giftOfferedToMe.owner.abstractCreature.ID);
            var flag = plt.fuseCounter <= 0;
            if (orInitiateRelationship.like > -.9f)
            {
                if (ModManager.MSC && rm.game.session is ArenaGameSession sess && sess.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge && sess.arenaSitting.gameTypeSetup.challengeMeta.tamingDifficultyMultiplier > 0)
                {
                    var num = sess.arenaSitting.gameTypeSetup.challengeMeta.tamingDifficultyMultiplier / 10f;
                    orInitiateRelationship.InfluenceLike((flag ? 1.1f : .1f) / num);
                    orInitiateRelationship.InfluenceTempLike((flag ? 1.5f : .2f) / num);
                }
                else
                {
                    orInitiateRelationship.InfluenceLike((flag ? 1.1f : .1f) / friendTracker.tamingDifficlty);
                    orInitiateRelationship.InfluenceTempLike((flag ? 1.5f : .2f) / friendTracker.tamingDifficlty);
                }
            }
            if (giftOfferedToMe.owner is Player p)
                creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(CommunityID.TintedBeetles, creature.world.RegionNumber, creature.world.game.session is not StoryGameSession ? p.playerState.playerNumber : 0, .1f, .2f, .1f);
        }
    }
}