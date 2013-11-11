namespace InkForm
{
    partial class ImageResultSubForm
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
            this.panelInk = new System.Windows.Forms.Panel();
            this.labelNames = new System.Windows.Forms.Label();
            this.labelHausdorff = new System.Windows.Forms.Label();
            this.labelModifiedHausdorff = new System.Windows.Forms.Label();
            this.labelTanimoto = new System.Windows.Forms.Label();
            this.labelYule = new System.Windows.Forms.Label();
            this.labelUserName = new System.Windows.Forms.Label();
            this.labelCompleteness = new System.Windows.Forms.Label();
            this.labelPlatform = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelInk
            // 
            this.panelInk.BackColor = System.Drawing.Color.White;
            this.panelInk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelInk.Location = new System.Drawing.Point(2, 2);
            this.panelInk.Name = "panelInk";
            this.panelInk.Size = new System.Drawing.Size(200, 200);
            this.panelInk.TabIndex = 0;
            // 
            // labelNames
            // 
            this.labelNames.AutoSize = true;
            this.labelNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNames.Location = new System.Drawing.Point(4, 15);
            this.labelNames.Name = "labelNames";
            this.labelNames.Size = new System.Drawing.Size(84, 15);
            this.labelNames.TabIndex = 1;
            this.labelNames.Text = "Class: Symbol";
            // 
            // labelHausdorff
            // 
            this.labelHausdorff.AutoSize = true;
            this.labelHausdorff.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHausdorff.Location = new System.Drawing.Point(42, 15);
            this.labelHausdorff.Name = "labelHausdorff";
            this.labelHausdorff.Size = new System.Drawing.Size(66, 15);
            this.labelHausdorff.TabIndex = 2;
            this.labelHausdorff.Text = "Hausdorff: ";
            // 
            // labelModifiedHausdorff
            // 
            this.labelModifiedHausdorff.AutoSize = true;
            this.labelModifiedHausdorff.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelModifiedHausdorff.Location = new System.Drawing.Point(11, 37);
            this.labelModifiedHausdorff.Name = "labelModifiedHausdorff";
            this.labelModifiedHausdorff.Size = new System.Drawing.Size(97, 15);
            this.labelModifiedHausdorff.TabIndex = 3;
            this.labelModifiedHausdorff.Text = "Mod. Hausdorff: ";
            // 
            // labelTanimoto
            // 
            this.labelTanimoto.AutoSize = true;
            this.labelTanimoto.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTanimoto.Location = new System.Drawing.Point(12, 59);
            this.labelTanimoto.Name = "labelTanimoto";
            this.labelTanimoto.Size = new System.Drawing.Size(96, 15);
            this.labelTanimoto.TabIndex = 4;
            this.labelTanimoto.Text = "Tanimoto Coeff: ";
            // 
            // labelYule
            // 
            this.labelYule.AutoSize = true;
            this.labelYule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelYule.Location = new System.Drawing.Point(38, 81);
            this.labelYule.Name = "labelYule";
            this.labelYule.Size = new System.Drawing.Size(68, 15);
            this.labelYule.TabIndex = 5;
            this.labelYule.Text = "Yule Coeff: ";
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUserName.Location = new System.Drawing.Point(133, 15);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(39, 15);
            this.labelUserName.TabIndex = 6;
            this.labelUserName.Text = "User: ";
            // 
            // labelCompleteness
            // 
            this.labelCompleteness.AutoSize = true;
            this.labelCompleteness.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCompleteness.Location = new System.Drawing.Point(4, 37);
            this.labelCompleteness.Name = "labelCompleteness";
            this.labelCompleteness.Size = new System.Drawing.Size(147, 15);
            this.labelCompleteness.TabIndex = 7;
            this.labelCompleteness.Text = "Template Completeness: ";
            // 
            // labelPlatform
            // 
            this.labelPlatform.AutoSize = true;
            this.labelPlatform.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPlatform.Location = new System.Drawing.Point(4, 59);
            this.labelPlatform.Name = "labelPlatform";
            this.labelPlatform.Size = new System.Drawing.Size(114, 15);
            this.labelPlatform.TabIndex = 8;
            this.labelPlatform.Text = "Template Platform: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(202, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "Template";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(202, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "Image";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelYule);
            this.groupBox1.Controls.Add(this.labelTanimoto);
            this.groupBox1.Controls.Add(this.labelModifiedHausdorff);
            this.groupBox1.Controls.Add(this.labelHausdorff);
            this.groupBox1.Location = new System.Drawing.Point(286, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 105);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scores";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelPlatform);
            this.groupBox2.Controls.Add(this.labelCompleteness);
            this.groupBox2.Controls.Add(this.labelUserName);
            this.groupBox2.Controls.Add(this.labelNames);
            this.groupBox2.Location = new System.Drawing.Point(208, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(252, 85);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Template Information";
            // 
            // ImageResultSubForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 206);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelInk);
            this.Name = "ImageResultSubForm";
            this.Text = "ImageResultSubForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelInk;
        private System.Windows.Forms.Label labelNames;
        private System.Windows.Forms.Label labelHausdorff;
        private System.Windows.Forms.Label labelModifiedHausdorff;
        private System.Windows.Forms.Label labelTanimoto;
        private System.Windows.Forms.Label labelYule;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.Label labelCompleteness;
        private System.Windows.Forms.Label labelPlatform;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}