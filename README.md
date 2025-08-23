# 🐝 Swarm

Projeto de aprendizado feito com **.NET 8 + MonoGame**, seguindo princípios de **Clean Architecture**.  
O jogo é um protótipo de shooter 2D top-down, com camadas bem organizadas: **Domain → Application → Presentation**.  

---
# Protótipo:
Foi feito em 3 etapas. A partir desse ponto, registramos o progresso na linha do tempo a baixo e nos próximos passos.

## ✨ Etapa 1 – Primitivos do Domínio & Core
Começamos definindo as **bases** da camada de Domínio:

- ✅ **Guards**: verificações defensivas de invariantes
- 🧩 **Primitivos**: `Vector2`, `Direction`, `Radius`, `Bounds`, `DeltaTime`, `EntityId`, 
- ⏱️ **Cooldown**: struct para controlar ações baseadas em tempo

Isso garantiu blocos seguros para manter a lógica consistente e validada.

---

## 🎯 Etapa 2 – Combate & Entidades
Depois modelamos a **mecânica do jogo**:

- 🔫 **Weapon** com abstração para padrões de tiro
- 💥 **SingleShotPattern** implementado (tiro básico!)
- 🌀 **Projectile** com posição, direção, velocidade e tempo de vida
- 🧍 **Player** com movimento baseado em input
- 📦 **GameSession** para manter stage, player e projéteis
- ⚖️ **MovementIntegrator** (namespace Physics) para integrar movimento e limitar dentro do stage

Aqui já tínhamos o **modelo de gameplay rodando no domínio**.

---

## 🖥️ Etapa 3 – Aplicação & Apresentação
Por fim, deixamos o jogo **jogável**:

- 📜 **Contracts**: DTOs (`PlayerDto`, `ProjectileDto`, `StageDto`, `GameSnapshot`)  
- 🔌 **Services**: `IGameSessionService` + `GameSessionService` conectando Domain ↔ Presentation
- 🎮 **Presentation**: loop do MonoGame (`Swarm`)  
  - Captura inputs ⌨️  
  - Renderiza player & projéteis como círculos 🔵 🔴  
  - Chama serviços para atualizar e disparar  

Agora já é possível rodar o jogo e **mover + atirar**! 🚀

---

## 🚧 Próximos Passos
- 🔄 Rotação do jogador de acordo com a posição do mouse -- FEITO!  
- 🤝 Adicionar **detecção de colisão** (player ↔ inimigo, projétil ↔ inimigo) -- em progresso  
- 🧠 Implementar **IA de inimigos e spawners** -- em progreso
- 💾 Criar **sistema de score e persistência** (Infrastructure)


---

## 🛠️ Stack Tecnológico
- ⚙️ **.NET 8**
- 🎮 **MonoGame**
- 🏗️ **Clean Architecture** (Domain, Application, Presentation)

---

## 📜 Linha do Tempo (Commits Principais)

### 🔹 Step 1 – Primitivos - Domain
- :lock: `Guard` class  
- :x: `DomainException`  
- :sparkles: `EntityId`  
- :triangular_ruler: `Vector2`  
- :straight_ruler: `Bounds`  
- :o: `Radius`  
- :left_right_arrow: `Direction`  
- :heart: `HP (HitPoints)`  
- :broken_heart: `Damage`  
- :alarm_clock: `DeltaTime`  
- :stopwatch: `Cooldown`

### 🔹 Step 2 – Entidades e Lógica de Jogo - Domain
- :recycle: Renomeando `Value` → `Vector` em `Direction`  
- :sparkles: `Bounds` ganhou método `Clamp`  
- :video_game: `MovementIntegrator`  
- :boom: `Projectile`  
- :gun: `Weapon` + `FirePatterns`  
- :joystick: `Player`  
- :recycle: `Contains` em `Bounds`  
- :video_game: `GameSession` (controla estado da partida)

### 🔹 Step 3 – Camada de Aplicação
- :gun: `WeaponConfig`  
- :art: `StageConfig`  
- :joystick: `PlayerDTO`  
- :straight_ruler: `BoundsDTO`  
- :boom: `ProjectileDTO`  
- :camera: `GameSnapshot`  
- :earth_africa: `DomainMapper`  
- :video_game: `GameSessionService`

### 🔹 Step 4 – Presentation (MonoGame)
- :broom: Removendo referências desnecessárias  
- :sparkles: Protótipo inicial de renderização com **MonoGame**

### 🔹 Step 5 – Player Rotation
- :gun: Adicionando controle de tiro com o clique, já que estamos usando o mouse  
- :joystick: Desenhando player como um quadrado para visualizar rotação  
- :cyclone: Capturando rotação a partir da posição do mouse  
- :recycle: Making _session readonly  
- :earth_africa: DomainMapper converts Direction vector to radians  
- :joystick: PlayerDTO oferece ângulo de rotação  
- :video_game: GameSessionService implementa IGameSessionService RotateTowards  
- :video_game: IGameSessionService implementa RotateTowards  
- :video_game: :bangbang: RotatePlayerTowards na GameSession! Uma mudança de domínio necessária que passou desapercebida. GameSession é o aggregate entry-point do domínio. Application vai se comunicar com ele, apenas. GameSessionService da Application é responsável pela instância.  
- :gun: IFirePattern TryFire recebe facing  
- :joystick: Player tem rotação  
- :cyclone: Adicionando próximo passo, rotação do jogador  


---

## ▶️ Como Rodar

Pré-requisitos:
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [MonoGame 3.8.1](https://monogame.net/)

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/swarm.git
cd swarm

# Rode o jogo
dotnet run --project src/Swarm.Presentation
```

Feito com 💙 para estudar Clean Architecture & Game Dev.  
