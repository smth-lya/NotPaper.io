namespace GameShared;

public enum ClientToServerEvent
{
    JOIN = 1,           // Игрок подключается к игре
    MOVE,               // Игрок изменил направление
    STOP, // Игрок остановился
    DEATH, // Игрок умер
    EXIT,               // Игрок вышел из игры
    PING,               // Проверка активности сервера
    DIRECTION_CHANGE,   // Игрок изменил направление движения
    COLLISION_REPORT    // Клиент сообщает о коллизии
}