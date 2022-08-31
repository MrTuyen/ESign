using OnSign.Common.Helpers;

namespace SVC_DocumentRender
{
    partial class ProjectInstaller
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.SVC_Live_InitRabbitMQInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SVC_Live_RabbitMQ = new System.ServiceProcess.ServiceInstaller();
            // 
            // SVC_Live_InitRabbitMQInstaller
            // 
            this.SVC_Live_InitRabbitMQInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.SVC_Live_InitRabbitMQInstaller.Password = null;
            this.SVC_Live_InitRabbitMQInstaller.Username = null;
            // 
            // SVC_Live_RabbitMQ
            // 
            this.SVC_Live_RabbitMQ.Description = $"Receive task from RabbitMQ and convert to pdf and render to png (Nhận dữ liệu từ " +
                $"RabbitMQ, chuyển file gốc thành file PDF, chuyển file PDF thành file PNG)";
            this.SVC_Live_RabbitMQ.DisplayName = "[ONSIGN - LIVE] - Documents convert and render";
            this.SVC_Live_RabbitMQ.ServiceName = "[ONSIGN - LIVE] - Documents convert and render";
            this.SVC_Live_RabbitMQ.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SVC_Live_RabbitMQ,
            this.SVC_Live_InitRabbitMQInstaller});

        }

        #endregion
        private System.ServiceProcess.ServiceInstaller SVC_Live_RabbitMQ;
        private System.ServiceProcess.ServiceProcessInstaller SVC_Live_InitRabbitMQInstaller;
    }
}