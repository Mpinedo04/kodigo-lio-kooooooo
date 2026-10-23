namespace CodeLyokoFanGame
{
    // Lets a component cancel a hit before it lands (Odd's shield). Checked by Health after the parry.
    public interface IDamageInterceptor
    {
        bool TryIntercept(DamageInfo info);
    }
}
