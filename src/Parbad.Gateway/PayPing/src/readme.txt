---------------------------------------------------
Parbad - Online Payment Library for .NET developers
PayPing Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddPayPing()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.Name = "PayPing"; // optional if there is only 1 account for this gateway
                account.AcessToken = <Your Bearer Token>;
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice
        .SetAmount(viewModel.Amount)
        .SetCallbackUrl(callbackUrl)
        .SetPayPingData(payPing =>
        {
            // optional
        })
        .SetGateway("PayPing");

        // OR

        .UsePayPing();
});


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.