param(
    [string]$ServerInstance = ".\MSSQLSERVER01",
    [string]$Database = "AprendizajePersonalDb",
    [string]$BackupDirectory
)

$ErrorActionPreference = "Stop"

function Write-Failure {
    param([string]$Message)

    Write-Error $Message
    exit 1
}

function New-SqlConnection {
    param(
        [string]$Server,
        [string]$InitialCatalog
    )

    $connectionString = "Server=$Server;Database=$InitialCatalog;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True"
    $connection = [System.Data.SqlClient.SqlConnection]::new($connectionString)
    $connection.Open()
    return $connection
}

function Invoke-SqlScalar {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$CommandText
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = $CommandText
    $command.CommandTimeout = 0
    return $command.ExecuteScalar()
}

function Invoke-SqlNonQuery {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$CommandText
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = $CommandText
    $command.CommandTimeout = 0
    [void]$command.ExecuteNonQuery()
}

function Escape-SqlLiteral {
    param([string]$Value)
    return $Value.Replace("'", "''")
}

function Quote-SqlIdentifier {
    param([string]$Value)
    return "[" + $Value.Replace("]", "]]") + "]"
}

try {
    if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
        Write-Failure "sqlcmd no esta disponible en PATH. Instala las herramientas de linea de comandos de SQL Server antes de ejecutar este script."
    }

    if ([string]::IsNullOrWhiteSpace($Database)) {
        Write-Failure "El parametro -Database no puede estar vacio."
    }

    $connection = New-SqlConnection -Server $ServerInstance -InitialCatalog "master"

    try {
        $databaseLiteral = Escape-SqlLiteral $Database
        $state = Invoke-SqlScalar -Connection $connection -CommandText "SELECT state_desc FROM sys.databases WHERE name = N'$databaseLiteral';"

        if ($null -eq $state) {
            Write-Failure "La base de datos '$Database' no existe en '$ServerInstance'."
        }

        if ($state -ne "ONLINE") {
            Write-Failure "La base de datos '$Database' no esta ONLINE. Estado actual: $state."
        }

        if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
            $BackupDirectory = [string](Invoke-SqlScalar -Connection $connection -CommandText "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'));")

            if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
                Write-Failure "SQL Server no devolvio InstanceDefaultBackupPath. Proporciona -BackupDirectory explicitamente."
            }
        }

        if (-not $BackupDirectory.EndsWith("\")) {
            $BackupDirectory += "\"
        }

        $backupPath = $null
        for ($attempt = 0; $attempt -lt 5; $attempt++) {
            $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
            $candidate = Join-Path $BackupDirectory "$Database`_$timestamp.bak"
            $candidateLiteral = Escape-SqlLiteral $candidate
            $exists = Invoke-SqlScalar -Connection $connection -CommandText "DECLARE @result int; EXEC master.dbo.xp_fileexist N'$candidateLiteral', @result OUTPUT; SELECT @result;"

            if ([int]$exists -eq 0) {
                $backupPath = $candidate
                break
            }

            Start-Sleep -Seconds 1
        }

        if ([string]::IsNullOrWhiteSpace($backupPath)) {
            Write-Failure "No se pudo generar un nombre de backup libre en '$BackupDirectory'."
        }

        $backupPathLiteral = Escape-SqlLiteral $backupPath
        $databaseIdentifier = Quote-SqlIdentifier $Database
        $backupNameLiteral = Escape-SqlLiteral "$Database manual backup $(Get-Date -Format 'yyyyMMdd_HHmmss')"

        $backupSql = "BACKUP DATABASE $databaseIdentifier TO DISK = N'$backupPathLiteral' WITH COPY_ONLY, CHECKSUM, NOFORMAT, NOINIT, NAME = N'$backupNameLiteral', STATS = 10;"
        Invoke-SqlNonQuery -Connection $connection -CommandText $backupSql

        $verifySql = "RESTORE VERIFYONLY FROM DISK = N'$backupPathLiteral' WITH CHECKSUM;"
        Invoke-SqlNonQuery -Connection $connection -CommandText $verifySql

        $size = Invoke-SqlScalar -Connection $connection -CommandText @"
SELECT TOP (1) bs.backup_size
FROM msdb.dbo.backupset bs
JOIN msdb.dbo.backupmediafamily mf ON mf.media_set_id = bs.media_set_id
WHERE bs.database_name = N'$databaseLiteral'
  AND mf.physical_device_name = N'$backupPathLiteral'
ORDER BY bs.backup_finish_date DESC;
"@

        Write-Host "Backup Personal completado"
        Write-Host ""
        Write-Host "Database: $Database"
        Write-Host "Archivo: $backupPath"
        Write-Host "Tamano: $size bytes"
        Write-Host "RESTORE VERIFYONLY: PASS"
    }
    finally {
        if ($connection) {
            $connection.Dispose()
        }
    }
}
catch {
    Write-Error "Backup Personal fallo. El backup no debe considerarse valido. Detalle: $($_.Exception.Message)"
    exit 1
}
