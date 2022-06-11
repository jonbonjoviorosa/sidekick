
namespace Sidekick.Jobs
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
            this.BookingPaymentServiceInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.BookingPaymentService = new System.ServiceProcess.ServiceInstaller();
            // 
            // BookingPaymentServiceInstaller
            // 
            this.BookingPaymentServiceInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.BookingPaymentServiceInstaller.Password = null;
            this.BookingPaymentServiceInstaller.Username = null;
            // 
            // BookingPaymentService
            // 
            this.BookingPaymentService.ServiceName = "Booking Payment Service";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.BookingPaymentServiceInstaller,
            this.BookingPaymentService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller BookingPaymentServiceInstaller;
        private System.ServiceProcess.ServiceInstaller BookingPaymentService;
    }
}