using RedBjorn.SuperTiles.Editors.Level.Submenus.Tabs.BattleFinish;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors.Level.Submenus.Tabs
{
    public class BattleFinishTab : ITab
    {
        public IBattleFinishSubmenu Submenu;

        public BattleFinishTab(LevelWindow window)
        {
            Submenu = new Level.Submenus.Tabs.BattleFinish.Edit();
        }

        public void Draw(LevelWindow window)
        {
            var common = window.Submenu.GetType() == typeof(Edit);
            if (window.Level && common)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = false;
                window.CachedBattleFinish = EditorGUILayout.ObjectField("Battle Finish", window.CachedBattleFinish, typeof(BattleFinishHandler), allowSceneObjects: false) as BattleFinishHandler;
                GUI.enabled = true;
                Submenu.Draw(this, window);
            }
        }
    }
}
