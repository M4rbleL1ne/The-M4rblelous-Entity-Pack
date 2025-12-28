using DevInterface;
using System;
using System.Globalization;
using UnityEngine;
using RWCustom;

namespace LBMergedMods.Items;

public class ScoreTokenRepresentation : ResizeableObjectRepresentation
{
    public class IDController : PositionedDevUINode, IDevUISignals
    {
        public DevUILabel NumberLabel;
        public int Number;

        public virtual string NumberLabelText
        {
            get => NumberLabel.fLabels[0].text;
            set => NumberLabel.fLabels[0].text = value;
        }

        public IDController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int number) : base(owner, IDstring, parentNode, pos)
        {
            subNodes.Add(new DevUILabel(owner, "Title", this, default, 110f, title));
            subNodes.Add(NumberLabel = new(owner, "Number", this, new(140f, 0f), 80f, number.ToString()));
            subNodes.Add(new ArrowButton(owner, "Less", this, new(120f, 0f), -90f));
            subNodes.Add(new ArrowButton(owner, "More", this, new(224f, 0f), 90f));
            Number = number;
        }

        public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "Less")
                Increment(Input.GetKey(KeyCode.LeftShift) ? -10 : -1);
            else if (sender.IDstring == "More")
                Increment(!Input.GetKey(KeyCode.LeftShift) ? 1 : 10);
        }

        public virtual void Increment(int change)
        {
            if (owner?.game?.overWorld?.activeWorld?.name is string s)
            {
                Number = Math.Max(Number + change, 0);
                ((parentNode.parentNode as ScoreTokenRepresentation)!.pObj.data as ScoreTokenData)!.ID = s + "_" + Number;
                Refresh();
            }
        }

        public override void Refresh()
        {
            var ar = ((parentNode.parentNode as ScoreTokenRepresentation)!.pObj.data as ScoreTokenData)!.ID.Split('_');
            if (ar.Length >= 2)
                NumberLabelText = ar[1].ToString();
            base.Refresh();
        }
    }

    public class ScoreController : PositionedDevUINode, IDevUISignals
    {
        public DevUILabel NumberLabel;
        public int Number;

        public virtual string NumberLabelText
        {
            get => NumberLabel.fLabels[0].text;
            set => NumberLabel.fLabels[0].text = value;
        }

        public ScoreController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int number) : base(owner, IDstring, parentNode, pos)
        {
            subNodes.Add(new DevUILabel(owner, "Title", this, default, 110f, title));
            subNodes.Add(NumberLabel = new(owner, "Number", this, new(140f, 0f), 80f, number.ToString()));
            subNodes.Add(new ArrowButton(owner, "Less", this, new(120f, 0f), -90f));
            subNodes.Add(new ArrowButton(owner, "More", this, new(224f, 0f), 90f));
            Number = number;
        }

        public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "Less")
                Increment(Input.GetKey(KeyCode.LeftShift) ? -10 : -1);
            else if (sender.IDstring == "More")
                Increment(!Input.GetKey(KeyCode.LeftShift) ? 1 : 10);
        }

        public virtual void Increment(int change)
        {
            ((parentNode.parentNode as ScoreTokenRepresentation)!.pObj.data as ScoreTokenData)!.Score = Number = Math.Max(Number + change, 0);
            Refresh();
        }

        public override void Refresh()
        {
            NumberLabelText = ((parentNode.parentNode as ScoreTokenRepresentation)!.pObj.data as ScoreTokenData)!.Score.ToString();
            base.Refresh();
        }
    }

    public class TokenControlPanel : Panel, IDevUISignals
    {
        public Button[] SlugcatButtons;

        public ScoreTokenData Data => ((parentNode as ScoreTokenRepresentation)!.pObj.data as ScoreTokenData)!;

        public TokenControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new(250f, 45f), "Bonus Score Token")
        {
            subNodes.Add(new ScoreController(owner, "Score", this, new(5f, 5f), "Score : ", Data.Score));
            var ar = Data.ID.Split('_');
            var n = 0;
            if (ar.Length >= 2)
                int.TryParse(ar[1], NumberStyles.Any, CultureInfo.InvariantCulture, out n);
            subNodes.Add(new IDController(owner, "ID", this, new(5f, 25f), "ID : ", n));
            var bts = SlugcatButtons = new Button[SlugcatStats.Name.values.Count];
            var psX = 0f;
            for (var i = 0; i < bts.Length; i++)
            {
                subNodes.Add(bts[i] = new(owner, "Button_" + i, this, new(5f + psX, 45f + 20f * Mathf.Floor(i / 2f)), 115f, string.Empty));
                if (psX > 0f)
                {
                    psX = 0f;
                    size.y += 20f;
                }
                else
                    psX = 125f;
            }
            UpdateButtonText();
        }

        public virtual void UpdateButtonText()
        {
            var bts = SlugcatButtons;
            for (var i = 0; i < bts.Length; i++)
            {
                var name = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
                bts[i].Text = Data.UnavailableToPlayers.Contains(new(name)) ? "--" : name;
            }
        }

        public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            var bts = SlugcatButtons;
            for (var i = 0; i < bts.Length; i++)
            {
                if (bts[i] == sender)
                {
                    var name = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
                    var list = Data.UnavailableToPlayers;
                    var nm = new SlugcatStats.Name(name);
                    if (list.Contains(nm))
                        list.Remove(nm);
                    else
                        list.Add(nm);
                }
            }
            UpdateButtonText();
        }
    }

    public int LineSprite;

    public ScoreTokenRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj) : base(owner, IDstring, parentNode, pObj, "Bonus Score Token", false)
    {
        subNodes.Add(new TokenControlPanel(owner, "Token_Panel", this, new(0f, 100f))
        {
            pos = (pObj.data as ScoreTokenData)!.PanelPos
        });
        fSprites.Add(new("pixel") { anchorY = 0f });
        LineSprite = fSprites.Count - 1;
        owner.placedObjectsContainer.AddChild(fSprites[LineSprite]);
    }

    public override void Refresh()
    {
        base.Refresh();
        MoveSprite(LineSprite, absPos);
        var panel = (subNodes[1] as TokenControlPanel)!;
        var spr = fSprites[LineSprite];
        spr.scaleY = panel.pos.magnitude;
        spr.rotation = Custom.AimFromOneVectorToAnother(absPos, panel.absPos);
        (pObj.data as ScoreTokenData)!.PanelPos = panel.pos;
    }
}