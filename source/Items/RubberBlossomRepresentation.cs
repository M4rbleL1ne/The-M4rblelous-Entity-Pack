using DevInterface;
using UnityEngine;

namespace LBMergedMods.Items;

public class RubberBlossomRepresentation : ConsumableRepresentation
{
    public class RubberBlossomControlPanel : ConsumableControlPanel, IDevUISignals
    {
        public class RubberBlossomSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : Slider(owner, IDstring, parentNode, pos, title, false, 110f)
        {
            public override void Refresh()
            {
                base.Refresh();
                var nubPos = 0f;
                var data = ((parentNode.parentNode as RubberBlossomRepresentation)!.pObj.data as RubberBlossomData)!;
                switch (IDstring)
                {
                    case "FoodAmount_Slider":
                        nubPos = data.FoodAmount / 3f;
                        NumberText = data.FoodAmount.ToString();
                        break;
                    case "CyclesOpen_Slider":
                        nubPos = (data.CyclesOpen - 1) / 9f;
                        NumberText = data.CyclesOpen.ToString();
                        break;
                    case "CyclesClosed_Slider":
                        nubPos = (data.CyclesClosed - 1) / 9f;
                        NumberText = data.CyclesClosed.ToString();
                        break;
                    case "Red_Slider":
                        nubPos = data.Red;
                        NumberText = ((int)(data.Red * 255f)).ToString();
                        break;
                    case "Green_Slider":
                        nubPos = data.Green;
                        NumberText = ((int)(data.Green * 255f)).ToString();
                        break;
                    case "Blue_Slider":
                        nubPos = data.Blue;
                        NumberText = ((int)(data.Blue * 255f)).ToString();
                        break;
                    case "MaxUpwardVel_Slider":
                        nubPos = (data.MaxUpwardVel - 2f) / 48f;
                        NumberText = ((int)data.MaxUpwardVel).ToString();
                        break;

                }
                RefreshNubPos(nubPos);
            }

            public override void NubDragged(float nubPos)
            {
                var data = ((parentNode.parentNode as RubberBlossomRepresentation)!.pObj.data as RubberBlossomData)!;
                switch (IDstring)
                {
                    case "FoodAmount_Slider":
                        data.FoodAmount = (int)(nubPos * 3f);
                        break;
                    case "CyclesOpen_Slider":
                        data.CyclesOpen = (int)(nubPos * 9f) + 1;
                        if (!data.StartsOpen)
                        {
                            data.maxRegen = data.CyclesOpen + 1;
                            data.minRegen = data.RandomOpen ? 2 : data.maxRegen;
                        }
                        break;
                    case "CyclesClosed_Slider":
                        data.CyclesClosed = (int)(nubPos * 9f) + 1;
                        if (data.StartsOpen)
                        {
                            data.maxRegen = data.CyclesClosed + 1;
                            data.minRegen = data.RandomClosed ? 2 : data.maxRegen;
                        }
                        break;
                    case "Red_Slider":
                        data.Red = nubPos;
                        break;
                    case "Green_Slider":
                        data.Green = nubPos;
                        break;
                    case "Blue_Slider":
                        data.Blue = nubPos;
                        break;
                    case "MaxUpwardVel_Slider":
                        data.MaxUpwardVel = nubPos * 48f + 2f;
                        break;
                }
                parentNode.parentNode.Refresh();
                Refresh();
            }
        }

        public Button StartsOpenButton, FoodChanceButton, RandomOpenButton, RandomClosedButton, AlwaysOpenButton, AlwaysClosedButton;

        public RubberBlossomControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
        {
            size.y = 265f;
            subNodes[subNodes.Count - 1].ClearSprites();
            subNodes.RemoveAt(subNodes.Count - 1);
            subNodes[subNodes.Count - 1].ClearSprites();
            subNodes.RemoveAt(subNodes.Count - 1);
            var data = ((parentNode as RubberBlossomRepresentation)!.pObj.data as RubberBlossomData)!;
            subNodes.Add(AlwaysOpenButton = new(owner, "AlwaysOpen_Button", this, new(5f, 245f), 240f, data.AlwaysOpen && !data.AlwaysClosed ? "Always Open: Yes" : "Always Open: No"));
            subNodes.Add(AlwaysClosedButton = new(owner, "AlwaysClosed_Button", this, new(5f, 225f), 240f, data.AlwaysClosed && !data.AlwaysOpen ? "Always Closed: Yes" : "Always Closed: No"));
            subNodes.Add(new RubberBlossomSlider(owner, "MaxUpwardVel_Slider", this, new(5f, 205f), "Max Upward Vel: "));
            subNodes.Add(new RubberBlossomSlider(owner, "Red_Slider", this, new(5f, 185f), "Red: "));
            subNodes.Add(new RubberBlossomSlider(owner, "Green_Slider", this, new(5f, 165f), "Green: "));
            subNodes.Add(new RubberBlossomSlider(owner, "Blue_Slider", this, new(5f, 145f), "Blue: "));
            subNodes.Add(new RubberBlossomSlider(owner, "CyclesOpen_Slider", this, new(5f, 125f), "Cycles Open: "));
            subNodes.Add(RandomOpenButton = new(owner, "RandomOpen_Button", this, new(5f, 105f), 240f, data.RandomOpen ? "Random Open: Yes" : "Random Open: No"));
            subNodes.Add(new RubberBlossomSlider(owner, "CyclesClosed_Slider", this, new(5f, 85f), "Cycles Closed: "));
            subNodes.Add(RandomClosedButton = new(owner, "RandomClosed_Button", this, new(5f, 65f), 240f, data.RandomClosed ? "Random Closed: Yes" : "Random Closed: No"));
            subNodes.Add(new RubberBlossomSlider(owner, "FoodAmount_Slider", this, new(5f, 45f), "Food Amount: "));
            subNodes.Add(FoodChanceButton = new(owner, "FoodChance_Button", this, new(5f, 25f), 240f, data.FoodChance ? "Food Chance: Yes" : "Food Chance: No"));
            subNodes.Add(StartsOpenButton = new(owner, "StartState_Button", this, new(5f, 5f), 240f, data.StartsOpen ? "Start State: Open" : "Start State: Closed"));
        }

        public override void Refresh()
        {
            base.Refresh();
            var data = ((parentNode as RubberBlossomRepresentation)!.pObj.data as RubberBlossomData)!;
            StartsOpenButton.Text = data.StartsOpen ? "Start State: Open" : "Start State: Closed";
            FoodChanceButton.Text = data.FoodChance ? "Food Chance: Yes" : "Food Chance: No";
            RandomClosedButton.Text = data.RandomClosed ? "Random Closed: Yes" : "Random Closed: No";
            RandomOpenButton.Text = data.RandomOpen ? "Random Open: Yes" : "Random Open: No";
            AlwaysClosedButton.Text = data.AlwaysClosed && !data.AlwaysOpen ? "Always Closed: Yes" : "Always Closed: No";
            AlwaysOpenButton.Text = data.AlwaysOpen && !data.AlwaysClosed ? "Always Open: Yes" : "Always Open: No";
        }

        public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            var data = ((parentNode as RubberBlossomRepresentation)!.pObj.data as RubberBlossomData)!;
            switch (sender.IDstring)
            {
                case "StartState_Button":
                    data.StartsOpen = !data.StartsOpen;
                    if (data.StartsOpen)
                    {
                        data.maxRegen = data.CyclesClosed + 1;
                        data.minRegen = data.RandomClosed ? 2 : data.maxRegen;
                    }
                    else
                    {
                        data.maxRegen = data.CyclesOpen + 1;
                        data.minRegen = data.RandomOpen ? 2 : data.maxRegen;
                    }
                    break;
                case "FoodChance_Button":
                    data.FoodChance = !data.FoodChance;
                    break;
                case "RandomClosed_Button":
                    data.RandomClosed = !data.RandomClosed;
                    if (data.StartsOpen)
                        data.minRegen = data.RandomClosed ? 2 : data.maxRegen;
                    break;
                case "RandomOpen_Button":
                    data.RandomOpen = !data.RandomOpen;
                    if (!data.StartsOpen)
                        data.minRegen = data.RandomOpen ? 2 : data.maxRegen;
                    break;
                case "AlwaysClosed_Button":
                    data.AlwaysClosed = !data.AlwaysClosed;
                    if (data.AlwaysClosed)
                        data.AlwaysOpen = false;
                    break;
                case "AlwaysOpen_Button":
                    data.AlwaysOpen = !data.AlwaysOpen;
                    if (data.AlwaysOpen)
                        data.AlwaysClosed = false;
                    break;
            }
            Refresh();
        }
    }

    public RubberBlossomRepresentation(DevUI owner, DevUINode parentNode, PlacedObject pObj) : base(owner, "RubberBlossom_Rep", parentNode, pObj, "RubberBlossom")
    {
        controlPanel.size = default;
        controlPanel.Title = string.Empty;
        controlPanel.ToggleCollapse();
        controlPanel.ClearSprites();
        subNodes.RemoveAt(subNodes.Count - 1);
        subNodes.Add(controlPanel = new RubberBlossomControlPanel(owner, "Rubber_Blossom_Panel", this, new(0f, 100f), "Consumable: RubberBlossom") { pos = (pObj.data as RubberBlossomData)!.panelPos });
    }
}