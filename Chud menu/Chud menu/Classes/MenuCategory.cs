using System;
using System.Collections.Generic;

namespace MalachiTemp.Classes
{
    public class ButtonInfo
    {
        public string buttonText = "Error";
        public Action method = null;
        public Action disableMethod = null;
        public bool? enabled = false;
        public bool? nontoggleable = false;
        public string toolTip = "This button doesn't have a tooltip/tutorial";
    }

    public class MenuCategory
    {
        public string Name;
        public List<ButtonInfo> Buttons = new List<ButtonInfo>();

        public MenuCategory(string name)
        {
            Name = name;
        }

        public MenuCategory(string name, List<ButtonInfo> buttons)
        {
            Name = name;
            Buttons = buttons;
        }
    }
}
