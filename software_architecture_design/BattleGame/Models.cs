using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleGame
{
    // ---------- БАЗОВЫЙ КЛАСС ЮНИТА ----------
    public abstract class Unit
    {
        public string Name { get; protected set; }
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Damage { get; protected set; }
        public int Defense { get; protected set; }
        public int Price { get; protected set; }
        public bool IsAlive => Health > 0;

        protected Unit(string name, int health, int damage, int defense, int price)
        {
            Name = name;
            MaxHealth = health;
            Health = health;
            Damage = damage;
            Defense = defense;
            Price = price;
        }

        public virtual void TakeDamage(int incomingDamage)
        {
            int actualDamage = Math.Max(1, incomingDamage - Defense);
            Health = Math.Max(0, Health - actualDamage);
        }

        public virtual void Attack(Unit target)
        {
            if (target?.IsAlive == true)
                target.TakeDamage(Damage);
        }

        protected abstract int CalculatePrice();
    }

    // ---------- ТЯЖЕЛЫЙ ПЕХОТИНЕЦ ----------
    public class HeavyInfantry : Unit
    {
        public HeavyInfantry() : base("Тяжелый пехотинец", 120, 25, 15, 150) { }
        protected override int CalculatePrice() => 150;
    }

    // ---------- ЛЕГКИЙ ПЕХОТИНЕЦ ----------
    public class LightInfantry : Unit
    {
        public LightInfantry() : base("Легкий пехотинец", 80, 35, 5, 80) { }
        protected override int CalculatePrice() => 80;
    }

    // ---------- ЛУЧНИК ----------
    public class Archer : Unit, ISpecialAbility
    {
        public int Range { get; private set; }
        public int Power { get; private set; }

        public Archer() : base("Лучник", 60, 15, 5, 120)
        {
            Range = 3;
            Power = 25;
        }

        public bool CanUseAbility(Army targetArmy, int currentPosition)
        {
            for (int i = 0; i < targetArmy.Units.Count; i++)
            {
                if (targetArmy.Units[i].IsAlive && Math.Abs(currentPosition - i) <= Range)
                    return true;
            }
            return false;
        }

        public void UseAbility(Army targetArmy, int currentPosition, IGameLogger logger)
        {
            Unit target = null;
            int minDistance = int.MaxValue;

            for (int i = 0; i < targetArmy.Units.Count; i++)
            {
                if (targetArmy.Units[i].IsAlive)
                {
                    int distance = Math.Abs(currentPosition - i);
                    if (distance <= Range && distance < minDistance)
                    {
                        minDistance = distance;
                        target = targetArmy.Units[i];
                    }
                }
            }

            if (target != null)
            {
                int oldHealth = target.Health;
                target.TakeDamage(Power);
                logger?.LogSpecialAbility(this, target, Power, oldHealth, target.Health);
            }
        }

        protected override int CalculatePrice() => 120;
    }

    // ---------- АРМИЯ ----------
    public class Army
    {
        private List<Unit> _units = new List<Unit>();
        public IReadOnlyList<Unit> Units => _units.AsReadOnly();
        public string Name { get; set; }
        public bool IsAlive => _units.Any(u => u.IsAlive);
        public int TotalPrice => _units.Sum(u => u.Price);
        public int AliveCount => _units.Count(u => u.IsAlive);

        public void AddUnit(Unit unit) => _units.Add(unit);

        public Unit GetFirstAliveUnit() => _units.FirstOrDefault(u => u.IsAlive);

        public void RemoveDeadUnits() => _units.RemoveAll(u => !u.IsAlive);
    }

    // ---------- ПОЛЕ БИТВЫ ----------
    public class Battlefield
    {
        private Army _army1;
        private Army _army2;
        private int _currentRound = 0;
        private IGameLogger _logger;

        public Battlefield(Army army1, Army army2, IGameLogger logger)
        {
            _army1 = army1;
            _army2 = army2;
            _logger = logger;
        }

        public bool IsBattleOver => !_army1.IsAlive || !_army2.IsAlive;
        public Army Winner => !_army1.IsAlive ? _army2 : _army1;
        public Army Army1 => _army1;
        public Army Army2 => _army2;

        public void MakeMove()
        {
            if (IsBattleOver) return;

            _currentRound++;
            _logger.LogRoundStart(_currentRound, _army1, _army2);

            Stage1_FirstArmyAttack();
            Stage2_SecondArmyCounterAttack();
            Stage3_SpecialAbilities();
            Stage4_Cleanup();

            _logger.LogRoundEnd(_currentRound, _army1, _army2);
        }

        public void MakeFullAutoBattle()
        {
            while (!IsBattleOver)
                MakeMove();

            _logger.LogBattleEnd(Winner);
        }

        private void Stage1_FirstArmyAttack()
        {
            _logger.LogStageStart(1, "Атака первой армии");

            var attacker = _army1.GetFirstAliveUnit();
            var defender = _army2.GetFirstAliveUnit();

            if (attacker == null || defender == null)
                return;

            int oldHealth = defender.Health;
            attacker.Attack(defender);
            _logger.LogAttack(attacker, defender, attacker.Damage, oldHealth, defender.Health);
        }

        private void Stage2_SecondArmyCounterAttack()
        {
            _logger.LogStageStart(2, "Ответный удар");

            var attacker = _army2.GetFirstAliveUnit();
            var defender = _army1.GetFirstAliveUnit();

            if (attacker == null || defender == null)
                return;

            int oldHealth = defender.Health;
            attacker.Attack(defender);
            _logger.LogCounterAttack(attacker, defender, attacker.Damage, oldHealth, defender.Health);
        }

        private void Stage3_SpecialAbilities()
        {
            _logger.LogStageStart(3, "Специальные способности");

            int maxPositions = Math.Max(_army1.Units.Count, _army2.Units.Count);
            bool anyAbilityUsed = false;

            for (int pos = 0; pos < maxPositions; pos++)
            {
                if (pos < _army1.Units.Count && _army1.Units[pos] is ISpecialAbility sa1 && _army1.Units[pos].IsAlive)
                {
                    sa1.UseAbility(_army2, pos, _logger);
                    anyAbilityUsed = true;
                }

                if (pos < _army2.Units.Count && _army2.Units[pos] is ISpecialAbility sa2 && _army2.Units[pos].IsAlive)
                {
                    sa2.UseAbility(_army1, pos, _logger);
                    anyAbilityUsed = true;
                }
            }

            if (!anyAbilityUsed)
                _logger.LogMessage("Никто не использовал спецспособности");
        }

        private void Stage4_Cleanup()
        {
            _logger.LogStageStart(4, "Очистка поля");

            int dead1 = _army1.Units.Count(u => !u.IsAlive);
            int dead2 = _army2.Units.Count(u => !u.IsAlive);

            _army1.RemoveDeadUnits();
            _army2.RemoveDeadUnits();

            _logger.LogCleanup(dead1, dead2, _army1.AliveCount, _army2.AliveCount);
        }
    }
}