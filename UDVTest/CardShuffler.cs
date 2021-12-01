using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace UDVTest
{
    class CardShuffler
    {
        ///<Summary>
        ///Общее количество карт
        ///</Summary>
        static int cardCount;

        ///<Summary>
        ///Имя файла с сохранением
        ///</Summary>
        const string saveFileName = "save.json";

        ///<Summary>
        ///Название выбранной колоды карт
        ///</Summary>
        static string selectedDeckName;

        ///<Summary>
        ///Все созданные пользователем колоды
        ///</Summary>
        static Dictionary<string, int[]> decks;

        ///<Summary>
        ///Значения карт
        ///</Summary>
        static Dictionary<int, string> values = new Dictionary<int, string>
        {
            {0, "2"},
            {1, "3"},
            {2, "4"},
            {3, "5"},
            {4, "6"},
            {5, "7"},
            {6, "8"},
            {7, "9"},
            {8, "10"},
            {9, "J"},
            {10, "Q"},
            {11, "K"},
            {12, "A"}
        };

        ///<Summary>
        ///Масти карт
        ///</Summary>
        static Dictionary<int, string> suits = new Dictionary<int, string>
        {
            {0, "hearts"},
            {1, "diamonds"},
            {2, "spades"},
            {3, "clubs"}
        };

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding= System.Text.Encoding.Unicode;
            
            //Считаем количество карт
            if (suits.Count > 0)
            {
                //Если количество мастей больше нуля
                cardCount = values.Count * suits.Count;
            }
            else
            {
                //Мастей нет
                cardCount = values.Count;
            }
            
            if (cardCount > 0)
            {
                Console.WriteLine("-= Welcome! =-\n");

                //Загружаем колоды         
                Load();
            
                //Отображаем главное меню
                CheckMainInput();  
            }
            else
            {
                Console.WriteLine("-= No cards found! =-\n");
                return;
            }    
        }

#region InputManagement
        ///<Summary>
        ///Проверяет ввод в главном меню
        ///</Summary>
        static void CheckMainInput()
        {
            //Выводим сообщение со списком команд
            Console.Write("Command list: /create + 'name' | /select + 'name' | /list | /close\nEnter command: ");

            //Проверяем пользовательский ввод
            string[] userInput = Console.ReadLine().Split(' ');

            //Переводим все символы команды в нижний регистр
            string command = userInput[0].ToLower();

            //Проверка ввода: если первое слово не является командой или если помимо команд /list и /close присутствует другой ввод, или если введено более 2 слов
            if ((command != "/create" && command != "/select" && command != "/list" && command != "/close") ||
            ((command == "/list" || command == "/close") && userInput.Length > 1) ||
            userInput.Length > 2)      
            {
                //Неверный ввод команды
                Console.WriteLine("\n-= Incorrect input =-\n");
                CheckMainInput();
            } 

            //Проверка ввода: если введена команда /create или /select, но не введено название колоды или если название пустое
            else if (((command == "/create" || command == "/select") && userInput.Length == 1) || 
            (userInput.Length == 2 && String.IsNullOrWhiteSpace(userInput[1])))
            {
                //Неверный ввод команды
                Console.WriteLine("\n-= Incorrect input. You must enter deck name =-\n");
                CheckMainInput();
            }

            //Если все введено верно
            else
            {                          
                //Создаем новую колоду
                if (command == "/create")
                {
                    //Запоминаем название колоды, введенное пользователем
                    selectedDeckName = userInput[1];
                    CreateNewDeck();
                }   

                //Выбираем существующую колоду
                else if (command == "/select")
                {          
                    //Запоминаем название колоды, введенное пользователем
                    selectedDeckName = userInput[1];
                    SelectDeck();
                }     

                //Показываем список всех колод
                else if (command == "/list")
                {
                    ShowDecks();
                    CheckMainInput();
                }   

                //Выход из программы
                else if (command == "close")
                {
                    return;
                }     
            }   
        }

        ///<Summary>
        ///Проверяет ввод в меню выбранной карты
        ///</Summary>
        static void CheckSelectedDeckInput()
        {
            //Выводим сообщение со списком команд
            Console.Write("\n-= Selected deck: " + selectedDeckName + " =-\n-= /show | /shuffle | /remove | /back =-\nEnter command: ");

            //Проверяем пользовательский ввод
            string[] userInput = Console.ReadLine().Split(' ');

            //Переводим все символы команды в нижний регистр
            string command = userInput[0].ToLower();
            
            //Если команда введена неверно
            if (userInput.Length > 2 ||
            (userInput.Length == 2 && command != "/shuffle") ||
            (userInput.Length == 1 && command != "/show" && command != "/shuffle" && command != "/remove" && command != "/back" ))
            {
                Console.WriteLine("\n-= Incorrect input =-");
                CheckSelectedDeckInput();
            }

            //Если ввод верный
            else
            {
                //Показываем колоду
                if (userInput[0] == "/show")
                {
                    ShowDeck();
                } 

                //Перемешиваем колоду  
                else if (userInput[0] == "/shuffle")
                {
                    if (userInput.Length > 1)
                    {
                        ShuffleDeck(userInput[1]);
                    }
                    else
                    {
                        ShuffleDeck(null);
                    }
                }               
                
                //Удаляем колоду
                else if (userInput[0] == "/remove")
                {
                    RemoveDeck();
                }  
               
                //Возвращаемся в главное меню
                else if (userInput[0] == "/back")
                {
                    CheckMainInput();
                }  
            }
        }
#endregion

#region DecksManagement
        ///<Summary>
        ///Создает новую колоду
        ///</Summary>
        static void CreateNewDeck()
        {
            //Проверяем, существует ли уже колода с таким именем
            if (decks.ContainsKey(selectedDeckName))
            {
                Console.WriteLine("\n-= Deck with this name already exists =-\n");
                selectedDeckName = null;
                CheckMainInput();
                return;  
            }

            //Стандартная колода, карты идут по порядку
            int[] newDeck = new int[cardCount];
            for (int i = 0; i < cardCount; i++)
            {
                newDeck[i] = i;
            }

            decks.Add(selectedDeckName, newDeck);               
            Save();  
            Console.WriteLine("\n-= Deck created =-\n"); 
            selectedDeckName = null;
            CheckMainInput();   
        }                                                

        ///<Summary>
        ///Выбирает колоду
        ///</Summary>
        static void SelectDeck()
        {
            //Проверяем, существует ли колода с таким названием
            if (!decks.ContainsKey(selectedDeckName))
            {
                Console.WriteLine("\n-= Deck with this name has not been found =-\n");
                selectedDeckName = null;
                CheckMainInput();
                return;
            }            

            //Переходим в меню выбора действий с колодой
            CheckSelectedDeckInput();
        }

        ///<Summary>
        ///Показывает все созданные пользователем колоды
        ///</Summary>
        static void ShowDecks()
        {
            //Колод нет, выходим
            if (decks.Count == 0)
            {
                Console.WriteLine("\n-= No decks created =-\n");
                CheckMainInput();
                return;
            }

            Console.WriteLine("\n-= Deck list =-");

            foreach (string deckName in decks.Keys)
            {
                Console.WriteLine("- " + deckName);
            }

            Console.Write("\n");

            //Возвращаемся в главное меню
            CheckMainInput();
        }
#endregion

#region SelectedDeckManagement
        ///<Summary>
        ///Удаляет выбранную колоду
        ///</Summary>
        static void RemoveDeck()
        {   
            decks.Remove(selectedDeckName);
            Console.WriteLine("\n-= Deck removed =-\n");
            selectedDeckName = null;
            Save();

            //Возвращаемся в главное меню
            CheckMainInput();                          
        }

        ///<Summary>
        ///Показывает выбранную колоду
        ///</Summary>
        static void ShowDeck()
        {           
            //Берем выбранную колоду
            int[] selectedDeck;
            decks.TryGetValue(selectedDeckName, out selectedDeck);

            string value = null;
            string suit = null;
            
            Console.WriteLine("\n-= Selected deck =-");

            //Показываем карту (значение + масть)
            for (int i = 0; i < cardCount; i++)
            {               
                if (suits.Count > 0)
                {
                    values.TryGetValue(selectedDeck[i] / suits.Count, out value);                
                    suits.TryGetValue(selectedDeck[i] % suits.Count, out suit);
                    Console.WriteLine("- " + value + " of " + suit);
                }
                else
                {
                    values.TryGetValue(selectedDeck[i], out value);
                    Console.WriteLine("- " + value);
                }                
            }

            //Возвращаемся в меню выбора действий с колодой
            CheckSelectedDeckInput();
        }

#region DeckShufflers
        ///<Summary>
        ///Размешивает выбранную колоду одним из двух методов (случайный | ручной)
        ///</Summary>
        static void ShuffleDeck(string mode)
        {
            if (cardCount >= 2)
            {
                if (mode == null)
                {
                    Console.WriteLine("\n-= Select shuffle mode: random | manual =-");
                    mode = Console.ReadLine().ToLower();
                }

                //Проверяем выбранный режим
                if (mode == "random")
                {
                    RandomDeckShuffle();
                    Console.WriteLine("\n-= Shuffle done =-");
                }
                else if (mode == "manual")
                {
                    ManualDeckShuffle();
                    Console.WriteLine("\n-= Shuffle done =-");
                }    
                else
                {
                    Console.WriteLine("\n-= Incorrect input =-\n");

                    //Возвращаемся в меню выбора действий с колодой
                    CheckSelectedDeckInput();  
                }
            }
            else
            {
                Console.WriteLine("\n-= Not enough cards to shuffle =-");
            }
            //Возвращаемся в меню выбора действий с колодой
            CheckSelectedDeckInput();       
        }

        ///<Summary>
        ///Случайное перемешивание колоды
        ///</Summary>
        static void RandomDeckShuffle()
        {
            //Берем выбранную колоду
            int[] selectedDeck;
            decks.TryGetValue(selectedDeckName, out selectedDeck);

            Random randomCounter = new Random();

            for (int i = 0; i < cardCount; i++)
            {
                int savedCardID = selectedDeck[i];
                int newPandomPosition = randomCounter.Next(0, cardCount);       
                selectedDeck[i] = selectedDeck[newPandomPosition];
                selectedDeck[newPandomPosition] = savedCardID;
            }
            Save();
        }

        ///<Summary>
        ///Эмуляция ручного перемешивания колоды
        ///</Summary>
        static void ManualDeckShuffle()
        {        
            Random randomCounter = new Random();

            //Берем выбранную колоду
            int[] selectedDeck;
            decks.TryGetValue(selectedDeckName, out selectedDeck);            

            //Сучайное количество перемешиваний от 50 до 100
            int shuffleCount = randomCounter.Next(50, 101);
            
            //Список для хранения четверти/половины колоды
            List<int> tempList;

            //Точки деления колоды
            int firstDiv = 0;   //Точка деления пополам
            int secondDiv = 0;  //Точка деления первой половины пополам

            //Погрешность деления колоды пополам (в одной из половин может оказаться до 10% карт больше)
            int divRandom;    
            int minRandValue = -(cardCount / 10);
            int maxRandValue = cardCount / 10 + 1;

                for (int j = 0; j < shuffleCount; j++)
                { 
                
                        divRandom = randomCounter.Next(minRandValue, maxRandValue);

                        //Разделение карт (половина колоды + погрешность деления)
                        firstDiv = cardCount / 2 + divRandom;
                            
                        if (firstDiv >= 2)
                        {
                            minRandValue = -(firstDiv / 10);
                            maxRandValue = firstDiv / 10 + 1;
                            divRandom = randomCounter.Next(minRandValue, maxRandValue);

                            //Разделение карт (половина колоды + погрешность деления)
                            secondDiv = firstDiv / 2 + divRandom;
                        }
                    

                    //Меняем местами первую и вторую четверти изначальной колоды
                    Shuffle(secondDiv, firstDiv);

                    //Меняем  местами первую и вторую половины измененной колоды
                    Shuffle(firstDiv, cardCount);                                         
                }
            
            //Алгоритм эмуляции ручного перемешивания (колода делится примерно пополам и части меняются местами)
            void Shuffle(int divPos, int border)
            {
                //Запоминаем значения первой четверти/половины
                tempList = new List<int>();
                for (int i = 0; i < divPos; i++)
                {
                    tempList.Add(selectedDeck[i]);
                }

                for (int i = 0; i < border; i++)
                {                          
                    int newPos = i + divPos;

                    //Если новая позиция карты лежит в пределах массива
                    if (newPos < border)
                    {
                        //Переносим карту в новую позицию
                        selectedDeck[i] = selectedDeck[newPos];
                    }
                    //Если новая позиция карты лежит за пределами массива
                    else if (newPos >= border)
                    {
                        newPos -= border;

                        //Берем значение из памяти
                        selectedDeck[i] = tempList[newPos];
                    }                       
                } 
            }  

            Save();
        }
#endregion
#endregion

#region SaveLoad
        ///<Summary>
        ///Загружает все сохранненые в файле колоды
        ///</Summary>
        static void Load()
        {          
            //Проверяем, существует ли файл
            if (File.Exists(saveFileName))
            {
                string jsonString = File.ReadAllText(saveFileName, System.Text.Encoding.Unicode);
                decks = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonString);
            }
            else
            {
                Console.WriteLine("-= Save file has not been found. New one has been created =-\n");
                decks = new Dictionary<string, int[]>(); 
                Save();
            }
        }

        ///<Summary>
        ///Сохраняет все колоды в файл
        ///</Summary>
        static void Save()
        {          
            string jsonString = JsonSerializer.Serialize(decks);
            File.WriteAllText(saveFileName, jsonString, System.Text.Encoding.Unicode);               
        }
#endregion
    }  
}
