﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using DTXMania2.曲;
using DTXMania2.演奏;

namespace DTXMania2.選曲
{
    class 曲ステータスパネル : IDisposable
    {

        // 生成と終了


        public 曲ステータスパネル()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._背景画像 = new 画像D2D( @"$(Images)\SelectStage\ScoreStatusPanel.png" );

            // 色ブラシを作成。

            var d2ddc = Global.GraphicResources.既定のD2D1DeviceContext;

            this._色 = new Dictionary<表示レーン種別, SolidColorBrush>() {
                { 表示レーン種別.LeftCymbal,   new SolidColorBrush( d2ddc, new Color4( 0xff7b1fff ) ) },
                { 表示レーン種別.HiHat,        new SolidColorBrush( d2ddc, new Color4( 0xffffc06a ) ) },
                { 表示レーン種別.Foot,         new SolidColorBrush( d2ddc, new Color4( 0xffed4bff ) ) },
                { 表示レーン種別.Snare,        new SolidColorBrush( d2ddc, new Color4( 0xff16fefc ) ) },
                { 表示レーン種別.Tom1,         new SolidColorBrush( d2ddc, new Color4( 0xff00ff02 ) ) },
                { 表示レーン種別.Bass,         new SolidColorBrush( d2ddc, new Color4( 0xffff819b ) ) },
                { 表示レーン種別.Tom2,         new SolidColorBrush( d2ddc, new Color4( 0xff0000ff ) ) },
                { 表示レーン種別.Tom3,         new SolidColorBrush( d2ddc, new Color4( 0xff19a9ff ) ) },
                { 表示レーン種別.RightCymbal,  new SolidColorBrush( d2ddc, new Color4( 0xffffb55e ) ) },
            };
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            foreach( var kvp in this._色 )
                kvp.Value.Dispose();

            this._背景画像.Dispose();
        }



        // 進行と描画


        public void 進行描画する( DeviceContext d2ddc, Node フォーカスノード )
        {
            var 領域dpx = new RectangleF( 320f, 532f, 239f, 505f );

            this._背景画像.描画する( d2ddc, 領域dpx.X, 領域dpx.Y );

            if( !( フォーカスノード is SongNode snode ) || snode.曲.フォーカス譜面 is null )
            {
                // 現状、BPMを表示できるノードは SongNode のみ。
                // SongNode 以外は背景の描画だけで終わり。
                return;
            }

            #region " フォーカスノードが変更されていれば更新する。"
            //----------------
            if( フォーカスノード != this._現在表示しているノード )
            {
                this._現在表示しているノード = フォーカスノード;
            }
            //----------------
            #endregion

            #region " Total Notes を描画する。"
            //----------------
            {
                var map = snode.曲.フォーカス譜面.レーン別ノート数;

                if( null != map )
                {
                    const float Yオフセット = +2f;

                    var Xオフセット = new Dictionary<表示レーン種別, float>() {
                        { 表示レーン種別.LeftCymbal,   + 70f },
                        { 表示レーン種別.HiHat,        + 88f },
                        { 表示レーン種別.Foot,         +106f },
                        { 表示レーン種別.Snare,        +124f },
                        { 表示レーン種別.Tom1,         +142f },
                        { 表示レーン種別.Bass,         +160f },
                        { 表示レーン種別.Tom2,         +178f },
                        { 表示レーン種別.Tom3,         +196f },
                        { 表示レーン種別.RightCymbal,  +214f },
                    };

                    foreach( 表示レーン種別? lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                    {
                        if( lane.HasValue && map.ContainsKey( lane.Value ) )
                        {
                            var rc = new RectangleF( 領域dpx.X + Xオフセット[ lane.Value ], 領域dpx.Y + Yオフセット, 6f, 405f );
                            rc.Top = rc.Bottom - ( rc.Height * Math.Min( map[ lane.Value ], 250 ) / 250f );

                            d2ddc.FillRectangle( rc, this._色[ lane.Value ] );
                        }
                    }
                }
            }
            //----------------
            #endregion
        }



        // ローカル


        private readonly 画像D2D _背景画像;

        private readonly Dictionary<表示レーン種別, SolidColorBrush> _色;

        private Node? _現在表示しているノード = null;
    }
}
