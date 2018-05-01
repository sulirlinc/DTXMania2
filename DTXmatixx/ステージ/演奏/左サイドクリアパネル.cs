﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
    class 左サイドクリアパネル : Activity
    {
        public 描画可能テクスチャ クリアパネル
        {
            get;
            protected set;
        } = null;

        public 左サイドクリアパネル()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子を追加する( this._背景 = new 画像( @"$(System)images\演奏\左サイドクリアパネル.png" ) );
                this.子を追加する( this.クリアパネル = new 描画可能テクスチャ( new Size2F( 388, 990 ) ) );  // this._背景.サイズはまだ設定されていない。
            }
        }
        /// <summary>
        ///		クリアパネルに初期背景を上書きすることで、それまで描かれていた内容を消去する。
        /// </summary>
        public void クリアする()
        {
            this.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                dcp.Transform = Matrix3x2.Identity;  // 等倍描画(DPXtoDPX)
                dcp.PrimitiveBlend = PrimitiveBlend.Copy;
                dcp.DrawBitmap( this._背景.Bitmap, opacity: 1f, interpolationMode: InterpolationMode.Linear );
            } );
        }
        public void 描画する( DeviceContext1 dc )
        {
            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
                -グラフィックデバイス.Instance.設計画面サイズ.Width / 2f,
                +グラフィックデバイス.Instance.設計画面サイズ.Height / 2f,
                0f );

            var 変換行列 =
                Matrix.Scaling( this.クリアパネル.サイズ.Width, this.クリアパネル.サイズ.Height, 1f ) *
                Matrix.RotationY( MathUtil.DegreesToRadians( -48f ) ) *
                Matrix.Translation( 画面左上dpx.X + 230f, 画面左上dpx.Y - 530f, 0f );

            this.クリアパネル.描画する( 変換行列 );
        }

        private 画像 _背景 = null;
    }
}
