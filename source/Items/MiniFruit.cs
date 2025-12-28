using MoreSlugcats;
using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class MiniFruit : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public MiniFruit? Fruit;
        public Vector2[][] Segments;
        public Vector2 RootPos, Direction, FruitPos;
        public bool Kill;

        public Stalk(MiniFruit fruit, Vector2? rootPos, Room room)
        {
            Fruit = fruit;
            FruitPos = fruit.firstChunk.pos;
            base.room = room;
            if (rootPos.HasValue)
                RootPos = rootPos.Value;
            else
            {
                var tilePosition = Room.StaticGetTilePosition(FruitPos);
                while (tilePosition.y < room.TileHeight && !room.GetTile(tilePosition).Solid)
                    ++tilePosition.y;
                if (tilePosition.y == room.TileHeight)
                    Kill = true;
                RootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, 10f);
            }
            var segs = Segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(FruitPos, RootPos) / 15f), 4, 60)][];
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = Vector2.Lerp(RootPos, FruitPos, (float)i / segs.Length);
                segs[i] = [seg, seg, default];
            }
            Direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(FruitPos.x + FruitPos.y))));
            for (var j = 0; j < 100; j++)
                Update(false);
            fruit.ChangeCollisionLayer(0);
        }

        public override void Update(bool eu)
        {
            if (Kill)
            {
                if (Fruit is MiniFruit f1)
                    f1.MyStalk = null;
                Destroy();
                return;
            }
            base.Update(eu);
            var segments = Segments;
            for (var i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                seg[1] = seg[0];
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                else if (i == segments.Length - 1)
                {
                    seg[0] = FruitPos;
                    seg[2] *= 0f;
                }
                else
                {
                    seg[0] += seg[2];
                    seg[2] *= .7f;
                    seg[2] += .3f * Custom.DirVec(RootPos, FruitPos);
                    seg[2] += Direction * .4f * (1f - (i + 1f) / segments.Length);
                }
                if (i < segments.Length - 1)
                {
                    var segip1 = segments[i + 1];
                    var normalized = (seg[0] - segip1[0]).normalized;
                    var num2 = Vector2.Distance(seg[0], segip1[0]);
                    seg[0] += normalized * (15f - num2) * .5f;
                    seg[2] += normalized * (15f - num2) * .5f;
                    segip1[0] -= normalized * (15f - num2) * .5f;
                    segip1[2] -= normalized * (15f - num2) * .5f;
                }
                if (i < segments.Length - 2)
                {
                    Vector2 normalized2 = (seg[0] - segments[i + 2][0]).normalized;
                    seg[2] += normalized2 * 1.5f;
                    segments[i + 2][2] -= normalized2 * 1.5f;
                }
                if (i == 0)
                {
                    seg[0] = RootPos;
                    seg[2] *= 0f;
                }
                else if (i == segments.Length - 1)
                {
                    var d = Custom.Dist(seg[0], FruitPos);
                    if (d > 2.5f)
                        seg[0] += Custom.DirVec(seg[0], FruitPos) * (d - 2.5f);
                    seg[0].y += 2f;
                }
                if (Custom.DistLess(seg[1], seg[0], 10f))
                    seg[1] = seg[0];
            }
            if (Fruit is MiniFruit f)
            {
                var chunk = f.firstChunk;
                var ps = FruitPos + Custom.DirVec(FruitPos, segments[segments.Length - 1][0]) * 10f;
                if (!Custom.DistLess(ps, chunk.pos, f.grabbedBy.Count == 0 ? 40f : 4f) || f.room != room || f.slatedForDeletetion || chunk.vel.magnitude > 15f)
                {
                    chunk.mass = .1f;
                    f.ChangeCollisionLayer(1);
                    f.AbstrCons.Consume();
                    f.MyStalk = null;
                    Fruit = null;
                }
                else
                {
                    chunk.vel.y += f.gravity;
                    chunk.vel *= .6f;
                    chunk.vel += (ps - chunk.pos) / 20f;
                    chunk.mass = 50f;
                    f.SetRotation = Custom.DirVec(chunk.pos, segments[segments.Length - 2][0]);
                }
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segments.Length, false, false)];
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var segments = Segments;
            var segment = segments[0];
            var vector = Vector2.Lerp(segment[1], segment[0], timeStacker);
            var s0 = (TriangleMesh)sLeaser.sprites[0];
            for (var i = 0; i < segments.Length; i++)
            {
                segment = segments[i];
                Vector2 vector2 = Vector2.Lerp(segment[1], segment[0], timeStacker),
                    normalized = (vector2 - vector).normalized * (Vector2.Distance(vector2, vector) / 4f),
                    vector3 = Custom.PerpendicularVector(normalized) * .8f;
                var i4 = i * 4;
                s0.MoveVertice(i4, vector - vector3 + normalized - camPos);
                s0.MoveVertice(i4 + 1, vector + vector3 + normalized - camPos);
                s0.MoveVertice(i4 + 2, vector2 - vector3 - normalized - camPos);
                s0.MoveVertice(i4 + 3, vector2 + vector3 - normalized - camPos);
                vector = vector2;
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[0].color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");
            var sprite = sLeaser.sprites[0];
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
    }

    public Stalk? MyStalk;
    public float Darkness, LastDarkness;
    public Vector2 Rotation, LastRotation;
	public Vector2? SetRotation;

	public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

	public virtual int BitesLeft => 1;

	public virtual int FoodPoints => room?.game?.session is ArenaGameSession ? 1 : 0;

	public virtual bool Edible => true;

	public virtual bool AutomaticPickUp => true;

    public virtual bool StalkActive => MyStalk is not null;

    public MiniFruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
	{
		bodyChunks = [new(this, 0, default, 3f, .1f)];
		bodyChunkConnections = [];
		airFriction = .999f;
		gravity = .9f;
		bounce = .2f;
		surfaceFriction = .7f;
		collisionLayer = 1;
		waterFriction = .95f;
		buoyancy = 1.1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		var fc = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fc.vel += Custom.DirVec(fc.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        LastRotation = Rotation;
		if (grabbedBy.Count > 0)
		{
			Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			Rotation.y = Math.Abs(Rotation.y);
		}
		if (SetRotation is Vector2 v)
		{
			Rotation = v;
			SetRotation = null;
		}
		if (fc.ContactPoint.y < 0)
		{
			Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
			fc.vel.x *= .8f;
		}
		var crits = room.abstractRoom.creatures;
        if (fc.submersion > .5f && crits.Count > 0 && grabbedBy.Count == 0)
		{
			var crit = crits[Random.Range(0, crits.Count)];
			if (abstractPhysicalObject.SameRippleLayer(crit) && crit.creatureTemplate.type == CreatureTemplate.Type.JetFish && crit.realizedCreature is JetFish j && !j.dead && j.AI.goToFood is null && j.AI.WantToEatObject(this))
				j.AI.goToFood = this;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
        if (MiniFruits.TryGetValue(AbstrCons, out var fprops))
        {
            if (!AbstrCons.isConsumed && fprops.Spawner is AbstractConsumable cons && cons.placedObjectIndex >= 0 && cons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count && MiniFruitSpawners.TryGetValue(cons, out var props))
            {
                firstChunk.HardSetPosition(fprops.FruitPos);
                placeRoom.AddObject(MyStalk = new(this, props.RootPos, placeRoom));
            }
            else if (ModManager.MMF && room.game.session is ArenaGameSession sess && (MMF.cfgSandboxItemStems.Value || sess.chMeta is not null) && sess.counter < 10)
            {
                firstChunk.HardSetPosition(fprops.FruitPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
                placeRoom.AddObject(MyStalk = new(this, null, placeRoom));
                return;
            }
            else
            {
                firstChunk.HardSetPosition(fprops.FruitPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
                LastRotation = Rotation = Custom.RNV();
            }
        }
        else
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            LastRotation = Rotation = Custom.RNV();
        }
    }

	public override void HitByWeapon(Weapon weapon)
	{
        firstChunk.mass = .1f;
        ChangeCollisionLayer(1);
        base.HitByWeapon(weapon);
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = [new("DangleFruit0A") { scale = .7071f, anchorY = .35f }, new("DangleFruit0B") { scale = .7071f, anchorY = .35f }];
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
			rot = Vector3.Slerp(LastRotation, Rotation, timeStacker);
		LastDarkness = Darkness;
		Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (Darkness != LastDarkness)
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		var sprs = sLeaser.sprites;
		for (var i = 0; i < sprs.Length; i++)
		{
			var sprite = sprs[i];
			sprite.SetPosition(pos - camPos);
            sprite.rotation = Custom.VecToDeg(rot);
			if (i == 1)
                sprite.color = blink > 0 && Random.value < .5f ? blinkColor : color;
        }
		if (slatedForDeletetion || room != rCam.room)
			sLeaser.CleanSpritesAndRemove();
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
        color = Color.Lerp(ModManager.MSC && rCam.room.game.session is StoryGameSession && rCam.room.world.name == "HR" ? RainWorld.SaturatedGold : Color.blue, palette.blackColor, Darkness);
    }

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
	{
		newContainer ??= rCam.ReturnFContainer("Items");
		var sprs = sLeaser.sprites;
		for (var i = 0; i < sprs.Length; i++)
		{
			var sprite = sprs[i];
			sprite.RemoveFromContainer();
			newContainer.AddChild(sprite);
        }
	}

	public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk);
		firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        (grasp.grabber as Player)?.ObjectEaten(this);
        grasp.Release();
        Destroy();
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        firstChunk.mass = .1f;
        ChangeCollisionLayer(1);
        base.Grabbed(grasp);
    }

    public virtual void ThrowByPlayer() { }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st)
        {
            if (st.Fruit is MiniFruit fr)
            {
                fr.AbstrCons.Consume();
                st.Fruit = null;
            }
            MyStalk = null;
        }
    }
}