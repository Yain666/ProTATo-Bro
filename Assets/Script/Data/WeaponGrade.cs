/// <summary>
/// 武器品阶相关常量与系数（方案 A：全局系数，后续可换成每阶配表）。
/// 品阶 1=普通 2=稀有 3=史诗 4=神话。
/// </summary>
public static class WeaponGrade
{
    public const int Min = 1;
    public const int Mythic = 4; // 神话封顶，不再进化

    // 数值系数：武器最终数值 = 基础值 × StatMultiplier(grade)
    private static readonly float[] StatMul = { 1f, 1f, 1.5f, 2.25f, 3.4f }; // 下标即品阶，[0] 占位

    // 价格系数：售价 = 基础 coin × PriceMultiplier(grade)
    private static readonly int[] PriceMul = { 1, 1, 2, 4, 8 };

    public static int Clamp(int grade)
    {
        if (grade < Min) return Min;
        if (grade > Mythic) return Mythic;
        return grade;
    }

    public static float StatMultiplier(int grade)
    {
        return StatMul[Clamp(grade)];
    }

    public static int PriceMultiplier(int grade)
    {
        return PriceMul[Clamp(grade)];
    }
}
