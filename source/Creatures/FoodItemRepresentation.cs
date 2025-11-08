using UnityEngine;

namespace LBMergedMods.Creatures;

public class FoodItemRepresentation(Tracker parent, AbstractPhysicalObject representedItem, float priority, bool forgetWhenNotVisible)
{
    public Tracker Parent = parent;
    public AbstractPhysicalObject RepresentedItem = representedItem;
    public int ForgetCounter, Age;
    public bool VisualContact = true, GoneToOtherRoomSense = true;
    public int TicksSinceSeen;
    public bool DeleteMeNextFrame, ForgetWhenNotVisible = forgetWhenNotVisible;
    public float Priority = priority;
    public WorldCoordinate LastSeenCoord = representedItem.pos.WashNode();

    public virtual bool GetVisualContact
    {
        get
        {
            if (RepresentedItem.realizedObject is not PhysicalObject o || o.room != Parent.AI.creature.Room.realizedRoom || o is Player { IsHidden: true, VisibilityBonus: <= -1f })
                VisualContact = false;
            return VisualContact;
        }
    }

    public virtual int GetTicksSinceSeen => Mathf.Max(TicksSinceSeen - Parent.seeAroundCorners, 0);

    public virtual int LowestGenerationAvailable => 1;

    public virtual float EstimatedChanceOfFinding
    {
        get
        {
            var num = (LowestGenerationAvailable * 10f + GetTicksSinceSeen) / 4f;
            if (VisualContact)
                return 1f;
            if (num < 45f)
                return Mathf.Clamp(1f / (0f - (1f + Mathf.Pow(Mathf.Epsilon, 0f - (num / 12f - 5f)))) + 1.007f, 0f, 1f);
            return 1f / (num - 7f) * 30f;
        }
    }

    public virtual void HeardThisCreature()
    {
        var seeAC = Parent.seeAroundCorners;
        if (TicksSinceSeen > seeAC + 5)
            TicksSinceSeen = (TicksSinceSeen - seeAC) / 2;
    }

    public void SetVisualContact(bool val) => VisualContact = val;

    public virtual void MakeVisible()
    {
        TicksSinceSeen = 0;
        VisualContact = true;
    }

    public virtual void Update()
    {
        var AI = Parent.AI;
        var crit = AI.creature;
        var critRoom = crit.pos.room;
        var repRoom = RepresentedItem.pos.room;
        ++Age;
        bool flag = VisualContact;
        ++TicksSinceSeen;
        if (TicksSinceSeen > Parent.seeAroundCorners)
        {
            VisualContact = false;
            if (!RepresentedItem.InDen && repRoom == critRoom && RepresentedItem.realizedObject is PhysicalObject obj)
            {
                var bodyChunks = obj.bodyChunks;
                for (var i = 0; i < bodyChunks.Length; i++)
                {
                    if (AI.VisualContact(bodyChunks[i]))
                    {
                        TicksSinceSeen = 0;
                        VisualContact = true;
                        break;
                    }
                }
            }
        }
        if (VisualContact)
        {
            if (RepresentedItem.realizedObject is null || repRoom != critRoom)
            {
                VisualContact = false;
                return;
            }
            LastSeenCoord = RepresentedItem.pos.WashNode();
            if (!flag)
                Parent.AI.FoodItemSpotted(false, this);
            ForgetCounter = 0;
            return;
        }
        ++ForgetCounter;
        if (GoneToOtherRoomSense && repRoom != LastSeenCoord.room && RepresentedItem.realizedObject?.room is not null)
        {
            SenseThatCreatureHasLeftRoom();
            GoneToOtherRoomSense = false;
        }
        var frms = Parent.framesToRememberCreatures;
        if (frms > -1 && ForgetCounter > frms || ForgetWhenNotVisible && !GetVisualContact)
            Destroy();
    }

    public virtual WorldCoordinate BestGuessForPosition() => LastSeenCoord;

    public virtual void SenseThatCreatureHasLeftRoom() { }

    public virtual void Destroy() => DeleteMeNextFrame = true;
}