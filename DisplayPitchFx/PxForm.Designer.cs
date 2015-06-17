namespace DisplayPitchFx
{
   partial class FrmPitchFx
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.dgvMain = new System.Windows.Forms.DataGridView();
         this.dtpSince = new System.Windows.Forms.DateTimePicker();
         this.dtpUntil = new System.Windows.Forms.DateTimePicker();
         this.btnGetData = new System.Windows.Forms.Button();
         this.lblStatus = new System.Windows.Forms.Label();
         this.lblSince = new System.Windows.Forms.Label();
         this.lblUntil = new System.Windows.Forms.Label();
         this.cbPitches = new System.Windows.Forms.ComboBox();
         this.lblPitchType = new System.Windows.Forms.Label();
         this.lblPitchMin = new System.Windows.Forms.Label();
         this.txtPitchMin = new System.Windows.Forms.TextBox();
         this.lblSort = new System.Windows.Forms.Label();
         this.cbSortColumns = new System.Windows.Forms.ComboBox();
         this.btnSort = new System.Windows.Forms.Button();
         this.cbOrderBy = new System.Windows.Forms.ComboBox();
         ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
         this.SuspendLayout();
         // 
         // dgvMain
         // 
         this.dgvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.dgvMain.Location = new System.Drawing.Point(13, 88);
         this.dgvMain.Name = "dgvMain";
         this.dgvMain.Size = new System.Drawing.Size(877, 369);
         this.dgvMain.TabIndex = 0;
         // 
         // dtpSince
         // 
         this.dtpSince.Location = new System.Drawing.Point(151, 12);
         this.dtpSince.Name = "dtpSince";
         this.dtpSince.Size = new System.Drawing.Size(200, 20);
         this.dtpSince.TabIndex = 1;
         this.dtpSince.Value = new System.DateTime(2015, 4, 1, 0, 0, 0, 0);
         // 
         // dtpUntil
         // 
         this.dtpUntil.Location = new System.Drawing.Point(151, 38);
         this.dtpUntil.Name = "dtpUntil";
         this.dtpUntil.Size = new System.Drawing.Size(200, 20);
         this.dtpUntil.TabIndex = 2;
         // 
         // btnGetData
         // 
         this.btnGetData.Location = new System.Drawing.Point(13, 12);
         this.btnGetData.Name = "btnGetData";
         this.btnGetData.Size = new System.Drawing.Size(75, 23);
         this.btnGetData.TabIndex = 3;
         this.btnGetData.Text = "Get Data";
         this.btnGetData.UseVisualStyleBackColor = true;
         this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
         // 
         // lblStatus
         // 
         this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lblStatus.AutoSize = true;
         this.lblStatus.Location = new System.Drawing.Point(568, 12);
         this.lblStatus.Name = "lblStatus";
         this.lblStatus.Size = new System.Drawing.Size(0, 13);
         this.lblStatus.TabIndex = 4;
         // 
         // lblSince
         // 
         this.lblSince.AutoSize = true;
         this.lblSince.Location = new System.Drawing.Point(100, 14);
         this.lblSince.Name = "lblSince";
         this.lblSince.Size = new System.Drawing.Size(37, 13);
         this.lblSince.TabIndex = 5;
         this.lblSince.Text = "Since:";
         // 
         // lblUntil
         // 
         this.lblUntil.AutoSize = true;
         this.lblUntil.Location = new System.Drawing.Point(106, 44);
         this.lblUntil.Name = "lblUntil";
         this.lblUntil.Size = new System.Drawing.Size(31, 13);
         this.lblUntil.TabIndex = 6;
         this.lblUntil.Text = "Until:";
         // 
         // cbPitches
         // 
         this.cbPitches.FormattingEnabled = true;
         this.cbPitches.Location = new System.Drawing.Point(412, 14);
         this.cbPitches.Name = "cbPitches";
         this.cbPitches.Size = new System.Drawing.Size(126, 21);
         this.cbPitches.TabIndex = 7;
         // 
         // lblPitchType
         // 
         this.lblPitchType.AutoSize = true;
         this.lblPitchType.Location = new System.Drawing.Point(367, 14);
         this.lblPitchType.Name = "lblPitchType";
         this.lblPitchType.Size = new System.Drawing.Size(34, 13);
         this.lblPitchType.TabIndex = 8;
         this.lblPitchType.Text = "Pitch:";
         // 
         // lblPitchMin
         // 
         this.lblPitchMin.AutoSize = true;
         this.lblPitchMin.Location = new System.Drawing.Point(367, 45);
         this.lblPitchMin.Name = "lblPitchMin";
         this.lblPitchMin.Size = new System.Drawing.Size(65, 13);
         this.lblPitchMin.TabIndex = 9;
         this.lblPitchMin.Text = "Pitches Min:";
         // 
         // txtPitchMin
         // 
         this.txtPitchMin.Location = new System.Drawing.Point(438, 41);
         this.txtPitchMin.Name = "txtPitchMin";
         this.txtPitchMin.Size = new System.Drawing.Size(100, 20);
         this.txtPitchMin.TabIndex = 10;
         this.txtPitchMin.Text = "500";
         // 
         // lblSort
         // 
         this.lblSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.lblSort.AutoSize = true;
         this.lblSort.Location = new System.Drawing.Point(658, 48);
         this.lblSort.Name = "lblSort";
         this.lblSort.Size = new System.Drawing.Size(29, 13);
         this.lblSort.TabIndex = 12;
         this.lblSort.Text = "Sort:";
         // 
         // cbSortColumns
         // 
         this.cbSortColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.cbSortColumns.FormattingEnabled = true;
         this.cbSortColumns.Location = new System.Drawing.Point(764, 45);
         this.cbSortColumns.Name = "cbSortColumns";
         this.cbSortColumns.Size = new System.Drawing.Size(126, 21);
         this.cbSortColumns.TabIndex = 11;
         // 
         // btnSort
         // 
         this.btnSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnSort.Location = new System.Drawing.Point(764, 14);
         this.btnSort.Name = "btnSort";
         this.btnSort.Size = new System.Drawing.Size(126, 25);
         this.btnSort.TabIndex = 13;
         this.btnSort.Text = "Sort By Column";
         this.btnSort.UseVisualStyleBackColor = true;
         this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
         // 
         // cbOrderBy
         // 
         this.cbOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.cbOrderBy.FormattingEnabled = true;
         this.cbOrderBy.Location = new System.Drawing.Point(686, 45);
         this.cbOrderBy.Name = "cbOrderBy";
         this.cbOrderBy.Size = new System.Drawing.Size(72, 21);
         this.cbOrderBy.TabIndex = 14;
         // 
         // FrmPitchFx
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(902, 469);
         this.Controls.Add(this.cbOrderBy);
         this.Controls.Add(this.btnSort);
         this.Controls.Add(this.lblSort);
         this.Controls.Add(this.cbSortColumns);
         this.Controls.Add(this.txtPitchMin);
         this.Controls.Add(this.lblPitchMin);
         this.Controls.Add(this.lblPitchType);
         this.Controls.Add(this.cbPitches);
         this.Controls.Add(this.lblUntil);
         this.Controls.Add(this.lblSince);
         this.Controls.Add(this.lblStatus);
         this.Controls.Add(this.btnGetData);
         this.Controls.Add(this.dtpUntil);
         this.Controls.Add(this.dtpSince);
         this.Controls.Add(this.dgvMain);
         this.Name = "FrmPitchFx";
         this.Text = "Pitch Fx";
         ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.DataGridView dgvMain;
      private System.Windows.Forms.DateTimePicker dtpSince;
      private System.Windows.Forms.DateTimePicker dtpUntil;
      private System.Windows.Forms.Button btnGetData;
      private System.Windows.Forms.Label lblStatus;
      private System.Windows.Forms.Label lblSince;
      private System.Windows.Forms.Label lblUntil;
      private System.Windows.Forms.ComboBox cbPitches;
      private System.Windows.Forms.Label lblPitchType;
      private System.Windows.Forms.Label lblPitchMin;
      private System.Windows.Forms.TextBox txtPitchMin;
      private System.Windows.Forms.Label lblSort;
      private System.Windows.Forms.ComboBox cbSortColumns;
      private System.Windows.Forms.Button btnSort;
      private System.Windows.Forms.ComboBox cbOrderBy;
   }
}

