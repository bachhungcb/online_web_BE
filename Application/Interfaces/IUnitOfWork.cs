namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
   
    int Complete();
}