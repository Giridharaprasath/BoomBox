using System.Collections.Generic;
using UnityEngine;

namespace BoomBox
{
    [CreateAssetMenu(fileName = "BoomBoxLevel_", menuName = "Boom Box Levels")]
    public class BoomBoxLevels : ScriptableObject
    {
        public int LevelNumber;
        public int CubePlaceSlotCount;
        public int CubeButtonCount;
        public int CubeButtonColumnCount;
        public int CubeButtonRowCount;

        public List<CubeButtonInfo> CubeButtonInfos;
    }
}
