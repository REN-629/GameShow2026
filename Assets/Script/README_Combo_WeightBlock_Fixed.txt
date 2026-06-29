Script Combo + WeightBlock Fixed

修正:
- RoomCellのスコア差分UI呼び出しを修正
- 実際にscoreAddedした時だけScoreComboDeltaNotifier.AddScoreDelta(1)を呼ぶ
- 古いScoreDeltaNotifier参照をRoomCellから削除
- 未参照のScoreDeltaNotifier.csは削除
- PressurePlatePuzzle_DoorOnly.cs / WeightDoorCondition.csを削除
- PressurePlatePuzzleがCarryWeightBlock / PressureWeightSource / PlayerWeightProxyを重量として読めるように修正

追加:
- CarryWeightBlock.cs
- CarryWeightBlockController.cs
- WeightBlockSpawnPoint.cs
- WeightBlockSpawner.cs

導入:
1. PlayerにCarryWeightBlockControllerを追加
2. playerCameraにMainCameraを登録
3. holdPointに重り用HoldPointを登録
4. heldItemControllerにHeldItemControllerを登録
5. itemThrowerにItemThrowerを登録
6. 重りPrefabにRigidbody / Collider / CarryWeightBlockを追加
7. 部屋PrefabまたはPuzzlePrefabにWeightBlockSpawnerを追加
8. 子にWeightBlockSpawnPointを配置
9. WeightBlockSpawner.blockPrefabsに重りPrefabを登録

操作:
- Eで重りを持つ
- 持っている間は通常アイテム操作停止
- 右クリック+マウスで重り回転
- EまたはGで手放す

スコアコンボUI:
- Canvas上の常時ONオブジェクトにScoreComboDeltaNotifierを置く
- scorePopupにComboDeltaPopupUIを登録
- ComboDeltaPopupUI.rootには非表示にしていい子オブジェクトを登録
