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
            _info.FontSize = 18;

            // Add the text block as a child of the border element
            blockBorder.Child = _info;

            // Get the HDT Overlay canvas object
            var canvas = Hearthstone_Deck_Tracker.API.Core.OverlayCanvas;
            // Get canvas centre
            var fromTop = canvas.Height / 2;
            var fromLeft = canvas.Width / 2;
            // Give the text block its position within the canvas, roughly in the center
            Canvas.SetTop(blockBorder, fromTop);
            Canvas.SetLeft(blockBorder, fromLeft);
            // Add the text block and image to the canvas
            canvas.Children.Add(blockBorder);

            // Register methods to be called when GameEvents occur
            GameEvents.OnOpponentDraw.Add(HandInfo);
        }

        // Track opponent's hand
        public static void HandInfo()
        {
            _info.Text = "";


            foreach (var e in Entities)
            {
                if (e.IsInHand && e.GetTag(GAME_TAG.CONTROLLER) == game.OpponentEntity.GetTag(GAME_TAG.CONTROLLER))
                    _info.Text += e.Id + ": " + e.Card + "\n";
            }
        }
    }
}
