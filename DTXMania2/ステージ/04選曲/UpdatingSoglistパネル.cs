﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DTXMania2.選曲
{
    class UpdatingSoglistパネル : IDisposable
    {

        // 生成と終了


        public UpdatingSoglistパネル()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._パネル画像 = new 画像( @"$(Images)\SelectStage\UpdatingSonglist.png" );
            this._明滅カウンタ = new LoopCounter( 0, 99, 10 );
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._パネル画像.Dispose();
        }



        // 進行と描画


        public void 進行描画する( float x, float y )
        {
            // 現行化タスクが停止していれば表示OFF。
            if( !Global.App.現行化.現行化中 )
                return; 

            float 不透明度 = (float) Math.Sin( Math.PI * this._明滅カウンタ.現在値 / 100.0 );
            this._パネル画像.描画する( x, y, 不透明度0to1: 不透明度 );
        }



        // ローカル


        private readonly 画像 _パネル画像;

        private readonly LoopCounter _明滅カウンタ;
    }
}
