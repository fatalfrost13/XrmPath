CrmDynamicEntityHelper contains all the helpers to perform GET, SAVE, DELETE, UPDATE
The Sample Folder has a few samples of how to use the DataAccess helper. I created a sample model called "Product" with some fields to display how these helpers work.

Connection String Example:

On Cloud Example
<add name="CrmContext" connectionString="Url=https://phoenixsit.api.crm3.dynamics.com;Username=edt.crm-uat.s@gov.ab.ca;Password=EDcrP0rTal123UA;authtype=Office365" />

On Prem Example
<add name="CrmContext" connectionString="Url=https://tfr.crm365dev.alberta.ca/tfr/XRMServices/2011/Organization.svc;Username=GOA\CRM-AutoProcess.S;Password=5m59pMTb;Domain=tfr.crm365dev.alberta.ca;authtype=AD;" />
