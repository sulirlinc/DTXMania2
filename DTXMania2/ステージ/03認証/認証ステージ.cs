﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using FDK;

namespace DTXMania2.認証
{
    /// <summary>
    ///		ユーザ選択画面。
    /// </summary>
    class 認証ステージ : IStage
    {

        // プロパティ


        public enum フェーズ
        {
            フェードイン,
            ユーザ選択,
            フェードアウト,
            完了,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.完了;

        public int 現在選択中のユーザ => this._ユーザリスト.選択中のユーザ;



        // 生成と終了


        public 認証ステージ()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._舞台画像 = new 舞台画像();
            this._ウィンドウ画像 = new 画像D2D( @"$(Images)\AuthStage\UserSelectFrame.png" );
            this._プレイヤーを選択してください = new 文字列画像D2D() {
                表示文字列 = Properties.Resources.TXT_プレイヤーを選択してください,
                フォントサイズpt = 30f,
                描画効果 = 文字列画像D2D.効果.ドロップシャドウ,
            };
            this._ユーザリスト = new ユーザリスト();
            this._システム情報 = new システム情報();

            Global.App.アイキャッチ管理.現在のアイキャッチ.オープンする();

            Global.App.システムサウンド.再生する( システムサウンド種別.認証ステージ_開始音 );
            Global.App.システムサウンド.再生する( システムサウンド種別.認証ステージ_ループBGM, ループ再生する: true );

            // 最初のフェーズへ。
            this.現在のフェーズ = フェーズ.フェードイン;
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._システム情報.Dispose();
            this._ユーザリスト.Dispose();
            this._プレイヤーを選択してください.Dispose();
            this._ウィンドウ画像.Dispose();
            this._舞台画像.Dispose();

            Global.App.システムサウンド.停止する( システムサウンド種別.認証ステージ_開始音 );
            Global.App.システムサウンド.停止する( システムサウンド種別.認証ステージ_ループBGM );
            //Global.App.システムサウンド.停止する( システムサウンド種別.認証ステージ_ログイン音 );    // --> 鳴りっぱなしでいい
        }



        // 進行と描画


        public void 進行する()
        {
            this._システム情報.FPSをカウントしプロパティを更新する();

            Global.App.ドラム入力.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                {
                    #region " 認証画面＆フェードインを描画する。"
                    //----------------
                    if( Global.App.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
                    {
                        // フェードイン描画が完了したらユーザ選択フェーズへ。
                        this.現在のフェーズ = フェーズ.ユーザ選択;
                    }
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.ユーザ選択:
                {
                    #region " 入力処理。"
                    //----------------
                    if( Global.App.ドラム入力.確定キーが入力された() )
                    {
                        #region " 確定 → フェードアウトフェーズへ "
                        //----------------
                        Global.App.システムサウンド.停止する( システムサウンド種別.認証ステージ_ループBGM );
                        Global.App.システムサウンド.再生する( システムサウンド種別.認証ステージ_ログイン音 );

                        Global.App.アイキャッチ管理.アイキャッチを選択しクローズする( nameof( 回転幕 ) );

                        this.現在のフェーズ = フェーズ.フェードアウト;
                        //----------------
                        #endregion
                    }
                    else if( Global.App.ドラム入力.キャンセルキーが入力された() )
                    {
                        #region " キャンセル → キャンセルフェーズへ "
                        //----------------
                        Global.App.システムサウンド.再生する( システムサウンド種別.取消音 );

                        this.現在のフェーズ = フェーズ.キャンセル;
                        //----------------
                        #endregion
                    }
                    else if( Global.App.ドラム入力.上移動キーが入力された() )
                    {
                        #region " 上移動 → 前のユーザを選択 "
                        //----------------
                        Global.App.システムサウンド.再生する( システムサウンド種別.カーソル移動音 );

                        this._ユーザリスト.前のユーザを選択する();
                        //----------------
                        #endregion
                    }
                    else if( Global.App.ドラム入力.下移動キーが入力された() )
                    {
                        #region " 下移動 → 次のユーザを選択 "
                        //----------------
                        Global.App.システムサウンド.再生する( システムサウンド種別.カーソル移動音 );

                        this._ユーザリスト.次のユーザを選択する();
                        //----------------
                        #endregion
                    }
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.フェードアウト:
                {
                    #region " 背景画面＆フェードアウトを描画する。"
                    //----------------
                    if( Global.App.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
                    {
                        // アイキャッチが完了したら完了フェーズへ。
                        this.現在のフェーズ = フェーズ.完了;
                    }
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.完了:
                case フェーズ.キャンセル:
                {
                    #region " 遷移終了。Appによるステージ遷移を待つ。"
                    //----------------
                    //----------------
                    #endregion

                    break;
                }
            }
        }

        public void 描画する()
        {
            this._システム情報.VPSをカウントする();

            var 描画領域 = new RectangleF( 566f, 60f, 784f, 943f );

            var d2ddc = Global.GraphicResources.既定のD2D1DeviceContext;
            d2ddc.Transform = Matrix3x2.Identity;

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                {
                    #region " 認証画面＆フェードインを描画する。"
                    //----------------
                    d2ddc.BeginDraw();

                    this._舞台画像.進行描画する( d2ddc, 黒幕付き: true );
                    this._ウィンドウ画像.描画する( d2ddc, 描画領域.X, 描画領域.Y );
                    this._プレイヤーを選択してください.描画する( d2ddc, 描画領域.X + 28f, 描画領域.Y + 45f );
                    this._ユーザリスト.進行描画する( d2ddc );
                    Global.App.アイキャッチ管理.現在のアイキャッチ.進行描画する( d2ddc );
                    this._システム情報.描画する( d2ddc );

                    d2ddc.EndDraw();
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.ユーザ選択:
                {
                    #region " 認証画面を描画する。"
                    //----------------
                    d2ddc.BeginDraw();

                    this._舞台画像.進行描画する( d2ddc, 黒幕付き: true );
                    this._ウィンドウ画像.描画する( d2ddc, 描画領域.X, 描画領域.Y );
                    this._プレイヤーを選択してください.描画する( d2ddc, 描画領域.X + 28f, 描画領域.Y + 45f );
                    this._ユーザリスト.進行描画する( d2ddc );
                    this._システム情報.描画する( d2ddc );

                    d2ddc.EndDraw();
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.フェードアウト:
                {
                    #region " 背景画面＆フェードアウトを描画する。"
                    //----------------
                    d2ddc.BeginDraw();

                    this._舞台画像.進行描画する( d2ddc, true );
                    Global.App.アイキャッチ管理.現在のアイキャッチ.進行描画する( d2ddc );
                    this._システム情報.描画する( d2ddc );

                    d2ddc.EndDraw();
                    //----------------
                    #endregion

                    break;
                }
                case フェーズ.完了:
                case フェーズ.キャンセル:
                {
                    break;
                }
            }
        }



        // ローカル


        private readonly 舞台画像 _舞台画像;

        private readonly 画像D2D _ウィンドウ画像;

        private readonly 文字列画像D2D _プレイヤーを選択してください;

        private readonly ユーザリスト _ユーザリスト;

        private readonly システム情報 _システム情報;
    }
}
