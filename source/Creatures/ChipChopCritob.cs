using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class ChipChopCritob : Critob
{
    internal ChipChopCritob() : base(CreatureTemplateType.ChipChop)
    {
        Icon = new SimpleIcon("Kill_ChipChop", new(.1f, .9f, .2f));
        RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.ChipChop);
        SandboxPerformanceCost = new(.1f, .5f);
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override int ExpeditionScore() => 3;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.1f, .9f, .2f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "chch";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override IEnumerable<string> WorldFileAliases() => ["chipchop", "chip chop"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1f, Allowed),
                Wall = new(1f, Allowed),
                Ceiling = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(2f, Allowed),
                ShortCut = new(.2f, Allowed),
                NPCTransportation = new(20f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed),
                DropToFloor = new(10f, Allowed),
                Slope = new(1.6f, Allowed),
                CeilingSlope = new(1.6f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = .95f },
            StunResistances = new() { Base = 1.1f },
            HasAI = false,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BlueLizard)
        }.IntoTemplate();
        t.instantDeathDamageLimit = 1f;
        t.pickupAction = "Grab/Eat";
        t.throwAction = "Release";
        t.abstractedLaziness = 100;
        t.requireAImap = true;
        t.doPreBakedPathing = false;
        t.offScreenSpeed = .2f;
        t.bodySize = .5f;
        t.grasps = 1;
        t.visualRadius = 1300f;
        t.dangerousToPlayer = .05f;
        t.communityID = CreatureCommunities.CommunityID.None;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.usesNPCTransportation = true;
        t.canSwim = true;
        t.waterPathingResistance = 2f;
        t.movementBasedVision = .2f;
        t.meatPoints = 2;
        t.lungCapacity = 600f;
        t.communityInfluence = 0f;
        t.shortcutSegments = 2;
        t.stowFoodInDen = true;
        t.throughSurfaceVision = .1f;
        t.waterVision = .2f;
        t.deliciousness = 1f;
        t.scaryness = 0f;
        return t;
    }

    public override void EstablishRelationships()
    {
        var ctp = new Relationships(Type);
        ctp.Fears(CreatureTemplate.Type.Slugcat, .6f);
        ctp.Fears(CreatureTemplate.Type.BigNeedleWorm, .6f);
        ctp.Fears(CreatureTemplate.Type.Scavenger, .6f);
        ctp.Fears(CreatureTemplate.Type.LizardTemplate, 1f);
        ctp.Fears(CreatureTemplate.Type.Leech, .4f);
        ctp.Fears(CreatureTemplate.Type.Spider, .8f);
        ctp.Fears(CreatureTemplate.Type.BigSpider, 1f);
        ctp.Fears(CreatureTemplate.Type.Deer, 1f);
        ctp.Fears(CreatureTemplate.Type.MirosBird, 1f);
        ctp.Fears(CreatureTemplate.Type.BigEel, 1f);
        ctp.Fears(CreatureTemplate.Type.DropBug, 1f);
        ctp.Fears(CreatureTemplate.Type.Centipede, 1f);
        ctp.Fears(CreatureTemplate.Type.Vulture, 1f);
        ctp.Fears(CreatureTemplate.Type.PoleMimic, .8f);
        ctp.Fears(CreatureTemplate.Type.TempleGuard, 1f);
        ctp.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        ctp.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        ctp.Eats(CreatureTemplate.Type.Fly, 1f);
        ctp.Eats(CreatureTemplate.Type.Hazer, 1f);
        ctp.Eats(CreatureTemplate.Type.VultureGrub, 1f);
        ctp.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        ctp.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        ctp.Eats(CreatureTemplate.Type.EggBug, 1f);
        ctp.FearedBy(CreatureTemplate.Type.Fly, 1f);
        ctp.FearedBy(CreatureTemplate.Type.Hazer, 1f);
        ctp.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        ctp.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        ctp.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        ctp.FearedBy(CreatureTemplate.Type.Slugcat, .05f);
        ctp.IgnoredBy(CreatureTemplate.Type.BigNeedleWorm);
        ctp.AttackedBy(CreatureTemplate.Type.Scavenger, .2f);
        ctp.EatenBy(CreatureTemplate.Type.LizardTemplate, 1f);
        ctp.EatenBy(CreatureTemplate.Type.Leech, 1f);
        ctp.EatenBy(CreatureTemplate.Type.Spider, .8f);
        ctp.EatenBy(CreatureTemplate.Type.BigSpider, 1f);
        ctp.IgnoredBy(CreatureTemplate.Type.Deer);
        ctp.EatenBy(CreatureTemplate.Type.MirosBird, 1f);
        ctp.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        ctp.EatenBy(CreatureTemplate.Type.DropBug, 1f);
        ctp.EatenBy(CreatureTemplate.Type.Centipede, 1f);
        ctp.EatenBy(CreatureTemplate.Type.Vulture, 1f);
        ctp.EatenBy(CreatureTemplate.Type.PoleMimic, 1f);
        ctp.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        ctp.EatenBy(CreatureTemplate.Type.TentaclePlant, 1f);
        ctp.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        if (ModManager.DLCShared)
        {
            ctp.Fears(DLCSharedEnums.CreatureTemplateType.BigJelly, 1f);
            ctp.EatenBy(DLCSharedEnums.CreatureTemplateType.BigJelly, 1f);
            ctp.Ignores(DLCSharedEnums.CreatureTemplateType.Inspector);
            ctp.IgnoredBy(DLCSharedEnums.CreatureTemplateType.Inspector);
        }
        ctp.Ignores(Type);
        ctp.Ignores(CreatureTemplateType.SurfaceSwimmer);
        ctp.Ignores(CreatureTemplateType.ThornBug);
        ctp.Ignores(CreatureTemplateType.TintedBeetle);
        ctp.Ignores(CreatureTemplateType.NoodleEater);
        ctp.Fears(CreatureTemplateType.DivingBeetle, .8f);
        ctp.Fears(CreatureTemplateType.CommonEel, 1f);
        ctp.Fears(CreatureTemplateType.Killerpillar, 1f);
        ctp.Eats(CreatureTemplateType.WaterBlob, 1f);
        ctp.IgnoredBy(CreatureTemplateType.SurfaceSwimmer);
        ctp.IgnoredBy(CreatureTemplateType.ThornBug);
        ctp.IgnoredBy(CreatureTemplateType.TintedBeetle);
        ctp.IgnoredBy(CreatureTemplateType.NoodleEater);
        ctp.EatenBy(CreatureTemplateType.DivingBeetle, .8f);
        ctp.EatenBy(CreatureTemplateType.CommonEel, 1f);
        ctp.FearedBy(CreatureTemplateType.WaterBlob, 1f);
        ctp.EatenBy(CreatureTemplateType.Killerpillar, 1f);
        if (ModManager.Watcher)
        {
            ctp.Fears(WatcherEnums.CreatureTemplateType.Angler, 1f);
            ctp.AntagonizedBy(WatcherEnums.CreatureTemplateType.Angler, .8f);
        }
    }

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new ChipChop(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BigSpider;
}