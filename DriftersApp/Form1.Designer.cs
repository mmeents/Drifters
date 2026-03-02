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
      SuspendLayout();
      // 
      // checkBox1
      // 
      checkBox1.AutoSize = true;
      checkBox1.Location = new Point(1090, 12);
      checkBox1.Name = "checkBox1";
      checkBox1.Size = new Size(89, 24);
      checkBox1.TabIndex = 0;
      checkBox1.Text = "Let it run";
      checkBox1.UseVisualStyleBackColor = true;
      // 
      // displayTimer
      // 
      displayTimer.Interval = 1000;
      displayTimer.Tick += displayTimer_Tick;
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF(8F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = Color.FromArgb(0, 0, 12);
      ClientSize = new Size(1203, 842);
      Controls.Add(checkBox1);
      ForeColor = SystemColors.Control;
      Name = "Form1";
      Text = "Form1";
      Shown += Form1_Shown;
      ResizeEnd += Form1_ResizeEnd;
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private CheckBox checkBox1;
    private System.Windows.Forms.Timer displayTimer;
  }
}
