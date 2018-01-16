using System.Collections.Generic;

namespace Bulb.Data
{
    public class LevelData
    {
        public int LevelDataVersionNumber; 

        #region LEVEL DATA
        public string GoalDescription;
        public int MaxWireAvailable;
        public int Max4VBatteries;
        public int Max9VBatteries;
        public int MaxBulbs;
        public int MaxMotors;
        public int MaxBuzzers;
        public int MaxSwitches;

        public bool CanBridge;
        public bool CanSnap;
        public bool CanBranch;

        public bool SuccessByShortCircuit;
        #endregion

        public GridData Grid;

        #region DRAWABLE DATA
        public List<BatteryCharacterData> Batteries;
        public List<LightBulbCharacterData> LightBulbs;
        public List<ObstructionCharacterData> Obstructions;
        public List<MotorCharacterData> Motors;
        public List<BuzzerCharacterData> Buzzers;
        public List<SwitchCharacterData> Switches;
        public List<WirePieceData> WirePieces;
        #endregion
    }
}