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
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace IDSnoopy
{
    // Largely copied from example plugin framework: https://github.com/andburn/hdt-plugin-example/blob/master/PluginExample/MyCode.cs
    class IDSnoopy
    {
        private static HearthstoneTextBlock _info;
        private static Dictionary<int, Entity> knownEntities = new Dictionary<int, Entity>();
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
            _info.TextWrapping = TextWrapping.WrapWithOverflow;
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

            GameEvents.OnOpponentDraw.Add(HandInfo);

        }

        public static void newGame()
        {
            knownEntities = new Dictionary<int, Entity>();
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
                    knownEntities.Add(e.Id, e);
                    Log.WriteLine("Identified a card! " + e.Id + "=" + e, LogType.Info);
                }
            }
        }

        // Track opponent's hand
        public static void HandInfo()
        {
            _info.Text = "";

            List<Entity> opponentsHand = new List<Entity>(game.Opponent.Hand);

            foreach (var cardEntity in opponentsHand)
            {
                var e = cardEntity;
                Entity value;
                if (knownEntities.TryGetValue(e.Id, out value))
                {
                    _info.FontSize = 20;  // flash, change color, do something better
                }
                _info.Text += e.Id + ": " + (value != null ? value.Card.Name : "") + "\n";
            }
        }
    }
}
