---------------------------------------------------
Parbad - Online Payment Library for .NET developers
IranKish Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddIranKish()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.TerminalId = <Your Terminal ID>;
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
    invoice.SetGateway("IranKish");

    // OR

	invoice.UseIranKish();
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.
