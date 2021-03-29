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
                account.BearerToken = <Your Bearer Token>;   // https://app.payping.ir/token
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice.SetAmount(viewModel.Amount)
        .SetCallbackUrl(callbackUrl)
        .UseAutoIncrementTrackingNumber() // important need it 
        .SetPayPingData(payPingRequest => // optional
        {
            // for example Mobile for save card number and etc on ipgs  // https://github.com/Sina-Soltani/Parbad/issues/159
        });
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.