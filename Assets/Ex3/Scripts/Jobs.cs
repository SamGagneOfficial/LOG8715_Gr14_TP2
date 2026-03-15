using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float dt;

    void Execute(ref Position pos, in Velocity speed)
    {
        pos.Value.x += speed.Value.x * dt;
        pos.Value.y += speed.Value.y * dt;
    }
}

[BurstCompile]
public partial struct TimerDeathJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;

    void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref Timer timer)
    {
        if (timer.Value <= 0f)
        {
            ecb.DestroyEntity(chunkIndex, entity);
        }
    }
}