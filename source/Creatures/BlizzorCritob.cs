using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using RWCustom;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class BlizzorCritob : Critob
{
    internal BlizzorCritob() : base(CreatureTemplateType.Blizzor)
    {
        Icon = new SimpleIcon("Kill_Blizzor", Color.white);
        RegisterUnlock(KillScore.Configurable(18), SandboxUnlockID.Blizzor);
        SandboxPerformanceCost = new(1.5f, .5f);
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) => paralyzing = true;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

    public override string DevtoolsMapName(AbstractCreature acrit) => "Blzz";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Dark,
        RoomAttractivenessPanel.Category.LikesOutside
    ];

    public override int ExpeditionScore() => 18;

    public override IEnumerable<string> WorldFileAliases() => ["blizzor"];

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow)
    {
        var prox = map.getTerrainProximity(tilePos);
        if (prox <= 1)
            allow = false;
        else if (map.room?.game?.GetArenaGameSession?.arenaSitting?.sandboxPlayMode is true or null)
        {
            var aItile = map.getAItile(tilePos);
            if (aItile.smoothedFloorAltitude > 2 && aItile.smoothedFloorAltitude + aItile.floorAltitude > Custom.LerpMap(prox, 2f, 6f, 6f, 4f) * 2f)
                allow = false;
        }
    }

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.MirosBird, this)
        {
            TileResistances = new() { Air = new(1f, Allowed) },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                SideHighway = new(100f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 8f, Water = 102f },
            StunResistances = new() { Base = 3f, Water = 102f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.MirosBird)
        }.IntoTemplate();
        t.abstractedLaziness = 50;
        t.requireAImap = true;
        t.canFly = false;
        t.offScreenSpeed = 1.5f;
        t.bodySize = 7f;
        t.grasps = 1;
        t.stowFoodInDen = true;
        t.visualRadius = 1800f;
        t.movementBasedVision = .1f;
        t.hibernateOffScreen = true;
        t.dangerousToPlayer = .7f;
        t.communityInfluence = .1f;
        t.meatPoints = 13;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.BlizzardAdapted = true;
        t.BlizzardWanderer = true;
        t.isTooCloseToTerrain = (aimap, pos) =>
        {
            var prox = aimap.getTerrainProximity(pos);
            if (prox <= 1)
                return 1;
            var aItile = aimap.getAItile(pos);
            return (aItile.smoothedFloorAltitude > 2 && (aItile.smoothedFloorAltitude + aItile.floorAltitude) > Custom.LerpMap(prox, 2f, 6f, 6f, 4f) * 2f) ? 1 : 0;
        };
        t.canAutoAbstractPath = false;
        return t;
    }

    public override void EstablishRelationships()
    {
        var b = new Relationships(Type);
        b.AttackedBy(CreatureTemplate.Type.RedLizard, .4f);
        b.FearedBy(CreatureTemplate.Type.Slugcat, .8f);
        b.FearedBy(CreatureTemplate.Type.TentaclePlant, .8f);
        b.AntagonizedBy(CreatureTemplate.Type.MirosBird, .1f);
        b.Antagonizes(CreatureTemplate.Type.MirosBird, .1f);
        b.Antagonizes(Type, .1f);
        b.Eats(CreatureTemplate.Type.Slugcat, .6f);
        b.Fears(CreatureTemplate.Type.DaddyLongLegs, .7f);
        b.Ignores(CreatureTemplate.Type.SmallCentipede);
        b.Ignores(CreatureTemplate.Type.Fly);
        b.Ignores(CreatureTemplate.Type.Leech);
        b.Ignores(CreatureTemplate.Type.Spider);
        b.Eats(CreatureTemplate.Type.Centipede, .8f);
        b.Eats(CreatureTemplate.Type.BigSpider, .5f);
        b.Fears(CreatureTemplate.Type.RedCentipede, .7f);
        b.FearedBy(CreatureTemplate.Type.Scavenger, .7f);
        b.FearedBy(CreatureTemplate.Type.BigSpider, .9f);
        b.FearedBy(CreatureTemplate.Type.DropBug, .9f);
        b.FearedBy(CreatureTemplate.Type.BigNeedleWorm, .9f);
        b.Ignores(CreatureTemplate.Type.Overseer);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new MirosBirdAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Blizzor(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new MirosBirdAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.MirosBird;
}