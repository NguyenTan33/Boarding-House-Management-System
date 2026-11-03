namespace Application.Interfaces.Auth
{
    public interface IPhoneService
    {
        bool IsValid(string phoneNumber);
        string Normalize(string phoneNumber);
    }
}
