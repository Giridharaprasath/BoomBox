using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoomBox
{
    public enum CubeColor
    {
        Yellow,
        Red,
        Green,
        Pink,
    }

    public class BoomBoxGameManager : MonoBehaviour
    {
        [Header("Level Objects")]
        public GameObject CubePlaceSpotParentObject;
        public GameObject CubeButtonParentObject;
        public List<GameObject> CubeButtonsParentObject;

        public GameObject CubePlaceSpotPrefab;
        public GameObject CubeButtonRowPrefab;
        public GameObject CubeButtonColumnPrefab;
        public CubeButton CubeButtonPrefab;

        [Header("Level Objects Created")]
        public List<GameObject> CubePlaceSpots;
        public List<CubeButton> CubeButtons;

        public List<BoomBoxLevels> AllBoomBoxLevels;
        public BoomBoxLevels CurrentBoomBoxLevel { get; set; }
        public int CurrentLevelNumber;

        public List<bool> IsCubePlaceSpotOccupied { get; set; }

        public void SetCurrentLevel(string changeValue)
        {
            if (changeValue.Equals("+"))
            {
                CurrentLevelNumber++;
            }
            else
            {
                CurrentLevelNumber--;
            }

            if (CurrentLevelNumber < 1) CurrentLevelNumber = 1;
            if (CurrentLevelNumber > AllBoomBoxLevels.Count) CurrentLevelNumber = AllBoomBoxLevels.Count;

            PlayerPrefs.SetInt("CurrentLevelNumber", CurrentLevelNumber);

            LoadScene();
        }

        public void LoadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Start()
        {
            CurrentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber", 1);

            CurrentBoomBoxLevel = AllBoomBoxLevels[CurrentLevelNumber - 1];

            CreateCubePlaceSpots();
            CreateCubeButtons();

            IsCubePlaceSpotOccupied = new List<bool>(new bool[CubePlaceSpots.Count]);

            foreach (var item in CubeButtons)
            {
                item.OnClicked += AddCubeButtonToCubePlaceSpot;
            }
        }

        private void CreateCubePlaceSpots()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubePlaceSlotCount; i++)
            {
                GameObject cubePlaceSpot = Instantiate(CubePlaceSpotPrefab, CubePlaceSpotParentObject.transform);
                cubePlaceSpot.name = $"Cube PS{i}";

                CubePlaceSpots.Add(cubePlaceSpot);
            }
        }

        private void CreateCubeButtons()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonRowCount; i++)
            {
                GameObject cubeButtonRowObject = Instantiate(CubeButtonRowPrefab, CubeButtonParentObject.transform);
                cubeButtonRowObject.name = $"Cube BR{i}";

                for (int j = 0; j < CurrentBoomBoxLevel.CubeButtonColumnCount; j++)
                {
                    GameObject cubeButtonColumnObject = Instantiate(CubeButtonColumnPrefab, cubeButtonRowObject.transform);
                    cubeButtonColumnObject.name = $"Cube BC{j}";

                    CubeButtonsParentObject.Add(cubeButtonColumnObject);
                }
            }

            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonCount; i++)
            {
                CubeButton cubeButton = Instantiate(CubeButtonPrefab, CubeButtonsParentObject[i].transform);
                cubeButton.name = $"Cube B{i}";

                cubeButton.CubeButtonInfo = new CubeButtonInfo(CurrentBoomBoxLevel.CubeButtonInfos[i])
                {
                    ColumnIndex = i % CurrentBoomBoxLevel.CubeButtonColumnCount,
                    StartRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                    CurrentRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                };

                CubeButtons.Add(cubeButton);
            }
        }

        public void AddCubeButtonToCubePlaceSpot(CubeButton cubeButton)
        {
            if (!cubeButton.CubeButtonInfo.CanSelect)
            {
                Handheld.Vibrate();
                return;
            }

            int freeSpotIndex = GetFirstFreeCubePlaceSpot();
            if (freeSpotIndex == -1)
            {
                Handheld.Vibrate();
                return;
            }

            IsCubePlaceSpotOccupied[freeSpotIndex] = true;

            cubeButton.SetSpotIndex(freeSpotIndex);
            cubeButton.transform.DOMove(CubePlaceSpots[freeSpotIndex].transform.position, 0.5f).OnComplete(() =>
            {
                cubeButton.transform.SetParent(CubePlaceSpots[freeSpotIndex].transform);
                cubeButton.transform.localPosition = Vector3.zero;
            });

            MoveCubeColumnUp(cubeButton.CubeButtonInfo.ColumnIndex);
        }

        private void MoveCubeColumnUp(int ColumnIndex)
        {
            for (int i = 0; i < CubeButtons.Count; i++)
            {
                if (CubeButtons[i].CubeButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (CubeButtons[i].CubeButtonInfo.CurrentRowIndex == -1) continue;

                int index = i;
                CubeButton currentButton = CubeButtons[index];
                currentButton.SetRowIndex(currentButton.CubeButtonInfo.CurrentRowIndex - 1);
                int nextPos = index - (CurrentBoomBoxLevel.CubeButtonColumnCount * (currentButton.CubeButtonInfo.StartRowIndex - currentButton.CubeButtonInfo.CurrentRowIndex));
                print(nextPos);
                currentButton.transform.DOMove(CubeButtonsParentObject[nextPos].transform.position, 0.5f).OnComplete(() =>
                {
                    currentButton.transform.SetParent(CubeButtonsParentObject[nextPos].transform);
                    currentButton.transform.localPosition = Vector3.zero;
                });
            }
        }

        private int GetFirstFreeCubePlaceSpot()
        {
            for (int i = 0; i < IsCubePlaceSpotOccupied.Count; i++)
            {
                if (!IsCubePlaceSpotOccupied[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
