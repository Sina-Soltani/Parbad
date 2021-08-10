---------------------------------------------------
Parbad - Online Payment Library for .NET developers
				fanavacard.ir Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddFanAva()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.UserId = "<Your UserId>";
                account.Password = "<Your Password>";
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice.UseFanAva();
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.