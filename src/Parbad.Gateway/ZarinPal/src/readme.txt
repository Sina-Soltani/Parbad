---------------------------------------------------
Parbad - Online Payment Library for .NET developers
				ZarinPal Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddZarinPal()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.MerchantId = "<Your ID>";
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
    // other invoice data
    .SetZarinPalData(...)
	.UseZarinPal();
})

-------------
Getting the original verification result
-------------
var result = _onlinePayment.VerifyAsync(invoice);

var originalResult = result.GetZarinPalOriginalVerificationResult();

------------------------------------------------------------------------
Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.
