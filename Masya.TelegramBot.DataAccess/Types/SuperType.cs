namespace Masya.TelegramBot.DataAccess.Types
{
    public enum SuperType : int
    {
        // Квартира
        Flat = 1<<0,
        //  Дом
        House = 1<<1,
        //  Участок
        Sector = 1<<2,
        //  Коммерческая
        Commercial = 1<<3,
        //  Новостройка
        NewBuilding = 1<<4,
        //  Аренда
        Rental = 1<<5,
        //  Любой
        Any = Flat | House | Sector | Commercial | NewBuilding | Rental
    }
}
