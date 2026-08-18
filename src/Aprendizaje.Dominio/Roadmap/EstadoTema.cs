namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Estado calculado, nunca persistido. Debe mantenerse en paridad exacta con
/// roadmap.vw_TemaEstado — ver convención 20 (verificación de paridad).
/// </summary>
public enum EstadoTema
{
    NoIniciado,
    EnPractica,
    Dominado,
    EnRepaso
}
