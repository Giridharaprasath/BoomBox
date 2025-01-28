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
        Blue,
        Pink,
        Orange,
    }

    public class BoomBoxGameManager : MonoBehaviour
    {
        [Header("Level Objects")]
        public GameObject CubePlaceSpotParentObject;
        public GameObject CubeButtonParentObject;
        public List<GameObject> CubeButtonsParentObject;
        public GameObject BoxButtonParentObject;
        public List<GameObject> BoxButtonsParentObject;

        public GameObject CubePlaceSpotPrefab;
        public GameObject CubeButtonRowPrefab;
        public GameObject CubeButtonColumnPrefab;
        public CubeButton CubeButtonPrefab;

        public GameObject BoxButtonRowPrefab;
        public GameObject BoxButtonColumnPrefab;
        public BoxButton BoxButtonPrefab;

        public GameObject CubeLeftSpot, CubeRightSpot;

        [Header("Level Objects Created")]
        public List<GameObject> CubePlaceSpots;
        public List<CubeButton> CubeButtons;
        public List<BoxButton> BoxButtons;

        public List<BoomBoxLevels> AllBoomBoxLevels;
        public BoomBoxLevels CurrentBoomBoxLevel { get; set; }
        public int CurrentLevelNumber;

        public List<bool> IsCubePlaceSpotOccupied { get; set; }

        public List<CubeButton> CubeButtonsInPlaceSpots;

        public List<BoxButton> BoxButtonToShoot;

        [Header("Level Complete")]
        public GameObject LevelComplete;

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
            VibrationManager.Init();

            CurrentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber", 1);

            CurrentBoomBoxLevel = AllBoomBoxLevels[CurrentLevelNumber - 1];

            CreateCubePlaceSpots();
            CreateBoxButtons();
            CreateCubeButtons();

            IsCubePlaceSpotOccupied = new List<bool>(new bool[CubePlaceSpots.Count]);
        }

        #region Level Objects Creation
        private void CreateCubePlaceSpots()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubePlaceSlotCount; i++)
            {
                GameObject cubePlaceSpot = Instantiate(CubePlaceSpotPrefab, CubePlaceSpotParentObject.transform);
                cubePlaceSpot.name = $"Cube PS{i}";

                CubePlaceSpots.Add(cubePlaceSpot);
            }
        }

        private void CreateBoxButtons()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonRowCount; i++)
            {
                GameObject boxButtonRowObject = Instantiate(BoxButtonRowPrefab, BoxButtonParentObject.transform);
                boxButtonRowObject.name = $"Box BR{i}";

                for (int j = 0; j < CurrentBoomBoxLevel.BoxButtonColumnCount; j++)
                {
                    GameObject boxButtonColumnObject = Instantiate(BoxButtonColumnPrefab, boxButtonRowObject.transform);
                    boxButtonColumnObject.name = $"Box BC{j}";

                    BoxButtonsParentObject.Add(boxButtonColumnObject);
                }
            }

            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonInfos.Count; i++)
            {
                BoomBoxLevels.BoxButtonRowInfo item = CurrentBoomBoxLevel.BoxButtonInfos[i];
                // print(i);
                for (int j = 0; j < item.BoxButtonSetRows.Count; j++)
                {
                    BoomBoxLevels.BoxButtonSetRow boxButtonSetRow = item.BoxButtonSetRows[j];

                    for (int k = boxButtonSetRow.StartColumnIndex; k <= boxButtonSetRow.EndColumnIndex; k++)
                    {
                        BoxButton boxButton = Instantiate(BoxButtonPrefab, BoxButtonsParentObject[i * CurrentBoomBoxLevel.BoxButtonColumnCount + k].transform);
                        boxButton.name = $"Box B{i * CurrentBoomBoxLevel.BoxButtonColumnCount + k}";

                        boxButton.BoxButtonInfo = new()
                        {
                            CubeColor = boxButtonSetRow.BoxColor,
                            ColumnIndex = k,
                            StartRowIndex = i,
                            CurrentRowIndex = i,
                        };

                        if (boxButton.BoxButtonInfo.CurrentRowIndex == 0)
                        {
                            boxButton.BoxButtonInfo.CanShoot = true;
                            AddBoxButtonToBeShot(boxButton);
                        }

                        BoxButtons.Add(boxButton);
                        boxButton.OnCanShoot += AddBoxButtonToBeShot;
                    }
                }
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
                cubeButton.OnClicked += AddCubeButtonToCubePlaceSpot;
            }
        }
        #endregion

        #region Cube Button Movement
        public void AddCubeButtonToCubePlaceSpot(CubeButton cubeButton)
        {
            if (!cubeButton.CubeButtonInfo.CanSelect)
            {
                VibrationManager.VibrateNope();
                return;
            }

            int freeSpotIndex = GetFirstFreeCubePlaceSpot();
            if (freeSpotIndex == -1)
            {
                VibrationManager.VibrateNope();
                return;
            }

            VibrationManager.VibratePeek();

            IsCubePlaceSpotOccupied[freeSpotIndex] = true;

            cubeButton.SetSpotIndex(freeSpotIndex);
            cubeButton.transform.DOMove(CubePlaceSpots[freeSpotIndex].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                cubeButton.transform.SetParent(CubePlaceSpots[freeSpotIndex].transform);
                cubeButton.transform.localPosition = Vector3.zero;

                AddCubeButtonToSpot(cubeButton);
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
                currentButton.transform.DOMove(CubeButtonsParentObject[nextPos].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
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
        #endregion

        #region Cube Place Spots Shotting
        public void AddCubeButtonToSpot(CubeButton cubeButton)
        {
            if (CubeButtonsInPlaceSpots.Contains(cubeButton)) return;

            CubeButtonsInPlaceSpots.Add(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = true;
            OnCubePlaceSpotUpdated();
        }
        public void RemoveCubeButtonFromSpot(CubeButton cubeButton)
        {
            if (!CubeButtonsInPlaceSpots.Contains(cubeButton)) return;

            CubeButtonsInPlaceSpots.Remove(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = false;
            OnCubePlaceSpotUpdated();
        }
        public void OnCubePlaceSpotUpdated()
        {
            Debug.Log($"GG : Cube Place Spot Updated");
            CheckShoot = CubeButtonsInPlaceSpots.Count != 0;
        }
        public void AddBoxButtonToBeShot(BoxButton boxButton)
        {
            if (BoxButtonToShoot.Contains(boxButton)) return;

            BoxButtonToShoot.Add(boxButton);
        }

        private bool CheckShoot = false;
        public int BoxButtonShootCount = 0;

        private void FixedUpdate()
        {
            if (!CheckShoot) return;

            // HashSet<CubeColor> cubeColors = new();
            // HashSet<BoxButton> ShootBoxButton = new();

            // foreach (BoxButton item in BoxButtonToShoot)
            // {
            //     if (!item.BoxButtonInfo.CanShoot) continue;

            //     if (cubeColors.Contains(item.BoxButtonInfo.CubeColor)) continue;

            //     cubeColors.Add(item.BoxButtonInfo.CubeColor);
            // }

            // foreach (CubeButton item in CubeButtonsInPlaceSpots)
            // {
            //     if (!cubeColors.Contains(item.CubeButtonInfo.CubeColor)) continue;

            //     if (item.CubeButtonInfo.Count == 0)
            //     {
            //         continue;
            //     }
            // }

            if (CubeButtonsInPlaceSpots.Count == 0)
            {
                CheckShoot = false;
                return;
            }

            for (int i = 0; i < CubeButtonsInPlaceSpots.Count; i++)
            {
                CubeButton item = CubeButtonsInPlaceSpots[i];
                if (item == null) continue;

                if (item.CubeButtonInfo.Count == 0) continue;

                CheckShootForCube(item);
            }
        }

        private void CheckShootForCube(CubeButton cubeButton)
        {
            foreach (BoxButton item in BoxButtonToShoot)
            {
                if (!item.BoxButtonInfo.CanShoot) continue;

                if (item.BoxButtonInfo.CubeColor != cubeButton.CubeButtonInfo.CubeColor) continue;

                item.BoxButtonInfo.CanShoot = false;
                item.BoxButtonInfo.IsHit = true;

                BoxButtonShootCount++;
                cubeButton.DecreaseCount();

                if (cubeButton.CubeButtonInfo.Count == 0)
                {
                    RemoveCubeButtonFromSpot(cubeButton);
                    if (cubeButton.CubeButtonInfo.CurrentSpotIndex < IsCubePlaceSpotOccupied.Count / 2)
                    {
                        cubeButton.transform.DOMove(CubeLeftSpot.transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            cubeButton.transform.SetParent(CubeLeftSpot.transform);
                            cubeButton.transform.localPosition = Vector3.zero;
                        });
                    }
                    else
                    {
                        cubeButton.transform.DOMove(CubeRightSpot.transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            cubeButton.transform.SetParent(CubeRightSpot.transform);
                            cubeButton.transform.localPosition = Vector3.zero;
                        });
                    }
                }

                item.gameObject.SetActive(false);
                BoxButtonToShoot.Remove(item);

                VibrationManager.VibratePop();

                MoveBoxColumnDown(item.BoxButtonInfo.ColumnIndex);

                if (BoxButtonShootCount == BoxButtons.Count)
                {
                    OnLevelCompleted();
                    CheckShoot = false;
                }

                return;
            }
        }
        #endregion

        #region Box Button Movement
        public void MoveBoxColumnDown(int ColumnIndex)
        {
            for (int i = 0; i < BoxButtons.Count; i++)
            {
                if (BoxButtons[i].BoxButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (BoxButtons[i].BoxButtonInfo.CurrentRowIndex == 0) continue;

                int index = i;
                BoxButton currentButton = BoxButtons[index];
                currentButton.SetRowIndex(currentButton.BoxButtonInfo.CurrentRowIndex - 1);
                int nextPos = index + (CurrentBoomBoxLevel.BoxButtonColumnCount * (currentButton.BoxButtonInfo.CurrentRowIndex - currentButton.BoxButtonInfo.StartRowIndex));
                currentButton.transform.DOMove(BoxButtonsParentObject[nextPos].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    currentButton.transform.SetParent(BoxButtonsParentObject[nextPos].transform);
                    currentButton.transform.localPosition = Vector3.zero;
                });
            }
        }
        #endregion

        private void OnLevelCompleted()
        {
            Debug.Log($"GG : Level Completed");
            LevelComplete?.SetActive(true);
        }
    }
}
