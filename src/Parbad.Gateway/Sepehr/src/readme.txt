---------------------------------------------------
Parbad - Online Payment Library for .NET developers
				Sepehr Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddSepehr()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.TerminalId = "<Your Terminal ID>";
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
    invoice.SetGateway("Sepehr");

    // OR

	invoice.UseSepehr();
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.