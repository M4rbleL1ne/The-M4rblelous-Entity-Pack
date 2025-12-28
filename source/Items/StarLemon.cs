using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class StarLemon : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IProvideWarmth, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public StarLemon? Fruit;
        public Vector2[] Displacements;
        public Vector2[][] Segs;
        public Vector2 StuckPos, StartRot;
        public float RopeLength, ConnRad;
        public int ReleaseCounter;
        public Color YellowCol;

        public Stalk(StarLemon fruit, Room room, Vector2 fruitPos)
        {
            Fruit = fruit;
            YellowCol = fruit.YellowCol;
            fruit.firstChunk.HardSetPosition(fruitPos);
            StuckPos.x = fruitPos.x;
            RopeLength = -1f;
            var fpos = Room.StaticGetTilePosition(fruitPos);
            var x = fpos.x;
            int i, height = room.TileHeight;
            for (i = fpos.y; i < height; i++)
            {
                if (room.GetTile(x, i).Solid)
                {
                    StuckPos.y = room.MiddleOfTile(x, i).y - 10f;
                    RopeLength = Math.Abs(StuckPos.y - fruitPos.y);
                    break;
                }
            }
            var segs = Segs = new Vector2[Math.Max(1, (int)(RopeLength / 7.5f))][];
            height = segs.Length;
            var lf = (float)(height - 1);
            var sp = StuckPos;
            for (i = 0; i < segs.Length; i++)
            {
                var seg = segs[i] = new Vector2[3];
                seg[0] = Vector2.Lerp(sp, fruitPos, i / lf);
                seg[1] = seg[0];
            }
            ConnRad = RopeLength / Mathf.Pow(height, 1.1f);
            var disps = Displacements = new Vector2[height];
            var state = Random.state;
            Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
            for (i = 0; i < disps.Length; i++)
                disps[i] = Custom.RNV();
            StartRot = Custom.DegToVec(Random.Range(-40f, 40f));
            Random.state = state;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (RopeLength == -1f)
            {
                if (Fruit is StarLemon fruit1)
                    fruit1.MyStalk = null;
                Destroy();
                return;
            }
            ConnectSegments(true);
            ConnectSegments(false);
            var segs = Segs;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                seg[1] = seg[0];
                seg[0] += seg[2];
                seg[2] *= .99f;
                seg[2].y -= .9f;
            }
            ConnectSegments(false);
            ConnectSegments(true);
            if (ReleaseCounter > 0)
                --ReleaseCounter;
            if (Fruit is StarLemon fruit)
            {
                var fpos = fruit.firstChunk.pos;
                fruit.SetRotation = Custom.DirVec(fpos, segs[segs.Length - 1][0]) + StartRot;
                if (!Custom.DistLess(fpos, StuckPos, RopeLength * 1.8f + 10f) || fruit.slatedForDeletetion || fruit.Bites < 6 || fruit.room != room || ReleaseCounter == 1)
                {
                    fruit.AbstrCons.Consume();
                    fruit.MyStalk = null;
                    Fruit = null;
                }
            }
        }

        public virtual void ConnectSegments(bool dir)
        {
            var segs = Segs;
            var sp = StuckPos;
            var crad = ConnRad;
            var l = segs.Length;
            var num = dir ? 0 : l - 1;
            var flag = false;
            while (!flag)
            {
                var seg = segs[num];
                if (num == 0)
                {
                    if (!Custom.DistLess(seg[0], sp, crad))
                    {
                        var vector = Custom.DirVec(seg[0], sp) * (Vector2.Distance(seg[0], sp) - crad);
                        seg[0] += vector;
                        seg[2] += vector;
                    }
                }
                else
                {
                    var segm1 = segs[num - 1];
                    if (!Custom.DistLess(seg[0], segm1[0], crad))
                    {
                        var vector2 = Custom.DirVec(seg[0], segm1[0]) * (Vector2.Distance(seg[0], segm1[0]) - crad) * .5f;
                        seg[0] += vector2;
                        seg[2] += vector2;
                        segm1[0] -= vector2;
                        segm1[2] -= vector2;
                    }
                    if (num == l - 1 && Fruit is StarLemon fruit && !Custom.DistLess(segm1[0], fruit.firstChunk.pos, crad))
                    {
                        var fc = fruit.firstChunk;
                        Vector2 vector3 = Custom.DirVec(seg[0], fc.pos) * (Vector2.Distance(seg[0], fc.pos) - crad);
                        seg[0] += vector3 * .75f;
                        seg[2] += vector3 * .75f;
                        fc.vel -= vector3 * .25f;
                    }
                }
                num += dir ? 1 : -1;
                if (dir && num >= l)
                    flag = true;
                else if (!dir && num < 0)
                    flag = true;
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segs.Length, false, true)];
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var vector = StuckPos;
            var num = 6f;
            var mesh = (TriangleMesh)sLeaser.sprites[0];
            var segs = Segs;
            var l = segs.Length - 1;
            var crad = ConnRad;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                float num2 = i / (float)l,
                    num3 = Custom.LerpMap(num2, 0f, .5f, 1f, 0f) + Mathf.Lerp(1f, .5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * Mathf.PI)) * 4f;
                Vector2 vector2;
                if (i == l && Fruit is StarLemon fruit)
                    vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
                else
                    vector2 = Vector2.Lerp(seg[1], seg[0], timeStacker);
                Vector2 normalized = (vector - vector2).normalized,
                    vector3 = Custom.PerpendicularVector(normalized);
                if (i < l)
                {
                    var disp = Displacements[i];
                    vector2 += (normalized * disp.y + vector3 * disp.x) * Custom.LerpMap(Vector2.Distance(vector, vector2), crad, crad * 5f, 4f, 0f);
                }
                vector2 = new Vector2(Mathf.Floor(vector2.x) + .5f, Mathf.Floor(vector2.y) + .5f);
                var i4 = i * 4;
                mesh.MoveVertice(i4, vector - vector3 * num - camPos);
                mesh.MoveVertice(i4 + 1, vector + vector3 * num - camPos);
                mesh.MoveVertice(i4 + 2, vector2 - vector3 * num3 - camPos);
                mesh.MoveVertice(i4 + 3, vector2 + vector3 * num3 - camPos);
                vector = vector2;
                var vertL = mesh.verticeColors.Length;
                if (i4 + 1 < vertL)
                {
                    mesh.verticeColors[i4] = Color.Lerp(rCam.currentPalette.blackColor, YellowCol, .2f * (i * 2f / vertL));
                    mesh.verticeColors[i4 + 1] = Color.Lerp(rCam.currentPalette.blackColor, YellowCol, .4f * (i * 2f / vertL));
                    if (i4 + 3 < vertL)
                    {
                        mesh.verticeColors[i4 + 2] = Color.Lerp(rCam.currentPalette.blackColor, YellowCol, .2f * ((i * 2f + 1f) / vertL));
                        mesh.verticeColors[i4 + 3] = Color.Lerp(rCam.currentPalette.blackColor, YellowCol, .4f * ((i * 2f + 1f) / vertL));
                    }
                }
                num = num3;
            }
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var mesh = (TriangleMesh)sLeaser.sprites[0];
            mesh.color = palette.blackColor;
            var segsL = Segs.Length;
            var vertL = mesh.verticeColors.Length;
            for (var i = 0; i < segsL; i++)
            {
                var i4 = i * 4;
                if (i4 + 1 < vertL)
                {
                    mesh.verticeColors[i4] = Color.Lerp(palette.blackColor, YellowCol, .2f * (i * 2f / vertL));
                    mesh.verticeColors[i4 + 1] = Color.Lerp(palette.blackColor, YellowCol, .4f * (i * 2f / vertL));
                    if (i4 + 3 < vertL)
                    {
                        mesh.verticeColors[i4 + 2] = Color.Lerp(palette.blackColor, YellowCol, .2f * ((i * 2f + 1f) / vertL));
                        mesh.verticeColors[i4 + 3] = Color.Lerp(palette.blackColor, YellowCol, .4f * ((i * 2f + 1f) / vertL));
                    }
                }
            }
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            var spr = sLeaser.sprites[0];
            spr.RemoveFromContainer();
            (newContainer ?? rCam.ReturnFContainer("Background")).AddChild(spr);
        }
    }

    public Stalk? MyStalk;
    public LightSource? Light, FlatLight;
    public float Darkness, LastDarkness, LightDarkness;
    public int Bites = 6;
    public Color YellowCol;
    public Vector2 Rotation, LastRotation;
    public Vector2? SetRotation;
    public bool Side;

    public virtual float warmth => RainWorldGame.DefaultHeatSourceWarmth;

    public virtual Room loadedRoom => room;

    public virtual float range => 360f;

    public virtual AbstractConsumable AbstrCons => (AbstractConsumable)abstractPhysicalObject;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 2;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public virtual bool StalkActive => MyStalk is not null;

    public StarLemon(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 8.5f, .3f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .98f;
        bounce = .35f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = .4f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        YellowCol = new(1f, Mathf.Lerp(191f / 255f, 242f / 255f, Random.value), 0f);
        Side = Random.value > .5f;
        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fc.vel += Custom.DirVec(fc.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        var crits = room.abstractRoom.creatures;
        for (var i = 0; i < crits.Count; i++)
        {
            var cr = crits[i];
            if (cr.realizedCreature is Creature c && c.Consious && c.mainBodyChunk is BodyChunk ch && Custom.DistLess(ch.pos, fc.pos, 70f) && (c.Template.type == CreatureTemplate.Type.BigSpider || c.Template.type == CreatureTemplate.Type.Spider || c.Template.type?.value == "MaracaSpider" || c is MiniLeech))
                ch.vel += Custom.DirVec(fc.pos, ch.pos) * 6f;
        }
        LastRotation = Rotation;
        if (grabbedBy.Count > 0)
        {
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Math.Abs(Rotation.y);
        }
        if (SetRotation.HasValue)
        {
            Rotation = SetRotation.Value;
            SetRotation = null;
        }
        if (fc.ContactPoint.y < 0)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
        }
        LightDarkness = room.Darkness(fc.pos);
        if (Light is LightSource lh)
        {
            lh.stayAlive = true;
            lh.setPos = fc.pos;
            lh.setRad = 280f;
            lh.setAlpha = Bites / 6f * .6f + .4f;
            lh.color = YellowCol;
            if (lh.slatedForDeletetion || LightDarkness == 0f)
                Light = null;
        }
        else if (LightDarkness > 0f)
            room.AddObject(Light = new(fc.pos, false, YellowCol, this) { requireUpKeep = true });
        if (FlatLight is LightSource lh2)
        {
            lh2.stayAlive = true;
            lh2.setPos = fc.pos;
            lh2.setRad = 35f;
            lh2.setAlpha = .25f;
            lh2.color = YellowCol;
            if (lh2.slatedForDeletetion || LightDarkness == 0f)
                FlatLight = null;
        }
        else if (LightDarkness > 0f)
            room.AddObject(FlatLight = new(fc.pos, false, YellowCol, this) { flat = true, requireUpKeep = true });
    }

    public override void Destroy()
    {
        if (Light is not null)
        {
            Light.Destroy();
            Light = null;
        }
        if (FlatLight is not null)
        {
            FlatLight.Destroy();
            FlatLight = null;
        }
        base.Destroy();
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta is not null) && room.game.GetArenaGameSession.counter < 10)
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            placeRoom.AddObject(MyStalk = new(this, placeRoom, firstChunk.pos));
        }
        else if (!AbstrCons.isConsumed && AbstrCons.placedObjectIndex >= 0 && AbstrCons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrCons.placedObjectIndex].pos);
            placeRoom.AddObject(MyStalk = new(this, placeRoom, firstChunk.pos));
        }
        else
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            Rotation = Custom.RNV();
            LastRotation = Rotation;
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = Random.Range(30, 50);
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var scaleX = Side ? .8f : -.8f;
        var scaleX2 = Side ? .7f : -.7f;
        sLeaser.sprites =
        [
            new("StarLemonGraf0") { scaleX = scaleX2, scaleY = .7f },
            new("StarLemonGrad0") { scaleX = scaleX2, scaleY = .7f, alpha = .5f },
            new("StarLemonGraf0") { scaleX = scaleX, scaleY = .8f },
            new("StarLemonGrad0") { scaleX = scaleX, scaleY = .8f },
            new("StarLemonGradi0") { scaleX = scaleX, scaleY = .8f, alpha = .8f },
            new("StarLemonGraf0") { scaleX = scaleX, scaleY = .8f, shader = Custom.rainWorld.Shaders["StarLemonBloom"] }
        ];
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
            v = Vector3.Slerp(LastRotation, Rotation, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var sprite = sprs[i];
            sprite.x = pos.x - camPos.x;
            sprite.y = pos.y - camPos.y;
            sprite.rotation = Custom.VecToDeg(v);
            if (i < 2)
                sprite.rotation += Side ? -30f : 30f;
            sprite.element = Futile.atlasManager.GetElementWithName("StarLemon" + (i is 0 or 2 or 5 ? "Graf" : (i is 1 or 3 ? "Grad" : "Gradi")) + Custom.IntClamp(6 - Bites, 0, 5));
        }
        sprs[5].isVisible = firstChunk.submersion < .5f;
        if (blink > 0 && Random.value < .5f)
            sprs[4].color = sprs[3].color = sprs[1].color = blinkColor;
        else
        {
            sprs[4].color = rCam.currentPalette.blackColor;
            sprs[3].color = sprs[1].color = color;
        }
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[2].color = sLeaser.sprites[0].color = palette.blackColor;
        color = YellowCol;
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        var sprs = sLeaser.sprites;
        newContainer ??= rCam.ReturnFContainer("Items");
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            if (i < 5)
                newContainer.AddChild(spr);
            else
                rCam.ReturnFContainer("Bloom").AddChild(spr);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        room.PlaySound(Bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            if (grasp.grabber is Player p)
            {
                p.ObjectEaten(this);
                p.glowing = true;
            }
            else if (grasp.grabber is ChipChop ch)
                ch.Glowing = true;
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer() { }

    public virtual Vector2 Position() => firstChunk.pos;

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = 2;
    }
}