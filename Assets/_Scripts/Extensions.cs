using System.Runtime.CompilerServices;
using UnityEngine;

namespace _Scripts
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 As2(this Vector3 vector3)
            => new(vector3.x, vector3.y);
    }
}