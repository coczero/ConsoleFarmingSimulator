﻿using System;
using System.Timers;
using Polenter.Serialization;

namespace ConsoleFarmingSimulator
{
  class Program
  {
    private static Game _game = null;
    private static Shop _shop;
    private static Timer GlobalSeedTimer = new Timer(5000);
    private static SharpSerializer _serializer;

    /// <summary>
    /// Gets the current running game
    /// </summary>
    public static Game Game
    {
      get { return _game; }
      private set { _game = value; }
    }

    static void Main(string[] args)
    {
      Standards.InitializeStandards();
      _shop = new Shop();
      GlobalSeedTimer.Elapsed += new ElapsedEventHandler(GlobalSeedTimer_Tick);
      _serializer = new SharpSerializer();
      MainMenueDialog();
      MainDialog();
    }

    /// <summary>
    /// The main choice window
    /// </summary>
    private static void MainDialog()
    {
      GlobalSeedTimer.Start();

      string anweisung;
      while (true)
      {
        PrintInfoMessage("Where do you want to go? [fields; stall; shop; save; exit]");
        anweisung = Console.ReadLine();

        if (anweisung == "fields" || anweisung == "f")
        {
          FieldsDialog();
        }
        else if (anweisung == "stall" || anweisung == "st")
        {

        }
        else if (anweisung == "shop" || anweisung == "sh")
        {
          ShopDialog();
        }
        else if (anweisung == "save" || anweisung == "sa")
        {
          SaveGame();
        }
        else if (anweisung == "exit" || anweisung == "e")
        {
          Console.WriteLine("Hope you enyjoyed your stay! :)");
          Environment.Exit(0);
        }
      }
    }

    private static void SaveGame()
    {
      _serializer.Serialize(_game, "polenter.xml");
    }

    private static void LoadGame()
    {
      _game = (Game)_serializer.Deserialize("polenter.xml");
    }

    #region FieldDialogs
    private static void FieldsDialog()
    {
      string anweisung;

      while (true)
      {
        PrintInfoMessage("Which field do you want to manage? [enter index; exit]\r\n\r\n" + Game.GetFieldInfo());

        anweisung = Console.ReadLine();

        if (anweisung == "exit" || anweisung == "e")
          break;

        try
        {
          int index = int.Parse(anweisung);
          ManageFieldDialog(Game.Fields[index - 1]);
        }
        catch (Exception ex)
        {
          PrintInfoMessageAndWait(ex.Message);
        }
      }
    }

    private static void PlantSeedDialog(FieldSlot field)
    {
      string seedToPlant;
      while (true)
      {
        PrintInfoMessage("Which seed do you want to plant? [enter name and index; back]\r\n\r\n" + Game.GetSeedInventoryInfo());

        seedToPlant = Console.ReadLine();
        if (seedToPlant == "back" || seedToPlant == "b")
          break;

        Seed seed = GetSeedFromUserInput(seedToPlant);
        field.PlantSeed(seed);
        PrintInfoMessageAndWait("You planted a " + seed.Name + " seed! Take good care of it!");
        break;
      }
    }

    private static void RemoveSeedDialog(FieldSlot field)
    {
      string remove;
      while (true)
      {
        Console.Clear();
        DrawStatusBar();
        Seed plantedSeed = field.PlantedSeed;
        if (plantedSeed != null)
        {
          Console.WriteLine("Do you really want to remove the following seed? This kills the seed. [yes; no; more info]");
          plantedSeed.GetInfo();
          remove = Console.ReadLine();
          if (remove == "yes" || remove == "y")
          {
            field.RemoveSeed();
            PrintInfoMessageAndWait("Seed successfully removed.");
          }
          else if (remove == "no" || remove == "n")
            break;
          else if (remove == "more info" || remove == "more" || remove == "m")
            PrintInfoMessageAndWait("The seed has the following Crops:\r\n\r\n" + plantedSeed.GetCropInfo());
        }
      }
    }

    private static void WaterFieldDialog(FieldSlot field)
    {
      string waterCount;
      while (true)
      {
        Console.Clear();
        DrawStatusBar();
        Console.WriteLine("How many litres to you want to water? This field currently has " + field.Water + " litres of water in it. [number; back]");
        waterCount = Console.ReadLine();
        if (waterCount == "back" || waterCount == "b")
          break;

        try
        {
          int water = int.Parse(waterCount);
          double price = 0; //TODO: calculate water price based on weather, difficulty etc...

          string answer;
          while (true)
          {
            PrintInfoMessage("You are about to water your field with " + water + " litres. \r\nThat will cost " + price + "$. Do you want to continue? [yes; no]");
            answer = Console.ReadLine();

            if (answer == "yes" || answer == "y")
            {
              Game.Money -= price;
              field.Water += water;
              PrintInfoMessageAndWait("You watered your field with " + water + " litres.");
              break;
            }
            else if (answer == "no" || answer == "n")
              break;
          }

          //break; ?
        }
        catch
        {
          PrintInfoMessageAndWait("Thats not a valid number!");
        }
      }
    }

    /// <summary>
    /// Prints info about the crops of the planted seed of the given <paramref name="field"/> 
    /// and lets you select which crop to manage
    /// </summary>
    /// <param name="field">Field with the planted seed whose crops should be shown</param>
    private static void CropInfoDialog(FieldSlot field)
    {
      string anweisung;
      while (true)
      {
        PrintInfoMessage("Which crop do you want to manage? [enter index; back]\r\n\r\n" + field.PlantedSeed.GetCropInfo());

        anweisung = Console.ReadLine();
        if (anweisung == "back" || anweisung == "b")
          break;

        //TODO: implement manage crop
      }
    }

    #region Harvest Dialogs
    private static void HarvestFieldDialog(Seed seed)
    {
      string anweisung;
      while (true)
      {
        PrintInfoMessage("Which crops to you want to harvest [all; selection; back]\r\n\r\n" + seed.GetCropInfo());

        anweisung = Console.ReadLine();
        if (anweisung == "all" || anweisung == "a")
        {
          HarvestAllDialog(seed);
          break;
        }
        else if (anweisung == "selection" || anweisung == "s")
        {
          HarvestSelectionDialog(seed);
          if (seed.Crops.Count == 0)
            break;
        }
        else if (anweisung == "back" || anweisung == "b")
          break;

      }
    }

    private static void HarvestAllDialog(Seed seed)
    {
      string anweisung;
      while (true)
      {
        PrintInfoMessage("Do you really want to harvest all crops? They will be put in your inventory. [yes; no]\r\n\r\n" + seed.GetCropInfo());
        anweisung = Console.ReadLine();

        if (anweisung == "yes" || anweisung == "y")
        {
          seed.Harvest();
          PrintInfoMessageAndWait("Crops harvested! They have been placed in your inventory.");
          break;
        }
        else if (anweisung == "no" || anweisung == "n")
          break;
      }
    }

    private static void HarvestSelectionDialog(Seed seed)
    {
      string anweisung;
      while(true)
      {
        PrintInfoMessage("Which crop do you want to harvest? [enter comma seperated indexes; back]\r\n\r\n" + seed.GetCropInfo());

        anweisung = Console.ReadLine();
        if (anweisung == "back" || anweisung == "b")
          break;

        try
        {
          string[] indexStrings = anweisung.Split(',');
          int[] indexes = Array.ConvertAll(indexStrings, int.Parse);

          seed.Harvest(indexes);
          break;
        }
        catch(Exception ex)
        {
          PrintInfoMessageAndWait(ex.Message);
        }
      }
    }

    #endregion

    private static void ManageFieldDialog(FieldSlot field)
    {
      string anweisung;
      while (true)
      {
        PrintInfoMessage("What do you want to do? [plant seed; remove seed; water field; crop info; harvest; sell field; back]\r\n\r\n" + field.GetInfo());

        anweisung = Console.ReadLine();

        if (anweisung == "plant seed" || anweisung == "plant" || anweisung == "p")
        {
          PlantSeedDialog(field);
        }
        else if (anweisung == "remove seed" || anweisung == "remove" || anweisung == "r")
        {
          RemoveSeedDialog(field);
        }
        else if (anweisung == "water field" || anweisung == "water" || anweisung == "w")
        {
          WaterFieldDialog(field);
        }
        else if (anweisung == "crop info" || anweisung == "c")
        {
          CropInfoDialog(field);
        }
        else if (anweisung == "harvest" || anweisung == "h")
        {
          HarvestFieldDialog(field.PlantedSeed);
        }
        else if (anweisung == "sell field" || anweisung == "sell" || anweisung == "s")
        {

        }
        else if (anweisung == "back" || anweisung == "b")
          break;
      }
    }
    #endregion

    #region ShopDialogs
    static void ShopDialog()
    {
      string anweisung;
      while (true)
      {
        Console.Clear();
        DrawStatusBar();
        Console.WriteLine("Welcome to the shop! What do you want? [buy; sell; exit]");
        anweisung = Console.ReadLine();

        if (anweisung == "buy" || anweisung == "b")
        {
          ShopBuyDialog();
        }
        else if (anweisung == "sell" || anweisung == "s")
        {
          ShopSellDialog();
        }
        else if (anweisung == "exit" || anweisung == "e")
        {
          break;
        }

        Console.Clear();
      }
    }

    private static void ShopSellDialog()
    {
      string anweisung;
      while (true)
      {
        Console.Clear();
        DrawStatusBar();
        Console.WriteLine("What do you want to sell? [enter name + index; back]");
        Game.GetSeedInventoryInfo();
        anweisung = Console.ReadLine();

        if (anweisung == "back" || anweisung == "b")
          break;
        else
        {
          try
          {
            Seed seedToSell = GetSeedFromUserInput(anweisung);
            string sellSeed;
            double price = seedToSell.CalculatePrice();
            while (true)
            {
              PrintInfoMessage("Do you really want to sell the following seed? It will give you " + price + "$ [yes; no]");
              seedToSell.GetInfo();
              sellSeed = Console.ReadLine();

              if (sellSeed == "yes" || sellSeed == "y")
              {
                Game.RemoveSeedFromInventory(seedToSell);
                Game.Money += price;
                PrintInfoMessageAndWait("You sold a " + seedToSell.Name + " seed for " + price + "$");
                break;
              }
              else if (sellSeed == "no" || sellSeed == "n")
                break;
            }
          }
          catch (Exception ex)
          {
            PrintInfoMessageAndWait("You dont have that seed!");
          }
        }
      }
    }

    static void ShopBuyDialog()
    {
      while (true)
      {
        PrintInfoMessage("What do you want to buy? [enter name; back]\r\n\r\n" + _shop.GetSoldSeedInfo());

        string kaufAnweisung = Console.ReadLine();
        if (kaufAnweisung == "back" || kaufAnweisung == "b")
          break;

        try
        {
          _shop.BuyItem(kaufAnweisung);
          PrintInfoMessageAndWait("You bought a " + kaufAnweisung + "! It has been placed in your inventory.");
          break;
        }
        catch (Exception ex)
        {
          PrintInfoMessageAndWait(ex.Message);
        }
      }
    }
    #endregion

    #region MainMenueDialogs
    static void MainMenueDialog()
    {
      string anweisung = "";
      while (true)
      {
        Console.WriteLine("Welcome to the 'Console Farming Simulator'! What do you want to do? [new game; continue; exit]");
        anweisung = Console.ReadLine();

        if (anweisung == "new game" || anweisung == "n")
        {
          NewGameDialog();
          break;
        }
        else if (anweisung == "continue" || anweisung == "c")
        {
          LoadGame();
          break;
        }
        else if (anweisung == "exit" || anweisung == "e")
        {
          Environment.Exit(0);
        }
        else
          Console.Clear();
      }
    }

    private static void NewGameDialog()
    {
      Console.WriteLine("What's the name of your farm?");
      string name = Console.ReadLine();
      Enumerations.Difficulty difficulty;

      while (true)
      {
        Console.Clear();
        Console.WriteLine("On what difficulty do you want to play? [easy; medium; hard]");
        string diff = Console.ReadLine();
        try
        {
          difficulty = (Enumerations.Difficulty)Enum.Parse(typeof(Enumerations.Difficulty), diff);
          break;
        }
        catch (Exception ex)
        {
          //Dont use PrintInfoMessage here because _game is not yet initialized.
          Console.Clear();
          Console.WriteLine("Thats not a valid difficulty!");
          Console.ReadKey();
        }
      }

      Game = new Game(name, difficulty);
      Console.WriteLine("Alright, all set up! Enjoy your farm!");
      Console.ReadKey();
      Console.Clear();
    }
    #endregion

    /// <summary>
    /// Draw the status bar
    /// </summary>
    private static void DrawStatusBar()
    {
      int oldTop = Console.CursorTop;
      int oldLeft = Console.CursorLeft;
      Console.SetCursorPosition(0, 0);

      //TODO: figure out what the fuck I did here.
      int length = Game.FarmName.Length + 3 + Game.Difficulty.ToString().Length + 3 + Game.Money.ToString().Length + 3 + Game.Level.ToString().Length + 3 + Game.XP.ToString().Length + 1 + Game.MaxXP.ToString().Length + 10 + Game.Day.ToString().Length + 9 + 41;
      if (length > Console.BufferWidth)
      {
        Console.SetBufferSize(length, 30);
        Console.SetWindowSize(length, 30);
      }

      Console.ForegroundColor = ConsoleColor.Red;
      for (int i = 0; i < length; i++)
      {
        Console.Write("|");
      }

      Console.WriteLine();
      Console.Write("|||||");
      Console.ResetColor();
      Console.Write("Farm Name: " + Game.FarmName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(" | ");
      Console.ResetColor();
      Console.Write("Difficulty: " + Game.Difficulty);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(" | ");
      Console.ResetColor();
      Console.Write("Money: " + Game.Money);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(" | ");
      Console.ResetColor();
      Console.Write("Level: " + Game.Level);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(" | ");
      Console.ResetColor();
      Console.Write("XP: " + Game.XP + "/" + Game.MaxXP);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write(" | ");
      Console.ResetColor();
      Console.Write("Days: " + Game.Day);
      Console.SetCursorPosition(length - 5, 1);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("|||||");
      Console.WriteLine();
      for (int i = 0; i < length; i++)
      {
        Console.Write("|");
      }
      Console.WriteLine();
      Console.ResetColor();
      if (oldTop != 0)
        Console.SetCursorPosition(oldLeft, oldTop);
      else
        Console.SetCursorPosition(oldLeft, 4);
    }

    /// <summary>
    /// Lets all planted seeds make their grow procedure
    /// </summary>
    private static void GlobalSeedTimer_Tick(object sender, ElapsedEventArgs e)
    {
      Game.Day++;
      Game.CurrentWeather.CalculateWeather();
      DrawStatusBar();

      foreach (FieldSlot field in _game.Fields)
      {
        if (field.PlantedSeed != null)
          field.PlantedSeed.Grow();
      }
    }

    /// <summary>
    /// Clears the console, prints the status bar and message and waits for a keystroke
    /// </summary>
    /// <param name="message">Message to print after the status bar</param>
    private static void PrintInfoMessageAndWait(string message)
    {
      Console.Clear();
      DrawStatusBar();
      Console.WriteLine(message);
      Console.ReadKey();
    }

    /// <summary>
    /// Clears the console, prints the status bar and message
    /// </summary>
    /// <param name="message">Message to print after the status bar</param>
    private static void PrintInfoMessage(string message)
    {
      Console.Clear();
      DrawStatusBar();
      Console.WriteLine(message);
    }

    /// <summary>
    /// Convert plant input string eg "Cucumber 1" into the corresponding seed from the seed inventory
    /// </summary>
    /// <param name="userInput">Input string</param>
    /// <returns>Seed from seed inventory</returns>
    private static Seed GetSeedFromUserInput(string userInput)
    {
      int index = int.Parse(userInput[userInput.Length - 1].ToString());
      string name = userInput.Substring(0, userInput.Length - 2);
      return Game.SeedInventory[name][index - 1];
    }
  }
}