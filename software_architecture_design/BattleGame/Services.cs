using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BattleGame
{
    // ---------- ЛОГГЕР ----------
    public class ConsoleGameLogger : IGameLogger
    {
        public void LogRoundStart(int round, Army army1, Army army2)
        {
            Console.WriteLine($"\n╔═══ РАУНД {round} ═══╗");
            Console.WriteLine($"Армия 1: {army1.AliveCount} юнитов | Армия 2: {army2.AliveCount} юнитов");
            Console.WriteLine(new string('─', 40));
        }

        public void LogRoundEnd(int round, Army army1, Army army2)
        {
            Console.WriteLine(new string('─', 40));
            Console.WriteLine($"Итог раунда {round}: A1:{army1.AliveCount} A2:{army2.AliveCount}");
            Console.WriteLine(new string('═', 40));
        }

        public void LogStageStart(int stage, string stageName)
        {
            Console.WriteLine($"\n▶ Этап {stage}: {stageName}");
        }

        public void LogAttack(Unit attacker, Unit target, int damage, int oldHealth, int newHealth)
        {
            string deathMark = !target.IsAlive ? " ★УБИТ★" : "";
            Console.WriteLine($"  ⚔ {attacker.Name} → {target.Name}: {damage} урона ({oldHealth}→{newHealth}){deathMark}");
        }

        public void LogCounterAttack(Unit attacker, Unit target, int damage, int oldHealth, int newHealth)
        {
            string deathMark = !target.IsAlive ? " ★УБИТ★" : "";
            Console.WriteLine($"  ⚔ ОТВЕТ: {attacker.Name} → {target.Name}: {damage} урона ({oldHealth}→{newHealth}){deathMark}");
        }

        public void LogSpecialAbility(Unit archer, Unit target, int power, int oldHealth, int newHealth)
        {
            string deathMark = !target.IsAlive ? " ★УБИТ★" : "";
            Console.WriteLine($"  🏹 СПЕЦ: {archer.Name} → {target.Name}: {power} урона ({oldHealth}→{newHealth}){deathMark}");
        }

        public void LogCleanup(int deadInArmy1, int deadInArmy2, int aliveInArmy1, int aliveInArmy2)
        {
            Console.WriteLine($"  Удалено мертвых: A1:{deadInArmy1} A2:{deadInArmy2}");
            Console.WriteLine($"  Осталось живых: A1:{aliveInArmy1} A2:{aliveInArmy2}");
        }

        public void LogMessage(string message)
        {
            Console.WriteLine($"  {message}");
        }

        public void LogBattleEnd(Army winner)
        {
            Console.WriteLine($"\n🎉 ПОБЕДА! {winner.Name} 🎉");
        }
    }

    // ---------- СОЗДАТЕЛЬ АРМИЙ ----------
    public class ArmyCreator
    {
        private Random _random = new Random();

        public Army CreateManualArmy(int budget, string armyName)
        {
            Army army = new Army { Name = armyName };
            int remaining = budget;

            Console.WriteLine($"\n════════════════════════════════");
            Console.WriteLine($"СОЗДАНИЕ {armyName} (бюджет: {budget})");
            Console.WriteLine($"════════════════════════════════");

            while (remaining >= 80)
            {
                Console.WriteLine($"\nОсталось: {remaining} монет");
                Console.WriteLine("1. Тяжелый пехотинец (150)");
                Console.WriteLine("2. Легкий пехотинец (80)");
                Console.WriteLine("3. Лучник (120)");
                Console.WriteLine("0. Завершить создание");
                Console.Write("Ваш выбор: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Ошибка ввода!");
                    continue;
                }

                if (choice == 0) break;

                Unit unit = choice switch
                {
                    1 => new HeavyInfantry(),
                    2 => new LightInfantry(),
                    3 => new Archer(),
                    _ => null
                };

                if (unit == null)
                {
                    Console.WriteLine("Неверный выбор!");
                    continue;
                }

                if (unit.Price > remaining)
                {
                    Console.WriteLine($"Не хватает денег! Нужно {unit.Price}, есть {remaining}");
                    continue;
                }

                army.AddUnit(unit);
                remaining -= unit.Price;
                Console.WriteLine($"Добавлен {unit.Name}");
            }

            Console.WriteLine($"\nАрмия создана! Всего юнитов: {army.Units.Count}");
            return army;
        }

        public Army CreateRandomArmy(int budget, string armyName)
        {
            Army army = new Army { Name = armyName };
            int remaining = budget;

            Console.WriteLine($"\nСоздание случайной армии {armyName} на бюджет {budget}...");

            while (remaining >= 80)
            {
                int type = _random.Next(1, 4);
                Unit unit = type switch
                {
                    1 => new HeavyInfantry(),
                    2 => new LightInfantry(),
                    3 => new Archer(),
                    _ => new LightInfantry()
                };

                if (unit.Price <= remaining)
                {
                    army.AddUnit(unit);
                    remaining -= unit.Price;
                    Console.WriteLine($"  + {unit.Name} (осталось: {remaining})");
                }
            }

            Console.WriteLine($"Армия создана! Всего юнитов: {army.Units.Count}");
            return army;
        }
    }

    // ---------- ДАННЫЕ ДЛЯ СОХРАНЕНИЯ ----------
    [Serializable]
    public class UnitData
    {
        public string Type { get; set; }
        public int Health { get; set; }
    }

    [Serializable]
    public class GameData
    {
        public List<UnitData> Army1Units { get; set; } = new List<UnitData>();
        public List<UnitData> Army2Units { get; set; } = new List<UnitData>();
        public int CurrentRound { get; set; }
    }

    // ---------- СОХРАНЕНИЕ/ЗАГРУЗКА ----------
    public class GameSaver
    {
        private readonly string _savesFolder = "saves";

        public GameSaver()
        {
            if (!Directory.Exists(_savesFolder))
                Directory.CreateDirectory(_savesFolder);
        }

        public void SaveGame(Battlefield battlefield, string fileName)
        {
            var data = new GameData
            {
                Army1Units = battlefield.Army1.Units.Select(u => new UnitData
                {
                    Type = u.GetType().Name,
                    Health = u.Health
                }).ToList(),

                Army2Units = battlefield.Army2.Units.Select(u => new UnitData
                {
                    Type = u.GetType().Name,
                    Health = u.Health
                }).ToList(),

                CurrentRound = 0 // Нужно добавить свойство в Battlefield
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            string path = Path.Combine(_savesFolder, fileName + ".json");

            File.WriteAllText(path, json);
            Console.WriteLine($"Игра сохранена в файл {fileName}.json");
        }

        public GameData LoadGame(string fileName)
        {
            string path = Path.Combine(_savesFolder, fileName + ".json");

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден!");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameData>(json);
        }
    }
}