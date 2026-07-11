using NewSmartAIFactory.CompanyApi.Models;
using Npgsql;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class ApprovalWorkflowService
{
    private static readonly Dictionary<string,string> Actions = new(StringComparer.OrdinalIgnoreCase) { ["approve"]="Approved",["reject"]="Rejected",["defer"]="Deferred",["need-more-info"]="NeedsMoreInfo" };
    private readonly string _connectionString;
    public ApprovalWorkflowService(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");

    public async Task<IReadOnlyList<ApprovalSummary>> ListAsync(string? status,CancellationToken token)
    {
        const string sql="select id,project_id,title,requested_by,status,created_at_utc,updated_at_utc from approval_requests where (@status is null or status=@status) order by created_at_utc desc";var items=new List<ApprovalSummary>();
        await using var c=new NpgsqlConnection(_connectionString);await c.OpenAsync(token);await using var cmd=new NpgsqlCommand(sql,c);cmd.Parameters.Add("status",NpgsqlTypes.NpgsqlDbType.Text).Value=(object?)status??DBNull.Value;await using var r=await cmd.ExecuteReaderAsync(token);while(await r.ReadAsync(token))items.Add(new ApprovalSummary(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetFieldValue<DateTimeOffset>(5),r.GetFieldValue<DateTimeOffset>(6)));return items;
    }
    public async Task<ApprovalDetail?> GetAsync(string id,CancellationToken token)
    {
        const string sql="select id,project_id,title,description,requested_by,status,scope_impact,cost_impact,timeline_impact,security_impact,architecture_impact,created_at_utc,updated_at_utc from approval_requests where id=@id";
        await using var c=new NpgsqlConnection(_connectionString);await c.OpenAsync(token);await using var cmd=new NpgsqlCommand(sql,c);cmd.Parameters.AddWithValue("id",id);await using var r=await cmd.ExecuteReaderAsync(token);if(!await r.ReadAsync(token))return null;
        var v=new {Id=r.GetString(0),Project=r.GetString(1),Title=r.GetString(2),Description=r.GetString(3),RequestedBy=r.GetString(4),Status=r.GetString(5),Scope=r.IsDBNull(6)?null:r.GetString(6),Cost=r.IsDBNull(7)?null:r.GetString(7),Timeline=r.IsDBNull(8)?null:r.GetString(8),Security=r.IsDBNull(9)?null:r.GetString(9),Architecture=r.IsDBNull(10)?null:r.GetString(10),Created=r.GetFieldValue<DateTimeOffset>(11),Updated=r.GetFieldValue<DateTimeOffset>(12)};await r.CloseAsync();var history=new List<ApprovalHistoryItem>();await using var h=new NpgsqlCommand("select id,action,actor,comment,created_at_utc from approval_history where approval_id=@id order by created_at_utc desc",c);h.Parameters.AddWithValue("id",id);await using var hr=await h.ExecuteReaderAsync(token);while(await hr.ReadAsync(token))history.Add(new ApprovalHistoryItem(hr.GetInt64(0),hr.GetString(1),hr.GetString(2),hr.IsDBNull(3)?null:hr.GetString(3),hr.GetFieldValue<DateTimeOffset>(4)));return new ApprovalDetail(v.Id,v.Project,v.Title,v.Description,v.RequestedBy,v.Status,v.Scope,v.Cost,v.Timeline,v.Security,v.Architecture,v.Created,v.Updated,history);
    }
    public async Task<string> CreateAsync(CreateApprovalRequest request,CancellationToken token)
    {
        var id=$"APR-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";await using var c=new NpgsqlConnection(_connectionString);await c.OpenAsync(token);await using var t=await c.BeginTransactionAsync(token);
        const string sql="insert into approval_requests(id,project_id,title,description,requested_by,status,scope_impact,cost_impact,timeline_impact,security_impact,architecture_impact) values(@id,@project,@title,@description,@requestedBy,'Pending',@scope,@cost,@timeline,@security,@architecture)";
        await using(var cmd=new NpgsqlCommand(sql,c,t)){cmd.Parameters.AddWithValue("id",id);cmd.Parameters.AddWithValue("project",request.ProjectId);cmd.Parameters.AddWithValue("title",request.Title.Trim());cmd.Parameters.AddWithValue("description",request.Description.Trim());cmd.Parameters.AddWithValue("requestedBy",request.RequestedBy.Trim());AddNullable(cmd,"scope",request.ScopeImpact);AddNullable(cmd,"cost",request.CostImpact);AddNullable(cmd,"timeline",request.TimelineImpact);AddNullable(cmd,"security",request.SecurityImpact);AddNullable(cmd,"architecture",request.ArchitectureImpact);await cmd.ExecuteNonQueryAsync(token);}await AddHistoryAsync(c,t,id,"Submitted",request.RequestedBy,null,token);await AddAuditAsync(c,t,"approval.created",id,request.RequestedBy,null,"Pending",request.Title,token);await t.CommitAsync(token);return id;
    }
    public async Task<bool> ActAsync(string id,ApprovalActionRequest request,CancellationToken token)
    {
        if(!Actions.TryGetValue(request.Action,out var status))throw new ArgumentException($"Unsupported approval action: {request.Action}");await using var c=new NpgsqlConnection(_connectionString);await c.OpenAsync(token);await using var t=await c.BeginTransactionAsync(token);await using var select=new NpgsqlCommand("select status from approval_requests where id=@id for update",c,t);select.Parameters.AddWithValue("id",id);var previous=await select.ExecuteScalarAsync(token) as string;if(previous is null)return false;await using var update=new NpgsqlCommand("update approval_requests set status=@status,updated_at_utc=now() where id=@id",c,t);update.Parameters.AddWithValue("id",id);update.Parameters.AddWithValue("status",status);await update.ExecuteNonQueryAsync(token);await AddHistoryAsync(c,t,id,status,request.Actor,request.Comment,token);await AddAuditAsync(c,t,$"approval.{request.Action}",id,request.Actor,previous,status,request.Comment,token);await t.CommitAsync(token);return true;
    }
    private static void AddNullable(NpgsqlCommand c,string name,string? value)=>c.Parameters.AddWithValue(name,(object?)value??DBNull.Value);
    private static async Task AddHistoryAsync(NpgsqlConnection c,NpgsqlTransaction t,string id,string action,string actor,string? comment,CancellationToken token){await using var cmd=new NpgsqlCommand("insert into approval_history(approval_id,action,actor,comment) values(@id,@action,@actor,@comment)",c,t);cmd.Parameters.AddWithValue("id",id);cmd.Parameters.AddWithValue("action",action);cmd.Parameters.AddWithValue("actor",actor);AddNullable(cmd,"comment",comment);await cmd.ExecuteNonQueryAsync(token);}
    private static async Task AddAuditAsync(NpgsqlConnection c,NpgsqlTransaction t,string action,string id,string actor,string? oldValue,string? newValue,string? reason,CancellationToken token){await using var cmd=new NpgsqlCommand("insert into audit_logs(action,entity_type,entity_id,actor,previous_value,new_value,reason) values(@action,'approval',@id,@actor,@old,@new,@reason)",c,t);cmd.Parameters.AddWithValue("action",action);cmd.Parameters.AddWithValue("id",id);cmd.Parameters.AddWithValue("actor",actor);AddNullable(cmd,"old",oldValue);AddNullable(cmd,"new",newValue);AddNullable(cmd,"reason",reason);await cmd.ExecuteNonQueryAsync(token);}
}
