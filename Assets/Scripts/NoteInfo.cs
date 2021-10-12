using UnityEngine;

/// <summary>
/// 表示 <c>Note</c> 的类型
/// </summary>
public enum NoteType
{
    /// <summary>
    /// 伪音符，不需要按键，仅用于调整轨道
    /// </summary>
    Pseudo,
    /// <summary>
    /// 拥有精确的判定点，参与判定
    /// </summary>
    Single,
    /// <summary>
    /// 不需要精确点击，只需要在判定时为持续按下状态即可
    /// </summary>
    Hold,
}

/// <summary>
/// 表示 <c>Note</c> 的渲染样式
/// </summary>
public enum NoteStyle
{
    Normal,
}

/// <summary>
/// 在场景中生成 <c>Note</c> 所需要的信息
/// </summary>
public class NoteInfo
{
    /// <summary>
    /// <c>Note</c> 所在的轨道
    /// </summary>
    public int Track;
    /// <summary>
    /// <c>Note</c> 的类型
    /// </summary>
    public NoteType NoteType;
    /// <summary>
    /// <c>Note</c> 的显示风格
    /// </summary>
    public NoteStyle NoteStyle;
    /// <summary>
    /// <c>Note</c> 所属的判定组，通常用于长键中。
    /// 0表示单独成组。
    /// </summary>
    public int Group;
    /// <summary>
    /// <c>Note</c> 出现时所在的位置，默认为 (0, 0, 10)
    /// </summary>
    public Vector3 AppearedAtPos;
    /// <summary>
    /// <c>Note</c> 判定时所在的位置
    /// </summary>
    public Vector3 ShouldHitAtPos;
    /// <summary>
    /// <c>Note</c> 出现时的时间，以音乐的采样点为单位。
    /// </summary>
    public int AppearedAtSample;
    /// <summary>
    /// <c>Note</c> 判定时的时间，以音乐的采样点为单位。
    /// </summary>
    public int ShouldHitAtSample;

    /// <summary>
    /// <c>Note</c> 在 <c>sampleTime</c> 时刻所在的位置。
    /// </summary>
    public Vector3 CalcPosition(int sampleTime)
    {
        var duration = ShouldHitAtSample - AppearedAtSample;
        var deltaTime = sampleTime - AppearedAtSample;
        var t = (float)deltaTime / duration;
        var deltaPos = ShouldHitAtPos - AppearedAtPos;
        return AppearedAtPos + deltaPos * t;
    }
}
