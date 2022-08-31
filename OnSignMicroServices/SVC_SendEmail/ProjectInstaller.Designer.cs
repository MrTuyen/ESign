using System.Configuration;

namespace SVC_SendEmail
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SVC_InitSendEmailInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SVC_InitSendEmail = new System.ServiceProcess.ServiceInstaller();
            // 
            // SVC_InitSendEmailInstaller
            // 
            this.SVC_InitSendEmailInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SVC_InitSendEmailInstaller.Password = null;
            this.SVC_InitSendEmailInstaller.Username = null;
            // 
            // SVC_InitSendEmail
            // 
            this.SVC_InitSendEmail.Description = "Trigger nhận thông tin và gửi email tự động";
            this.SVC_InitSendEmail.DisplayName = "[ONSIGN - LIVE] Send email automation";
            this.SVC_InitSendEmail.ServiceName = "ONSIGN_LIVE_SEND_EMAIL_AUTOMATION";
            this.SVC_InitSendEmail.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SVC_InitSendEmail,
            this.SVC_InitSendEmailInstaller});
        }

        #endregion
        private System.ServiceProcess.ServiceInstaller SVC_InitSendEmail;
        private System.ServiceProcess.ServiceProcessInstaller SVC_InitSendEmailInstaller;
    }
}