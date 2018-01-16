using Settings.Model;
using UnityEngine;

public class ExtendedManagerSettings : BaseSettings
{
    ////add your application-specific settings here, examples:
    //public Setting<bool> BoolSetting = new Setting<bool> { Category = "extended settings", Defaultvalue = false };
    //public Setting<string> StringSetting = new Setting<string> { Category = "extended settings", Defaultvalue = "" };
    //public Setting<int> IntSetting = new Setting<int> { Category = "extended settings", Defaultvalue = 0 };
    //public Setting<float> FloatSetting = new Setting<float> { Category = "extended settings", Defaultvalue = 0 };
    //public MinMaxSetting SliderSetting = new MinMaxSetting { Category = "extended settings", Min = 0, Max = 1, Defaultvalue = 0 };
    //public Setting<Vector2> Vector2Setting = new Setting<Vector2> { Category = "extended settings", Defaultvalue = new Vector2(0, 0) };
    //public Setting<Vector3> Vector3Setting = new Setting<Vector3> { Category = "extended settings", Defaultvalue = new Vector3(0, 0, 0) };

    public Setting<bool> ShowCurrent = new Setting<bool> { Category = "Wire Debug Settings", Defaultvalue = false };
    public Setting<bool> ShowVoltage = new Setting<bool> { Category = "Wire Debug Settings", Defaultvalue = false };
    public Setting<bool> ShowWireDirection = new Setting<bool> { Category = "Wire Debug Settings", Defaultvalue = false };


    public Setting<bool> ShowCellOccupated = new Setting<bool> { Category = "Debug Settings", Defaultvalue = false };
    public Setting<bool> ShowCellCenter = new Setting<bool> { Category = "Debug Settings", Defaultvalue = false };
    public Setting<bool> ShowDrawablePivot = new Setting<bool> { Category = "Debug Settings", Defaultvalue = false };
    public Setting<bool> ShowDebugColors = new Setting<bool> { Category = "Debug Settings", Defaultvalue = false };
}