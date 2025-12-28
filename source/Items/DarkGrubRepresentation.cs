using DevInterface;
using UnityEngine;
using RWCustom;

namespace LBMergedMods.Items;

public class DarkGrubRepresentation : ConsumableRepresentation
{
    public class DarkGrubControlPanel : ConsumableControlPanel, IDevUISignals
    {
        public Button RootDirButton;

        public DarkGrubControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
        {
            size.y = 65f;
            subNodes.Add(RootDirButton = new(owner, "RootDir_Button", this, new(5f, 45f), 240f, ((parentNode as DarkGrubRepresentation)!.pObj.data as DarkGrubData)!.RootDirText));
        }

        public override void Refresh()
        {
            base.Refresh();
            RootDirButton.Text = ((parentNode as DarkGrubRepresentation)!.pObj.data as DarkGrubData)!.RootDirText;
        }

        public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "RootDir_Button")
            {
                var data = ((parentNode as DarkGrubRepresentation)!.pObj.data as DarkGrubData)!;
                if (data.RootDir == new IntVector2(0, 1))
                    data.RootDir = new IntVector2(0, -1);
                else if (data.RootDir == new IntVector2(0, -1))
                    data.RootDir = new IntVector2(-1, 0);
                else if (data.RootDir == new IntVector2(-1, 0))
                    data.RootDir = new IntVector2(1, 0);
                else
                    data.RootDir = new IntVector2(0, 1);
            }
            Refresh();
        }
    }

    public DarkGrubRepresentation(DevUI owner, DevUINode parentNode, PlacedObject pObj) : base(owner, "DarkGrub_Rep", parentNode, pObj, "DarkGrub")
    {
        controlPanel.size = default;
        controlPanel.Title = string.Empty;
        controlPanel.ToggleCollapse();
        controlPanel.ClearSprites();
        subNodes.RemoveAt(subNodes.Count - 1);
        subNodes.Add(controlPanel = new DarkGrubControlPanel(owner, "Dark_Grub_Panel", this, new(0f, 100f), "Consumable: DarkGrub") { pos = (pObj.data as DarkGrubData)!.panelPos });
    }
}