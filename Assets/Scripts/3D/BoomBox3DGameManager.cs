using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace BoomBox
{
    public class BoomBox3DGameManager : MonoBehaviour
    {
        [Header("Level Objects 3D")]

        [Header("Cube Place Spots 3D")]
        public GameObject CubePlaceSpot3DParentObject;
        public GameObject CubePlaceSpot3DPrefab;

        [Header("Cube Button 3D")]
        public GameObject CubeButton3DParentObject;
        public GameObject CubeButtonRow3DPrefab;
        public GameObject CubeButtonColumn3DPrefab;
        public CubeButton3D CubeButton3DPrefab;

        [Header("Box Button 3D")]
        public GameObject BoxButton3DParentObject;
        public GameObject BoxButtonRow3DPrefab;
        public GameObject BoxButtonColumn3DPrefab;
        public BoxButton3D BoxButton3DPrefab;

        [Header("Level Objects 3D Created")]
        public List<GameObject> CubePlaceSpots3D;
        public List<GameObject> CubeButtons3DParentObject;
        public List<CubeButton3D> CubeButtons3D;
        public List<GameObject> BoxButtons3DParentObject;
        public List<BoxButton3D> BoxButtons3D;

        [Header("Level Progress Status")]
        private int BoxButton3DShootCount = 0;
        private bool CheckShoot = false;

        public List<bool> IsCubePlaceSpotOccupied { get; set; }
        public List<CubeButton3D> CubeButtons3DInPlaceSpots;
        public List<BoxButton3D> BoxButtons3DToShoot;
        public Transform CubeLeftSpot, CubeRightSpot;

        [Header("Level Info")]
        public List<BoomBoxLevels> AllBoomBoxLevels;
        public BoomBoxLevels CurrentBoomBoxLevel { get; set; }
        public int CurrentLevelNumber;

        [Header("Add Extra Place Slot")]
        private const int maxSlots = 7;
        public int CurrentSlotCount;
        public GameObject GetExtraSlot;
        public TMP_Text NextSlotRateText;

        [Header("Coin Manager")]
        public BoomBox3DCoinManager CoinManager;

        [Header("Reload")]
        public Button ReloadButton;
        public Transform ReloadButtonStartPoint, ReloadButtonEndPoint;

        [Header("Progress Bar")]
        public GameObject ProgressPanel;
        public Image ProgressBar;
        public TMP_Text[] LevelText;
        public Transform ProgressPanelStartPoint, ProgressPanelEndPoint;

        private void Start()
        {
            VibrationManager.Init();
            DOTween.SetTweensCapacity(2000, 100);

            CurrentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
            CurrentBoomBoxLevel = AllBoomBoxLevels[CurrentLevelNumber - 1];

            CreateCubePlaceSpots3D();
            CreateBoxButtons3D();
            CreateCubeButtons3D();

            IsCubePlaceSpotOccupied = new List<bool>(new bool[CubePlaceSpots3D.Count]);
            CurrentSlotCount = CubePlaceSpots3D.Count;

            NextSlotRateText.text = CurrentSlotCount.ToString();

            if (CurrentLevelNumber == 1)
                GetExtraSlot.SetActive(false);


            ReloadButton.onClick.AddListener(LoadScene);
            ShowReloadButton();

            foreach (var item in LevelText)
            {
                item.text = "Level " + CurrentLevelNumber;
            }
            ShowProgressBar();
        }
        private void OnDestroy()
        {
            DOTween.KillAll();
        }

        #region Level Management
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

            if (CurrentLevelNumber < 1) CurrentLevelNumber = AllBoomBoxLevels.Count;
            if (CurrentLevelNumber > AllBoomBoxLevels.Count) CurrentLevelNumber = 1;

            PlayerPrefs.SetInt("CurrentLevelNumber", CurrentLevelNumber);

            LoadScene();
        }
        public void LoadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        #endregion

        #region Level Objects Creation
        private void CreateCubePlaceSpots3D()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubePlaceSlotCount; i++)
            {
                GameObject cubePlaceSpot = Instantiate(CubePlaceSpot3DPrefab, CubePlaceSpot3DParentObject.transform);
                cubePlaceSpot.name = $"Cube PS{i} 3D";

                CubePlaceSpots3D.Add(cubePlaceSpot);
            }
        }
        private void CreateBoxButtons3D()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonRowCount; i++)
            {
                GameObject boxButtonRowObject = Instantiate(BoxButtonRow3DPrefab, BoxButton3DParentObject.transform);
                boxButtonRowObject.name = $"Box BR{i} 3D";

                for (int j = 0; j < CurrentBoomBoxLevel.BoxButtonColumnCount; j++)
                {
                    GameObject boxButtonColumnObject = Instantiate(BoxButtonColumn3DPrefab, boxButtonRowObject.transform);
                    boxButtonColumnObject.name = $"Box BC{j} 3D";

                    BoxButtons3DParentObject.Add(boxButtonColumnObject);
                }

                boxButtonRowObject.transform.SetAsFirstSibling();
            }

            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonInfos.Count; i++)
            {
                BoomBoxLevels.BoxButtonRowInfo item = CurrentBoomBoxLevel.BoxButtonInfos[i];

                for (int j = 0; j < item.BoxButtonSetRows.Count; j++)
                {
                    BoomBoxLevels.BoxButtonSetRow boxButtonSetRow = item.BoxButtonSetRows[j];

                    for (int k = boxButtonSetRow.StartColumnIndex; k <= boxButtonSetRow.EndColumnIndex; k++)
                    {
                        BoxButton3D boxButton = Instantiate(BoxButton3DPrefab, BoxButtons3DParentObject[i * CurrentBoomBoxLevel.BoxButtonColumnCount + k].transform);
                        boxButton.name = $"Box B{i * CurrentBoomBoxLevel.BoxButtonColumnCount + k}";

                        boxButton.BoxButtonInfo = new()
                        {
                            CubeColor = boxButtonSetRow.BoxColor,
                            ColumnIndex = k,
                            StartRowIndex = i,
                            CurrentRowIndex = i,
                        };

                        boxButton.TotalCount = boxButtonSetRow.Height;
                        boxButton.CurrentCount = boxButtonSetRow.Height;

                        if (boxButton.BoxButtonInfo.CurrentRowIndex == 0)
                        {
                            boxButton.BoxButtonInfo.CanShoot = true;
                            AddBoxButtonToBeShot(boxButton);
                        }

                        BoxButtons3D.Add(boxButton);
                        boxButton.OnCanShoot += AddBoxButtonToBeShot;
                        boxButton.OnAllHit += MoveBoxColumnDown;
                    }
                }
            }
        }
        private void CreateCubeButtons3D()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonRowCount; i++)
            {
                GameObject cubeButtonRowObject = Instantiate(CubeButtonRow3DPrefab, CubeButton3DParentObject.transform);
                cubeButtonRowObject.name = $"Cube BR{i} 3D";

                for (int j = 0; j < CurrentBoomBoxLevel.CubeButtonColumnCount; j++)
                {
                    GameObject cubeButtonColumnObject = Instantiate(CubeButtonColumn3DPrefab, cubeButtonRowObject.transform);
                    cubeButtonColumnObject.name = $"Cube BC{j} 3D";

                    CubeButtons3DParentObject.Add(cubeButtonColumnObject);
                }
            }

            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonCount; i++)
            {
                CubeButton3D cubeButton = Instantiate(CubeButton3DPrefab, CubeButtons3DParentObject[i].transform);
                cubeButton.name = $"Cube B{i}";

                cubeButton.CubeButtonInfo = new CubeButtonInfo(CurrentBoomBoxLevel.CubeButtonInfos[i])
                {
                    ColumnIndex = i % CurrentBoomBoxLevel.CubeButtonColumnCount,
                    StartRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                    CurrentRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                };

                CubeButtons3D.Add(cubeButton);
                cubeButton.OnClicked += AddCubeButtonToCubePlaceSpot;
                cubeButton.OnHitSuccess += OnBoxHitByCube;
            }
        }
        #endregion

        #region Cube Button Movement
        public void AddCubeButtonToCubePlaceSpot(CubeButton3D cubeButton)
        {
            if (!cubeButton.CubeButtonInfo.CanSelect)
            {
                cubeButton.OnClickShake();
                VibrationManager.VibrateNope();
                return;
            }

            int freeSpotIndex = GetFirstFreeCubePlaceSpot();
            if (freeSpotIndex == -1)
            {
                cubeButton.OnClickShake();
                VibrationManager.VibrateNope();
                return;
            }

            VibrationManager.VibratePeek();

            IsCubePlaceSpotOccupied[freeSpotIndex] = true;
            cubeButton.SetSpotIndex(freeSpotIndex);

            cubeButton.StartMoving();
            cubeButton.transform.LookAt(CubePlaceSpots3D[freeSpotIndex].transform.position);
            cubeButton.transform.DOMove(CubePlaceSpots3D[freeSpotIndex].transform.position, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cubeButton.transform.SetParent(CubePlaceSpots3D[freeSpotIndex].transform);
                cubeButton.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

                AddCubeButton3DToSpot(cubeButton);
                cubeButton.StopMoving();
                cubeButton.DisableCollider();
            });

            MoveCubeColumnUp(cubeButton.CubeButtonInfo.ColumnIndex);
        }

        private void MoveCubeColumnUp(int ColumnIndex)
        {
            for (int i = 0; i < CubeButtons3D.Count; i++)
            {
                if (CubeButtons3D[i].CubeButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (CubeButtons3D[i].CubeButtonInfo.CurrentRowIndex == -1) continue;

                int index = i;
                CubeButton3D currentButton = CubeButtons3D[index];
                currentButton.StartMoving();
                currentButton.SetRowIndex(currentButton.CubeButtonInfo.CurrentRowIndex - 1);
                int nextPos = index - (CurrentBoomBoxLevel.CubeButtonColumnCount * (currentButton.CubeButtonInfo.StartRowIndex - currentButton.CubeButtonInfo.CurrentRowIndex));
                currentButton.transform.DOMove(CubeButtons3DParentObject[nextPos].transform.position, 1f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    currentButton.StopMoving();
                    currentButton.CheckRowIndex();
                    currentButton.transform.SetParent(CubeButtons3DParentObject[nextPos].transform);
                    // currentButton.transform.localPosition = Vector3.zero;
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

        private void AnimateCubeButtonOutOfSpot(CubeButton3D cubeButton)
        {
            Transform endTransform = cubeButton.CubeButtonInfo.CurrentSpotIndex < IsCubePlaceSpotOccupied.Count / 2 ? CubeLeftSpot.transform : CubeRightSpot.transform;

            cubeButton.transform.SetParent(endTransform, true);
            cubeButton.StartMoving();

            Sequence sequence = DOTween.Sequence();

            cubeButton.AnimateColor();
            sequence.Append(cubeButton.CharacterHeadObject.transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.15f).OnComplete(() =>
            {
                RemoveCubeButton3DFromSpot(cubeButton);
            }));

            if (cubeButton.CubeButtonInfo.CurrentSpotIndex != 0 && cubeButton.CubeButtonInfo.CurrentSpotIndex != IsCubePlaceSpotOccupied.Count - 1)
            {
                sequence.Append(cubeButton.transform.DOMove(cubeButton.transform.position + new Vector3(0f, 0f, 1.5f), 0.15f).SetEase(Ease.OutQuad));
            }

            sequence.Append(cubeButton.transform.DOMove(endTransform.position, 2f).SetEase(Ease.OutQuad).SetDelay(0.1f).OnStart(() =>
            {
                cubeButton.transform.LookAt(endTransform.position);
            }));
            sequence.Play().OnComplete(() =>
            {
                cubeButton.gameObject.SetActive(false);
            });
        }
        #endregion

        #region Box Button Movement
        public void MoveBoxColumnDown(int ColumnIndex)
        {
            for (int i = 0; i < BoxButtons3D.Count; i++)
            {
                if (BoxButtons3D[i].BoxButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (BoxButtons3D[i].BoxButtonInfo.CurrentRowIndex == 0) continue;

                int index = i;
                BoxButton3D currentButton = BoxButtons3D[index];
                currentButton.SetRowIndex(currentButton.BoxButtonInfo.CurrentRowIndex - 1);
                int nextPos = index + (CurrentBoomBoxLevel.BoxButtonColumnCount * (currentButton.BoxButtonInfo.CurrentRowIndex - currentButton.BoxButtonInfo.StartRowIndex));
                currentButton.transform.DOMove(BoxButtons3DParentObject[nextPos].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    currentButton.transform.SetParent(BoxButtons3DParentObject[nextPos].transform);
                    currentButton.transform.localPosition = Vector3.zero;
                });
            }
        }
        #endregion

        #region Cube Place Spots Shotting
        public void AddCubeButton3DToSpot(CubeButton3D cubeButton)
        {
            if (CubeButtons3DInPlaceSpots.Contains(cubeButton)) return;

            CubeButtons3DInPlaceSpots.Add(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = true;
            OnCubePlaceSpotUpdated();
        }
        public void RemoveCubeButton3DFromSpot(CubeButton3D cubeButton)
        {
            if (!CubeButtons3DInPlaceSpots.Contains(cubeButton)) return;

            CubeButtons3DInPlaceSpots.Remove(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = false;
            OnCubePlaceSpotUpdated();
        }
        public void OnCubePlaceSpotUpdated()
        {
            // Debug.Log($"GG : Cube Place Spot Updated");
            CheckShoot = CubeButtons3DInPlaceSpots.Count != 0;
        }
        public void AddBoxButtonToBeShot(BoxButton3D boxButton)
        {
            if (BoxButtons3DToShoot.Contains(boxButton)) return;

            BoxButtons3DToShoot.Add(boxButton);
        }
        private void FixedUpdate()
        {
            if (!CheckShoot) return;

            if (CubeButtons3DInPlaceSpots.Count == 0)
            {
                CheckShoot = false;
                return;
            }

            for (int i = 0; i < CubeButtons3DInPlaceSpots.Count; i++)
            {
                CubeButton3D item = CubeButtons3DInPlaceSpots[i];
                if (item == null) continue;

                if (item.CubeButtonInfo.Count == 0) continue;

                CheckShootForCube(item);
            }
        }
        private void CheckShootForCube(CubeButton3D cubeButton)
        {
            foreach (BoxButton3D box in BoxButtons3DToShoot)
            {
                if (!box.BoxButtonInfo.CanShoot) continue;

                if (box.BoxButtonInfo.CubeColor != cubeButton.CubeButtonInfo.CubeColor) continue;

                BoxButtons3DToShoot.Remove(box);
                cubeButton.StartShootingAt(box);

                VibrationManager.VibratePop();
                return;
            }
        }
        private void OnBoxHitByCube(CubeButton3D cubeButton, BoxButton3D boxButton)
        {
            BoxButton3DShootCount++;
            ProgressBar.fillAmount = (float)BoxButton3DShootCount / CurrentBoomBoxLevel.BoxButtonCount;
            
            if (cubeButton.CubeButtonInfo.Count == 0)
            {
                AnimateCubeButtonOutOfSpot(cubeButton);
            }

            boxButton.StartShrinking();
            boxButton.OnHit();

            if (BoxButton3DShootCount == CurrentBoomBoxLevel.BoxButtonCount)
            {
                OnLevelCompleted();
                CheckShoot = false;
            }
        }
        #endregion

        #region Extra Slot Process
        public void BuyExtraSlotProcess()
        {
            if (CurrentSlotCount == maxSlots) return;

            int nextRate = CurrentSlotCount;
            int currentCoinCount = CoinManager.GetCoinCount();

            if (currentCoinCount < nextRate)
            {
                VibrationManager.VibrateNope();
                return;
            }

            CurrentSlotCount++;
            IsCubePlaceSpotOccupied.Add(false);
            GameObject cubePlaceSpot = Instantiate(CubePlaceSpot3DPrefab, CubePlaceSpot3DParentObject.transform);
            cubePlaceSpot.name = $"Cube PS{CurrentSlotCount} 3D";

            CubePlaceSpots3D.Add(cubePlaceSpot);

            CoinManager.SetCoinCount(currentCoinCount - nextRate);
            CoinManager.SetCoinCountText();

            NextSlotRateText.text = CurrentSlotCount.ToString();

            if (CurrentSlotCount == maxSlots)
            {
                GetExtraSlot.SetActive(false);
            }
        }
        #endregion

        #region Reload Button
        private void ShowReloadButton()
        {
            ReloadButton.transform.DOMove(ReloadButtonEndPoint.position, 0.5f).SetEase(Ease.InSine).OnComplete(() =>
            {
                ReloadButton.enabled = true;
            });
        }
        private void HideReloadButton()
        {
            ReloadButton.enabled = false;
            ReloadButton.transform.DOMove(ReloadButtonStartPoint.position, 0.5f).SetEase(Ease.InSine);
        }
        #endregion

        #region Show Progress Bar
        public void ShowProgressBar()
        {
            ProgressPanel.transform.DOMove(ProgressPanelEndPoint.position, 0.5f).SetEase(Ease.InOutSine);
        }
        public void HideProgressBar()
        {
            ProgressPanel.transform.DOMove(ProgressPanelStartPoint.position, 0.5f).SetEase(Ease.OutBack);
        }
        #endregion

        private void OnLevelCompleted()
        {
            Debug.Log($"GG : Level Completed");
            CoinManager.OnLevelCompleted();
            HideReloadButton();
            HideProgressBar();

            SaveLevelProgress();
        }

        private void SaveLevelProgress()
        {
            CurrentLevelNumber++;

            if (CurrentLevelNumber > AllBoomBoxLevels.Count) CurrentLevelNumber = 1;

            PlayerPrefs.SetInt("CurrentLevelNumber", CurrentLevelNumber);
        }

        #region Editor Functions
        [ContextMenu("Next Level")]
        public void Editor_OpenNextLevel()
        {
            SetCurrentLevel("+");
        }
        [ContextMenu("Previous Level")]
        public void Editor_OpenPreviousLevel()
        {
            SetCurrentLevel("-");
        }
        [ContextMenu("Restart Level")]
        public void Editor_RestartLevel()
        {
            LoadScene();
        }
        #endregion
    }
}
