namespace GrandDevs.Tavern
{
    public interface IServiceLocator
    {
        T GetService<T>();
        void Update();
    }
}