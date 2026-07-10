# Архитектура игры "Космический бой"

## Диаграмма контекста системы (C4 Level 1)

```mermaid
flowchart LR
    Player["Игрок"]
    Organizer["Организатор турнира"]
    Agent["Агент\n(алгоритм управления)"]

    subgraph System["Платформа SpaceBattle"]
        SB["Космический бой"]
    end

    Player -->|"Регистрация, турниры, заявки, просмотр боёв"| System
    Organizer -->|"Создание и управление турнирами"| System
    Player -->|"Загрузка алгоритма"| Agent
    System -->|"Состояние боя"| Agent
    Agent -->|"Команды управления"| System
```



## Диаграмма контейнеров - микросервисы (C4 Level 2)

```mermaid
flowchart TB
    subgraph Clients["Внешние клиенты"]
        Web["Web Client\n(браузер)"]
        Agent["Агент\n(приложение игрока)"]
    end

    subgraph Platform["Платформа SpaceBattle"]
        Auth["Auth Service\nРегистрация и вход"]
        Game["Game Server\nПроведение боёв"]
        Tournament["Tournament Service\nУправление турнирами"]
        Rating["Rating Service\nРасчёт рейтингов"]
        Notify["Notification Service\nУведомления"]
        History["BattleHistory Service\nХранение и просмотр боёв"]
        Broker["Message Broker\nАсинхронные сообщения"]
    end

    Web -->|"REST"| Auth
    Web -->|"REST"| Tournament
    Web -->|"REST"| History
    Web -->|"REST"| Rating

    Agent -->|"WebSocket"| Game

    Auth -->|"События"| Broker
    Tournament -->|"События"| Broker
    Game -->|"События"| Broker

    Broker -->|"Подписка"| Notify
    Broker -->|"Подписка"| Rating
    Broker -->|"Подписка"| History

    Notify -->|"E-mail / Push"| Web
```



## Взаимодействия между сервисами

### Синхронные запросы (REST)

| Источник | Получатель | Протокол | Описание |
|----------|------------|----------|----------|
| Web Client | Auth Service | REST | Регистрация, вход, получение токена |
| Web Client | Tournament Service | REST | Список турниров, подача заявки, результаты |
| Web Client | BattleHistory Service | REST | Просмотр записей прошедших боев |
| Web Client | Rating Service | REST | Таблица лидеров, рейтинг игрока или турнира |

### Реальное время

| Источник | Получатель | Протокол | Описание |
|----------|------------|----------|----------|
| Agent | Game Server | WebSocket | Команды управления кораблями |
| Game Server | Agent | WebSocket | Текущее состояние боя |

### Асинхронные события (через Message Broker)

| Отправитель | Событие | Получатели |
|-------------|---------|------------|
| Auth Service | Пользователь зарегистрирован | Notification Service, Rating Service |
| Tournament Service | Турнир создан | Notification Service |
| Tournament Service | Заявка одобрена / отклонена | Notification Service |
| Tournament Service | Турнир завершен | Rating Service, Notification Service |
| Game Server | Бой завершен | BattleHistory Service, Rating Service, Notification Service |
| Game Server | Бой скоро начнется | Notification Service |