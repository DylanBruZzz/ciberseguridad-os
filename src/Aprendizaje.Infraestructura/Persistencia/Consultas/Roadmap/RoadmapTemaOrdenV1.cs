namespace Aprendizaje.Infraestructura.Persistencia.Consultas.Roadmap;

internal static class RoadmapTemaOrdenV1
{
    private const int OrdenDesconocido = int.MaxValue;

    private static readonly IReadOnlyDictionary<int, FaseOrdenV1> Fases = new Dictionary<int, FaseOrdenV1>
    {
        [1] = FaseOrden(
            "Fundamentos de Informática y Redes",
            [
                "Modelo OSI / TCP-IP",
                "Routing & Switching",
                "Subnetting / CIDR",
                "DNS, DHCP, HTTP/S",
                "SSH, FTP, SMB",
                "Permisos Linux",
                "Bash scripting",
                "Virtualización",
                "Firewall básico",
                "Wireshark basics",
            ]),
        [2] = FaseOrden(
            "Sistemas Operativos, Scripting y Virtualización",
            [
                "Gestión de usuarios",
                "Active Directory",
                "Group Policy (GPO)",
                "PowerShell scripting",
                "Cron jobs",
                "Python para seguridad",
                "Event Viewer / Syslog",
                "Hardening SSOO",
                "VMware Workstation",
                "Kali Linux intro",
            ]),
        [3] = FaseOrden(
            "Fundamentos de Ciberseguridad y Blue Team",
            [
                "Cyber Kill Chain",
                "MITRE ATT&CK Framework",
                "Threat Intelligence",
                "OSINT básico",
                "Criptografía simétrica/asimétrica",
                "PKI y certificados TLS",
                "Análisis de logs",
                "Splunk / Wazuh",
                "IDS/IPS (Snort/Suricata)",
                "Nessus / OpenVAS",
            ]),
        [4] = FaseOrden(
            "Red Team · Ethical Hacking · Pentesting",
            [
                "Nmap / Nessus avanzado",
                "OWASP Top 10",
                "SQLi / XSS / SSRF",
                "Burp Suite Pro",
                "Metasploit Framework",
                "Buffer Overflow básico",
                "AD Attacks",
                "BloodHound / Mimikatz",
                "Pivoting & Tunneling",
                "C2 Frameworks (Cobalt Strike/Havoc)",
                "Escritura de reportes",
            ]),
    };

    public static int ObtenerOrden(int faseOrden, string faseNombre, string temaNombre)
    {
        if (!Fases.TryGetValue(faseOrden, out var fase)
            || !string.Equals(fase.Nombre, faseNombre, StringComparison.Ordinal))
        {
            return OrdenDesconocido;
        }

        return fase.Temas.TryGetValue(temaNombre, out var orden)
            ? orden
            : OrdenDesconocido;
    }

    private static FaseOrdenV1 FaseOrden(string nombre, IReadOnlyList<string> temas) =>
        new(
            nombre,
            temas
                .Select((tema, index) => new { tema, orden = index + 1 })
                .ToDictionary(t => t.tema, t => t.orden, StringComparer.Ordinal));

    private sealed record FaseOrdenV1(string Nombre, IReadOnlyDictionary<string, int> Temas);
}
