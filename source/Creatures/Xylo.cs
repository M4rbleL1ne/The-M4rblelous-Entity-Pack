using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class Xylo : Creature
{
    public const float BASE_RAD = 70f;
    public PlacedObject? PlacedObj;
    public Color EffectColor;
    public Vector2 RootPos;
    public float Lerper, RadUp;
    public int Worms, RottenWorms;
    public bool LerpUp = true, NoHolesMode;

    public new HazerMomState State => (base.State as HazerMomState)!;

    public override bool SandstormImmune => true;

    public Xylo(AbstractCreature abstractCreature, World world, bool noHolesMode = false) : base(abstractCreature, world)
    {
        NoHolesMode = noHolesMode;
        bodyChunks = [new(this, 0, default, BASE_RAD, 3f)];
        bodyChunkConnections = [];
        abstractCreature.tentacleImmune = true;
        abstractCreature.HypothermiaImmune = true;
        abstractCreature.lavaImmune = true;
        GoThroughFloors = true;
        airFriction = .99f;
        gravity = 0f;
        bounce = .01f;
        surfaceFriction = .47f;
        collisionLayer = 1;
        waterFriction = .94f;
        buoyancy = .01f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        RottenWorms = Random.Range(4, 6);
        Worms = Random.Range(3, 6);
        EffectColor = abstractCreature.Albino() ? Color.Lerp(new(1f, 193f / 255f, 0f), new(236f / 255f, 1f, 0f), Random.value) : (abstractCreature.superSizeMe ? new(Random.Range(102f / 255f, 139f / 255f), 6f / 255f, 6f / 255f) : new(Random.Range(82f / 255f, 119f / 255f), 3f / 255f, 252f / 255f));
        Random.state = state;
        baseCurrentAffectiveness = 0f;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new XyloGraphics(this);

    public virtual void Swallow(PhysicalObject obj)
    {
        room?.PlaySound(NewSoundID.M4R_Xylo_Swallow, firstChunk);
        if (obj is Creature cr)
        {
            cr.killTag = abstractCreature;
            cr.Die();
        }
        if (obj is Weapon w)
        {
            if (w is ExplosiveSpear s)
                s.exploded = true;
            killTag = w.thrownBy?.abstractCreature;
            State.health -= .225f;
        }
        obj.Destroy();
        if (graphicsModule is XyloGraphics gr)
            gr.LightUp = .5f;
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (otherObject is not null && CanBeSwallowed(otherObject))
            Swallow(otherObject);
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (weapon is not null && CanBeSwallowed(weapon))
            Swallow(weapon);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm)
            return;
        var speed = .1f * (2.5f - State.ClampedHealth * 1.5f);
        var fch = firstChunk;
        var objs = rm.physicalObjects;
        for (var i = 0; i < objs.Length; i++)
        {
            var list = objs[i];
            for (var j = 0; j < list.Count; j++)
            {
                if (list[j] is PhysicalObject obj && obj.firstChunk is BodyChunk b && Denture.DistLess(b.pos, fch.pos, b.rad + fch.rad + 5f) && CanBeSwallowed(obj))
                    Swallow(obj);
            }
        }
        Lerper = Mathf.Clamp(Lerper + (LerpUp ? speed * 2.5f : -speed), -2f, 1f);
        if (Lerper >= 1f)
        {
            rm.PlaySound(NewSoundID.M4R_Xylo_Swell, fch);
            LerpUp = false;
        }
        else if (Lerper <= -2f)
            LerpUp = true;
        fch.rad = BASE_RAD * Mathf.Clamp(1f + Lerper * .1f, 1f, 1.25f);
        RadUp = BASE_RAD * Mathf.Clamp(1f + Lerper * .15f, 1f, 1.8f);
        fch.HardSetPosition(RootPos);
        if (!dead && State.health < 0f)
            Die();
    }

    public override void Stun(int st) { }

    public virtual bool CanBeSwallowed(PhysicalObject item) => 
        (item is Creature c && Template.CreatureRelationship(c).type == CreatureTemplate.Relationship.Type.Eats) ||
        (item.bodyChunks.Length == 1 && item.firstChunk.rad <= 10f && item is not KarmaFlower and not FlyLure and not FirecrackerPlant and not BubbleGrass and not SingularityBomb and not JokeRifle and not VultureMask and not MoonCloak and not EnergyCell and not Pomegranate and not DendriticNeuron and not StarLemon && (item is not Lantern l || l.stick is null) && item.grabbedBy?.Count == 0);

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk) { }

    public override void HeardNoise(InGameNoise noise) { }

    public override bool AllowableControlledAIOverride(MovementConnection.MovementType movementType) => false;

    public override void Blind(int blnd) { }

    public override bool CanBeGrabbed(Creature grabber) => false;

    public override void Deafen(int df) { }

    public override void Die()
    {
        base.Die();
        if (PlacedObj?.data is PlacedObject.ConsumableObjectData dt && grabbedBy.Count > 0)
        {
            if (room?.game?.session is StoryGameSession sess)
                sess.saveState.ReportConsumedItem(room.world, false, State.OrigRoom, State.PlacedObjectIndex, Random.Range(dt.minRegen, dt.maxRegen));
            PlacedObj = null;
        }
        if (room is Room rm)
        {
            var vector = firstChunk.pos;
            rm.AddObject(new SootMark(rm, vector, 400f, true));
            rm.AddObject(new Explosion(rm, this, vector, 5, 250f, 14f, 1.6f, 80f, .6f, this, .8f, 0f, .8f));
            for (var i = 0; i < 14; i++)
                rm.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * Random.value, 1f));
            rm.AddObject(new Explosion.ExplosionLight(vector, 250f, 1f, 3, EffectColor));
            rm.AddObject(new ExplosionSpikes(rm, vector, 9, 4f, 7f, 12f, 150f, EffectColor));
            rm.AddObject(new ShockWave(vector, 150f, .25f, 5));
            for (var j = 0; j < 25; j++)
            {
                var vector2 = Custom.RNV();
                rm.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), EffectColor, null, 4, 18));
            }
            rm.ScreenMovement(vector, default, 1f);
            rm.PlaySound(SoundID.Bomb_Explode, firstChunk);
            AbstractCreature abstractWorm;
            var wormTpl = StaticWorld.GetCreatureTemplate(CreatureTemplateType.XyloWorm);
            XyloWorm worm;
            for (; Worms > 0; Worms--)
            {
                rm.abstractRoom.AddEntity(abstractWorm = new(rm.world, wormTpl, null, abstractPhysicalObject.pos, rm.game.GetNewID()));
                abstractWorm.RealizeInRoom();
                worm = (abstractWorm.realizedObject as XyloWorm)!;
                worm.firstChunk.HardSetPosition(rm.MiddleOfTile(vector));
            }
            rm.abstractRoom.AddEntity(abstractWorm = new(rm.world, wormTpl, null, abstractPhysicalObject.pos, rm.game.GetNewID())
            {
                superSizeMe = true
            });
            abstractWorm.RealizeInRoom();
            worm = (abstractWorm.realizedObject as XyloWorm)!;
            worm.firstChunk.HardSetPosition(rm.MiddleOfTile(vector));
            for (; RottenWorms > 0; RottenWorms--)
            {
                rm.abstractRoom.AddEntity(abstractWorm = new(rm.world, wormTpl, null, abstractPhysicalObject.pos, rm.game.GetNewID()));
                abstractWorm.RealizeInRoom();
                worm = (abstractWorm.realizedObject as XyloWorm)!;
                if (AbsProps.TryGetValue(abstractWorm, out var box))
                    box.Albino = true;
                worm.Rotten = true;
                worm.firstChunk.HardSetPosition(rm.MiddleOfTile(vector));
            }
        }
        Destroy();
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus) { }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction) => false;

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying) => false;

    public override void Grabbed(Grasp grasp) => grasp?.Release();

    public override void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief) { }

    public override void LoseAllGrasps() { }

    public override Color ShortCutColor() => default;

    public override void ReleaseGrasp(int grasp) { }

    public override void PlaceInRoom(Room placeRoom)
    {
        placeRoom.AddObject(this);
        NewRoom(placeRoom);
        var state = State;
        if (state.OrigRoom > -1 && state.OrigRoom == placeRoom.abstractRoom.index && state.PlacedObjectIndex >= 0 && state.PlacedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            PlacedObj = placeRoom.roomSettings.placedObjects[state.PlacedObjectIndex];
            firstChunk.HardSetPosition(RootPos = PlacedObj.pos);
        }
        else
            firstChunk.HardSetPosition(RootPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk) { }
}