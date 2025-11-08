namespace LBMergedMods.Creatures;

public class WaterSpitterAI : LizardAI
{
    public WaterSpitterAI(AbstractCreature creature, World world) : base(creature, world) => AddModule(redSpitAI = new(this));

    public override void Update()
    {
        if (lizard?.Submersion >= 1f && redSpitAI is LizardSpitTracker t)
        {
            t.wantToSpit = false;
            t.spitting = false;
        }
        base.Update();
        if (lizard is WaterSpitter li)
        {
            noiseTracker.hearingSkill = 1.6f;
            if (redSpitAI is LizardSpitTracker tr && tr.spitting && li.animation != Lizard.Animation.Spit)
            {
                tr.delay = 0;
                li.voice.MakeSound(LizardVoice.Emotion.BloodLust);
                li.EnterAnimation(Lizard.Animation.Spit, false);
                li.bubble = 10;
                li.bubbleIntensity = 1f;
            }
        }
    }
}