using RWCustom;
using System.Globalization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class BlobPiece : PlayerCarryableItem, IPlayerEdible, IDrawable
{
    public class AbstractBlobPiece(World world, PhysicalObject? obj, WorldCoordinate pos, EntityID ID, float color) : AbstractPhysicalObject(world, Enums.AbstractObjectType.BlobPiece, obj, pos, ID)
    {
        public float Color = color;

        public override string ToString() => SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}", IDAndRippleLayerString, type.ToString(), pos.SaveToString(), Color.ToString()), "<oA>", unrecognizedAttributes);
    }

    public float Prop, LastProp, PropSpeed, Darkness, LastDarkness, Plop, LastPlop;
    public int Bites;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public BlobPiece(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        Bites = 2;
        bodyChunks = [new(this, 0, default, 6.5f, .34f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .4f;
        surfaceFriction = .95f;
        collisionLayer = 1;
        waterFriction = .91f;
        buoyancy = 1.2f;
        Prop = 0f;
        LastProp = 0f;
        Plop = 1f;
        LastPlop = 1f;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm)
            return;
        var fch = firstChunk;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
            fch.vel += Custom.DirVec(fch.pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 3f;
        if (fch.ContactPoint.y < 0)
            fch.vel.x *= .8f;
        LastProp = Prop;
        Prop += PropSpeed;
        PropSpeed *= .85f;
        PropSpeed -= Prop / 10f;
        Prop = Mathf.Clamp(Prop, -15f, 15f);
        if (grabbedBy?.Count == 0)
        {
            Prop += (fch.lastPos.x - fch.pos.x) / 15f;
            Prop -= (fch.lastPos.y - fch.pos.y) / 15f;
        }
        LastPlop = Plop;
        if (Plop > 0f && Plop < 1f)
            Plop = Mathf.Min(1f, Plop + .1f);
        var crits = rm.abstractRoom.creatures;
        if (fch.submersion > .5f && crits.Count > 0 && grabbedBy?.Count == 0)
        {
            var abstractCreature = crits[Random.Range(0, crits.Count)];
            if (abstractPhysicalObject.SameRippleLayer(abstractCreature) && abstractCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish && abstractCreature.realizedCreature is JetFish c && !c.dead && c.AI is JetFishAI ai && ai.goToFood is null && ai.WantToEatObject(this))
                ai.goToFood = this;
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (direction.y != 0)
        {
            Prop += speed;
            PropSpeed += speed / 10f;
        }
        else
        {
            Prop -= speed;
            PropSpeed -= speed / 10f;
        }
        if (speed > 1.2f && firstContact && room is Room rm)
        {
            var pos = firstChunk.pos + direction.ToVector2() * firstChunk.rad;
            for (var i = 0; i < Mathf.RoundToInt(Custom.LerpMap(speed, 1.2f, 6f, 2f, 5f, 1.2f)); i++)
                rm.AddObject(new WaterDrip(pos, Custom.RNV() * (2f + speed) * Random.value * .5f + -direction.ToVector2() * (3f + speed) * .35f, true));
            rm.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, pos, Custom.LerpMap(speed, 1.2f, 6f, .2f, 1f), 1f);
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("Futile_White") { shader = rCam.game.rainWorld.Shaders["WaterNut"] }];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        LastDarkness = Darkness;
        Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
        if (Darkness != LastDarkness)
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        var s0 = sLeaser.sprites[0];
        s0.x = pos.x - camPos.x;
        s0.y = pos.y - camPos.y;
        s0.alpha = (1f - Darkness * .25f) * (1f - firstChunk.submersion * .25f);
        var num = Mathf.Lerp(LastPlop, Plop, timeStacker);
        num = Mathf.Lerp(0f, 1f + Mathf.Sin(num * Mathf.PI), num);
        s0.scaleX = (1.2f * Custom.LerpMap(Bites, 2f, 1f, .8f, .6f) * 1f + Mathf.Lerp(LastProp, Prop, timeStacker) / 20f) * num;
        s0.scaleY = (1.2f * Custom.LerpMap(Bites, 2f, 1f, .8f, .6f) * 1f - Mathf.Lerp(LastProp, Prop, timeStacker) / 20f) * num;
        if (blink > 0 && Random.value < .5f)
            s0.color = Color.white;
        else
            s0.color = color;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (abstractPhysicalObject is AbstractBlobPiece a)
        {
            color = Color.Lerp(palette.waterColor1, palette.waterColor2, a.Color);
            sLeaser.sprites[0].color = Color.Lerp(palette.waterColor1, palette.waterColor2, a.Color);
        }
        else
        {
            color = Color.Lerp(palette.waterColor1, palette.waterColor2, .5f);
            sLeaser.sprites[0].color = Color.Lerp(palette.waterColor1, palette.waterColor2, .5f);
        }
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        var spr = sLeaser.sprites[0];
        spr.RemoveFromContainer();
        rCam.ReturnFContainer("GrabShaders").AddChild(spr);
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        room?.PlaySound(Bites != 0 ? SoundID.Slugcat_Bite_Water_Nut : SoundID.Slugcat_Eat_Water_Nut, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
        PropSpeed += Mathf.Lerp(-1f, 1f, Random.value) * 7f;
        firstChunk.rad = Mathf.InverseLerp(3f, 0f, Bites) * 9.5f;
    }

    public virtual void ThrowByPlayer() { }
}