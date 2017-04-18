実行時にプリベーク済みのライトマップを切り替えるためのツール。
このツールは、5.5より下の単一バージョンと互換性がありません（ノートを参照）。
さまざまな照明シナリオで異なる解像度のライトマップを使用する場合は、PlayerSettingsで静的なバッチ処理を無効にする必要があります（ライトマップが変化する場合は、静的なバッチングを無効にする必要があります）。

システムは2つのコンポーネントに依存します。
- LevelLightmapData：さまざまな照明シナリオを参照し、照明を構築し、その依存関係をライトマップに保存します。
- LightingScenarioSwitcher：これは、実行時にライトマップを切り替えるためにLevelLightmapDataを呼び出すアセットの単なる例です。同じ方法で、異なるイベントでLevelLightmapDataを呼び出す他のコンポーネントを構築することもできます（OnTriggerEnterで同じスクリプトを実行するボックストリガーを使用するなど）。

使い方 ：
- 静的ジオメトリだけでシーンを作成します。自動ベーキングを無効にします。
- 「シーン」フォルダ（Assets /の下）に複数のライティングシーンを作成します。これらのシーンには静的なジオメトリが含まれてはいけません（余分な焼けた影が必要な場合を除きます）。照明シーンの設定は、次のいずれかでなければなりません。自動ベーキング、焼きGIのみ。場面のあなたのライトは、ベーキングする必要があります、ノートを参照してください。
- 静的ジオメトリシーンで空のgameObjectを追加し、LevelLightmapDataコンポーネントを追加します。
- 「照明シナリオサイズ」に必要な照明シナリオの数を入力し、「要素」フィールドで、プロジェクトビューからシーンをドラッグアンドドロップするか、フィールドの右側にある小さなポイントをクリックしてシーンを選択しますあなたのプロジェクト
- 1つ1つ、照明シナリオを作成してから保存することができます。照明シナリオの順序でこれらのステップを実行する必要があります（照明シナリオ1の前に照明シナリオ2を構築して保存することはできません）。一部のビルド（5.5.0f1に見られる）では、ベイク処理後にライトマップが正しく読み込まれないため、「ビルド」と「ストア」のステップはマージされていません。この問題が発生した場合は、シーンの中にLightScenarioSwitcherが存在しない場合はそれを無効にしてゲームモードを開始し、ゲームモードを停止し（ライトマップが正しく表示されるはずです）、Storeを起動してから良いのために遊ぶ）。
- 空のGameオブジェクトをシーンに追加し、スイッチをプレビューするためのLightingScenarioSwitcherを追加します
- [デフォルト照明シナリオ]フィールドは、再生を開始するときにロードする照明シナリオのIDです。 IDは、LevelLightmapDataの照明シナリオに関連付けられた番号です（たとえば、「Element 0」のIDは0です）
- 再生を開始し、クリックすると（マウスの左ボタンまたは入力割り当てを変更した場合はFire1）、ライトマップはさまざまな照明シナリオを切り替える必要があります。

NEW：
- バグの修正
- 照明シナリオリストは文字列ではなくなったので、sceneAssetsです。シーンを直接ドラッグ＆ドロップすることができます。
- StaticGeometryシーンのライトプローブも格納します（ライトプローブの数と位置は照明シナリオの間で変更できません）
- リファクタリング、グループ化されたカスタムラッパー、一貫性と明快さのために名前が変更された変数

変更：
- 照明シナリオリストはSceneAssetsを参照する必要があります（古い文字列値はもう使用できません）
- デフォルトの照明シナリオをLightingScenarioSwitcherに移動し、LevelLightmapDataにランタイムロジックがないようにするため、すべてのライトマップスイッチをスイッチャから起動する必要があります。

ノート ：
5.5未満のユニティバージョンを使用する場合は、少なくともLightLightmapDataスクリプトのライトマップの名前を変更する必要があります（lightmapLightとlightmapDirをそれぞれlightmapNearとlightmapFarで置き換えます）。
- 直接照明が焼き付けられていないライトを使用する場合は、スイッチを操作する際にライトマップに関連付けられたライティングシーンをロードする必要があります。

TODO：
- 反射プローブを含める
- GIを焼いたライトで照明切替えの例を含めるが、リアルタイムで直接照明する

ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー原文　↓ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー

Tool intended for switching pre-baked lightmaps at runtime.
This tool is not compatible with unity versions below 5.5, see notes.
If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to disable static batching in the PlayerSettings (if the lightmaps atlassing changes you need to disable static batching, if just the lightmaps' scale changes, that's fine).

The system relies on 2 components :
- LevelLightmapData : References the different lighting scenarios, builds the lighting, and stores the dependencies to the lightmaps.
- LightingScenarioSwitcher : This is just an example of asset that calls the LevelLightmapData in order to switch lightmaps at runtime. You could build other components that call the LevelLightmapData in the same way but on different events (like you could use a box trigger running the same script on OnTriggerEnter ).

How it works :
- Make a scene with your static geometry only. Disable Auto baking.
- Make several lighting scenes in a "Scenes" folder (under Assets/ ). These scenes should not contain static geometry (unless you want extra baked shadows). The Lighting scene settings must be : no Auto baking, only baked GI. Your lights in the scene must be baked, see notes.
- In your static geometry scene, add an empty gameObject and attach a LevelLightmapData component to it. 
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields, either drag and drop scenes from your Project view or click the little point on the right of the field and choose a scene from your project
- One by one, you can now Build the lighting scenario, and then Store it. You need to do these steps in the lighting scenario order ( you cannot build and store lighting scenario 2 before lighting scenario 1 ). The "Build" and the "Store" steps have not been merged because in some builds ( seen in 5.5.0f1 ) the lightmaps don't load correctly after baking. If you encounter this problem, you need to disable the LightingScenarioSwitcher if there is one in the scene, start game mode, stop game mode, (you should see the lightmaps properly then) and then Store (and re-anable the LightingScenarioSwitcher before you start playing for good).
- Now add an empty Gameobject to your scene and add a LightingScenarioSwitcher for previewing the switch
- The Default lighting scenario field is the ID of the lighting scenario you want to load when you start playing. The ID is the number associated to the lighting scenario in the LevelLightmapData ( ID of "Element 0" is 0 for example )
- Start playing and when clicking ( left mouse button or Fire1 if you have changed the input assignments ) the lightmaps should switch between your different lighting scenarios.

NEW :
- Bugfixes
- Now the lighting scenarios list is no longer strings, it's sceneAssets, so you can directly drag and drop your scene.
- Stores also the lightprobes from your StaticGeometry scene ( the number and position of the lightprobes cannot change between lighting scenarios )
- Refactor, grouped custom wrappers, renamed variables for consistency and clarity

CHANGES :
- The lighting scenarios list needs to reference SceneAssets ( old string values won't work anymore )
- Default lighting scenario moved to LightingScenarioSwitcher so that the LevelLightmapData does not have any runtime logic, all lightmap switches need to be triggered from the switcher.

NOTES : 
- if you want to use unity versions below 5.5, you should at least change the name of the lightmaps in the LevelLightmapData script ( replace lightmapLight and lightmapDir respectively with lightmapNear and lightmapFar, not tested ).
- if you want to use lights that don't have direct lighting baked, then you would need to also load the lighting scene associated with the lightmaps when operating the switch.

TODO :
- Include reflection probes
- Include an example of lighting switching with lights that have baked GI but realtime direct lighting
