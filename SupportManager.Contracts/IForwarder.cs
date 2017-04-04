namespace SupportManager.Contracts
{
    public interface IForwarder
    {
        void ApplyForward(int id);

        void ReadStatus();
    }
}