	USE li4;
	GO

	CREATE TABLE Utilizador (
		Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Nome VARCHAR(100) NOT NULL,
		Email VARCHAR(100) NOT NULL UNIQUE,
		Tipo INT NOT NULL,
		Username VARCHAR(50) NOT NULL UNIQUE,
		Senha VARCHAR(50) NOT NULL,
		ContactoTel VARCHAR(15) NOT NULL,
		NIF VARCHAR(9) NOT NULL UNIQUE
	)

	CREATE TABLE Encomenda (
		Numero INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Custo DECIMAL(10,2) NOT NULL,
		Data DATETIME NOT NULL,
		DataPrevEntrega DATETIME,
		PagamentoEfetuado TINYINT NOT NULL DEFAULT 0,
		Estado VARCHAR(50) NOT NULL,
		IdCliente INT NOT NULL,
		FOREIGN KEY (IdCliente) REFERENCES Utilizador(id)
	)

	CREATE TABLE Montagem (
		IdEtapa INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Descricao TEXT NOT NULL,
		Duracao TIME NOT NULL,
		PassoSeguinte INT,
		FOREIGN KEY (PassoSeguinte) REFERENCES Montagem(IdEtapa)
	)

	CREATE TABLE Produto (
		Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Nome VARCHAR(100) NOT NULL,
		Descricao TEXT NOT NULL,
		Quantidade INT NOT NULL,
		Preco DECIMAL(10,2) NOT NULL,
		Montagem INT NOT NULL,
		FOREIGN KEY (Montagem) REFERENCES Montagem(IdEtapa)
	)

	CREATE TABLE Encomenda_tem_Produto (
		Quantidade INT NOT NULL,
		NumEncomenda INT NOT NULL,
		IdProduto INT NOT NULL,
		FOREIGN KEY(NumEncomenda) REFERENCES Encomenda(Numero),
		FOREIGN KEY(IdProduto) REFERENCES Produto(Id),
		PRIMARY KEY (NumEncomenda, IdProduto)
	)

	CREATE TABLE Material (
		Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Nome VARCHAR(100) NOT NULL,
		Quantidade INT NOT NULL
	)

	CREATE TABLE Produto_tem_Material (
		Quantidade INT NOT NULL,
		IdProduto INT NOT NULL,
		IdMaterial INT NOT NULL,
		FOREIGN KEY (IdProduto) REFERENCES Produto(Id),
		FOREIGN KEY (IdMaterial) REFERENCES Material(Id),
		PRIMARY KEY (IdProduto, IdMaterial)
	)