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
    private int _hoveredBlendShapeIndex = -1;

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
                if (isParentHasChild)
                {
                    _orchestrator.OpenPreviewWindow();
                }
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
            // ボタンを配置、マウスオーバーしている物は、OverrideWeightを適用
            int hoveredBlendShapeIndex = -1;
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
                    hoveredBlendShapeIndex = i;
                }
            }
            EditorGUILayout.EndScrollView();

            if (hoveredBlendShapeIndex != _hoveredBlendShapeIndex)
            {
                _hoveredBlendShapeIndex = hoveredBlendShapeIndex;
                if (_hoveredBlendShapeIndex >= 0)
                {
                    _context.ApplyPreviewWeight(_hoveredBlendShapeIndex, _context.BlendShapeOverrideWeights[_hoveredBlendShapeIndex]);
                    _orchestrator.RequestPreviewRefresh();
                }
                else if (!_context.IsOpeningSettingWindow)
                {
                    // 設定画面を開いている間は、そちらのプレビューを優先する
                    _context.ResetPreviewWeights();
                    _orchestrator.RequestPreviewRefresh();
                }
            }

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
        if (avatar != null)
        {
            avatar.gameObject.SetActive(true);
        }
        _orchestrator.CloseSettingWindow();
        _orchestrator.ClosePreviewWindow();
        _context.DeleteAll();
        _context = null;
        _orchestrator = null;
        Debug.Log("Called : SelectWindow.OnDestroy");
    }
}
