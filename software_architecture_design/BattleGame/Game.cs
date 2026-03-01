using System;

namespace BattleGame
{
    public class Game
    {
        private Battlefield _battlefield;
        private readonly ArmyCreator _creator = new ArmyCreator();
        private readonly IGameLogger _logger = new ConsoleGameLogger();
        private readonly GameSaver _saver = new GameSaver();

        public void Start()
        {
            Console.WriteLine("╔════════════════════════════════╗");
            Console.WriteLine("║         БИТВА АРМИЙ           ║");
            Console.WriteLine("╚════════════════════════════════╝");

            int budget = GetBudgetFromUser();

            // Создание армий
            Army army1 = CreateArmy(budget, "АРМИЯ 1");
            Army army2 = CreateArmy(budget, "АРМИЯ 2");

            // Показываем составы
            ShowArmyComposition(army1);
            ShowArmyComposition(army2);

            // Начинаем битву
            _battlefield = new Battlefield(army1, army2, _logger);
            RunGameLoop();
        }

        private int GetBudgetFromUser()
        {
            while (true)
            {
                Console.Write("\nВведите бюджет для армий (мин. 80): ");
                if (int.TryParse(Console.ReadLine(), out int budget) && budget >= 80)
                    return budget;

                Console.WriteLine("Ошибка! Бюджет должен быть числом не меньше 80");
            }
        }

        private Army CreateArmy(int budget, string armyName)
        {
            Console.WriteLine($"\nСоздание {armyName}");
            Console.WriteLine("1. Создать случайно");
            Console.WriteLine("2. Создать вручную");
            Console.Write("Выберите способ: ");

            string choice = Console.ReadLine();

            return choice == "1"
                ? _creator.CreateRandomArmy(budget, armyName)
                : _creator.CreateManualArmy(budget, armyName);
        }

        private void ShowArmyComposition(Army army)
        {
            Console.WriteLine($"\nСостав {army.Name}:");
            for (int i = 0; i < army.Units.Count; i++)
            {
                Unit u = army.Units[i];
                string saMark = u is ISpecialAbility ? " [SA]" : "";
                Console.WriteLine($"  {i + 1}. {u.Name}{saMark} ❤{u.Health} ⚔{u.Damage} 🛡{u.Defense}");
            }
            Console.WriteLine($"Общая стоимость: {army.TotalPrice}");
        }

        private void RunGameLoop()
        {
            bool playing = true;

            while (playing && !_battlefield.IsBattleOver)
            {
                Console.WriteLine("\n╔════════════════════════════════╗");
                Console.WriteLine("║            МЕНЮ               ║");
                Console.WriteLine("╚════════════════════════════════╝");
                Console.WriteLine("1. Сделать ход");
                Console.WriteLine("2. Автоматическая битва до конца");
                Console.WriteLine("3. Показать состояние армий");
                Console.WriteLine("4. Сохранить игру");
                Console.WriteLine("5. Загрузить игру");
                Console.WriteLine("6. Выход");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _battlefield.MakeMove();
                        break;

                    case "2":
                        Console.WriteLine("\nЗапуск автоматической битвы...");
                        _battlefield.MakeFullAutoBattle();
                        playing = false;
                        break;

                    case "3":
                        ShowArmyStatus();
                        break;

                    case "4":
                        Console.Write("Введите имя файла для сохранения: ");
                        string saveName = Console.ReadLine();
                        _saver.SaveGame(_battlefield, saveName);
                        break;

                    case "5":
                        Console.Write("Введите имя файла для загрузки: ");
                        string loadName = Console.ReadLine();
                        LoadGame(loadName);
                        break;

                    case "6":
                        playing = false;
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        private void ShowArmyStatus()
        {
            Console.WriteLine("\nТЕКУЩЕЕ СОСТОЯНИЕ:");
            Console.WriteLine($"Армия 1: {_battlefield.Army1.AliveCount} живых юнитов");
            Console.WriteLine($"Армия 2: {_battlefield.Army2.AliveCount} живых юнитов");
        }

        private void LoadGame(string fileName)
        {
            var data = _saver.LoadGame(fileName);

            if (data == null)
            {
                Console.WriteLine("Не удалось загрузить игру");
                return;
            }

            Console.WriteLine("Загрузка игры... (функция требует доработки)");
            // Здесь нужно восстановить состояние игры из data
        }
    }
}