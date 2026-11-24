# Bibliotech — Sistema de Gerenciamento de Biblioteca Acadêmica

Projeto prático integrativo (**P2**) desenvolvido para a disciplina de **Programação Orientada a Objetos (ILP007)** do curso superior de tecnologia em **Análise e Desenvolvimento de Sistemas (ADS)** — **FATEC Ribeirão Preto** (2º Semestre / 2026).

---

## Proposta de Valor

O **Bibliotech** é um portal web em **ASP.NET Core MVC** com persistência relacional via **Dapper + SQLite** voltado ao gerenciamento do acervo bibliográfico, controle de usuários e circulação de exemplares em uma instituição de ensino.

O sistema centraliza:
- **Consulta de catálogo** com pesquisa por título, autor ou ISBN, filtros por categoria e disponibilidade e ordenação alfabética;
- **Gestão de acervo** (categorias, livros e exemplares físicos), com controle de conservação, disponibilidade e classificação temporal (`Novo` / `Velho`);
- **Controle de usuários e permissões por perfil** (`Cliente` — Aluno, Funcionário e Professor —, `Bibliotecário` e `Administrador`);
- **Gestão completa de empréstimos e devoluções**, com prazos e limites diferenciados por perfil, renovação controlada, filtros por situação/prazo e cálculo automático de multas por atraso (R$ 1,00/dia).

---

## Equipe

| Nome Completo | RA |
| :--- | :--- |
| Antonio Carlos Farias Gouveia | `2840482511013` |
| Cauã Vitor Gonçalves Leão | `2840482511029` |

---

## Diagrama de Classes UML

![Diagrama de Classes UML](Diagrama%20de%20Classes.png)

### Principais Pilares de POO e Arquitetura Aplicados
- **Encapsulamento e Ciclo de Vida:** entidades ricas em `BibliotecaMVC/Models/` com propriedades `private set`, construtores validados e métodos de domínio (sem modelos anêmicos).
- **Composição e Agregação:** agregação `Categoria` → `Livro` (`IReadOnlyCollection<Livro>`) e composição `Livro` → `Exemplar` (`Livro.CriarExemplar` com construtor `internal` em `Exemplar`).
- **Herança e Polimorfismo:** hierarquia `Usuario` (abstrata) → `Cliente`, `Bibliotecario` → `Administrador`, sobrescrevendo `PossuiPermissao(TipoOperacao)` e aplicada de forma transparente pelo filtro `[AutorizarOperacao]`.
- **Generics e Coleções Tipadas:** interface genérica `IRepository<T> where T : class` e coleções fortemente tipadas (`List<T>`, `IEnumerable<T>`, `IReadOnlyCollection<T>`).
- **Exceções de Negócio:** `RegraNegocioException` tratada nos controllers com alertas amigáveis na interface Razor (sem exibir erro HTTP 500 para violações de regra).
- **Persistência com Dapper:** repositórios isolados em `BibliotecaMVC/Repositories/Dapper/` usando `SqliteConnection`, queries 100% parametrizadas (proteção contra SQL Injection) e Injeção de Dependência nativa em `Program.cs`.

---

## Estrutura do Projeto

```text
BibliotecaAcademica/
├── BibliotecaMVC/
│   ├── Controllers/         # Controladores MVC (orquestração HTTP)
│   ├── Database/            # script.sql (DDL + DML) e DbInitializer.cs
│   ├── Enums/               # Tipos enumerados de domínio
│   ├── Exceptions/          # RegraNegocioException
│   ├── Filters/             # AutorizarOperacaoAttribute (controle de acesso)
│   ├── Models/              # Entidades de domínio (Usuario, Livro, Exemplar, Emprestimo, etc.)
│   ├── Repositories/
│   │   ├── Interfaces/      # IRepository<T> e interfaces específicas
│   │   └── Dapper/          # Implementações Dapper + SQLite
│   ├── Services/            # Regras de aplicação e coordenação de fluxos
│   ├── ViewModels/          # Modelos de exibição e validação de formulários
│   ├── Views/               # Telas Razor (.cshtml)
│   ├── appsettings.json     # Configuração da Connection String
│   └── Program.cs           # Injeção de dependência, sessão e pipeline HTTP
├── Diagrama de Classes.png  # Diagrama UML do domínio
└── README.md
```

---

## Como Executar o Projeto

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado.
- Nenhuma instalação externa de SGBD é necessária, pois o projeto utiliza **SQLite** embarcado via `Microsoft.Data.Sqlite`.

### 1. Clonar o repositório
```bash
git clone https://github.com/CauaLeao/BibliotecaAcademica
cd BibliotecaAcademica
```

### 2. Conferir a Connection String
No arquivo `BibliotecaMVC/appsettings.json`, a string de conexão já vem configurada para criar/abrir o banco SQLite localmente:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Database/bibliotech.db"
  }
}
```

### 3. Criação do Banco de Dados e Carga Inicial (`script.sql`)
O script completo de criação das tabelas (DDL) e carga inicial com 10 registros de teste por tabela (DML) está em:

```text
BibliotecaMVC/Database/script.sql
```

Ao iniciar a aplicação, a classe `DbInitializer` executa automaticamente o `script.sql`, criando o arquivo `BibliotecaMVC/Database/bibliotech.db` (caso não exista) e populando as tabelas com dados iniciais de teste sem duplicar registros (`CREATE TABLE IF NOT EXISTS` e `INSERT OR IGNORE`).

> **Dica para resetar o banco:** para restaurar os dados iniciais do zero, basta encerrar a aplicação, apagar o arquivo `BibliotecaMVC/Database/bibliotech.db` e executá-la novamente.

### 4. Restaurar dependências e iniciar a aplicação
Na raiz do repositório (`BibliotecaAcademica/`), execute:

```bash
dotnet run --project BibliotecaMVC
```

Após a inicialização, acesse no navegador o endereço exibido no terminal (por padrão `http://localhost:5158`).

---

## Contas de Teste (Carga Inicial)

Todas as contas criadas pelo `script.sql` utilizam a senha **`123456`**:

| Perfil | E-mail | Senha | Observação |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@teste.com` | `123456` | Acesso total, incluindo gestão de bibliotecários |
| **Bibliotecário** | `bibliotecario@teste.com` | `123456` | Gestão de acervo, empréstimos e clientes |
| **Cliente (Aluno)** | `aluno@teste.com` | `123456` | Limite: 3 exemplares / Prazo: 7 dias |
| **Cliente (Funcionário)** | `funcionario@teste.com` | `123456` | Limite: 3 exemplares / Prazo: 10 dias |
| **Cliente (Professor)** | `professor@teste.com` | `123456` | Limite: 5 exemplares / Prazo: 14 dias |
| **Cliente Inativo** | `desativado@teste.com` | `123456` | Bloqueado no login (validação da RN03) |
