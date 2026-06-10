# Project Initialization Scripts

This directory contains modular PowerShell scripts for initializing the Azure DevOps MCP Server project.

## Scripts Structure

```
scripts/
├── init-project.ps1              # Main entry script (interactive wizard)
├── modules/
│   ├── Check-Deps.ps1            # Check system dependencies
│   ├── Install-GitNexus.ps1      # Install GitNexus
│   ├── Install-Skills.ps1        # Install mattpocock/skills
│   ├── Config-Mcp.ps1             # Configure MCP connections
│   ├── Config-Project.ps1        # Configure project mapping
│   └── Setup-Docs.ps1             # Initialize project documentation
└── Initialize-McpServer.ps1      # MCP Server initialization (existing)
```

## Usage

### Full Initialization (Interactive)

Run the main script to initialize the project:

```powershell
.\scripts\init-project.ps1
```

This will guide you through all initialization steps.

### Individual Steps

You can also run individual modules:

```powershell
# Check dependencies only
.\scripts\modules\Check-Deps.ps1

# Install GitNexus only
.\scripts\modules\Install-GitNexus.ps1

# Configure project mapping only
.\scripts\modules\Config-Project.ps1
```

### Command Line Options

The main script supports various options:

```powershell
# Skip certain steps
.\scripts\init-project.ps1 -SkipGitNexus -SkipSkills

# Run specific step only
.\scripts\init-project.ps1 -Step deps
.\scripts\init-project.ps1 -Step mcp

# Silent mode (non-interactive)
.\scripts\init-project.ps1 -Silent
```

## Module Descriptions

### 1. Check-Deps.ps1

Checks if required dependencies are installed:
- Node.js (>= 18.0.0)
- Git
- npm (part of Node.js)
- .NET SDK (optional, required for MCP Server)

### 2. Install-GitNexus.ps1

Installs GitNexus for code knowledge graph:
- Installs via npm: `npm install -g gitnexus`
- Optionally runs initial code analysis

### 3. Install-Skills.ps1

Installs mattpocock/skills for improved AI coding:
- Installs via npx: `npx skills@latest add mattpocock/skills`
- Creates .claude/skills.json configuration

### 4. Config-Mcp.ps1

Configures MCP connections:
- Claude Code MCP settings (.claude/settings.json)
- Cursor MCP settings (.cursor/mcp.json)
- Local MCP Server config (config/mcp-config.json)
- PAT Token configuration

### 5. Config-Project.ps1

Configures project mapping:
- Maps local project to Azure DevOps project
- Stores in config/project-mapping.json
- Prompts for Azure DevOps organization URL and project ID

### 6. Setup-Docs.ps1

Initializes project documentation:
- Creates docs/adr/ directory (Architecture Decision Records)
- Creates docs/issues/ directory
- Creates docs/templates/ directory
- Generates CONTEXT.md

## Requirements

- Windows PowerShell 5.1 or higher
- PowerShell 7 (pwsh) recommended for better Unicode support
- Internet connection for downloading dependencies

## Next Steps

After running the initialization scripts:

1. Configure Azure DevOps PAT Token
   ```powershell
   [System.Environment]::SetEnvironmentVariable("AZURE_DEVOPS_PAT", "your-token", "User")
   ```

2. Restart Claude Code / Cursor to load MCP configuration

3. Run GitNexus code indexing
   ```powershell
   gitnexus analyze
   ```

4. View CONTEXT.md to understand project context
   ```powershell
   Get-Content CONTEXT.md
   ```

## Troubleshooting

If you encounter encoding issues, try using PowerShell 7 (pwsh):

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\init-project.ps1
```
