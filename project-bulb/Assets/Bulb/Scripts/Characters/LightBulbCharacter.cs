using Bulb.Data;
using System;
using UnityEngine;

namespace Bulb.Characters
{
    public class LightBulbCharacter : CharacterBase
    {
        [Header("Light Bulb Properties")]
        public LightBulbParams Light;
        public bool InSocket;

        private float _currentIntensity = 0f;

        protected override void Awake()
        {
            base.Awake();

            Image.material.SetFloat("_BlendFactor", Light.Intensity);
        }

        public override void PowerOn(float value)
        {
            var percentage = Mathf.Clamp01(value / Power);
            Light.Intensity = percentage;
        }

        protected override void Update()
        {
            base.Update();

            if (_currentIntensity != Light.Intensity)
            {
                _currentIntensity = Mathf.MoveTowards(_currentIntensity, Light.Intensity, .05f);
                Image.material.SetFloat("_BlendFactor", _currentIntensity);
            }
        }

        public override void Reset()
        {
            base.Reset();

            Light.Intensity = 0f;
        }

        public new LightBulbCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new LightBulbCharacterData()
            {
                Params = Light,
                InSocket = InSocket,
                GridCellPositions = charData.GridCellPositions,
                PositionOnGrid = charData.PositionOnGrid,
                Rotation = charData.Rotation,
                State = charData.State,
                Type = charData.Type
            };

            return data;
        }
    }

    [Serializable]
    public struct LightBulbParams
    {
        public bool IsOn;

        [Range(0,1)]
        public float Intensity;
    }
}