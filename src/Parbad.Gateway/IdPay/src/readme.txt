---------------------------------------------------
Parbad - Online Payment Library for .NET developers
				IDPay.ir Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddIdPay()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.Api = "<Your API>";
                account.IsTestAccount = true | false;
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice.UseIdPay();
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.