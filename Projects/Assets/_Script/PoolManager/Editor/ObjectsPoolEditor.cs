using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UFramework.Pool
{
    [CustomEditor(typeof(ObjectsPool))]
    public class ObjectsPoolEditor : UnityEditor.Editor
    {
        private ObjectsPool poolManager
        {
            get { return (ObjectsPool) target; }
        }

        private GUIStyle background;
        private GUIStyle poolBackground;
        private GUIStyle dropBox;

        private string searchStr = "";

        private void OnEnable()
        {
            background = new GUIStyle();
            poolBackground = new GUIStyle();
            dropBox = new GUIStyle();

            background.normal.background = MakeTex(new Color(0.5f, 0.5f, 0.5f, 0.5f));
            poolBackground.normal.background = MakeTex(new Color(0, 0, 0, 0.5f));
            dropBox.normal.background = MakeTex(Color.grey);
            dropBox.hover.background = MakeTex(new Color(1, 1, 1, 0.5f));

            poolBackground.margin = new RectOffset(2, 2, 2, 2);
            dropBox.margin = new RectOffset(4, 4, 4, 4);

            dropBox.alignment = TextAnchor.MiddleCenter;

            dropBox.fontSize = 14;

            dropBox.normal.textColor = Color.black;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(poolManager, "poolmanager");

            PoolManagerSettings();
            Toolbar();

            EditorUtility.SetDirty(poolManager);
        }

        private void PoolManagerSettings()
        {
            GUILayout.BeginHorizontal();

            poolManager.debugMessages = EditorGUILayout.Toggle("Show debug ", poolManager.debugMessages);
            poolManager.spawnDespawnMessages =
                EditorGUILayout.Toggle("Send messages ", poolManager.spawnDespawnMessages);

            GUILayout.EndHorizontal();
        }

        private void Toolbar()
        {
            GUILayout.Space(10f);
            DropArea();
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"), GUILayout.ExpandWidth(true));
            GUILayout.Label("Pools (" + poolManager.pools.Count + ")");
            SearchField();

            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(65)))
                poolManager.pools.ForEach(pool => pool.foldout = true);

            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(71)))
                poolManager.pools.ForEach(pool => pool.foldout = false);

            GUILayout.EndHorizontal();
            GUILayout.BeginVertical(background);

            var results = searchStr == ""
                ? poolManager.pools
                : poolManager.pools.Where(pool => pool.Prefab.name.Contains(searchStr)).ToList();

            for (int i = 0; i < results.Count; i++)
            {
                Pool pool = results[i];

                GUILayout.BeginHorizontal();
                GUILayout.Space(5f);
                GUILayout.BeginVertical();
                PoolArea(pool);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void PoolArea(Pool pool)
        {
            GUILayout.BeginVertical(poolBackground);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10f);

            pool.foldout = EditorGUILayout.Foldout(pool.foldout, pool.PoolName);
            GUILayout.FlexibleSpace();

            if (Application.isPlaying)
                GUILayout.Label("Spawned: " + pool.SpawnedCount + "/" + pool.TotalCount);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
                pool.ClearAndDestroy();

            if (GUILayout.Button("Preinstantiate", EditorStyles.toolbarButton, GUILayout.Width(80)))
                pool.PreInstantiate();

            GUI.color = Color.red;
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(15)))
            {
                pool.ClearAndDestroy();
                if (pool.m_Root != null)
                    GameObject.DestroyImmediate(pool.m_Root.gameObject);
                poolManager.pools.Remove(pool);
            }

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (pool.foldout)
            {
                pool.Prefab =
                    EditorGUILayout.ObjectField("Prefab: ", pool.Prefab, typeof(GameObject), false) as GameObject;
                pool.size = EditorGUILayout.IntField("Pool size: ", pool.size);
                pool.allowMore = EditorGUILayout.Toggle("Allow more: ", pool.allowMore);
                pool.debugMessages = EditorGUILayout.Toggle("Debug messages: ", pool.debugMessages);
                pool.mode = (DespawnMode) EditorGUILayout.EnumPopup("DespawnMode", pool.mode);
                if (pool.mode == DespawnMode.Move)
                {
                    pool.despawnPos = EditorGUILayout.Vector3Field("    MovePosisition", pool.despawnPos);
                }
            }

            GUILayout.EndVertical();
        }

        private void DropArea()
        {
            GUILayout.Box("拖拽预制体到此", dropBox, GUILayout.ExpandWidth(true), GUILayout.Height(60));

            UnityEngine.EventType eventType = UnityEngine.Event.current.type;

            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                UnityEngine.Event.current.Use();
            }

            if (isAccepted)
            {
                var pools = DragAndDrop.objectReferences
                    .Where(obj => obj.GetType() == typeof(GameObject))
                    .Cast<GameObject>()
                    .Where(obj => PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                    .Except(poolManager.Prefabs)
                    .Select(obj => new Pool(obj));

                poolManager.pools.AddRange(pools);
            }
        }

        private void SearchField()
        {
            searchStr = GUILayout.TextField(searchStr, GUI.skin.FindStyle("ToolbarSeachTextField"),
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                searchStr = "";
                GUI.FocusControl(null);
            }
        }

        private Texture2D MakeTex(Color col)
        {
            Color[] pix = new Color[1 * 1];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            result.hideFlags = HideFlags.HideAndDontSave;
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}