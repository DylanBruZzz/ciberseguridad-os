# Especificacion de importacion Roadmap V1

## 1. Fuente canonica

La fuente original e inmutable es:

```text
docs/roadmap-original/cybersecurity_roadmap_original.html
```

El runtime y la capa Application no deben parsear este HTML. El flujo aprobado es:

```text
HTML original -> data/roadmap/roadmap-v1.json -> importador futuro
```

## 2. Artefacto normalizado

El artefacto consumible por el futuro importador es:

```text
data/roadmap/roadmap-v1.json
```

Contiene informacion persistible V1 y metadata documental separada. No contiene GUIDs de base de datos ni IDs E2E.

## 3. Que se persiste en V1

- `roadmap.Fase`: orden, nombre, objetivos, criterios descriptivos de avance, meses relativos recomendados y carga semanal recomendada.
- `roadmap.Tema`: temas clave directamente bajo fase, con `TipoConocimiento` conservador.
- `study.Herramienta`: catalogo global deduplicado por nombre normalizado; categoria global cuando es razonable.
- `roadmap.Certificacion`: catalogo global con nombre, proveedor y `TipoCosto`.
- `resource.Recurso`: recursos nombrados por el HTML, con `Url = null` cuando la fuente no trae URL.

## 4. Que NO se persiste en V1

- Sesiones de estudio.
- Evidence real: Proyecto, Laboratorio, Writeup, ArtefactoTecnico, CertificacionObtenida, Nota.
- Evidence planificada.
- Portafolio futuro como entidades.
- Mercado laboral, salarios, demanda o proyecciones.
- SnapshotProgreso.
- `roadmap.vw_TemaEstado`.
- TemaDependencia.
- Competencias nuevas.
- FaseHerramienta o prioridad contextual persistida.

## 5. Politica de normalizacion de temas

La politica V1 es normalizacion selectiva conservadora.

Por defecto se preservan agrupaciones originales como:

- Modelo OSI / TCP-IP
- DNS, DHCP, HTTP/S
- Splunk / Wazuh
- SQLi / XSS / SSRF
- Docker / Kubernetes security

Solo se divide un tema compuesto si la fuente separa explicitamente objetivos, recursos o criterios propios para cada parte. En esta especificacion inicial no se divide ningun tema compuesto.

## 6. Objetivos y criterios

Los objetivos del HTML son principalmente objetivos de fase. Se persisten en `Fase.Objetivos`.

Los criterios de avance del HTML son criterios pedagogicos de fase. Se persisten en `Fase.CriteriosAvance`.

No se generan `Tema.Objetivos` salvo que exista objetivo explicitamente atribuible a un tema. En V1 inicial, los temas importados usan `objetivos: []`.

No se generan `CriterioTema` a partir de criterios de fase.

## 7. Idempotencia

El JSON usa `sourceKey` como identificador estable del dataset. No es un GUID de dominio.

El importador futuro debe resolver:

- Fase: por `UsuarioId + Orden` y/o `sourceKey`.
- Tema: por `UsuarioId + Fase + Nombre + TemaPadre`.
- Herramienta: por `Nombre` global normalizado.
- Certificacion: por `Nombre` global normalizado.
- Recurso: por `UsuarioId + Titulo + Tipo + Url`.
- Relaciones: por claves del dataset y PK fisicas ya existentes.

Una importacion repetida no debe duplicar fases, temas, herramientas, certificaciones, recursos ni relaciones.

## 8. Source keys

Cada elemento relevante del dataset tiene `sourceKey`.

Ejemplos:

```text
fase-01
tema-f01-modelo-osi-tcpip
herramienta-wireshark
cert-comptia-security-plus
recurso-f01-cisco-netacad-it-essentials-ccna
```

Las relaciones internas del JSON apuntan a `sourceKey`, nunca a GUIDs.

## 9. Ownership

El futuro importador recibira un Usuario objetivo existente.

Se debe persistir por usuario:

- Fase.
- Tema.
- Recurso.

Se debe persistir como catalogo global:

- Herramienta.
- Certificacion.

No se debe agregar `UsuarioId` a Herramienta ni Certificacion.

## 10. Orden futuro de importacion

Orden recomendado:

1. Validar que el usuario objetivo existe.
2. Cargar y validar `roadmap-v1.json`.
3. Importar o actualizar Fases.
4. Importar Temas bajo Fase.
5. Importar Herramientas globales.
6. Importar Certificaciones globales.
7. Importar Recursos del usuario.
8. Crear relaciones permitidas que tengan mapping inequivoco.

No importar en este flujo:

- Sesiones.
- Evidence.
- CertificacionObtenida.
- Notas.
- Snapshot.
- Conector.

## 11. Resolucion de relaciones

Las relaciones futuras deben resolverse por `sourceKey`.

En V1 inicial:

- `CertificacionTema` queda vacio porque la fuente no asigna certificaciones a temas concretos.
- `CompetenciaTema` queda vacio porque la fuente no define competencias formales.
- `TemaDependencia` queda vacio por decision arquitectonica diferida.
- `RecursoTema` solo debe crearse cuando exista mapping inequivoco; el dataset conserva recursos por fase como trazabilidad, no como FK persistente.

## 12. Prohibicion de Evidence planificada

Los proyectos, labs y entregables de portafolio del HTML son planes pedagogicos futuros. No son evidencia realizada por el usuario.

El importador V1 debe ignorar:

- `documental.proyectosPlanificadosPorFase`
- `documental.homeLab`
- `documental.portafolioFuturo`

No debe crear Proyecto, Laboratorio, Writeup ni ArtefactoTecnico a partir de esos textos.

## 13. Peso de CertificacionTema

`CertificacionTema.Peso` permanece semantica y operativamente diferido.

Si en una version futura se crean relaciones `CertificacionTema`, `Peso` debe quedar `null` hasta aprobar una formula.

## 14. TemaDependencia

TemaDependencia queda fuera de la importacion V1.

Motivos:

- requiere estrategia completa de aciclicidad;
- requiere definir garantia concurrente para read -> validate -> insert;
- no es necesaria para importacion inicial.

## 15. Competencias

El HTML no trae una taxonomia formal equivalente a `roadmap.Competencia`.

No se crean competencias nuevas en V1. Quedan para auditoria pedagogica posterior.

## 16. Politica de errores

El importador futuro debe fallar de forma explicita si:

- el JSON no es valido;
- existen `sourceKey` duplicados donde deben ser unicos;
- falta una referencia interna;
- hay fases distintas a orden 1..7 en V1;
- los meses recomendados son incoherentes;
- aparece Evidence persistible planificada;
- aparece `Peso` distinto de `null`;
- un enum no existe en el dominio.

## 17. Transaccion futura

La importacion debe ejecutarse en una transaccion unica por usuario.

Si falla cualquier paso persistente, no debe quedar una importacion parcial.

## 18. Versionado

`roadmap-v1.json` es versionable y revisable. Cambios posteriores al dataset deben pasar por diff humano antes del importador.

El HTML original no se modifica.
