using System;
using System.Configuration;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;

namespace XrmPath.CRM.DataAccess.Base
{
    public abstract class CrmBase
    {

        public readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //App Settings Key in web.config
        //<add key="CrmDataContextExpirationMinutes" value="60" />
        private static readonly int ContextExpirationMinutes = int.Parse(ConfigurationManager.AppSettings["CrmDataContextExpirationMinutes"]);     //60 

        //Connection String in web.config
        //<add name="CrmContext" connectionString="Url=https://tfr.crm365dev.alberta.ca/tfr/XRMServices/2011/Organization.svc;Username=GOA\CRM-AutoProcess.S;Password=5m59pMTb;Domain=tfr.crm365dev.alberta.ca;authtype=AD;" />
        private static string CrmConnection = ConfigurationManager.ConnectionStrings["CrmContext"].ConnectionString;

        public DateTime ContextLastUpdated { get; set; } = DateTime.MinValue;
        private static readonly object LockDataContext = new object();

        /// <summary>
        /// Use this Global DataContext for readonly
        /// </summary>
        private CrmDataContext _DataContext;
        public CrmDataContext DataContext
        {
            get
            {
                var utcNow = DateTime.UtcNow;
                var expirationDateTime = ContextLastUpdated.AddMinutes(ContextExpirationMinutes);

                if (RequestNewContext(utcNow, expirationDateTime))
                {
                    lock (LockDataContext)
                    {
                        expirationDateTime = ContextLastUpdated.AddMinutes(ContextExpirationMinutes);   //this value may have changed by previous lock
                        if (RequestNewContext(utcNow, expirationDateTime))
                        {
                            _service = null;
                            _DataContext = null;
                            ContextLastUpdated = utcNow;
                        }
                    }
                }
                return this._DataContext ?? (this._DataContext = new CrmDataContext(service));
            }
            set
            {
                lock (LockDataContext)
                {
                    _DataContext = value;
                }
            }
        }

        public IOrganizationService GetOrganizationService()
        {
            var crmConnection = new CrmServiceClient(CrmConnection);
            var myService = crmConnection.OrganizationWebProxyClient ?? (IOrganizationService)crmConnection.OrganizationServiceProxy;
            return myService;
        }

        private OrganizationServiceProxy _service;
        public OrganizationServiceProxy service
        {
            get
            {
                if (RequestNewContext(DateTime.UtcNow))
                {
                    _service = null;
                }

                return this._service ?? (this._service = (OrganizationServiceProxy)GetOrganizationService());
                //return this._service ?? (this._service = this.crmManager.GetOrganizationService());
            }
            set
            {
                _service = value;
            }
        }

        /// <summary>
        /// Refreshes Global DataContext
        /// </summary>
        public void ResetContext()
        {
            try
            {
                var utcNow = DateTime.UtcNow;
                ContextLastUpdated = utcNow;
                //service = (OrganizationServiceProxy)GetOrganizationService(); //reset service variable
                //DataContext = new CrmDataContext(service);
                DataContext = GetDataContext();
            }
            catch (Exception ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmBase.ResetContext()", ex);
            }
        }

        public bool SaveCrmChanges(CrmDataContext dataContext = null, bool refreshDataContext = false)
        {
            try
            {
                if (dataContext == null)
                {   
                    dataContext = DataContext;
                }

                dataContext.SaveChanges();
                if (refreshDataContext)
                {
                    ResetContext();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"XrmPath.CRM.DataAccess caught error on CrmBase.SaveCrmChanges()", ex);
                return false;
            }
        }

        private bool RequestNewContext(DateTime utcNow, DateTime? expirationDateTime = null)
        {
            var newContext = _service != null && (!_service.IsAuthenticated || _service.SecurityTokenResponse.Token.ValidTo <= utcNow);

            //don't do this check if newContext is already set to true
            if (!newContext && expirationDateTime != null && utcNow > expirationDateTime)
            {
                //check if context is expired, set to true if it is.
                newContext = true;
            }
            return newContext;
        }

        protected CrmBase()
        {
            InitializeCrmSdkSettings();
        }

        protected CrmBase(IOrganizationService organizationService)
        {
            InitializeCrmSdkSettings();
            service = (OrganizationServiceProxy)organizationService;
            DataContext = new CrmDataContext(service);
        }

        protected CrmBase(IOrganizationService organizationService, CrmDataContext context)
        {
            InitializeCrmSdkSettings();
            service = (OrganizationServiceProxy)organizationService;
            DataContext = context;
        }

        private void InitializeCrmSdkSettings()
        {
            // Set security protocol to TLS 1.2 for version 9.0 of Customer Engagement Platform
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public CrmDataContext GetDataContext(bool organizationServiceRefresh = true)
        {
            var contextService = service;
            if (organizationServiceRefresh)
            {
                contextService = (OrganizationServiceProxy)GetOrganizationService();
            }
            var dataContext = new CrmDataContext(contextService);
            return dataContext;
        }

    }
}