using System.Collections.Generic;
using UnityEngine;

public class BlendShapeOverrideService : IBlendShapeProcessor
{
    public Mesh ApplyOverride(SkinnedMeshRenderer avatarInstance, List<string> blendShapeNames, List<float> overrideValues)
    {

        if (avatarInstance == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found on the avatar instance.");
            return null;
        }

        Mesh mesh = Mesh.Instantiate(avatarInstance.sharedMesh);
        mesh.ClearBlendShapes();

        for (int i = 0; i < blendShapeNames.Count; i++)
        { 
            List<BlendShapeFrameData> frameDatas = GetBlendShapeVertices(avatarInstance.sharedMesh, i, overrideValues[i]);

            for (int j = 0; j < frameDatas.Count; j++)
            {
                mesh.AddBlendShapeFrame(blendShapeNames[i], frameDatas[j].blendShapeWeight, frameDatas[j].deltaVertices, frameDatas[j].deltaNormals, frameDatas[j].deltaTangents);
            }
        }

        return mesh;
    }

    private List<BlendShapeFrameData> GetBlendShapeVertices(Mesh mesh, int blendShapeIndex, float overrideValue)
    {
        int vertexCount = mesh.vertexCount;
        Vector3[] deltaVertices = new Vector3[vertexCount];
        Vector3[] deltaNormals = new Vector3[vertexCount];
        Vector3[] deltaTangents = new Vector3[vertexCount];

        int blendShapeFrameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);

        // 変化させないBlendShapeの場合、そのままのdeltaVertices, deltaNormals, deltaTangentsを返す
        // ただし、複数のフレームがある場合は、List<BlendShapeFrameData>の長さが2以上になることに注意
        if (overrideValue == 100f)
        {
            List<BlendShapeFrameData> frameDatas = new List<BlendShapeFrameData>();

            for (int i = 0; i < blendShapeFrameCount; i++)
            {
                Vector3[] __deltaVertices = new Vector3[vertexCount];
                Vector3[] __deltaNormals = new Vector3[vertexCount];
                Vector3[] __deltaTangents = new Vector3[vertexCount];

                mesh.GetBlendShapeFrameVertices(blendShapeIndex, i, __deltaVertices, __deltaNormals, __deltaTangents);

                BlendShapeFrameData frameData = new BlendShapeFrameData
                {
                    deltaVertices = __deltaVertices,
                    deltaNormals = __deltaNormals,
                    deltaTangents = __deltaTangents,
                    blendShapeWeight = overrideValue
                };
                frameDatas.Add(frameData);
            }
            return (frameDatas);
        }


        // 変化させるBlendShapeの場合、overrideValueに応じてdeltaVertices, deltaNormals, deltaTangentsを計算する
        // まず、blendShapeFrameWeightsを取得し、overrideValueがどのフレームの間に属するかを判定する

        float[] blendShapeFrameWeights = new float[blendShapeFrameCount];
        int overrideFrameIndex = -1;

        for (int i = 0; i < blendShapeFrameCount; i++)
        {
            blendShapeFrameWeights[i] = mesh.GetBlendShapeFrameWeight(blendShapeIndex, i);
        }

        for (int i = 0; i < blendShapeFrameCount; i++)
        {
            if (blendShapeFrameWeights[i] >= overrideValue)
            {
                overrideFrameIndex = i;
                break;
            }
        }

        // 0fと最初のフレームの間にoverrideValueがある場合、最初のフレームのdeltaVertices, deltaNormals, deltaTangentsをoverrideValueに応じてスケーリングする
        if (overrideFrameIndex == 0)
        {
            float scale = overrideValue / blendShapeFrameWeights[0];
            mesh.GetBlendShapeFrameVertices(blendShapeIndex, overrideFrameIndex, deltaVertices, deltaNormals, deltaTangents);

            for (int i = 0; i < vertexCount; i++)
            {
                deltaVertices[i] *= scale;
                deltaNormals[i] *= scale;
                deltaTangents[i] *= scale;
            }

            BlendShapeFrameData frameData = new BlendShapeFrameData
            {
                deltaVertices = deltaVertices,
                deltaNormals = deltaNormals,
                deltaTangents = deltaTangents,
                blendShapeWeight = 100f
            };
            return (new List<BlendShapeFrameData> { frameData });
        }
        // overrideValueがフレームとフレームの間にある場合、前のフレームと次のフレームのdeltaVertices, deltaNormals, deltaTangentsをoverrideValueに応じて線形補間する(内分)
        // この時、List<BlendShapeFrameData>の長さが2以上になることに注意
        else
        {
            List<BlendShapeFrameData> frameDatas = new List<BlendShapeFrameData>();

            for (int i = 0; i < overrideFrameIndex; i++)
            {
                Vector3[] __deltaVertices = new Vector3[vertexCount];
                Vector3[] __deltaNormals = new Vector3[vertexCount];
                Vector3[] __deltaTangents = new Vector3[vertexCount];

                mesh.GetBlendShapeFrameVertices(blendShapeIndex, i, __deltaVertices, __deltaNormals, __deltaTangents);

                BlendShapeFrameData _frameData = new BlendShapeFrameData
                {
                    deltaVertices = __deltaVertices,
                    deltaNormals = __deltaNormals,
                    deltaTangents = __deltaTangents,
                    blendShapeWeight = blendShapeFrameWeights[i] * 100f / overrideValue
                };
                frameDatas.Add(_frameData);
            }

            Vector3[] _deltaVertices = new Vector3[vertexCount];
            Vector3[] _deltaNormals = new Vector3[vertexCount];
            Vector3[] _deltaTangents = new Vector3[vertexCount];

            mesh.GetBlendShapeFrameVertices(blendShapeIndex, overrideFrameIndex - 1, _deltaVertices, _deltaNormals, _deltaTangents);
            mesh.GetBlendShapeFrameVertices(blendShapeIndex, overrideFrameIndex, deltaVertices, deltaNormals, deltaTangents);

            float smallCoefficient = (blendShapeFrameWeights[overrideFrameIndex] - overrideValue) / (blendShapeFrameWeights[overrideFrameIndex] - blendShapeFrameWeights[overrideFrameIndex - 1]);
            float largeCoefficient = (overrideValue - blendShapeFrameWeights[overrideFrameIndex - 1]) / (blendShapeFrameWeights[overrideFrameIndex] - blendShapeFrameWeights[overrideFrameIndex - 1]);

            for (int i = 0; i < vertexCount; i++)
            {
                deltaVertices[i] = _deltaVertices[i] * smallCoefficient + deltaVertices[i] * largeCoefficient;
                deltaNormals[i] = _deltaNormals[i] * smallCoefficient + deltaNormals[i] * largeCoefficient;
                deltaTangents[i] = _deltaTangents[i] * smallCoefficient + deltaTangents[i] * largeCoefficient;
            }

            BlendShapeFrameData frameData = new BlendShapeFrameData
            {
                deltaVertices = deltaVertices,
                deltaNormals = deltaNormals,
                deltaTangents = deltaTangents,
                blendShapeWeight = 100f
            };
            frameDatas.Add(frameData);

            return (frameDatas);
        }
    }
}
