using MalachiTemp.Classes;
using MalachiTemp.UI;
using System.Collections.Generic;
using System.Linq;

namespace MalachiTemp.Backend
{
    public static class MenuManager
    {
        public static List<MenuCategory> Categories = new List<MenuCategory>();
        public static string CurrentCategoryName = "Main";

        public static MenuCategory CurrentCategory =>
            Categories.Find(c => c.Name == CurrentCategoryName);

        public static List<ButtonInfo> CurrentButtons =>
            CurrentCategory?.Buttons;

        public static void AddCategory(string name, List<ButtonInfo> buttons = null)
        {
            if (Categories.Any(c => c.Name == name)) return;
            Categories.Add(new MenuCategory(name, buttons ?? new List<ButtonInfo>()));
        }

        public static void RemoveCategory(string name)
        {
            if (name == "Main") return;
            if (CurrentCategoryName == name)
                CurrentCategoryName = "Main";
            Categories.RemoveAll(c => c.Name == name);
        }

        public static void AddMod(string categoryName, ButtonInfo button)
        {
            var cat = Categories.Find(c => c.Name == categoryName);
            cat?.Buttons.Add(button);
        }

        public static void RemoveMod(string categoryName, string buttonText)
        {
            var cat = Categories.Find(c => c.Name == categoryName);
            cat?.Buttons.RemoveAll(b => b.buttonText == buttonText);
        }

        public static ButtonInfo FindButton(string categoryName, string buttonText)
        {
            var cat = Categories.Find(c => c.Name == categoryName);
            return cat?.Buttons.Find(b => b.buttonText == buttonText);
        }

        public static bool IsInCategory(string name)
        {
            return CurrentCategoryName == name;
        }

        public static void ToggleCategory(string name)
        {
            if (CurrentCategoryName == name)
            {
                CurrentCategoryName = "Main";
            }
            else
            {
                CurrentCategoryName = name;
            }
            WristMenu.pageNumber = 0;
            WristMenu.DestroyMenu();
            if (WristMenu.instance != null)
                WristMenu.instance.Draw();
        }
    }
}
