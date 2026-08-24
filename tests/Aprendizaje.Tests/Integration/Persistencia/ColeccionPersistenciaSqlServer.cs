using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[CollectionDefinition(Nombre, DisableParallelization = true)]
public sealed class ColeccionPersistenciaSqlServer
{
    public const string Nombre = "Persistencia SQL Server";
}
