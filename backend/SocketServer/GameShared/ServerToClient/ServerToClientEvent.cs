namespace GameShared;

public enum ServerToClientEvent
{
    WELCOME = 1,         // Подтверждение входа в игру
    PLAYER_MOVE,         // Сообщает о движении игрока
    PLAYER_DEAD,         // Игрок был убит
    PLAYER_JOIN,         // Новый игрок вошел в игру
    PLAYER_EXIT,         // Игрок покинул игру
    GAME_STATE,          // Полное обновление карты
    SCORE_UPDATE,        // Обновление очков игрока
    VICTORY,             // Кто-то выиграл
    PING_RESPONSE        // Ответ на пинг клиента
}