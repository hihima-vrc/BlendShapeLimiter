using UnityEditor;
using UnityEngine;

public class BlendShapeOrchestrator
{
    public static BlendShapeOrchestrator Instance { get; private set; } = new BlendShapeOrchestrator();

    private AvatarEditorContext _context = AvatarEditorContext.Instance;

    private IBlendShapeProcessor _processor = new BlendShapeOverrideService();

    private static SelectWindow selectWindow;
    private static SettingWindow settingWindow;
    private static PreviewWindow previewWindow;
    private bool _previewDirty;
    private bool _updateHooked;


    [MenuItem("Tools/BlendShapeLimiter")]
    public static void OpenSelectWindow()
    {
        selectWindow = SelectWindow.ShowWindow();
    }

    public void CloseSelectWindow()
    {
        selectWindow.Close();
    }

    public void OpenSettingWindow()
    {
        if (!_context.IsOpeningSettingWindow) return;
        settingWindow = SettingWindow.Open();
        settingWindow.Focus();
    }

    public void CloseSettingWindow()
    {
        if (!_context.IsOpeningSettingWindow) return;
        settingWindow.Close();
        selectWindow.Focus();
    }


    public void OpenPreviewWindow()
    {
        previewWindow = PreviewWindow.OpenWindow();
        if (_context.Body != null)
        {
            previewWindow.Initialize(_context.Body.bounds.center);
        }
        if (!_updateHooked)
        {
            EditorApplication.update += OnEditorUpdate;
            _updateHooked = true;
        }
    }

    public void ClosePreviewWindow()
    {
        if (_updateHooked)
        {
            EditorApplication.update -= OnEditorUpdate;
            _updateHooked = false;
        }
        previewWindow?.CloseIfNotDocked();
        previewWindow = null;
    }

    public void RequestPreviewRefresh()
    {
        _previewDirty = true;
    }

    private void OnEditorUpdate()
    {
        if (!_previewDirty) return;
        _previewDirty = false;
        if (previewWindow != null)
        {
            previewWindow.UpdateRenderCache();
            previewWindow.Repaint();
        }
    }


    public bool SetBody(string componentName)
    {
        SkinnedMeshRenderer[] bodies = _context.Avatar.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var b in bodies)
        {
            if (b.name == componentName)
            {
                _context.SetBody(b);
                _context.SetBlendShapeNames();
                _context.SetBlendShapeOriginalWeights();
                _context.SetBlendModeOverrideWeights();
                RequestPreviewRefresh();
                return true;
            }
        }
        return false;
    }

    public void Convert()
    {
        if (_context.Body == null && _context.BlendShapeNames == null && _context.BlendShapeOverrideWeights == null) return;
        Mesh mesh = _processor.ApplyOverride(_context.Body, _context.BlendShapeNames, _context.BlendShapeOverrideWeights);
        if (mesh == null) return;
        string folderPath = "Assets/HIHIMA/BlendShapeLimiter/Output";
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(folderPath + $"/{_context.Body.name}_modified.asset");
        UnityEditor.AssetDatabase.CreateAsset(mesh, path);
        UnityEditor.EditorUtility.SetDirty(mesh);
        UnityEditor.AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("変換終了", "変換が終了しました。\n結果はAssets/HIHIMA/BlendShapeLimiter/Outputに保存されています\nSkinnedMeshRendererコンポーネントの「Mesh」を差し替えて使ってください", "OK");
        _context.UpdateIsConverting(false);
    }
}
