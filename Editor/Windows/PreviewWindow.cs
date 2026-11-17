using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// BlendShape プレビュー専用の SceneView
// suzuryg/face-emo の PreviewWindow の構成を参考に実装したもの:
// 専用のレンダリング用カメラでテクスチャに描画してキャッシュし、
// OnSceneGUI でそのキャッシュを表示する
public class PreviewWindow : SceneView
{
    public bool InUse => ReferenceEquals(focusedWindow, this);

    private Camera _renderCamera;
    private Texture2D _renderCache;
    private SceneView _lastActiveSceneViewCache;
    private bool _isInitialized;

    public static PreviewWindow OpenWindow()
    {
        var window = GetWindow<PreviewWindow>();
        window.titleContent = new GUIContent("BlendShapeLimiter Preview");
        return window;
    }

    // プレビュー対象（Body の中心など）にカメラを寄せる
    public void Initialize(Vector3 target)
    {
        if (_isInitialized) return;

        const float initialZoom = 0.12f;
        LookAt(
            point: target,
            direction: Quaternion.Euler(-5, 180, 0),
            newSize: initialZoom,
            ortho: false,
            instant: true);
        _isInitialized = true;
    }

    public void CloseIfNotDocked()
    {
        _isInitialized = false;
        if (!docked) Close();
    }

    // 現在の見た目をテクスチャに描画してキャッシュを更新する
    public void UpdateRenderCache()
    {
        if (camera == null || _renderCamera == null) return;

        try
        {
            _renderCamera.CopyFrom(camera);
            _renderCamera.enabled = true;

            var pixelWidth = _renderCamera.pixelWidth > 0 ? _renderCamera.pixelWidth : (int)position.width;
            var drawScale = position.width / pixelWidth;
            var scaledTextureWidth = Math.Max(1, (int)Math.Round(position.width, MidpointRounding.AwayFromZero));
            var scaledTextureHeight = Math.Max(1, (int)Math.Round(_renderCamera.pixelHeight * drawScale, MidpointRounding.AwayFromZero));

            var texture = PreviewRenderer.GetRenderedTexture(scaledTextureWidth, scaledTextureHeight, _renderCamera);
            if (texture != null)
            {
                if (_renderCache != null) DestroyImmediate(_renderCache);
                _renderCache = texture;
            }
        }
        finally
        {
            _renderCamera.targetTexture = null;
            _renderCamera.enabled = false;
        }
    }

    public override void OnEnable()
    {
        minSize = new Vector2(300, 300);
        base.OnEnable();
        titleContent = new GUIContent("BlendShapeLimiter Preview");

        // 直前にアクティブだった SceneView のカメラ設定を引き継ぐ
        var source = _lastActiveSceneViewCache;
        if (source == null && lastActiveSceneView != null && !ReferenceEquals(lastActiveSceneView, this))
        {
            source = lastActiveSceneView;
        }
        if (source != null && source.cameraSettings != null)
        {
            cameraSettings = new CameraSettings
            {
                speed = source.cameraSettings.speed,
                speedNormalized = source.cameraSettings.speedNormalized,
                speedMin = source.cameraSettings.speedMin,
                speedMax = source.cameraSettings.speedMax,
                easingEnabled = source.cameraSettings.easingEnabled,
                easingDuration = source.cameraSettings.easingDuration,
                accelerationEnabled = source.cameraSettings.accelerationEnabled,
                fieldOfView = source.cameraSettings.fieldOfView,
                nearClip = source.cameraSettings.nearClip,
                farClip = source.cameraSettings.farClip,
                dynamicClip = source.cameraSettings.dynamicClip,
                occlusionCulling = source.cameraSettings.occlusionCulling,
            };
        }

        // ギズモは描画しない
        drawGizmos = false;

        // レンダリング専用カメラを作成
        var cameraRoot = new GameObject
        {
            hideFlags = HideFlags.HideAndDontSave,
        };
        _renderCamera = cameraRoot.AddComponent<Camera>();
        _renderCamera.enabled = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (_lastActiveSceneViewCache != null)
        {
            SetLastActiveSceneView(_lastActiveSceneViewCache);
        }

        if (_renderCamera != null) DestroyImmediate(_renderCamera.gameObject);
        if (_renderCache != null)
        {
            DestroyImmediate(_renderCache);
            _renderCache = null;
        }
    }

    protected override void OnSceneGUI()
    {
        if (!_isInitialized)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("プレビュー対象が設定されていません");
                GUILayout.FlexibleSpace();
            }
            return;
        }

        base.OnSceneGUI();

        // このウィンドウを操作している間はライブ表示を優先する
        if (InUse || _renderCache == null) return;

        GUI.DrawTexture(new Rect(0, 0, _renderCache.width, _renderCache.height), _renderCache, ScaleMode.ScaleToFit, false);
    }

    private void OnFocus()
    {
        // base.OnSceneGUI() の呼び出しで lastActiveSceneView がアクティブになり
        // 視点操作に問題が出るため、lastActiveSceneView を差し替える（face-emo と同じ対策）
        if (lastActiveSceneView != null && !ReferenceEquals(lastActiveSceneView, this))
        {
            _lastActiveSceneViewCache = lastActiveSceneView;
        }
        SetLastActiveSceneView(this);
    }

    private void OnLostFocus()
    {
        if (_lastActiveSceneViewCache != null)
        {
            SetLastActiveSceneView(_lastActiveSceneViewCache);
        }

        // 視点操作が終わった時点の見た目でキャッシュを取り直す
        UpdateRenderCache();
    }

    private static void SetLastActiveSceneView(SceneView sceneView)
    {
        if (ReferenceEquals(sceneView, lastActiveSceneView)) return;

        var lastActiveSceneViewInfo =
            typeof(SceneView).GetProperty("lastActiveSceneView", BindingFlags.Public | BindingFlags.Static);
        if (lastActiveSceneViewInfo != null)
        {
            lastActiveSceneViewInfo.SetValue(null, sceneView, null);
        }
        else
        {
            Debug.LogError("lastActiveSceneView property not found");
        }
    }
}
