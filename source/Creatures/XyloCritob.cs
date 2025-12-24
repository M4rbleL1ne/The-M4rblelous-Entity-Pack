using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using UnityEngine;
using DevInterface;
using RWCustom;
using Watcher;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

sealed class XyloCritob : Critob, ISandboxHandler
{
    internal XyloCritob() : base(CreatureTemplateType.Xylo)
    {
        Icon = new SimpleIcon("Kill_Xylo", new(.2f, .05f, .45f));
        RegisterUnlock(KillScore.Configurable(4), SandboxUnlockID.Xylo);
        SandboxPerformanceCost = new(.4f, .4f);
        LoadedPerformanceCost = 10f;
        ShelterDanger = ShelterDanger.TooLarge;
    }

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 3;

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [];

    public override int ExpeditionScore() => 4;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.2f, .05f, .45f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "xy";

    public override IEnumerable<string> WorldFileAliases() => ["xylo"];

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
        t.stowFoodInDen = false;
        t.offScreenSpeed = 0f;
        t.bodySize = 4f;
        t.grasps = 0;
        t.visualRadius = 0f;
        t.movementBasedVision = 0f;
        t.waterVision = 0f;
        t.throughSurfaceVision = 0f;
        t.dangerousToPlayer = .1f;
        t.communityInfluence = .0f;
        t.wormGrassImmune = true;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.BlizzardWanderer = true;
		t.countsAsAKill = 0;
		t.wormgrassTilesIgnored = true;
		t.BlizzardAdapted = true;
		t.BlizzardWanderer = true;
		t.shortcutColor = default;
		t.shortcutSegments = 3;
		t.scaryness = 1f;
		t.deliciousness = 0f;
		t.meatPoints = 0;
		t.canSwim = false;
		t.canFly = false;
        t.daddyCorruptionImmune = true;
        t.isTooCloseToTerrain = (aimap, pos) => aimap.getTerrainProximity(pos) <= 3 ? 1 : 0;
        return t;
    }

	public override void EstablishRelationships()
	{
		var self = new Relationships(Type);
        var tps = CreatureTemplate.Type.values.entries;
        for (var i = 0; i < tps.Count; i++)
            self.IgnoredBy(new(tps[i]));
        self.Eats(CreatureTemplate.Type.Leech, 1f);
        self.FearedBy(CreatureTemplate.Type.Leech, 1f);
        self.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        self.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
        self.Eats(CreatureTemplate.Type.Snail, 1f);
        self.FearedBy(CreatureTemplate.Type.Snail, 1f);
        self.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        self.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        self.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        self.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        self.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        self.FearedBy(CreatureTemplate.Type.BigNeedleWorm, .5f);
        self.Eats(CreatureTemplate.Type.Fly, 1f);
        self.FearedBy(CreatureTemplate.Type.Fly, 1f);
        self.Eats(CreatureTemplate.Type.Hazer, 1f);
        self.FearedBy(CreatureTemplate.Type.Hazer, 1f);
        self.Eats(CreatureTemplate.Type.VultureGrub, 1f);
        self.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        self.Ignores(CreatureTemplateType.XyloWorm);
        self.IgnoredBy(CreatureTemplateType.XyloWorm);
        self.Eats(CreatureTemplateType.MiniScutigera, 1f);
        self.FearedBy(CreatureTemplateType.MiniScutigera, 1f);
        self.Eats(CreatureTemplateType.WaterBlob, 1f);
        self.FearedBy(CreatureTemplateType.WaterBlob, 1f);
        self.AttackedBy(CreatureTemplate.Type.Scavenger, .5f);
        self.FearedBy(CreatureTemplate.Type.Slugcat, .1f);
        self.FearedBy(CreatureTemplateType.ChipChop, .3f);
        self.FearedBy(CreatureTemplateType.TintedBeetle, .3f);
        self.FearedBy(CreatureTemplateType.Hoverfly, .3f);
        self.FearedBy(CreatureTemplate.Type.LizardTemplate, .1f);
        self.FearedBy(CreatureTemplate.Type.DropBug, .1f);
        self.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        self.FearedBy(CreatureTemplate.Type.EggBug, .3f);
        self.FearedBy(CreatureTemplate.Type.Deer, .1f);
        if (ModManager.DLCShared)
            self.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, .3f);
        if (ModManager.Watcher)
        {
            self.Eats(WatcherEnums.CreatureTemplateType.Frog, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.Frog, 1f);
            self.Eats(WatcherEnums.CreatureTemplateType.Rat, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.Rat, 1f);
            self.Eats(WatcherEnums.CreatureTemplateType.SandGrub, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.SandGrub, 1f);
            self.Ignores(WatcherEnums.CreatureTemplateType.BigSandGrub);
            self.IgnoredBy(WatcherEnums.CreatureTemplateType.BigSandGrub);
            self.FearedBy(WatcherEnums.CreatureTemplateType.SmallMoth, .3f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.Tardigrade, .3f);
        }
    }

	public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Xylo(acrit, acrit.world, LBMergedModsPlugin.NoXyloHoles);

    public override CreatureState CreateState(AbstractCreature acrit) => new HazerMomState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Snail;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        var rand = Random.value;
        if (rand < .1f)
            abstractCreature.superSizeMe = true;
        else if (rand < .2f && AbsProps.TryGetValue(abstractCreature, out var props))
            props.Albino = true;
        Random.state = state;
        return abstractCreature;
    }
}