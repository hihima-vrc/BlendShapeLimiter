# BlendShapeLimiter
　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
> [!NOTE]
> 変換した結果は、`Assets/HIHIMA/BlendShapeLimiter/Output`に保存されています。  
> ここに置かれているメッシュをSkinnedMeshRendererコンポーネントの「Mesh」のところに設定しないと反映されません！  
> 元に戻したいときは「Mesh」を右クリック → Revertで戻せます。

## 概要
VRChat想定、アバターのBlendShape（ブレンドシェイプ、Shape Key、シェイプキー）の値を、0～100の間で自由に上書きできるようにするツールです。  
瞬きで表情が破綻する時などに使用することを想定しています。が、顔のメッシュ以外でも別に制限できるので、使い道は自由です。  

## 使い方
0. Unitypackageをインポート  
1. `Tools > BlendShapeLimiter`から開けます。  
2. `Avatar`のところにヒエラルキーに置いてあるアバターをD&D、`Body`にはBodyとかBody_BaseとかのSkinnedMeshRendererをD&Dしてください。アバターをD&Dした時点でそのアバターがActiveでなくなり、また、ヒエラルキーにクローンが生成されますが、**BodyにD&Dするのは、Activeでなくなった、もとのアバターのGameObjectです。**  
3. BlendShape一覧が表示されるので、マウスカーソルを合わせると、そのBlendShapeが100の時のプレビューを見ることができ、破綻の有無がわかります。  
4. BlendShape一覧の上にあるスライダーは、ボタンの高さを変えられます。マウスホイールを回してピッタリ1ボタン分進むようにすると見やすいと思います。  

> [!WARNING]
> いろいろ試したんですが、マウスホイールを回しても、前のボタンにホバー判定が吸われるということが起こっています。  
> なので、マウスホイールを回しながら表情の破綻を見つけたと思っても、一旦カーソルをブンブン振り回して本当にそのBlendShapeが破綻の原因か確認してください。  

5. 破綻させているBlendShapeをクリックすると、設定画面が開きます。ちょうどいいところを狙ってスライダーで値を調整してあげてください。  
6. `Apply`をクリックして、その値を上限値に設定、`Cancel`か×ボタンをクリックでキャンセルできます。  
7. BlendShape一覧で、設定済みのものは緑色になっているはずです。  
8. `Convert`をクリックして、ポップアップが出たら変換完了です。  
9. `Assets/HIHIMA/BlendShapeLimiter/Output`にメッシュが保存されているので、元のアバターのBodyなどのSkinnedMeshRendererコンポーネントの設定項目、「Mesh」のところに、出力されたメッシュをD&Dしてください。  
10. 一応、うまく変換できているか確認して、完了です。  

## 注意点
* UIに書いてある通りにすれば基本的に困りません。大事なことだけ書きます。  
* アバターを間違えた時は、一度**ウィンドウを閉じて**再びツールを開いてください。  
* 元に戻したいときは、SkinnedMeshRendererの「Mesh」のところから、右クリック → Revertで戻ります。
* 何らかの原因で、ツールのウィンドウを消してもアバターのクローンが消えないとき、手動で消してください。  
* その他不具合はissueにてお待ちしております。  

## 以下、逸般人向けの説明

### 技術的詳細
このツールは`Mesh.ClearBlendShapes()`と、`Mesh.AddBlendShapeFrame()`の力技で構成されています。  
前者で`SkinnedMeshRenderer.sharedMesh`のBlendShapeを全て剥がし、`Mesh.GetBlendShapeFrameVertices()`で取ったフレームの情報をもとに再構築していく力技です。  
ただし、値が調整されたものに関しては、`Mesh.GetBlendShapeFrameVertices()`で得た、頂点、法線、接線の移動量を調整された値の時の移動量となるように計算しています。  
具体的に、1フレーム目のWeightまでに調整された値（`overrideWeight`）がある場合、  
```csharp
Vector3[] finalVertices = overrideWeight * firstFrameVertices / firstFrameWeight
```
で計算され、フレームとフレームの間にOverrideWeightがある時、
```csharp
Vector3[] finalVertices = ((nextFrameWeight - OverrideWeight) * previousFrameVertices + (overrideWeight - previousFrameWeight) * nextFrameVertices) / (nextFrameWeight - previousFrameWeight)
```
で計算されます。  
そのほかにも、中間フレームがあって、`overrideWeight`が調整されている場合には、各フレームに設定されているWeightを、`overrideWeight / 100f`することで破綻しないようにしています。  
