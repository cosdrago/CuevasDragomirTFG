using RedBjorn.SuperTiles.Editors.Unit;
using RedBjorn.Utils;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors
{
    public class UnitWindow : EditorWindowExtended
    {
        public SerializedObject SerializedObject;
        public SerializedObject SerializedTurn;
        public TurnResolver CachedTurn;
        public ISubmenu Submenu;
        public UnitWindowSettings Settings;

        public bool Dirty;

        public string CachedRootFolderPath;
        public string CachedUnitName;

        public Vector2 StatsScroll;
        public Vector2 ItemsScroll;

        float CommonHeight = 60;
        float MenuWidth = 150;
        float Border = 4;

        public float WindowWidth;
        public float WindowHeight;

        Rect Common;
        Rect CommonContent;
        Rect Menu;
        Rect MenuContent;
        Rect WorkArea;
        Rect WorkAreaContent;

        Color CommonColor => Settings.CommonColor;
        Color MenuColor => Settings.MenuColor;
        Color WorkAreaColor => Settings.WorkAreaColor;

        static readonly string[] Tabs = new string[]
        {
            "Visual",
            nameof(UnitData.Stats),
            nameof(UnitData.DefaultItems),
        };

        [SerializeField]
        UnitData CachedUnit;
        public UnitData Unit
        {
            get
            {
                return CachedUnit;
            }
            set
            {
                if (CachedUnit != value)
                {
                    CachedUnit = value;
                    OnChangedItem();
                }
            }
        }

        [SerializeField]
        int CachedTab;
        public int Tab
        {
            get
            {
                return CachedTab;
            }
            set
            {
                CachedTab = value;
            }
        }

        [MenuItem("Tools/Red Bjorn/Editors/Unit", priority = 225)]
        public static void DoShow()
        {
            DoShow(null);
        }

        public static void DoShow(UnitData unit)
        {
            var window = (UnitWindow)EditorWindow.GetWindow(typeof(UnitWindow));
            window.minSize = new Vector2(400f, 500f);
            window.Unit = unit;
            window.titleContent = new GUIContent("Unit Editor");
            if (window.Unit)
            {
                window.CachedRootFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(window.Unit));
            }
            window.Show();
        }

        void OnEnable()
        {
            Settings = UnitWindowSettings.Instance;
            Submenu = new EditSubmenu();
            OnChangedItem();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        void OnGUI()
        {
            var scale = EditorGUIUtility.pixelsPerPoint;
            WindowWidth = Screen.width / scale;
            WindowHeight = Screen.height / scale;

            Undo.RecordObject(this, "Unit");
            var label = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90f;
            Common.x = 2 * Border;
            Common.y = 2 * Border;
            Common.width = WindowWidth - 4 * Border;
            Common.height = CommonHeight;
            EditorGUI.DrawRect(Common, CommonColor);
            CommonContent.x = Common.x + 2 * Border;
            CommonContent.y = Common.y + 2 * Border;
            CommonContent.width = Common.width - 4 * Border;
            CommonContent.height = Common.height - 4 * Border;

            GUILayout.BeginArea(CommonContent);
            Unit = EditorGUILayout.ObjectField("Unit Asset", Unit, typeof(UnitData), allowSceneObjects: false) as UnitData;
            Submenu.DrawCommon(this);
            GUILayout.EndArea();

            float x = 0;
            float y = 0;
            float menuWidth = 0;
            float menuheight = 0;
            if (Unit)
            {
                if (Submenu.HasMenu())
                {
                    x = Common.x;
                    y = Common.y + Common.height + 2 * Border;
                    menuWidth = MenuWidth;
                    menuheight = WindowHeight - Common.y - Common.height - 10 * Border;
                }
            }
            Menu.x = x;
            Menu.y = y;
            Menu.width = menuWidth;
            Menu.height = menuheight;
            EditorGUI.DrawRect(Menu, MenuColor);
            MenuContent.x = Menu.x + 2 * Border;
            MenuContent.y = Menu.y + 2 * Border;
            MenuContent.width = Menu.width - 4 * Border;
            MenuContent.height = Menu.height - 4 * Border;

            GUILayout.BeginArea(MenuContent);
            Tab = GUILayout.SelectionGrid(Tab, Tabs, 1);
            GUILayout.EndArea();

            WorkArea.x = Menu.x + Menu.width + 2 * Border;
            WorkArea.y = Common.y + Common.height + 2 * Border;
            WorkArea.width = WindowWidth - Menu.width - Menu.x - 4 * Border;
            WorkArea.height = WindowHeight - Common.y - Common.height - 10 * Border;
            EditorGUI.DrawRect(WorkArea, WorkAreaColor);
            WorkAreaContent.x = WorkArea.x + 2 * Border;
            WorkAreaContent.y = WorkArea.y + 2 * Border;
            WorkAreaContent.width = WorkArea.width - 4 * Border;
            WorkAreaContent.height = WorkArea.height - 4 * Border;

            GUILayout.BeginArea(WorkAreaContent);
            Submenu.DrawWork(this);
            GUILayout.EndArea();

            if (SerializedObject != null)
            {
                if (!SerializedObject.targetObject)
                {
                    DefaultValues();
                }
                if (SerializedObject != null)
                {
                    SerializedObject.ApplyModifiedProperties();
                }
            }

            if (Dirty)
            {
                Dirty = false;
                Repaint();
            }
            EditorGUIUtility.labelWidth = label;
        }

        void OnUndoRedoPerformed()
        {
            Repaint();
        }

        void OnChangedItem()
        {
            if (Unit)
            {
                SerializedObject = new SerializedObject(Unit);
            }
            else
            {
                DefaultValues();
            }
        }

        void DefaultValues()
        {
            SerializedObject = null;
            CachedUnit = null;
        }
    }
}
