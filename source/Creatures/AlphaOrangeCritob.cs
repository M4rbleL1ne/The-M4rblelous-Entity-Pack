using System.Collections.Generic;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using Watcher;
using UnityEngine;

namespace LBMergedMods.Creatures;

sealed class AlphaOrangeCritob : Critob
{
	public AlphaOrangeCritob() : base(CreatureTemplateType.AlphaOrange)
	{
		Icon = new SimpleIcon("Kill_Yellow_Lizard", new(1f, 1f / 3f, 0f));
		LoadedPerformanceCost = 60f;
		SandboxPerformanceCost = new(.6f, .6f);
		CreatureName = "AlphaOrange";
		RegisterUnlock(KillScore.Configurable(10), SandboxUnlockID.AlphaOrange);
	}

	public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new AlphaOrangeAI(acrit);

	public override Creature CreateRealizedCreature(AbstractCreature acrit) => new AlphaOrange(acrit, acrit.world);

	public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

	public override string DevtoolsMapName(AbstractCreature acrit) => "AlpOr";

	public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.yellow;

	public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.Lizards];

	public override IEnumerable<string> WorldFileAliases() => ["alphaorange", "alpha orange"];

	public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.YellowLizard;

	public override int ExpeditionScore() => 10;

	public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

	public override void EstablishRelationships()
	{
		var self = new Relationships(Type);
		self.IsInPack(Type, .2f);
		self.IsInPack(CreatureTemplate.Type.YellowLizard, .2f);
        self.Attacks(CreatureTemplateType.Polliwog, .5f);
		self.FearedBy(CreatureTemplateType.Polliwog, .2f);
		if (ModManager.Watcher)
		{
            self.Attacks(WatcherEnums.CreatureTemplateType.PeachLizard, .5f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.PeachLizard, .2f);
        }
    }

	public override void LoadResources(RainWorld rainWorld) { }
}