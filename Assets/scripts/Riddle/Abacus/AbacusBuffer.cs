using UnityEngine;

namespace Riddle.Abacus
{
    public static class AbacusBuffer
    {
        public static string currentId = "1";    // 当前算盘编号
        public static Vector3 returnPosition = Vector3.zero;

        public static void Set(string id, Vector3 pos)
        {
            currentId = id;
            returnPosition = pos;
        }
    }
}