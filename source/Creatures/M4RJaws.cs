using LBMergedMods.Hooks;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Joint = (UnityEngine.Vector2 LastPos, UnityEngine.Vector2 Pos);
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class M4RJaws : Creature
{
    public class Leg(M4RJaws ow, int legNumber)
    {
        public M4RJaws Ow = ow;
        public Joint[] Joints = new Joint[3];
        public const float LOWER_LEG_LGT = 55f, TIGH_LGT = 70f;
        public int LegNum = legNumber;
        public float ForwardPower, Flip, LastRunMode, CurrentRunMode, MoveProgress, FootSecureFrames, LightUp;
        public Vector2 MoveFromPos, LightUpPos1, LightUpPos2;
        public Vector2? FootSecurePos, MoveToPos, LastFootSecurePos;
        public bool GroundContact;

        public virtual Room Room => Ow.room;

        public virtual int LegSide => LegNum % 2;

        public virtual int OtherLeg => LegSide == 0 ? LegNum + 1 : LegNum - 1;

        public virtual int PairLeg => LegNum + Ow.Legs.Length / 2;

        public virtual Vector2 ConnectionPos
        {
            get
            {
                var chs = Ow.bodyChunks;
                var ps1 = chs[2].pos;
                var ps2 = chs[3].pos;
                if (LegNum > 1)
                {
                    var ch0 = chs[0];
                    var dir = Custom.DirVec(ch0.pos, chs[1].pos);
                    var ps = ch0.pos - dir * 5f;
                    ps1 = Vector2.Lerp(ps1, ps, .9f);
                    ps2 = Vector2.Lerp(ps2, ps, .9f);
                }
                return Vector2.Lerp(ps1, ps2, Mathf.Lerp(.5f + Flip * .5f, .5f + .5f * Mathf.Sin(Ow.RunCycle(LegSide * .5f, 1f) * Mathf.PI * 2f), CurrentRunMode)); //connec side
            }
        }

        public virtual ref Joint Hip => ref Joints[0];

        public virtual ref Joint Knee => ref Joints[1];

        public virtual ref Joint Foot => ref Joints[2];

        public virtual void Reset()
        {
            var connPos = ConnectionPos;
            ref var foot = ref Foot;
            ref var hip = ref Hip;
            ref var knee = ref Knee;
            foot.Pos = connPos;
            hip.Pos = connPos;
            knee.Pos = connPos;
            foot.LastPos = connPos;
            hip.LastPos = connPos;
            knee.LastPos = connPos;
            FootSecurePos = null;
            LastFootSecurePos = null;
            MoveToPos = null;
        }

        public virtual void Update()
        {
            if (FootSecurePos.HasValue)
                LastFootSecurePos = FootSecurePos.Value;
            else
                LastFootSecurePos = null;
            ForwardPower = 0f;
            GroundContact = false;
            LastRunMode = CurrentRunMode;
            if (Math.Abs(Ow.BodyFlip) > (Ow.MoveDir.y < -.1f ? .1f : .3f) && Math.Abs(Ow.firstChunk.vel.x) > 3f && (Math.Abs(Ow.MoveDir.x) > .5f || Ow.MoveDir.y < -.1f) && Ow.AI?.pathFinder is M4RJawsPather pather && Custom.ManhattanDistance(pather.GetDestination, Ow.abstractPhysicalObject.pos) > 3)
                CurrentRunMode = Math.Min(1f, CurrentRunMode + .025f);
            else
                CurrentRunMode = Math.Max(0f, CurrentRunMode - .1f);
            Walk();
            if (FootSecurePos.HasValue && LastFootSecurePos.HasValue && FootSecurePos.Value == LastFootSecurePos.Value)
                ++FootSecureFrames;
            else
                FootSecureFrames = 0;
            if (Room is not Room rm)
                return;
            ref var foot = ref Foot;
            ref var hip = ref Hip;
            ref var knee = ref Knee;
            if (!FootSecurePos.HasValue && LastFootSecurePos is Vector2 lastPos && foot.Pos.x - lastPos.x < 0f != Ow.MoveDir.x < 0f && !Custom.DistLess(foot.Pos, lastPos, 18f) && !Custom.DistLess(foot.Pos, foot.LastPos, 18f))
            {
                SmallSparks(lastPos, foot.Pos);
                rm.PlaySound(SoundID.Miros_Piston_Scrape, foot.Pos, Ow.abstractPhysicalObject);
                rm.InGameNoise(new(foot.Pos, 1200f, Ow, 1f));
            }
            else if (FootSecurePos is Vector2 pos && !LastFootSecurePos.HasValue)
            {
                rm.PlaySound(SoundID.Vulture_Tentacle_Grab_Terrain, foot.Pos, Ow.abstractPhysicalObject);
                rm.InGameNoise(new(pos, 800f, Ow, 1f));
            }
            if (FootSecurePos.HasValue && !LastFootSecurePos.HasValue && !Custom.DistLess(foot.Pos, foot.LastPos, 60f))
            {
                rm.PlaySound(SoundID.Miros_Piston_Sharp_Impact, foot.Pos, Ow.abstractPhysicalObject);
                SmallSparks(foot.Pos, foot.Pos);
            }
            if (LightUp > 0)
                --LightUp;
            if (!Custom.DistLess(knee.Pos, hip.Pos, TIGH_LGT))
                knee.Pos = hip.Pos + Custom.DirVec(hip.Pos, knee.Pos) * TIGH_LGT;
            var dst = LOWER_LEG_LGT + (GroundContact ? 20f : 0f);
            if (!Custom.DistLess(knee.Pos, foot.Pos, dst))
                foot.Pos = knee.Pos + Custom.DirVec(knee.Pos, foot.Pos) * dst;
        }

        public virtual void Walk()
        {
            if (Room is not Room rm)
                return;
            ref var foot = ref Foot;
            ref var hip = ref Hip;
            ref var knee = ref Knee;
            var b0 = Ow.firstChunk;
            Flip = Mathf.Lerp(Flip, -1f + 2f * LegSide, .2f);
            hip.LastPos = hip.Pos;
            hip.Pos = ConnectionPos;
            Vector2 dir = Vector3.Slerp(Custom.DirVec(Ow.bodyChunks[1].pos, b0.pos), Vector2.down, .75f),
                mvPs = hip.Pos + ((dir * 1.8f + Custom.DirVec(b0.pos, hip.Pos)).normalized * .7f + new Vector2(Ow.MoveDir.x, Ow.MoveDir.y * .4f)).normalized * (TIGH_LGT + LOWER_LEG_LGT);
            foot.LastPos = foot.Pos;
            if (MoveToPos.HasValue)
            {
                MoveProgress = Math.Min(1f, MoveProgress + .1f);
                mvPs = Vector2.Lerp(MoveFromPos, MoveToPos.Value, MoveProgress);
                foot.Pos = mvPs;
                if (MoveProgress >= 1f)
                {
                    FootSecurePos = MoveToPos;
                    MoveToPos = null;
                    MoveProgress = 0f;
                }
            }
            else if (FootSecurePos is Vector2 pos)
            {
                ForwardPower = .5f * Custom.LerpMap(Vector2.Dot(Custom.DirVec(foot.Pos, b0.pos), Ow.MoveDir), -1f, 1f, 0f, .2f);
                GroundContact = true;
                foot.Pos = pos;
                if (!Custom.DistLess(hip.Pos, pos, TIGH_LGT + LOWER_LEG_LGT + (Ow.Legs[OtherLeg].GroundContact ? 0f : 20f)))
                    FootSecurePos = null;
            }
            else
            {
                foot.Pos += Vector2.ClampMagnitude(mvPs - foot.Pos, 20f);
                MoveFromPos = foot.Pos;
                MoveProgress = 0f;
                var rayTracePos = SharedPhysics.ExactTerrainRayTracePos(rm, hip.Pos, mvPs);
                if (rayTracePos.HasValue)
                {
                    mvPs = rayTracePos.Value;
                    MoveToPos = mvPs;
                    GroundContact = true;
                }
            }
            var kneeKinPos = Custom.InverseKinematic(hip.Pos, foot.Pos, TIGH_LGT, LOWER_LEG_LGT, -Flip);
            var rayTracePos2 = SharedPhysics.ExactTerrainRayTracePos(rm, kneeKinPos, foot.Pos);
            if (rayTracePos2.HasValue)
                foot.Pos = rayTracePos2.Value;
            knee.LastPos = knee.Pos;
            knee.Pos += Vector2.ClampMagnitude(kneeKinPos - knee.Pos, 60f);
            rayTracePos2 = SharedPhysics.ExactTerrainRayTracePos(rm, hip.Pos, knee.Pos);
            if (rayTracePos2.HasValue)
            {
                ForwardPower = Math.Max(ForwardPower, .06f);
                knee.Pos = rayTracePos2.Value;
            }
            if (LegSide == 1 && FootSecurePos.HasValue && Ow.AI?.pathFinder is M4RJawsPather pather)
            {
                var legs = Ow.Legs;
                for (var n = 0; n <= 2; n += 2)
                {
                    var lg0 = legs[n];
                    var lg1 = legs[n + 1];
                    if (lg0.FootSecurePos.HasValue && Custom.ManhattanDistance(pather.GetDestination, Ow.abstractPhysicalObject.pos) < 4 && lg0.Foot.Pos.x < b0.pos.x == lg1.Foot.Pos.x < b0.pos.x)
                    {
                        var otherLeg = legs[Ow.MoveDir.x < 0f != lg0.Foot.Pos.x < lg1.Foot.Pos.x ? n + 1 : n];
                        if (otherLeg.FootSecurePos.HasValue)
                            otherLeg.MoveFromPos = otherLeg.FootSecurePos.Value;
                        otherLeg.MoveProgress = 0f;
                        otherLeg.FootSecurePos = null;
                    }
                }
            }
        }

        public virtual void SmallSparks(Vector2 lastPs, Vector2 ps)
        {
            var rm = Room;
            var mx = 1 + (int)(Vector2.Distance(lastPs, ps) / 12f);
            var dir = Custom.DirVec(ps, Knee.Pos);
            var vel = (ps - lastPs) * .3f;
            for (var i = 0; i < mx; i++)
                rm.AddObject(new Spark(Vector2.Lerp(lastPs, ps, Random.value) + dir, Custom.RNV() * 4f + vel, new(1f, 1f, .8f), null, 4, 14));
            LightUp = Random.Range(1, 3) + (int)Custom.LerpMap(Vector2.Distance(lastPs, ps), 10f, 40f, 0f, 3f);
            LightUpPos1 = lastPs;
            LightUpPos2 = ps;
        }
    }

    public M4RJawsAI? AI;
    public Tentacle Neck;
    public Leg[] Legs;
    public List<IntVector2>? PastPositions;
    public float Fatness, ForwardPower, WeightDownToStandOnAllLegs, LastBodyFlip, BodyFlip,
        JawOpen, LastJawOpen, JawVel, KeepJawOpenPos, CurrentRunCycle, LastRunCycle;
    public int Lungs = 500, JawSlamPause, JawKeepOpenPause, StuckCounter;
    public Vector2 MoveDir, NeutralDir, RemMoveDir;
    public bool EnterRoomHalf, ControlledJawSnap;

    public virtual BodyChunk Head => bodyChunks[4];

    public override Vector2 VisionPoint => Head.pos;

    public M4RJaws(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        Fatness = Random.value;
        Random.state = state;
        NeutralDir = new(0f, -.025f);
        var chs = bodyChunks =
        [
            new(this, 0, default, 18f, 3.8f),
            new(this, 1, default, 8f, 1.8f),
            new(this, 2, default, 7f, 1.6f),
            new(this, 3, default, 7f, 1.6f),
            new(this, 4, default, 9f, .6f) { goThroughFloors = true }
        ];
        var cons = bodyChunkConnections = new BodyChunkConnection[6];
        var ind = 0;
        Vector2[] ar = [default, new(0f, 25f), new(-17f, 0f), new(17f, 0f)];
        for (var k = 0; k < ar.Length; k++)
        {
            for (var l = k + 1; l < ar.Length; l++)
            {
                cons[ind] = new(chs[k], chs[l], Vector2.Distance(ar[k], ar[l]), BodyChunkConnection.Type.Normal, 1f, -1f);
                ++ind;
            }
        }
        var legs = Legs = new Leg[4];
        for (var m = 0; m < legs.Length; m++)
            legs[m] = new(this, m);
        Neck = new(this, chs[1], 180f)
        {
            tProps = new(false, false, true, .5f, .1f, .5f, 1.8f, .2f, 1.2f, 10f, .25f, 10f, 15, 20, 20, 0),
            tChunks = new Tentacle.TentacleChunk[5]
        };
        var tchs = Neck.tChunks;
        for (var n = 0; n < tchs.Length; n++)
            tchs[n] = new(Neck, n, (n + 1) / (float)tchs.Length, 3f);
        tchs[tchs.Length - 1].rad = 5f;
        Neck.stretchAndSqueeze = 0f;
        airFriction = .999f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = .4f;
    }

    public virtual bool RoomHalf(Room room) => abstractPhysicalObject.pos.x > room.TileWidth / 2;

    public virtual float RunCycle(float cycleSpot, float timeStacker)
    {
        var runCyc = Mathf.Lerp(LastRunCycle, CurrentRunCycle, timeStacker) + cycleSpot;
        if (runCyc < 0f)
            runCyc += Mathf.Floor(Math.Abs(runCyc) + 1f);
        return runCyc - Mathf.Floor(runCyc);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new M4RJawsGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm)
            return;
        CheckFlip();
        if (!enteringShortCut.HasValue)
            UpdateNeck();
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
        {
            var b0 = firstChunk;
            b0.vel += Custom.DirVec(b0.pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        Act();
        var legs = Legs;
        for (var i = 0; i < legs.Length; i++)
            legs[i].Update();
        if (grasps[0] is not null)
            Carry();
    }

    public override void Blind(int blnd)
    {
        blnd /= 4;
        base.Blind(blnd);
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public virtual void Act()
    {
        if (room is not Room rm)
            return;
        var chs = bodyChunks;
        var b0 = chs[0];
        var head = chs[4];
        LastJawOpen = JawOpen;
        if (Consious)
            head.vel *= .95f;
        if (head.submersion >= 1f)
        {
            LoseAllGrasps();
            if (!dead)
            {
                if (Consious && grasps[0] is null)
                {
                    if (safariControlled && (!lastInputWithDiagonals.HasValue || !lastInputWithDiagonals.Value.jmp) && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
                    {
                        JawOpen += .2f;
                        rm.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, head, false, .65f, 1.5f);
                    }
                    else if (JawOpen > .1f && Random.value < .055f)
                        rm.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, head, false, .65f, 1.5f);
                }
                JawOpen = Math.Min(JawOpen + .2f, 1f);
                if (Lungs > 0)
                {
                    --Lungs;
                    var rnv = Custom.RNV() * 3f;
                    head.vel -= rnv;
                    b0.vel += rnv;
                    if (Random.value < 1f / 3f)
                        rm.AddObject(new Bubble(head.pos, rnv, false, false));
                }
                else
                    Die();
            }
            else
                JawOpen = 1f;
            AI?.Update();
        }
        else
        {
            if (Consious && grasps[0] is null)
            {
                if (safariControlled && (!lastInputWithDiagonals.HasValue || !lastInputWithDiagonals.Value.jmp) && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
                {
                    JawOpen += .2f;
                    rm.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, head, false, .65f, 1.5f);
                }
                else if (JawOpen > .1f && Random.value < .045f)
                    rm.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, head, false, .65f, 1.5f);
            }
            if (Lungs < 500)
                ++Lungs;
            if (safariControlled)
            {
                for (var i = 0; i < chs.Length - 1; i++)
                    chs[i].vel -= new Vector2(0f, .5f);
            }
            if (Consious)
            {
                if (MoveDir != NeutralDir)
                    RemMoveDir = MoveDir;
                if (grasps[0] is not null)
                    JawOpen = .15f;
                else if (JawSlamPause > 0)
                    --JawSlamPause;
                else
                {
                    if (JawVel == 0f && !dead)
                        JawVel = .15f;
                    if (safariControlled && JawVel >= 0f && JawVel < 1f && !ControlledJawSnap)
                    {
                        JawVel = 0f;
                        JawOpen = 0f;
                    }
                    JawOpen += JawVel;
                    if (JawKeepOpenPause > 0)
                    {
                        --JawKeepOpenPause;
                        JawOpen = Mathf.Clamp(Mathf.Lerp(JawOpen, KeepJawOpenPos, Random.value * .5f), 0f, 1f);
                    }
                    else if (Random.value < 1f / (Blinded ? 15f : 40f) && !safariControlled)
                    {
                        JawKeepOpenPause = Random.Range(40, Random.Range(60, 100));
                        KeepJawOpenPos = Random.value < .5f ? 0f : 1f;
                        JawVel = Mathf.Lerp(-.4f, .4f, Random.value);
                        JawOpen = Mathf.Clamp(JawOpen, 0f, 1f);
                    }
                    else if (JawOpen <= 0f)
                    {
                        JawOpen = 0f;
                        if (JawVel < -.4f)
                        {
                            JawSlamShut();
                            ControlledJawSnap = false;
                        }
                        JawVel = .15f;
                        JawSlamPause = 20;
                    }
                    else if (JawOpen >= 1f)
                    {
                        JawOpen = 1f;
                        JawVel = -.5f;
                    }
                }
                var legs = Legs;
                var num = 0;
                var dest = AI?.pathFinder?.GetEffectualDestination ?? new(-1, -1, -1, -1);
                if (PastPositions is List<IntVector2> positions && Custom.ManhattanDistance(abstractPhysicalObject.pos, dest) > 3)
                {
                    var tl = abstractPhysicalObject.pos.Tile;
                    positions.Insert(0, tl);
                    if (positions.Count > 40)
                        positions.RemoveAt(positions.Count - 1);
                    for (var i = 20; i < positions.Count; i++)
                    {
                        if (Custom.DistLess(tl, positions[i], 4f))
                            ++num;
                    }
                }
                if (num > 10)
                    ++StuckCounter;
                else
                    StuckCounter -= 2;
                StuckCounter = Custom.IntClamp(StuckCounter, 0, 200);
                if (safariControlled)
                    StuckCounter = 0;
                if (StuckCounter > 100)
                {
                    if (Random.value < 1f / 30f)
                    {
                        var ind = Random.Range(0, 2);
                        var lg = legs[ind];
                        lg.FootSecurePos = null;
                        legs[lg.PairLeg].FootSecurePos = null;
                    }
                    for (var j = 0; j < chs.Length - 1; j++)
                        chs[j].vel += .35f * Custom.RNV() * Custom.LerpMap(StuckCounter, 100f, 200f, 0f, 3f);
                }
                LastRunCycle = CurrentRunCycle;
                CurrentRunCycle += Math.Sign(BodyFlip) / Custom.LerpMap(Math.Abs(b0.vel.x), 2f, 10f, 50f, 20f);
                AI?.Update();
                if (safariControlled)
                    MoveDir = NeutralDir;
                else
                    MoveDir = Vector2.up;
                if (!safariControlled && rm.aimap?.TileAccessibleToCreature(rm.GetTilePosition(b0.pos), Template) is true && dest.TileDefined && dest.room == rm.abstractRoom?.index && Custom.DistLess(b0.pos, rm.MiddleOfTile(dest), 30f))
                    MoveDir = Vector2.ClampMagnitude(rm.MiddleOfTile(dest) - b0.pos, 30f) / 30f;
                else
                    FollowPath();
                LastBodyFlip = BodyFlip;
                if (Math.Abs(MoveDir.x) < .3f)
                    BodyFlip *= .7f;
                else if (MoveDir.x < 0f)
                    BodyFlip = Math.Max(-1f, BodyFlip - .1f);
                else
                    BodyFlip = Math.Min(1f, BodyFlip + .1f);
                float groundAff = 0f,
                    pw = ForwardPower;
                var b1 = chs[1];
                ForwardPower = 0f;
                var flag2 = true;
                for (var m = 0; m < legs.Length; m++)
                {
                    var leg = legs[m];
                    if (leg.GroundContact)
                    {
                        groundAff = 1f;
                        ForwardPower += leg.ForwardPower * .5f;
                    }
                    else
                        flag2 = false;
                }
                if (!flag2 && Custom.ManhattanDistance(abstractPhysicalObject.pos, dest) < 6)
                    WeightDownToStandOnAllLegs = Math.Min(1f, WeightDownToStandOnAllLegs + 1f / 30f);
                else if (Math.Abs(MoveDir.x) > .3f)
                    WeightDownToStandOnAllLegs = Math.Max(0f, WeightDownToStandOnAllLegs - .1f);
                var flag3 = false;
                var basePosY = abstractPhysicalObject.pos.y;
                while (basePosY >= abstractPhysicalObject.pos.y - 3 && !flag3)
                {
                    flag3 = rm.aimap?.TileAccessibleToCreature(new IntVector2(abstractPhysicalObject.pos.x, basePosY), Template) is true;
                    --basePosY;
                }

                if (!flag3)
                    groundAff = 0f;
                ForwardPower = Mathf.Pow(ForwardPower, .4f);
                ForwardPower = Custom.LerpMap(StuckCounter, 100f, 200f, ForwardPower, 1.5f);
                var vlFac = Mathf.Lerp(1f, .85f, Mathf.Pow(groundAff, .5f));
                var runModeSum = 0f;
                for (var i = 0; i < legs.Length; i++)
                    runModeSum += legs[i].CurrentRunMode;
                var yAf = g * rm.gravity * Mathf.Pow(groundAff, .5f) * Mathf.InverseLerp(1f, .5f, WeightDownToStandOnAllLegs * (1f - runModeSum / legs.Length));
                var mvDir = MoveDir * (Math.Max(ForwardPower, pw * .5f) * 2.6f);
                for (var n = 0; n < chs.Length; n++)
                {
                    var ch = chs[n];
                    ch.vel *= vlFac;
                    ch.vel.y += yAf;
                    ch.vel += mvDir;
                }
                head.vel += MoveDir * (ForwardPower * 1.5f);
                WeightedPush(1, 2, new(MoveDir.x, 0f), Custom.LerpMap(Vector2.Dot(Custom.DirVec(b0.pos, b1.pos), Vector2.up), 0f, 1f, 0f, 1f));
                WeightedPush(1, 2, Vector2.up, Custom.LerpMap(Vector2.Dot(Custom.DirVec(b0.pos, b1.pos), Vector2.up), -1f, 1f, 8f, 0f) * groundAff);
                if (!safariControlled && EnterRoomHalf != RoomHalf(rm))
                {
                    var pos = abstractPhysicalObject.pos;
                    if (pos.x < 4 && EnterRoomHalf && !rm.GetTile(0, pos.y).Solid)
                    {
                        b0.pos.x -= 3.5f;
                        b0.vel.x -= 3.5f;
                    }
                    else if (pos.x < 4 && EnterRoomHalf && !rm.GetTile(0, pos.y + 2).Solid)
                    {
                        b0.pos.x -= 3.5f;
                        b0.vel.x -= 3.5f;
                        b0.pos.y += 2.5f;
                        b0.vel.y += 2.5f;
                    }
                    else if (pos.x > rm.TileWidth - 5 && !EnterRoomHalf && !rm.GetTile(rm.TileWidth - 1, pos.y).Solid)
                    {
                        b0.pos.x += 3.5f;
                        b0.vel.x += 3.5f;
                    }
                    else if (pos.x > rm.TileWidth - 5 && !EnterRoomHalf && !rm.GetTile(rm.TileWidth - 1, pos.y + 2).Solid)
                    {
                        b0.pos.x += 3.5f;
                        b0.vel.x += 3.5f;
                        b0.pos.y += 2.5f;
                        b0.vel.y += 2.5f;
                    }
                }
                if (safariControlled || AI is not M4RJawsAI ai || ai.Behav != M4RJawsAI.Behavior.Hunt || ai.FocusCreature is not Tracker.CreatureRepresentation rep || (rep.representedCreature is AbstractCreature cr && !ai.DoIWantToBiteCreature(cr)) || !rep.VisualContact)
                {
                    ForwardPower = Mathf.Lerp(ForwardPower, 0f, .125f);
                    for (var i = 0; i < chs.Length; i++)
                    {
                        var b = chs[i];
                        b.vel.x = Mathf.Lerp(b.vel.x, 0f, .0625f);
                    }
                }
            }
        }
    }

    public virtual void FollowPath()
    {
        if (room is not Room rm)
            return;
        var bcIndex = 0;
        var bs = bodyChunks;
        var b0 = bs[0];
        if (AI?.pathFinder is not M4RJawsPather pather)
            return;
        var b0coord = rm.GetWorldCoordinate(b0.pos);
        var movementConnection = pather.FollowPath(b0coord, true);
        if (movementConnection == default)
        {
            for (var i = 0; i < 4; i++)
            {
                if (movementConnection != default)
                    break;
                for (var j = 0; j < bs.Length; j++)
                {
                    if (movementConnection != default)
                        break;
                    movementConnection = pather.FollowPath(rm.GetWorldCoordinate(bs[i].pos) + Custom.fourDirectionsAndZero[j], true);
                    if (movementConnection != default)
                    {
                        bcIndex = i;
                        break;
                    }
                }
            }
        }
        var fourDirs = Custom.fourDirections;
        if (movementConnection == default)
        {
            for (var k = 2; k < 4; k++)
            {
                if (movementConnection != default)
                    break;
                for (var l = 0; l < 4; l++)
                {
                    if (movementConnection != default)
                        break;
                    for (var m = 0; m < fourDirs.Length; m++)
                    {
                        if (movementConnection != default)
                            break;
                        movementConnection = pather.FollowPath(rm.GetWorldCoordinate(bs[l].pos) + fourDirs[m] * k, true);
                        if (movementConnection != default)
                        {
                            bcIndex = l;
                            break;
                        }
                    }
                }
            }
        }
        if (movementConnection == default)
        {
            var b0Tl = rm.GetTilePosition(b0.pos);
            var access = rm.aimap.AccessibilityForCreature(b0Tl, Template);
            IntVector2? tlPos = null;
            var eightDirs = Custom.eightDirections;
            for (var n = 1; n < 3; n++)
            {
                for (var num3 = 0; num3 < eightDirs.Length; num3++)
                {
                    var dir = eightDirs[num3] * n;
                    var access2 = rm.aimap.AccessibilityForCreature(b0Tl + dir, Template);
                    if (access2 > access)
                    {
                        access = access2;
                        tlPos = b0Tl + dir;
                    }
                }
            }
            if (tlPos.HasValue)
                movementConnection = new(MovementConnection.MovementType.Standard, b0coord, rm.GetWorldCoordinate(tlPos.Value), 1);
        }
        if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
        {
            movementConnection = default;
            if (inputWithDiagonals is Player.InputPackage input)
            {
                var mvType = MovementConnection.MovementType.Standard;
                if (movementConnection != default)
                    mvType = movementConnection.type;
                if (input.AnyDirectionalInput)
                {
                    Vector2 dirVel = default;
                    if (input.y == 0 && input.x != 0)
                        dirVel = new(0f, 80f);
                    movementConnection = new(mvType, b0coord, rm.GetWorldCoordinate(b0.pos + new Vector2(input.x, input.y) * 200f + dirVel), 2);
                }
                if (input.pckp && (lastInputWithDiagonals is not Player.InputPackage lastInput || lastInput.pckp))
                    ControlledJawSnap = true;
                if (input.thrw && (lastInputWithDiagonals is not Player.InputPackage lastInput2 || lastInput2.thrw))
                    LoseAllGrasps();
                GoThroughFloors = input.y < 0;
            }
        }
        if (movementConnection == default)
            return;
        var chosenCh = bs[bcIndex];
        MoveDir = Custom.DirVec(chosenCh.pos, rm.MiddleOfTile(movementConnection.DestTile));
        var movementConnection2 = movementConnection;
        var middleOfDestTile = rm.MiddleOfTile(movementConnection2.destinationCoord);
        for (var num5 = 0; num5 < 10; num5++)
        {
            movementConnection2 = pather.FollowPath(movementConnection2.destinationCoord, false);
            if (movementConnection2 == default || !rm.VisualContact(movementConnection.StartTile, movementConnection2.DestTile) || Vector2.Distance(rm.MiddleOfTile(movementConnection.startCoord), middleOfDestTile) > Vector2.Distance(rm.MiddleOfTile(movementConnection.startCoord), rm.MiddleOfTile(movementConnection2.destinationCoord)))
                break;
            middleOfDestTile = rm.MiddleOfTile(movementConnection2.destinationCoord);
            MoveDir += Custom.DirVec(chosenCh.pos, rm.MiddleOfTile(movementConnection2.DestTile));
        }
        MoveDir.Normalize();
    }

    public virtual void CheckFlip()
    {
        var bs = bodyChunks;
        var b3 = bs[3];
        if (Custom.DistanceToLine(b3.pos, bs[1].pos, bs[0].pos) < 0f)
        {
            var b2 = bs[2];
            Vector2 pos = b3.pos,
                vel = b3.vel,
                lastPos = b3.lastPos,
                lastLastPos = b3.lastLastPos;
            b3.pos = b2.pos;
            b3.vel = b2.vel;
            b3.lastPos = b2.lastPos;
            b3.lastLastPos = b2.lastLastPos;
            b2.pos = pos;
            b2.vel = vel;
            b2.lastPos = lastPos;
            b2.lastLastPos = lastLastPos;
        }
    }

    public virtual void UpdateNeck()
    {
        Neck.Update();
        var mvDir = MoveDir;
        if (safariControlled)
            mvDir = RemMoveDir;
        var tChs = Neck.tChunks;
        var b0 = firstChunk;
        var b1 = bodyChunks[1];
        var head = Head;
        var conCh = Neck.connectedChunk;
        var backtrack = Neck.backtrackFrom;
        for (var i = 0; i < tChs.Length; i++)
        {
            var tch = tChs[i];
            var t = (float)i / (tChs.Length - 1);
            tch.vel *= .95f;
            tch.vel.y -= Neck.limp ? .7f : .1f;
            if (backtrack == -1 || backtrack > i)
                tch.vel += Custom.DirVec(b0.pos, b1.pos) * Mathf.Lerp(3f, 1f, t);
            tch.vel -= conCh.vel;
            tch.vel *= .75f;
            tch.vel += conCh.vel;
        }
        Neck.limp = !Consious;
        var num = backtrack == -1 ? .5f : 0f;
        var tipCh = tChs[tChs.Length - 1];
        var prelastCh = tChs[tChs.Length - 2];
        if (grasps[0] is null)
        {
            var tipPos = Custom.DirVec(head.pos, tipCh.pos);
            var dst = Vector2.Distance(head.pos, tipCh.pos);
            head.pos -= tipPos * ((6f - dst) * (1f - num));
            head.vel -= tipPos * ((6f - dst) * (1f - num));
            tipCh.pos += tipPos * ((6f - dst) * num);
            tipCh.vel += tipPos * ((6f - dst) * num);
            head.vel += Custom.DirVec(prelastCh.pos, head.pos) * (6f * (1f - num));
            head.vel += Custom.DirVec(tipCh.pos, head.pos) * (6f * (1f - num));
            tipCh.vel -= Custom.DirVec(prelastCh.pos, head.pos) * (6f * num);
            prelastCh.vel -= Custom.DirVec(prelastCh.pos, head.pos) * (6f * num);
        }
        if (!Consious)
        {
            Neck.retractFac = .5f;
            Neck.floatGrabDest = null;
            return;
        }
        head.vel.y += gravity;
        var mvPs = AI?.Looker.lookCreature is not Tracker.CreatureRepresentation tcr ? (b0.pos + mvDir * 400f) : (!tcr.VisualContact ? room.MiddleOfTile(tcr.BestGuessForPosition()) : tcr.representedCreature.realizedCreature.DangerPos);
        var runMean = 0f;
        var legs = Legs;
        for (var i = 0; i < legs.Length; i++)
            runMean += legs[i].CurrentRunMode;
        runMean /= legs.Length;
        Neck.retractFac = Mathf.Lerp(.5f, .8f, runMean);
        mvPs = Vector2.Lerp(mvPs, b0.pos + mvDir * 200f, Mathf.Pow(runMean, 6f));
        if (Blinded)
            mvPs = b0.pos + Custom.RNV() * (Random.value * 400f);
        if ((Custom.DistLess(mvPs, b0.pos, 220f) && !room.VisualContact(mvPs, head.pos)) || runMean > .5f)
        {
            List<IntVector2> path = [];
            Neck.MoveGrabDest(mvPs, ref path);
        }
        else if (backtrack == -1)
            Neck.floatGrabDest = null;
        var headDir = Custom.DirVec(head.pos, mvPs);
        if (grasps[0]?.grabbedChunk is not BodyChunk gch)
        {
            tipCh.vel += headDir * (num * 1.2f);
            prelastCh.vel -= headDir * (.5f * num);
            head.vel += headDir * (6f * (1f - num));
        }
        else
        {
            tipCh.vel += headDir * (2f * num);
            prelastCh.vel -= headDir * (2f * num);
            gch.vel += headDir / gch.mass;
        }
        if (Custom.DistLess(head.pos, mvPs, 80f * Mathf.InverseLerp(1f, .5f, JawOpen)))
        {
            var velRmv = headDir * (Mathf.InverseLerp(80f, 20f, Vector2.Distance(head.pos, mvPs)) * 8f * num);
            for (var j = 0; j < tChs.Length; j++)
                tChs[j].vel -= velRmv;
        }
    }

    public virtual void Carry()
    {
        if (!Consious)
        {
            LoseAllGrasps();
            return;
        }
        var grabbedChunk = grasps[0].grabbedChunk;
        if (grabbedChunk.owner is TentaclePlant t && Random.value < .1f)
        {
            t.Stun(100);
            LoseAllGrasps();
            return;
        }
        if (!safariControlled && Random.value < 1f / 120f && (grabbedChunk.owner is not Creature cr || Template.CreatureRelationship(cr.Template).type != CreatureTemplate.Relationship.Type.Eats))
        {
            LoseAllGrasps();
            return;
        }
        var head = Head;
        var b0 = firstChunk;
        float weightedMass1 = grabbedChunk.mass / (grabbedChunk.mass + head.mass),
            weightedMass2 = grabbedChunk.mass / (grabbedChunk.mass + b0.mass);
        if (Neck.backtrackFrom != -1 || enteringShortCut.HasValue)
        {
            weightedMass1 = 0f;
            weightedMass2 = 0f;
        }
        var tipCh = Neck.Tip;
        if (!Custom.DistLess(grabbedChunk.pos, tipCh.pos, 20f))
        {
            var weightDir = Custom.DirVec(grabbedChunk.pos, tipCh.pos);
            var dist = Vector2.Distance(grabbedChunk.pos, tipCh.pos);
            var vel = (20f - dist) * weightDir * (1f - weightedMass1);
            grabbedChunk.pos -= vel;
            grabbedChunk.vel -= vel;
            vel = weightDir * ((20f - dist) * weightedMass1);
            tipCh.pos += vel;
            tipCh.vel += vel;
        }
        if (!enteringShortCut.HasValue)
        {
            head.pos = Vector2.Lerp(tipCh.pos, grabbedChunk.pos, .1f);
            head.vel = tipCh.vel;
        }
        var num4 = 40f;
        if (!Custom.DistLess(b0.pos, grabbedChunk.pos, num4))
        {
            if (!Custom.DistLess(b0.pos, grabbedChunk.pos, num4 * 3f))
            {
                LoseAllGrasps();
                return;
            }
            var dir = Custom.DirVec(grabbedChunk.pos, b0.pos);
            var dist = Vector2.Distance(grabbedChunk.pos, b0.pos);
            var vel = dir * ((num4 - dist) * (1f - weightedMass2));
            grabbedChunk.pos -= vel;
            grabbedChunk.vel -= vel;
            vel = dir * ((num4 - dist) * weightedMass2);
            b0.pos += vel;
            b0.vel += vel;
        }
        if (grabbedChunk.owner is Creature cr2 && cr2.enteringShortCut.HasValue)
            LoseAllGrasps();
    }

    public virtual void JawSlamShut()
    {
        var tip = Neck.Tip;
        var head = Head;
        var neckEndDir = Custom.DirVec(tip.pos, head.pos);
        tip.vel -= neckEndDir * 10f;
        tip.pos += neckEndDir * 20f;
        head.pos += neckEndDir * 20f;
        var num = 0;
        var creatures = room.abstractRoom.creatures;
        for (var i = 0; i < creatures.Count; i++)
        {
            if (grasps[0] is not null)
                break;
            var crit = creatures[i];
            if (crit == abstractCreature || (AI is M4RJawsAI ai && !ai.DoIWantToBiteCreature(crit)) || !crit.SameRippleLayer(abstractPhysicalObject) || crit.realizedCreature is not Creature cr || cr.enteringShortCut.HasValue)
                continue;
            var bs = cr.bodyChunks;
            for (var j = 0; j < bs.Length; j++)
            {
                if (grasps[0] is not null)
                    break;
                var b = bs[j];
                if (Custom.DistLess(head.pos + neckEndDir * 20f, b.pos, 20f + b.rad) && room.VisualContact(head.pos, b.pos))
                {
                    Grab(cr, 0, j, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                    JawOpen = .15f;
                    JawVel = 0f;
                    num = cr is not Player ? 1 : 2;
                    cr.Violence(head, Custom.DirVec(head.pos, b.pos) * 4f, b, null, DamageType.Bite, 4.4f, 0f);
                    break;
                }
            }
            if (cr is not DaddyLongLegs daddy)
                continue;
            var tents = daddy.tentacles;
            for (var k = 0; k < tents.Length; k++)
            {
                var tent = tents[k];
                var tchs = tent.tChunks;
                for (var l = 0; l < tchs.Length; l++)
                {
                    if (Custom.DistLess(head.pos + neckEndDir * 20f, tchs[l].pos, 20f))
                    {
                        tent.stun = Random.Range(10, 70);
                        for (var m = l; m < tchs.Length; m++)
                        {
                            var tch = tchs[m];
                            tch.vel += Custom.DirVec(tch.pos, tent.connectedChunk.pos) * Mathf.Lerp(10f, 50f, Random.value);
                        }
                        break;
                    }
                }
            }
        }
        switch (num)
        {
            case 0:
                room.PlaySound(SoundID.Miros_Beak_Snap_Miss, head, false, 1f, .7f);
                break;
            case 1:
                room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Slugcat, head, false, 1f, .7f);
                break;
            default:
                room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Other, head, false, 1f, .7f);
                break;
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (speed > 2.5f && firstContact)
            room.PlaySound(speed < 12f ? SoundID.Vulture_Light_Terrain_Impact : SoundID.Vulture_Heavy_Terrain_Impact, firstChunk);
    }

    public override void Stun(int st)
    {
        if (Random.value < Mathf.InverseLerp(st, 0f, 30f))
            LoseAllGrasps();
        base.Stun(st);
    }

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        var res = base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        if (res && graphicsModule is M4RJawsGraphics gr)
            gr.MoveJawsAbove = true;
        return res;
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var chs = bodyChunks;
        Neck.Reset(chs[0].pos);
        var holeDir = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        for (var i = 0; i < chs.Length; i++)
        {
            var ch = chs[i];
            ch.pos = newRoom.MiddleOfTile(pos) - holeDir * ((-1.5f + i) * 15f);
            ch.lastPos = newRoom.MiddleOfTile(pos);
            ch.vel = holeDir * 8f;
        }
        graphicsModule?.Reset();
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (!RippleViolenceCheck(source) || room is not Room rm)
            return;
        if (JawOpen > .1f && !dead && grasps[0] is null)
            rm.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, Head, false, .7f, 1.5f);
        AI?.Disencouraged += (damage * .25f + stunBonus * .015f) * (rm.game.StoryCharacter == SlugcatStats.Name.Yellow ? 1.5f : 1f);
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public override void Die()
    {
        if (!dead && grasps[0] is null)
            room?.PlaySound(NewSoundID.M4R_DoubleJaw_Hiss, Head, false, .7f, 1.5f);
        base.Die();
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        Neck.NewRoom(newRoom);
        EnterRoomHalf = RoomHalf(newRoom);
        var legs = Legs;
        for (var i = 0; i < legs.Length; i++)
            legs[i].Reset();
        PastPositions = [];
    }
}