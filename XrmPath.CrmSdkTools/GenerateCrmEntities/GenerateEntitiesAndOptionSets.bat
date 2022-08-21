CrmSvcUtil.exe ^
/url:https://phoenixsit.api.crm3.dynamics.com/XRMServices/2011/Organization.svc ^
/out:Generated/Entities.cs ^
/username:EDT.CRM-SYST.S@gov.ab.ca  ^
/password:2RrBs3R4mAQe9RzU1EinxW ^
/namespace:AEDWI.CrmData ^
/serviceContextName:CrmDataContext

CrmSvcUtil.exe ^
/codewriterfilter:"Microsoft.Crm.Sdk.Samples.FilteringService, GeneratePicklistEnums" ^
/codecustomization:"Microsoft.Crm.Sdk.Samples.CodeCustomizationService, GeneratePicklistEnums" ^
/namingservice:"Microsoft.Crm.Sdk.Samples.NamingService, GeneratePicklistEnums" ^
/url:https://phoenixsit.api.crm3.dynamics.com/XRMServices/2011/Organization.svc ^
/out:Generated/OptionSets.cs ^
/username:EDT.CRM-SYST.S@gov.ab.ca  ^
/password:2RrBs3R4mAQe9RzU1EinxW ^
/namespace:AEDWI.CrmData

pause