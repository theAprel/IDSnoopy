using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace IDSnoopy
{
    // Largely copied from example plugin framework: https://github.com/andburn/hdt-plugin-example/blob/master/PluginExample/MyCode.cs
    class IDSnoopy
    {
        private static HearthstoneTextBlock _info;
        private static Dictionary<int, string> knownEntities = new Dictionary<int, string>();
        // Rafaam fields
        private static bool rafaamLogFlag = false;
        private static int rafaamLogLineCount = 0;
        private static int[] artifactIds;

        private static IGame game
        {
            get
            {
                return Hearthstone_Deck_Tracker.API.Core.Game;
            }
        }

        private static Entity[] Entities
        {
            // Get the Game.Entities
            get
            {
                return Helper.DeepClone<Dictionary<int, Entity>>(
                    Hearthstone_Deck_Tracker.API.Core.Game.Entities).Values.ToArray<Entity>();
            }
        }

        public static void Load()
        {
            // A border to put around the text block
            Border blockBorder = new Border();
            blockBorder.BorderBrush = Brushes.Black;
            blockBorder.BorderThickness = new Thickness(1.0);
            blockBorder.Padding = new Thickness(8.0);

            // A text block using the HS font
            _info = new HearthstoneTextBlock();
            _info.Text = "";
            _info.FontSize = 14;

            // Add the text block as a child of the border element
            blockBorder.Child = _info;

            // Get the HDT Overlay canvas object
            var canvas = Hearthstone_Deck_Tracker.API.Core.OverlayCanvas;
            // Get canvas centre
            var fromTop = canvas.Height / 2 + 50;
            var fromLeft = canvas.Width / 2;
            // Give the text block its position within the canvas, roughly in the center
            Canvas.SetTop(blockBorder, fromTop);
            Canvas.SetLeft(blockBorder, fromLeft);
            // Add the text block and image to the canvas
            canvas.Children.Add(blockBorder);

            // Register methods to be called when GameEvents occur
            
            GameEvents.OnGameStart.Add(newGame);
            GameEvents.OnOpponentCreateInDeck.Add(scanEntities);
            GameEvents.OnOpponentCreateInPlay.Add(scanEntities);
            GameEvents.OnOpponentDeckDiscard.Add(scanEntities);
            GameEvents.OnOpponentDeckToPlay.Add(scanEntities);
            GameEvents.OnOpponentDraw.Add(scanEntities);
            GameEvents.OnOpponentFatigue.Add(scanEntities);
            GameEvents.OnOpponentGet.Add(scanEntities);
            GameEvents.OnOpponentHandDiscard.Add(scanEntities);
            GameEvents.OnOpponentJoustReveal.Add(scanEntities);
            GameEvents.OnOpponentPlay.Add(scanEntities);
            GameEvents.OnOpponentPlayToDeck.Add(scanEntities);
            GameEvents.OnOpponentPlayToHand.Add(scanEntities);
            GameEvents.OnTurnStart.Add(scanEntities);
            GameEvents.OnTurnStart.Add(HandInfo);

            GameEvents.OnOpponentDraw.Add(HandInfo);

            LogEvents.OnPowerLogLine.Add(TrackRafaam);

        }

        public static void newGame()
        {
            knownEntities = new Dictionary<int, string>();
            _info.FontSize = 14;
        }

        public static void scanEntities(Card c)
        {
            scanEntities();
        }

        public static void scanEntities(int i)
        {
            scanEntities();
        }

        public static void scanEntities(ActivePlayer ap)
        {
            scanEntities();
        }

        public static void scanEntities()
        {
            foreach (var e in Entities)
            {
                if (!e.Card.Name.Equals("UNKNOWN", StringComparison.InvariantCultureIgnoreCase) && !knownEntities.ContainsKey(e.Id))
                {
                    knownEntities.Add(e.Id, e.Card.Name);
                    Logger.WriteLine("Identified a card! " + e.Id + "=" + e);
                }
            }
        }

        // Track opponent's hand
        public static void HandInfo()
        {
            _info.Text = "";

            List<CardEntity> opponentsHand = new List<CardEntity>(game.Opponent.Hand);
            opponentsHand.Reverse();    // reverse to last drawn last

            foreach (var cardEntity in opponentsHand)
            {
                var e = cardEntity.Entity;
                string value = "";
                if (knownEntities.TryGetValue(e.Id, out value))
                {
                    _info.FontSize = 20;  // flash, change color, do something better
                }
                _info.Text += e.Id + ": " + value + "\n";
            }
        }

        public static void HandInfo(ActivePlayer ap)
        {
            if (ap == ActivePlayer.Player)  // Only necessary on player's turn (for Rafaam artifact) because on opponent's, check is after draw
                HandInfo();
        }

        public static void TrackRafaam(string logLine)
        {
            if(logLine.Contains("Source=[name=Arch-Thief Rafaam"))  // identify target log line and prepare
            {
                rafaamLogFlag = true;
                rafaamLogLineCount = 0;
                artifactIds = new int[3];
                return;
            }
            if(rafaamLogFlag)  // the next 3 lines in the log are the artifact cards
            {
                int id = Int32.Parse(GetValueInLogEntry(logLine, "id"));
                artifactIds[rafaamLogLineCount++] = id;
                if(rafaamLogLineCount == 3)  // all ids have been identified; process and clean up
                {
                    Array.Sort(artifactIds);
                    knownEntities.Add(artifactIds[0], "Lantern of Power");
                    knownEntities.Add(artifactIds[1], "Mirror of Doom");
                    knownEntities.Add(artifactIds[2], "Timepiece of Horror");

                    rafaamLogFlag = false;
                }
            }
        }

        // maybe refactor this into a library. Copied and pasted from OptionsDisplay
        private static String GetValueInLogEntry(String entry, String key)
        {
            int keyIndex = entry.IndexOf(key);
            String afterEqualsSign = entry.Substring(keyIndex + key.Length + 1); // +1 is the equals sign
            if (!afterEqualsSign.Contains("="))
            {
                return afterEqualsSign; // no more key-value pairs on log line; return this last value
            }
            int nextEquals = afterEqualsSign.IndexOf("=");
            String valueAndNextKey = afterEqualsSign.Substring(0, nextEquals);
            int lastSpaceIndex = valueAndNextKey.LastIndexOf(" ");
            return valueAndNextKey.Substring(0, lastSpaceIndex);
        }
    }
}
