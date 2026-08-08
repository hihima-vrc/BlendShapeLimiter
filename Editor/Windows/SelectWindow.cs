using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

public class SelectWindow : EditorWindow
{
    private AvatarEditorContext _context = AvatarEditorContext.Instance;
    private BlendShapeOrchestrator _orchestrator = BlendShapeOrchestrator.Instance;

    private VRC_AvatarDescriptor avatar;
    private SkinnedMeshRenderer body;
    private bool isParentHasChild = true;
    private Vector2 scrollPosition;
    private float buttonHeight = 30f;

    public static SelectWindow ShowWindow()
    {
        var window = GetWindow<SelectWindow>("Select BlendShape");
        return window;
    }

    private void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(_context.IsConverting);
        EditorGUI.BeginChangeCheck();
        avatar = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRC_AvatarDescriptor), true);
        if (EditorGUI.EndChangeCheck())
        {
            if (avatar != null)
            {
                _context.SetAvatar(Instantiate(avatar.gameObject));
                avatar.gameObject.SetActive(false);
            }
            else
            {
                _context.DeleteAll();
                body = null;
            }
        }

        EditorGUI.BeginDisabledGroup(avatar == null);
        EditorGUI.BeginChangeCheck();
        body = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Body", body, typeof(SkinnedMeshRenderer), true);
        if (avatar != null && body == null)
        {
            EditorGUILayout.HelpBox("今無効化されたアバターからSkinnedMeshRendererをD&D", MessageType.Info);
            EditorGUILayout.HelpBox("もし間違ったアバターを入れた場合は、一度ウィンドウを閉じてください", MessageType.Warning);
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (body != null)
            {
                isParentHasChild = _orchestrator.SetBody(body.name);
            }
            else
            {
                isParentHasChild = true;
                _context.DeleteBody();
            }
        }
        if (!isParentHasChild)
        {
            EditorGUILayout.HelpBox("このBody(もしくはゲームオブジェクト)はアバターの子に含まれていません。", MessageType.Error);
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);
        // ここからメイン
        if (_context.Body != null && _context.BlendShapeNames != null && _context.BlendShapeOriginalWeights != null && _context.BlendShapeOverrideWeights != null)
            buttonHeight = EditorGUILayout.Slider("ボタンの高さ", buttonHeight, 10, 70);
        {
            // 毎フレームOriginalWeightに戻す
            for (int i = 0; i < _context.BlendShapeNames.Count; i++)
            {
                _context.UpdateBodyBlendShapeWeight(i, _context.BlendShapeOriginalWeights[i]);
            }

            // ボタンを配置、マウスオーバーしている物は、OverrideWeightを適用
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < _context.BlendShapeNames.Count; i++)
            {
                if (_context.BlendShapeOverrideWeights[i] != 100f)
                {
                    GUI.color = Color.green;
                }
                bool clicked = GUILayout.Button(_context.BlendShapeNames[i], GUILayout.Height(buttonHeight));
                GUI.color = Color.white;
                if (clicked)
                {
                    _context.UpdateSelectedBlendShapeIndex(i);
                    _context.UpdateIsOpeningSettingWindow(true);
                    _orchestrator.OpenSettingWindow();
                }

                Rect rect = GUILayoutUtility.GetLastRect();

                if (rect.Contains(Event.current.mousePosition) && !_context.IsOpeningSettingWindow)
                {
                    _context.UpdateBodyBlendShapeWeight(i, _context.BlendShapeOverrideWeights[i]);
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(20);
            if (GUILayout.Button("Convert", GUILayout.Height(20)) && _context.Body != null)
            {
                _context.UpdateIsConverting(true);
                _orchestrator.Convert();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox("結果はAssets/HIHIMA/BlendShapeLimietr/Outputに保存されます", MessageType.Info);
        }
    }

    private void OnDestroy()
    {
        avatar.gameObject.SetActive(true);
        _orchestrator.CloseSettingWindow();
        _context.DeleteAll();
        _context = null;
        _orchestrator = null;
        Debug.Log("Called : SelectWindow.OnDestroy");
    }
}
