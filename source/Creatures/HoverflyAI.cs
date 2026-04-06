using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using Noise;
using System.Collections.Generic;

namespace LBMergedMods.Creatures;

public class HoverflyAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
    public class FlyBehavior(string value, bool register = false) : ExtEnum<FlyBehavior>(value, register)
    {
        public static readonly FlyBehavior Idle = new("Idle", true),
            Flee = new("Flee", true),
            Hunt = new("Hunt", true),
            EscapeRain = new("EscapeRain", true),
            GetUnstuck = new("GetUnstuck", true);
    }

    public Hoverfly Fly;
    public float CurrentUtility;
    public FlyBehavior Behavior;
    public WorldCoordinate IdleSitSpot, ForbiddenIdleSitSpot;
    public int IdleSitCounter, HuntAttackCounter, TiredOfHuntingCounter;
    public AbstractPhysicalObject? TiredOfHuntingItem;
    public Tracker.CreatureRepresentation? FocusCreature;
    public FoodItemRepresentation? FocusItem;
    public FoodItemTracker FoodTracker;
    public Vector2? SwooshToPos;

    public HoverflyAI(AbstractCreature creature, World world) : base(creature, world)
    {
        Fly = (creature.realizedCreature as Hoverfly)!;
        Fly.AI = this;
        AddModule(FoodTracker = new(this, 5, 1f, 5f, 15f));
        AddModule(new HoverflyPather(this, world, creature) { accessibilityStepsPerFrame = 60 });
        AddModule(new Tracker(this, 10, 10, 250, .5f, 5, 5, 20));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new StuckTracker(this, true, true));
        AddModule(new NoiseTracker(this, tracker));
        stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(stuckTracker));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new UtilityComparer(this));
        AddModule(new DiscomfortTracker(this, tracker, 0f));
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(FoodTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(stuckTracker, null, 1f, 1.1f);
        Behavior = FlyBehavior.Idle;
    }

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        cost = base.TravelPreference(coord, cost);
        if (Fly?.room is Room rm && !Fly.safariControlled && rm.readyForAI is true)
        {
            if (coord.destinationCoord.TileDefined)
            {
                var aiTile = rm.aimap.getAItile(coord.destinationCoord.Tile);
                if (aiTile.narrowSpace || rm.aimap.getTerrainProximity(coord.destinationCoord) < 4 || aiTile.floorAltitude < 4)
                    cost.resistance += 100f;
            }
        }
        return cost;
    }

    public override void NewRoom(Room room)
    {
        IdleSitSpot = room.GetWorldCoordinate(new IntVector2(Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight)));
        if (pathFinder.GetDestination.room == room.abstractRoom.index && !pathFinder.GetDestination.TileDefined && pathFinder.GetDestination.CompareDisregardingTile(creature.pos))
            creature.abstractAI.SetDestination(IdleSitSpot);
        base.NewRoom(room);
    }

    public virtual void ScanForFood(AbstractCreature ow)
    {
        if (HoverflyData.TryGetValue(ow, out var data1) && data1.CanEat && ow.Room is AbstractRoom arm && arm.entities is List<AbstractWorldEntity> l && arm.realizedRoom is Room ro)
        {
            for (var i = 0; i < l.Count; i++)
            {
                if (l[i] is AbstractPhysicalObject obj && obj.SameRippleLayer(ow) && obj.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit && obj.realizedObject is DangleFruit dan && (ro.GetTile(dan.firstChunk.pos with { y = dan.firstChunk.pos.y - 20f })?.Solid is true || data1.CanEatRoot))
                    FoodTracker.AddItem(this.CreateTrackerRepresentationForItem(obj));
            }
        }
    }

    public virtual bool CanHunt(PhysicalObject obj) => obj is DangleFruit;

    public override void Update()
    {
        FocusCreature = null;
        base.Update();
        if (creature is not AbstractCreature ow)
            return;
        if (Fly.LickedByPlayer is Player p)
            tracker.SeeCreature(p.abstractCreature);
        ScanForFood(ow);
        var aIModule = utilityComparer.HighestUtilityModule();
        CurrentUtility = utilityComparer.HighestUtility();
        if (aIModule is not null)
        {
            if (Fly.safariControlled)
                CurrentUtility = 0f;
            if (aIModule is ThreatTracker)
                Behavior = FlyBehavior.Flee;
            else if (aIModule is RainTracker)
                Behavior = FlyBehavior.EscapeRain;
            else if (aIModule is FoodItemTracker)
            {
                if (FoodTracker.MostAttractiveItem is null)
                    Behavior = FlyBehavior.Idle;
                else
                    Behavior = FlyBehavior.Hunt;
            }
            else if (aIModule is StuckTracker)
                Behavior = FlyBehavior.GetUnstuck;
        }
        if (CurrentUtility < .1f)
            Behavior = FlyBehavior.Idle;
        SwooshToPos = null;
        stuckTracker.satisfiedWithThisPosition = !Fly.AtSitDestination;
        if (Behavior == FlyBehavior.Idle)
        {
            if (denFinder.GetDenPosition() is WorldCoordinate w && w.room != ow.pos.room)
                ow.abstractAI.SetDestination(w);
            if (!IdleSitSpot.TileDefined || IdleSitSpot.room != Fly.room.abstractRoom.index || !Fly.Climbable(IdleSitSpot.Tile))
                IdleSitSpot = Fly.room.GetWorldCoordinate(new IntVector2(Random.Range(0, Fly.room.TileWidth), Random.Range(0, Fly.room.TileHeight)));
            ow.abstractAI.SetDestination(IdleSitSpot);
            --IdleSitCounter;
            if (IdleSitCounter < 1 || Fly.room.aimap.getAItile(IdleSitSpot).narrowSpace)
            {
                IdleSitCounter = Random.Range(0, Random.Range(0, 650));
                ForbiddenIdleSitSpot = IdleSitSpot;
            }
            if (IdleSitSpot == ForbiddenIdleSitSpot)
            {
                var intVector = new IntVector2(Random.Range(0, Fly.room.TileWidth), Random.Range(0, Fly.room.TileHeight));
                if (Fly.Climbable(intVector) && pathFinder.CoordinateReachable(Fly.room.GetWorldCoordinate(intVector)) && (Random.value < .3f || VisualContact(Fly.room.MiddleOfTile(intVector), 0f)))
                    IdleSitSpot = Fly.room.GetWorldCoordinate(intVector);
            }
        }
        else if (Behavior == FlyBehavior.Flee)
        {
            var destination = threatTracker.FleeTo(ow.pos, 1, 30, CurrentUtility > .3f);
            if (threatTracker.mostThreateningCreature is Tracker.CreatureRepresentation cr)
                FocusCreature = cr;
            FocusItem = null;
            ow.abstractAI.SetDestination(destination);
        }
        else if (Behavior == FlyBehavior.EscapeRain)
        {
            if (denFinder.GetDenPosition() is WorldCoordinate w)
                ow.abstractAI.SetDestination(w);
        }
        else if (Behavior == FlyBehavior.Hunt && Fly.grasps is Creature.Grasp[] g && g[0]?.grabbed is null)
        {
            FocusCreature = null;
            FocusItem = FoodTracker.MostAttractiveItem;
            ow.abstractAI.SetDestination(FocusItem!.BestGuessForPosition());
            if (Fly.room is Room rm && FocusItem.GetVisualContact && FocusItem.RepresentedItem.realizedObject is PhysicalObject obj && CanHunt(obj) && obj.abstractPhysicalObject.SameRippleLayer(ow) && HoverflyData.TryGetValue(ow, out var data) && (rm.GetTile(obj.firstChunk.pos with { y = obj.firstChunk.pos.y - 20f })?.Solid is true && data.CanEat || data.CanEatRoot) && obj.grabbedBy?.Count == 0 && Custom.DistLess(obj.firstChunk.pos, Fly.firstChunk.pos, Fly.firstChunk.rad * 9f) && Custom.InsideRect(FocusItem.BestGuessForPosition().Tile, new(-30, -30, rm.TileWidth + 30, rm.TileHeight + 30)))
            {
                if (HuntAttackCounter < 50)
                {
                    ++HuntAttackCounter;
                    SwooshToPos = obj.firstChunk.pos;
                    if (Custom.DistLess(Fly.firstChunk.pos, SwooshToPos.Value, Fly.firstChunk.rad * 1.5f))
                        Fly.TryToGrabPrey(obj);
                }
                else if (Random.value < .1f)
                {
                    ++HuntAttackCounter;
                    if (HuntAttackCounter > 200)
                        HuntAttackCounter = 0;
                }
            }
            ++TiredOfHuntingCounter;
            if (TiredOfHuntingCounter > 600)
            {
                TiredOfHuntingItem = FocusItem.RepresentedItem;
                TiredOfHuntingCounter = 0;
                FoodTracker.ForgetItem(TiredOfHuntingItem);
            }
        }
        else if (Behavior == FlyBehavior.GetUnstuck)
            ow.abstractAI.SetDestination(stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
    }

    public override float VisualScore(Vector2 lookAtPoint, float targetSpeed) => base.VisualScore(lookAtPoint, targetSpeed) - Mathf.InverseLerp(1f, -.3f, Vector2.Dot(default, (Fly.firstChunk.pos - lookAtPoint).normalized));

    public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
    {
        if (Fly.graphicsModule is HoverflyGraphics g)
            g.CreatureLooker.ReevaluateLookObject(creatureRep, 2f);
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        Tracker.CreatureRepresentation creatureRep = !otherCreature.creatureTemplate.smallCreature ? new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3) : new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false);
        if (Fly.graphicsModule is HoverflyGraphics g)
            g.CreatureLooker.ReevaluateLookObject(creatureRep, 2f);
        return creatureRep;
    }

    public virtual void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        if (Fly is Hoverfly fly)
        {
            if (fly.graphicsModule is HoverflyGraphics g && Custom.DistLess(noise.pos, fly.firstChunk.pos, fly.firstChunk.rad * 10f) && noise.strength > .25f && noise.interesting > 0f)
                g.EyeFearCounter = 30;
            if (Random.value > .5f)
                fly.room?.PlaySound(NewSoundID.M4R_Hoverfly_Startle, fly.firstChunk, false, 1.25f, 1f + fly.IVars.SoundPitchBonus);
        }
    }

    public override void HeardNoise(InGameNoise noise)
    {
        base.HeardNoise(noise);
        if (Fly is Hoverfly fly)
        {
            if (fly.graphicsModule is HoverflyGraphics g && Custom.DistLess(noise.pos, fly.firstChunk.pos, fly.firstChunk.rad * 10f) && noise.strength > .25f && noise.interesting > 0f)
                g.EyeFearCounter = 30;
            if (Random.value > .5f)
                fly.room?.PlaySound(NewSoundID.M4R_Hoverfly_Startle, fly.firstChunk, false, 1.25f, 1f + fly.IVars.SoundPitchBonus);
        }
    }

    public virtual AIModule? ModuleToTrackRelationship(CreatureTemplate.Relationship relationship) => relationship.type == CreatureTemplate.Relationship.Type.Afraid ? threatTracker : null;

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation) => StaticRelationship(dRelation.trackerRep.representedCreature);
}