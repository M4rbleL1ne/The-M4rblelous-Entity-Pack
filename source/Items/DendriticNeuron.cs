using CoralBrain;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class DendriticNeuron : PhysicalObject, IDrawable, IPlayerEdible, IOwnProjectedCircles, IOwnMycelia
{
    public sealed class MovementMode(string value, bool register = false) : ExtEnum<MovementMode>(value, register)
    {
        public static readonly MovementMode Swarm = new(nameof(Swarm), true),
            FollowPath = new(nameof(FollowPath), true);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Behavior
    {
        public readonly Vector2 Color, AltColor, AltColor2;
        public readonly float IdealDistance, AimInFront, Torque, RandomVibrations, RevolveSpeed;

        public Behavior()
        {
            IdealDistance = Mathf.Lerp(10f, 300f, Random.value * Random.value);
            Color = new(Random.Range(0, 3) / 2f, Random.value < .75f ? 0f : 1f);
            AltColor = new((Color.x + .5f) % 1.5f, Random.value < .75f ? 0f : 1f);
            AltColor2 = new(Math.Abs((Color.x - .5f) % 1.5f), Random.value < .75f ? 0f : 1f);
            AimInFront = Mathf.Lerp(40f, 300f, Random.value);
            Torque = Random.value < .5f ? 0f : Mathf.Lerp(-1f, 1f, Random.value);
            RandomVibrations = Random.value * Random.value * Random.value * .25f;
            RevolveSpeed = (Random.value < .5f ? -.5f : .5f) / Mathf.Lerp(15f, 25f, Random.value);
        }
    }

    public CoralNeuronSystem? System;
    public List<Vector2> StuckList;
    public MovementMode Mode = MovementMode.Swarm;
    public CreatureTemplate FlyTemplate;
    public ProjectedCircle? Circle;
    public Mycelium[] Mycelia;
    public float[] MyceliaDisplace;
    public Vector2 TravelDirection, MyColor;
    public float Torque, Size = 3f, Rotation, LastRotation, RevolveSpeed, AffectedByGravity = 1f, Lerper;
    public int ListBreakPoint, StuckListCounter, DestNode, Bites = 5, Ping;
    public Vector2 Drift, Direction, LastDirection, MyceliaDir;
    public Behavior CurrentBehavior;
    public bool LerpUp;

    public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

    public virtual bool Edible => grabbedBy.Count == 0 || grabbedBy[0].grabber is not Player p || p.FoodInStomach < p.MaxFoodInStomach;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => 1;

    public virtual bool AutomaticPickUp => false;

    public virtual Room OwnerRoom => room;

    public DendriticNeuron(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        var state = Random.state;
        collisionLayer = 1;
        bodyChunks = [new(this, 0, default, 3.5f, .25f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = 0f;
        bounce = .4f;
        surfaceFriction = .4f;
        waterFriction = .98f;
        buoyancy = .02f;
        Rotation = .25f;
        LastRotation = Rotation;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        TravelDirection = Custom.RNV();
        CurrentBehavior = new();
        var mycelia = Mycelia = new Mycelium[Random.Range(6, 10)];
        var mlg = MyceliaDisplace = new float[mycelia.Length];
        for (var l = 0; l < mycelia.Length; l++)
        {
            mycelia[l] = new(null, this, l, Mathf.Lerp(70f, 90f, Random.value), firstChunk.pos)
            {
                color = Color.white,
                useStaticCulling = false
            };
        }
        if (mlg.Length >= 8)
        {
            mlg[7] = -1f;
            mlg[6] = 1f;
        }
        mlg[0] = -.25f;
        mlg[1] = .25f;
        mlg[2] = -.5f;
        mlg[3] = .5f;
        mlg[4] = -.75f;
        mlg[5] = .75f;
        Random.state = state;
        MyColor = CurrentBehavior.Color;
        StuckList = [];
        FlyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        var fc = firstChunk;
        fc.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        var mycelia = Mycelia;
        for (var l = 0; l < mycelia.Length; l++)
            mycelia[l].Reset(fc.pos);
        if (placeRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
        {
            if (Circle is null)
                placeRoom.AddObject(Circle = new(placeRoom, this, 0, 0f));
            else if (Circle.room != placeRoom)
            {
                Circle.Destroy();
                placeRoom.AddObject(Circle = new(placeRoom, this, 0, 0f));
            }
        }
        else if (Circle is not null)
        {
            Circle.Destroy();
            Circle = null;
        }
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        if (grabbedBy.Count > 0 && AbstrCons.originRoom >= 0 && AbstrCons.originRoom != newRoom.abstractRoom.index)
            AbstrCons.Consume();
        if (newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
        {
            if (Circle is null)
                newRoom.AddObject(Circle = new(newRoom, this, 0, 0f));
            else if (Circle.room != newRoom)
            {
                Circle.Destroy();
                newRoom.AddObject(Circle = new(newRoom, this, 0, 0f));
            }
        }
        else if (Circle is not null)
        {
            Circle.Destroy();
            Circle = null;
        }
        System = null;
        var mycelia = Mycelia;
        for (var l = 0; l < mycelia.Length; l++)
            mycelia[l].Reset(firstChunk.pos);
        var uads = newRoom.updateList;
        for (var i = 0; i < uads.Count; i++)
        {
            if (uads[i] is CoralNeuronSystem sys)
            {
                System = sys;
                for (var l = 0; l < mycelia.Length; l++)
                {
                    var myc = mycelia[l];
                    if (myc.system is CoralNeuronSystem mycSys)
                        mycSys.mycelia?.Remove(myc);
                    myc.ConnectSystem(sys);
                }
                break;
            }
        }
        StuckList.Clear();
        StuckListCounter = 10;
    }

    public override void Update(bool eu)
    {
        if (System is CoralNeuronSystem sys && sys.Frozen || room is not Room rm)
            return;
        collisionLayer = grabbedBy.Count == 0 ? 1 : 0;
        Lerper = Mathf.Clamp(Lerper + (LerpUp ? .0075f : -.0075f), -1f, 1f);
        if (Lerper == 1f)
            LerpUp = false;
        else if (Lerper == -1f)
            LerpUp = true;
        if (Lerper >= 0f)
            MyColor = Vector2.Lerp(MyColor, CurrentBehavior.AltColor, Lerper);
        else
            MyColor = Vector2.Lerp(MyColor, CurrentBehavior.AltColor2, -Lerper);
        var fc = firstChunk;
        fc.rad = 3.4f * Size / 3f;
        AffectedByGravity = 1f - fc.submersion;
        fc.vel.y -= rm.gravity * AffectedByGravity;
        LastDirection = Direction;
        LastRotation = Rotation;
        Rotation += RevolveSpeed;
        if (rm.gravity * AffectedByGravity > .5f)
        {
            if (fc.ContactPoint.y < 0)
            {
                Direction = Vector3.Slerp(Direction, new(Mathf.Sign(Direction.x), 0f), .4f);
                RevolveSpeed *= .8f;
            }
            else if (grabbedBy.Count > 0)
                Direction = Custom.PerpendicularVector(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos) * (grabbedBy[0].graspUsed != 0 ? 1f : -1f);
            else
                Direction = Vector3.Slerp(Direction, Custom.DirVec(fc.lastLastPos, fc.pos), .4f);
            Rotation = Mathf.Lerp(Rotation, Mathf.Floor(Rotation) + .25f, Mathf.InverseLerp(.5f, 1f, rm.gravity * AffectedByGravity) * .1f);
        }
        base.Update(eu);
        if (!rm.readyForAI)
            return;
        if (rm.gravity * AffectedByGravity <= .5f)
        {
            Direction = TravelDirection;
            if (Mode == MovementMode.Swarm)
            {
                SwarmBehavior(rm, fc);
                if (rm.aimap.getTerrainProximity(fc.pos) < 7)
                {
                    if (StuckListCounter > 0)
                        --StuckListCounter;
                    else
                    {
                        StuckList.Insert(0, fc.pos);
                        if (StuckList.Count > 10)
                            StuckList.RemoveAt(StuckList.Count - 1);
                        StuckListCounter = 80;
                    }
                    if (Random.value < .025f && StuckList.Count > 1 && Custom.DistLess(fc.pos, StuckList[StuckList.Count - 1], 200f))
                    {
                        var list = new List<int>();
                        var cons = rm.abstractRoom.connections;
                        for (var j = 0; j < cons.Length; j++)
                        {
                            if (rm.aimap.ExitDistanceForCreature(Room.StaticGetTilePosition(fc.pos), j, FlyTemplate) > 0)
                                list.Add(j);
                        }
                        if (list.Count > 0)
                        {
                            Mode = MovementMode.FollowPath;
                            DestNode = list[Random.Range(0, list.Count)];
                        }
                    }
                }
            }
            else if (Mode == MovementMode.FollowPath)
            {
                var tilePosition = Room.StaticGetTilePosition(fc.pos);
                int num2 = -1, num3 = int.MaxValue;
                var dirs = Custom.fourDirections;
                for (var k = 0; k < dirs.Length; k++)
                {
                    var tlpos = tilePosition + dirs[k];
                    if (!rm.GetTile(tlpos).Solid)
                    {
                        var num4 = rm.aimap.ExitDistanceForCreature(tlpos, DestNode, FlyTemplate);
                        if (num4 > 0 && num4 < num3)
                        {
                            num2 = k;
                            num3 = num4;
                        }
                    }
                }
                if (num2 > -1)
                    TravelDirection += dirs[num2].ToVector2().normalized * .175f + Custom.RNV() * Random.value * .0625f;
                else
                    Mode = MovementMode.Swarm;
                TravelDirection.Normalize();
                var num5 = rm.aimap.ExitDistanceForCreature(tilePosition, DestNode, FlyTemplate);
                if (Random.value < .025f && num5 < 34 || num5 < 12 || DestNode < 0 || Random.value < .0025f || rm.aimap.getTerrainProximity(fc.pos) >= 7 && Random.value < 1f / 60f)
                    Mode = MovementMode.Swarm;
            }
            fc.vel += TravelDirection * .175f * (1f - rm.gravity * AffectedByGravity);
            fc.vel *= Custom.LerpMap(fc.vel.magnitude, .2f, 3f, 1f, .9f);
        }
        var mycelia = Mycelia;
        for (var n = 0; n < mycelia.Length; n++)
        {
            var vec = MyceliaDir + Custom.PerpendicularVector(MyceliaDir) * MyceliaDisplace[n];
            var myc = mycelia[n];
            myc.Update();
            var pnts = myc.points;
            pnts[1, 2] += vec;
            if (pnts.GetLength(0) > 2)
                pnts[2, 2] += vec * .5f;
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (weapon is Spear s && s.IsNeedle && s.Spear_NeedleCanFeed() && s.thrownBy is Player p)
        {
            p.ObjectEaten(this);
            p.AddFood(1);
            p.AddQuarterFood();
            p.AddQuarterFood();
            AbstrCons.Consume();
            room?.PlaySound(SoundID.Slugcat_Eat_Swarmer, firstChunk);
            if (!p.isNPC)
            {
                if (room?.game?.session is StoryGameSession sess)
                    sess.saveState.theGlow = true;
            }
            else
                (p.State as PlayerNPCState)!.Glowing = true;
            p.glowing = true;
            var grabbers = grabbedBy;
            for (var i = 0; i < grabbers.Count; i++)
                grabbers[i]?.Release();
            Destroy();
        }
    }

    public virtual void SwarmBehavior(Room rm, BodyChunk fc)
    {
        Vector2 vector2 = default;
        float num2 = CurrentBehavior.Torque, num3 = 0f, num4 = CurrentBehavior.RevolveSpeed;
        TravelDirection += Custom.RNV() * .5f * CurrentBehavior.RandomVibrations;
        Torque = Mathf.Lerp(Torque, num2, .1f);
        RevolveSpeed = Mathf.Lerp(RevolveSpeed, num4, .2f);
        if (num3 > 0f)
            MyColor = Vector2.Lerp(MyColor, vector2 / num3, .4f);
        MyColor = Vector2.Lerp(MyColor, CurrentBehavior.Color, .05f);
        if (rm.aimap.getTerrainProximity(fc.pos) < 5)
        {
            var tilePosition = Room.StaticGetTilePosition(fc.pos);
            Vector2 vector3 = default;
            var dirs = Custom.fourDirections;
            for (var j = 0; j < dirs.Length; j++)
            {
                var dir = dirs[j];
                if (!rm.GetTile(tilePosition + dir).Solid && !rm.aimap.getAItile(tilePosition + dir).narrowSpace)
                {
                    float num9 = 0f;
                    for (var k = 0; k < 4; k++)
                        num9 += rm.aimap.getTerrainProximity(tilePosition + dir + dirs[k]);
                    vector3 += dir.ToVector2() * num9;
                }
            }
            TravelDirection = Vector2.Lerp(TravelDirection, vector3.normalized * .5f, .125f * Mathf.Pow(Mathf.InverseLerp(5f, 1f, rm.aimap.getTerrainProximity(fc.pos)), .25f));
        }
        TravelDirection.Normalize();
        var fisobs = rm.physicalObjects;
        var ps = firstChunk.pos;
        for (var i = 0; i < fisobs.Length; i++)
        {
            if (fisobs[i] is List<PhysicalObject> list)
            {
                for (var j = 0; j < list.Count; j++)
                {
                    if (list[j] is SSOracleSwarmer swarmer)
                    {
                        var sfc = swarmer.firstChunk;
                        if (Custom.DistLess(sfc.pos, ps, 80f))
                        {
                            var myc = Mycelia[Random.Range(0, Mycelia.Length)];
                            sfc.vel += Custom.DirVec(sfc.pos, fc.pos) * .08f;
                            if (Custom.DistLess(sfc.pos, myc.Tip, 8f) && Random.value <= .35f)
                            {
                                swarmer.mode = SSOracleSwarmer.MovementMode.SuckleMycelia;
                                swarmer.suckleMyc = myc;
                            }
                        }
                    }
                }
            }
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprs = sLeaser.sprites = new FSprite[Mycelia.Length + 4];
        var mycelia = Mycelia;
        for (var j = 0; j < mycelia.Length; j++)
            mycelia[j].InitiateSprites(j, sLeaser, rCam);
        var num = mycelia.Length;
        sprs[num] = new("Futile_White") { shader = Custom.rainWorld.Shaders["FlatLightBehindTerrain"] };
        sprs[num + 1] = new("DendriticNeuronBody") { anchorY = 20.5f / 36f };
        sprs[num + 2] = new("DendriticNeuronEye") { color = new(.5f, .5f, .5f), alpha = .75f };
        sprs[num + 3] = new("DendriticNeuronEye") { color = new(.5f, .5f, .5f) };
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        var num = Mycelia.Length;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            if (i == num)
                rCam.ReturnFContainer("Foreground").AddChild(spr);
            else
                newContainer.AddChild(spr);
        }
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        var sprs = sLeaser.sprites;
        var mycelia = Mycelia;
        var num = mycelia.Length;
        var light = sprs[num];
        Vector2 vector = Vector3.Slerp(LastDirection, Direction, timeStacker),
            perp = Custom.PerpendicularVector(vector);
        var rt = Mathf.Lerp(LastRotation, Rotation, timeStacker) * (Mathf.PI * 2f);
        float numSc = Mathf.Sin(rt),
            num2 = Mathf.Cos(rt);
        light.SetPosition(pos - camPos);
        sprs[num + 1].SetPosition(pos - camPos);
        var ps = pos + perp * 2f * num2 * Mathf.Sign(numSc) - camPos;
        sprs[num + 2].SetPosition(ps);
        sprs[num + 3].SetPosition(ps);
        sprs[num + 3].rotation = sprs[num + 2].rotation = sprs[num + 1].rotation = Custom.VecToDeg(vector);
        MyceliaDir = -vector;
        sprs[num + 2].scaleX = (.25f * .8f - Math.Abs(num2) * .25f * .8f) * Size;
        sprs[num + 3].scaleX = (.25f * .6f - Math.Abs(num2) * .25f * .6f) * Size;
        Color color;
        bool flag;
        var darkness = rCam.currentPalette.darkness;
        if (!(room?.roomSettings is RoomSettings rms && (rms.Palette == 24 || rms.fadePalette?.palette == 24)))
        {
            flag = MyColor.x < .5f;
            color = Custom.HSL2RGB(flag ? Custom.LerpMap(MyColor.x, 0f, .5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, .5f + .5f * MyColor.y);
            sprs[num + 3].color = sprs[num + 2].color = Custom.HSL2RGB(flag ? Custom.LerpMap(MyColor.x, 0f, .5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f - MyColor.y, Mathf.Lerp(.8f + .2f * Mathf.InverseLerp(.4f, .1f, MyColor.x), .35f, Mathf.Pow(MyColor.y, 2f)));
        }
        else
        {
            flag = MyColor.x <= .5f;
            color = Custom.HSL2RGB(flag ? 2f / 3f : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, Mathf.Lerp(.1f, .5f, MyColor.y));
            sprs[num + 3].color = sprs[num + 2].color = Custom.HSL2RGB(flag ? 2f / 3f : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, Mathf.Lerp(.75f, .9f, MyColor.y));
        }
        light.scale = 1.5f * Size;
        light.alpha = darkness * .25f;
        for (var j = 0; j < mycelia.Length; j++)
        {
            var myc = mycelia[j];
            myc.color = color;
            myc.UpdateColor(color, 0f, j, sLeaser);
            myc.DrawSprites(j, sLeaser, rCam, timeStacker, camPos);
        }
        sprs[num + 1].color = sprs[num].color = color;
        sprs[num + 2].scaleY = .25f * .8f * Size;
        sprs[num + 3].scaleY = .25f * .6f * Size;
        sprs[num + 1].scaleY = .3f * .8f * Size;
        sprs[num + 1].scaleX = .1875f * .8f * Size;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var num = Mycelia.Length;
        var sprs = sLeaser.sprites;
        Color color;
        bool flag;
        if (!(room?.roomSettings is RoomSettings rms && (rms.Palette == 24 || rms.fadePalette?.palette == 24)))
        {
            flag = MyColor.x < .5f;
            color = Custom.HSL2RGB(flag ? Custom.LerpMap(MyColor.x, 0f, .5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, .5f + .5f * MyColor.y);
            sprs[num + 3].color = sprs[num + 2].color = Custom.HSL2RGB(flag ? Custom.LerpMap(MyColor.x, 0f, .5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f - MyColor.y, Mathf.Lerp(.8f + .2f * Mathf.InverseLerp(.4f, .1f, MyColor.x), .35f, Mathf.Pow(MyColor.y, 2f)));
        }
        else
        {
            flag = MyColor.x <= .5f;
            color = Custom.HSL2RGB(flag ? 2f / 3f : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, Mathf.Lerp(.1f, .5f, MyColor.y));
            sprs[num + 3].color = sprs[num + 2].color = Custom.HSL2RGB(flag ? 2f / 3f : Custom.LerpMap(MyColor.x, .5f, 1f, 2f / 3f, .99722224f), 1f, Mathf.Lerp(.75f, .9f, MyColor.y));
        }
        sprs[num + 1].color = sprs[num].color = color;
    }

    public virtual Room HostingCircleFromRoom() => room;

    public virtual bool CanHostCircle() => !slatedForDeletetion;

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --Bites;
        Size -= .25f;
        room.PlaySound(Bites == 0 ? SoundID.Slugcat_Eat_Swarmer : SoundID.Slugcat_Bite_Swarmer, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites >= 1)
            return;
        if (grasp.grabber is Player p)
        {
            p.ObjectEaten(this);
            if (!p.isNPC)
            {
                if (room.game.session is StoryGameSession sess)
                    sess.saveState.theGlow = true;
            }
            else
                (p.State as PlayerNPCState)!.Glowing = true;
            p.glowing = true;
        }
        else if (grasp.grabber is ChipChop ch)
            ch.Glowing = true;
        grasp.Release();
        Destroy();
        AbstrCons.Consume();
    }

    public virtual void ThrowByPlayer() { }

    public virtual Vector2 ConnectionPos(int index, float timeStacker) => Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - (Vector2)Vector3.Slerp(LastDirection, Direction, timeStacker) * 12f;

    public virtual Vector2 ResetDir(int index) => MyceliaDir;

    public virtual Vector2 CircleCenter(int index, float timeStacker) => Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);

    public override void Destroy()
    {
        base.Destroy();
        if (Circle is not null)
        {
            Circle.Destroy();
            Circle = null;
        }
    }
}