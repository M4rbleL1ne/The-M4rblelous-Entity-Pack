using DevInterface;
using UnityEngine;
using RWCustom;

namespace LBMergedMods.Items;

public class MiniFruitSpawnerRepresentation : ConsumableRepresentation
{
    public class MiniFruitSpawnerControlPanel : ConsumableControlPanel //, IDevUISignals // removal intended
    {
        public class FoodAmountSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : Slider(owner, IDstring, parentNode, pos, title, false, 110f)
        {
            public override void Refresh()
            {
                base.Refresh();
                var data = ((parentNode.parentNode as MiniFruitSpawnerRepresentation)!.pObj.data as MiniFruitSpawnerData)!;
                NumberText = data.FoodAmount.ToString();
                RefreshNubPos(data.FoodAmount / 10f);
            }

            public override void NubDragged(float nubPos)
            {
                ((parentNode.parentNode as MiniFruitSpawnerRepresentation)!.pObj.data as MiniFruitSpawnerData)!.FoodAmount = (int)(nubPos * 10f);
                parentNode.parentNode.Refresh();
                Refresh();
            }
        }

        //public Button FoodChanceButton; // removal intended

        public MiniFruitSpawnerControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
        {
            size.y = 65f;
            subNodes.Add(new FoodAmountSlider(owner, "FoodAmount_Slider", this, new(5f, 45f), "Food Amount: "));
            //subNodes.Add(FoodChanceButton = new(owner, "FoodChance_Button", this, new(5f, 45f), 240f, ((parentNode as MiniFruitSpawnerRepresentation)!.pObj.data as MiniFruitSpawnerData)!.FoodChance ? "Food Chance: Yes" : "Food Chance: No")); // removal intended
        }

        /*public override void Refresh()
        {
            base.Refresh();
            FoodChanceButton.Text = ((parentNode as MiniFruitSpawnerRepresentation)!.pObj.data as MiniFruitSpawnerData)!.FoodChance ? "Food Chance: Yes" : "Food Chance: No"; // removal intended
        }*/

        /*public virtual void Signal(DevUISignalType type, DevUINode sender, string message) // removal intended
        {
            if (sender.IDstring == "FoodChance_Button")
            {
                var data = ((parentNode as MiniFruitSpawnerRepresentation)!.pObj.data as MiniFruitSpawnerData)!;
                data.FoodChance = !data.FoodChance;
            }
            Refresh();
        }*/
    }

    public MiniFruitSpawnerRepresentation(DevUI owner, DevUINode parentNode, PlacedObject pObj) : base(owner, "MiniFruitSpawner_Rep", parentNode, pObj, "Mini Fruit Branch")
    {
        controlPanel.size = default;
        controlPanel.Title = string.Empty;
        controlPanel.ToggleCollapse();
        controlPanel.ClearSprites();
        subNodes.RemoveAt(subNodes.Count - 1);
        subNodes.Add(controlPanel = new MiniFruitSpawnerControlPanel(owner, "MiniFruitSpawner_Panel", this, new(0f, 100f), "Consumable: Mini Fruit Branch") { pos = (pObj.data as MiniFruitSpawnerData)!.panelPos });
        subNodes.Add(new Handle(owner, "Rad_Handle", this, new(0f, 100f)) { pos = (pObj.data as MiniFruitSpawnerData)!.HandlePos });
        subNodes.Add(new Handle(owner, "Root_Handle", this, new(-50f, 50f)) { pos = (pObj.data as MiniFruitSpawnerData)!.RootHandlePos });
        FSprite spr;
        var cont = owner.placedObjectsContainer;
        fSprites.Add(spr = new("Futile_White") { shader = Custom.rainWorld.Shaders["VectorCircle"] });
        cont.AddChild(spr);
        fSprites.Add(spr = new("pixel") { anchorY = 0f });
        cont.AddChild(spr);
        fSprites.Add(spr = new("pixel") { anchorY = 0f });
        cont.AddChild(spr);
    }

    public override void Refresh()
    {
        var sb = subNodes;
        for (var num = sb.Count - 1; num >= 0; num--)
            sb[num].Refresh();
        MoveSprite(0, absPos);
        MoveLabel(0, absPos + new Vector2(20f, 20f));
        var data = (pObj.data as MiniFruitSpawnerData)!;
        FSprite spr;
        MoveSprite(fSprites.Count - 4, absPos);
        spr = fSprites[fSprites.Count - 4];
        spr.scaleY = (data.panelPos = controlPanel.pos).magnitude;
        spr.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        MoveSprite(fSprites.Count - 3, absPos);
        var handle = (subNodes[subNodes.Count - 2] as Handle)!;
        var mag = (data.HandlePos = handle.pos).magnitude;
        spr = fSprites[fSprites.Count - 3];
        spr.scale = mag / 8f;
        spr.alpha = 2f / mag;
        MoveSprite(fSprites.Count - 2, absPos);
        spr = fSprites[fSprites.Count - 2];
        spr.scaleY = mag;
        spr.rotation = Custom.AimFromOneVectorToAnother(absPos, handle.absPos);
        MoveSprite(fSprites.Count - 1, absPos);
        spr = fSprites[fSprites.Count - 1];
        handle = (subNodes[subNodes.Count - 1] as Handle)!;
        spr.scaleY = (data.RootHandlePos = handle.pos).magnitude;
        spr.rotation = Custom.AimFromOneVectorToAnother(absPos, handle.absPos);
    }
}