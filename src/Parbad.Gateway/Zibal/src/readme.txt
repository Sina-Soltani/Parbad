---------------------------------------------------
Parbad - Online Payment Library for .NET developers
				Zibal Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddZibal()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.Merchant = "<Your ID>";
                account.IsSandbox = true | false;
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice
    .SetZibalData(new ZibalRequest
    {
        // Zibal data
    })
    .UseZibal();
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.