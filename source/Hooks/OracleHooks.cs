﻿global using static LBMergedMods.Hooks.OracleHooks;

namespace LBMergedMods.Hooks;

public static class OracleHooks
{
    internal static void On_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        orig(self);
        if (self.id == Conversation.ID.Moon_Misc_Item)
        {
            var item = self.describeItem;
            if (item == MiscItemType.ThornyStrawberry)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a delicious fruit but remove the thorns before eating it."), 0));
            else if (item == MiscItemType.BlobPiece)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a glob of something gelatinous... It's making my skin tingle."), 0));
            else if (item == MiscItemType.LittleBalloon)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's an unstable plant. Be careful when you throw it!"), 0));
            else if (item == MiscItemType.BouncingMelon)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a large green fruit.<LINE>It's not very nutritious but it could have other effects."), 0));
            else if (item == MiscItemType.Physalis)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a tasty little fruit. Do not confuse it with a pearl!"), 0));
            else if (item == MiscItemType.LimeMushroom)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a poisonous mushroom, don't eat it! It even scares away some creatures."), 0));
            else if (item == MiscItemType.GummyAnther)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a plant anther. You can eat it safely!"), 0));
            else if (item == MiscItemType.MarineEye)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a strange edible flower filled with a blue liquid."), 0));
            else if (item == MiscItemType.StarLemon)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a very big shiny fruit, how did you bring it here?"), 0));
            else if (item == MiscItemType.SporeProjectile)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's the egg of a creature.<LINE>Although its membrane is thin, it is covered with a fungus whose spores keep predators away."), 0));
            else if (item == MiscItemType.DendriticNeuron)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("What have you brought me this time?"), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("Ah..."), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("Thank you small creature."), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("I am sincerely grateful for this, however in my current predicament<LINE>I lack the processing strata required to interface with this type of neuron."), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("Their morphology, in addition to being more imposing,<LINE>uses vastly more complex signaling than the ones you see before me."), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("Although, the gesture is appreciated..."), 0));
            }
            else if (item == MiscItemType.MiniBlueFruit)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("WIP"), 0));
        }
    }

    internal static void On_SLOracleBehaviorHasMark_CreatureJokeDialog(On.SLOracleBehaviorHasMark.orig_CreatureJokeDialog orig, SLOracleBehaviorHasMark self)
    {
        orig(self);
        var type = self.CheckStrayCreatureInRoom();
        if (type == CreatureTemplateType.RedHorrorCenti)
            self.dialogBox.NewMessage(self.Translate("Oh, that is not a friend..."), 10);
        else if (type == CreatureTemplateType.FatFireFly)
            self.dialogBox.NewMessage(self.Translate("It is on fire! How did you bring that in here?!"), 10);
        else if (type == CreatureTemplateType.FlyingBigEel || type == CreatureTemplateType.Blizzor)
            self.dialogBox.NewMessage(self.Translate("Your friend is very large, how did you fit them in here?"), 10);
    }

    internal static SLOracleBehaviorHasMark.MiscItemType On_SLOracleBehaviorHasMark_TypeOfMiscItem(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem)
    {
        if (testItem is ThornyStrawberry)
            return MiscItemType.ThornyStrawberry;
        if (testItem is BlobPiece)
            return MiscItemType.BlobPiece;
        if (testItem is LittleBalloon)
            return MiscItemType.LittleBalloon;
        if (testItem is BouncingMelon)
            return MiscItemType.BouncingMelon;
        if (testItem is Physalis)
            return MiscItemType.Physalis;
        if (testItem is LimeMushroom)
            return MiscItemType.LimeMushroom;
        if (testItem is GummyAnther)
            return MiscItemType.GummyAnther;
        if (testItem is MarineEye)
            return MiscItemType.MarineEye;
        if (testItem is StarLemon)
            return MiscItemType.StarLemon;
        if (testItem is SmallPuffBall)
            return MiscItemType.SporeProjectile;
        if (testItem is DendriticNeuron)
            return MiscItemType.DendriticNeuron;
        if (testItem is MiniFruit)
            return MiscItemType.MiniBlueFruit;
        return orig(self, testItem);
    }

    internal static void On_SSOracleBehavior_CreatureJokeDialog(On.SSOracleBehavior.orig_CreatureJokeDialog orig, SSOracleBehavior self)
    {
        orig(self);
        var type = self.CheckStrayCreatureInRoom();
        if (type == CreatureTemplateType.RedHorrorCenti || type == CreatureTemplateType.FlyingBigEel || type == CreatureTemplateType.Blizzor)
            self.dialogBox.NewMessage(self.Translate("How did you fit them inside here anyhow?"), 10);
        else if (type == CreatureTemplateType.FatFireFly)
            self.dialogBox.NewMessage(self.Translate("Your friend is on fire! Take it with you, please!"), 10);
    }
}