---------------------------------------------------
Parbad - Online Payment Library for .NET developers
YekPay Gateway
---------------------------------------------------

GitHub: https://github.com/Sina-Soltani/Parbad
Tutorials: https://github.com/Sina-Soltani/Parbad/wiki

-------------
Configuration
-------------

.ConfigureGateways(gateways =>
{
    gateways
        .AddYekPay()
        .WithAccounts(accounts =>
        {
            accounts.AddInMemory(account =>
            {
                account.MerchantId = <Your Merchant ID>;
            });
        });
})

-------------
Making a request
-------------

var result = _onlinePayment.RequestAsync(invoice => 
{
	invoice.UseYekPay(yekPay => 
    {
       // payment details
    });
})


Do you like Parbad? Then don't forget to **Star** the Parbad on GitHub.