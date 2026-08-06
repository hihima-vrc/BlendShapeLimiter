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

    // BlendShapeNames
    public List<string> BlendShapeNames { get; private set; }
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
        BlendShapeNames.Clear();
    }

    // BlendShapeOriginalWeights
    public List<float> BlendShapeOriginalWeights { get; private set; }
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
        BlendShapeOriginalWeights.Clear();
    }

    // BlendShapeOverrideWeights
    public List<float> BlendShapeOverrideWeights { get; private set; }
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
        BlendShapeOverrideWeights.Clear();
    }
    public void UpdateBlendShapeOverrideWeights(int blendShapeIndex, float overrideWeight)
    {
        if (BlendShapeOverrideWeights == null || BlendShapeOverrideWeights.Count < blendShapeIndex + 1) return;
        BlendShapeOverrideWeights[blendShapeIndex] = overrideWeight;
    }

    // SelectedBlendShapeIndex
    public int SelectedBlendShapeIndex { get; private set; }
    public void UpdateSelectedBlendShapeIndex(int selectedBlendShapeIndex)
    {
        SelectedBlendShapeIndex = selectedBlendShapeIndex;
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
