# Kusto Graph Assistant

A Model Context Protocol (MCP) server that provides comprehensive guidance and best practices for creating and managing Kusto graph models. This tool integrates seamlessly with GitHub Copilot in Visual Studio Code to assist developers in building effective graph-based analytics solutions using Azure Data Explorer (Kusto).

## What is KustoGraphAssistant?

Kusto Graph Assistant is an intelligent assistant that helps you:

- **Design effective graph schemas** - Get guidance on structuring nodes, edges, and relationships based on your data patterns
- **Optimize graph performance** - Learn best practices for query optimization and efficient graph model design
- **Simplify complex schemas** - Understand when and how to use property bags, dedicated columns, and schema evolution
- **Master graph-match queries** - Get comprehensive guidance on using the graph-match operator with persistent graphs
- **Follow KQL best practices** - Learn proper syntax, patterns, and performance optimization techniques

## Features

The MCP server provides several specialized tools:

### 🎯 GetGraphModelBestPractices

Comprehensive best practices covering:

- Core design principles for graph models
- Schema design guidelines and simplification strategies
- Performance optimization techniques
- Common patterns for identity graphs, network topology, and process flows

### 📋 GetGraphCommandGuidance

Detailed guidance on KQL commands including:

- Graph model creation and management syntax
- Query patterns for graph-match operations
- Performance tips and debugging strategies
- Common mistakes and how to avoid them

### 🔧 GetSchemaSimplificationGuidance

Specialized guidance on:

- Using property bags to reduce schema complexity
- ID type consistency (always use strings!)
- When to use dedicated properties vs. property bags
- Schema evolution strategies

### 🔍 GetGraphMatchGuidance

Complete reference for graph-match operator:

- Pattern notation and syntax
- Label checking with WHERE clauses
- Variable-length edge patterns
- Performance optimization techniques

## Prerequisites

- .NET 9.0 or later
- Visual Studio Code
- GitHub Copilot extension for VS Code

## Installation and Setup

### 1. Clone and Build the Project

```bash
git clone https://github.com/cosh/KustoGraphAssistant.git
cd KustoGraphAssistant
dotnet build
```

### 2. Configure the MCP Server in Visual Studio Code

Configure GitHub Copilot for Visual Studio Code to use your custom MCP server:

1. If you haven't already, open your project folder in Visual Studio Code.
2. Create a `.vscode` folder at the root of your project.
3. Add an `mcp.json` file in the `.vscode` folder with the following content:

   ```json
   {
     "inputs": [],
     "servers": {
       "KustoGraphAssistant": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "${workspaceFolder}/KustoGraphAssistant.csproj"
         ]
       }
     }
   }
   ```

4. Save the file.

### 3. Test the MCP Server

1. Open GitHub Copilot in Visual Studio Code and switch to agent mode.
2. Select the **Select tools** icon to verify your **KustoGraphAssistant** is available with all tools listed.

## Usage Examples

Once configured, you can interact with the assistant through GitHub Copilot:

### Getting Started with Graph Models

```text
What are the best practices for designing a graph model for user relationships?
```

### Schema Design Help

```text
How should I structure my graph schema for network topology analysis?
```

### Query Optimization

```text
Show me how to write efficient graph-match queries for finding shortest paths.
```

### Troubleshooting

```text
I'm getting errors with my graph model definition. What are common mistakes to avoid?
```

## Key Design Principles

This assistant emphasizes several important principles:

### 🔤 Always Use String IDs

- Convert all node and edge IDs to string type using `tostring()`
- Prevents type mismatch errors and ensures consistency

### 📦 Leverage Property Bags

- Use `pack_all()` to automatically capture all properties
- Create dedicated typed properties only for frequently filtered fields
- Schema definition is optional - start simple!

### ⚡ Optimize for Performance

- Apply filters early in Definition queries
- Use specific labels in graph-match WHERE clauses
- Limit variable-length path searches appropriately

### 🎯 Query Pattern Awareness

- Design your graph schema based on your query patterns, not just data structure
- Use graph-match for pattern matching, graph-shortest-paths for optimal paths
- Always include a project clause in graph-match queries

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions:

1. Check the common patterns and troubleshooting guidance provided by the assistant
2. Review the KQL documentation for Azure Data Explorer
3. Submit an issue in this repository

---

**Happy Graph Modeling!** 🚀
