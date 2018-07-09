namespace Parbad.Infrastructure.Translating
{
    internal interface IGatewayResultTranslator
    {
        string Translate(object result);
    }
}