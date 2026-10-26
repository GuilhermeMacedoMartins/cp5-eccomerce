namespace Ecommerce.Domain.Common;

public static class DomainErrors
{
    public static class Produto
    {
        public static Error NaoEncontrado(int id) =>
            Error.NotFound("PRODUTO_NAO_ENCONTRADO", $"Produto {id} nao foi encontrado.");

        public static Error Inativo(string nome) =>
            Error.Conflict("PRODUTO_INATIVO", $"O produto '{nome}' esta inativo e nao pode ser vendido.");

        public static Error EstoqueInsuficiente(string nome, int disponivel, int solicitado) =>
            Error.Conflict("ESTOQUE_INSUFICIENTE",
                $"Estoque insuficiente para '{nome}'. Disponivel: {disponivel}, solicitado: {solicitado}.");

        public static Error PrecoInvalido() =>
            Error.Validation("PRECO_INVALIDO", "O preco do produto deve ser maior que zero.");

        public static Error VinculadoAPedido(int id) =>
            Error.Conflict("PRODUTO_VINCULADO", $"O produto {id} esta vinculado a pedidos e nao pode ser removido.");
    }

    public static class Pedido
    {
        public static Error NaoEncontrado(int id) =>
            Error.NotFound("PEDIDO_NAO_ENCONTRADO", $"Pedido {id} nao foi encontrado.");

        public static Error SemItens() =>
            Error.Validation("PEDIDO_SEM_ITENS", "O pedido deve conter ao menos um item.");

        public static Error ItensDuplicados(IEnumerable<int> ids) =>
            Error.Validation("ITENS_DUPLICADOS",
                $"Ha itens duplicados para o(s) produto(s): {string.Join(", ", ids)}.");

        public static Error StatusFinalizado(int id, string status) =>
            Error.Conflict("STATUS_FINALIZADO", $"O pedido {id} esta {status} e nao pode mais ser alterado.");

        public static Error NaoPertenceAoUsuario(int id) =>
            Error.Unauthorized("PEDIDO_DE_OUTRO_USUARIO", $"O pedido {id} nao pertence ao usuario autenticado.");
    }

    public static class Cep
    {
        public static Error Invalido(string cep) =>
            Error.Validation("CEP_INVALIDO", $"O CEP '{cep}' nao e valido. Informe 8 digitos.");

        public static Error NaoEncontrado(string cep) =>
            Error.NotFound("CEP_NAO_ENCONTRADO", $"O CEP '{cep}' nao foi encontrado na base dos Correios.");

        public static Error ServicoIndisponivel() =>
            Error.External("CEP_SERVICO_INDISPONIVEL",
                "O servico de consulta de CEP esta indisponivel no momento. Tente novamente em instantes.");
    }

    public static class Autenticacao
    {
        public static Error CredenciaisInvalidas() =>
            Error.Unauthorized("CREDENCIAIS_INVALIDAS", "E-mail ou senha incorretos.");

        public static Error EmailJaCadastrado(string email) =>
            Error.Conflict("EMAIL_JA_CADASTRADO", $"O e-mail '{email}' ja esta cadastrado.");
    }
}
