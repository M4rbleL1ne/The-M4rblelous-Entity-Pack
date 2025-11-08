using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using UnityEngine;
using DevInterface;
using MoreSlugcats;
using Random = UnityEngine.Random;
using System;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class DentureCritob : Critob, ISandboxHandler
{
    internal DentureCritob() : base(CreatureTemplateType.Denture)
    {
        Icon = new SimpleIcon("Kill_Denture", Ext.MenuGrey);
        RegisterUnlock(KillScore.Constant(0), SandboxUnlockID.Denture);
        SandboxPerformanceCost = new(.4f, .4f);
        LoadedPerformanceCost = 10f;
        ShelterDanger = ShelterDanger.TooLarge;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [];

    public override int ExpeditionScore() => 0;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Ext.MenuGrey;

    public override string DevtoolsMapName(AbstractCreature acrit) => "dt";

    public override IEnumerable<string> WorldFileAliases() => ["denture"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = float.MaxValue },
            StunResistances = new() { Base = float.MaxValue },
            HasAI = false
        }.IntoTemplate();
        t.requireAImap = true;
        t.doPreBakedPathing = false;
        t.stowFoodInDen = true;
        t.offScreenSpeed = 0f;
        t.bodySize = 2.5f;
        t.grasps = 0;
        t.visualRadius = 400f;
        t.movementBasedVision = 1f;
        t.waterVision = 1f;
        t.throughSurfaceVision = 1f;
        t.dangerousToPlayer = .5f;
        t.communityInfluence = .05f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.BlizzardWanderer = true;
		t.countsAsAKill = 0;
		t.wormGrassImmune = true;
		t.wormgrassTilesIgnored = true;
		t.BlizzardAdapted = true;
		t.BlizzardWanderer = true;
        t.pickupAction = "Chomp";
		t.shortcutColor = Ext.MenuGrey;
		t.shortcutSegments = 2;
		t.scaryness = 1f;
		t.deliciousness = 0f;
		t.meatPoints = 0;
		t.canSwim = false;
		t.canFly = false;
        t.daddyCorruptionImmune = true;
        return t;
    }

	public override void EstablishRelationships()
	{
		var dt = new Relationships(Type);
        dt.Eats(CreatureTemplate.Type.Slugcat, 1f);
        dt.Eats(CreatureTemplate.Type.LanternMouse, 1f);
        dt.Eats(CreatureTemplate.Type.BlueLizard, 1f);
        dt.Eats(CreatureTemplate.Type.Fly, 1f);
        dt.Eats(CreatureTemplate.Type.Leech, 1f);
        dt.Eats(CreatureTemplate.Type.Snail, 1f);
        dt.Eats(CreatureTemplate.Type.CicadaA, 1f);
        dt.Eats(CreatureTemplate.Type.Spider, 1f);
        dt.Eats(CreatureTemplate.Type.JetFish, 1f);
        dt.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        dt.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        dt.Eats(CreatureTemplate.Type.Scavenger, 1f);
        dt.Eats(CreatureTemplate.Type.VultureGrub, 1f);
        dt.Eats(CreatureTemplate.Type.Hazer, 1f);
        dt.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        dt.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        dt.Eats(CreatureTemplate.Type.EggBug, 1f);
        dt.Eats(CreatureTemplate.Type.BigSpider, 1f);
        dt.Eats(CreatureTemplate.Type.DropBug, 1f);
        dt.Attacks(CreatureTemplate.Type.GreenLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.WhiteLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.PinkLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.YellowLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.BlackLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.Salamander, 1f);
        dt.Attacks(CreatureTemplate.Type.CyanLizard, 1f);
        dt.Attacks(CreatureTemplate.Type.Vulture, 1f);
        dt.Attacks(CreatureTemplate.Type.SpitterSpider, 1f);
        dt.Ignores(CreatureTemplate.Type.KingVulture);
        dt.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        dt.FearedBy(CreatureTemplate.Type.LanternMouse, 1f);
        dt.FearedBy(CreatureTemplate.Type.BlueLizard, 1f);
        dt.FearedBy(CreatureTemplate.Type.Fly, 1f);
        dt.FearedBy(CreatureTemplate.Type.Leech, 1f);
        dt.FearedBy(CreatureTemplate.Type.Snail, 1f);
        dt.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        dt.FearedBy(CreatureTemplate.Type.Spider, 1f);
        dt.FearedBy(CreatureTemplate.Type.JetFish, 1f);
        dt.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
        dt.IgnoredBy(CreatureTemplate.Type.Centipede);
        dt.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        dt.FearedBy(CreatureTemplate.Type.Scavenger, 1f);
        dt.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        dt.FearedBy(CreatureTemplate.Type.Hazer, 1f);
        dt.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        dt.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        dt.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        dt.FearedBy(CreatureTemplate.Type.BigSpider, 1f);
        dt.FearedBy(CreatureTemplate.Type.DropBug, 1f);
        dt.FearedBy(CreatureTemplate.Type.GreenLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.WhiteLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.PinkLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.YellowLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.BlackLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.Salamander, .5f);
        dt.FearedBy(CreatureTemplate.Type.CyanLizard, .5f);
        dt.FearedBy(CreatureTemplate.Type.Vulture, .5f);
        dt.FearedBy(CreatureTemplate.Type.SpitterSpider, .5f);
        dt.IgnoredBy(CreatureTemplate.Type.KingVulture);
        dt.IgnoredBy(CreatureTemplate.Type.MirosBird);
        dt.IgnoredBy(CreatureTemplate.Type.BigEel);
        dt.IgnoredBy(CreatureTemplate.Type.DaddyLongLegs);
        dt.IgnoredBy(CreatureTemplate.Type.Deer);
        dt.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        dt.IgnoredBy(CreatureTemplate.Type.RedLizard);
        dt.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        dt.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        dt.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        if (ModManager.DLCShared)
        {
            dt.Eats(DLCSharedEnums.CreatureTemplateType.JungleLeech, 1f);
            dt.Eats(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            dt.Eats(DLCSharedEnums.CreatureTemplateType.MotherSpider, 1f);
            dt.Eats(DLCSharedEnums.CreatureTemplateType.JungleLeech, 1f);
            dt.Eats(DLCSharedEnums.CreatureTemplateType.ZoopLizard, 1f);
            dt.Eats(DLCSharedEnums.CreatureTemplateType.ScavengerElite, 1f);
            dt.Attacks(DLCSharedEnums.CreatureTemplateType.EelLizard, 1f);
            dt.Ignores(DLCSharedEnums.CreatureTemplateType.MirosVulture);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.JungleLeech, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.MotherSpider, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.JungleLeech, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.ScavengerElite, 1f);
            dt.FearedBy(DLCSharedEnums.CreatureTemplateType.EelLizard, .5f);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.MirosVulture);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.AquaCenti);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.BigJelly);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.Inspector);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs);
            dt.IgnoredBy(DLCSharedEnums.CreatureTemplateType.SpitLizard);
        }
        if (ModManager.MSC)
        {
            dt.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1f);
            dt.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
            dt.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1f);
            dt.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1f);
            dt.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
            dt.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1f);
            dt.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard);
            dt.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy);
        }
        dt.Eats(CreatureTemplateType.HazerMom, 1f);
        dt.Eats(CreatureTemplateType.Hoverfly, 1f);
        dt.Eats(CreatureTemplateType.SurfaceSwimmer, 1f);
        dt.Eats(CreatureTemplateType.ThornBug, 1f);
        dt.Eats(CreatureTemplateType.TintedBeetle, 1f);
        dt.Eats(CreatureTemplateType.WaterBlob, 1f);
        dt.Eats(CreatureTemplateType.BouncingBall, 1f);
        dt.Eats(CreatureTemplateType.Polliwog, 1f);
        dt.Eats(CreatureTemplateType.DivingBeetle, 1f);
        dt.Eats(CreatureTemplateType.NoodleEater, 1f);
        dt.Eats(CreatureTemplateType.MiniBlackLeech, 1f);
        dt.Eats(CreatureTemplateType.ChipChop, 1f);
        dt.Attacks(CreatureTemplateType.HunterSeeker, 1f);
        dt.Attacks(CreatureTemplateType.MoleSalamander, 1f);
        dt.Attacks(CreatureTemplateType.WaterSpitter, 1f);
        dt.Attacks(CreatureTemplateType.Sporantula, 1f);
        dt.Ignores(CreatureTemplateType.FatFireFly);
        dt.FearedBy(CreatureTemplateType.HazerMom, 1f);
        dt.FearedBy(CreatureTemplateType.Hoverfly, 1f);
        dt.FearedBy(CreatureTemplateType.SurfaceSwimmer, 1f);
        dt.FearedBy(CreatureTemplateType.ThornBug, 1f);
        dt.FearedBy(CreatureTemplateType.TintedBeetle, 1f);
        dt.FearedBy(CreatureTemplateType.WaterBlob, 1f);
        dt.FearedBy(CreatureTemplateType.BouncingBall, 1f);
        dt.FearedBy(CreatureTemplateType.Polliwog, 1f);
        dt.FearedBy(CreatureTemplateType.DivingBeetle, 1f);
        dt.FearedBy(CreatureTemplateType.NoodleEater, 1f);
        dt.FearedBy(CreatureTemplateType.MiniBlackLeech, 1f);
        dt.FearedBy(CreatureTemplateType.ChipChop, 1f);
        dt.FearedBy(CreatureTemplateType.HunterSeeker, .5f);
        dt.FearedBy(CreatureTemplateType.MoleSalamander, .5f);
        dt.FearedBy(CreatureTemplateType.WaterSpitter, .5f);
        dt.FearedBy(CreatureTemplateType.Sporantula, .5f);
        dt.IgnoredBy(CreatureTemplateType.MiniLeviathan);
        dt.IgnoredBy(CreatureTemplateType.MiniFlyingBigEel);
        dt.IgnoredBy(CreatureTemplateType.FatFireFly);
        dt.IgnoredBy(CreatureTemplateType.FlyingBigEel);
        dt.IgnoredBy(CreatureTemplateType.Blizzor);
        dt.IgnoredBy(CreatureTemplateType.SilverLizard);
        dt.IgnoredBy(CreatureTemplateType.Scutigera);
        dt.IgnoredBy(CreatureTemplateType.RedHorrorCenti);
        dt.IgnoredBy(CreatureTemplateType.CommonEel);
        dt.IgnoredBy(CreatureTemplateType.Killerpillar);
        dt.IgnoredBy(CreatureTemplateType.Glowpillar);
        if (ModManager.Watcher)
        {
            dt.Eats(WatcherEnums.CreatureTemplateType.FireSprite, 1f);
            dt.Eats(WatcherEnums.CreatureTemplateType.Frog, 1f);
            dt.Eats(WatcherEnums.CreatureTemplateType.Rat, 1f);
            dt.Eats(WatcherEnums.CreatureTemplateType.Tardigrade, 1f);
            dt.Eats(WatcherEnums.CreatureTemplateType.MothGrub, 1f);
            dt.Eats(WatcherEnums.CreatureTemplateType.PeachLizard, 1f);
            dt.Attacks(WatcherEnums.CreatureTemplateType.Barnacle, 1f);
            dt.Attacks(WatcherEnums.CreatureTemplateType.SmallMoth, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.FireSprite, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.Frog, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.Rat, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.PeachLizard, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.Tardigrade, 1f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.Barnacle, .5f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.SmallMoth, .5f);
            dt.FearedBy(WatcherEnums.CreatureTemplateType.BigMoth, .25f);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.Loach);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.SandGrub);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.BigSandGrub);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.Rattler);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.BoxWorm);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.DrillCrab);
            dt.IgnoredBy(WatcherEnums.CreatureTemplateType.SkyWhale);
        }
    }

	public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Denture(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new NoHealthState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.PoleMimic;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        if (Random.value < .08f && AbsProps.TryGetValue(abstractCreature, out var props))
            props.Albino = true;
        Random.state = state;
        return abstractCreature;
    }
}