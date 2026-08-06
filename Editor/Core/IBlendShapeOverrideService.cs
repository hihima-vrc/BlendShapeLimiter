using System.Collections.Generic;
using UnityEngine;

public interface IBlendShapeProcessor
{
    Mesh ApplyOverride(SkinnedMeshRenderer avatarInstance, List<string> blendShapeNames, List<float> overrideValues);
}
