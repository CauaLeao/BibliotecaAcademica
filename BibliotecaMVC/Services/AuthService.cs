using System.Security.Cryptography;
using System.Text;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;

namespace BibliotecaMVC.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Usuario> AutenticarAsync(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            throw new RegraNegocioException("E-mail e senha são obrigatórios.");

        //  Busca o usuário pelo e-mail no banco
        var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

        // Se não encontrar ou o hash da senha não bater
        if (usuario is null || !VerificarSenha(senha, usuario.SenhaHash))
            throw new RegraNegocioException("E-mail ou senha incorretos.");

        // Bloqueia acesso de usuários com cadastro inativo
        if (!usuario.CadastroAtivo)
            throw new RegraNegocioException("Seu cadastro está inativo. Procure a administração da biblioteca.");

        return usuario;
    }

    // Gera o Hash da senha
    public static string GerarHashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        var builder = new StringBuilder();

        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));

        return builder.ToString();
    }

    public static bool VerificarSenha(string senhaDigitada, string senhaHashBanco)
    {
        var hashDigitado = GerarHashSenha(senhaDigitada);
        return string.Equals(hashDigitado, senhaHashBanco, StringComparison.OrdinalIgnoreCase);
    }
}