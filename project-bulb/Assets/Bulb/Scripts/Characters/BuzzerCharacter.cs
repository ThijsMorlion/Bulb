using Bulb.Data;
using System;
using UnityEngine;

namespace Bulb.Characters
{
    public class BuzzerCharacter : CharacterBase
    {
        [Header("Buzzer Properties")]
        public BuzzerParams BuzzerParams;
        public AudioSource Audio;

        public new BuzzerCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new BuzzerCharacterData()
            {
                Params = BuzzerParams,
                GridCellPositions = charData.GridCellPositions,
                PositionOnGrid = charData.PositionOnGrid,
                Rotation = charData.Rotation,
                State = charData.State,
                Type = charData.Type
            };

            return data;
        }

        public override void Reset()
        {
            base.Reset();

            Resistance = BuzzerParams.LowResistance;
            PowerOn(0);
        }

        public void EvaluateDiodeDirection()
        {
            var negPoleConn = GetConnection(Electricity.Pole.Negative).Current;
            var posPoleConn = GetConnection(Electricity.Pole.Positive).Current;

            if(!float.IsNaN(negPoleConn) && float.IsNaN(posPoleConn))
            {
                Resistance = BuzzerParams.HighResistance;
            }
            else if(float.IsNaN(negPoleConn) && !float.IsNaN(posPoleConn))
            {
                Resistance = BuzzerParams.LowResistance;
            }
        }

        public override void EvaluatePower()
        {
            if(!float.IsNaN(Current))
            {
                PowerOn(Resistance == BuzzerParams.LowResistance ? 1 : 0);
            }
        }

        public override void PowerOn(float value)
        {
            if (value != 0)
            {
                Audio.Play();
                DebugColor = Color.yellow;
            }
            else
            {
                Audio.Stop();
                DebugColor = Color.white;
            }
        }
    }

    [Serializable]
    public struct BuzzerParams
    {
        public float LowResistance;
        public float HighResistance;
    }
}