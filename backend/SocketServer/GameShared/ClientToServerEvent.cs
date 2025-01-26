namespace GameShared;

public enum ClientToServerEvent
{
    JOIN = 1,            // Игрок подключается к игре
    MOVE,               // Игрок изменил направление
    EXIT,               // Игрок вышел из игры
    PING,               // Проверка активности сервера
    RESPAWN,            // Игрок просит возродиться после смерти
    DIRECTION_CHANGE,   // Игрок изменил направление движения
    COLLISION_REPORT    // Клиент сообщает о коллизии
}