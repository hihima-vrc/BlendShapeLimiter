using UnityEngine;

// プレビュー描画用ユーティリティ
// suzuryg/face-emo の DrawingUtility.GetRenderedTexture の構成を参考に、
// このツールで必要な部分だけを切り出したもの
public static class PreviewRenderer
{
    // 指定したカメラの映像を Texture2D として取得する
    public static Texture2D GetRenderedTexture(int width, int height, Camera camera)
    {
        if (width <= 0 || height <= 0 || camera == null) return null;

        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        var renderTexture = RenderTexture.GetTemporary(
            width, height,
            depthBuffer: 24,
            format: RenderTextureFormat.ARGB32,
            readWrite: RenderTextureReadWrite.sRGB,
            antiAliasing: 8);
        try
        {
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Bilinear;

            RenderCamera(renderTexture, camera);
            CopyRenderTexture(renderTexture, texture);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        return texture;
    }

    private static void RenderCamera(RenderTexture renderTexture, Camera camera)
    {
        var targetTextureCache = camera.targetTexture;
        var aspectCache = camera.aspect;
        try
        {
            camera.targetTexture = renderTexture;
            camera.aspect = (float)renderTexture.width / renderTexture.height;
            camera.Render();
        }
        finally
        {
            camera.targetTexture = targetTextureCache;
            camera.aspect = aspectCache;
        }
    }

    private static void CopyRenderTexture(RenderTexture source, Texture2D destination)
    {
        var activeRenderTextureCache = RenderTexture.active;
        try
        {
            RenderTexture.active = source;
            destination.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            destination.Apply();
        }
        finally
        {
            RenderTexture.active = activeRenderTextureCache;
        }
    }
}
