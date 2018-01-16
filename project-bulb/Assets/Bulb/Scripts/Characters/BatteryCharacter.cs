using Bulb.Data;
using System;
using UnityEngine;
using Bulb.Electricity;
using Bulb.Controllers;

namespace Bulb.Characters
{
    public class BatteryCharacter : CharacterBase
    {
        [Header("Battery Properties")]
        public BatteryParams Battery;

        private float _currentBlendFactor = 0f;
        private float _targetBlendFactor = 0f;

        protected override void Awake()
        {
            base.Awake();

            Image.material.SetFloat("_BlendFactor", 0f);
        }

        public void OnEnable()
        {
            var currentWalker = ApplicationController.Instance.CurrentWalker;
            CurrentWalker.OnSimulationFailed += CurrentWalker_OnSimulationFailed;
        }

        public void OnDisable()
        {
            var currentWalker = ApplicationController.Instance.CurrentWalker;
            CurrentWalker.OnSimulationFailed -= CurrentWalker_OnSimulationFailed;
        }

        private void CurrentWalker_OnSimulationFailed(ErrorCode code)
        {
            if (code == ErrorCode.ShortCircuit)
            {
                //SetImage(DeadImage, Color.white);
                _targetBlendFactor = 1f;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_currentBlendFactor != _targetBlendFactor)
            {
                _currentBlendFactor = Mathf.MoveTowards(_currentBlendFactor, _targetBlendFactor, 0.05f);
                Image.material.SetFloat("_BlendFactor", _currentBlendFactor);
            }
        }

        public override void Reset()
        {
            base.Reset();

            _currentBlendFactor = 0f;
            _targetBlendFactor = 0f;
            Image.material.SetFloat("_BlendFactor", _currentBlendFactor);
        }

        public new BatteryCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new BatteryCharacterData()
            {
                Params = Battery,
                GridCellPositions = charData.GridCellPositions,
                PositionOnGrid = charData.PositionOnGrid,
                Rotation = charData.Rotation,
                State = charData.State,
                Type = charData.Type
            };

            return data;
        }

        public bool IsShortCircuited()
        {
            return GetPotentialDifference() == 0;
        }
    }

    [Serializable]
    public struct BatteryParams
    {
        public int Voltage;
    }
}