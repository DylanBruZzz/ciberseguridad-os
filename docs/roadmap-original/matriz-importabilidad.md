# Matriz de importabilidad Roadmap V1

Fuente original: `docs/roadmap-original/cybersecurity_roadmap_dylan.html`

Esta matriz define la trazabilidad HTML -> JSON -> destino futuro. El HTML permanece como fuente original inmutable. El JSON normalizado es el artefacto consumible por el futuro importador.

| Fuente HTML | Elemento | JSON | Destino futuro | Persistir V1 | Transformacion | Observaciones |
|---|---|---|---|---|---|---|
| Header | Duracion 4 anos | `fuente.resumenOriginal.anios` | Ninguno | No | Conservado como metadata | No existe entidad Ano. |
| Header | 7 fases | `fases[]` | `roadmap.Fase` | Si | Orden 1..7 | Cada fase usa `sourceKey` estable, no GUID. |
| Tab Fases | Nombre de fase | `fases[].nombre` | `roadmap.Fase.Nombre` | Si | Texto literal normalizado | Se mantiene la agrupacion original. |
| Tab Fases | Periodo "Meses X-Y" | `fases[].mesInicioRecomendado`, `mesFinRecomendado` | `roadmap.Fase` | Si | Meses relativos enteros | No son fechas reales de estudio. |
| Tab Fases | Carga "~10 hrs/semana" / "Tiempo completo" | `fases[].cargaSemanalRecomendada` | `roadmap.Fase.CargaSemanalRecomendada` | Si | Texto literal | No se extraen horas numericas. |
| Tab Fases | Objetivos de fase | `fases[].objetivos` | `roadmap.Fase.Objetivos` | Si | Lista textual | No se copian a `Tema.Objetivos`. |
| Tab Fases | Criterios para avanzar | `fases[].criteriosAvance` | `roadmap.Fase.CriteriosAvance` | Si | Lista textual | No se convierten en `CriterioTema`. |
| Tab Fases | Temas clave | `fases[].temas[]` | `roadmap.Tema` | Si | Normalizacion selectiva conservadora | Por defecto se preservan nombres compuestos. |
| Tab Fases | Tags visuales de fase | `fases[].metadataDocumental.tags` | Ninguno | No | Conservado como metadata documental | No hay campo persistente. |
| Tab Fases | Alertas de prerequisito o recomendacion | `fases[].metadataDocumental.notas` | Ninguno | No | Texto documental | No se convierte en regla. |
| Tab Fases | Recursos principales | `recursos[]` | `resource.Recurso` | Si, sin relacion si no hay tema inequivoco | Titulo, tipo, url null si no existe | No se inventan URLs. |
| Tab Fases | Proyectos practicos | `documental.proyectosPlanificadosPorFase[]` | Ninguno | No | Texto documental | No se crea Evidence planificada. |
| Tab Lab | Home lab por pasos | `documental.homeLab` | Ninguno | No | Texto documental | Puede alimentar herramientas globales si hay tecnologia explicita. |
| Tab Lab | VMs, distribuciones y objetivos vulnerables | `herramientas[]` y `documental.homeLab` | `study.Herramienta` para tecnologias explicitas | Parcial | Herramientas explicitas como catalogo; plan como documental | No se crea Laboratorio Evidence. |
| Tab Certificaciones | Certificaciones | `certificaciones[]` | `roadmap.Certificacion` | Si | Nombre, proveedor, tipoCosto | Descripcion, nivel y costo quedan documentales. |
| Tab Certificaciones | Costo textual | `certificaciones[].metadataDocumental.costoFuente` | Ninguno | No | Texto literal documental | Solo `TipoCosto` se persiste. |
| Tab Certificaciones | Nivel comercial | `certificaciones[].metadataDocumental.nivelFuente` | Ninguno | No | Texto literal documental | No se modela governance ni ranking. |
| Alertas de fase | Certificacion objetivo de fase | `certificaciones[].metadataDocumental.fasesSugeridas` | Ninguno | No | Asociacion documental a fase | No fabrica `CertificacionTema`. |
| CertificacionTema | Temario certificado-tema | `relacionesCertificacionTema[]` | `roadmap.CertificacionTema` | No en V1 inicial | Lista vacia | El HTML no asigna certificaciones a temas concretos; `Peso` sigue null/diferido. |
| Tab Herramientas | Herramientas por categoria | `herramientas[]` | `study.Herramienta` | Si | Deduplicacion por nombre normalizado | `Categoria` se persiste si es razonable. |
| Tab Herramientas | Prioridad | `herramientas[].metadataDocumental.contextos[].prioridad` | Ninguno | No | Metadata contextual | No existe `FaseHerramienta`. |
| Tab Herramientas | Fase recomendada | `herramientas[].metadataDocumental.contextos[].faseSourceKey` | Ninguno | No | Metadata contextual | No agregar `FaseId` a Herramienta global. |
| Tab Portfolio | Portafolio futuro | `documental.portafolioFuturo` | Ninguno | No | Texto documental | No se convierte en Proyecto/Lab/Writeup/Artefacto. |
| Tab Mercado Laboral | Salarios/demanda/proyecciones | No copiado al JSON, solo referencia en especificacion | Ninguno | No | Se conserva en HTML | Datos temporales/documentales. |
| Roadmap conceptual | Competencias | `competencias` | `roadmap.Competencia` | No | Lista vacia | No hay taxonomia formal de Competencia en la fuente. |
| Roadmap conceptual | Prerequisitos transversales | `temaDependencias` | `roadmap.TemaDependencia` | No | Lista vacia | TemaDependencia sigue diferida por aciclicidad/concurrencia. |
| Dataset | Source keys | `sourceKey` | Lookup del importador | No como dato de dominio | Identificador estable del dataset | El dominio genera GUID v7 reales. |
