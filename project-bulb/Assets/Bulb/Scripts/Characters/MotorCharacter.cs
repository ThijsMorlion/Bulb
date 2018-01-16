using System;
using Bulb.Data;
using UnityEngine;

namespace Bulb.Characters
{
    public class MotorCharacter : CharacterBase
    {
        [Header("Motor Properties")]
        public MotorParams MotorParams;
        public Animator Animator;

        private float _currentSpeed = 0f;

        public override void PowerOn(float value)
        {
            var percentage = value / Power;
            _currentSpeed = MotorParams.MaxSpeed * percentage;
            
            var potDiff = GetPotentialDifference();
            if(float.IsNaN(potDiff) == false)
            {
                Animator.SetFloat("potentialSign", Mathf.Sign(potDiff) * _currentSpeed);
                Animator.SetBool("isRotating", true);
            }
            else
            {
                Animator.SetBool("isRotating", false);
            }
        }

        public new MotorCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new MotorCharacterData()
            {
                Params = MotorParams,
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
    public struct MotorParams
    {
        public float MaxSpeed;
    }
}
