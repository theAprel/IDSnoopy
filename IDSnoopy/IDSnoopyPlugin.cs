using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IDSnoopy
{
    public class IDSnoopyPlugin : IPlugin
    {

        public string Author
        {
            get { return "Aprel"; }
        }

        public string ButtonText
        {
            get { return "Settings"; }
        }

        public string Description
        {
            get { return "Keeps track of card ids and exposes cards whose ids reveal them."; }
        }

        public MenuItem MenuItem
        {
            get { return null; }
        }

        public string Name
        {
            get { return "IDSnoopy"; }
        }

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
            IDSnoopy.Load();
        }

        public void OnUnload()
        {
        }

        public void OnUpdate()
        {
        }

        public Version Version
        {
            get { return new Version(0, 0, 1); }
        }
    }
}
