# 幸せな夢（個人製作）

## ゲームのデモ映像

- デモ映像は3分、5分、7分三つのバージョン用意しております、お手数をおかけしますが、必要に応じてYoutubeのリンク先でご覧ください。
- 動画名:[DemoVideo 3min Ver](https://www.youtube.com/watch?v=rQxwyfMHoT8)
- 動画名:[DemoVideo 5min Ver](https://www.youtube.com/watch?v=r2Qcn6qZNOk)
- 動画名:[DemoVideo 7min Ver](https://www.youtube.com/watch?v=NGVvPrMSUng)

---

## プロジェクト概要

本プロジェクトは、JRPGの魅力に、現代のコンピュータビジョン技術を融合させたRPGゲームです。Unityエンジンを基盤とし、C#とPythonを用いて各種機能を実装しています。クラシックなJRPGの要素を保ちながら、ジェスチャーコントロールやQRコード認識といった今までRPGでは珍しいなインタラクション方式を取り入れ、全く新しいゲーム体験を挑戦します。

## プロジェクト製作期間（6ヶ月）

2024.1 ~ 2024.2 :探索システム及びバトルシステムのテスト版製作
2024.2 ~ 2024.3 :コンピュータービジョンの導入
2024.3 ~ 2024.4 :ゲームのコアシステムをまとめ

(卒業論文製作、引っ越し、兵役)

2024.12 ~ 2025.1 :ゲーム要素を追加、バトルシステムゲーム性の見直し
2025.1  ~ 2025.2 :ゲームシステム全体の見直し(作り直す)
2025.2  ~ 2025.3.16 :デモ版完成

---

## ゲーム背景とストーリー

背景は、火災によって左額に傷跡が残り、内向的な女子高生が夢の世界に迷い込む物語です。  
彼女は夢の中で探索、謎解き、さまざまなインタラクションを通じ、夢から脱出して現実へ戻る方法を模索します。  
そして、夢の中で様々な記憶が蘇るにつれて、火災と家族に関する真実が徐々に解き明かされ、最終的には自分の忘れた過去と向き合うストーリーとなっています。（デモ版では未完成）

---

## ビジュアルスタイル

物語のテーマに合わせ、ゲームはピクセルアートを採用。基本的なビジュアルはモノクロを基調を用いることで夢の幻想的な雰囲気を表現しています。

---

## ゲームの特徴

### クラシックJRPG＋alphaのゲーム体験

- **ターン制バトルシステム**  
  伝統的なターン制の戦闘システムとジェスチャーコントロールを融合した、戦略的なバトルを実現。
  コンピュータビジョン技術でプレイヤーの手のキーポイントを検出し、戦闘中にプレイヤーのジェスチャーによる操作を実現。
  
- **固定ストーリーラインと精緻なステージ設計**  
  しっかりと構築された物語とステージにより、没入感のあるゲーム体験。（デモ版シナリオは未完成）
  
- **豊富なNPCとのインタラクション**  
  多彩なキャラクターとの対話を通じ、物語が深まります。（デモ版シナリオは未完成）
  
- **QRコード認識**  
  QRコードの導入によってゲームのインタラクション性を向上、謎解き要素として、ゲームに組み込んでおり、
  今までJRPGのない要素を取り入れ、プレイヤーに新鮮な体験を提供。

### 美術デザイン

- **美術デザイン**  
  CLIP STUDIO PAINTやAsepriteを用いて、キャラクター設定、シーン、エフェクトのデザインを製作。

---

## 開発ツールと技術

- **開発エンジン**：Unity
- **プログラミング言語**：C# 、Python
- **美術ツール**：CLIP STUDIO PAINT、Aseprite
- **デモ版映像編集**：Adobe Premiere
- **コンピュータビジョン技術**：CVZoneライブラリを用いたジェスチャートラッキング、Pyzbarライブラリを用いたQRコード解読

---

## ゲーム開発環境

- **OS**：Windows 11
- **ハードウェア**：AMD Ryzen 9 6900HS、AMD Radeon RX 6700S、16GB RAM
- **開発ソフトウェア**：Visual Studio 2022、Visual Studio Code、Unity 2021.1.3f1

---

## ソースコードの取得

- ほんファイル内の"SourceCode"ファイルに収納しております

### ゲームの実行

- ほんファイル内の"Executable Build"ファイルの中の"Lucid Dream.exe"で実行可能

---

## 使用素材一覧

### フォント

- **Cubic-11**
  - URL: [https://github.com/ACh-K/Cubic-11](https://github.com/ACh-K/Cubic-11)

### BGM / SE

- **びたちー素材館**  
  - URL: [http://www.vita-chi.net/sozai1.htm](http://www.vita-chi.net/sozai1.htm)  

- **seadenden**  
  - URL: [https://seadenden-8bit.com](https://seadenden-8bit.com)  

- **効果音ラボ**  
  - URL: [https://soundeffect-lab.info/](https://soundeffect-lab.info/)  

- **F@ct**  
  - URL: [https://facton.booth.pm/](https://facton.booth.pm/)  

### Unity Asset Store アセット

- **Retro RPG Tileset 01**  
  - URL: [https://assetstore.unity.com/packages/2d/environments/retro-rpg-tileset-01-70733](https://assetstore.unity.com/packages/2d/environments/retro-rpg-tileset-01-70733)  
  - ドット絵タイルセット

- **Easy Transitions**  
  - URL: [https://assetstore.unity.com/packages/tools/gui/easy-transitions-225607](https://assetstore.unity.com/packages/tools/gui/easy-transitions-225607)  
  - トランジション演出用パッケージ

- **Free 1 Bit Forest**  
  - URL: [https://assetstore.unity.com/packages/2d/environments/free-1-bit-forest-232615](https://assetstore.unity.com/packages/2d/environments/free-1-bit-forest-232615)  
  - 1ビットスタイルの森アセット

- **White Scape - 1bit asset pack characters and sprites**  
  - URL: [https://assetstore.unity.com/packages/2d/characters/white-scape-1bit-asset-pack-characters-and-sprites-268429](https://assetstore.unity.com/packages/2d/characters/white-scape-1bit-asset-pack-characters-and-sprites-268429)  
  - キャラクター＆スプライトアセット

- **Slime Enemy - Pixel Art**  
  - URL: [https://assetstore.unity.com/packages/2d/characters/slime-enemy-pixel-art-228568](https://assetstore.unity.com/packages/2d/characters/slime-enemy-pixel-art-228568)  
  - ピクセルアートの敵キャラクター

- **Free Pixel Art FX Package**  
  - URL: [https://assetstore.unity.com/packages/2d/textures-materials/free-pixel-art-fx-package-185612](https://assetstore.unity.com/packages/2d/textures-materials/free-pixel-art-fx-package-185612)  
  - エフェクトアセット

### パッケージ

- **xNode**
  - URL: [Siccity/xNode](https://github.com/Siccity/xNode)
  - 説明: ノードベースのエディタライブラリとして使用されています。  
    対話グラフやその他のノードベースのシステム実装に活用されています。 

  - **KinoGlitch (URPGlitch)**
  - URL: [Keijiro](https://github.com/keijiro/KinoGlitch)
  - 説明: Unity の URP 用に実装されたグリッチエフェクトパッケージです。  
    このパッケージは、ゲーム中に映像にグリッチ効果を加えるために使用されています。

- **OpenCV (cv2)**
  - URL: [https://opencv.org/](https://opencv.org/)
  - 説明: 画像処理およびコンピュータビジョンライブラリ。動画キャプチャや画像の前処理に利用しています。

- **pyzbar**
  - URL: [https://github.com/NaturalHistoryMuseum/pyzbar](https://github.com/NaturalHistoryMuseum/pyzbar)
  - 説明: QRコードやバーコードのデコードライブラリとして使用しています。

- **NumPy**
  - URL: [https://numpy.org/](https://numpy.org/)
  - 説明: 数値計算ライブラリ。画像データや座標処理など、数値演算に利用しています。

- **cvzone (HandTrackingModule)**
  - URL: [https://github.com/cvzone/cvzone](https://github.com/cvzone/cvzone)
  - 説明: OpenCV の機能を拡張する便利なモジュール群。手の検出（HandDetector）などの機能を提供します。
---