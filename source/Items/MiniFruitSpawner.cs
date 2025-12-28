using RWCustom;
using UnityEngine;

namespace LBMergedMods.Items;

public class MiniFruitSpawner : PhysicalObject
{
    public Vector2 RootPos;

    public override float EffectiveRoomGravity => 0f;

    public override bool SandstormImmune => true;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public MiniFruitSpawner(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, .0001f, .0001f)
        {
            goThroughFloors = true,
            collideWithObjects = false,
            collideWithSlopes = false,
            collideWithTerrain = false
        }];
        bodyChunkConnections = [];
        airFriction = .0001f;
        gravity = .0001f;
        bounce = .0001f;
        surfaceFriction = .0001f;
        burrowFriction = .0001f;
        collisionLayer = 0;
        waterFriction = .0001f;
        buoyancy = .0001f;
    }

    public override void Update(bool eu)
    {
        firstChunk.HardSetPosition(RootPos);
        if (!AbstrCons.isConsumed && MiniFruitSpawners.TryGetValue(AbstrCons, out var prps) && prps.NumberOfFruits == 0)
            AbstrCons.Consume();
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;
        if (MiniFruitSpawners.TryGetValue(AbstrCons, out var props))
        {
            var ind = AbstrCons.placedObjectIndex;
            if (ind >= 0 && ind < placeRoom.roomSettings.placedObjects.Count)
                firstChunk.HardSetPosition(RootPos = props.RootPos);
            else
                firstChunk.HardSetPosition(props.RootPos = RootPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        }
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk) { }

    public override void RemoveGraphicsModule() { }

    public override void DisposeGraphicsModule() { }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override void Grabbed(Creature.Grasp grasp) { }
}