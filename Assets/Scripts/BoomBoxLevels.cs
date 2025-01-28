using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomBox
{
    [CreateAssetMenu(fileName = "BoomBoxLevel_", menuName = "Boom Box Levels")]
    public class BoomBoxLevels : ScriptableObject
    {
        [Serializable]
        public class BoxButtonSetRow
        {
            public CubeColor BoxColor;
            public int StartColumnIndex, EndColumnIndex;
            public int Height = 1;
        }
        [Serializable]
        public class BoxButtonRowInfo
        {
            public List<BoxButtonSetRow> BoxButtonSetRows;
        }

        public int LevelNumber;
        public int CubePlaceSlotCount;
        public int CubeButtonCount;
        public int CubeButtonColumnCount;
        public int CubeButtonRowCount;

        public int BoxButtonCount;
        public int BoxButtonColumnCount;
        public int BoxButtonRowCount;

        public List<CubeButtonInfo> CubeButtonInfos;

        public List<BoxButtonRowInfo> BoxButtonInfos;
    }
}
