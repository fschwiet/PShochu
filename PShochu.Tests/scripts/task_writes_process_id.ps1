
task default -depends WriteProcessId

task WriteProcessId {
    "Process ID: " + [System.Diagnostics.Process]::GetCurrentProcess().Id
}