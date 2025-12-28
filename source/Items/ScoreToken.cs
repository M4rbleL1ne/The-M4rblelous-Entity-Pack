using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using HUD;
using LBMergedMods.Hooks;
using System;

namespace LBMergedMods.Items;

public class ScoreToken : UpdatableAndDeletable, IDrawable
{
    public class TokenStalk : UpdatableAndDeletable, IDrawable
    {
        public const int BASE_SPRITE = 0, ARM1_SPRITE = 1, ARM2_SPRITE = 2, ARM3_SPRITE = 3, ARM4_SPRITE = 4, ARM5_SPRITE = 5,
            ARM_JOINT_SPRITE = 6, SOCKET_SPRITE = 7, HEAD_SPRITE = 8, LAMP_SPRITE = 9;
        public const float COORD_SEG = 3f;
        public ScoreToken? Token;
        public Vector2[][] Coord;
        public float[][] CurveLerps;
        public Vector2 HoverPos, BasePos, MainDir, ArmPos, LastArmPos, ArmVel, ArmGetToPos, Head, LastHead, HeadVel, HeadDir, LastHeadDir;
        public Color LampColor, LampOffCol;
        public float Flip, HeadDist = 15f, ArmLength, CoordLength, KeepDistance, SinCounter, LastSinCounter, LampPower, LastLampPower;
        public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData;
        public bool ForceSatellite;

        public virtual int TotalSprites => 10 + Coord.Length;

        public virtual float Alive
        {
            get
            {
                if (Token is not ScoreToken tk)
                    return 0f;
                return .25f + .75f * tk.Power;
            }
        }

        public TokenStalk(Room room, Vector2 hoverPos, Vector2 basePos, ScoreToken? token)
        {
            this.room = room;
            Token = token;
            HoverPos = hoverPos;
            BasePos = basePos;
            if (token is not null)
            {
                LampPower = 1f;
                LastLampPower = 1f;
            }
            LampColor = Color.Lerp(TokenColor, Color.white, .4f);
            var state = Random.state;
            Random.InitState((int)(hoverPos.x * 10f) + (int)(hoverPos.y * 10f));
            var lrps = CurveLerps = new float[2][];
            for (var i = 0; i < lrps.Length; i++)
                lrps[i] = [1f, 1f, 0f, 0f, 0f];
            lrps[0][3] = Random.value * 360f;
            lrps[1][3] = Mathf.Lerp(10f, 20f, Random.value);
            Flip = Random.value < .5f ? -1f : 1f;
            MainDir = Custom.DirVec(basePos, hoverPos);
            CoordLength = Vector2.Distance(basePos, hoverPos) * .6f;
            var crd = Coord = new Vector2[(int)(CoordLength / COORD_SEG)][];
            ArmLength = Vector2.Distance(basePos, hoverPos) / 2f;
            ArmGetToPos = LastArmPos = ArmPos = basePos + MainDir * ArmLength;
            for (var i = 0; i < crd.Length; i++)
                crd[i] = [ArmPos, ArmPos, default];
            LastHead = Head = hoverPos - MainDir * HeadDist;
            Random.state = state;
        }

        public virtual int CoordSprite(int s) => 10 + s;

        public override void Update(bool eu)
        {
            var curveLerps = CurveLerps;
            LastArmPos = ArmPos;
            ArmPos += ArmVel;
            ArmPos = Custom.MoveTowards(ArmPos, ArmGetToPos, (.8f + ArmLength / 150f) / 2f);
            ArmVel *= .8f;
            ArmVel += Vector2.ClampMagnitude(ArmGetToPos - ArmPos, 4f) / 11f;
            LastHead = Head;
            Head += HeadVel;
            HeadVel *= .8f;
            if (Token is ScoreToken tk && tk.slatedForDeletetion)
                Token = null;
            LastLampPower = LampPower;
            LastSinCounter = SinCounter;
            SinCounter += Random.value * LampPower;
            if (Token is not null)
                LampPower = Custom.LerpAndTick(LampPower, 1f, .02f, 1f / 60f);
            else
                LampPower = Mathf.Max(0f, LampPower - 1f / 120f);
            Vector2 vl;
            if (!Custom.DistLess(Head, ArmPos, CoordLength))
            {
                vl = Custom.DirVec(ArmPos, Head) * (Vector2.Distance(ArmPos, Head) - CoordLength) * .8f;
                HeadVel -= vl;
                Head -= vl;
            }
            HeadVel += (Vector2)Vector3.Slerp(Custom.DegToVec(GetCurveLerp(0, .5f, 1f)), Vector2.up, .4f) * .4f;
            LastHeadDir = HeadDir;
            var vector = HoverPos;
            if (Token is ScoreToken tk2 && tk2.Expand == 0f && !tk2.Contract)
                vector = Vector2.Lerp(vector, tk2.Pos, Alive);
            vl = Custom.DirVec(vector, Head) * (Vector2.Distance(vector, Head) - HeadDist) * .8f;
            HeadVel -= vl;
            Head -= vl;
            HeadDir = Custom.DirVec(Head, vector);
            if (Random.value < 1f / Mathf.Lerp(300f, 60f, Alive))
            {
                vl = BasePos + MainDir * ArmLength * .7f + Custom.RNV() * Random.value * ArmLength * Mathf.Lerp(.1f, .3f, Alive);
                if (SharedPhysics.RayTraceTilesForTerrain(room, ArmGetToPos, vl))
                    ArmGetToPos = vl;
                NewCurveLerp(0, curveLerps[0][3] + Mathf.Lerp(-180f, 180f, Random.value), Mathf.Lerp(1f, 2f, Alive));
                NewCurveLerp(1, Mathf.Lerp(10f, 20f, Mathf.Pow(Random.value, .75f)), Mathf.Lerp(.4f, .8f, Alive));
            }
            HeadDist = GetCurveLerp(1, .5f, 1f);
            if (Token is ScoreToken tk3)
                KeepDistance = Custom.LerpAndTick(KeepDistance, Mathf.Sin(Mathf.Clamp01(tk3.Glitch) * Mathf.PI) * Alive, .006f, Alive / (KeepDistance < tk3.Glitch ? 40f : 80f));
            HeadDist = Mathf.Lerp(HeadDist, 50f, Mathf.Pow(KeepDistance, .5f));
            var vector2 = Custom.DirVec(Custom.InverseKinematic(BasePos, ArmPos, ArmLength * .65f, ArmLength * .35f, Flip), ArmPos);
            var coord = Coord;
            for (var i = 0; i < coord.Length; i++)
            {
                var crd = coord[i];
                var num = Mathf.InverseLerp(-1f, coord.Length, i);
                var vector3 = Custom.Bezier(ArmPos, ArmPos + vector2 * CoordLength * .5f, Head, Head - HeadDir * CoordLength * .5f, num);
                crd[1] = crd[0];
                crd[0] += crd[2];
                crd[2] *= .8f;
                vl = (vector3 - crd[0]) * Mathf.Lerp(0f, .25f, Mathf.Sin(num * Mathf.PI));
                crd[2] += vl;
                crd[0] += vl;
                if (i > 2)
                {
                    var crd2 = coord[i - 2];
                    vl = Custom.DirVec(crd2[0], crd[0]);
                    crd[2] += vl;
                    crd2[2] -= vl;
                }
                if (i > 3)
                {
                    var crd3 = coord[i - 3];
                    vl = Custom.DirVec(crd3[0], crd[0]) * .5f;
                    crd[2] += vl;
                    crd3[2] -= vl;
                }
                if (num < .5f)
                    crd[2] += vector2 * Mathf.InverseLerp(.5f, 0f, num) * Mathf.InverseLerp(5f, 0f, i);
                else
                    crd[2] -= HeadDir * Mathf.InverseLerp(.5f, 1f, num);
            }
            ConnectCoord();
            ConnectCoord();
            for (var j = 0; j < coord.Length; j++)
            {
                var crd = coord[j];
                var cd = SharedPhysics.VerticalCollision(room, SharedPhysics.HorizontalCollision(room, ScratchTerrainCollisionData.Set(crd[0], crd[1], crd[2], 2f, default, true)));
                crd[0] = cd.pos;
                crd[2] = cd.vel;
            }
            for (var k = 0; k < curveLerps.Length; k++)
            {
                var cv = curveLerps[k];
                cv[1] = cv[0];
                cv[0] = Mathf.Min(1f, cv[0] + cv[4]);
            }
            base.Update(eu);
        }

        public virtual void NewCurveLerp(int curveLerp, float to, float speed)
        {
            var cv = CurveLerps[curveLerp];
            if (cv[0] >= 1f && cv[1] >= 1f)
            {
                cv[2] = cv[3];
                cv[3] = to;
                cv[4] = speed / Math.Abs(cv[2] - cv[3]);
                cv[1] = cv[0] = 0f;
            }
        }

        public virtual float GetCurveLerp(int curveLerp, float sCurveK, float timeStacker)
        {
            var cv = CurveLerps[curveLerp];
            return Mathf.Lerp(cv[2], cv[3], Custom.SCurve(Mathf.Lerp(cv[1], cv[0], timeStacker), sCurveK));
        }

        public virtual void ConnectCoord()
        {
            var coord = Coord;
            var c0 = coord[0];
            var vl = Custom.DirVec(ArmPos, c0[0]) * (Vector2.Distance(ArmPos, c0[0]) - COORD_SEG);
            c0[2] -= vl;
            c0[0] -= vl;
            for (var i = 1; i < coord.Length; i++)
            {
                var c = coord[i];
                var cm1 = coord[i - 1];
                if (!Custom.DistLess(cm1[0], c[0], COORD_SEG))
                {
                    var vector = Custom.DirVec(c[0], cm1[0]) * (Vector2.Distance(cm1[0], c[0]) - COORD_SEG) * .5f;
                    c[2] += vector;
                    c[0] += vector;
                    cm1[2] -= vector;
                    cm1[0] -= vector;
                }
            }
            c0 = coord[coord.Length - 1];
            vl = Custom.DirVec(Head, c0[0]) * (Vector2.Distance(Head, c0[0]) - COORD_SEG);
            c0[2] -= vl;
            c0[0] -= vl;
        }

        public virtual Vector2 EyePos(float timeStacker) => Vector2.Lerp(LastHead, Head, timeStacker) + (Vector2)Vector3.Slerp(LastHeadDir, HeadDir, timeStacker) * 3f;

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var sprites = sLeaser.sprites = new FSprite[TotalSprites];
            sprites[BASE_SPRITE] = new("Circle20")
            {
                scaleX = .5f,
                scaleY = .7f,
                rotation = Custom.VecToDeg(MainDir)
            };
            sprites[ARM1_SPRITE] = new("pixel")
            {
                scaleX = 4f,
                anchorY = 0f
            };
            sprites[ARM2_SPRITE] = new("pixel")
            {
                scaleX = 3f,
                anchorY = 0f
            };
            sprites[ARM3_SPRITE] = new("pixel")
            {
                scaleX = 1.5f,
                scaleY = ArmLength * .6f,
                anchorY = 0f
            };
            sprites[ARM4_SPRITE] = new("pixel")
            {
                scaleX = 3f,
                scaleY = 8f
            };
            sprites[ARM5_SPRITE] = new("pixel")
            {
                scaleX = 6f,
                scaleY = 8f
            };
            sprites[ARM_JOINT_SPRITE] = new("JetFishEyeA");
            sprites[LAMP_SPRITE] = new("tinyStar");
            sprites[SOCKET_SPRITE] = new("pixel")
            {
                scaleX = 5f,
                scaleY = 9f
            };
            sprites[HEAD_SPRITE] = new("pixel")
            {
                scaleX = 4f,
                scaleY = 6f
            };
            var l = Coord.Length;
            for (var i = 0; i < l; i++)
                sprites[CoordSprite(i)] = new("pixel")
                {
                    scaleX = i % 2 == 0 ? 2f : 3f,
                    scaleY = 5f
                };
            AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var sprites = sLeaser.sprites;
            var coord = Coord;
            sprites[BASE_SPRITE].SetPosition(BasePos - camPos);
            Vector2 head = Vector2.Lerp(LastHead, Head, timeStacker),
                headDir = Vector3.Slerp(LastHeadDir, HeadDir, timeStacker),
                armPos = Vector2.Lerp(LastArmPos, ArmPos, timeStacker),
                vector3 = Custom.InverseKinematic(BasePos, armPos, ArmLength * .65f, ArmLength * .35f, Flip);
            var spr = sprites[ARM1_SPRITE];
            spr.SetPosition(BasePos - camPos);
            spr.scaleY = Vector2.Distance(BasePos, vector3);
            spr.rotation = Custom.AimFromOneVectorToAnother(BasePos, vector3);
            spr = sprites[ARM2_SPRITE];
            spr.SetPosition(vector3 - camPos);
            spr.scaleY = Vector2.Distance(vector3, armPos);
            spr.rotation = Custom.AimFromOneVectorToAnother(vector3, armPos);
            spr = sprites[SOCKET_SPRITE];
            spr.SetPosition(armPos - camPos);
            var c0 = coord[0];
            spr.rotation = Custom.VecToDeg(Vector3.Slerp(Custom.DirVec(vector3, armPos), Custom.DirVec(armPos, Vector2.Lerp(c0[1], c0[0], timeStacker)), .4f));
            Vector2 p = Vector2.Lerp(BasePos, vector3, .3f),
                p2 = Vector2.Lerp(vector3, armPos, .4f);
            spr = sprites[ARM3_SPRITE];
            spr.SetPosition(p - camPos);
            spr.rotation = Custom.AimFromOneVectorToAnother(p, p2);
            spr = sprites[ARM4_SPRITE];
            spr.SetPosition(p2 - camPos);
            spr.rotation = Custom.AimFromOneVectorToAnother(p, p2);
            p += Custom.DirVec(BasePos, vector3) * (ArmLength * .1f + 2f);
            spr = sprites[ARM5_SPRITE];
            spr.SetPosition(p - camPos);
            spr.rotation = Custom.AimFromOneVectorToAnother(BasePos, vector3);
            spr = sprites[LAMP_SPRITE];
            spr.SetPosition(p - camPos);
            spr.color = Color.Lerp(LampOffCol, LampColor, Mathf.Lerp(LastLampPower, LampPower, timeStacker) * Mathf.Pow(Random.value, .5f) * (.5f + .5f * Mathf.Sin(Mathf.Lerp(LastSinCounter, SinCounter, timeStacker) / 6f)));
            sprites[ARM_JOINT_SPRITE].SetPosition(vector3 - camPos);
            spr = sprites[HEAD_SPRITE];
            spr.SetPosition(head - camPos);
            spr.rotation = Custom.VecToDeg(headDir);
            Vector2 p3 = armPos;
            for (var i = 0; i < coord.Length; i++)
            {
                var crd = coord[i];
                var vector4 = Vector2.Lerp(crd[1], crd[0], timeStacker);
                spr = sprites[CoordSprite(i)];
                spr.SetPosition(vector4 - camPos);
                spr.rotation = Custom.AimFromOneVectorToAnother(p3, vector4);
                p3 = vector4;
            }
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var sprites = sLeaser.sprites;
            var col = palette.blackColor;
            for (var i = 0; i < sprites.Length; i++)
                sprites[i].color = col;
            LampOffCol = Color.Lerp(col, Color.white, .15f);
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            var sprites = sLeaser.sprites;
            newContainer ??= rCam.ReturnFContainer("Midground");
            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                sprite.RemoveFromContainer();
                newContainer.AddChild(sprite);
            }
        }
    }

    public class TokenSpark : CosmeticSprite
    {
        public Color Color;
        public Vector2 LastLastPos;
        public float Dir, Life, LifeTime;
        public bool Underwater;

        public TokenSpark(Vector2 pos, Vector2 vel, Color color, bool underWater)
        {
            this.pos = pos;
            this.vel = vel;
            Color = color;
            Underwater = underWater;
            lastPos = pos;
            LastLastPos = pos;
            LifeTime = Mathf.Lerp(20f, 40f, Random.value);
            Life = 1f;
            Dir = Custom.VecToDeg(vel.normalized);
        }

        public override void Update(bool eu)
        {
            LastLastPos = lastPos;
            base.Update(eu);
            Dir += Mathf.Lerp(-1f, 1f, Random.value) * 50f;
            vel *= .8f;
            vel += Custom.DegToVec(Dir) * Mathf.Lerp(.2f, .2f, Life);
            Life -= 1f / LifeTime;
            if (Life < 0f)
                Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [new FSprite("pixel") { color = Color, anchorY = 0, alpha = Underwater ? .5f : 1f }];
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker),
                vector2 = Vector2.Lerp(LastLastPos, lastPos, timeStacker);
            var spr = sLeaser.sprites[0];
            spr.SetPosition(vector - camPos);
            spr.scaleY = Vector2.Distance(vector, vector2) * Mathf.InverseLerp(0f, .5f, Life);
            spr.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
            spr.isVisible = Random.value < Mathf.InverseLerp(0f, .5f, Life);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Foreground");
            var spr = sLeaser.sprites[0];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public const int LIGHT_SPRITE = 0, MAIN_SPRITE = 1, TRAIL_SPRITE = 2, GOLD_SPRITE = 9, TOTAL_SPRITES = 10;
    public static Color TokenColor = new(193f / 255f, 242f / 255f, 17f / 255f);
    public Player? TargetPlayer;
    public PlacedObject PlacedObj;
    public TokenStalk Stalk;
    public StaticSoundLoop SoundLoop, GlitchLoop;
    public Vector2[][] Lines;
    public Vector2[] Trail;
    public Vector2 HoverPos, Pos, LastPos, Vel;
    public float SinCounter, SinCounter2, Expand, LastExpand, Glitch, LastGlitch, GeneralGlitch, Power, LastPower;
    public int LockdownCounter, Score;
    public bool Contract, Underwater, PoweredOn, Locked, AnythingUnlocked;

    public ScoreToken(Room room, PlacedObject placedObj)
    {
        PlacedObj = placedObj;
        base.room = room;
        var data = (placedObj.data as ScoreTokenData)!;
        Score = data.Score;
        room.AddObject(Stalk = new(room, placedObj.pos, placedObj.pos + data.handlePos, this));
        var pos = LastPos = HoverPos = Pos = placedObj.pos;
        var lns = Lines = new Vector2[6][];
        for (var i = 0; i < lns.Length; i++)
            lns[i] = [pos, pos, default, default];
        lns[0][2] = new(-7f, -4f);
        lns[1][2] = new(-7f, 4f);
        lns[2][2].y = 10f;
        lns[3][2] = new(7f, 4f);
        lns[4][2] = new(7f, -4f);
        lns[5][2].y = -10f;
        Trail = [pos, pos, pos, pos, pos];
        SoundLoop = new(SoundID.Token_Idle_LOOP, Pos, room, 0f, 1f);
        GlitchLoop = new(SoundID.Token_Upset_LOOP, Pos, room, 0f, 1f);
    }

    public virtual int LineSprite(int line) => 3 + line;

    public override void Update(bool eu)
    {
        if (room is not Room rm || rm.game is not RainWorldGame game || game.session is not StoryGameSession session)
            return;
        if (ModManager.MMF && !AvailableToPlayer(rm))
        {
            Stalk.Destroy();
            Destroy();
        }
        Underwater = rm.PointSubmerged(Pos);
        SinCounter += Random.value * Power;
        SinCounter2 += (1f + Mathf.Lerp(-10f, 10f, Random.value) * Glitch) * Power;
        var f = Mathf.Sin(SinCounter2 / 20f);
        f = Mathf.Pow(Math.Abs(f), .5f) * Mathf.Sign(f);
        var snd = SoundLoop;
        snd.Update();
        snd.pos = Pos;
        snd.pitch = 1f + .25f * f * Glitch;
        snd.volume = Mathf.Pow(Power, .5f) * Mathf.Pow(1f - Glitch, .5f);
        snd = GlitchLoop;
        snd.Update();
        snd.pos = Pos;
        snd.pitch = Mathf.Lerp(.75f, 1.25f, Glitch) - .25f * f * Glitch;
        snd.volume = Mathf.Pow(Mathf.Sin(Mathf.Clamp(Glitch, 0f, 1f) * Mathf.PI), .1f) * Mathf.Pow(Power, .1f);
        LastPos = Pos;
        var lns = Lines;
        int i;
        Vector2[] ln;
        for (i = 0; i < lns.Length; i++)
        {
            ln = lns[i];
            ln[1] = ln[0];
        }
        LastGlitch = Glitch;
        LastExpand = Expand;
        ln = Trail;
        for (i = ln.Length - 1; i >= 1; i--)
            ln[i] = ln[i - 1];
        ln[0] = LastPos;
        LastPower = Power;
        Power = Custom.LerpAndTick(Power, PoweredOn ? 1f : 0f, .07f, .025f);
        Glitch = Mathf.Max(Glitch, 1f - Power);
        Pos += Vel;
        for (i = 0; i < lns.Length; i++)
        {
            ln = lns[i];
            if (Stalk is TokenStalk st)
                ln[0] += st.Head - st.LastHead;
            if (Mathf.Pow(Random.value, .1f + Glitch * 5f) > ln[3].x)
                ln[0] = Vector2.Lerp(ln[0], Pos + new Vector2(ln[2].x * f, ln[2].y), Mathf.Pow(Random.value, 1f + ln[3].x * 17f));
            if (Random.value < Mathf.Pow(ln[3].x, .2f) && Random.value < Mathf.Pow(Glitch, .8f - .4f * ln[3].x))
            {
                ln[0] += Custom.RNV() * 17f * ln[3].x * Power;
                ln[3].y = Mathf.Max(ln[3].y, Glitch);
            }
            ln[3].x = Custom.LerpAndTick(ln[3].x, ln[3].y, .01f, 1f / 30f);
            ln[3].y = Mathf.Max(0f, ln[3].y - 1f / 70f);
            if (Random.value < 1f / Mathf.Lerp(210f, 20f, Glitch))
                ln[3].y = Mathf.Max(Glitch, Random.value < .5f ? GeneralGlitch : Random.value);
        }
        Vel *= .995f;
        Vel += Vector2.ClampMagnitude(HoverPos + new Vector2(0f, Mathf.Sin(SinCounter / 15f) * 7f) - Pos, 15f) / 81f + Custom.RNV() * Random.value * Random.value * Mathf.Lerp(.06f, .4f, Glitch);
        Pos += Custom.RNV() * Mathf.Pow(Random.value, 7f - 6f * GeneralGlitch) * Mathf.Lerp(.06f, 1.2f, Glitch);
        if (TargetPlayer is Player p)
        {
            p.Blink(5);
            var mbcPos = p.mainBodyChunk.pos;
            var ch1Pos = p.bodyChunks[1].pos;
            if (!Contract)
            {
                Expand += 1f / 30f;
                if (Expand > 1f)
                {
                    Expand = 1f;
                    Contract = true;
                }
                GeneralGlitch = 0f;
                Glitch = Custom.LerpAndTick(Glitch, Expand * .5f, .07f, 1f / 15f);
                var num2 = Custom.SCurve(Mathf.InverseLerp(.35f, .55f, Expand), .4f);
                var b = Vector2.Lerp(mbcPos + new Vector2(0f, 40f), Vector2.Lerp(ch1Pos, mbcPos + Custom.DirVec(ch1Pos, mbcPos) * 10f, .65f), Expand);
                for (i = 0; i < lns.Length; i++)
                {
                    ln = lns[i];
                    var vector = Vector2.Lerp(ln[2] * (2f + 5f * Mathf.Pow(Expand, .5f)), Custom.RotateAroundOrigo(ln[2] * (2f + 2f * Mathf.Pow(Expand, .5f)), Custom.AimFromOneVectorToAnother(ch1Pos, mbcPos)), num2);
                    ln[0] = Vector2.Lerp(ln[0], Vector2.Lerp(Pos, b, Mathf.Pow(num2, 2f)) + vector, Mathf.Pow(Expand, .5f));
                    ln[3] *= 1f - Expand;
                }
                HoverPos = Vector2.Lerp(HoverPos, b, Mathf.Pow(Expand, 2f));
                Pos = Vector2.Lerp(Pos, b, Mathf.Pow(Expand, 2f));
                Vel *= 1f - Expand;
            }
            else
            {
                GeneralGlitch *= 1f - Expand;
                Glitch = .15f;
                Expand -= 1f / Mathf.Lerp(60f, 2f, Expand);
                var vector2 = Vector2.Lerp(ch1Pos, mbcPos + Custom.DirVec(ch1Pos, mbcPos) * 10f, Mathf.Lerp(1f, .65f, Expand));
                for (i = 0; i < lns.Length; i++)
                {
                    ln = lns[i];
                    var vector3 = Custom.RotateAroundOrigo(Vector2.Lerp(Random.value > Expand ? ln[2] : lns[Random.Range(0, lns.Length)][2], lns[Random.Range(0, lns.Length)][2], Random.value * (1f - Expand)) * (4f * Mathf.Pow(Expand, .25f)), Custom.AimFromOneVectorToAnother(ch1Pos, mbcPos)) * Mathf.Lerp(Random.value, 1f, Expand);
                    ln[0] = vector2 + vector3;
                    ln[3] *= 1f - Expand;
                }
                HoverPos = Pos = vector2;
                if (Expand < 0f)
                {
                    Destroy();
                    for (i = 0; i < 20; i++)
                        rm.AddObject(new TokenSpark(Pos + Custom.RNV() * 2f, Custom.RNV() * 16f * Random.value, Color.Lerp(TokenColor, Color.white, .5f + .5f * Random.value), Underwater));
                    rm.PlaySound(SoundID.Token_Collected_Sparks, Pos);
                    if (AnythingUnlocked && session.saveState?.deathPersistentSaveData is DeathPersistentSaveData dt && ScoreData.TryGetValue(dt, out var data) && game.cameras[0].hud?.textPrompt is TextPrompt prompt)
                    {
                        data.Score += Score;
                        prompt.AddMessage(game.rainWorld.inGameTranslator.Translate("Score:") + " +" + Score, 20, 160, true, true);
                    }
                }
            }
        }
        else
        {
            GeneralGlitch = Mathf.Max(0f, GeneralGlitch - 1f / 120f);
            if (Random.value < .0027027028f)
                GeneralGlitch = Random.value;
            if (!Custom.DistLess(Pos, HoverPos, 11f))
                Pos += Custom.DirVec(HoverPos, Pos) * (11f - Vector2.Distance(Pos, HoverPos)) * .7f;
            var f2 = Mathf.Sin(Mathf.Clamp(Glitch, 0f, 1f) * Mathf.PI);
            if (Random.value < .05f + .35f * Mathf.Pow(f2, .5f) && Random.value < Power)
                rm.AddObject(new TokenSpark(Pos + Custom.RNV() * 6f * Glitch, Custom.RNV() * Mathf.Lerp(2f, 9f, Mathf.Pow(f2, 2f)) * Random.value, GoldCol(Glitch), Underwater));
            Glitch = Custom.LerpAndTick(Glitch, GeneralGlitch / 2f, .01f, 1f / 30f);
            if (Random.value < 1f / Mathf.Lerp(360f, 10f, GeneralGlitch))
                Glitch = Mathf.Pow(Random.value, 1f - .85f * GeneralGlitch);
            var num3 = float.MaxValue;
            var flag = AvailableToPlayer(rm);
            if (RainWorld.lockGameTimer)
                flag = false;
            var num4 = 140f;
            var players = session.Players;
            for (var n = 0; n < players.Count; n++)
            {
                if (players[n].realizedCreature is not Player pl || !pl.Consious || pl.dangerGrasp is not null || pl.room != rm)
                    continue;
                var mbcPos = pl.mainBodyChunk.pos;
                num3 = Mathf.Min(num3, Vector2.Distance(mbcPos, Pos));
                if (!flag)
                    continue;
                if (Custom.DistLess(mbcPos, Pos, 18f))
                {
                    Pop(pl, rm);
                    break;
                }
                if (Custom.DistLess(mbcPos, Pos, num4))
                {
                    if (Custom.DistLess(Pos, HoverPos, 80f))
                        Pos += Custom.DirVec(Pos, mbcPos) * Custom.LerpMap(Vector2.Distance(Pos, mbcPos), 40f, num4, 2.2f, 0f, .5f) * Random.value;
                    if (Random.value < .05f && Random.value < Mathf.InverseLerp(num4, 40f, Vector2.Distance(Pos, mbcPos)))
                        Glitch = Mathf.Max(Glitch, Random.value * .5f);
                }
            }
            if (!flag && PoweredOn)
            {
                ++LockdownCounter;
                if (Random.value < 1f / 60f || num3 < num4 - 40f || LockdownCounter > 30)
                    Locked = true;
                if (Random.value < 1f / 7f)
                    Glitch = Mathf.Max(Glitch, Random.value * Random.value * Random.value);
            }
            if (PoweredOn && (Locked || (Expand == 0f && !Contract && Random.value < Mathf.InverseLerp(num4 + 160f, num4 + 460f, num3))))
            {
                PoweredOn = false;
                rm.PlaySound(SoundID.Token_Turn_Off, Pos);
            }
            else if (!PoweredOn && !Locked && Random.value < Mathf.InverseLerp(num4 + 60f, num4 - 20f, num3))
            {
                PoweredOn = true;
                rm.PlaySound(SoundID.Token_Turn_On, Pos);
            }
        }
        base.Update(eu);
    }

    public virtual bool AvailableToPlayer(Room rm) => rm.game?.StoryCharacter is SlugcatStats.Name nm && !(PlacedObj.data as ScoreTokenData)!.UnavailableToPlayers.Contains(nm);

    public virtual void Pop(Player player, Room rm)
    {
        if (Expand > 0f)
            return;
        TargetPlayer = player;
        Expand = .01f;
        rm.PlaySound(SoundID.Token_Collect, Pos);
        var mbcPos = player.mainBodyChunk.pos;
        AnythingUnlocked = rm.game?.GetStorySession?.saveState?.deathPersistentSaveData?.SetScoreTokenCollected((PlacedObj.data as ScoreTokenData)!.ID) is true;
        for (var i = 0; i < 10; i++)
            rm.AddObject(new TokenSpark(Pos + Custom.RNV() * 2f, Custom.RNV() * 11f * Random.value + Custom.DirVec(mbcPos, Pos) * 5f * Random.value, GoldCol(Glitch), Underwater));
    }

    public virtual Color GoldCol(float g) => Color.Lerp(TokenColor, Color.white, .4f + .4f * Mathf.Max(Contract ? .5f : (Expand * .5f), Mathf.Pow(g, .5f)));

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprites[LIGHT_SPRITE] = new("Futile_White");
        sprites[GOLD_SPRITE] = new("Futile_White")
        {
            color = Color.Lerp(Color.black, RainWorld.GoldRGB, .2f),
            shader = Custom.rainWorld.Shaders["FlatLight"]
        };
        sprites[MAIN_SPRITE] = new("JetFishEyeA")
        {
            shader = Custom.rainWorld.Shaders["Hologram"]
        };
        sprites[TRAIL_SPRITE] = new("JetFishEyeA")
        {
            shader = Custom.rainWorld.Shaders["Hologram"]
        };
        for (var i = 0; i < 6; i++)
            sprites[LineSprite(i)] = new("pixel")
            {
                anchorY = 0f,
                shader = Custom.rainWorld.Shaders["Hologram"]
            };
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprites = sLeaser.sprites;
        var vector = Vector2.Lerp(LastPos, Pos, timeStacker);
        float num = Mathf.Lerp(LastGlitch, Glitch, timeStacker),
            num2 = Mathf.Lerp(LastExpand, Expand, timeStacker),
            num3 = Mathf.Lerp(LastPower, Power, timeStacker);
        if (room is Room rm && !AvailableToPlayer(rm))
        {
            num = Mathf.Lerp(num, 1f, Random.value);
            num3 *= .3f + .7f * Random.value;
        }
        var gold = sprites[GOLD_SPRITE];
        gold.SetPosition(vector - camPos);
        gold.alpha = .75f * Mathf.Lerp(Mathf.Lerp(.8f, .5f, Mathf.Pow(num, .6f + .2f * Random.value)), .7f, num2) * num3;
        gold.scale = Mathf.Lerp(110f, 300f, num2) / 16f;
        var color = GoldCol(num);
        var main = sprites[MAIN_SPRITE];
        main.color = color;
        main.SetPosition(vector - camPos);
        main.alpha = (1f - num) * Mathf.InverseLerp(.5f, 0f, num2) * num3 * (Underwater ? .5f : 1f);
        var trail = sprites[TRAIL_SPRITE];
        trail.color = color;
        trail.SetPosition(Vector2.Lerp(Trail[Trail.Length - 1], Trail[Trail.Length - 2], timeStacker) - camPos);
        trail.alpha = .75f * (1f - num) * Mathf.InverseLerp(.5f, 0f, num2) * num3 * (Underwater ? .5f : 1f);
        trail.scaleX = Random.value < num ? (1f + 20f * Random.value * Glitch) : 1f;
        trail.scaleY = Random.value < num ? (1f + 2f * Random.value * Random.value * Glitch) : 1f;
        var light = sprites[LIGHT_SPRITE];
        light.SetPosition(vector - camPos);
        if (Underwater)
        {
            light.shader = Custom.rainWorld.Shaders["UnderWaterLight"];
            light.alpha = Mathf.Pow(.9f * (1f - num) * Mathf.InverseLerp(.5f, 0f, num2) * num3, .5f);
            light.scale = Mathf.Lerp(60f, 120f, num) / 16f;
        }
        else
        {
            light.shader = Custom.rainWorld.Shaders["FlatLight"];
            light.alpha = .9f * (1f - num) * Mathf.InverseLerp(.5f, 0f, num2) * num3;
            light.scale = Mathf.Lerp(20f, 40f, num) / 16f;
        }
        light.color = Color.Lerp(TokenColor, color, .4f);
        trail.isVisible = light.isVisible = main.isVisible = !Contract && num3 > 0f;
        var lns = Lines;
        var last = lns.Length - 1;
        for (var i = 0; i < lns.Length; i++)
        {
            var ln = lns[i];
            var vector2 = Vector2.Lerp(ln[1], ln[0], timeStacker);
            var num4 = i != last ? (i + 1) : 0;
            var ln2 = lns[num4];
            var vector3 = Vector2.Lerp(ln2[1], ln2[0], timeStacker);
            var f = Mathf.Pow(Mathf.Max(ln2[3].x, ln2[3].x) * (1f - num), 2f) * (1f - num2);
            if (Random.value < f)
            {
                vector3 = Vector2.Lerp(vector2, vector3, Random.value);
                if (Stalk is TokenStalk st)
                    vector2 = st.EyePos(timeStacker);
                if (TargetPlayer?.mainBodyChunk is BodyChunk mbc && (Random.value < Expand || Contract))
                    vector2 = Vector2.Lerp(mbc.lastPos, mbc.pos, timeStacker);
            }
            var lnSpr = sprites[LineSprite(i)];
            lnSpr.SetPosition(vector2 - camPos);
            lnSpr.scaleY = Vector2.Distance(vector2, vector3);
            lnSpr.rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
            lnSpr.alpha = (1f - f) * num3 * (Underwater ? .2f : 1f);
            lnSpr.color = color;
            lnSpr.isVisible = num3 > 0f;
        }
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("GrabShaders");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
            sprites[i].RemoveFromContainer();
        newContainer.AddChild(sprites[GOLD_SPRITE]);
        newContainer.AddChild(sprites[LIGHT_SPRITE]);
        newContainer.AddChild(sprites[MAIN_SPRITE]);
        newContainer.AddChild(sprites[TRAIL_SPRITE]);
        for (var l = 0; l < 6; l++)
            newContainer.AddChild(sprites[LineSprite(l)]);
    }
}

/*public static class ScoreTokenUtils
{
    static List<SlugcatStats.Name> s_emptyList = [];

    static void On_RainWorld_ReadTokenCache(On.RainWorld.orig_ReadTokenCache orig, RainWorld self)
    {
        orig(self);
        if (!RegionScoreData.TryGetValue(self, out var data))
            RegionScoreData.Add(self, data = new());
        var regions = AssetManager.ResolveFilePath(Path.Combine("world", "regions.txt"));
        if (!File.Exists(regions))
            return;
        var array = File.ReadAllLines(regions);
        for (var i = 0; i < array.Length; i++)
        {
            var text = array[i].ToLowerInvariant();
            var tks = data.RegionScoreTokens[text] = [];
            var access = data.RegionScoreTokensAccessibility[text] = [];
            var pth = Path.Combine("mpkscoretokens", "tokencache" + text + ".txt");
            var path = AssetManager.ResolveFilePath(pth);
            if (!File.Exists(path))
            {
                EmergencyBuildScoreTokenCache(data, text);
                path = AssetManager.ResolveFilePath(pth);
            }
            var ar1 = File.ReadAllText(path).Split(',');
            for (var k = 0; k < ar1.Length; k++)
            {
                var ar2 = ar1[k].Split('~');
                if (ar2.Length >= 1)
                {
                    tks.Add(ar2[0]);
                    if (ar2.Length >= 2)
                    {
                        var list = new List<SlugcatStats.Name>();
                        var ar3 = ar2[1].Split('|');
                        for (var l = 0; l < ar3.Length; l++)
                            list.Add(new(ar3[l]));
                        access.Add(list);
                    }
                    else
                        access.Add([]);
                }
            }
        }
    }

    static void On_RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
    {
        orig(self);
        if (RegionScoreData.TryGetValue(self, out var data))
        {
            data.RegionScoreTokens.Clear();
            data.RegionScoreTokensAccessibility.Clear();
        }
    }

    public static void PartialFromString(this PlacedObject self, string[] s)
    {
        self.type = new(s[0]);
        try
        {
            self.data?.FromString(s[3]);
        }
        catch { }
    }

    public static void EmergencyBuildScoreTokenCache(ScoreTokens data, string regionLowercase)
    {
        //lock (data)
        //{
            var tks = data.RegionScoreTokens[regionLowercase] = [];
            var accesses = data.RegionScoreTokensAccessibility[regionLowercase] = [];
        //}
        var array = AssetManager.ListDirectory(Path.Combine("world", regionLowercase + "-rooms"));
        List<string> list = [], list2 = [];
        for (var i = 0; i < array.Length; i++)
        {
            var item = array[i];
            var fl = Path.GetFileName(item);
            if (fl.Contains("settings"))
            {
                list.Add(item);
                if (fl.Contains("settings-"))
                    list2.Add(item);
            }
        }
        var txtInfo = CultureInfo.InvariantCulture.TextInfo;
        for (var j = 0; j < list.Count; j++)
        {
            var item = list[j];
            var list3 = new List<SlugcatStats.Name>();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
            string? text2 = null;
            var entries = ExtEnum<SlugcatStats.Name>.values.entries;
            if (list2.Contains(item))
            {
                text2 ??= txtInfo.ToTitleCase(fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf("settings-") + 9));
                if (entries.Contains(text2))
                    list3.Add(new(text2));
            }
            else
            {
                var list4 = new List<SlugcatStats.Name>();
                for (var u = 0; u < list2.Count; u++)
                {
                    var item2 = list2[u];
                    if (item2.Contains(fileNameWithoutExtension))
                    {
                        text2 ??= txtInfo.ToTitleCase(fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf("settings-") + 9));
                        if (entries.Contains(text2))
                            list4.Add(new(text2));
                    }
                }
                for (var u = 0; u < entries.Count; u++)
                {
                    var name = new SlugcatStats.Name(entries[u]);
                    if ((!ModManager.MSC || name != MoreSlugcatsEnums.SlugcatStatsName.Slugpup) && !list4.Contains(name))
                        list3.Add(name);
                }
            }
            var array2 = File.ReadAllLines(item);
            var list5 = new List<string[]>();
            for (var k = 0; k < array2.Length; k++)
            {
                var array3 = Custom.ValidateSpacedDelimiter(array2[k], ":").Split([": "], StringSplitOptions.None);
                if (array3.Length == 2)
                    list5.Add(array3);
            }
            for (var l = 0; l < list5.Count; l++)
            {
                var item5 = list5[l];
                if (item5[0] == "PlacedObjects")
                    continue;
                var array4 = Custom.ValidateSpacedDelimiter(item5[1], ",").Split([", "], StringSplitOptions.None);
                for (var m = 0; m < array4.Length; m++)
                {
                    var array5 = Regex.Split(array4[m].Trim(), "><");
                    if (array5.Length < 4)
                        continue;
                    var pObj = new PlacedObject(PlacedObject.Type.None, null);
                    pObj.PartialFromString(array5);
                    if (pObj.type == BonusScoreToken)
                    {
                        var pData = (pObj.data as ScoreToken.TokenData)!;
                        var id = pData.ID;
                        var index = tks.IndexOf(id);
                        if (index == -1)
                        {
                            tks.Add(id);
                            accesses.Add(FilterScoreTokenClearance(pData.UnavailableToPlayers, s_emptyList, list3));
                        }
                        else
                            accesses[index] = FilterScoreTokenClearance(pData.UnavailableToPlayers, accesses[index], list3);
                    }
                }
                break;
            }
        }
        var folder = Path.Combine(Custom.RootFolderDirectory(), "mergedmods", "mpkscoretokens").ToLowerInvariant();
        Directory.CreateDirectory(folder);
        folder += Path.DirectorySeparatorChar;
        var sb = new StringBuilder();
        for (var n = 0; n < tks.Count; n++)
        {
            var access = accesses[n];
            if (n > 0)
                sb.Append(',');
            sb.Append(tks[n]).Append('~');
            for (var i = 0; i < access.Count; i++)
            {
                if (access[i]?.value is string s)
                {
                    if (i > 0)
                        sb.Append('|');
                    sb.Append(s);
                }
            }
        }
        File.WriteAllText(folder + "tokencache" + regionLowercase + ".txt", sb.ToString());
    }

    static void IL_RainWorld_BuildTokenCache(ILContext il)
    {
        var c = new ILCursor(il);
        var loc = 0;
        var vars = il.Body.Variables;
        FieldReference? field = null;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<RainWorld>(nameof(RainWorld.regionGoldTokens)),
            x => x.MatchLdloc(out loc),
            x => x.MatchLdfld(out field),
            x => x.MatchNewobj(out _),
            x => x.MatchCallOrCallvirt(out _))
            && field is not null)
        {
            var local1 = vars[loc];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, local1)
             .Emit(OpCodes.Ldfld, field)
             .EmitDelegate((RainWorld self, string fileName) =>
             {
                 if (!RegionScoreData.TryGetValue(self, out var data))
                     RegionScoreData.Add(self, data = new());
                 data.RegionScoreTokens[fileName] = [];
                 data.RegionScoreTokensAccessibility[fileName] = [];
             });
            int locOld = 0, locList = 0;
            if (!c.TryGotoNext(MoveType.After,
                 x => x.MatchNewobj<List<string>>()) ||
                !c.TryGotoNext(MoveType.After,
                 x => x.MatchNewobj<List<SlugcatStats.Name>>(),
                 x => x.MatchStloc(out locList)) ||
                !c.TryGotoNext(MoveType.After,
                 x => x.MatchNewobj<List<string[]>>()) ||
                !c.TryGotoNext(MoveType.After,
                 x => x.MatchNewobj<List<SlugcatStats.Name>>(),
                 x => x.MatchStloc(out locOld)))
            {
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook RainWorld.BuildTokenCache (part 2)!");
                return;
            }
            var loc2 = 0;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out loc2),
                x => x.MatchLdfld<PlacedObject>(nameof(PlacedObject.type)),
                x => x.MatchLdsfld<PlacedObject.Type>(nameof(PlacedObject.Type.GoldToken)),
                x => x.MatchCall(out _))
            && field is not null)
            {
                c.Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldloc, local1)
                 .Emit(OpCodes.Ldfld, field)
                 .Emit(OpCodes.Ldloc, vars[loc2])
                 .Emit(OpCodes.Ldloc, vars[locOld])
                 .Emit(OpCodes.Ldloc, vars[locList])
                 .EmitDelegate((bool flag, RainWorld self, string fileName, PlacedObject placedObject, List<SlugcatStats.Name> oldData, List<SlugcatStats.Name> list3) =>
                 {
                     if (flag)
                         return true;
                     if (RegionScoreData.TryGetValue(self, out var data) && placedObject.type == BonusScoreToken)
                     {
                         var tokens = data.RegionScoreTokens[fileName];
                         var pData = (placedObject.data as ScoreToken.TokenData)!;
                         var id = pData.ID;
                         var index = tokens.IndexOf(id);
                         var access = data.RegionScoreTokensAccessibility[fileName];
                         if (index == -1)
                         {
                             tokens.Add(id);
                             access.Add(FilterScoreTokenClearance(pData.UnavailableToPlayers, oldData, list3));
                         }
                         else
                             access[index] = FilterScoreTokenClearance(pData.UnavailableToPlayers, access[index], list3);
                     }
                     return false;
                 });
                c.Index = il.Body.Instructions.Count - 1;
                c.Emit(OpCodes.Ldarg_0)
                 //.Emit(OpCodes.Ldarg_1)
                 .Emit(OpCodes.Ldloc, local1)
                 .Emit(OpCodes.Ldfld, field)
                 .EmitDelegate((RainWorld self, /*bool modded,*
string fileName) =>
                 {
                     if (RegionScoreData.TryGetValue(self, out var data))
                     {
                         /*string folder;
                         if (modded)
                         {*
                             var folder = Path.Combine(Custom.RootFolderDirectory(), "mergedmods", "mpkscoretokens").ToLowerInvariant();
Directory.CreateDirectory(folder);
                             folder += Path.DirectorySeparatorChar;
                         /*}
                         else
                             folder = Path.Combine(Custom.RootFolderDirectory(), "mpkscoretokens").ToLowerInvariant() + Path.DirectorySeparatorChar;*
                         var sb = new StringBuilder();
var tks = data.RegionScoreTokens[fileName];
var accesses = data.RegionScoreTokensAccessibility[fileName];
                         for (var n = 0; n<tks.Count; n++)
                         {
                             var access = accesses[n];
                             if (n > 0)
                                 sb.Append(',');
                             sb.Append(tks[n]).Append('~');
                             for (var i = 0; i<access.Count; i++)
                             {
                                 if (access[i]?.value is string s)
                                 {
                                     if (i > 0)
                                         sb.Append('|');
                                     sb.Append(s);
                                 }
                             }
                         }
                         File.WriteAllText(folder + "tokencache" + fileName + ".txt", sb.ToString());
                     }
                 });
            }
            else
    LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook RainWorld.BuildTokenCache (part 3)!");
        }
        else
    LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook RainWorld.BuildTokenCache (part 1)!");
    }

    public static List<SlugcatStats.Name> FilterScoreTokenClearance(List<SlugcatStats.Name> unavailableToPlayers, List<SlugcatStats.Name> oldData, List<SlugcatStats.Name> filterSlots)
{
    var list = new List<SlugcatStats.Name>();
    var entries = ExtEnum<SlugcatStats.Name>.values.entries;
    for (var i = 0; i < entries.Count; i++)
    {
        var name = new SlugcatStats.Name(entries[i]);
        if (filterSlots.Contains(name))
        {
            if (!unavailableToPlayers.Contains(name))
                list.Add(name);
        }
        else if (oldData.Contains(name))
            list.Add(name);
    }
    return list;
}
}
*/