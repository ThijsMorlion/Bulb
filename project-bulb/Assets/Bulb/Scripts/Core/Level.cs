using System.Collections;
using System.Collections.Generic;
using Bulb.Controllers;
using Bulb.Electricity;
using Bulb.Game;
using Bulb.LevelEditor.Popups;
using UnityEngine;

namespace Bulb.Core
{
    public class Level
    {
        public enum PropertyType
        {
            Wire,
            Battery4V,
            Battery9V,
            Bulb,
            Motor,
            Buzzer,
            Switch,
            GoalDescription,
            CanBridge,
            CanSnap,
            CanBranch,
            SuccessByShortCircuit
        }

        public string GoalDescription { get; set; }
        public int MaxWire { get; set; }
        public int Max4VBatteries { get; set; }
        public int Max9VBatteries { get; set; }
        public int MaxBulbs { get; set; }
        public int MaxMotors { get; set; }
        public int MaxBuzzers { get; set; }
        public int MaxSwitches { get; set; }

        public bool CanBridge { get; set; }
        public bool CanSnap { get; set; }
        public bool CanBranch { get; set; }

        public bool SuccessByShortCircuit { get; set; }

        public Level()
        {
            CurrentWalker.OnSimulationSucceeded += CurrentWalker_OnSimulationSucceeded;
            CurrentWalker.OnSimulationFailed += CurrentWalker_OnSimulationFailed;
        }

        public void Clean()
        {
            CurrentWalker.OnSimulationSucceeded -= CurrentWalker_OnSimulationSucceeded;
            CurrentWalker.OnSimulationFailed -= CurrentWalker_OnSimulationFailed;
            WarningPopup.Instance.OnPopupClosed -= WarningPopup_OnPopupClosed;
        }

        private void CurrentWalker_OnSimulationSucceeded()
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                if (SuccessByShortCircuit == false)
                {
                    WarningPopup.Instance.StartCoroutine(ShowSuccessMessage());
                }
                else
                {
                    WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Kijk nog eens goed naar je doelen!");
                }
            }
        }

        private void CurrentWalker_OnSimulationFailed(ErrorCode code)
        {
            if (code == ErrorCode.ShortCircuit)
            {
                if (SuccessByShortCircuit)
                {
                    WarningPopup.Instance.StartCoroutine(ShowSuccessMessage());
                }
                else
                {
                    WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Je hebt een kortsluiting gemaakt!");
                    WarningPopup.Instance.OnPopupClosed += WarningPopup_OnPopupClosedForReset;
                }
            }
        }

        private IEnumerator ShowSuccessMessage()
        {
            yield return new WaitForSeconds(0f);

            WarningPopup.Instance.OnPopupClosed += WarningPopup_OnPopupClosed;
            WarningPopup.Instance.PopupMessage(WarningPopup.Type.Info, "Goed gedaan!");
        }

        private void WarningPopup_OnPopupClosed()
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                WarningPopup.Instance.OnPopupClosed -= WarningPopup_OnPopupClosed;
                CurrentWalker.OnSimulationSucceeded -= CurrentWalker_OnSimulationSucceeded;

                var analytics = ApplicationController.Instance.AnalyticsController;
                var chapterIndexParam = new KeyValuePair<string, object>(BulbEvents.ChapterIndexParam, ApplicationController.Instance.ChapterController.CurrentChapterIndex);
                var levelIndexParam = new KeyValuePair<string, object>(BulbEvents.LevelIndexParam, ApplicationController.Instance.LevelController.CurrentLevelIndex);
                var levelCompletionTime = new KeyValuePair<string, object>(BulbEvents.LevelCompletionTime, BulbEvents.FinishTimer().ToString());
                analytics.LogCustomEvent(BulbEvents.Category_UserEvents, BulbEvents.Action_Level, BulbEvents.LevelCompleted, chapterIndexParam, levelIndexParam, levelCompletionTime);


                PlayerState.UnlockNextLevel();

                if (ApplicationController.Instance.LevelController.WorldViewAnimationIndices.Contains(ApplicationController.Instance.LevelController.CurrentLevelIndex + 1))
                    ApplicationController.Instance.LevelController.GoToChapterMain();
                else
                    ApplicationController.Instance.LevelController.LoadNextLevel();
            }
        }

        private void WarningPopup_OnPopupClosedForReset()
        {
            ApplicationController.Instance.WireController.ResetAllWirePieces();
            ApplicationController.Instance.CharacterController.ResetAllCharacters();
            ApplicationController.Instance.CurrentWalker.SetIsSimulating(false);
            WarningPopup.Instance.OnPopupClosed -= WarningPopup_OnPopupClosedForReset;

        }
    }
}
