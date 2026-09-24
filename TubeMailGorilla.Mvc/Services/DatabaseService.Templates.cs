using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>Email templates and the customizable [token] parameters.</summary>
public partial class DatabaseService
{
    private const string InsertTemplateSql =
        $"INSERT INTO {TemplatesTable} (Name, Subject, Body, CreatedAt, UpdatedAt) " +
        "VALUES (@Name, @Subject, @Body, @CreatedAt, @UpdatedAt)";

    private const string UpdateTemplateSql =
        $"UPDATE {TemplatesTable} SET Name = @Name, Subject = @Subject, Body = @Body, " +
        "CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt WHERE Id = @Id";

    private static readonly MessageParameter[] DefaultParameters =
    {
        new() { Token = "f_name", Field = "first-name" },
        new() { Token = "l_name", Field = "last-name" },
        new() { Token = "icebreaker", Field = "icebreaker" },
        new() { Token = "title", Field = "video-title" },
        new() { Token = "descr", Field = "video-description" },
        new() { Token = "channel", Field = "channel-name" }
    };

    public async Task<List<EmailTemplate>> GetTemplatesAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {TemplateCols} FROM {TemplatesTable} ORDER BY Id", DbHelpers.ReadTemplate);
    }

    public async Task<int> SaveTemplateAsync(EmailTemplate template)
    {
        await using var connection = await OpenAsync();

        if (template.Id == 0)
        {
            return await DbHelpers.InsertAsync(connection, InsertTemplateSql, c => BindTemplate(c, template),
                id => template.Id = (int)id);
        }

        template.UpdatedAt = DateTime.Now;
        return await DbHelpers.ExecuteAsync(connection, UpdateTemplateSql, c =>
        {
            BindTemplate(c, template);
            c.Parameters.AddWithValue("@Id", template.Id);
        });
    }

    public async Task<int> DeleteTemplateAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {TemplatesTable} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    private static void BindTemplate(SqliteCommand command, EmailTemplate template)
    {
        command.Parameters.AddWithValue("@Name", template.Name ?? string.Empty);
        command.Parameters.AddWithValue("@Subject", template.Subject ?? string.Empty);
        command.Parameters.AddWithValue("@Body", template.Body ?? string.Empty);
        command.Parameters.AddWithValue("@CreatedAt", DbHelpers.Ticks(template.CreatedAt));
        command.Parameters.AddWithValue("@UpdatedAt", DbHelpers.NullableTicks(template.UpdatedAt));
    }

    /// <summary>
    /// Every custom token. The built-in defaults ([f_name], [icebreaker], ...) are
    /// seeded on first use so personalization works out of the box.
    /// </summary>
    public async Task<List<MessageParameter>> GetMessageParametersAsync()
    {
        await using var connection = await OpenAsync();
        var parameters = await DbHelpers.QueryAsync(connection,
            $"SELECT {ParameterCols} FROM {ParametersTable} ORDER BY Id", DbHelpers.ReadParameter);

        if (parameters.Count == 0)
        {
            foreach (var parameter in DefaultParameters)
                await SaveMessageParameterAsync(parameter);

            parameters = await DbHelpers.QueryAsync(connection,
                $"SELECT {ParameterCols} FROM {ParametersTable} ORDER BY Id", DbHelpers.ReadParameter);
        }

        return parameters;
    }

    public async Task<int> SaveMessageParameterAsync(MessageParameter parameter)
    {
        await using var connection = await OpenAsync();

        if (parameter.Id == 0)
        {
            return await DbHelpers.InsertAsync(connection,
                $"INSERT INTO {ParametersTable} (Token, Field) VALUES (@Token, @Field)",
                c =>
                {
                    c.Parameters.AddWithValue("@Token", parameter.Token ?? string.Empty);
                    c.Parameters.AddWithValue("@Field", parameter.Field ?? string.Empty);
                },
                id => parameter.Id = (int)id);
        }

        return await DbHelpers.ExecuteAsync(connection,
            $"UPDATE {ParametersTable} SET Token = @Token, Field = @Field WHERE Id = @Id",
            c =>
            {
                c.Parameters.AddWithValue("@Token", parameter.Token ?? string.Empty);
                c.Parameters.AddWithValue("@Field", parameter.Field ?? string.Empty);
                c.Parameters.AddWithValue("@Id", parameter.Id);
            });
    }

    public async Task<int> DeleteMessageParameterAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {ParametersTable} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }
}
