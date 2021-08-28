namespace Masya.TelegramBot.DataAccess.Models
{
    public enum Permission
    {
        //  0
        Guest = 0,
        //  1
        User = 1 << 0,
        //  2
        Agent = 1 << 1,
        //  4
        Admin = 1 << 2,
        //  8
        SuperAdmin = 1 << 3
    }
}
