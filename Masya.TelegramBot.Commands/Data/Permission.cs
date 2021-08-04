namespace Masya.TelegramBot.Commands.Data
{
    public enum Permission
    {
        User = 1<<0,
        Agent = 1<<1,
        Admin = 1<<2,
        SuperAdmin = 1<<3,
        All = User | Agent | Admin | SuperAdmin
    }
}
