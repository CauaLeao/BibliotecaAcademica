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


-- 1. USUÁRIOS 
-- Senha de todos: 123456
INSERT OR IGNORE INTO Usuarios -- inserção de usuários fictícios no banco para fins de teste
(ID, Nome, CPF, Email, SenhaHash, DataCadastro, CadastroAtivo, TipoUsuario, Matricula, Perfil, RegistroInstitucional)
VALUES
(1, 'admin', '11111111111', 'admin@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 3, 'ADM001', NULL, NULL),
(2, 'Bibliotecson', '22222222222', 'bibliotecario@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 2, 'BIB001', NULL, NULL),
(3, 'Alunilson', '33333333333', 'aluno@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 1, '3333333333333'),
(4, 'Funcionelson', '44444444444', 'funcionario@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 2, '4444444444444'),
(5, 'Profersson', '55555555555', 'professor@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 1, 1, NULL, 3, '5555555555555'),
(6, 'Desativaluno', '66666666666', 'desativado@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-01-10 08:00:00', 0, 1, NULL, 1, '6666666666666'),
(7, 'Mariana Leitora', '77777777777', 'mariana@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-02-01 10:00:00', 1, 1, NULL, 1, '7777777777777'),
(8, 'Carlos Leitor', '88888888888', 'carlos@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-02-05 11:30:00', 1, 1, NULL, 2, '8888888888888'),
(9, 'Fernanda Bibliotecaria', '99999999999', 'fernanda@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-02-10 08:00:00', 1, 2, 'BIB002', NULL, NULL),
(10, 'Roberto Admin', '10101010101', 'roberto@teste.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '2026-02-15 09:00:00', 1, 3, 'ADM002', NULL, NULL);


-- 2. CATEGORIAS
INSERT OR IGNORE INTO Categorias (ID, Nome, Descricao)
VALUES
(1, 'Ficção Científica', 'Narrativas que exploram conceitos futuristas, ciência, espaço e distopias.'),
(2, 'Tecnologia e Programação', 'Livros sobre desenvolvimento de software, arquitetura, linguagens e bancos de dados.'),
(3, 'Literatura e Clássicos', 'Obras consagradas da literatura brasileira e internacional.'),
(4, 'Suspense e Mistério', 'Romances policiais, investigações, crimes e enigmas psicológicos.'),
(5, 'Fantasia', 'Mundos imaginários, magia, jornadas épicas e criaturas mitológicas.'),
(6, 'História e Filosofia', 'Reflexões sobre a evolução das sociedades, civilizações e pensamento humano.'),
(7, 'Biografias', 'Histórias de vida e memórias de personalidades inspiradoras.'),
(8, 'Psicologia e Comportamento', 'Estudos sobre a mente humana, inteligência emocional e hábitos.'),
(9, 'Negócios e Gestão', 'Empreendedorismo, liderança estratégica, finanças e produtividade.'),
(10, 'Arte e Poesia', 'Coletâneas líricas, história da arte, design e sensibilidade cultural.');



-- 3. LIVROS
INSERT OR IGNORE INTO Livros (ID, Titulo, Autor, ISBN, Resumo, CategoriaID)
VALUES
(1, 'Duna', 'Frank Herbert', '9788576572008', 'Uma jornada épica no planeta desértico Arrakis envolvendo política, religião e poder.', 1),
(2, 'Código Limpo (Clean Code)', 'Robert C. Martin', '9788576082675', 'Boas práticas e padrões para escrever código limpo, legível e manutenível.', 2),
(3, 'Dom Casmurro', 'Machado de Assis', '9788520938447', 'As memórias de Bento Santiago e o eterno enigma sobre Capitu.', 3),
(4, 'O Assassinato no Expresso do Oriente', 'Agatha Christie', '9788525414328', 'Hercule Poirot investiga um assassinato ocorrido em um trem retido pela neve.', 4),
(5, 'O Hobbit', 'J.R.R. Tolkien', '9788595084742', 'A aventura inesperada de Bilbo Bolseiro em direção à Montanha Solitária.', 5),
(6, 'Sapiens: Uma Breve História da Humanidade', 'Yuval Noah Harari', '9788525432186', 'Uma jornada fascinante desde a aurora da humanidade até a revolução biotecnológica.', 6),
(7, 'Steve Jobs', 'Walter Isaacson', '9788535919714', 'A biografia autorizada do líder visionário da Apple.', 7),
(8, 'Rápido e Devagar: Duas Formas de Pensar', 'Daniel Kahneman', '9788539003839', 'Como o cérebro humano toma decisões sob a ótica da economia comportamental.', 8),
(9, 'O Homem Mais Rico da Babilônia', 'George S. Clason', '9788595081536', 'Parábolas milenares sobre economia pessoal e sucesso financeiro.', 9),
(10, 'Neuromancer', 'William Gibson', '9788576573005', 'O romance visionário que deu origem ao gênero Cyberpunk.', 1);


-- 4. EXEMPLARES 
-- conservação: 1=Novo, 2=Bom, 3=Reg., 4=Danif.
-- Disponibilidade: 1=Disponível, 2=Emprestado, 3=Indisponível

INSERT OR IGNORE INTO Exemplares (ID, Codigo, EstadoConservacao, DataAquisicao, Disponibilidade, LivroID)
VALUES
(1,  'DUN-001', 1, '2026-01-15', 2, 1),  -- Duna (Emprestado)
(2,  'CLE-001', 1, '2026-01-20', 2, 2),  -- Clean Code (Emprestado)
(3,  'DOM-001', 2, '2024-03-10', 1, 3),  -- Dom Casmurro (Disponível - >2 anos: Velho)
(4,  'EXP-001', 2, '2026-02-01', 1, 4),  -- Expresso do Oriente (Disponível)
(5,  'HOB-001', 1, '2026-02-10', 2, 5),  -- O Hobbit (Emprestado)
(6,  'SAP-001', 2, '2025-06-18', 1, 6),  -- Sapiens (Disponível)
(7,  'JOB-001', 2, '2024-01-10', 1, 7),  -- Steve Jobs (Disponível)
(8,  'RAP-001', 1, '2026-02-20', 2, 8),  -- Rápido e Devagar (Emprestado)
(9,  'BAB-001', 2, '2025-08-05', 1, 9),  -- Babilônia (Disponível)
(10, 'NEU-001', 4, '2023-05-15', 3, 10); -- Neuromancer (Danificado - Indisponível)


-- 5. EMPRÉSTIMOS
INSERT OR IGNORE INTO Emprestimos 
(ID, DataRetirada, DataDevolucaoPrevista, DataDevolucaoEfetiva, PrazoDias, Renovado, UsuarioID, ExemplarID)
VALUES
-- Empréstimo 1: Ativo, no prazo (Alunilson com Duna)
(1,  '2026-11-20', '2026-11-27', NULL, 7, 0, 3, 1),
-- Empréstimo 2: Ativo, em atraso (Mariana com Clean Code - Prazo de 7 dias venceu dia 17 para testar multa)
(2,  '2026-11-10', '2026-11-17', NULL, 7, 0, 7, 2),
-- Empréstimo 3: Ativo, renovado (Profersson com O Hobbit - 14 dias de prazo)
(3,  '2026-11-01', '2026-11-29', NULL, 14, 1, 5, 5),
-- Empréstimo 4: Ativo, no prazo (Carlos com Rápido e Devagar - 10 dias de prazo)
(4,  '2026-11-18', '2026-11-28', NULL, 10, 0, 8, 8),
-- Empréstimos 5 a 10: Encerrados (Histórico de Devoluções)
(5,  '2026-10-01', '2026-10-08', '2026-10-07', 7, 0, 3, 3),
(6,  '2026-10-05', '2026-10-12', '2026-10-14', 7, 0, 4, 4), -- Devolvido com 2 dias de atraso
(7,  '2026-10-10', '2026-10-24', '2026-10-22', 14, 0, 5, 6),
(8,  '2026-10-15', '2026-10-25', '2026-10-25', 10, 0, 7, 7),
(9,  '2026-10-20', '2026-10-27', '2026-10-26', 7, 0, 8, 9),
(10, '2026-09-01', '2026-09-15', '2026-09-14', 7, 1, 3, 10);