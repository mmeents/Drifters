namespace DriftersApp {
  partial class Form1 {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      components = new System.ComponentModel.Container();
      checkBox1 = new CheckBox();
      displayTimer = new System.Windows.Forms.Timer(components);
      tbOut = new TextBox();
      RunTimer = new System.Windows.Forms.Timer(components);
      tbCharacterName = new TextBox();
      tbSystemPrompt = new TextBox();
      tbMotives = new TextBox();
      tbObjectives = new TextBox();
      tbRank = new NumericUpDown();
      btnCancelChar = new Button();
      btnUpdateChar = new Button();
      tbIterations = new NumericUpDown();
      lbIterations = new Label();
      ((System.ComponentModel.ISupportInitialize)tbRank).BeginInit();
      ((System.ComponentModel.ISupportInitialize)tbIterations).BeginInit();
      SuspendLayout();
      // 
      // checkBox1
      // 
      checkBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      checkBox1.AutoSize = true;
      checkBox1.Location = new Point(761, 12);
      checkBox1.Name = "checkBox1";
      checkBox1.Size = new Size(89, 24);
      checkBox1.TabIndex = 0;
      checkBox1.Text = "Let it run";
      checkBox1.UseVisualStyleBackColor = true;
      checkBox1.CheckedChanged += checkBox1_CheckedChanged;
      // 
      // displayTimer
      // 
      displayTimer.Interval = 1000;
      displayTimer.Tick += displayTimer_Tick;
      // 
      // tbOut
      // 
      tbOut.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      tbOut.BackColor = SystemColors.InactiveBorder;
      tbOut.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
      tbOut.Location = new Point(12, 82);
      tbOut.Multiline = true;
      tbOut.Name = "tbOut";
      tbOut.ScrollBars = ScrollBars.Both;
      tbOut.Size = new Size(829, 695);
      tbOut.TabIndex = 1;
      // 
      // RunTimer
      // 
      RunTimer.Tick += RunTimer_Tick;
      // 
      // tbCharacterName
      // 
      tbCharacterName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      tbCharacterName.Location = new Point(310, 82);
      tbCharacterName.Name = "tbCharacterName";
      tbCharacterName.Size = new Size(531, 27);
      tbCharacterName.TabIndex = 2;
      // 
      // tbSystemPrompt
      // 
      tbSystemPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      tbSystemPrompt.Location = new Point(310, 148);
      tbSystemPrompt.Multiline = true;
      tbSystemPrompt.Name = "tbSystemPrompt";
      tbSystemPrompt.Size = new Size(531, 69);
      tbSystemPrompt.TabIndex = 3;
      // 
      // tbMotives
      // 
      tbMotives.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      tbMotives.Location = new Point(310, 298);
      tbMotives.Multiline = true;
      tbMotives.Name = "tbMotives";
      tbMotives.Size = new Size(531, 69);
      tbMotives.TabIndex = 4;
      // 
      // tbObjectives
      // 
      tbObjectives.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      tbObjectives.Location = new Point(310, 223);
      tbObjectives.Multiline = true;
      tbObjectives.Name = "tbObjectives";
      tbObjectives.Size = new Size(531, 69);
      tbObjectives.TabIndex = 5;
      // 
      // tbRank
      // 
      tbRank.Location = new Point(310, 115);
      tbRank.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
      tbRank.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
      tbRank.Name = "tbRank";
      tbRank.Size = new Size(69, 27);
      tbRank.TabIndex = 6;
      tbRank.Value = new decimal(new int[] { 1, 0, 0, 0 });
      // 
      // btnCancelChar
      // 
      btnCancelChar.ForeColor = SystemColors.ActiveCaptionText;
      btnCancelChar.Location = new Point(313, 387);
      btnCancelChar.Name = "btnCancelChar";
      btnCancelChar.Size = new Size(94, 29);
      btnCancelChar.TabIndex = 7;
      btnCancelChar.Text = "Cancel";
      btnCancelChar.UseVisualStyleBackColor = true;
      btnCancelChar.Click += btnCancelChar_Click;
      // 
      // btnUpdateChar
      // 
      btnUpdateChar.ForeColor = SystemColors.ActiveCaptionText;
      btnUpdateChar.Location = new Point(199, 387);
      btnUpdateChar.Name = "btnUpdateChar";
      btnUpdateChar.Size = new Size(94, 29);
      btnUpdateChar.TabIndex = 8;
      btnUpdateChar.Text = "Update";
      btnUpdateChar.UseVisualStyleBackColor = true;
      btnUpdateChar.Click += btnUpdateChar_Click;
      // 
      // tbIterations
      // 
      tbIterations.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      tbIterations.Location = new Point(659, 9);
      tbIterations.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
      tbIterations.Name = "tbIterations";
      tbIterations.Size = new Size(57, 27);
      tbIterations.TabIndex = 9;
      tbIterations.Value = new decimal(new int[] { 3, 0, 0, 0 });
      // 
      // lbIterations
      // 
      lbIterations.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      lbIterations.AutoSize = true;
      lbIterations.Location = new Point(582, 13);
      lbIterations.Name = "lbIterations";
      lbIterations.Size = new Size(71, 20);
      lbIterations.TabIndex = 10;
      lbIterations.Text = "iterations";
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF(8F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Color.FromArgb(0, 0, 12);
      ClientSize = new Size(853, 789);
      Controls.Add(lbIterations);
      Controls.Add(tbIterations);
      Controls.Add(btnUpdateChar);
      Controls.Add(btnCancelChar);
      Controls.Add(tbRank);
      Controls.Add(tbObjectives);
      Controls.Add(tbMotives);
      Controls.Add(tbSystemPrompt);
      Controls.Add(tbCharacterName);
      Controls.Add(tbOut);
      Controls.Add(checkBox1);
      ForeColor = SystemColors.Control;
      Name = "Form1";
      Text = "Drifers";
      Shown += Form1_Shown;
      ResizeEnd += Form1_ResizeEnd;
      MouseClick += Form1_MouseClick;
      ((System.ComponentModel.ISupportInitialize)tbRank).EndInit();
      ((System.ComponentModel.ISupportInitialize)tbIterations).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private CheckBox checkBox1;
    private System.Windows.Forms.Timer displayTimer;
    private TextBox tbOut;
    private System.Windows.Forms.Timer RunTimer;
    private TextBox tbCharacterName;
    private TextBox tbSystemPrompt;
    private TextBox tbMotives;
    private TextBox tbObjectives;
    private NumericUpDown tbRank;
    private Button btnCancelChar;
    private Button btnUpdateChar;
    private NumericUpDown tbIterations;
    private Label lbIterations;
  }
}
