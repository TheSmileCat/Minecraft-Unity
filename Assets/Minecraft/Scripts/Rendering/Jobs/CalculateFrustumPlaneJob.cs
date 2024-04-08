#region

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

#endregion

namespace Minecraft.Scripts.Rendering.Jobs
{
  [BurstCompile]
  public struct CalculateFrustumPlaneJob : IJob
  {
    public float Near;
    public float Far;
    public float FOV;
    public float Aspect;
    public float3x4 Camera;
    [WriteOnly] public NativeArray<float4> Planes;

    public void Execute()
    {
      var halfHeight = Far * tan(radians(FOV * 0.5f));
      var up = Camera.c1 * halfHeight;
      var right = Camera.c0 * halfHeight * Aspect;
      var nearCenter = Camera.c3 + Near * Camera.c2;
      var farCenter = Camera.c3 + Far * Camera.c2;
      var corners = float3x4(
        farCenter - up - right,
        farCenter - up + right,
        farCenter + up - right,
        farCenter + up + right
      );

      Planes[0] = CalculatePlane(Camera.c3, corners.c2, corners.c0); // left
      Planes[1] = CalculatePlane(Camera.c3, corners.c1, corners.c3); // right
      Planes[2] = CalculatePlane(Camera.c3, corners.c3, corners.c2); // top
      Planes[3] = CalculatePlane(Camera.c3, corners.c0, corners.c1); // down
      Planes[4] = CalculatePlane(Camera.c2, nearCenter); // near
      Planes[5] = CalculatePlane(-Camera.c2, farCenter); // far
    }

    private float4 CalculatePlane(in float3 a, in float3 b, in float3 c)
    {
      var normal = normalize(cross(b - a, c - a));
      return CalculatePlane(normal, a);
    }

    private float4 CalculatePlane(in float3 normal, in float3 a)
    {
      return float4(normal, -dot(normal, a));
    }
  }
}