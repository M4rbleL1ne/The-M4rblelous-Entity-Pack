using System.Collections.Generic;

namespace LBMergedMods.Creatures;

public class M4RTailFlyAI(AbstractCreature creature, World world) : HoverflyAI(creature, world)
{
    public override void ScanForFood(AbstractCreature ow)
    {
        if (HoverflyData.TryGetValue(ow, out var data1) && data1.CanEat && ow.Room is AbstractRoom arm && arm.entities is List<AbstractWorldEntity> l && arm.realizedRoom is Room ro)
        {
            for (var i = 0; i < l.Count; i++)
            {
                if (l[i] is AbstractPhysicalObject obj && obj.SameRippleLayer(ow) && (obj.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || obj.type == AbstractObjectType.LimeMushroom || obj.type == AbstractPhysicalObject.AbstractObjectType.SlimeMold) && obj.realizedObject is PhysicalObject pobj && (ro.GetTile(pobj.firstChunk.pos with { y = pobj.firstChunk.pos.y - 20f })?.Solid is true || data1.CanEatRoot))
                    FoodTracker.AddItem(this.CreateTrackerRepresentationForItem(obj));
            }
        }
    }

    public override bool CanHunt(PhysicalObject obj) => obj is Mushroom or LimeMushroom or SlimeMold;
}