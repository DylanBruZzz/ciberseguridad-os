using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;

public sealed record CrearArtefactoTecnicoSolicitud(Guid UsuarioId, TipoArtefacto TipoArtefacto, string Nombre);
