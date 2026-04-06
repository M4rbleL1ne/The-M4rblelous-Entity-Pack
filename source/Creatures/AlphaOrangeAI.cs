namespace LBMergedMods.Creatures;

public class AlphaOrangeAI : LizardAI
{
	public AlphaOrangeAI(AbstractCreature creature) : base(creature, creature.world) => AddModule(yellowAI = new(this));
}