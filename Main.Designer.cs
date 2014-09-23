namespace FissureStationImport
{
    partial class Main
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
            this.btnFissure = new System.Windows.Forms.Button();
            this.btnNonFissure = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnFissure
            // 
            this.btnFissure.Location = new System.Drawing.Point(12, 12);
            this.btnFissure.Name = "btnFissure";
            this.btnFissure.Size = new System.Drawing.Size(198, 45);
            this.btnFissure.TabIndex = 0;
            this.btnFissure.Text = "Import Fissure Waypoints";
            this.btnFissure.UseVisualStyleBackColor = true;
            this.btnFissure.Click += new System.EventHandler(this.btnFissure_Click);
            // 
            // btnNonFissure
            // 
            this.btnNonFissure.Location = new System.Drawing.Point(12, 63);
            this.btnNonFissure.Name = "btnNonFissure";
            this.btnNonFissure.Size = new System.Drawing.Size(198, 45);
            this.btnNonFissure.TabIndex = 1;
            this.btnNonFissure.Text = "Import Non-Fissure Waypoints";
            this.btnNonFissure.UseVisualStyleBackColor = true;
            this.btnNonFissure.Click += new System.EventHandler(this.btnNonFissure_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(12, 165);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(198, 45);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "I\'m all done.";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(12, 114);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(198, 45);
            this.progress.TabIndex = 3;
            this.progress.Visible = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 223);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnNonFissure);
            this.Controls.Add(this.btnFissure);
            this.Name = "Main";
            this.Text = "Fissure Stations";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnFissure;
        private System.Windows.Forms.Button btnNonFissure;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progress;
    }
}

