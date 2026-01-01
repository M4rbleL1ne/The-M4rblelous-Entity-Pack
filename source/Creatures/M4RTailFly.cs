using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class M4RTailFly : Hoverfly, IClimbableVine
{
    public int TailLength;

    public M4RTailFly(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        TailLength = Random.Range(5, 8);
        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (Consious)
            Vector2.ClampMagnitude(firstChunk.vel, 5f);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new M4RTailFlyGraphics(this);

    public override void GenerateIVars()
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        IVars = new(Random.value * .2f, -.1f - .15f * Random.value, Random.value, .1f + Random.value * .15f, -.3f + Random.value * .1f, 1, Color.Lerp(new(248f / 255f, 210f / 255f, 0f), new(248f / 255f, 245f / 255f, 0f), Random.value));
        Random.state = state;
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        if (newRoom is null)
            return;
        if (newRoom.climbableVines is null)
        {
            newRoom.AddObject(newRoom.climbableVines = new());
            newRoom.climbableVines.vines.Add(this);
        }
        else if (!newRoom.climbableVines.vines.Contains(this))
            newRoom.climbableVines.vines.Add(this);
    }

    public override void SafariPickup()
    {
        if (inputWithDiagonals!.Value.pckp && grasps.Length > 0)
        {
            if (grasps[0]?.grabbed is null)
            {
                var physobs = room.physicalObjects;
                for (var j = 0; j < physobs.Length; j++)
                {
                    var objs = physobs[j];
                    for (var k = 0; k < objs.Count; k++)
                    {
                        if (objs[k] is PhysicalObject m && (m is Mushroom or LimeMushroom or SlimeMold) && m.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject) && Custom.DistLess(firstChunk.pos, m.firstChunk.pos, m.firstChunk.rad * 2f))
                            TryToGrabPrey(m);
                    }
                }
            }
            else if (lastInputWithDiagonals.HasValue && !lastInputWithDiagonals.Value.pckp && grasps[0]?.grabbed is PhysicalObject m)
            {
                if (m is Mushroom or LimeMushroom)
                {
                    room.PlaySound(SoundID.Slugcat_Eat_Slime_Mold, m.firstChunk, false, 1.25f, 1f);
                    grasps[0].Release();
                    AI.FoodTracker.ForgetItem(m.abstractPhysicalObject);
                    m.Destroy();
                }
                else if (m is SlimeMold s)
                {
                    --s.bites;
                    room.PlaySound(s.bites == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, s.firstChunk, false, 1.25f, 1f);
                    if (s.bites < 1)
                    {
                        grasps[0].Release();
                        AI.FoodTracker.ForgetItem(s.abstractPhysicalObject);
                        s.Destroy();
                    }
                }
            }
        }
    }

    public override void CarryObject()
    {
        if (grasps[0].grabbed is not PhysicalObject m || m is not SlimeMold and not Mushroom and not LimeMushroom)
        {
            ReleaseGrasp(0);
            return;
        }
        var fch = firstChunk;
        var mfch = m.firstChunk;
        var dst = Vector2.Distance(fch.pos, mfch.pos);
        if (dst > 50f)
        {
            ReleaseGrasp(0);
            return;
        }
        var adjPos = Custom.DirVec(fch.pos, mfch.pos) * (dst - (fch.rad + mfch.rad));
        var relativeMass = mfch.mass / (fch.mass + mfch.mass) * (m is SlimeMold ? .2f : .8f) * (1f - AI?.stuckTracker.Utility() ?? 0f);
        fch.pos += adjPos * relativeMass;
        fch.vel += adjPos * relativeMass;
        mfch.pos -= adjPos * (1f - relativeMass);
        mfch.vel -= adjPos * (1f - relativeMass);
        mfch.MoveFromOutsideMyUpdate(evenUpdate, DangerPos);
        PushOutOf(fch.pos, fch.rad, 0);
        if (HoverflyData.TryGetValue(abstractCreature, out var data) && data.BiteWait == 0)
        {
            if (m is Mushroom or LimeMushroom)
            {
                room.PlaySound(SoundID.Slugcat_Eat_Slime_Mold, m.firstChunk, false, 1.25f, 1f);
                grasps[0].Release();
                AI?.FoodTracker.ForgetItem(m.abstractPhysicalObject);
                m.Destroy();
                --data.Hunger;
            }
            else if (m is SlimeMold s)
            {
                --s.bites;
                room.PlaySound(s.bites == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, s.firstChunk, false, 1.25f, 1f);
                if (s.bites < 1)
                {
                    grasps[0].Release();
                    AI?.FoodTracker.ForgetItem(s.abstractPhysicalObject);
                    s.Destroy();
                    --data.Hunger;
                }
            }
            data.BiteWait = 1000;
        }
    }

    public virtual int TotalPositions() => TailLength;

    public virtual Vector2 Pos(int index)
    {
        if (graphicsModule is M4RTailFlyGraphics grs)
            return grs.Tail.segments[index].pos;
        return firstChunk.pos;
    }

    public virtual float Rad(int index)
    {
        if (graphicsModule is M4RTailFlyGraphics grs)
            return grs.Tail.segments[index].rad;
        return 2f;
    }

    public virtual float Mass(int index)
    {
        if (graphicsModule is M4RTailFlyGraphics grs)
            return grs.Tail.segments[index].rad * .1f;
        return .2f;
    }

    public virtual void Push(int index, Vector2 movement)
    {
        if (graphicsModule is M4RTailFlyGraphics grs)
            grs.Tail.segments[index].vel += movement * 1.3f;
    }

    public virtual void BeingClimbedOn(Creature crit)
    {
        if (crit is null || !Consious)
            return;
        var fc = firstChunk;
        var contact = false;
        var bcs = crit.bodyChunks;
        for (var i = 0; i < bcs.Length; i++)
        {
            if (bcs[i].ContactPoint.y < 0)
            {
                contact = true;
                break;
            }
        }
        if (!contact)
            fc.vel.y -= crit.TotalMass;
        if (crit is Player p)
        {
            ref readonly var input = ref p.input[0];
            Vector2.ClampMagnitude(fc.vel, 2f);
            if (input.x != 0)
                fc.vel.x += .25f * input.x;
            if (input.y != 0)
                fc.vel.y += .25f * input.y;
            if (p.room is Room rm && !rm.IsPositionInsideBoundries(Room.StaticGetTilePosition(fc.pos)))
                p.Stun(30);
            if (AI is M4RTailFlyAI ai)
            {
                ai.SwooshToPos = null;
                if (ai.Behavior != HoverflyAI.FlyBehavior.EscapeRain)
                    ai.Behavior = HoverflyAI.FlyBehavior.Idle;
                ai.FocusItem = null;
            }
        }
    }

    public virtual bool CurrentlyClimbable() => graphicsModule is not null && Submersion <= 0f && !inShortcut;
}