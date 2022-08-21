CrmSvcUtil.exe ^
/url:https://tfr.crm365dev.alberta.ca/tfr/XRMServices/2011/Organization.svc ^
/out:Generated/Entities.cs ^
/username:goa\crm-autoprocess.s  ^
/password:5m59pMTb ^
/domain:tfr.crm365dev.alberta.ca ^
/namespace:GOA.CRM.DataAccess ^
/serviceContextName:CrmDataContext

CrmSvcUtil.exe ^
/codewriterfilter:"Microsoft.Crm.Sdk.Samples.FilteringService, GeneratePicklistEnums" ^
/codecustomization:"Microsoft.Crm.Sdk.Samples.CodeCustomizationService, GeneratePicklistEnums" ^
/namingservice:"Microsoft.Crm.Sdk.Samples.NamingService, GeneratePicklistEnums" ^
/url:https://tfr.crm365dev.alberta.ca/tfr/XRMServices/2011/Organization.svc ^
/out:Generated/OptionSets.cs ^
/username:goa\crm-autoprocess.s  ^
/password:5m59pMTb ^
/domain:tfr.crm365dev.alberta.ca ^
/namespace:GOA.CRM.DataAccess

pause