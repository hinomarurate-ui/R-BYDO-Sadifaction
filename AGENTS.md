# R-BYDO-Sadifaction

## 敵キャラ追加ルール

- 新しい敵の移動・攻撃・常時処理は、必ず `EnemyBehaviourBase` を継承して実装する。
- 役割に応じて `IEnemyMovement`、`IEnemyAttackPattern`、`IEnemyRoutine` を実装し、`EnemyController` の共通ライフサイクルへ参加させる。
- `Initialize(EnemyController)` を敵ごとに再実装しない。初回設定は `OnInitialized`、再有効化時の設定は `OnBehaviourEnabled` をオーバーライドする。
- `Rigidbody2D`、`Collider2D`、`Animator`、`SpriteRenderer`、`AudioSource`、対象プレイヤーは基盤クラスのプロパティを使い、各敵で重複取得しない。
- 移動速度、射程、攻撃間隔、威力など敵固有の調整値はPrefabのInspectorを正とする。`EnemyDefinition` へ移動・攻撃値を追加しない。
- `EnemyDefinition` はHP、スコア、怯み、死亡、共通Animator設定に限定する。
- 特殊な動きや攻撃は専用Controllerに残してよいが、初期化・停止・対象取得は共通基盤を使う。
- 新しい敵Prefabには `EnemyController`、`EnemyHealth`、必要なBehaviour、`EnemyActivator`、対応する `EnemyDefinition` を設定する。
- 既存敵のInspector値や挙動を、共通化だけを理由に変更しない。

## 追加後の確認

- 同じBehaviourが複数の役割を持つ場合も、初期化が1回だけ行われることを確認する。
- 起動トリガー、移動、攻撃、被弾、怯み、死亡、スコア加算をUnity上で確認する。
- Prefabに `fallbackDefinition` や `useDefinitionSettings` を再追加しない。
