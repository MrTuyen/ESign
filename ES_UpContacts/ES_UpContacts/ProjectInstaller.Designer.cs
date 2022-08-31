namespace ES_UpContacts
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
            this.ES_InitUploadContactsInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ES_InitUploadContacts = new System.ServiceProcess.ServiceInstaller();
            // 
            // ES_InitUploadContactsInstaller
            // 
            this.ES_InitUploadContactsInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ES_InitUploadContactsInstaller.Password = null;
            this.ES_InitUploadContactsInstaller.Username = null;
            // 
            // ES_InitUploadContacts
            // 
            this.ES_InitUploadContacts.Description = "Dịch vụ tự động đẩy liên hệ OnSign (Receiver) lên ElasticSearch";
            this.ES_InitUploadContacts.DisplayName = "ES_Upload Contacts Service";
            this.ES_InitUploadContacts.ServiceName = "ES_UpContactsService";
            this.ES_InitUploadContacts.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ES_InitUploadContacts,
            this.ES_InitUploadContactsInstaller});

        }

        #endregion
        private System.ServiceProcess.ServiceInstaller ES_InitUploadContacts;
        private System.ServiceProcess.ServiceProcessInstaller ES_InitUploadContactsInstaller;
    }
}