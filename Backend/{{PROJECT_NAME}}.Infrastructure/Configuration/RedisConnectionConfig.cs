namespace {{PROJECT_NAME}}.Infrastructure.Configuration;


public class RedisConnectionConfig
{
    public string ConnectionString { get; set; } = default!;
    public int Database { get; set; }
}
