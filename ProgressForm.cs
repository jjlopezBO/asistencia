using System;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
 


namespace Asistencia
{
    public partial class ProgressForm : XtraForm
    {
        private ProgressBarControl progressBar;
        private SimpleButton cancelButton;
        private LabelControl statusLabel;
        private TaskCompletionSource<bool> taskCompletionSource;

        public ProgressForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.progressBar = new DevExpress.XtraEditors.ProgressBarControl();
            this.cancelButton = new DevExpress.XtraEditors.SimpleButton();
            this.statusLabel = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 12);
            this.progressBar.Name = "progressBar";
            this.progressBar.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            this.progressBar.Size = new System.Drawing.Size(360, 18);
            this.progressBar.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(297, 36);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(12, 40);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 2;
            // 
            // ProgressForm
            // 
            this.ClientSize = new System.Drawing.Size(384, 71);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.progressBar);
            this.Name = "ProgressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Progreso";
            this.Load += new System.EventHandler(this.ProgressForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.progressBar.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // This method is called to update the progress
        public void UpdateProgress(int value, string status)
        {
            progressBar.EditValue = value;
            statusLabel.Text = status;
            if (value == 100)
            {
                statusLabel.Text = "Completed";
                taskCompletionSource?.SetResult(true); // Mark the task as completed when progress reaches 100
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Handle cancel button click
            taskCompletionSource?.SetResult(false); // Mark the task as canceled
            this.Close();
        }

        // This method returns a Task that can be awaited
        public Task RunTaskAsync()
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            return taskCompletionSource.Task;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {

        }
    }

   
}
