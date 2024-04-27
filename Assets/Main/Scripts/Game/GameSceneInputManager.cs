using System.Collections.Generic;

using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum PlayerGadget {
        Snowball,
        Hammer,
        Wrench,
        Mover
    }

    public class GameSceneInputManager : MonoBehaviour {


        public float fireKeyReleaseTime;

        public Color snowballDirInstructorColor;
        public Color hammerDirInstructorColor;
        public Color wrenchDirInstructorColor;
        public Color moverDirInstructorColor;


        public bool IsCharacterControllable => (!Global.gameSceneManager.IsInGameMenuOpened && Global.CurrentRoundInstance != null && Global.CurrentRoundInstance.IsRunning);

// ===================== temp =====================
        public Vector2 CurrentMoveInput => IsCharacterControllable ? (Input.GetAxisRaw("Horizontal") * Vector2.right + Input.GetAxisRaw("Vertical") * Vector2.up).normalized : Vector2.zero;
// ===================== ==== =====================

        public bool OptionsButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.menu);
        public bool InGameInfoButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.inGameInfo);
        public bool FireButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.fire);
        public bool SecondaryFireButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.secondaryFire);
        public bool CollectButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.collect);
        public bool QuackButtonDown => Input.GetKeyDown(Global.currentInputMethodKeys.quack);

        public bool FireButton => Input.GetKey(Global.currentInputMethodKeys.fire) || Time.time - _prevFireKeyUpTime < fireKeyReleaseTime;
        public bool SecondaryFireButton => Input.GetKey(Global.currentInputMethodKeys.secondaryFire);

        public bool InGameInfoButtonUp => Input.GetKeyUp(Global.currentInputMethodKeys.inGameInfo);
        public bool FireButtonUp => Input.GetKeyUp(Global.currentInputMethodKeys.fire);

        public PlayerGadget CurrentGadget => _currentGadget;

        public Color CurrentFireDirInstructorColor => (_currentGadget == PlayerGadget.Snowball) ? snowballDirInstructorColor :
                                                      (_currentGadget == PlayerGadget.Hammer)   ? hammerDirInstructorColor   :
                                                      (_currentGadget == PlayerGadget.Wrench)   ? wrenchDirInstructorColor   :
                                                      (_currentGadget == PlayerGadget.Mover)    ? moverDirInstructorColor    :
                                                      Color.white;


        Dictionary<KeyCode, PlayerGadget> _gadgetsSwtichByKey = new Dictionary<KeyCode, PlayerGadget>() {
            { KeyCode.Alpha1, PlayerGadget.Snowball },
            { KeyCode.Alpha2, PlayerGadget.Hammer },
            { KeyCode.Alpha3, PlayerGadget.Wrench },
            { KeyCode.Alpha4, PlayerGadget.Mover }
        };
        List<PlayerGadget> _enabledGadgets = new List<PlayerGadget>();
        PlayerGadget _currentGadget = PlayerGadget.Snowball;
        float _prevFireKeyUpTime = 0f;
        bool _isCurrentCurveBallDirRight = true;

        public void InitForRound () {

            _enabledGadgets.Clear();
            _enabledGadgets.Add(PlayerGadget.Snowball);

            if (!Global.CurrentRoundInstance.activeRules.buildProhibited)
                _enabledGadgets.Add(PlayerGadget.Hammer);

            if (!Global.CurrentRoundInstance.activeRules.repairProhibited)
                _enabledGadgets.Add(PlayerGadget.Wrench);

            if (Global.CurrentRoundInstance.activeRules.eggsWallMovable)
                _enabledGadgets.Add(PlayerGadget.Mover);

            SwitchGadget(PlayerGadget.Snowball);
        }

        public void InitForVoting() {

        }

        void Update () {

            if (OptionsButtonDown) {
                Global.gameSceneManager.SwitchInGameMenu();
            }

            if (InGameInfoButtonDown) {
                Global.gameSceneManager.OpenInGameInfoPanel();
            }
            if (InGameInfoButtonUp) {
                Global.gameSceneManager.CloseInGameInfoPanel();
            }

            if (IsCharacterControllable) {

                // Player fire direction instructor
                PlayerManager.LocalPlayerManager.ShowFireDirInstructor(CurrentFireDirInstructorColor);

                // Player build preview
                if (_currentGadget == PlayerGadget.Hammer && PlayerManager.LocalPlayerManager.IsActionable) {
                    PlayerManager.LocalPlayerManager.ShowBuildPreview();
                }

                // Reveal Repairable Target Statue
                if (_currentGadget == PlayerGadget.Wrench && PlayerManager.LocalPlayerManager.IsActionable) {
                    PlayerManager.LocalPlayerManager.RevealRepairableTarget();
                }

                // Switching Gadgets
                foreach (var key in _gadgetsSwtichByKey.Keys) {
                    if (Input.GetKeyDown(key)) {

                        PlayerGadget gadget = _gadgetsSwtichByKey[key];
                        SwitchGadget(gadget);
                        break;
                    }
                }

                // Actions
                if (FireButtonDown) {
                    if (_currentGadget == PlayerGadget.Snowball) {
                        if (Global.CurrentRoundInstance.activeRules.curveBall)
                            PlayerManager.LocalPlayerManager.TryToFire(_isCurrentCurveBallDirRight);
                        else
                            PlayerManager.LocalPlayerManager.TryToFire();
                    }
                    else if (_currentGadget == PlayerGadget.Hammer) {
                        PlayerManager.LocalPlayerManager.TryToBuild();
                    }
                    else if (_currentGadget == PlayerGadget.Wrench) {
                        PlayerManager.LocalPlayerManager.TryToRepair();
                    }
                    else if (_currentGadget == PlayerGadget.Mover) {
                        // PlayerManager.LocalPlayerManager.
                    }
                }
                else if (SecondaryFireButtonDown) {
                    if (Global.CurrentRoundInstance.activeRules.duckSnowCake || Global.globalManager.permanentDuckSnowCake) {
                        PlayerManager.LocalPlayerManager.TryToSwingCake();
                    }
                }
                else if (CollectButtonDown) {
                    PlayerManager.LocalPlayerManager.TryToCollect();
                }
                else if (QuackButtonDown) {
                    PlayerManager.LocalPlayerManager.TryToQuack();
                }


                if (FireButtonUp) {
                    _prevFireKeyUpTime = Time.time;
                }
            }

        }

        void SwitchGadget (PlayerGadget gadget) {
            if (_enabledGadgets.Contains(gadget)) {
                _currentGadget = gadget;

                if (gadget == PlayerGadget.Snowball && Global.CurrentRoundInstance.activeRules.curveBall) {
                    _isCurrentCurveBallDirRight = !_isCurrentCurveBallDirRight;
                }

                if (Global.CurrentRoundInstance != null && Global.CurrentRoundInstance.IsInited)
                    Global.CurrentRoundInstance.gadgetsDisplay.UpdateDisplay(_currentGadget, _enabledGadgets);
            }
        }


    }
}
