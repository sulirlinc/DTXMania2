﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX.Direct2D1;
using FDK;
using DTXMania.設定;

namespace DTXMania.ステージ.オプション設定
{
    class オプション設定ステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            入力割り当て,
			曲読み込みフォルダ割り当て,
			再起動,
			フェードアウト,
            確定,
            キャンセル,
        }
        public フェーズ 現在のフェーズ { get; protected set; }


        public オプション設定ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._舞台画像 = new 舞台画像() );
                this.子Activityを追加する( this._パネルリスト = new パネルリスト() );
                //this.子を追加する( this._ルートパネルフォルダ = new パネル_フォルダ( "Root", null, null ) ); --> 活性化のたびに、子パネルとまとめて動的に追加する。
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.現在のフェーズ = フェーズ.フェードイン;
                this._初めての進行描画 = true;

                // パネルフォルダツリーを構築する。

                var user = App.ユーザ管理.ログオン中のユーザ;
                this._ルートパネルフォルダ = new パネル_フォルダ( "Root", null );

                #region "「自動演奏」フォルダ"
                //----------------
                {
                    var 自動演奏フォルダ = new パネル_フォルダ(

                        パネル名:
                            "自動演奏",

                        親パネル:
                            this._ルートパネルフォルダ,

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.子のパネルを選択する();
                                this._パネルリスト.フェードインを開始する();
                            } );

                    this._ルートパネルフォルダ.子パネルリスト.Add( 自動演奏フォルダ );

                    // 子フォルダツリーの構築

                    #region "「すべてON/OFF」パネル "
                    //----------------
                    自動演奏フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "すべてON/OFF",

                            値の変更処理:
                                ( panel ) => {
                                    bool 設定値 = !( this._パネル_自動演奏_ONOFFトグルリスト[ 0 ].ONである );  // 最初の項目値の反対にそろえる
                                    foreach( var typePanel in this._パネル_自動演奏_ONOFFトグルリスト )
                                    {
                                        if( typePanel.ONである != 設定値 )    // 設定値と異なるなら
                                            typePanel.確定キーが入力された(); // ON/OFF反転
                                    }
                                }
                        ) );
                    //----------------
                    #endregion
                    #region " 各パッドのON/OFFパネル "
                    //----------------
                    this._パネル_自動演奏_ONOFFトグルリスト = new List<パネル_ONOFFトグル>();

                    foreach( AutoPlay種別 apType in Enum.GetValues( typeof( AutoPlay種別 ) ) )
                    {
                        if( apType == AutoPlay種別.Unknown )
                            continue;

                        var typePanel = new パネル_ONOFFトグル(

                            パネル名:
                                apType.ToString(),

                            初期状態はON:
                                ( user.AutoPlay[ apType ] ),

                            値の変更処理:
                                ( panel ) => {
                                    user.AutoPlay[ apType ] = ( (パネル_ONOFFトグル) panel ).ONである;
                                }
                        );

                        自動演奏フォルダ.子パネルリスト.Add( typePanel );

                        this._パネル_自動演奏_ONOFFトグルリスト.Add( typePanel );
                    }
                    //----------------
                    #endregion
                    #region "「設定完了（戻る）」システムボタン
                    //----------------
                    自動演奏フォルダ.子パネルリスト.Add(

                        new パネル_システムボタン(

                            パネル名:
                                "設定完了（戻る）",

                            値の変更処理:
                                ( panel ) => {
                                    this._パネルリスト.親のパネルを選択する();
                                    this._パネルリスト.フェードインを開始する();
                                }
                        ) );
                    //----------------
                    #endregion

                    自動演奏フォルダ.子パネルリスト.SelectFirst();
                }
                //----------------
                #endregion

                #region "「画面モード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "画面モード",

                        選択肢初期値リスト:
                            new[] { "ウィンドウ", "全画面" },

                        初期選択肢番号:
                            ( user.全画面モードである ) ? 1 : 0,

                        値の変更処理:
                            ( panel ) => {
                                user.全画面モードである = ( 1 == ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 );
                                App.全画面モード = user.全画面モードである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「演奏モード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "演奏モード",

                        選択肢初期値リスト:
                            new[] { "BASIC", "EXPERT" },

                        初期選択肢番号:
                            (int) user.演奏モード,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏モード = (PlayMode) ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号;
                                user.ドラムチッププロパティ管理.反映する( user.演奏モード );
                            }
                    ) );
                //----------------
                #endregion
                #region "「譜面スピード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(
                    new パネル_譜面スピード( "譜面スピード" ) );
                //----------------
                #endregion
                #region "「演奏中の動画表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "演奏中の動画表示",

                        初期状態はON:
                            user.演奏中に動画を表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏中に動画を表示する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「シンバルフリー」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "シンバルフリー",

                        初期状態はON:
                            user.シンバルフリーモードである,

                        値の変更処理:
                            ( panel ) => {
                                user.シンバルフリーモードである = ( (パネル_ONOFFトグル) panel ).ONである;
                                user.ドラムチッププロパティ管理.反映する( ( user.シンバルフリーモードである ) ? 入力グループプリセット種別.シンバルフリー : 入力グループプリセット種別.基本形 );
                            }
                    ) );
                //----------------
                #endregion
                #region "「Rideの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Rideの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Rideは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                    Chinaは左 = user.表示レーンの左右.Chinaは左,
                                    Splashは左 = user.表示レーンの左右.Splashは左,
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「Chinaの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Chinaの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Chinaは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = user.表示レーンの左右.Rideは左,
                                    Chinaは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                    Splashは左 = user.表示レーンの左右.Splashは左,
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「Splashの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Splashの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Splashは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = user.表示レーンの左右.Rideは左,
                                    Chinaは左 = user.表示レーンの左右.Splashは左,
                                    Splashは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「ドラムサウンド」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "ドラムサウンド",

                        初期状態はON:
                            user.ドラムの音を発声する,

                        値の変更処理:
                            ( panel ) => {
                                user.ドラムの音を発声する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「レーン配置」リスト "
                //----------------
                {
                    var 選択肢リスト = 演奏.レーンフレーム.レーン配置リスト.Keys.ToList();

                    this._ルートパネルフォルダ.子パネルリスト.Add(

                        new パネル_文字列リスト(

                            パネル名:
                                "レーン配置",

                            選択肢初期値リスト:
                                選択肢リスト,

                            初期選択肢番号:
                                ( 選択肢リスト.Contains( user.レーン配置 ) ) ? 選択肢リスト.IndexOf( user.レーン配置 ) : 0,

                            値の変更処理:
                                ( panel ) => {
                                    user.レーン配置 = 選択肢リスト[ ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 ];
                                }
                        ) );
                }
                //----------------
                #endregion
                #region "「レーンの透明度」数値ボックス "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_整数(

                        パネル名:
                            "レーンの透明度",

                        最小値:
                            0,

                        最大値:
                            100,

                        初期値:
                            user.レーンの透明度,

                        増加減単位値:
                            5,

                        単位:
                            "%",

                        値の変更処理:
                            ( panel ) => {
                                user.レーンの透明度 = ( (パネル_整数) panel ).現在の値;
                            }
                    ) );
                //----------------
                #endregion

                #region "「入力発声スレッドのスリープ量」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "入力発声スレッドのスリープ量",

                        選択肢初期値リスト:
                            new[] { "1 ms", "2 ms", "3 ms", "4 ms", "5 ms", "6 ms", "7 ms", "8 ms", "9 ms", "10 ms" },

                        初期選択肢番号:
                            ( App.システム設定.入力発声スレッドのスリープ量ms - 1 ),   // 1～10 → 0～9

                        値の変更処理:
                            ( panel ) => {
                                App.システム設定.入力発声スレッドのスリープ量ms = ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 + 1;  // 0～9 → 1～10
                            }
                    ) );
                //----------------
                #endregion
                #region "「入力割り当て」パネル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル(

                        パネル名:
                            "入力割り当て",

                        値の変更処理:
                            ( panel ) => {
                                this.現在のフェーズ = フェーズ.入力割り当て;
                            },

                        ヘッダ色:
                            パネル.ヘッダ色種別.赤
                    ) );
                //----------------
                #endregion
                #region "「曲読み込みフォルダ」パネル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル(

                        パネル名:
                            "曲読み込みフォルダ",

                        値の変更処理:
                            ( panel ) => {
                                this.現在のフェーズ = フェーズ.曲読み込みフォルダ割り当て;
                            },

                        ヘッダ色:
                            パネル.ヘッダ色種別.赤
                    ) );
                //----------------
                #endregion

                #region "「初期化」フォルダ "
                //----------------
                {
                    var 初期化フォルダ = new パネル_フォルダ(

                        パネル名:
                            "初期化",

                        親パネル:
                            this._ルートパネルフォルダ,

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.子のパネルを選択する();
                                this._パネルリスト.フェードインを開始する();
                            } );

                    this._ルートパネルフォルダ.子パネルリスト.Add( 初期化フォルダ );

                    // 子フォルダツリーの構築

                    #region "「戻る」システムボタン "
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル_システムボタン(

                            パネル名:
                                "戻る",

                            値の変更処理:
                                ( panel ) => {
                                    this._パネルリスト.親のパネルを選択する();
                                    this._パネルリスト.フェードインを開始する();
                                }
                        ) );
                    //----------------
                    #endregion

                    #region "「設定を初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "設定を初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    App.システム設定を初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion
                    #region "「曲DBを初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "曲DBを初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    App.曲データベースを初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion
                    #region "「ユーザDBを初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "ユーザDBを初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    App.ユーザデータベースを初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion

                    初期化フォルダ.子パネルリスト.SelectFirst();
                }
                //----------------
                #endregion

                #region "「設定完了」システムボタン "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_システムボタン(

                        パネル名:
                            "設定完了",

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.フェードアウトを開始する();
                                App.ステージ管理.アイキャッチを選択しクローズする( nameof( アイキャッチ.シャッター ) );
                                this.現在のフェーズ = フェーズ.フェードアウト;
                            }
                    ) );
                //----------------
                #endregion


                // 最後のパネルを選択。
                this._ルートパネルフォルダ.子パネルリスト.SelectLast();

                // ルートパネルフォルダを最初のツリーとして表示する。
                this._パネルリスト.パネルリストを登録する( this._ルートパネルフォルダ );

                // ルートパネルフォルダを活性化する。
                this._ルートパネルフォルダ.活性化する();


                App.システムサウンド.再生する( システムサウンド種別.オプション設定ステージ_開始音 );
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ルートパネルフォルダ.非活性化する();
                this._ルートパネルフォルダ = null;
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            // (1) 全フェーズ共通の進行描画。

            if( this._初めての進行描画 )
            {
                this._舞台画像.ぼかしと縮小を適用する( 0.5 );
                this._初めての進行描画 = false;
            }

            this._舞台画像.進行描画する( dc );
            this._パネルリスト.進行描画する( dc, 613f, 0f );


            // (2) フェーズ別の進行描画。

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.表示:
                    break;

                case フェーズ.入力割り当て:
                    using( var dlg = new 入力割り当てダイアログ() )
                        dlg.表示する();
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.曲読み込みフォルダ割り当て:
                    {
                        bool 変更された = false;
                        using( var dlg = new 曲読み込みフォルダ割り当てダイアログ() )
                            変更された = dlg.表示する();
                        this.現在のフェーズ = ( 変更された ) ? フェーズ.再起動 : フェーズ.表示;
                    }
                    break;

                case フェーズ.再起動:
                    break;

                case フェーズ.フェードアウト:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );
                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                        this.現在のフェーズ = フェーズ.確定;
                    break;

                case フェーズ.確定:
                case フェーズ.キャンセル:
                    break;
            }


            // (3) フェーズ別の入力。

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:

                    if( App.入力管理.キャンセルキーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.取消音 );

                        if( null == this._パネルリスト.現在のパネルフォルダ.親パネル )
                        {
                            this._パネルリスト.フェードアウトを開始する();
                            App.ステージ管理.アイキャッチを選択しクローズする( nameof( アイキャッチ.半回転黒フェード ) );
                            this.現在のフェーズ = フェーズ.フェードアウト;
                        }
                        else
                        {
                            this._パネルリスト.親のパネルを選択する();
                            this._パネルリスト.フェードインを開始する();
                        }
                    }
                    else if( App.入力管理.上移動キーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.カーソル移動音 );
                        this._パネルリスト.前のパネルを選択する();
                    }
                    else if( App.入力管理.下移動キーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.カーソル移動音 );
                        this._パネルリスト.次のパネルを選択する();
                    }
                    else if( App.入力管理.左移動キーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._パネルリスト.現在選択中のパネル.左移動キーが入力された();
                    }
                    else if( App.入力管理.右移動キーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._パネルリスト.現在選択中のパネル.右移動キーが入力された();
                    }
                    else if( App.入力管理.確定キーが入力された() )
                    {
                        App.システムサウンド.再生する( 設定.システムサウンド種別.変更音 );
                        this._パネルリスト.現在選択中のパネル.確定キーが入力された();
                    }
                    break;
            }
        }


        private bool _初めての進行描画 = true;

        private 舞台画像 _舞台画像 = null;

        private パネルリスト _パネルリスト = null;

        private パネル_フォルダ _ルートパネルフォルダ = null;


        // 以下、コード内で参照が必要になるパネルのホルダ。
        private List<パネル_ONOFFトグル> _パネル_自動演奏_ONOFFトグルリスト = null;
    }
}
