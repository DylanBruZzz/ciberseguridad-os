/* ======================================================================
   MODELO RELACIONAL — Plataforma de gestión de aprendizaje en Ciberseguridad
   Fase 1: Traducción del modelo de dominio (DDD) a esquema SQL Server
   ======================================================================
   Regla general de borrado aplicada en todo el script — SIN EXCEPCIONES no documentadas:
   - FK -> nucleo.Usuario                         => NO ACTION (sin excepción alguna;
           el borrado de cuenta es una operación explícita orquestada por la aplicación)
   - FK -> roadmap.Tema (desde cualquier otro
           módulo o tabla de unión)                => NO ACTION
   - FK -> catálogos compartidos (Herramienta,
           Certificacion)                          => NO ACTION
   - FK -> agregado dueño de una entidad interna
           o tabla de unión de su propio módulo    => CASCADE
   - FK entre dos Aggregate Roots del mismo módulo
           que son referencia de agrupación opcional
           (único caso: Tema -> Fase)               => SET NULL
           (Fase es metadato de organización visual; perder la agrupación
           no implica ninguna pérdida de información de aprendizaje)

   Nota de diseño: Nota (evidence.Nota) es un Aggregate Root independiente,
   NO una entidad interna de Proyecto/Laboratorio/Writeup/Artefacto — por eso
   sus 5 FKs son NO ACTION uniformemente, igual que su FK hacia Tema.
   ====================================================================== */

------------------------------------------------------------------------
-- SCHEMAS (uno por módulo / bounded context)
------------------------------------------------------------------------
CREATE SCHEMA nucleo;
GO
CREATE SCHEMA roadmap;
GO
CREATE SCHEMA study;
GO
CREATE SCHEMA evidence;
GO
CREATE SCHEMA resource;
GO
CREATE SCHEMA analytics;
GO
CREATE SCHEMA integration;
GO

------------------------------------------------------------------------
-- NUCLEO: Usuario (shared kernel, dueño transitivo de todo)
------------------------------------------------------------------------
CREATE TABLE nucleo.Usuario (
    Id                              UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Usuario.Registrar(...)
    Nombre                          NVARCHAR(200)    NOT NULL,
    Email                           NVARCHAR(320)    NOT NULL,
    FechaRegistro                   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    IntervaloRepasoDefectoDias      INT              NOT NULL DEFAULT 30,
    CertificacionObjetivoActivaId   UNIQUEIDENTIFIER NULL,   -- FK diferida, ver ALTER al final del bloque roadmap
    FechaCreacionUtc                DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc            DATETIME2        NULL,
    FechaEliminacionUtc             DATETIME2        NULL,   -- borrado lógico: Usuario sí implementa IEliminableLogicamente
    RowVersion                      ROWVERSION,
    CONSTRAINT PK_Usuario PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuario_Email UNIQUE (Email)
);
GO

------------------------------------------------------------------------
-- ROADMAP ENGINE
------------------------------------------------------------------------

CREATE TABLE roadmap.Fase (
    Id          UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Fase.Crear(...)
    UsuarioId   UNIQUEIDENTIFIER NOT NULL,
    Nombre      NVARCHAR(150)    NOT NULL,
    Orden       INT              NOT NULL,
    Color       NVARCHAR(20)     NULL,
    Descripcion NVARCHAR(1000)   NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: Fase no implementa IEliminableLogicamente (metadato
    -- organizativo, no historia de aprendizaje; Tema.FaseId ya es SET NULL si se elimina).
    CONSTRAINT PK_Fase PRIMARY KEY (Id),
    CONSTRAINT FK_Fase_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Fase_Usuario_Orden UNIQUE (UsuarioId, Orden)
);
GO
CREATE INDEX IX_Fase_UsuarioId ON roadmap.Fase (UsuarioId);
GO

-- Catálogo global de certificaciones (compartido entre usuarios; ver justificación más abajo)
CREATE TABLE roadmap.Certificacion (
    Id          UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Certificacion.Crear(...)
    Nombre      NVARCHAR(200)    NOT NULL,
    Proveedor   NVARCHAR(150)    NULL,
    TipoCosto   NVARCHAR(20)     NOT NULL,
    Url         NVARCHAR(500)    NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: catálogo compartido, no implementa IEliminableLogicamente.
    CONSTRAINT PK_Certificacion PRIMARY KEY (Id),
    CONSTRAINT CK_Certificacion_TipoCosto CHECK (TipoCosto IN ('Gratuita','Pago')),
    CONSTRAINT UQ_Certificacion_Nombre UNIQUE (Nombre)
);
GO

ALTER TABLE nucleo.Usuario
    ADD CONSTRAINT FK_Usuario_CertificacionObjetivo
        FOREIGN KEY (CertificacionObjetivoActivaId)
        REFERENCES roadmap.Certificacion (Id) ON DELETE NO ACTION;
GO

CREATE TABLE roadmap.Tema (
    Id                  UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Tema.Crear(...)
    UsuarioId           UNIQUEIDENTIFIER NOT NULL,
    FaseId              UNIQUEIDENTIFIER NULL,
    TemaPadreId         UNIQUEIDENTIFIER NULL,
    Nombre              NVARCHAR(200)    NOT NULL,
    Descripcion         NVARCHAR(MAX)    NULL,
    Objetivos           NVARCHAR(MAX)    NULL,   -- lista de objetivos, texto estructurado (una línea por objetivo)
    TipoConocimiento    NVARCHAR(20)     NOT NULL,
    FechaInicio         DATE             NULL,
    FechaFin            DATE             NULL,
    DificultadPercibida TINYINT          NULL,
    Confianza           TINYINT          NULL,
    IntervaloRepasoDias INT              NULL,
    FechaCreacionUtc    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2       NULL,
    FechaEliminacionUtc DATETIME2        NULL,   -- borrado lógico (Hallazgo 1): Tema tiene valor histórico
    RowVersion          ROWVERSION,
    CONSTRAINT PK_Tema PRIMARY KEY (Id),
    -- Usuario: NO ACTION — el borrado de una cuenta es una operación explícita orquestada
    -- por la aplicación (sin excepciones en todo el esquema), nunca una cascada automática
    CONSTRAINT FK_Tema_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Tema_Fase FOREIGN KEY (FaseId)
        REFERENCES roadmap.Fase (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Tema_TemaPadre FOREIGN KEY (TemaPadreId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Tema_TipoConocimiento CHECK (TipoConocimiento IN ('Conceptual','Procedimental','Herramienta')),
    CONSTRAINT CK_Tema_Dificultad CHECK (DificultadPercibida IS NULL OR DificultadPercibida BETWEEN 1 AND 5),
    CONSTRAINT CK_Tema_Confianza CHECK (Confianza IS NULL OR Confianza BETWEEN 1 AND 5),
    CONSTRAINT CK_Tema_NoAutoPadre CHECK (TemaPadreId IS NULL OR TemaPadreId <> Id)
);
GO
CREATE INDEX IX_Tema_UsuarioId ON roadmap.Tema (UsuarioId);
CREATE INDEX IX_Tema_FaseId ON roadmap.Tema (FaseId);
CREATE INDEX IX_Tema_TemaPadreId ON roadmap.Tema (TemaPadreId);
GO

-- Entidad interna del agregado Tema: sin repositorio propio, se escribe solo vía Tema
CREATE TABLE roadmap.CriterioTema (
    Id            UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en el constructor internal de CriterioTema
    TemaId        UNIQUEIDENTIFIER NOT NULL,
    TipoCriterio  NVARCHAR(20)     NOT NULL,
    Cumplido      BIT              NOT NULL DEFAULT 0,
    FechaCumplido DATETIME2        NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    CONSTRAINT PK_CriterioTema PRIMARY KEY (Id),
    CONSTRAINT FK_CriterioTema_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE CASCADE,   -- único camino hacia Tema: seguro cascadear
    CONSTRAINT CK_CriterioTema_Tipo CHECK (TipoCriterio IN ('Teoria','Practica','Explicacion','Ejercicios','Laboratorio')),
    CONSTRAINT UQ_CriterioTema_TemaTipo UNIQUE (TemaId, TipoCriterio)
);
GO
CREATE INDEX IX_CriterioTema_TemaId ON roadmap.CriterioTema (TemaId);
GO

-- Grafo de dependencias: entidad de unión explícita, direccional (NO es many-to-many simétrico)
CREATE TABLE roadmap.TemaDependencia (
    TemaId          UNIQUEIDENTIFIER NOT NULL,
    TemaRequisitoId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_TemaDependencia PRIMARY KEY (TemaId, TemaRequisitoId),
    CONSTRAINT FK_TemaDependencia_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_TemaDependencia_TemaRequisito FOREIGN KEY (TemaRequisitoId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_TemaDependencia_NoAutoDependencia CHECK (TemaId <> TemaRequisitoId)
    -- NOTA: la ausencia de ciclos en el grafo NO se puede garantizar con constraints de SQL.
    -- Se valida en un servicio de dominio (IValidadorDependencias) antes de insertar la fila.
);
GO
CREATE INDEX IX_TemaDependencia_TemaRequisitoId ON roadmap.TemaDependencia (TemaRequisitoId);
GO

CREATE TABLE roadmap.Competencia (
    Id          UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Competencia.Crear(...)
    UsuarioId   UNIQUEIDENTIFIER NOT NULL,
    Nombre      NVARCHAR(150)    NOT NULL,
    Descripcion NVARCHAR(1000)   NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: Competencia no implementa IEliminableLogicamente.
    CONSTRAINT PK_Competencia PRIMARY KEY (Id),
    CONSTRAINT FK_Competencia_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Competencia_Usuario_Nombre UNIQUE (UsuarioId, Nombre)
);
GO

CREATE TABLE roadmap.CompetenciaTema (
    CompetenciaId UNIQUEIDENTIFIER NOT NULL,
    TemaId        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_CompetenciaTema PRIMARY KEY (CompetenciaId, TemaId),
    CONSTRAINT FK_CompetenciaTema_Competencia FOREIGN KEY (CompetenciaId)
        REFERENCES roadmap.Competencia (Id) ON DELETE CASCADE,
    CONSTRAINT FK_CompetenciaTema_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_CompetenciaTema_TemaId ON roadmap.CompetenciaTema (TemaId);
GO

CREATE TABLE roadmap.CertificacionTema (
    CertificacionId UNIQUEIDENTIFIER NOT NULL,
    TemaId          UNIQUEIDENTIFIER NOT NULL,
    Peso            DECIMAL(5,2)     NULL,   -- opcional: importancia relativa dentro del temario oficial
    CONSTRAINT PK_CertificacionTema PRIMARY KEY (CertificacionId, TemaId),
    CONSTRAINT FK_CertificacionTema_Certificacion FOREIGN KEY (CertificacionId)
        REFERENCES roadmap.Certificacion (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CertificacionTema_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_CertificacionTema_TemaId ON roadmap.CertificacionTema (TemaId);
GO

------------------------------------------------------------------------
-- STUDY ENGINE
------------------------------------------------------------------------

-- Catálogo global compartido (Nmap, Wireshark, Burp Suite... no se duplican por usuario)
CREATE TABLE study.Herramienta (
    Id        UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Herramienta.Crear(...)
    Nombre    NVARCHAR(150)    NOT NULL,
    Categoria NVARCHAR(50)     NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: catálogo compartido, no implementa IEliminableLogicamente.
    CONSTRAINT PK_Herramienta PRIMARY KEY (Id),
    CONSTRAINT UQ_Herramienta_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE study.SesionEstudio (
    Id               UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en SesionEstudio.Registrar(...)
    UsuarioId        UNIQUEIDENTIFIER NOT NULL,
    TemaId           UNIQUEIDENTIFIER NOT NULL,
    Fecha            DATE             NOT NULL,
    DuracionMinutos  INT              NOT NULL,
    Tipo             NVARCHAR(20)     NOT NULL,
    Notas            NVARCHAR(2000)   NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: SesionEstudio tiene valor histórico
    RowVersion       ROWVERSION,
    CONSTRAINT PK_SesionEstudio PRIMARY KEY (Id),
    CONSTRAINT FK_SesionEstudio_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    -- Restrict deliberado: no se puede borrar un Tema con historial de estudio sin gestión explícita
    CONSTRAINT FK_SesionEstudio_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_SesionEstudio_Duracion CHECK (DuracionMinutos > 0),
    CONSTRAINT CK_SesionEstudio_Tipo CHECK (Tipo IN ('Teoria','Practica','Laboratorio','Repaso'))
);
GO
CREATE INDEX IX_SesionEstudio_TemaId ON study.SesionEstudio (TemaId);
-- Índice compuesto clave para dashboard/heatmap: consultas por usuario + rango de fechas
CREATE INDEX IX_SesionEstudio_Usuario_Fecha ON study.SesionEstudio (UsuarioId, Fecha);
GO

CREATE TABLE study.SesionHerramienta (
    SesionId      UNIQUEIDENTIFIER NOT NULL,
    HerramientaId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_SesionHerramienta PRIMARY KEY (SesionId, HerramientaId),
    CONSTRAINT FK_SesionHerramienta_Sesion FOREIGN KEY (SesionId)
        REFERENCES study.SesionEstudio (Id) ON DELETE CASCADE,
    CONSTRAINT FK_SesionHerramienta_Herramienta FOREIGN KEY (HerramientaId)
        REFERENCES study.Herramienta (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_SesionHerramienta_HerramientaId ON study.SesionHerramienta (HerramientaId);
GO

CREATE TABLE study.EntradaBitacora (
    Id        UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en EntradaBitacora.Escribir(...)
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    TemaId    UNIQUEIDENTIFIER NULL,
    Fecha     DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    Texto     NVARCHAR(MAX)    NOT NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: EntradaBitacora tiene valor histórico
    CONSTRAINT PK_EntradaBitacora PRIMARY KEY (Id),
    CONSTRAINT FK_EntradaBitacora_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    -- NO ACTION, igual que el resto de referencias a Tema desde otros módulos:
    -- no se puede eliminar un Tema con entradas de bitácora sin gestión explícita
    CONSTRAINT FK_EntradaBitacora_Tema FOREIGN KEY (TemaId)
        REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_EntradaBitacora_Usuario_Fecha ON study.EntradaBitacora (UsuarioId, Fecha);
GO

------------------------------------------------------------------------
-- EVIDENCE ENGINE (lo que el usuario produce)
------------------------------------------------------------------------

CREATE TABLE evidence.Proyecto (
    Id             UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Proyecto.Crear(...)
    UsuarioId      UNIQUEIDENTIFIER NOT NULL,
    Nombre         NVARCHAR(200)    NOT NULL,
    Descripcion    NVARCHAR(MAX)    NULL,
    Estado         NVARCHAR(20)     NOT NULL DEFAULT 'Idea',
    EstadoMadurez  NVARCHAR(20)     NOT NULL DEFAULT 'Borrador',
    RepositorioUrl NVARCHAR(500)    NULL,
    FechaInicio    DATE             NULL,
    FechaFin       DATE             NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: Proyecto tiene valor histórico
    RowVersion     ROWVERSION,
    CONSTRAINT PK_Proyecto PRIMARY KEY (Id),
    CONSTRAINT FK_Proyecto_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Proyecto_Estado CHECK (Estado IN ('Idea','Desarrollo','Documentado','Publicado')),
    CONSTRAINT CK_Proyecto_EstadoMadurez CHECK (EstadoMadurez IN ('Borrador','Documentado','ListoPortafolio','Publicado'))
);
GO

CREATE TABLE evidence.Laboratorio (
    Id                    UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Laboratorio.Crear(...)
    UsuarioId             UNIQUEIDENTIFIER NOT NULL,
    Nombre                NVARCHAR(200)    NOT NULL,
    Objetivo              NVARCHAR(1000)   NULL,
    EntornoVms            NVARCHAR(1000)   NULL,
    Hallazgos             NVARCHAR(MAX)    NULL,
    TiempoInvertidoMinutos INT             NULL,
    EstadoMadurez         NVARCHAR(20)     NOT NULL DEFAULT 'Borrador',
    Fecha                 DATE             NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: Laboratorio tiene valor histórico
    CONSTRAINT PK_Laboratorio PRIMARY KEY (Id),
    CONSTRAINT FK_Laboratorio_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Laboratorio_EstadoMadurez CHECK (EstadoMadurez IN ('Borrador','Documentado','ListoPortafolio','Publicado'))
);
GO

CREATE TABLE evidence.Writeup (
    Id                UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Writeup.Crear(...)
    UsuarioId         UNIQUEIDENTIFIER NOT NULL,
    Titulo            NVARCHAR(200)    NOT NULL,
    PlataformaOrigen  NVARCHAR(100)    NULL,
    Url               NVARCHAR(500)    NULL,
    EstadoMadurez     NVARCHAR(20)     NOT NULL DEFAULT 'Borrador',
    Fecha             DATE             NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: Writeup tiene valor histórico
    CONSTRAINT PK_Writeup PRIMARY KEY (Id),
    CONSTRAINT FK_Writeup_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Writeup_EstadoMadurez CHECK (EstadoMadurez IN ('Borrador','Documentado','ListoPortafolio','Publicado'))
);
GO

CREATE TABLE evidence.ArtefactoTecnico (
    Id                  UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en ArtefactoTecnico.Crear(...)
    UsuarioId           UNIQUEIDENTIFIER NOT NULL,
    TipoArtefacto       NVARCHAR(30)     NOT NULL,
    Nombre              NVARCHAR(200)    NOT NULL,
    ContenidoOUrl       NVARCHAR(MAX)    NULL,
    LenguajeTecnologia  NVARCHAR(100)    NULL,
    EstadoMadurez       NVARCHAR(20)     NOT NULL DEFAULT 'Borrador',
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: ArtefactoTecnico tiene valor histórico
    CONSTRAINT PK_ArtefactoTecnico PRIMARY KEY (Id),
    CONSTRAINT FK_ArtefactoTecnico_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_ArtefactoTecnico_Tipo CHECK (TipoArtefacto IN
        ('Script','Herramienta','Cheatsheet','Dashboard','Playbook','ReglaDeteccion','ConsultaSiem','Automatizacion','Plantilla','Otro')),
    CONSTRAINT CK_ArtefactoTecnico_EstadoMadurez CHECK (EstadoMadurez IN ('Borrador','Documentado','ListoPortafolio','Publicado'))
);
GO

CREATE TABLE evidence.CertificacionObtenida (
    Id              UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en CertificacionObtenida.Registrar(...)
    UsuarioId       UNIQUEIDENTIFIER NOT NULL,
    CertificacionId UNIQUEIDENTIFIER NOT NULL,
    FechaObtencion  DATE             NOT NULL,
    EvidenciaUrl    NVARCHAR(500)    NULL,
    EstadoMadurez   NVARCHAR(20)     NOT NULL DEFAULT 'Documentado',
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: CertificacionObtenida tiene valor histórico
    CONSTRAINT PK_CertificacionObtenida PRIMARY KEY (Id),
    CONSTRAINT FK_CertificacionObtenida_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CertificacionObtenida_Certificacion FOREIGN KEY (CertificacionId)
        REFERENCES roadmap.Certificacion (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_CertificacionObtenida_EstadoMadurez CHECK (EstadoMadurez IN ('Borrador','Documentado','ListoPortafolio','Publicado'))
);
GO
CREATE INDEX IX_CertificacionObtenida_UsuarioId ON evidence.CertificacionObtenida (UsuarioId);
GO

-- Tablas de unión evidencia <-> tema
CREATE TABLE evidence.ProyectoTema (
    ProyectoId UNIQUEIDENTIFIER NOT NULL,
    TemaId     UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ProyectoTema PRIMARY KEY (ProyectoId, TemaId),
    CONSTRAINT FK_ProyectoTema_Proyecto FOREIGN KEY (ProyectoId) REFERENCES evidence.Proyecto (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProyectoTema_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE TABLE evidence.LaboratorioTema (
    LaboratorioId UNIQUEIDENTIFIER NOT NULL,
    TemaId        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_LaboratorioTema PRIMARY KEY (LaboratorioId, TemaId),
    CONSTRAINT FK_LaboratorioTema_Laboratorio FOREIGN KEY (LaboratorioId) REFERENCES evidence.Laboratorio (Id) ON DELETE CASCADE,
    CONSTRAINT FK_LaboratorioTema_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE TABLE evidence.WriteupTema (
    WriteupId UNIQUEIDENTIFIER NOT NULL,
    TemaId    UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_WriteupTema PRIMARY KEY (WriteupId, TemaId),
    CONSTRAINT FK_WriteupTema_Writeup FOREIGN KEY (WriteupId) REFERENCES evidence.Writeup (Id) ON DELETE CASCADE,
    CONSTRAINT FK_WriteupTema_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE TABLE evidence.ArtefactoTema (
    ArtefactoTecnicoId UNIQUEIDENTIFIER NOT NULL,
    TemaId             UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ArtefactoTema PRIMARY KEY (ArtefactoTecnicoId, TemaId),
    CONSTRAINT FK_ArtefactoTema_Artefacto FOREIGN KEY (ArtefactoTecnicoId) REFERENCES evidence.ArtefactoTecnico (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ArtefactoTema_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO

-- Índices inversos: la PK compuesta ya cubre la búsqueda "por entidad de evidencia",
-- estos cubren la búsqueda igual de frecuente "todo lo vinculado a este Tema"
CREATE INDEX IX_ProyectoTema_TemaId ON evidence.ProyectoTema (TemaId);
CREATE INDEX IX_LaboratorioTema_TemaId ON evidence.LaboratorioTema (TemaId);
CREATE INDEX IX_WriteupTema_TemaId ON evidence.WriteupTema (TemaId);
CREATE INDEX IX_ArtefactoTema_TemaId ON evidence.ArtefactoTema (TemaId);
GO

-- Tablas de unión evidencia <-> herramienta
CREATE TABLE evidence.ProyectoHerramienta (
    ProyectoId    UNIQUEIDENTIFIER NOT NULL,
    HerramientaId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ProyectoHerramienta PRIMARY KEY (ProyectoId, HerramientaId),
    CONSTRAINT FK_ProyectoHerramienta_Proyecto FOREIGN KEY (ProyectoId) REFERENCES evidence.Proyecto (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProyectoHerramienta_Herramienta FOREIGN KEY (HerramientaId) REFERENCES study.Herramienta (Id) ON DELETE NO ACTION
);
GO
CREATE TABLE evidence.LaboratorioHerramienta (
    LaboratorioId UNIQUEIDENTIFIER NOT NULL,
    HerramientaId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_LaboratorioHerramienta PRIMARY KEY (LaboratorioId, HerramientaId),
    CONSTRAINT FK_LaboratorioHerramienta_Laboratorio FOREIGN KEY (LaboratorioId) REFERENCES evidence.Laboratorio (Id) ON DELETE CASCADE,
    CONSTRAINT FK_LaboratorioHerramienta_Herramienta FOREIGN KEY (HerramientaId) REFERENCES study.Herramienta (Id) ON DELETE NO ACTION
);
GO
CREATE TABLE evidence.ArtefactoHerramienta (
    ArtefactoTecnicoId UNIQUEIDENTIFIER NOT NULL,
    HerramientaId      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ArtefactoHerramienta PRIMARY KEY (ArtefactoTecnicoId, HerramientaId),
    CONSTRAINT FK_ArtefactoHerramienta_Artefacto FOREIGN KEY (ArtefactoTecnicoId) REFERENCES evidence.ArtefactoTecnico (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ArtefactoHerramienta_Herramienta FOREIGN KEY (HerramientaId) REFERENCES study.Herramienta (Id) ON DELETE NO ACTION
);
GO

-- Índices inversos: necesarios para "horas/proyectos por herramienta" en Analytics
CREATE INDEX IX_ProyectoHerramienta_HerramientaId ON evidence.ProyectoHerramienta (HerramientaId);
CREATE INDEX IX_LaboratorioHerramienta_HerramientaId ON evidence.LaboratorioHerramienta (HerramientaId);
CREATE INDEX IX_ArtefactoHerramienta_HerramientaId ON evidence.ArtefactoHerramienta (HerramientaId);
GO

-- Nota: Aggregate Root propio con asociación polimórfica resuelta como 5 FKs nulas + CHECK
CREATE TABLE evidence.Nota (
    Id                 UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en las fábricas Nota.Sobre*(...)
    UsuarioId          UNIQUEIDENTIFIER NOT NULL,
    TemaId             UNIQUEIDENTIFIER NULL,
    ProyectoId         UNIQUEIDENTIFIER NULL,
    LaboratorioId      UNIQUEIDENTIFIER NULL,
    WriteupId          UNIQUEIDENTIFIER NULL,
    ArtefactoTecnicoId UNIQUEIDENTIFIER NULL,
    Fecha              DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    Texto              NVARCHAR(MAX)    NOT NULL,
    Tipo               NVARCHAR(30)     NOT NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: Nota tiene valor histórico
    CONSTRAINT PK_Nota PRIMARY KEY (Id),
    CONSTRAINT FK_Nota_Usuario FOREIGN KEY (UsuarioId) REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    -- Las 5 FKs de Nota son NO ACTION de forma uniforme: Nota es un Aggregate Root
    -- independiente (no una entidad interna de su "padre"), así que borrar un Proyecto,
    -- Laboratorio, Writeup o Artefacto nunca debe arrastrar silenciosamente las notas
    -- que documentan la evolución de tu entendimiento sobre ellos.
    CONSTRAINT FK_Nota_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Nota_Proyecto FOREIGN KEY (ProyectoId) REFERENCES evidence.Proyecto (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Nota_Laboratorio FOREIGN KEY (LaboratorioId) REFERENCES evidence.Laboratorio (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Nota_Writeup FOREIGN KEY (WriteupId) REFERENCES evidence.Writeup (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Nota_Artefacto FOREIGN KEY (ArtefactoTecnicoId) REFERENCES evidence.ArtefactoTecnico (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Nota_Tipo CHECK (Tipo IN ('Nota','Hallazgo','Actualizacion','Autoexplicacion')),
    -- Exactamente un padre debe estar informado (integridad de la asociación polimórfica)
    CONSTRAINT CK_Nota_UnSoloPadre CHECK (
        (CASE WHEN TemaId IS NOT NULL THEN 1 ELSE 0 END) +
        (CASE WHEN ProyectoId IS NOT NULL THEN 1 ELSE 0 END) +
        (CASE WHEN LaboratorioId IS NOT NULL THEN 1 ELSE 0 END) +
        (CASE WHEN WriteupId IS NOT NULL THEN 1 ELSE 0 END) +
        (CASE WHEN ArtefactoTecnicoId IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
GO
-- Índices filtrados: cada uno solo indexa las filas donde ese padre aplica (evita indexar NULLs innecesariamente)
CREATE INDEX IX_Nota_TemaId ON evidence.Nota (TemaId) WHERE TemaId IS NOT NULL;
CREATE INDEX IX_Nota_ProyectoId ON evidence.Nota (ProyectoId) WHERE ProyectoId IS NOT NULL;
CREATE INDEX IX_Nota_LaboratorioId ON evidence.Nota (LaboratorioId) WHERE LaboratorioId IS NOT NULL;
CREATE INDEX IX_Nota_WriteupId ON evidence.Nota (WriteupId) WHERE WriteupId IS NOT NULL;
CREATE INDEX IX_Nota_ArtefactoTecnicoId ON evidence.Nota (ArtefactoTecnicoId) WHERE ArtefactoTecnicoId IS NOT NULL;
GO

------------------------------------------------------------------------
-- RESOURCE LIBRARY
------------------------------------------------------------------------

CREATE TABLE resource.Recurso (
    Id                UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Recurso.Guardar(...)
    UsuarioId         UNIQUEIDENTIFIER NOT NULL,
    Tipo              NVARCHAR(30)     NOT NULL,
    Titulo            NVARCHAR(300)    NOT NULL,
    Url               NVARCHAR(500)    NULL,
    Estado            NVARCHAR(20)     NOT NULL DEFAULT 'PorClasificar',
    Rating            TINYINT          NULL,
    Notas             NVARCHAR(2000)   NULL,
    HerramientaIA     NVARCHAR(100)    NULL,   -- generalización deliberada: no acoplado a NotebookLM específicamente
    PromptsUtilizados NVARCHAR(MAX)    NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    FechaEliminacionUtc  DATETIME2 NULL,   -- borrado lógico: Recurso tiene valor histórico
    CONSTRAINT PK_Recurso PRIMARY KEY (Id),
    CONSTRAINT FK_Recurso_Usuario FOREIGN KEY (UsuarioId) REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Recurso_Tipo CHECK (Tipo IN
        ('Documentacion','Libro','Curso','Video','Laboratorio','Writeup','Cheatsheet','Script','RepositorioGitHub','NotebookIA','Otro')),
    CONSTRAINT CK_Recurso_Estado CHECK (Estado IN ('PorClasificar','PorRevisar','EnUso','Consultado','Referencia')),
    CONSTRAINT CK_Recurso_Rating CHECK (Rating IS NULL OR Rating BETWEEN 1 AND 5)
);
GO

CREATE TABLE resource.RecursoTema (
    RecursoId UNIQUEIDENTIFIER NOT NULL,
    TemaId    UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_RecursoTema PRIMARY KEY (RecursoId, TemaId),
    CONSTRAINT FK_RecursoTema_Recurso FOREIGN KEY (RecursoId) REFERENCES resource.Recurso (Id) ON DELETE CASCADE,
    CONSTRAINT FK_RecursoTema_Tema FOREIGN KEY (TemaId) REFERENCES roadmap.Tema (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_RecursoTema_TemaId ON resource.RecursoTema (TemaId);
GO

------------------------------------------------------------------------
-- ANALYTICS ENGINE
------------------------------------------------------------------------

CREATE TABLE analytics.SnapshotProgreso (
    Id               UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en SnapshotProgreso.Generar(...)
    UsuarioId        UNIQUEIDENTIFIER NOT NULL,
    Fecha            DATE             NOT NULL,
    PorcentajeGlobal DECIMAL(5,2)     NOT NULL,
    HorasTotales     DECIMAL(10,2)    NOT NULL,
    TemasDominados   INT              NOT NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: SnapshotProgreso no implementa IEliminableLogicamente (dato
    -- operativo/derivado, no historia de aprendizaje directa — excluido explícitamente en la convención 5).
    CONSTRAINT PK_SnapshotProgreso PRIMARY KEY (Id),
    CONSTRAINT FK_SnapshotProgreso_Usuario FOREIGN KEY (UsuarioId) REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_SnapshotProgreso_Usuario_Fecha UNIQUE (UsuarioId, Fecha)
);
GO

------------------------------------------------------------------------
-- CAPA DE INTEGRACIÓN
------------------------------------------------------------------------

CREATE TABLE integration.Conector (
    Id                    UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Conector.Crear(...)
    UsuarioId             UNIQUEIDENTIFIER NOT NULL,
    Plataforma            NVARCHAR(50)     NOT NULL,
    EstadoConexion        NVARCHAR(20)     NOT NULL DEFAULT 'Desconectado',
    UltimaSincronizacion  DATETIME2        NULL,
    CredencialRef         NVARCHAR(200)    NULL,   -- referencia a secret store externo; nunca credenciales en claro
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: Conector no implementa IEliminableLogicamente (excluido explícitamente en la convención 5).
    CONSTRAINT PK_Conector PRIMARY KEY (Id),
    CONSTRAINT FK_Conector_Usuario FOREIGN KEY (UsuarioId) REFERENCES nucleo.Usuario (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Conector_Plataforma CHECK (Plataforma IN ('GitHub','TryHackMe','HackTheBox','NotebookLM','Otro')),
    CONSTRAINT CK_Conector_Estado CHECK (EstadoConexion IN ('Conectado','Desconectado','Error')),
    CONSTRAINT UQ_Conector_Usuario_Plataforma UNIQUE (UsuarioId, Plataforma)
);
GO

CREATE TABLE integration.LogSincronizacion (
    Id          UNIQUEIDENTIFIER NOT NULL,   -- sin DEFAULT: el Guid v7 nace exclusivamente en Conector.RegistrarLog(...)
    ConectorId  UNIQUEIDENTIFIER NOT NULL,
    Fecha       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    Resultado   NVARCHAR(20)     NOT NULL,
    Resumen     NVARCHAR(1000)   NULL,
    FechaCreacionUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacionUtc DATETIME2 NULL,
    -- Sin FechaEliminacionUtc: entidad interna sin borrado lógico propio, ligada al ciclo de vida de Conector.
    CONSTRAINT PK_LogSincronizacion PRIMARY KEY (Id),
    CONSTRAINT FK_LogSincronizacion_Conector FOREIGN KEY (ConectorId)
        REFERENCES integration.Conector (Id) ON DELETE CASCADE,
    CONSTRAINT CK_LogSincronizacion_Resultado CHECK (Resultado IN ('Exito','Error','Parcial'))
);
GO
CREATE INDEX IX_LogSincronizacion_Conector_Fecha ON integration.LogSincronizacion (ConectorId, Fecha);
GO

------------------------------------------------------------------------
-- VISTA: estado calculado de Tema (NUNCA se persiste como columna)
-- Traduce la regla de negocio: dominado = 100% criterios + práctica < 90 días;
-- degradado a "En repaso" si pasó el intervalo sin practicar.
------------------------------------------------------------------------
CREATE VIEW roadmap.vw_TemaEstado AS
WITH CriteriosPorTema AS (
    SELECT
        TemaId,
        COUNT(*)                                   AS TotalCriterios,
        SUM(CASE WHEN Cumplido = 1 THEN 1 ELSE 0 END) AS CriteriosCumplidos
    FROM roadmap.CriterioTema
    GROUP BY TemaId
),
UltimaPractica AS (
    SELECT TemaId, MAX(Fecha) AS UltimaFecha
    FROM study.SesionEstudio
    GROUP BY TemaId
)
SELECT
    t.Id AS TemaId,
    ISNULL(c.CriteriosCumplidos, 0)                    AS CriteriosCumplidos,
    ISNULL(c.TotalCriterios, 0)                        AS TotalCriterios,
    up.UltimaFecha                                     AS UltimaPractica,
    CASE
        WHEN ISNULL(c.TotalCriterios, 0) = 0 THEN 'NoIniciado'
        WHEN c.CriteriosCumplidos = 0 THEN 'NoIniciado'
        WHEN c.CriteriosCumplidos < c.TotalCriterios THEN 'EnPractica'
        WHEN c.CriteriosCumplidos = c.TotalCriterios
             AND up.UltimaFecha IS NOT NULL
             AND DATEDIFF(DAY, up.UltimaFecha, SYSUTCDATETIME()) > ISNULL(t.IntervaloRepasoDias, 90)
            THEN 'EnRepaso'
        WHEN c.CriteriosCumplidos = c.TotalCriterios THEN 'Dominado'
        ELSE 'NoIniciado'
    END AS Estado
FROM roadmap.Tema t
LEFT JOIN CriteriosPorTema c ON c.TemaId = t.Id
LEFT JOIN UltimaPractica up ON up.TemaId = t.Id;
GO
