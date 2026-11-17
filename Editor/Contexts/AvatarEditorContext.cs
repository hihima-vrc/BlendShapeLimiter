using System.Collections.Generic;
using UnityEngine;

public class AvatarEditorContext
{
    public static AvatarEditorContext Instance { get; private set; } = new AvatarEditorContext();

    // Avatar
    public GameObject Avatar { get; private set; }
    public void SetAvatar(GameObject avatarInstance)
    {
        if (avatarInstance == null) return;
        Avatar = avatarInstance;
        // Avatar.hideFlags = HideFlags.HideAndDontSave;
    }
    public void DeleteAvatar()
    {
        if (Avatar == null) return;
        GameObject.DestroyImmediate(Avatar);
        Avatar = null;
    }

    // Body
    public SkinnedMeshRenderer Body { get; private set; }
    public void SetBody(SkinnedMeshRenderer bodyInstance)
    {
        if (bodyInstance == null) return;
        Body = bodyInstance;
        // Body.hideFlags = HideFlags.HideAndDontSave;
    }
    public void DeleteBody()
    {
        if (Body == null) return;
        SkinnedMeshRenderer.DestroyImmediate(Body);
        Body = null;
    }
    public void UpdateBodyBlendShapeWeight(int blendShapeIndex, float blendShapeWeight)
    {
        Body.SetBlendShapeWeight(blendShapeIndex, blendShapeWeight);
    }
    public void ResetPreviewWeights()
    {
        if (Body == null || BlendShapeOriginalWeights == null) return;
        for (int i = 0; i < Body.sharedMesh.blendShapeCount; i++)
        {
            if (i >= BlendShapeOriginalWeights.Count) break;
            Body.SetBlendShapeWeight(i, BlendShapeOriginalWeights[i]);
        }
    }
    public void ApplyPreviewWeight(int blendShapeIndex, float blendShapeWeight)
    {
        if (Body == null || BlendShapeOriginalWeights == null) return;
        if (blendShapeIndex < 0 || blendShapeIndex >= Body.sharedMesh.blendShapeCount) return;
        ResetPreviewWeights();
        Body.SetBlendShapeWeight(blendShapeIndex, blendShapeWeight);
    }

    // BlendShapeNames
    public List<string> BlendShapeNames { get; private set; } = new();
    public void SetBlendShapeNames()
    {
        if (Body == null) return;
        for (int i = 0; i < Body.sharedMesh.blendShapeCount; i++)
        {
            BlendShapeNames.Add(Body.sharedMesh.GetBlendShapeName(i));
        }
    }
    public void DeleteBlendShapeNames()
    {
        if (BlendShapeNames == null) return;
        BlendShapeNames.Clear();
    }

    // BlendShapeOriginalWeights
    public List<float> BlendShapeOriginalWeights { get; private set; } = new();
    public void SetBlendShapeOriginalWeights()
    {
        if (Body == null) return;
        for (int i = 0; i < Body.sharedMesh.blendShapeCount; i++)
        {
            BlendShapeOriginalWeights.Add(Body.GetBlendShapeWeight(i));
        }
    }
    public void DeleteBlendShapeOriginalWeight()
    {
        if (BlendShapeOriginalWeights == null) return;
        BlendShapeOriginalWeights.Clear();
    }

    // BlendShapeOverrideWeights
    public List<float> BlendShapeOverrideWeights { get; private set; } = new();
    public void SetBlendModeOverrideWeights()
    {
        if (Body == null) return;
        for (int i = 0; i < Body.sharedMesh.blendShapeCount; i++)
        {
            BlendShapeOverrideWeights.Add(100f);
        }
    }
    public void DeleteBlendShapeOverrideWeights()
    {
        if (BlendShapeOverrideWeights == null) return;
        BlendShapeOverrideWeights.Clear();
    }
    public void UpdateBlendShapeOverrideWeights(int blendShapeIndex, float overrideWeight)
    {
        if (BlendShapeOverrideWeights == null || BlendShapeOverrideWeights.Count < blendShapeIndex + 1) return;
        BlendShapeOverrideWeights[blendShapeIndex] = overrideWeight;
    }

    // SelectedBlendShapeIndex
    public int SelectedBlendShapeIndex { get; private set; } = -1;
    public void UpdateSelectedBlendShapeIndex(int selectedBlendShapeIndex)
    {
        SelectedBlendShapeIndex = selectedBlendShapeIndex;
    }

    // IsOpeningSettingWindow
    public bool IsOpeningSettingWindow { get; private set; } = false;
    public void UpdateIsOpeningSettingWindow(bool isOpeningSettingWindow)
    {
        IsOpeningSettingWindow = isOpeningSettingWindow;
    }

    // isConverting
    public bool IsConverting { get; private set; } = false;
    public void UpdateIsConverting(bool isConverting)
    {
        IsConverting = isConverting;
    }


    // Other
    public void DeleteAll()
    {
        DeleteAvatar();
        DeleteBody();
        DeleteBlendShapeNames();
        DeleteBlendShapeOriginalWeight();
        DeleteBlendShapeOverrideWeights();
    }
}
