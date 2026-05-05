using System.Windows.Controls;
using WinForge.Services;

namespace WinForge.Views;

public sealed class NavigationPageFactory
{
    private readonly OptimisationService _optimisation;
    private readonly SystemStatusService _status;

    public NavigationPageFactory(OptimisationService optimisation, SystemStatusService status)
    {
        _optimisation = optimisation;
        _status = status;
    }

    public UserControl Dashboard() => new DashboardView(_status);
    public UserControl Optimise() => new OptimiseView(_optimisation);
    public UserControl Gaming() => new GamingView();
    public UserControl Profiles() => new ProfilesView(_optimisation);
    public UserControl Restore() => new RestoreView(_optimisation);
    public UserControl Logs() => new LogsView();

    public UserControl Apps() => Group(
        "Apps and Startup",
        "Control startup entries, running processes, services, tasks and installed software.",
        Tab("Startup", "Control which apps open when Windows starts.", () => new StartupView()),
        Tab("Processes", "Review the apps and services using system resources.", () => Tool("Processes", "Review the apps and services using system resources.")),
        Tab("Services", "Review services, third party background tasks and startup modes.", () => Tool("Services", "Review services, third party background tasks and startup modes.")),
        Tab("Tasks", "Review scheduled tasks that affect startup and background usage.", () => Tool("Tasks", "Review scheduled tasks that affect startup and background usage.")),
        Tab("Installed Apps", "Review installed software and open uninstall tools.", () => Tool("Apps", "Review installed software and open uninstall tools.")),
        Tab("App History", "Track software changes over time.", () => Tool("App History", "Track software changes over time.")));

    public UserControl Storage() => Group(
        "Storage",
        "Clean clutter, analyse disk usage and review files that take up space.",
        Tab("Clean Up", "Remove safe clutter and recover storage space.", () => new CategoryView(_optimisation, "Clean Up", "Remove safe clutter and recover storage space.")),
        Tab("Storage", "Analyse drive space, large folders and storage pressure.", () => Tool("Storage", "Analyse drive space, large folders and storage pressure.")),
        Tab("Deep Storage", "Map storage usage, duplicate candidates and old installers.", () => Tool("Deep Storage", "Map storage usage, duplicate candidates and old installers.")),
        Tab("Downloads", "Review old downloads and installers before removing anything.", () => Tool("Downloads", "Review old downloads and installers before removing anything.")),
        Tab("Browsers", "Review browser cache size and extension managers.", () => Tool("Browsers", "Review browser cache size and extension managers.")),
        Tab("Desktop", "Review desktop clutter, broken shortcuts and Start Menu items.", () => Tool("Desktop Tools", "Review desktop clutter, broken shortcuts and Start Menu items.")),
        Tab("History", "Review recent files, run history, clipboard history and Explorer history.", () => Tool("History Cleanup", "Review recent files, run history, clipboard history and Explorer history.")));

    public UserControl Network() => Group(
        "Network",
        "Check connectivity, DNS, firewall, ports, proxy, VPN and remote access.",
        Tab("Network", "Run connection checks and repair common network issues.", () => Tool("Network", "Run connection checks and repair common network issues.")),
        Tab("Profiles", "Save DNS profiles, review WiFi quality and check public network details.", () => Tool("Network Profiles", "Save DNS profiles, review WiFi quality and check public network details.")),
        Tab("Firewall", "Review firewall state, inbound rules and listening ports.", () => Tool("Firewall", "Review firewall state, inbound rules and listening ports.")),
        Tab("Open Ports", "Review listening ports and the processes using them.", () => Tool("Open Ports", "Review listening ports and the processes using them.")),
        Tab("Proxy and VPN", "Review proxy settings, WinHTTP proxy and VPN adapters.", () => Tool("Proxy and VPN", "Review proxy settings, WinHTTP proxy and VPN adapters.")),
        Tab("Hosts", "Inspect and back up the Windows hosts file.", () => Tool("Hosts", "Inspect and back up the Windows hosts file.")),
        Tab("Time Sync", "Check and repair Windows time synchronisation.", () => Tool("Time Sync", "Check and repair Windows time synchronisation.")),
        Tab("Remote Access", "Review Remote Desktop, Remote Assistance and remote access firewall state.", () => Tool("Remote Access", "Review Remote Desktop, Remote Assistance and remote access firewall state.")),
        Tab("Shares", "Review network shares and shared folder exposure.", () => Tool("Shares", "Review network shares and shared folder exposure.")));

    public UserControl Security() => Group(
        "Security",
        "Review privacy, Defender, baselines, permissions, accounts and persistence locations.",
        Tab("Security Scan", "Check Windows security health and Defender exclusions.", () => Tool("Security Scan", "Check Windows security health and Defender exclusions.")),
        Tab("Privacy", "Review privacy settings and Windows permission controls.", () => Tool("Privacy Tools", "Review privacy settings and Windows permission controls.")),
        Tab("Baselines", "Create security baseline reports for different PC types.", () => Tool("Baselines", "Create security baseline reports for different PC types.")),
        Tab("Persistence", "Review common startup and persistence locations.", () => Tool("Persistence", "Review common startup and persistence locations.")),
        Tab("Permissions", "Review Windows permission areas such as camera, microphone and location.", () => Tool("Permissions", "Review Windows permission areas such as camera, microphone and location.")),
        Tab("Accounts", "Review local accounts, administrators and remote access groups.", () => Tool("Accounts", "Review local accounts, administrators and remote access groups.")),
        Tab("Safety", "Review WinForge safety mode guidance.", () => Tool("Safety", "Review WinForge safety mode guidance.")),
        Tab("Advanced Mode", "Show actions that are reserved for advanced users.", () => Tool("Advanced Mode", "Show actions that are reserved for advanced users.")));

    public UserControl System() => Group(
        "System",
        "Review health, hardware, Windows version, visuals, search and developer settings.",
        Tab("Health", "Review reliability records, event logs and device problems.", () => Tool("System Health", "Review reliability records, event logs and device problems.")),
        Tab("Health Score", "Score the PC across startup, storage, security, network and update health.", () => Tool("Health Score", "Score the PC across startup, storage, security, network and update health.")),
        Tab("Monitoring", "Create scheduled health scans, alerts and change watchers.", () => Tool("Monitoring", "Create scheduled health scans, alerts and change watchers.")),
        Tab("Hardware", "Review hardware, drivers, temperatures and throttling hints.", () => Tool("Hardware", "Review hardware, drivers, temperatures and throttling hints.")),
        Tab("Battery", "Review laptop battery status and create battery reports.", () => Tool("Battery", "Review laptop battery status and create battery reports.")),
        Tab("Edition", "Review Windows edition, activation state, install date and build details.", () => Tool("Edition", "Review Windows edition, activation state, install date and build details.")),
        Tab("Defaults", "Generate recommendations based on Windows version and device type.", () => Tool("Version Defaults", "Generate recommendations based on Windows version and device type.")),
        Tab("Performance", "Tune power and visual settings for a smoother system.", () => new CategoryView(_optimisation, "Performance", "Tune power and visual settings for a smoother system.")),
        Tab("Visuals", "Review Windows visual effects profiles.", () => Tool("Visuals", "Review Windows visual effects profiles.")),
        Tab("Search", "Review Windows Search indexing and rebuild options.", () => Tool("Search", "Review Windows Search indexing and rebuild options.")),
        Tab("Developer", "Check developer features, toolchains and Windows developer settings.", () => Tool("Developer", "Check developer features, toolchains and Windows developer settings.")),
        Tab("Features", "Review optional Windows features.", () => Tool("Windows Features", "Review optional Windows features.")));

    public UserControl Repair() => Group(
        "Repair and Recovery",
        "Run repair tools, fix updates, create recovery notes and check restart status.",
        Tab("Repair", "Run built in Windows repair and reset tools.", () => Tool("Repair Tools", "Run built in Windows repair and reset tools.")),
        Tab("Update Repair", "Review Windows Update state, repair services and export drivers.", () => Tool("Update Repair", "Review Windows Update state, repair services and export drivers.")),
        Tab("Recovery", "Create restore points, recovery notes and Windows recovery shortcuts.", () => Tool("Recovery", "Create restore points, recovery notes and Windows recovery shortcuts.")),
        Tab("Restart", "Check whether Windows needs a restart after updates or repairs.", () => Tool("Restart", "Check whether Windows needs a restart after updates or repairs.")),
        Tab("Support Bundle", "Export reports and logs for troubleshooting.", () => Tool("Support Bundle", "Export reports and logs for troubleshooting.")));

    public UserControl Tools() => Group(
        "Tools",
        "Access guided setup, reports, maintenance, themes, HomeForge integration and audit tools.",
        Tab("Guided", "Scan, review recommendations and create before and after reports.", () => Tool("Guided Optimise", "Scan, review recommendations and create before and after reports.")),
        Tab("Game Focus", "Create per game optimisation notes and focus launchers.", () => Tool("Game Focus", "Create per game optimisation notes and focus launchers.")),
        Tab("Changes", "Generate explanations for selected changes.", () => Tool("Changes", "Generate explanations for selected changes.")),
        Tab("Queue", "Review action queue files and optimisation snapshots.", () => Tool("Queue", "Review action queue files and optimisation snapshots.")),
        Tab("Reports", "Export system, security, storage and network reports.", () => Tool("Reports", "Export system, security, storage and network reports.")),
        Tab("Maintenance", "Create or remove scheduled maintenance tasks.", () => Tool("Maintenance", "Create or remove scheduled maintenance tasks.")),
        Tab("Ignore List", "Create and open ignore lists for apps, folders and rules.", () => Tool("Ignore List", "Create and open ignore lists for apps, folders and rules.")),
        Tab("Portable", "Prepare portable mode folders and usage notes.", () => Tool("Portable", "Prepare portable mode folders and usage notes.")),
        Tab("First Run", "Create a first run recommendation report for this PC.", () => Tool("First Run", "Create a first run recommendation report for this PC.")),
        Tab("Context Menu", "Review right click menu entries and shell extensions.", () => Tool("Context Menu", "Review right click menu entries and shell extensions.")),
        Tab("Explorer", "Review shell extensions, icon overlays and Explorer add ons.", () => Tool("Explorer Tools", "Review shell extensions, icon overlays and Explorer add ons.")),
        Tab("Extensions", "Review browser extension folders and extension managers.", () => Tool("Extensions", "Review browser extension folders and extension managers.")),
        Tab("Audit", "Create transparent audit reports.", () => Tool("Audit", "Create transparent audit reports.")),
        Tab("Help", "Open built in guidance for optimisation areas.", () => Tool("Help", "Open built in guidance for optimisation areas.")),
        Tab("Updater", "Prepare installer, portable package and release note helper files.", () => Tool("Updater", "Prepare installer, portable package and release note helper files.")),
        Tab("Themes", "Create theme preference files and review themes.", () => Tool("Themes", "Create theme preference files and review themes.")),
        Tab("HomeForge", "Detect HomeForge folders and prepare a server friendly profile.", () => Tool("HomeForge", "Detect HomeForge folders and prepare a server friendly profile.")),
        Tab("Action Catalog", "Export a catalogue of WinForge actions and safety metadata.", () => Tool("Action Catalog", "Export a catalogue of WinForge actions and safety metadata.")));

    private static ToolGroupTab Tab(string title, string description, Func<UserControl> createPage) => new(title, description, createPage);

    private static ToolPageView Tool(string title, string description) => new(title, description);

    private static ToolGroupView Group(string title, string subtitle, params ToolGroupTab[] tabs) => new(title, subtitle, tabs);
}
