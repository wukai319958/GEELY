﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.42000 版自动生成。
// 
#pragma warning disable 1591

namespace Interfaces.PtlToLes {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="PtlToLesServicePortBinding", Namespace="http://webservice.geelyles.com/")]
    public partial class PtlToLesServiceService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback PTLCartDepartOperationCompleted;
        
        private System.Threading.SendOrPostCallback PTLCartPickBackLesOperationCompleted;
        
        private System.Threading.SendOrPostCallback PTLPalletPickBackLesOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public PtlToLesServiceService() {
            this.Url = global::Interfaces.Properties.Settings.Default.Interfaces_PtlToLes_PtlToLesServiceService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event PTLCartDepartCompletedEventHandler PTLCartDepartCompleted;
        
        /// <remarks/>
        public event PTLCartPickBackLesCompletedEventHandler PTLCartPickBackLesCompleted;
        
        /// <remarks/>
        public event PTLPalletPickBackLesCompletedEventHandler PTLPalletPickBackLesCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://webservice.geelyles.com/", ResponseNamespace="http://webservice.geelyles.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string PTLCartDepart(string arg0) {
            object[] results = this.Invoke("PTLCartDepart", new object[] {
                        arg0});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PTLCartDepartAsync(string arg0) {
            this.PTLCartDepartAsync(arg0, null);
        }
        
        /// <remarks/>
        public void PTLCartDepartAsync(string arg0, object userState) {
            if ((this.PTLCartDepartOperationCompleted == null)) {
                this.PTLCartDepartOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPTLCartDepartOperationCompleted);
            }
            this.InvokeAsync("PTLCartDepart", new object[] {
                        arg0}, this.PTLCartDepartOperationCompleted, userState);
        }
        
        private void OnPTLCartDepartOperationCompleted(object arg) {
            if ((this.PTLCartDepartCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PTLCartDepartCompleted(this, new PTLCartDepartCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://webservice.geelyles.com/", ResponseNamespace="http://webservice.geelyles.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string PTLCartPickBackLes(string arg0) {
            object[] results = this.Invoke("PTLCartPickBackLes", new object[] {
                        arg0});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PTLCartPickBackLesAsync(string arg0) {
            this.PTLCartPickBackLesAsync(arg0, null);
        }
        
        /// <remarks/>
        public void PTLCartPickBackLesAsync(string arg0, object userState) {
            if ((this.PTLCartPickBackLesOperationCompleted == null)) {
                this.PTLCartPickBackLesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPTLCartPickBackLesOperationCompleted);
            }
            this.InvokeAsync("PTLCartPickBackLes", new object[] {
                        arg0}, this.PTLCartPickBackLesOperationCompleted, userState);
        }
        
        private void OnPTLCartPickBackLesOperationCompleted(object arg) {
            if ((this.PTLCartPickBackLesCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PTLCartPickBackLesCompleted(this, new PTLCartPickBackLesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapRpcMethodAttribute("", RequestNamespace="http://webservice.geelyles.com/", ResponseNamespace="http://webservice.geelyles.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string PTLPalletPickBackLes(string arg0) {
            object[] results = this.Invoke("PTLPalletPickBackLes", new object[] {
                        arg0});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PTLPalletPickBackLesAsync(string arg0) {
            this.PTLPalletPickBackLesAsync(arg0, null);
        }
        
        /// <remarks/>
        public void PTLPalletPickBackLesAsync(string arg0, object userState) {
            if ((this.PTLPalletPickBackLesOperationCompleted == null)) {
                this.PTLPalletPickBackLesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPTLPalletPickBackLesOperationCompleted);
            }
            this.InvokeAsync("PTLPalletPickBackLes", new object[] {
                        arg0}, this.PTLPalletPickBackLesOperationCompleted, userState);
        }
        
        private void OnPTLPalletPickBackLesOperationCompleted(object arg) {
            if ((this.PTLPalletPickBackLesCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PTLPalletPickBackLesCompleted(this, new PTLPalletPickBackLesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void PTLCartDepartCompletedEventHandler(object sender, PTLCartDepartCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PTLCartDepartCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PTLCartDepartCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void PTLCartPickBackLesCompletedEventHandler(object sender, PTLCartPickBackLesCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PTLCartPickBackLesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PTLCartPickBackLesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void PTLPalletPickBackLesCompletedEventHandler(object sender, PTLPalletPickBackLesCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PTLPalletPickBackLesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PTLPalletPickBackLesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591