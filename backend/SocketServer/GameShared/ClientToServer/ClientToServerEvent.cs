namespace GameShared
{
    public enum ClientToServerEvent : byte
    {
        JOIN = 1,           // Игрок подключается к игре
        MOVE,               // Игрок изменил направление
        STOP,               // Игрок остановился // ЭТОТ ТОЖЕ ДУМАЮ НЕ ПОНАДОБИТЬСЯ
        DEATH,              // Игрок умер
        EXIT,               // Игрок вышел из игры
        PING,               // Проверка активности сервера
        DIRECTION_CHANGE,   // Игрок изменил направление движения
        COLLISION_REPORT,    // Клиент сообщает о коллизии // НЕ ПОНАДОБИТЬСЯ СКОРЕЕ ВСЕГО
        SEND_POSITION
    }
}