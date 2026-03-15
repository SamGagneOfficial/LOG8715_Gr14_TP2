using Unity.Entities;
using Unity.Burst;

public partial struct MovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var job = new MovementJob
        {
            dt = SystemAPI.Time.DeltaTime
        };

        job.Schedule();
    }
}


