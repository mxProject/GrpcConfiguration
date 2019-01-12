namespace Examples.GrpcConfiguration.Clients
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnCallRpc = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnCallRpc
            // 
            this.BtnCallRpc.Location = new System.Drawing.Point(28, 20);
            this.BtnCallRpc.Name = "BtnCallRpc";
            this.BtnCallRpc.Size = new System.Drawing.Size(92, 28);
            this.BtnCallRpc.TabIndex = 0;
            this.BtnCallRpc.Text = "call RPC";
            this.BtnCallRpc.UseVisualStyleBackColor = true;
            this.BtnCallRpc.Click += new System.EventHandler(this.BtnCallRpc_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 98);
            this.Controls.Add(this.BtnCallRpc);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GrpcConfiguration Sample";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnCallRpc;
    }
}

