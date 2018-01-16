using System.Collections.Generic;
using Bulb.Core;
using Bulb.Electricity;
using Bulb.Game;
using Create.Analytics;
using UnityEngine;

namespace Bulb.Controllers
{
    public class ApplicationController : MonoBehaviour
    {
        public static ApplicationController Instance { get; private set; }

        #region CONTROLLERS

        public DataController DataController { get; private set; }
        public LevelController LevelController { get; private set; }
        public ChapterController ChapterController { get; private set; }
        public AnalyticsController AnalyticsController { get; private set; }

        private CharacterBaseController _characterController;
        public CharacterBaseController CharacterController
        {
            get
            {
                if (_characterController == null)
                    _characterController = FindObjectOfType<CharacterBaseController>();

                return _characterController;
            }
        }

        private SelectionController _selectionController;
        public SelectionController SelectionController
        {
            get
            {
                if (_selectionController == null)
                    _selectionController = FindObjectOfType<SelectionController>();

                return _selectionController;
            }
        }

        private DebugController _debugController;
        public DebugController DebugController
        {
            get
            {
                if (_debugController == null)
                    _debugController = FindObjectOfType<DebugController>();

                return _debugController;
            }
        }

        private WireController _wireController;
        public WireController WireController
        {
            get
            {
                if (_wireController == null)
                    _wireController = FindObjectOfType<WireController>();

                return _wireController;
            }
        }

        private CurrentWalker _currentWalker;
        public CurrentWalker CurrentWalker
        {
            get
            {
                if (_currentWalker == null)
                    _currentWalker = FindObjectOfType<CurrentWalker>();

                return _currentWalker;
            }
        }

        private CanvasController _canvasController;
        public CanvasController CanvasController
        {
            get
            {
                if (_canvasController == null)
                    _canvasController = FindObjectOfType<CanvasController>();

                return _canvasController;
            }
        }

        #endregion

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            DataController = GetComponent<DataController>();
            LevelController = GetComponent<LevelController>();
            ChapterController = GetComponent<ChapterController>();
            AnalyticsController = GetComponent<AnalyticsController>();
        }

        private void OnEnable()
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                var buildVersionParam = new KeyValuePair<string, object>(BulbEvents.BuildVersionNumberParam, Application.version);
                AnalyticsController.LogCustomEvent(BulbEvents.Category_GeneralEvents, BulbEvents.Action_Game, BulbEvents.GameLaunched, buildVersionParam);
            }
        }
    }
}