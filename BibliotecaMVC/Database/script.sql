PRAGMA foreign_keys = ON; -- liga a "fiscalização" das FKs para garantir a integridade do banco (vem desativado por padrão em versões mais antigas)

CREATE TABLE IF NOT EXISTS Usuarios (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    CPF TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    SenhaHash TEXT NOT NULL,
    DataCadastro TEXT NOT NULL,                -- SQLite não possui tipagem para datas, portanto, usaremos textos no lugar
    CadastroAtivo INTEGER NOT NULL DEFAULT 1,   -- SQLite não possui tipagem booleana, portanto, usaremos inteiros no lugar
    TipoUsuario INTEGER NOT NULL,               -- 1=Cliente, 2=Bibliotecario, 3=Administrador (SQLite também não possui enums, por isso o uso de inteiros)
    Matricula TEXT UNIQUE,                      -- atributo destinado a Bibliotecario/Administrador, nulo aos demais
    Perfil INTEGER,                             -- 1=Aluno, 2=Funcionario, 3=Professor, nulo aos demais
    RegistroInstitucional TEXT UNIQUE           -- atributo destinado a Cliente, nulo aos demais
);

CREATE TABLE IF NOT EXISTS Categorias (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL UNIQUE,
    Descricao TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Livros (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo TEXT NOT NULL,
    Autor TEXT NOT NULL,
    ISBN TEXT NOT NULL UNIQUE,
    Resumo TEXT NOT NULL,
    CategoriaID INTEGER NOT NULL,
    FOREIGN KEY (CategoriaID) REFERENCES Categorias(ID) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Exemplares (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    EstadoConservacao INTEGER NOT NULL,        -- 1=Novo, 2=Bom, 3=Regular, 4=Danificado
    DataAquisicao TEXT NOT NULL,
    Disponibilidade INTEGER NOT NULL DEFAULT 1, -- 1=Disponivel, 2=Emprestado, 3=Indisponivel
    LivroID INTEGER NOT NULL,
    FOREIGN KEY (LivroID) REFERENCES Livros(ID) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Emprestimos (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    DataRetirada TEXT NOT NULL,
    DataDevolucaoPrevista TEXT NOT NULL,
    DataDevolucaoEfetiva TEXT,
    PrazoDias INTEGER NOT NULL,
    Renovado INTEGER NOT NULL DEFAULT 0,
    UsuarioID INTEGER NOT NULL,
    ExemplarID INTEGER NOT NULL,
    FOREIGN KEY (UsuarioID) REFERENCES Usuarios(ID) ON DELETE RESTRICT,
    FOREIGN KEY (ExemplarID) REFERENCES Exemplares(ID) ON DELETE RESTRICT
);

INSERT OR IGNORE INTO Usuarios -- inserção de usuários fictícios no banco para fins de teste
(Nome, CPF, Email, SenhaHash, DataCadastro, CadastroAtivo, TipoUsuario, Matricula, Perfil, RegistroInstitucional)
VALUES
('admin', '11111111111', 'admin@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 3, 'ADM001', NULL, NULL),
('Bibliotecson', '22222222222', 'bibliotecario@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 2, 'BIB001', NULL, NULL),
('Alunilson', '33333333333', 'aluno@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 1, '3333333333333'),
('Funcionelson', '44444444444', 'funcionario@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 2, '4444444444444'),
('Profersson', '55555555555', 'professor@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 3, '5555555555555'),
('Desativaluno', '66666666666', 'desativado@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 0, 1, NULL, 1, '6666666666666');