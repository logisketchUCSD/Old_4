namespace InkForm
{
    partial class Sketcher
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveStrokeClassifierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openStrokeClassifierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.featurizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usingSoftmaxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalizedInARFFFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.featurizeGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawValuesPairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usingSoftmaxPairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.penToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabSketch = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonTop = new System.Windows.Forms.RadioButton();
            this.radioButtonTopComplete = new System.Windows.Forms.RadioButton();
            this.DisplayNoneRadioButton = new System.Windows.Forms.RadioButton();
            this.DisplayGroupingRadioButton = new System.Windows.Forms.RadioButton();
            this.DisplayClassificationRadioButton = new System.Windows.Forms.RadioButton();
            this.tabClassifier = new System.Windows.Forms.TabPage();
            this.clustersThroughZernikeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabSketch.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.featurizeToolStripMenuItem,
            this.featurizeGroupToolStripMenuItem,
            this.penToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(805, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSketchToolStripMenuItem,
            this.saveStrokeClassifierToolStripMenuItem,
            this.openStrokeClassifierToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openSketchToolStripMenuItem
            // 
            this.openSketchToolStripMenuItem.Name = "openSketchToolStripMenuItem";
            this.openSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openSketchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openSketchToolStripMenuItem.Text = "Open Sketch";
            this.openSketchToolStripMenuItem.Click += new System.EventHandler(this.openSketchToolStripMenuItem_Click);
            // 
            // saveStrokeClassifierToolStripMenuItem
            // 
            this.saveStrokeClassifierToolStripMenuItem.Name = "saveStrokeClassifierToolStripMenuItem";
            this.saveStrokeClassifierToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveStrokeClassifierToolStripMenuItem.Text = "Save Stroke Classifier";
            this.saveStrokeClassifierToolStripMenuItem.Click += new System.EventHandler(this.saveStrokeClassifierToolStripMenuItem_Click);
            // 
            // openStrokeClassifierToolStripMenuItem
            // 
            this.openStrokeClassifierToolStripMenuItem.Name = "openStrokeClassifierToolStripMenuItem";
            this.openStrokeClassifierToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openStrokeClassifierToolStripMenuItem.Text = "Open Stroke Classifier";
            this.openStrokeClassifierToolStripMenuItem.Click += new System.EventHandler(this.openStrokeClassifierToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // featurizeToolStripMenuItem
            // 
            this.featurizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rawValuesToolStripMenuItem,
            this.usingSoftmaxToolStripMenuItem,
            this.normalizedInARFFFormatToolStripMenuItem,
            this.clustersThroughZernikeToolStripMenuItem});
            this.featurizeToolStripMenuItem.Name = "featurizeToolStripMenuItem";
            this.featurizeToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.featurizeToolStripMenuItem.Text = "Featurize";
            // 
            // rawValuesToolStripMenuItem
            // 
            this.rawValuesToolStripMenuItem.Name = "rawValuesToolStripMenuItem";
            this.rawValuesToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.rawValuesToolStripMenuItem.Text = "Raw Values";
            this.rawValuesToolStripMenuItem.Click += new System.EventHandler(this.rawValuesSingleToolStripMenuItem_Click);
            // 
            // usingSoftmaxToolStripMenuItem
            // 
            this.usingSoftmaxToolStripMenuItem.Name = "usingSoftmaxToolStripMenuItem";
            this.usingSoftmaxToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.usingSoftmaxToolStripMenuItem.Text = "Using Softmax (Normalized)";
            this.usingSoftmaxToolStripMenuItem.Click += new System.EventHandler(this.usingSoftmaxSingleToolStripMenuItem_Click);
            // 
            // normalizedInARFFFormatToolStripMenuItem
            // 
            this.normalizedInARFFFormatToolStripMenuItem.Name = "normalizedInARFFFormatToolStripMenuItem";
            this.normalizedInARFFFormatToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.normalizedInARFFFormatToolStripMenuItem.Text = "Normalized in ARFF format";
            this.normalizedInARFFFormatToolStripMenuItem.Click += new System.EventHandler(this.normalizedInARFFFormatToolStripMenuItem_Click);
            // 
            // featurizeGroupToolStripMenuItem
            // 
            this.featurizeGroupToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rawValuesPairToolStripMenuItem,
            this.usingSoftmaxPairToolStripMenuItem});
            this.featurizeGroupToolStripMenuItem.Name = "featurizeGroupToolStripMenuItem";
            this.featurizeGroupToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
            this.featurizeGroupToolStripMenuItem.Text = "Featurize (Group)";
            // 
            // rawValuesPairToolStripMenuItem
            // 
            this.rawValuesPairToolStripMenuItem.Name = "rawValuesPairToolStripMenuItem";
            this.rawValuesPairToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rawValuesPairToolStripMenuItem.Text = "Raw Values";
            this.rawValuesPairToolStripMenuItem.Click += new System.EventHandler(this.rawValuesPairToolStripMenuItem_Click);
            // 
            // usingSoftmaxPairToolStripMenuItem
            // 
            this.usingSoftmaxPairToolStripMenuItem.Name = "usingSoftmaxPairToolStripMenuItem";
            this.usingSoftmaxPairToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.usingSoftmaxPairToolStripMenuItem.Text = "Using Softmax";
            this.usingSoftmaxPairToolStripMenuItem.Click += new System.EventHandler(this.usingSoftmaxPairToolStripMenuItem_Click);
            // 
            // penToolStripMenuItem
            // 
            this.penToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawToolStripMenuItem,
            this.selectToolStripMenuItem});
            this.penToolStripMenuItem.Name = "penToolStripMenuItem";
            this.penToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.penToolStripMenuItem.Text = "Pen";
            // 
            // drawToolStripMenuItem
            // 
            this.drawToolStripMenuItem.CheckOnClick = true;
            this.drawToolStripMenuItem.Name = "drawToolStripMenuItem";
            this.drawToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.drawToolStripMenuItem.Text = "Draw";
            this.drawToolStripMenuItem.Click += new System.EventHandler(this.drawToolStripMenuItem_Click);
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.Checked = true;
            this.selectToolStripMenuItem.CheckOnClick = true;
            this.selectToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.selectToolStripMenuItem.Text = "Select";
            this.selectToolStripMenuItem.Click += new System.EventHandler(this.selectToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 532);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(805, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabSketch);
            this.tabControl1.Controls.Add(this.tabClassifier);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(805, 508);
            this.tabControl1.TabIndex = 2;
            // 
            // tabSketch
            // 
            this.tabSketch.Controls.Add(this.groupBox1);
            this.tabSketch.Location = new System.Drawing.Point(4, 22);
            this.tabSketch.Name = "tabSketch";
            this.tabSketch.Padding = new System.Windows.Forms.Padding(3);
            this.tabSketch.Size = new System.Drawing.Size(797, 482);
            this.tabSketch.TabIndex = 0;
            this.tabSketch.Text = "Sketch";
            this.tabSketch.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButtonTop);
            this.groupBox1.Controls.Add(this.radioButtonTopComplete);
            this.groupBox1.Controls.Add(this.DisplayNoneRadioButton);
            this.groupBox1.Controls.Add(this.DisplayGroupingRadioButton);
            this.groupBox1.Controls.Add(this.DisplayClassificationRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(659, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(130, 137);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Display Options";
            // 
            // radioButtonTop
            // 
            this.radioButtonTop.AutoSize = true;
            this.radioButtonTop.Location = new System.Drawing.Point(6, 111);
            this.radioButtonTop.Name = "radioButtonTop";
            this.radioButtonTop.Size = new System.Drawing.Size(44, 17);
            this.radioButtonTop.TabIndex = 4;
            this.radioButtonTop.TabStop = true;
            this.radioButtonTop.Text = "Top";
            this.radioButtonTop.UseVisualStyleBackColor = true;
            this.radioButtonTop.CheckedChanged += new System.EventHandler(this.radioButtonTop_CheckedChanged);
            // 
            // radioButtonTopComplete
            // 
            this.radioButtonTopComplete.AutoSize = true;
            this.radioButtonTopComplete.Location = new System.Drawing.Point(6, 88);
            this.radioButtonTopComplete.Name = "radioButtonTopComplete";
            this.radioButtonTopComplete.Size = new System.Drawing.Size(91, 17);
            this.radioButtonTopComplete.TabIndex = 3;
            this.radioButtonTopComplete.TabStop = true;
            this.radioButtonTopComplete.Text = "Top Complete";
            this.radioButtonTopComplete.UseVisualStyleBackColor = true;
            this.radioButtonTopComplete.CheckedChanged += new System.EventHandler(this.radioButtonTopComplete_CheckedChanged);
            // 
            // DisplayNoneRadioButton
            // 
            this.DisplayNoneRadioButton.AutoSize = true;
            this.DisplayNoneRadioButton.Checked = true;
            this.DisplayNoneRadioButton.Location = new System.Drawing.Point(6, 19);
            this.DisplayNoneRadioButton.Name = "DisplayNoneRadioButton";
            this.DisplayNoneRadioButton.Size = new System.Drawing.Size(103, 17);
            this.DisplayNoneRadioButton.TabIndex = 0;
            this.DisplayNoneRadioButton.TabStop = true;
            this.DisplayNoneRadioButton.Text = "No Color Display";
            this.DisplayNoneRadioButton.UseVisualStyleBackColor = true;
            this.DisplayNoneRadioButton.CheckedChanged += new System.EventHandler(this.DisplayNoneRadioButton_CheckedChanged);
            // 
            // DisplayGroupingRadioButton
            // 
            this.DisplayGroupingRadioButton.AutoSize = true;
            this.DisplayGroupingRadioButton.Location = new System.Drawing.Point(6, 65);
            this.DisplayGroupingRadioButton.Name = "DisplayGroupingRadioButton";
            this.DisplayGroupingRadioButton.Size = new System.Drawing.Size(105, 17);
            this.DisplayGroupingRadioButton.TabIndex = 2;
            this.DisplayGroupingRadioButton.Text = "Display Grouping";
            this.DisplayGroupingRadioButton.UseVisualStyleBackColor = true;
            this.DisplayGroupingRadioButton.CheckedChanged += new System.EventHandler(this.DisplayGroupingRadioButton_CheckedChanged);
            // 
            // DisplayClassificationRadioButton
            // 
            this.DisplayClassificationRadioButton.AutoSize = true;
            this.DisplayClassificationRadioButton.Location = new System.Drawing.Point(6, 42);
            this.DisplayClassificationRadioButton.Name = "DisplayClassificationRadioButton";
            this.DisplayClassificationRadioButton.Size = new System.Drawing.Size(123, 17);
            this.DisplayClassificationRadioButton.TabIndex = 1;
            this.DisplayClassificationRadioButton.Text = "Display Classification";
            this.DisplayClassificationRadioButton.UseVisualStyleBackColor = true;
            this.DisplayClassificationRadioButton.CheckedChanged += new System.EventHandler(this.DisplayClassificationRadioButton_CheckedChanged);
            // 
            // tabClassifier
            // 
            this.tabClassifier.Location = new System.Drawing.Point(4, 22);
            this.tabClassifier.Name = "tabClassifier";
            this.tabClassifier.Padding = new System.Windows.Forms.Padding(3);
            this.tabClassifier.Size = new System.Drawing.Size(797, 482);
            this.tabClassifier.TabIndex = 1;
            this.tabClassifier.Text = "Classifer";
            this.tabClassifier.UseVisualStyleBackColor = true;
            // 
            // clustersThroughZernikeToolStripMenuItem
            // 
            this.clustersThroughZernikeToolStripMenuItem.Name = "clustersThroughZernikeToolStripMenuItem";
            this.clustersThroughZernikeToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.clustersThroughZernikeToolStripMenuItem.Text = "Clusters through Zernike";
            //this.clustersThroughZernikeToolStripMenuItem.Click += new System.EventHandler(this.clustersThroughZernikeToolStripMenuItem_Click);
            // 
            // Sketcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 554);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Sketcher";
            this.Text = "Sketcher";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabSketch.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSketch;
        private System.Windows.Forms.TabPage tabClassifier;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveStrokeClassifierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openStrokeClassifierToolStripMenuItem;
        private System.Windows.Forms.RadioButton DisplayNoneRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton DisplayGroupingRadioButton;
        private System.Windows.Forms.RadioButton DisplayClassificationRadioButton;
        private System.Windows.Forms.ToolStripMenuItem featurizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usingSoftmaxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem featurizeGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawValuesPairToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usingSoftmaxPairToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButtonTop;
        private System.Windows.Forms.RadioButton radioButtonTopComplete;
        private System.Windows.Forms.ToolStripMenuItem penToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalizedInARFFFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clustersThroughZernikeToolStripMenuItem;
    }
}

