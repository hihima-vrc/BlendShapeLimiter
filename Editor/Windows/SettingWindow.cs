using UnityEditor;
using UnityEngine;

public class SettingWindow : EditorWindow
{
    private AvatarEditorContext _context = AvatarEditorContext.Instance;
    private BlendShapeOrchestrator _orchestrator = BlendShapeOrchestrator.Instance;

    private int blendShapeIndex;
    private string blendShapeName;
    private float blendShapeOverrideWeight;

    public static SettingWindow Open()
    {
        var window = GetWindow<SettingWindow>();
        return window;
    }

    private void OnEnable()
    {
        blendShapeIndex = _context.SelectedBlendShapeIndex;
        blendShapeName = _context.BlendShapeNames[blendShapeIndex];
        blendShapeOverrideWeight = _context.BlendShapeOverrideWeights[blendShapeIndex];
        if (_context.Body != null)
        {
            _context.ApplyPreviewWeight(blendShapeIndex, blendShapeOverrideWeight);
            _orchestrator.RequestPreviewRefresh();
        }
    }
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        blendShapeOverrideWeight = EditorGUILayout.Slider(blendShapeName, blendShapeOverrideWeight, 0f, 100f);
        if (EditorGUI.EndChangeCheck())
        {
            if (_context.Body != null)
            {
                _context.ApplyPreviewWeight(blendShapeIndex, blendShapeOverrideWeight);
                _orchestrator.RequestPreviewRefresh();
            }
        }
        if (_context.Body == null)
        {
            _orchestrator.CloseSettingWindow();
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(25)))
        {
            _orchestrator.CloseSettingWindow();
        }
        if (GUILayout.Button("Apply", GUILayout.Height(25)))
        {
            _context.UpdateBlendShapeOverrideWeights(blendShapeIndex, blendShapeOverrideWeight);
            _orchestrator.CloseSettingWindow();
        }

    }

    private void OnDestroy()
    {
        _context.UpdateSelectedBlendShapeIndex(-1);
        _context.UpdateIsOpeningSettingWindow(false);
        _context = null;
        _orchestrator = null;
        Debug.Log("Called : SettingWindow.OnDestroy");
    }
}
