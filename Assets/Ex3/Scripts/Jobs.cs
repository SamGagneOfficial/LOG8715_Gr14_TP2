using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct RespawnJob : IJobEntity
{
    public float minLife;
    public float maxLife;

    public float halfWidth;
    public float halfHeight;

    public void Execute(ref Position pos, ref Timer timer, ref Flags flags)
    {
        if (timer.Value <= 0f && (flags & (1 << 7)))
        {
            pos.Value = new float2(
                Random.Range(-halfWidth, halfWidth),
                Random.Range(-halfHeight, halfHeight)
            );

            timer.Value = Random.Range(minLife, maxLife);
            flags ^= (1 << 7);
        }
        else if (timer.Value <= 0f && !(flags & (1 << 7)))
        {
            flags = (byte)-1;
        }
    }
}

[BurstCompile]
public partial struct TimerDeathJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref Timer timer, in Flags flags)
    {
        if (timer.Value <= 0f && (flags & (1 << 6)))
        {
            ecb.DestroyEntity(chunkIndex, entity);
        }
    }
}

[BurstCompile]
public partial struct CollisionJob : IJob
{
    [ReadOnly] public NativeArray<Position> positions;

    public NativeArray<Timer> timers;
    public NativeArray<Flags> flags;
    public float collisionRad;

    public void Execute()
    {
        int count = positions.Length;
        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                float2 delta = positions[i].Value - positions[j].Value;

                float distSq = math.lengthsq(delta);

                if (distSq < collisionRad * collisionRad)
                {
                    if (flags[i].Value == flags[j].Value) //Sets reproduction flags
                    {
                        flags[i].Value |= (1 << 7);
                        flags[j].Value |= (1 << 7);
                    }
                    else
                    {
                        if(!(flags[i].Value & 1) && (flags[j].Value & 1) || (flags[i].Value & 1) && (flags[j].Value & 2)) //plant and prey OR prey and pred
                        {
                            timers[i].Exponent -= 1;
                            timers[j].Exponent += 1;
                        }
                        else if (!(flags[j].Value & 1) && (flags[i].Value & 1) || (flags[i].Value & 2) && (flags[j].Value & 1)) //prey and plant OR pred and prey
                        {
                            timers[i].Exponent += 1;
                            timers[j].Exponent -= 1;
                        }
                    }
                }
            }
        }
    }
}

[BurstCompile]
public partial struct PlantSizeJob : IJobEntity
{
    public float dt;

    public void Execute(ref Size size, in Timer timer)
    {
        float delta = timer.DecaySpeed * dt * Math.Pow(2, timer.Exponent);
        size.Value *= timer.Value / (timer.Value + delta);
    }
}

[BurstCompile]
public partial struct DecayJob : IJobEntity
{
    public float dt;

    public void Execute(ref Timer timer)
    {
        timer.Value -= timer.DecaySpeed * dt * math.exp2(timer.Exponent);
        timer.Exponent = 0; //reset
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float dt;

    public void Execute(ref Position pos, in Velocity speed)
    {
        pos.Value.x += speed.Value.x * dt;
        pos.Value.y += speed.Value.y * dt;
    }
}

[BurstCompile]
public partial struct CloseNeighborJob : IJobEntity
{


    public void Execute(ref Position pos, in Velocity speed)
    {
        pos.Value.x += speed.Value.x * dt;
        pos.Value.y += speed.Value.y * dt;
    }
}