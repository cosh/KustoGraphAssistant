using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register the MCP server and configure it to use stdio transport.
// Scan the assembly for tool definitions.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Build and run the host. This starts the MCP server.
await builder.Build().RunAsync();

// Define a static class to hold MCP tools for Kusto graph models.
[McpServerToolType]
public static class KustoGraphModelTools
{
    [McpServerTool, Description("Provides comprehensive best practices and guidelines for creating effective Kusto graph models.")]
    public static string GetGraphModelBestPractices(string? focusArea = null)
    {
        var bestPractices = new
        {
            CorePrinciples = new[]
            {
                "Design your graph schema based on your query patterns, not just your data structure",
                "Start with a clear understanding of the relationships you want to model",
                "Use meaningful and consistent naming conventions for nodes and edges",
                "Balance between graph complexity and query performance",
                "Consider data volume and update frequency when choosing between transient and persistent graphs"
            },
            SchemaDesign = new[]
            {
                "Schema definition is completely optional - start without it for simplicity",
                "Define clear node types that represent distinct entities in your domain (only when schema is needed)",
                "Use edge types that represent meaningful relationships, not just data connections",
                "ALWAYS use string type for all node IDs and edge source/target IDs for consistency",
                "Use pack_all() to automatically include all columns as properties without explicitly listing them",
                "Create dedicated typed properties ONLY for fields that will be used in WHERE clauses or filters",
                "Use appropriate Kusto data types for filter properties: string, long, real, datetime, bool",
                "Add schema later only when you need type safety or performance optimization"
            },
            DefinitionSteps = new[]
            {
                "Always define AddNodes steps before AddEdges steps for better performance",
                "Use filters in your queries to reduce graph size and improve performance",
                "Project only the columns you need",
                "Use static labels when node/edge types are known, dynamic labels if there is a corresponding property in the actual data",
                "Ensure proper join keys exist between your source tables before creating edges",
                "ALWAYS convert IDs to string type using tostring() to ensure consistency"
            },
            SimplificationRules = new[]
            {
                "Use pack_all() instead of pack() to automatically include all columns as properties",
                "Schema definition is completely optional - omit it for simple use cases",
                "Create dedicated typed properties ONLY for fields used in WHERE clauses or aggregations",
                "Ensure ALL node IDs and source/target IDs are string type for consistency",
                "Use pack_all() function in Definition queries to capture all available properties automatically",
                "Access property bag values using bracket notation: Properties['key'] or dot notation if available"
            },
            PerformanceOptimization = new[]
            {
                "Add appropriate filters early in your Definition queries to reduce memory usage"
            },
            QueryOptimization = new[]
            {
                "Use specific node and edge labels in graph-match patterns for better performance",
                "Apply WHERE clauses early in graph-match queries to reduce search space",
                "Limit variable-length path searches with reasonable bounds (e.g., *1..5)",
                "Use graph-shortest-paths for optimal path queries instead of general graph-match",
                "Project only necessary columns in your final results"
            },
            CommonPatterns = new[]
            {
                "Identity graphs: Users, groups, roles, and their relationships",
                "Network topology: Devices, connections, and their properties",
                "Process flows: Steps, decisions, and their temporal relationships",
                "Hierarchical structures: Organizations, categories, and containment",
                "Event correlation: Events, entities, and their causal relationships"
            }
        };

        return JsonSerializer.Serialize(bestPractices, new JsonSerializerOptions { WriteIndented = true });
    }

	[McpServerTool, Description("Provides guidance on creating effective KQL commands for graph model management and querying.")]
	public static string GetGraphCommandGuidance(string? commandType = null)
	{
		var guidance = new
		{
			CreationCommands = new
			{
				BasicSyntax = ".create-or-alter graph_model <ModelName> ``` <JSON Definition> ```",
				BestPractices = new[]
				{
					"Use descriptive model names that reflect the business domain",
					"Start with a simple model without Schema definition - add it later if needed",
					"Test your Definition queries separately before creating the model",
					"Use pack_all() to automatically capture all properties without manual specification",
					"Document your model purpose and expected usage patterns"
				},
				SimpleExampleTemplate = @"
.create-or-alter graph_model MyDomainGraph ```
{
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"",
        ""Query"": ""SourceTable | where IsActive == true | extend NodeId = tostring(Id) | project NodeId, Name, Status, CreatedDate"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""EntityType""]
      },
      {
        ""Kind"": ""AddEdges"",
        ""Query"": ""RelationshipTable | extend SourceId = tostring(FromId), TargetId = tostring(ToId) | project SourceId, TargetId, Weight"",
        ""SourceColumn"": ""SourceId"",
        ""TargetColumn"": ""TargetId"",
        ""Labels"": [""RELATIONSHIP_TYPE""]
      }
    ]
  }
}
```",
				AdvancedExampleWithSchema = @"
.create-or-alter graph_model MyDomainGraph ```
{
  ""Schema"": {
    ""Nodes"": {
      ""EntityType"": {
        ""Id"": ""string"",
        ""Name"": ""string"",
        ""Status"": ""string"",
        ""CreatedDate"": ""datetime""
      }
    },
    ""Edges"": {
      ""RELATIONSHIP_TYPE"": {
        ""Weight"": ""real""
      }
    }
  },
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"",
        ""Query"": ""SourceTable | project NodeId, Name, Status, CreatedDate, Properties = pack_all()"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""EntityType""]
      },
      {
        ""Kind"": ""AddEdges"",
        ""Query"": ""RelationshipTable | project SourceId=tostring(FromId), TargetId=tostring(ToId), Weight, Properties = pack_all()"",
        ""SourceColumn"": ""SourceId"",
        ""TargetColumn"": ""TargetId"",
        ""Labels"": [""RELATIONSHIP_TYPE""]
      }
    ]
  }
}
```"
			},
			QueryPatterns = new
			{
				BasicGraphMatch = @"// Find direct relationships
graph(""ModelName"")
| graph-match (source)-[edge]->(target)
  where source.Property == ""Value""
  project source.Name, edge.Weight, target.Name",

				PathFinding = @"// Find shortest paths
graph(""ModelName"")
| graph-shortest-paths (start)-[*1..5]-(end)
  where start.Id == ""StartNode"" and end.Id == ""EndNode""
  project Path = path",

				VariableLengthPaths = @"// Variable length relationships
graph(""ModelName"")
| graph-match (user)-[follows*1..3]->(influencer)
  where influencer.FollowerCount > 10000
  project user.Name, PathLength = array_length(follows), influencer.Name",

				ManagementCommands = new
				{
					ShowModel = ".show graph_model <ModelName>",
					CreateSnapshot = ".make graph_snapshot <SnapshotName> from <ModelName>",
					ListSnapshots = ".show graph_snapshots | where GraphModel == \"<ModelName>\"",
					DropModel = ".drop graph_model <ModelName>",
					DropSnapshot = ".drop graph_snapshot <SnapshotName>"
				},
				PerformanceTips = new[]
			{
				"Use graph snapshots for frequently accessed read-only queries",
				"Prefer specific snapshots over latest for consistent results",
				"Use transient graphs for one-time analysis: graph(<Model>, transient = true) | graph-match ...",
				"Apply filters in graph-match WHERE clauses, not in separate where operators",
				"Use graph-to-table for simple node/edge exploration without patterns"
			},
				CommonMistakes = new[]
			{
				"Forgetting to include project clause in graph-match queries",
				"Using overly broad variable-length patterns without limits",
				"Not using labels in graph-match patterns, causing performance issues",
				"Mixing graph and regular table operations incorrectly",
				"Creating Definition queries that don't properly join related data"
			},
				DebuggingTips = new[]
			{
				"Test Definition queries separately before adding to graph model",
				"Use .show graph_model to verify your model structure",
				"Check graph-to-table output to verify nodes and edges are created correctly",
				"Use take operator to limit results while debugging complex patterns",
				"Verify data types match between source tables and Schema definitions"
			}
			}
		}; 

        return JsonSerializer.Serialize(guidance, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Provides specific guidance on simplifying graph model schemas with property bags and efficient design patterns.")]
    public static string GetSchemaSimplificationGuidance()
    {
        var guidance = new
        {
            PropertyBagStrategy = new
            {
                Overview = "Use property bags (dynamic type) to reduce schema complexity and improve maintainability",
                WhenToUse = new[]
                {
                    "When you have many properties that are rarely used in WHERE clauses",
                    "For metadata, configuration, or descriptive properties",
                    "When properties vary significantly between entity instances",
                    "For JSON-like data that doesn't need individual column access"
                },
                Implementation = new[]
                {
                    "Schema definition is optional - use it only when you need type safety",
                    "Use pack_all() function to automatically include all columns as properties",
                    "Extract only frequently filtered properties as dedicated schema columns",
                    "Access property bag values using bracket notation: Properties['key']"
                },
                Example = @"
// Simple approach without schema:
{
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"",
        ""Query"": ""Users | extend NodeId = tostring(Id) | project NodeId, Name"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""User""]
      }
    ]
  }
}

// Advanced approach with schema (only when needed):
{
  ""Schema"": {
    ""Nodes"": {
      ""User"": {
        ""Id"": ""string"",
        ""Name"": ""string"",
        ""Department"": ""string"",
        ""IsActive"": ""bool""
      }
    }
  },
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"", 
        ""Query"": ""Users | extend NodeId = tostring(Id), Properties = pack_all() | project NodeId, Name, Department, IsActive, Properties"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""User""]
      }
    ]
  }
}"
            },
            IDTypeConsistency = new
            {
                Rule = "ALWAYS use string type for all IDs in your graph model",
                Rationale = new[]
                {
                    "Prevents type mismatch errors when joining data from different sources",
                    "Handles cases where IDs might contain non-numeric characters",
                    "Ensures consistent behavior across all graph operations",
                    "Avoids conversion issues in queries and edge definitions"
                },
                RequiredStringFields = new[]
                {
                    "All NodeIdColumn values in AddNodes steps",
                    "All SourceColumn values in AddEdges steps", 
                    "All TargetColumn values in AddEdges steps",
                    "Any property used for node/edge identification"
                },
                ConversionTips = new[]
                {
                    "Use tostring() function to convert numeric IDs: extend NodeId = tostring(UserId)",
                    "Concatenate multiple fields for composite IDs: extend NodeId = strcat(Type, '_', Id)",
                    "Handle NULL values: extend NodeId = coalesce(tostring(Id), 'unknown')"
                }
            },
            DedicatedPropertyGuidelines = new
            {
                CreateTypedPropertiesFor = new[]
                {
                    "Fields used in WHERE clauses for filtering"
                }
            },
            SchemaEvolutionStrategy = new
            {
                BestPractices = new[]
                {
                    "Start with minimal schema - add dedicated properties only when needed",
                    "Monitor query patterns to identify frequently filtered properties",
                    "Use ALTER commands to add dedicated properties from property bags",
                    "Maintain backward compatibility when evolving schemas"
                },
                EvolutionExample = @"
// Start simple without schema
{
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"",
        ""Query"": ""Entities | extend NodeId = tostring(Id) | project NodeId, Type"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""Entity""]
      }
    ]
  }
}

// Add schema later when needed for performance
{
  ""Schema"": {
    ""Nodes"": {
      ""Entity"": {
        ""Id"": ""string"", 
        ""Type"": ""string"",
        ""Status"": ""string""
      }
    }
  },
  ""Definition"": {
    ""Steps"": [
      {
        ""Kind"": ""AddNodes"",
        ""Query"": ""Entities | extend NodeId = tostring(Id), Properties = pack_all() | project NodeId, Type, Status, Properties"",
        ""NodeIdColumn"": ""NodeId"",
        ""Labels"": [""Entity""]
      }
    ]
  }
}"
            },
            QueryingPropertyBags = new
            {
                AccessPatterns = new[]
                {
                    "Direct access: where Properties['department'] == 'IT'",
                    "Check existence: where isnotnull(Properties['manager'])",
                    "Extract and convert: extend Manager = tostring(Properties['manager'])",
                    "Filter on bag keys: where Properties has 'special_attribute'"
                },
                PerformanceConsiderations = new[]
                {
                    "Property bag queries are slower than dedicated columns",
                    "Use dedicated columns for high-frequency filters",
                    "Consider indexing strategies for property bag content",
                    "Test performance with representative data volumes"
                }
            }
        };

        return JsonSerializer.Serialize(guidance, new JsonSerializerOptions { WriteIndented = true });
    }

	[McpServerTool, Description("Provides comprehensive guidance on using the graph-match operator with persistent graphs using the intrinsic graph function.")]
	public static string GetGraphMatchGuidance(string? focusArea = null)
	{
		var guidance = new
		{
			Overview = new[]
			{
				"The graph-match operator searches for all occurrences of a graph pattern in a persistent graph",
				"Use the intrinsic graph() function to reference persistent graphs instead of make-graph for transient graphs",
				"Every graph-match query MUST include a 'project' clause - this is mandatory",
				"The 'where' clause is optional but recommended for filtering and performance optimization"
			},
			BasicSyntax = new[]
			{
				"graph('GraphName') | graph-match (pattern) project columns",
				"graph('GraphName', 'SnapshotName') | graph-match (pattern) where constraints project columns",
				"graph('GraphName', true) | graph-match (pattern) project columns  // transient from model",
				"Always start with graph() function, then pipe to graph-match operator"
			},
			PatternNotation = new[]
			{
				"Nodes: (n) for named variable, () for anonymous",
				"Directed edges: -[e]-> (left to right), <-[e]- (right to left)",
				"Any direction: -[e]- or --",
				"Variable length: -[e*1..5]-> for paths with 1 to 5 hops",
				"IMPORTANT: Do NOT include label checks in the pattern - use WHERE clause instead"
			},
			LabelChecking = new[]
			{
				"NEVER check labels in the pattern itself - this is incorrect syntax",
				"Use labels() function in WHERE clause: where labels(node) has 'LabelName'",
				"Example: where labels(user) has 'Employee' and labels(resource) has 'Database'",
				"Labels are defined in the graph model and accessed via labels() function",
				"For transient graphs (make-graph), labels() always returns empty array"
			},
			VariableLengthEdges = new[]
			{
				"Use asterisk notation: -[edge*min..max]-> for variable length paths",
				"Check all edges with all(): where all(edge, property > value)",
				"Check any edge with any(): where any(edge, property == value)",
				"Access inner nodes: all(inner_nodes(edge), condition)",
				"Example: where all(path, bandwidth > 100) and any(inner_nodes(path), wifi == true)"
			},
			WhereClause = new[]
			{
				"Optional but highly recommended for performance and filtering",
				"Use for node/edge property filtering: where user.age > 30",
				"Use for label checking: where labels(node) has 'Type'",
				"Use with variable length edges: where all(path, condition)",
				"Combine conditions with 'and' and 'or': where user.active == true and labels(user) has 'Employee'"
			},
			ProjectClause = new[]
			{
				"MANDATORY - every graph-match must have a project clause",
				"Define output columns: project UserName = user.name, ResourceName = resource.name",
				"Access node properties: node.propertyName",
				"Access edge properties: edge.propertyName",
				"For variable length edges, use map(): project path_names = map(path, name)",
				"Use array functions for variable length: project path_length = array_length(path)"
			},
			ExampleQueries = new[]
			{
				"// Basic pattern matching",
				"graph('SecurityGraph') | graph-match (user)-[permission]->(resource)",
				"  where labels(user) has 'Employee' and labels(resource) has 'Database'",
				"  project UserName = user.name, ResourceName = resource.name, PermissionType = permission.type",
				"",
				"// Variable length path with constraints",
				"graph('NetworkGraph') | graph-match (source)-[path*1..5]->(destination)",
				"  where source.name == 'Router1' and all(path, bandwidth > 100)",
				"  project Source = source.name, Destination = destination.name, PathLength = array_length(path)",
				"",
				"// Complex pattern with multiple constraints",
				"graph('SecurityGraph') | graph-match (attacker)-[attack]->(compromised)-[access*1..3]->(target)",
				"  where labels(attacker) has 'ThreatActor' and labels(target) has 'CriticalAsset'",
				"    and any(access, privilege_level == 'admin')",
				"  project AttackerName = attacker.name, TargetSystem = target.name,",
				"         AccessPath = map(access, edge_type), CompromisedEntity = compromised.name"
			},
			PerformanceOptimization = new[]
			{
				"Apply WHERE clauses early to reduce search space",
				"Use specific labels in WHERE clause instead of broad patterns",
				"Limit variable-length path bounds appropriately (*1..5 vs *1..20)",
				"Use graph-shortest-paths for optimal path queries when you need shortest paths",
				"Project only necessary columns to reduce memory usage"
			},
			CommonMistakes = new[]
			{
				"DON'T: Include labels in pattern - (user:Employee) is WRONG",
				"DO: Use WHERE clause - where labels(user) has 'Employee'",
				"DON'T: Forget project clause - this causes syntax error",
				"DON'T: Use make-graph with persistent graphs - use graph() function instead",
				"DON'T: Check properties in pattern - use WHERE clause for property filtering"
			},
			GraphFunctions = new[]
			{
				"graph('GraphName') - latest snapshot",
				"graph('GraphName', 'SnapshotName') - specific snapshot",
				"graph('GraphName', snapshot='SnapshotName') - named parameter syntax",
				"graph('GraphName', true) - transient graph from model",
				"graph('GraphName', false) - explicit latest snapshot (same as default)"
			}
		};

		if (!string.IsNullOrEmpty(focusArea))
		{
			return focusArea.ToLower() switch
			{
				"labels" => JsonSerializer.Serialize(new { LabelChecking = guidance.LabelChecking, CommonMistakes = guidance.CommonMistakes }, new JsonSerializerOptions { WriteIndented = true }),
				"patterns" => JsonSerializer.Serialize(new { PatternNotation = guidance.PatternNotation, ExampleQueries = guidance.ExampleQueries }, new JsonSerializerOptions { WriteIndented = true }),
				"variable" or "variablelength" => JsonSerializer.Serialize(new { VariableLengthEdges = guidance.VariableLengthEdges, ExampleQueries = guidance.ExampleQueries }, new JsonSerializerOptions { WriteIndented = true }),
				"performance" => JsonSerializer.Serialize(new { PerformanceOptimization = guidance.PerformanceOptimization, CommonMistakes = guidance.CommonMistakes }, new JsonSerializerOptions { WriteIndented = true }),
				"examples" => JsonSerializer.Serialize(new { ExampleQueries = guidance.ExampleQueries }, new JsonSerializerOptions { WriteIndented = true }),
				_ => JsonSerializer.Serialize(guidance, new JsonSerializerOptions { WriteIndented = true })
			};
		}

		return JsonSerializer.Serialize(guidance, new JsonSerializerOptions { WriteIndented = true });
	}
}