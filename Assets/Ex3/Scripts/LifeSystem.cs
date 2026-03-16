using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = UnityEngine.Random;


//public struct LifeSystem : ISystem
//{
//    public Ex3Config config;

//    public void OnCreate(ref SystemState state)
//    {
//        config = SystemAPI.GetSingleton<Ex3Config>();
//    }

//    public void OnUpdate(ref SystemState state)
//    {
//        var positions = SystemAPI.QueryBuilder()
//                                 .WithAll<Position>()
//                                 .Build()
//                                 .ToComponentDataArray<Position>(Allocator.TempJob);

//        var timers = SystemAPI.QueryBuilder()
//                                .WithAll<Timer>()
//                                .Build()
//                                .ToComponentDataArray<Timer>(Allocator.TempJob);

//        var tags = SystemAPI.QueryBuilder()
//                                .WithAll<Flags>()
//                                .Build()
//                                .ToComponentDataArray<Flags>(Allocator.TempJob);

//        var collisions = new CollisionJob
//        {
//            positions = positions,
//            timers = timers,
//            tags = tags,
//            collisionRad = config.TouchingDistance
//        }.Schedule(state.Dependency);

//        positions.Dispose();
//        timers.Dispose();
//        tags.Dispose();
//    }
//}
