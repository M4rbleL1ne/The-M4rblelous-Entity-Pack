using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class MoleSalamanderCritob : Critob
{
    internal MoleSalamanderCritob() : base(CreatureTemplateType.MoleSalamander)
    {
        Icon = new SimpleIcon("Kill_MoleSalamander", new(.368627459f, .368627459f, 37f / 85f));
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 50f;
        RegisterUnlock(KillScore.Configurable(7), SandboxUnlockID.MoleSalamander);
    }

    public override int ExpeditionScore() => 7;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.1f, .1f, .1f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "MSal";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Dark,
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.LikesInside
    ];

    public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) => paralyzing = true;

    public override IEnumerable<string> WorldFileAliases() => ["molesalamander", "mole salamander"];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var m = new Relationships(Type);
        m.Rivals(Type, .05f);
        m.Rivals(CreatureTemplate.Type.LizardTemplate, .1f);
        m.Rivals(CreatureTemplate.Type.BlackLizard, .05f);
        m.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);
        m.Fears(CreatureTemplate.Type.Vulture, .9f);
        m.Fears(CreatureTemplate.Type.KingVulture, 1f);
        m.Eats(CreatureTemplate.Type.TubeWorm, .025f);
        m.Eats(CreatureTemplate.Type.Scavenger, .8f);
        m.Eats(CreatureTemplate.Type.CicadaA, .05f);
        m.Eats(CreatureTemplate.Type.LanternMouse, .3f);
        m.Eats(CreatureTemplate.Type.BigSpider, .35f);
        m.Eats(CreatureTemplate.Type.EggBug, .45f);
        m.Eats(CreatureTemplate.Type.JetFish, .5f);
        m.Fears(CreatureTemplate.Type.BigEel, 1f);
        m.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        m.Eats(CreatureTemplate.Type.Centipede, .8f);
        m.Eats(CreatureTemplate.Type.BigNeedleWorm, .25f);
        m.Eats(CreatureTemplate.Type.SmallNeedleWorm, .3f);
        m.Eats(CreatureTemplate.Type.DropBug, .2f);
        m.Fears(CreatureTemplate.Type.RedCentipede, .9f);
        m.Fears(CreatureTemplate.Type.TentaclePlant, .2f);
        m.Eats(CreatureTemplate.Type.Hazer, .15f);
        m.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);
        m.EatenBy(CreatureTemplate.Type.Vulture, .5f);
        m.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        m.FearedBy(CreatureTemplate.Type.JetFish, .5f);
        m.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        m.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        m.FearedBy(CreatureTemplate.Type.Scavenger, .5f);
        m.EatenBy(CreatureTemplate.Type.BigSpider, .3f);
        m.IgnoredBy(CreatureTemplate.Type.Leech);
        m.Ignores(CreatureTemplate.Type.Leech);
        if (ModManager.Watcher)
        {
            m.Ignores(WatcherEnums.CreatureTemplateType.Angler);
            m.IgnoredBy(WatcherEnums.CreatureTemplateType.Angler);
        }
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new MoleSalamanderAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MoleSalamander(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Salamander;
}