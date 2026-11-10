using System.Globalization;

namespace SQLApp.View;

public static class CValidacao
{
    public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

    public static int Codigo(string? texto, string campo = "Código")
    {
        if (!int.TryParse(texto, out int valor) || valor <= 0)
            throw new InvalidOperationException(campo + " deve ser um número inteiro maior que zero.");
        return valor;
    }

    public static string Texto(string? texto, string campo, int limite)
    {
        string valor = (texto ?? string.Empty).Trim();
        if (valor.Length == 0 || valor.Length > limite)
            throw new InvalidOperationException(campo + " é obrigatório e deve ter até " + limite + " caracteres.");
        return valor;
    }

    public static long RG(string? texto)
    {
        string valor = (texto ?? string.Empty).Trim();
        if (valor.Length == 0 || valor.Length > 10 || valor.Any(c => c < '0' || c > '9') ||
            !long.TryParse(valor, out long rg) || rg <= 0)
            throw new InvalidOperationException("RG deve conter de 1 a 10 dígitos e ser maior que zero.");
        return rg;
    }

    public static string Telefone(string? texto)
    {
        string valor = Texto(texto, "Telefone", 14);
        int digitos = valor.Count(c => c >= '0' && c <= '9');
        if (digitos < 8 || digitos > 11 || valor.Any(c => !"0123456789()- +".Contains(c)))
            throw new InvalidOperationException("Telefone deve conter de 8 a 11 dígitos e até 14 caracteres com a formatação.");
        return valor;
    }

    public static decimal Preco(string? texto)
    {
        string valor = (texto ?? string.Empty).Trim().Replace('.', ',');
        if (!decimal.TryParse(valor, NumberStyles.AllowDecimalPoint, Cultura, out decimal preco) ||
            preco <= 0 || preco > 999999999.99m || decimal.Round(preco, 2) != preco)
            throw new InvalidOperationException("Preço deve ser positivo, com até duas casas decimais e no máximo 999999999,99.");
        return preco;
    }

    public static int Estoque(string? texto)
    {
        if (!int.TryParse(texto, out int estoque) || estoque < 0)
            throw new InvalidOperationException("Estoque deve ser um número inteiro maior ou igual a zero.");
        return estoque;
    }
}
