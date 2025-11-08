using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

sealed class CommonEelCritob : Critob, ISandboxHandler
{
    internal CommonEelCritob() : base(CreatureTemplateType.CommonEel)
    {
        Icon = new SimpleIcon("Kill_CommonEel", CommonEel.EelCol);
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 50f;
        RegisterUnlock(KillScore.Configurable(12), SandboxUnlockID.CommonEel);
    }

    public override int ExpeditionScore() => 12;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => CommonEel.EelCol;

    public override string DevtoolsMapName(AbstractCreature acrit) => "cE";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater
    ];

    public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) => paralyzing = true;

    public override IEnumerable<string> WorldFileAliases() => ["commoneel", "common eel"];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var me = new Relationships(Type);
        me.Ignores(Type);
        me.Eats(CreatureTemplate.Type.Slugcat, 1f);
        me.EatenBy(CreatureTemplate.Type.Slugcat, 1f);
        me.Eats(CreatureTemplate.Type.LizardTemplate, 1f);
        me.FearedBy(CreatureTemplate.Type.LizardTemplate, 1f);
        me.Ignores(CreatureTemplate.Type.GreenLizard);
        me.IgnoredBy(CreatureTemplate.Type.GreenLizard);
        me.Ignores(CreatureTemplate.Type.RedLizard);
        me.IgnoredBy(CreatureTemplate.Type.RedLizard);
        me.Ignores(CreatureTemplate.Type.Salamander);
        me.IgnoredBy(CreatureTemplate.Type.Salamander);
        me.Ignores(CreatureTemplate.Type.Fly);
        me.FearedBy(CreatureTemplate.Type.Fly, 1f);
        me.Ignores(CreatureTemplate.Type.Leech);
        me.FearedBy(CreatureTemplate.Type.Leech, .5f);
        me.Ignores(CreatureTemplate.Type.Snail);
        me.FearedBy(CreatureTemplate.Type.Snail, 1f);
        me.Ignores(CreatureTemplate.Type.Vulture);
        me.FearedBy(CreatureTemplate.Type.Vulture, .5f);
        me.Ignores(CreatureTemplate.Type.GarbageWorm);
        me.FearedBy(CreatureTemplate.Type.GarbageWorm, 1f);
        me.Eats(CreatureTemplate.Type.LanternMouse, 1f);
        me.FearedBy(CreatureTemplate.Type.LanternMouse, 1f);
        me.Eats(CreatureTemplate.Type.CicadaA, 1f);
        me.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        me.Eats(CreatureTemplate.Type.CicadaB, 1f);
        me.FearedBy(CreatureTemplate.Type.CicadaB, 1f);
        me.Ignores(CreatureTemplate.Type.Spider);
        me.FearedBy(CreatureTemplate.Type.Spider, 1f);
        me.Ignores(CreatureTemplate.Type.JetFish);
        me.FearedBy(CreatureTemplate.Type.JetFish, 1f);
        me.Fears(CreatureTemplate.Type.BigEel, 1f);
        me.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        me.Ignores(CreatureTemplate.Type.Deer);
        me.IgnoredBy(CreatureTemplate.Type.Deer);
        me.Ignores(CreatureTemplate.Type.TubeWorm);
        me.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
        me.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        me.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        me.Fears(CreatureTemplate.Type.BrotherLongLegs, .5f);
        me.IgnoredBy(CreatureTemplate.Type.BrotherLongLegs);
        me.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        me.EatenBy(CreatureTemplate.Type.TentaclePlant, 1f);
        me.Fears(CreatureTemplate.Type.PoleMimic, 1f);
        me.EatenBy(CreatureTemplate.Type.PoleMimic, 1f);
        me.Fears(CreatureTemplate.Type.MirosBird, 1f);
        me.EatenBy(CreatureTemplate.Type.MirosBird, .5f);
        me.Ignores(CreatureTemplate.Type.TempleGuard);
        me.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        me.Eats(CreatureTemplate.Type.Centipede, 1f);
        me.FearedBy(CreatureTemplate.Type.Centipede, 1f);
        me.Ignores(CreatureTemplate.Type.RedCentipede);
        me.IgnoredBy(CreatureTemplate.Type.RedCentipede);
        me.Eats(CreatureTemplate.Type.Scavenger, 1f);
        me.FearedBy(CreatureTemplate.Type.Scavenger, 1f);
        me.Ignores(CreatureTemplate.Type.Overseer);
        me.FearedBy(CreatureTemplate.Type.Overseer, .5f);
        me.Ignores(CreatureTemplate.Type.VultureGrub);
        me.IgnoredBy(CreatureTemplate.Type.VultureGrub);
        me.Eats(CreatureTemplate.Type.EggBug, 1f);
        me.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        me.Eats(CreatureTemplate.Type.BigSpider, 1f);
        me.FearedBy(CreatureTemplate.Type.BigSpider, 1f);
        me.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        me.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        me.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        me.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        me.Eats(CreatureTemplate.Type.DropBug, 1f);
        me.FearedBy(CreatureTemplate.Type.DropBug, 1f);
        me.Ignores(CreatureTemplate.Type.KingVulture);
        me.IgnoredBy(CreatureTemplate.Type.KingVulture);
        me.Ignores(CreatureTemplate.Type.Hazer);
        me.FearedBy(CreatureTemplate.Type.Hazer, .5f);
        if (ModManager.DLCShared)
        {
            me.Fears(DLCSharedEnums.CreatureTemplateType.MirosVulture, 1f);
            me.EatenBy(DLCSharedEnums.CreatureTemplateType.MirosVulture, 1f);
            me.Ignores(DLCSharedEnums.CreatureTemplateType.SpitLizard);
            me.IgnoredBy(DLCSharedEnums.CreatureTemplateType.SpitLizard);
            me.Ignores(DLCSharedEnums.CreatureTemplateType.EelLizard);
            me.IgnoredBy(DLCSharedEnums.CreatureTemplateType.EelLizard);
            me.Eats(DLCSharedEnums.CreatureTemplateType.MotherSpider, 1f);
            me.FearedBy(DLCSharedEnums.CreatureTemplateType.MotherSpider, 1f);
            me.Ignores(DLCSharedEnums.CreatureTemplateType.AquaCenti);
            me.IgnoredBy(DLCSharedEnums.CreatureTemplateType.AquaCenti);
            me.Fears(DLCSharedEnums.CreatureTemplateType.StowawayBug, 1f);
            me.EatenBy(DLCSharedEnums.CreatureTemplateType.StowawayBug, 1f);
            me.Ignores(DLCSharedEnums.CreatureTemplateType.Inspector);
            me.IgnoredBy(DLCSharedEnums.CreatureTemplateType.Inspector);
            me.Eats(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            me.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            me.Ignores(DLCSharedEnums.CreatureTemplateType.BigJelly);
            me.IgnoredBy(DLCSharedEnums.CreatureTemplateType.BigJelly);
        }
        if (ModManager.MSC)
        {
            me.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1f);
            me.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, .5f);
            me.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
            me.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
            me.Ignores(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard);
            me.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard);
        }
        if (ModManager.Watcher)
            me.Ignores(WatcherEnums.CreatureTemplateType.Angler);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new CommonEelAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new CommonEel(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Salamander;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        if (Random.value < .1f)
            abstractCreature.superSizeMe = true;
        Random.state = state;
        return abstractCreature;
    }
}