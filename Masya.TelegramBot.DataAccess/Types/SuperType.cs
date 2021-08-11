namespace Masya.TelegramBot.DataAccess.Types
{
    public enum SuperType : int
    {
        // Квартира 1
        Flat = 1 << 0,
        //  Дом 2
        House = 1 << 1,
        //  Участок 4
        Sector = 1 << 2,
        //  Коммерческая 8
        Commercial = 1 << 3,
        //  Новостройка 16
        NewBuilding = 1 << 4,
        //  Аренда 32
        Rental = 1 << 5,
        //  Любой
        Any = Flat | House | Sector | Commercial | NewBuilding | Rental
    }
}
