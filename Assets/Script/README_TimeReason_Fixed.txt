Time Reason Fixed

変更:
- GamePhaseTimerに AddTime(float amount, string reason) を追加
- GamePhaseTimerに RemoveTime(float amount, string reason) を追加
- RoomRunTimerの部屋到達時加算を理由付き表示に対応
- TimeDeltaReasonNotifierで理由テキストを表示
- ComboDeltaPopupUIの理由表示を「理由+〇」形式に変更
- RoomRunTimeChangeReason.csを追加

使い方:
1. CanvasにTimeDeltaControllerを作る
2. TimeDeltaControllerは常にON
3. 子にTimeDeltaViewを作る
4. TimeDeltaViewにテキストとゲージを置く
5. TimeDeltaControllerにComboDeltaPopupUIとTimeDeltaReasonNotifierを付ける
6. ComboDeltaPopupUI.rootにTimeDeltaViewを入れる
7. TimeDeltaReasonNotifier.timePopupにComboDeltaPopupUIを入れる

表示例:
- 部屋到達+8
- 早い突破+5
- 強引な突破+2
- ダメージ-5

ダメージなどで減算する場合:
RoomRunTimer.RunInstance.RemoveTime(5f, "ダメージ");

強引な突破などで加算する場合:
RoomRunTimer.RunInstance.AddTime(5f, "強引な突破");

RoomRunTimer設定:
Show Each Reason Separately ON
→ 部屋到達+8、早い突破+5、強引な突破+2 を別々に表示

Show Each Reason Separately OFF
→ 時間加算+15 のように合算表示
