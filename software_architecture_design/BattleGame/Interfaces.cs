using System;

namespace BattleGame
{
    // Интерфейс для специальных способностей
    public interface ISpecialAbility
    {
        int Range { get; }
        int Power { get; }
        bool CanUseAbility(Army targetArmy, int currentPosition);
        void UseAbility(Army targetArmy, int currentPosition, IGameLogger logger);
    }

    // Интерфейс для логирования
    public interface IGameLogger
    {
        void LogRoundStart(int round, Army army1, Army army2);
        void LogRoundEnd(int round, Army army1, Army army2);
        void LogStageStart(int stage, string stageName);
        void LogAttack(Unit attacker, Unit target, int damage, int oldHealth, int newHealth);
        void LogCounterAttack(Unit attacker, Unit target, int damage, int oldHealth, int newHealth);
        void LogSpecialAbility(Unit archer, Unit target, int power, int oldHealth, int newHealth);
        void LogCleanup(int deadInArmy1, int deadInArmy2, int aliveInArmy1, int aliveInArmy2);
        void LogMessage(string message);
        void LogBattleEnd(Army winner);
    }
}