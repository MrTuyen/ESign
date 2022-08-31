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
            this.SVC_InitCRMCallerInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SVC_InitCRMCaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SVC_InitCRMCallerInstaller
            // 
            this.SVC_InitCRMCallerInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SVC_InitCRMCallerInstaller.Password = null;
            this.SVC_InitCRMCallerInstaller.Username = null;
            // 
            // SVC_InitCRMCaller
            // 
            string Environment = "TEST"; //ConfigurationManager.AppSettings["Environment"].ToUpper();
            this.SVC_InitCRMCaller.Description = $"Gọi thoại OTP";
            this.SVC_InitCRMCaller.DisplayName = $"[ONSIGN_{Environment}] CRM Caller";
            this.SVC_InitCRMCaller.ServiceName = $"ONSIGN_{Environment}_CRM_CALLER";
            this.SVC_InitCRMCaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SVC_InitCRMCaller,
            this.SVC_InitCRMCallerInstaller});

        }

        #endregion
        private System.ServiceProcess.ServiceInstaller SVC_InitCRMCaller;
        private System.ServiceProcess.ServiceProcessInstaller SVC_InitCRMCallerInstaller;
    }
}