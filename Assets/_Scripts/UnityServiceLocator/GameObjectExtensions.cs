using UnityEngine;

namespace _Scripts.UnityServiceLocator
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// オブジェクトが存在するかを判定する
        /// </summary>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
    }
}